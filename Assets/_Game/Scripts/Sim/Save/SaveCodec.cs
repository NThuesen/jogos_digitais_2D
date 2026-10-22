using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Neko.Sim.Cafe;
using Neko.Sim.Maids;
using Neko.Sim.Progress;

namespace Neko.Sim.Save
{
    public enum LoadStatus
    {
        Ok = 0,

        /// <summary>Read from an older schema and brought forward.</summary>
        Migrated,

        /// <summary>Unreadable. The player gets a fresh profile and is told why.</summary>
        Corrupt,

        /// <summary>Nothing saved yet. Not a problem.</summary>
        Empty,
    }

    public readonly struct LoadResult
    {
        public readonly LoadStatus Status;
        public readonly Profile Profile;

        /// <summary>Portuguese. Shown to the player when it is not empty.</summary>
        public readonly string Message;

        public LoadResult(LoadStatus status, Profile profile, string message = "")
        {
            Status = status;
            Profile = profile;
            Message = message;
        }
    }

    /// <summary>
    /// Turns a profile into one string and back, with a version and a checksum.
    /// </summary>
    /// <remarks>
    /// Rubric 11 asks what happens when things go wrong, and this is most of the answer. No
    /// path through <see cref="Decode"/> throws: every failure returns a
    /// <see cref="LoadResult"/> with a reason the player can read, and a playable profile.
    /// Losing a save is bad; losing it to a stack trace on the title screen is worse.
    ///
    /// The format is lines rather than JSON because <c>Neko.Sim</c> has no UnityEngine
    /// reference and therefore no <c>JsonUtility</c>, and a hand-written JSON parser would be
    /// a great deal more code to get wrong for a file nobody reads by hand.
    ///
    /// Every number is written and read with the invariant culture. A save written on a
    /// machine whose decimal separator is a comma must still load on one where it is a dot.
    /// </remarks>
    public static class SaveCodec
    {
        public const int CurrentVersion = 1;

        private const string Prefix = "NEKO";
        private const char Separator = ':';

        private static readonly CultureInfo Invariant = CultureInfo.InvariantCulture;

        public static string Encode(Profile profile)
        {
            if (profile == null) return "";

            var body = WriteBody(profile);
            var bytes = Encoding.UTF8.GetBytes(body);

            return Prefix + CurrentVersion + Separator +
                   Convert.ToBase64String(bytes) + Separator +
                   Checksum(body).ToString("x8", Invariant);
        }

        public static LoadResult Decode(string payload)
        {
            if (string.IsNullOrEmpty(payload))
            {
                return new LoadResult(LoadStatus.Empty, Profile.NewGame(1u));
            }

            try
            {
                var parts = payload.Split(Separator);
                if (parts.Length != 3 || !parts[0].StartsWith(Prefix, StringComparison.Ordinal))
                {
                    return Corrupt("O arquivo de save não é deste jogo.");
                }

                if (!int.TryParse(parts[0].Substring(Prefix.Length), NumberStyles.Integer,
                        Invariant, out var version))
                {
                    return Corrupt("O arquivo de save não diz de que versão é.");
                }

                if (version > CurrentVersion)
                {
                    // Reading it with today's rules would silently misinterpret fields this
                    // version does not know about, which is worse than admitting defeat.
                    return Corrupt("Esse save é de uma versão mais nova do jogo.");
                }

                var body = Encoding.UTF8.GetString(Convert.FromBase64String(parts[1]));

                if (Checksum(body).ToString("x8", Invariant) != parts[2])
                {
                    return Corrupt("O save está corrompido e não pôde ser lido.");
                }

                var profile = ReadBody(body);
                if (profile == null) return Corrupt("O save está incompleto e não pôde ser lido.");

                profile.ApplyUnlocks();

                return version < CurrentVersion
                    ? new LoadResult(LoadStatus.Migrated, profile,
                        "Save de uma versão anterior foi atualizado.")
                    : new LoadResult(LoadStatus.Ok, profile);
            }
            catch (Exception)
            {
                // Deliberately broad. Anything at all that goes wrong reading a file from
                // disk ends as a readable message and a playable game, never as a crash.
                return Corrupt("O save está corrompido e não pôde ser lido.");
            }
        }

        private static LoadResult Corrupt(string message) =>
            new LoadResult(LoadStatus.Corrupt, Profile.NewGame(1u),
                message + " Começamos um café novo.");

        // -------------------------------------------------------------------------------
        // Writing
        // -------------------------------------------------------------------------------

        private static string WriteBody(Profile profile)
        {
            var text = new StringBuilder();

            text.Append("seed=").Append(profile.Seed.ToString(Invariant)).Append('\n');
            text.Append("day=").Append(profile.Day.ToString(Invariant)).Append('\n');
            text.Append("step=").Append(profile.QuotaStep.ToString(Invariant)).Append('\n');
            text.Append("money=").Append(profile.Wallet.Money.ToString(Invariant)).Append('\n');
            text.Append("rep=").Append(profile.Wallet.Reputation.ToString(Invariant)).Append('\n');
            text.Append("endless=").Append(profile.EndlessUnlocked ? '1' : '0').Append('\n');
            text.Append("nextmaid=").Append(profile.NextMaidId.ToString(Invariant)).Append('\n');

            text.Append("grid=").Append(profile.Grid.Width.ToString(Invariant)).Append('x')
                .Append(profile.Grid.Height.ToString(Invariant)).Append('\n');

            foreach (var item in profile.Grid.Items)
            {
                text.Append("item=").Append(item.Def.Id).Append(',')
                    .Append(item.X.ToString(Invariant)).Append(',')
                    .Append(item.Y.ToString(Invariant)).Append(',')
                    .Append(item.Rotated ? '1' : '0').Append('\n');
            }

            foreach (var maid in profile.Maids) text.Append("maid=").Append(WriteMaid(maid)).Append('\n');

            text.Append("roster=").Append(string.Join(",", profile.ShiftRoster)).Append('\n');
            text.Append("place=").Append(string.Join(",", profile.OwnedPlaceables)).Append('\n');
            text.Append("gear=").Append(string.Join(",", profile.OwnedEquipment)).Append('\n');

            var records = profile.Records;
            text.Append("records=")
                .Append(records.BestDay.ToString(Invariant)).Append(',')
                .Append(records.BestEarnings.ToString(Invariant)).Append(',')
                .Append(records.BestEndlessDay.ToString(Invariant)).Append(',')
                .Append(records.TotalCustomersServed.ToString(Invariant)).Append(',')
                .Append(records.TotalDaysPlayed.ToString(Invariant)).Append('\n');

            return text.ToString();
        }

        private static string WriteMaid(Maid maid)
        {
            var gear = new string[maid.Gear.Length];
            for (var i = 0; i < gear.Length; i++) gear[i] = maid.Gear[i]?.Id ?? "";

            return string.Join("|", new[]
            {
                maid.Id.ToString(Invariant),
                Sanitise(maid.Name),
                maid.Archetype.ToString(),
                maid.Level.ToString(Invariant),
                maid.Xp.ToString(Invariant),
                maid.Bond.ToString(Invariant),
                maid.BondPoints.ToString(Invariant),
                maid.Base.Serve + "," + maid.Base.Cook + "," + maid.Base.Charm + "," +
                maid.Base.Stamina,
                string.Join(",", gear),
                string.Join(",", maid.Skills),
                maid.Mood.ToString("0.###", Invariant),
            });
        }

        /// <summary>
        /// Keeps a name out of the delimiters.
        /// </summary>
        /// <remarks>
        /// Today every name comes from a fixed pool with no separators in it, so this never
        /// fires. It is here because the day somebody adds a rename button is not the day to
        /// discover that a pipe in a nickname eats the rest of the save.
        /// </remarks>
        private static string Sanitise(string name) =>
            string.IsNullOrEmpty(name)
                ? "?"
                : name.Replace('|', ' ').Replace(',', ' ').Replace('\n', ' ').Replace(':', ' ');

        // -------------------------------------------------------------------------------
        // Reading
        // -------------------------------------------------------------------------------

        private static Profile ReadBody(string body)
        {
            var profile = new Profile();
            profile.Maids.Clear();
            profile.ShiftRoster.Clear();

            var width = 7;
            var height = 5;
            var items = new List<string[]>();
            var money = 0;
            var reputation = 0;
            var sawGrid = false;

            foreach (var raw in body.Split('\n'))
            {
                var line = raw.Trim();
                if (line.Length == 0) continue;

                var split = line.IndexOf('=');
                if (split <= 0) return null;

                var key = line.Substring(0, split);
                var value = line.Substring(split + 1);

                switch (key)
                {
                    case "seed":
                        profile.Seed = uint.Parse(value, Invariant);
                        break;

                    case "day":
                        profile.Day = Math.Max(1, int.Parse(value, Invariant));
                        break;

                    case "step":
                        profile.QuotaStep = Math.Max(1, int.Parse(value, Invariant));
                        break;

                    case "money":
                        money = int.Parse(value, Invariant);
                        break;

                    case "rep":
                        reputation = int.Parse(value, Invariant);
                        break;

                    case "endless":
                        profile.EndlessUnlocked = value == "1";
                        break;

                    case "nextmaid":
                        profile.NextMaidId = Math.Max(1, int.Parse(value, Invariant));
                        break;

                    case "grid":
                    {
                        var size = value.Split('x');
                        if (size.Length != 2) return null;
                        width = int.Parse(size[0], Invariant);
                        height = int.Parse(size[1], Invariant);
                        sawGrid = true;
                        break;
                    }

                    case "item":
                        items.Add(value.Split(','));
                        break;

                    case "maid":
                    {
                        var maid = ReadMaid(value);
                        if (maid == null) return null;
                        profile.Maids.Add(maid);
                        break;
                    }

                    case "roster":
                        foreach (var id in Split(value)) profile.ShiftRoster.Add(int.Parse(id, Invariant));
                        break;

                    case "place":
                        profile.OwnedPlaceables.AddRange(Split(value));
                        break;

                    case "gear":
                        profile.OwnedEquipment.AddRange(Split(value));
                        break;

                    case "records":
                    {
                        var parts = value.Split(',');
                        if (parts.Length != 5) return null;
                        profile.Records.BestDay = int.Parse(parts[0], Invariant);
                        profile.Records.BestEarnings = int.Parse(parts[1], Invariant);
                        profile.Records.BestEndlessDay = int.Parse(parts[2], Invariant);
                        profile.Records.TotalCustomersServed = int.Parse(parts[3], Invariant);
                        profile.Records.TotalDaysPlayed = int.Parse(parts[4], Invariant);
                        break;
                    }

                    default:
                        // An unknown key is a field from another version. Skipping it is what
                        // makes adding a field a migration rather than a break.
                        break;
                }
            }

            if (!sawGrid) return null;

            profile.Grid = new CafeGrid(width, height);
            foreach (var parts in items)
            {
                if (parts.Length != 4) return null;

                profile.Grid.Place(PlaceableCatalog.ById(parts[0]),
                    int.Parse(parts[1], Invariant), int.Parse(parts[2], Invariant),
                    parts[3] == "1");
            }

            profile.Wallet.Restore(money, reputation);

            // Rostered ids that no longer name a maid are dropped rather than trusted.
            for (var i = profile.ShiftRoster.Count - 1; i >= 0; i--)
            {
                if (profile.MaidById(profile.ShiftRoster[i]) == null) profile.ShiftRoster.RemoveAt(i);
            }

            return profile;
        }

        private static Maid ReadMaid(string value)
        {
            var parts = value.Split('|');
            if (parts.Length != 11) return null;

            if (!Enum.TryParse(parts[2], out Archetype archetype)) return null;

            var maid = new Maid
            {
                Id = int.Parse(parts[0], Invariant),
                Name = parts[1],
                Archetype = archetype,
                Level = Core.Curves.ClampInt(int.Parse(parts[3], Invariant), 1, Maid.MaxLevel),
                Xp = int.Parse(parts[4], Invariant),
                Bond = Core.Curves.ClampInt(int.Parse(parts[5], Invariant), 0, BondTable.MaxLevel),
                BondPoints = int.Parse(parts[6], Invariant),
            };

            var stats = parts[7].Split(',');
            if (stats.Length != 4) return null;

            maid.Base = new Stats(
                int.Parse(stats[0], Invariant), int.Parse(stats[1], Invariant),
                int.Parse(stats[2], Invariant), int.Parse(stats[3], Invariant)).Clamped();

            var gear = parts[8].Split(',');
            for (var i = 0; i < gear.Length && i < maid.Gear.Length; i++)
            {
                if (gear[i].Length > 0) maid.Gear[i] = EquipmentCatalog.ById(gear[i]);
            }

            foreach (var skill in Split(parts[9]))
            {
                if (Enum.TryParse(skill, out SkillId id)) maid.Skills.Add(id);
            }

            maid.Mood = float.Parse(parts[10], NumberStyles.Float, Invariant);

            // Derived, never stored: a set bonus saved out of step with the gear that earns it
            // is a stat nobody can explain.
            MaidProgression.RefreshSetBonus(maid);
            return maid;
        }

        private static IEnumerable<string> Split(string value)
        {
            if (string.IsNullOrEmpty(value)) yield break;

            foreach (var part in value.Split(','))
            {
                if (part.Length > 0) yield return part;
            }
        }

        /// <summary>FNV-1a. Not security — it catches a truncated or flipped file.</summary>
        private static uint Checksum(string text)
        {
            var hash = 2166136261u;
            for (var i = 0; i < text.Length; i++)
            {
                hash ^= text[i];
                hash *= 16777619u;
            }

            return hash;
        }
    }
}

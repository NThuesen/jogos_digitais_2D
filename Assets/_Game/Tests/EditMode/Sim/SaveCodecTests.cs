using System;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using Neko.Sim.Cafe;
using Neko.Sim.Economy;
using Neko.Sim.Maids;
using Neko.Sim.Progress;
using Neko.Sim.Save;
using NUnit.Framework;

namespace Neko.Tests.Sim
{
    public sealed class SaveCodecTests
    {
        private static Profile Played()
        {
            var profile = Profile.NewGame(4242u);
            profile.Day = 9;
            profile.QuotaStep = 7;
            profile.Wallet.Earn(1234);
            profile.Wallet.AddReputation(CafeLevelTable.ReputationForLevel(4));
            profile.EndlessUnlocked = true;
            profile.Records.BestDay = 9;
            profile.Records.BestEarnings = 980;
            profile.Records.TotalCustomersServed = 61;
            profile.Records.TotalDaysPlayed = 8;
            profile.OwnedPlaceables.Add("plant");
            profile.OwnedPlaceables.Add("lantern_sakura");
            profile.OwnedEquipment.Add("acc_timer");
            profile.ApplyUnlocks();

            profile.Grid.Place(PlaceableCatalog.ById("lantern_sakura"), 1, 0);
            profile.Grid.Place(PlaceableCatalog.ById("table_2"), 5, 4, rotated: true);

            var maid = profile.Maids[0];
            MaidProgression.GrantXp(maid, MaidMath.XpForLevel(9), out _);
            MaidProgression.EquipSkill(maid, SkillId.QuickFeet, out _);
            MaidProgression.EquipSkill(maid, SkillId.Usher, out _);
            MaidProgression.Equip(maid, EquipmentCatalog.ById("head_sakura"), out _);
            MaidProgression.Equip(maid, EquipmentCatalog.ById("outfit_sakura"), out _);
            MaidProgression.Equip(maid, EquipmentCatalog.ById("acc_sakura"), out _);
            MaidProgression.GiveGift(maid, 55, out _);
            maid.Mood = 0.42f;

            return profile;
        }

        [Test]
        public void RoundTripPreservesTheProfile()
        {
            var original = Played();

            var loaded = SaveCodec.Decode(SaveCodec.Encode(original)).Profile;

            Assert.AreEqual(original.Seed, loaded.Seed);
            Assert.AreEqual(original.Day, loaded.Day);
            Assert.AreEqual(original.QuotaStep, loaded.QuotaStep);
            Assert.AreEqual(original.Wallet.Money, loaded.Wallet.Money);
            Assert.AreEqual(original.Wallet.Reputation, loaded.Wallet.Reputation);
            Assert.AreEqual(original.CafeLevel, loaded.CafeLevel);
            Assert.AreEqual(original.EndlessUnlocked, loaded.EndlessUnlocked);
            Assert.AreEqual(original.NextMaidId, loaded.NextMaidId);
            CollectionAssert.AreEqual(original.ShiftRoster, loaded.ShiftRoster);
            CollectionAssert.AreEqual(original.OwnedPlaceables, loaded.OwnedPlaceables);
            CollectionAssert.AreEqual(original.OwnedEquipment, loaded.OwnedEquipment);
        }

        [Test]
        public void RoundTripPreservesTheRoom()
        {
            var original = Played();

            var loaded = SaveCodec.Decode(SaveCodec.Encode(original)).Profile;

            Assert.AreEqual(original.Grid.Width, loaded.Grid.Width);
            Assert.AreEqual(original.Grid.Height, loaded.Grid.Height);
            Assert.AreEqual(original.Grid.Items.Count, loaded.Grid.Items.Count);

            foreach (var item in original.Grid.Items)
            {
                var same = loaded.Grid.ItemAt(item.X, item.Y);
                Assert.IsNotNull(same, item.Def.Id + " at " + item.X + "," + item.Y);
                Assert.AreEqual(item.Def.Id, same.Def.Id);
                Assert.AreEqual(item.Rotated, same.Rotated);
            }
        }

        [Test]
        public void RoundTripPreservesEveryAxisOfAMaid()
        {
            var original = Played();
            var before = original.Maids[0];

            var after = SaveCodec.Decode(SaveCodec.Encode(original)).Profile.MaidById(before.Id);

            Assert.IsNotNull(after);
            Assert.AreEqual(before.Name, after.Name);
            Assert.AreEqual(before.Archetype, after.Archetype);
            Assert.AreEqual(before.Level, after.Level);
            Assert.AreEqual(before.Xp, after.Xp);
            Assert.AreEqual(before.Bond, after.Bond);
            Assert.AreEqual(before.BondPoints, after.BondPoints);
            Assert.AreEqual(before.Base, after.Base);
            Assert.AreEqual(before.Total, after.Total);
            Assert.That(after.Mood, Is.EqualTo(before.Mood).Within(0.002f));
            CollectionAssert.AreEqual(before.Skills, after.Skills);

            for (var i = 0; i < before.Gear.Length; i++)
            {
                Assert.AreEqual(before.Gear[i]?.Id, after.Gear[i]?.Id, "slot " + i);
            }
        }

        [Test]
        public void TheSetBonusIsRecomputedRatherThanTrusted()
        {
            var original = Played();
            Assert.AreNotEqual(Stats.Zero, original.Maids[0].SetBonus, "precondition: a set is worn");

            var after = SaveCodec.Decode(SaveCodec.Encode(original)).Profile.Maids[0];

            Assert.AreEqual(original.Maids[0].SetBonus, after.SetBonus);
        }

        [Test]
        public void EncodingIsStable()
        {
            var profile = Played();

            Assert.AreEqual(SaveCodec.Encode(profile), SaveCodec.Encode(profile));
        }

        [Test]
        public void AFreshProfileSurvivesTheRoundTrip()
        {
            var loaded = SaveCodec.Decode(SaveCodec.Encode(Profile.NewGame(1u)));

            Assert.AreEqual(LoadStatus.Ok, loaded.Status);
            Assert.AreEqual(2, loaded.Profile.Maids.Count);
            Assert.AreEqual(2, loaded.Profile.ShiftRoster.Count);
        }

        [Test]
        public void AnEmptyOrNullPayloadYieldsEmptyAndAPlayableProfile()
        {
            foreach (var payload in new[] { null, "" })
            {
                var result = SaveCodec.Decode(payload);

                Assert.AreEqual(LoadStatus.Empty, result.Status);
                Assert.IsNotNull(result.Profile);
                Assert.AreEqual(2, result.Profile.Maids.Count);
                Assert.IsEmpty(result.Message, "a first run is not an error to report");
            }
        }

        [Test]
        public void ATruncatedPayloadIsCorruptAndNeverThrows()
        {
            var payload = SaveCodec.Encode(Played());

            for (var cut = 1; cut < payload.Length; cut += 7)
            {
                LoadResult result = default;
                Assert.DoesNotThrow(() => result = SaveCodec.Decode(payload.Substring(0, cut)),
                    "cut at " + cut);

                Assert.AreNotEqual(LoadStatus.Ok, result.Status, "cut at " + cut);
                Assert.IsNotNull(result.Profile, "cut at " + cut);
            }
        }

        [Test]
        public void AFlippedByteFailsTheChecksum()
        {
            var payload = SaveCodec.Encode(Played());
            var parts = payload.Split(':');
            var bytes = Convert.FromBase64String(parts[1]);
            bytes[bytes.Length / 2] ^= 0x20;
            var tampered = parts[0] + ":" + Convert.ToBase64String(bytes) + ":" + parts[2];

            var result = SaveCodec.Decode(tampered);

            Assert.AreEqual(LoadStatus.Corrupt, result.Status);
            Assert.IsNotNull(result.Profile);
        }

        [Test]
        public void ANewerSchemaVersionIsRefusedRatherThanMisread()
        {
            var payload = SaveCodec.Encode(Played());
            var parts = payload.Split(':');
            var fromTheFuture = "NEKO" + (SaveCodec.CurrentVersion + 1) + ":" + parts[1] + ":" + parts[2];

            var result = SaveCodec.Decode(fromTheFuture);

            Assert.AreEqual(LoadStatus.Corrupt, result.Status);
            StringAssert.Contains("nova", result.Message);
        }

        [Test]
        public void SomethingFromAnotherProgramIsCorruptNotCrash()
        {
            foreach (var payload in new[]
                     {
                         "{}", "{\"day\": 4}", "hello", "::", "NEKO:::", "NEKOx:a:b",
                         "NEKO1:not-base64!!:00000000", "NEKO1::", "\0\0\0",
                     })
            {
                LoadResult result = default;
                Assert.DoesNotThrow(() => result = SaveCodec.Decode(payload), payload);

                Assert.AreEqual(LoadStatus.Corrupt, result.Status, payload);
                Assert.IsNotNull(result.Profile, payload);
                Assert.IsNotEmpty(result.Message, payload);
            }
        }

        [Test]
        public void TheCorruptMessageIsPortugueseAndSaysWhatHappensNext()
        {
            var result = SaveCodec.Decode("lixo");

            StringAssert.Contains("café novo", result.Message);
        }

        [Test]
        public void AnOlderSchemaVersionIsMigratedAndSaysSo()
        {
            // Version 0 of the format is the same body; the path exists so that adding a
            // field later is a migration rather than a break.
            var payload = SaveCodec.Encode(Played());
            var parts = payload.Split(':');
            var older = "NEKO0:" + parts[1] + ":" + parts[2];

            var result = SaveCodec.Decode(older);

            Assert.AreEqual(LoadStatus.Migrated, result.Status);
            Assert.AreEqual(9, result.Profile.Day);
            Assert.IsNotEmpty(result.Message);
        }

        [Test]
        public void AnUnknownKeyIsSkippedRatherThanRejected()
        {
            var profile = Played();
            var parts = SaveCodec.Encode(profile).Split(':');
            var body = Encoding.UTF8.GetString(Convert.FromBase64String(parts[1]));
            body += "colour=blue\nfuture_thing=42\n";

            var rebuilt = "NEKO1:" + Convert.ToBase64String(Encoding.UTF8.GetBytes(body)) + ":" +
                          Checksum(body);

            var result = SaveCodec.Decode(rebuilt);

            Assert.AreEqual(LoadStatus.Ok, result.Status);
            Assert.AreEqual(profile.Day, result.Profile.Day);
        }

        [Test]
        public void ARosteredMaidWhoNoLongerExistsIsDroppedQuietly()
        {
            var profile = Played();
            profile.ShiftRoster.Add(9999);

            var loaded = SaveCodec.Decode(SaveCodec.Encode(profile)).Profile;

            CollectionAssert.DoesNotContain(loaded.ShiftRoster, 9999);
            Assert.IsTrue(loaded.ShiftRoster.All(id => loaded.MaidById(id) != null));
        }

        [Test]
        public void ASaveWrittenUnderACommaDecimalCultureLoadsUnderADotOne()
        {
            // The real failure this guards: a Brazilian machine writes "0,42" for a mood and
            // every other machine reads it as the number 42, or not at all.
            var previous = Thread.CurrentThread.CurrentCulture;
            try
            {
                Thread.CurrentThread.CurrentCulture = new CultureInfo("pt-BR");
                var payload = SaveCodec.Encode(Played());

                Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
                var result = SaveCodec.Decode(payload);

                Assert.AreEqual(LoadStatus.Ok, result.Status);
                Assert.That(result.Profile.Maids[0].Mood, Is.EqualTo(0.42f).Within(0.002f));
            }
            finally
            {
                Thread.CurrentThread.CurrentCulture = previous;
            }
        }

        [Test]
        public void EncodingANullProfileIsAnEmptyStringRatherThanAThrow()
        {
            Assert.AreEqual("", SaveCodec.Encode(null));
        }

        [Test]
        public void ASaveOfEveryArchetypeAndEveryPieceOfGearSurvives()
        {
            var profile = Profile.NewGame(1u);
            profile.Maids.Clear();
            profile.ShiftRoster.Clear();
            profile.Wallet.AddReputation(CafeLevelTable.ReputationForLevel(10));

            var id = 1;
            foreach (var archetype in ArchetypeTable.All)
            {
                var maid = Maid.Create(id++, "Teste" + id, archetype.Kind, 20);
                foreach (GearSlot slot in Enum.GetValues(typeof(GearSlot)))
                {
                    MaidProgression.Equip(maid, EquipmentCatalog.ForSlot(slot, 10).Last(), out _);
                }

                profile.Maids.Add(maid);
            }

            var loaded = SaveCodec.Decode(SaveCodec.Encode(profile)).Profile;

            Assert.AreEqual(ArchetypeTable.All.Count, loaded.Maids.Count);
            for (var i = 0; i < profile.Maids.Count; i++)
            {
                Assert.AreEqual(profile.Maids[i].Archetype, loaded.Maids[i].Archetype);
                Assert.AreEqual(profile.Maids[i].Total, loaded.Maids[i].Total);
            }
        }

        private static string Checksum(string text)
        {
            var hash = 2166136261u;
            for (var i = 0; i < text.Length; i++)
            {
                hash ^= text[i];
                hash *= 16777619u;
            }

            return hash.ToString("x8", CultureInfo.InvariantCulture);
        }
    }
}

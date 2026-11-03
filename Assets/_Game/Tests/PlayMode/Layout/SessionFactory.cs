using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Text;
using Neko.Game.Core;
using Neko.Game.Feel;
using Neko.Game.Ui;
using Neko.Sim.Cafe;
using Neko.Sim.Core;
using Neko.Sim.Economy;
using Neko.Sim.Maids;
using Neko.Sim.Progress;
using Neko.Sim.Save;
using Neko.Sim.Team;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using GameScreen = Neko.Game.Ui.Screen;

namespace Neko.Tests.PlayMode.Layout
{
    /// <summary>
    /// Builds the sessions the Layout_Check measures: a crafted profile, saved, the game booted on
    /// it, and walked to a Screen through the public entry points that the game's planks call.
    /// </summary>
    /// <remarks>
    /// Determinism comes from the save rather than from <c>MenuScreen.OnNewGame</c>, which seeds a
    /// new business from <c>DateTime.Now.Ticks</c>. <see cref="NewProfile"/> starts from
    /// <c>Profile.NewGame(seed)</c> and sets every value a state depends on explicitly;
    /// <see cref="Boot"/> writes it with <c>SaveBridge.Save</c> and loads <c>Boot.unity</c>, whose
    /// <c>GameRoot</c> decodes that save, then checks that the session holds it. <c>GameSession</c>
    /// seeds its <c>Rng</c> from <c>Profile.Seed</c>, so box openings and days replay exactly.
    ///
    /// The save lives in PlayerPrefs, which survives between runs and is the store the Editor's own
    /// play mode reads. <see cref="Boot"/> therefore writes before every load, so a stale save never
    /// leaks in, and <see cref="RestoreSave"/> puts back the save that was there before, so a crafted
    /// profile does not leak out into the next test or into the developer's game.
    ///
    /// The value providers, <c>LargeValues</c>, <c>Longest</c> and <c>ReplaceDisplay</c>, are in
    /// SessionValues.cs.
    /// </remarks>
    public static partial class SessionFactory
    {
        /// <summary>The scene the game boots from.</summary>
        public const string ScenePath = "Assets/_Game/Scenes/Boot.unity";

        /// <summary>Batch mode renders the first frames before material albedo is ready.</summary>
        public const int DiscardFrames = 12;

        /// <summary>How many tween ticks <see cref="Settle"/> runs before it checks that nothing moves.</summary>
        /// <remarks>
        /// 20 ticks of <see cref="SettleTick"/> are 2 s of UI time. The longest one-shot tween, the
        /// card reveal of the Caixas, takes about 1 s.
        /// </remarks>
        public const int SettleTicks = 20;

        /// <summary>The UI time, in seconds, of one tween tick of <see cref="Settle"/>.</summary>
        public const float SettleTick = 0.1f;

        /// <summary>How far, in Canvas_Units, an edge may move in the last tick of <see cref="Settle"/>.</summary>
        /// <remarks>Far below any visible motion; it only absorbs float noise in a repeated layout pass.</remarks>
        public const float SettledTolerance = 0.001f;

        /// <summary>The seed of the one <c>Rng</c> a <see cref="ProfileBuilder"/> mints its maids with.</summary>
        public const uint MintSeed = 0x4E454B4Fu;

        private static readonly CultureInfo Invariant = CultureInfo.InvariantCulture;

        /// <summary>Whether <see cref="RememberSave"/> holds the save to put back.</summary>
        private static bool _saveRemembered;

        /// <summary>The save payload that was there before the first boot; null when there was none.</summary>
        private static string _rememberedSave;

        // -------------------------------------------------------------------------------
        // Profiles and boot
        // -------------------------------------------------------------------------------

        /// <summary>
        /// A <see cref="ProfileBuilder"/> on <c>Profile.NewGame(seed)</c> at
        /// <paramref name="cafeLevel"/>, with the reputation, the room and the lineup slots that
        /// level has.
        /// </summary>
        public static ProfileBuilder NewProfile(uint seed, int cafeLevel) => new ProfileBuilder(seed, cafeLevel);

        /// <summary>
        /// Saves <paramref name="profile"/>, loads <c>Boot.unity</c> on it and discards
        /// <see cref="DiscardFrames"/> frames; fails unless the session holds the profile and the
        /// game is on the Menu with no Dialog open.
        /// </summary>
        /// <remarks>
        /// The profile is first decoded back from the save, so a value the save cannot hold fails
        /// here by name, before the game is involved. The game's clock is stopped while the scene
        /// loads, because a day still running from the last test could close and save over the
        /// profile in those frames; <see cref="LoadBoot"/> starts it again. The comparison covers
        /// every value <see cref="Describe"/> lists, and the first that differs is reported.
        /// </remarks>
        public static IEnumerator Boot(Profile profile)
        {
            Assert.IsTrue(profile != null, "SessionFactory.Boot: there is no profile to boot");

            PauseClock();
            RememberSave();
            SaveBridge.Save(profile);

            var expected = Describe(profile);
            var loaded = SaveBridge.Load();
            Assert.IsTrue(loaded.Status == LoadStatus.Ok,
                "SessionFactory.Boot: the crafted profile does not load back from the save: " + loaded.Status +
                (string.IsNullOrEmpty(loaded.Message) ? "" : ", \"" + loaded.Message + "\""));
            AssertHolds(expected, Describe(loaded.Profile), "the save does not hold the crafted profile");

            yield return LoadBoot();

            var root = GameRoot.Instance;
            Assert.IsTrue(root.Session != null, "SessionFactory.Boot: GameRoot woke up without a session");
            AssertHolds(expected, Describe(root.Session.Profile), "the session does not hold the saved profile");
            Assert.IsTrue(root.Ui.Current is MenuScreen,
                "SessionFactory.Boot: the game came up on " + NameOf(root.Ui.Current) + ", not on the Menu");
            Assert.IsFalse(root.Ui.DialogOpen,
                "SessionFactory.Boot: a Dialog is open over the Menu after boot: " + DialogText(root.Ui));
        }

        /// <summary>
        /// Loads <c>Boot.unity</c> on whatever is saved and discards <see cref="DiscardFrames"/>
        /// frames; fails if <c>GameRoot</c> does not wake up.
        /// </summary>
        /// <remarks>
        /// The clocks are static, and <c>GameRoot</c> does not reset them at boot, so this resets them
        /// to the state a fresh process boots in. <see cref="Boot"/> is the way in for a crafted
        /// profile; this alone is for a state that writes its own save, a corrupt one for example.
        /// </remarks>
        public static IEnumerator LoadBoot()
        {
            yield return SceneManager.LoadSceneAsync(ScenePath, LoadSceneMode.Single);
            Clocks.Reset();

            for (var i = 0; i < DiscardFrames; i++) yield return null;

            var root = GameRoot.Instance;
            Assert.IsTrue(root != null && root.Ui != null, "SessionFactory: GameRoot never woke up after loading " + ScenePath);
        }

        /// <summary>
        /// Puts back the save that was there before the first <see cref="Boot"/> since the last
        /// restore, or deletes the save when there was none. For <c>[TearDown]</c>.
        /// </summary>
        /// <remarks>
        /// The scene stays loaded after the test, and the game saves whenever the business changes:
        /// a day that <see cref="RestoreClock"/> lets run again can still close and save its books
        /// before the next load replaces the scene. A state that leaves a day close to its end
        /// should end it inside the test.
        /// </remarks>
        public static void RestoreSave()
        {
            if (!_saveRemembered) return;

            if (_rememberedSave == null) PlayerPrefs.DeleteKey(SaveBridge.Key);
            else PlayerPrefs.SetString(SaveBridge.Key, _rememberedSave);
            PlayerPrefs.Save();

            _saveRemembered = false;
            _rememberedSave = null;
        }

        /// <summary>
        /// Every value of <paramref name="profile"/> that a save holds and a state may set, one line
        /// per group, in a form two profiles can be compared by.
        /// </summary>
        public static string Describe(Profile profile)
        {
            Assert.IsTrue(profile != null, "SessionFactory.Describe: there is no profile to describe");

            var text = new StringBuilder();
            var wallet = profile.Wallet;
            var records = profile.Records;

            text.Append("seed ").Append(profile.Seed).Append(", next maid id ").Append(profile.NextMaidId)
                .Append(", bankrupt ").Append(profile.Bankrupt ? "yes" : "no").Append('\n');
            text.Append("day ").Append(profile.Day).Append(", money ").Append(wallet.Money).Append(", fichas ")
                .Append(wallet.Points).Append(", reputation ").Append(wallet.Reputation).Append(" (café level ")
                .Append(profile.CafeLevel).Append(")\n");
            text.Append("records: best day ").Append(records.BestDay).Append(", best revenue ").Append(records.BestRevenue)
                .Append(", customers served ").Append(records.TotalCustomersServed).Append(", days played ")
                .Append(records.TotalDaysPlayed).Append(", businesses lost ").Append(records.BusinessesLost)
                .Append(", legendaries found ").Append(records.LegendariesFound).Append('\n');

            var items = new List<string>();
            foreach (var item in profile.Grid.Items)
            {
                items.Add(item.Def.Id + " at " + item.X + "," + item.Y + (item.Rotated ? " rotated" : ""));
            }

            text.Append("room ").Append(profile.Grid.Width).Append('x').Append(profile.Grid.Height).Append(": ")
                .Append(items.Count > 0 ? string.Join("; ", items) : "empty").Append('\n');

            foreach (var maid in profile.Maids) text.Append(DescribeMaid(maid)).Append('\n');

            var slots = new List<string>();
            foreach (var slot in profile.Lineup.All)
            {
                slots.Add(slot.Role + " " + slot.Index + " " + (slot.Empty ? "empty" : "maid " + slot.MaidId));
            }

            text.Append("lineup: ").Append(string.Join(", ", slots)).Append('\n');
            text.Append("stock: placeables ").Append(Counts(profile.OwnedPlaceables)).Append("; equipment ")
                .Append(Counts(profile.OwnedEquipment));
            return text.ToString();
        }

        // -------------------------------------------------------------------------------
        // Navigation
        // -------------------------------------------------------------------------------

        /// <summary>Menu → Empresa, through <c>MenuScreen.OnPlay</c>, the Menu's play plank.</summary>
        /// <remarks>A bankrupt business goes to the Falência instead, and this fails.</remarks>
        public static IEnumerator ToHub() =>
            Go<MenuScreen, HubScreen>("ToHub", "MenuScreen.OnPlay", menu => menu.OnPlay);

        /// <summary>Empresa → Escalação, through <c>HubScreen.OnLineup</c>.</summary>
        public static IEnumerator ToLineup() =>
            Go<HubScreen, LineupScreen>("ToLineup", "HubScreen.OnLineup", hub => hub.OnLineup);

        /// <summary>Empresa → Elenco, through <c>HubScreen.OnRoster</c>.</summary>
        public static IEnumerator ToRoster() =>
            Go<HubScreen, RosterScreen>("ToRoster", "HubScreen.OnRoster", hub => hub.OnRoster);

        /// <summary>Empresa → Caixas, through <c>HubScreen.OnBoxes</c>.</summary>
        public static IEnumerator ToBoxes() =>
            Go<HubScreen, BoxScreen>("ToBoxes", "HubScreen.OnBoxes", hub => hub.OnBoxes);

        /// <summary>Empresa → Loja, through <c>HubScreen.OnShop</c>.</summary>
        public static IEnumerator ToShop() =>
            Go<HubScreen, ShopScreen>("ToShop", "HubScreen.OnShop", hub => hub.OnShop);

        /// <summary>Empresa → Salão, through <c>HubScreen.OnHall</c>.</summary>
        public static IEnumerator ToHall() =>
            Go<HubScreen, HallScreen>("ToHall", "HubScreen.OnHall", hub => hub.OnHall);

        /// <summary>
        /// Empresa → O dia, through <c>HubScreen.OnOpenDay</c>, with the game clock paused; fails
        /// unless a day is running under the <c>DayHud</c>.
        /// </summary>
        /// <remarks>
        /// <c>GameRoot.StartDay</c> resets the clocks, so the clock is paused right after it, in the
        /// same frame: no game time passes and the room stays frozen. A state that needs the day to
        /// move ticks the shift itself. <see cref="RestoreClock"/> undoes the pause.
        /// </remarks>
        public static IEnumerator ToDay()
        {
            yield return Go<HubScreen, DayHud>("ToDay", "HubScreen.OnOpenDay", hub => hub.OnOpenDay, PauseClock);

            var root = GameRoot.Instance;
            Assert.IsTrue(root.State.Current == AppState.Shift && root.Presenter.Shift != null,
                "SessionFactory.ToDay: the DayHud is up, but no day is running (the game is in " + root.State.Current + ")");
        }

        /// <summary>
        /// Escalação, Elenco, Caixas, Loja or Salão → Empresa, through the current Screen's
        /// <c>OnBack</c>, its "Voltar" plank.
        /// </summary>
        public static IEnumerator BackToHub()
        {
            var ui = Root("BackToHub").Ui;
            var current = ui.Current;

            Action back;
            switch (current)
            {
                case LineupScreen lineup:
                    back = lineup.OnBack;
                    break;
                case RosterScreen roster:
                    back = roster.OnBack;
                    break;
                case BoxScreen boxes:
                    back = boxes.OnBack;
                    break;
                case ShopScreen shop:
                    back = shop.OnBack;
                    break;
                case HallScreen hall:
                    back = hall.OnBack;
                    break;
                default:
                    Assert.Fail("SessionFactory.BackToHub: the current Screen, " + NameOf(current) +
                                ", has no \"Voltar\" back to Empresa");
                    yield break;
            }

            var entry = current.GetType().Name + ".OnBack";
            RequireReachable(ui, current, current.GetType().Name, "BackToHub", entry);
            Assert.IsTrue(back != null, "SessionFactory.BackToHub: " + entry + " is not wired, so it leads nowhere");

            var failures = ErrorBoundary.FailureCount;
            back();
            yield return null;

            RequireCurrent<HubScreen>(ui, "BackToHub", entry, failures);
        }

        /// <summary>Empresa → Menu, through <c>HubScreen.OnMenu</c>, its "Voltar" plank.</summary>
        /// <remarks>The game saves the session on the way, and the Menu shows its records from then on.</remarks>
        public static IEnumerator BackToMenu() =>
            Go<HubScreen, MenuScreen>("BackToMenu", "HubScreen.OnMenu", hub => hub.OnMenu);

        // -------------------------------------------------------------------------------
        // Settled and the clock
        // -------------------------------------------------------------------------------

        /// <summary>
        /// The design's Settled: one-shot tweens run out, the layout rebuilt and one frame drawn; then
        /// one more tween tick must leave every element of the Active_Layer where it was.
        /// </summary>
        /// <remarks>
        /// Runs <see cref="SettleTicks"/> ticks of <see cref="SettleTick"/>,
        /// <c>Canvas.ForceUpdateCanvases()</c> and one frame. It then records the canvas bounds of
        /// every active RectTransform of the Active_Layer and the text of every active Text, ticks
        /// once more, rebuilds the layout and fails with the first element whose bounds moved by
        /// more than <see cref="SettledTolerance"/> or whose text changed. A tween tick moves what a
        /// tween drives; motion driven by a frame, ScrollRect inertia for example, is the caller's
        /// to stop. The Menu's sign and parade sway forever on tweens, so a Menu state stops those
        /// tweens and sets the angles it measures before it settles.
        /// </remarks>
        public static IEnumerator Settle()
        {
            for (var i = 0; i < SettleTicks; i++) Tween.TickAll(SettleTick);
            Canvas.ForceUpdateCanvases();
            yield return null;

            var ui = Root("Settle").Ui;
            var layer = LayoutModel.ActiveLayer(ui);
            Canvas.ForceUpdateCanvases();
            var before = Snapshot(ui, layer);

            Tween.TickAll(SettleTick);
            Canvas.ForceUpdateCanvases();
            var after = Snapshot(ui, LayoutModel.ActiveLayer(ui));

            var motion = FirstMotion(before, after);
            if (motion == null) yield break;

            Assert.Fail("SessionFactory.Settle: " + motion + " in one more tick of " + Seconds(SettleTick) +
                        " after " + Seconds(SettleTicks * SettleTick) + " of UI time, so " +
                        LayoutModel.PathOf(layer) + " has not settled");
        }

        /// <summary>Stops the game clock, for O dia, so the room is frozen. <see cref="ToDay"/> calls it.</summary>
        public static void PauseClock() => Clocks.Pause(true);

        /// <summary>
        /// Undoes <see cref="PauseClock"/>, for <c>[TearDown]</c>: the clocks are back to what the
        /// game resets them to whenever it enters a Screen, running at normal speed.
        /// </summary>
        public static void RestoreClock() => Clocks.Reset();

        // -------------------------------------------------------------------------------
        // Reflection
        // -------------------------------------------------------------------------------

        /// <summary>
        /// Finds the field, property or method <paramref name="name"/> of <paramref name="type"/> that
        /// a session builder reaches through reflection, or fails with the type and member name.
        /// </summary>
        /// <remarks>
        /// Every reflection lookup of the session builders goes through here, so a private member
        /// that is renamed or removed fails the Layout_Check by name rather than with a null
        /// reference. An overloaded method is refused; <see cref="LayoutModel.RequireMethod"/> takes
        /// the parameter types that tell overloads apart.
        /// </remarks>
        public static T RequireMember<T>(Type type, string name, BindingFlags flags) where T : MemberInfo
        {
            Assert.IsTrue(type != null, "SessionFactory.RequireMember: there is no type to find " + name + " on");

            T found = null;
            var matches = 0;
            foreach (var member in type.GetMember(name, flags))
            {
                if (!(member is T typed)) continue;

                found = typed;
                matches++;
            }

            if (matches == 1) return found;

            Assert.Fail("SessionFactory: the " + KindOf(typeof(T)) + " " + type.FullName + "." + name + " (" + flags +
                        ") " + (matches == 0 ? "was not found" : "has " + matches + " overloads") +
                        ", so the session cannot be built through it");
            return null;
        }

        // -------------------------------------------------------------------------------
        // Helpers
        // -------------------------------------------------------------------------------

        /// <summary>Remembers the save that is there now, unless one is already remembered.</summary>
        private static void RememberSave()
        {
            if (_saveRemembered) return;

            _rememberedSave = PlayerPrefs.HasKey(SaveBridge.Key) ? PlayerPrefs.GetString(SaveBridge.Key) : null;
            _saveRemembered = true;
        }

        /// <summary>
        /// Presses <paramref name="entry"/> on <typeparamref name="TFrom"/>, waits a frame and fails
        /// unless <typeparamref name="TTo"/> is the current Screen and the Active_Layer.
        /// </summary>
        /// <param name="then">Runs right after the entry point, in the same frame.</param>
        private static IEnumerator Go<TFrom, TTo>(string caller, string entry, Func<TFrom, Action> pick,
            Action then = null) where TFrom : GameScreen where TTo : GameScreen
        {
            var ui = Root(caller).Ui;
            var from = ui.Get<TFrom>();
            RequireReachable(ui, from, typeof(TFrom).Name, caller, entry);

            var action = pick(from);
            Assert.IsTrue(action != null, "SessionFactory." + caller + ": " + entry + " is not wired, so it leads nowhere");

            var failures = ErrorBoundary.FailureCount;
            action();
            then?.Invoke();
            yield return null;

            RequireCurrent<TTo>(ui, caller, entry, failures);
        }

        /// <summary>Fails unless <paramref name="screen"/> is current and nothing is over it, as a player would need.</summary>
        private static void RequireReachable(UiRoot ui, GameScreen screen, string screenName, string caller, string entry)
        {
            Assert.IsTrue(screen != null && ui.Current == screen && screen.Visible,
                "SessionFactory." + caller + ": " + entry + " is on " + screenName + ", but the current Screen is " +
                NameOf(ui.Current));

            var layer = LayoutModel.ActiveLayer(ui);
            Assert.IsTrue(layer == screen.transform,
                "SessionFactory." + caller + ": " + LayoutModel.PathOf(layer) + " is over " + screenName +
                (ui.DialogOpen ? " (" + DialogText(ui) + ")" : "") + ", so a player could not reach " + entry);
        }

        /// <summary>Fails unless <typeparamref name="T"/> is the current Screen and the Active_Layer.</summary>
        private static void RequireCurrent<T>(UiRoot ui, string caller, string entry, int failuresBefore)
            where T : GameScreen
        {
            var current = ui.Current;
            var layer = LayoutModel.ActiveLayer(ui);
            if (current is T && current.Visible && layer == current.transform) return;

            var detail = "after " + entry + " the current Screen is " + NameOf(current) + ", not " + typeof(T).Name;
            if (ui.DialogOpen) detail += ", and a Dialog is open: " + DialogText(ui);
            else if (current is T) detail += ", under " + LayoutModel.PathOf(layer);

            if (ErrorBoundary.FailureCount > failuresBefore) detail += "; the game reported " + ErrorBoundary.LastFailure;
            Assert.Fail("SessionFactory." + caller + ": " + detail);
        }

        private static GameRoot Root(string caller)
        {
            var root = GameRoot.Instance;
            Assert.IsTrue(root != null && root.Ui != null, "SessionFactory." + caller + ": there is no GameRoot; boot the game first");
            return root;
        }

        private static string NameOf(GameScreen screen) => screen != null ? screen.GetType().Name : "none";

        /// <summary>The texts of the open Dialog, quoted, for a failure message.</summary>
        private static string DialogText(UiRoot ui)
        {
            var dialogs = ui.transform.Find("Dialogs");
            if (dialogs == null) return "(no dialog layer)";

            var texts = new List<string>();
            foreach (var text in dialogs.GetComponentsInChildren<Text>())
            {
                if (!string.IsNullOrEmpty(text.text)) texts.Add("\"" + text.text + "\"");
            }

            return texts.Count > 0 ? string.Join(" ", texts) : "(no text)";
        }

        /// <summary>Fails with the first line of two <see cref="Describe"/> results that differs.</summary>
        private static void AssertHolds(string expected, string actual, string what)
        {
            if (expected == actual) return;

            var want = expected.Split('\n');
            var got = actual.Split('\n');
            for (var i = 0; i < Math.Max(want.Length, got.Length); i++)
            {
                var wanted = i < want.Length ? want[i] : "(no line)";
                var found = i < got.Length ? got[i] : "(no line)";
                if (wanted == found) continue;

                Assert.Fail("SessionFactory.Boot: " + what + ". Line " + (i + 1) + " reads \"" + found +
                            "\" where the crafted profile has \"" + wanted + "\"");
            }
        }

        private static string DescribeMaid(Maid maid)
        {
            var traits = new List<string>();
            foreach (var trait in maid.Traits) traits.Add(trait.ToString());

            var gear = new List<string>();
            foreach (var item in maid.Gear) gear.Add(item != null ? item.Id : "-");

            var skills = new List<string>();
            foreach (var skill in maid.Skills) skills.Add(skill.ToString());

            var stats = maid.Base;
            return "maid " + maid.Id + " \"" + maid.Name + "\": " + maid.Archetype + ", " + maid.Rarity + ", level " +
                   maid.Level + " (" + maid.Xp + " xp), bond " + maid.Bond + " (" + maid.BondPoints +
                   " gift points), plays " + maid.NaturalRole + ", house " + maid.House + ", origin " + maid.Origin +
                   ", traits " + (traits.Count > 0 ? string.Join(" ", traits) : "none") + ", base stats " +
                   stats.Serve + "/" + stats.Cook + "/" + stats.Charm + "/" + stats.Stamina + ", gear " +
                   string.Join(",", gear) + ", skills " + (skills.Count > 0 ? string.Join(" ", skills) : "none") +
                   ", mood " + maid.Mood.ToString("0.###", Invariant);
        }

        /// <summary>Item ids with their counts, in the order they first appear.</summary>
        private static string Counts(IReadOnlyList<string> ids)
        {
            var order = new List<string>();
            var counts = new Dictionary<string, int>(StringComparer.Ordinal);
            foreach (var id in ids)
            {
                if (counts.TryGetValue(id, out var count))
                {
                    counts[id] = count + 1;
                    continue;
                }

                counts[id] = 1;
                order.Add(id);
            }

            if (order.Count == 0) return "none";

            var parts = new List<string>();
            foreach (var id in order) parts.Add(id + " x" + counts[id]);
            return string.Join(", ", parts);
        }

        private static string KindOf(Type memberType) =>
            typeof(FieldInfo).IsAssignableFrom(memberType) ? "field" :
            typeof(PropertyInfo).IsAssignableFrom(memberType) ? "property" :
            typeof(MethodInfo).IsAssignableFrom(memberType) ? "method" : "member";

        /// <summary>
        /// The canvas bounds of every active RectTransform under <paramref name="layer"/>, in
        /// hierarchy order, with the text of each active Text.
        /// </summary>
        private static List<Pose> Snapshot(UiRoot ui, Transform layer)
        {
            var canvas = (RectTransform)ui.transform;
            var origin = canvas.rect.min;
            var corners = new Vector3[4];
            var poses = new List<Pose>();

            foreach (var rect in layer.GetComponentsInChildren<RectTransform>())
            {
                rect.GetWorldCorners(corners);

                var min = new Vector2(float.PositiveInfinity, float.PositiveInfinity);
                var max = new Vector2(float.NegativeInfinity, float.NegativeInfinity);
                foreach (var corner in corners)
                {
                    var point = (Vector2)canvas.InverseTransformPoint(corner) - origin;
                    min = Vector2.Min(min, point);
                    max = Vector2.Max(max, point);
                }

                var text = rect.GetComponent<Text>();
                poses.Add(new Pose(LayoutModel.PathFrom(layer, rect), Rect.MinMaxRect(min.x, min.y, max.x, max.y),
                    text != null && text.isActiveAndEnabled ? text.text : null));
            }

            return poses;
        }

        /// <summary>The first difference between two snapshots, or null when there is none.</summary>
        private static string FirstMotion(List<Pose> before, List<Pose> after)
        {
            for (var i = 0; i < before.Count && i < after.Count; i++)
            {
                var was = before[i];
                var now = after[i];
                if (was.Path != now.Path) return "the hierarchy changed: " + was.Path + " became " + now.Path;

                if (!Same(was.Bounds, now.Bounds))
                {
                    return was.Path + " moved from " + Bounds(was.Bounds) + " to " + Bounds(now.Bounds);
                }

                if (was.Text != now.Text) return was.Path + " changed its text from \"" + was.Text + "\" to \"" + now.Text + "\"";
            }

            return before.Count == after.Count
                ? null
                : "the Active_Layer went from " + before.Count + " to " + after.Count + " active RectTransforms";
        }

        private static bool Same(Rect a, Rect b) =>
            Mathf.Abs(a.xMin - b.xMin) <= SettledTolerance && Mathf.Abs(a.xMax - b.xMax) <= SettledTolerance &&
            Mathf.Abs(a.yMin - b.yMin) <= SettledTolerance && Mathf.Abs(a.yMax - b.yMax) <= SettledTolerance;

        private static string Bounds(Rect rect) =>
            "x " + Number(rect.xMin) + ".." + Number(rect.xMax) + ", y " + Number(rect.yMin) + ".." + Number(rect.yMax);

        private static string Number(float value) => value.ToString("0.###", Invariant);

        private static string Seconds(float value) => value.ToString("0.##", Invariant) + " s";

        /// <summary>Where one RectTransform was, and what its Text read, in a <see cref="Snapshot"/>.</summary>
        private sealed class Pose
        {
            public Pose(string path, Rect bounds, string text)
            {
                Path = path;
                Bounds = bounds;
                Text = text;
            }

            public string Path { get; }

            /// <summary>The axis-aligned bounds of its four corners, in Canvas_Units.</summary>
            public Rect Bounds { get; }

            /// <summary>What its Text reads; null when it has no active Text.</summary>
            public string Text { get; }
        }
    }

    /// <summary>
    /// A <c>Profile</c> crafted for one Audit_State: <c>Profile.NewGame(seed)</c> with every value
    /// the state depends on set explicitly.
    /// </summary>
    /// <remarks>
    /// The café level is given up front, because the room and the lineup slots follow it: the
    /// builder sets the reputation that <c>CafeLevelTable</c> maps to the level and applies its
    /// unlocks, which grows the starting room and rebuilds the slots as loading a save does.
    ///
    /// The first <see cref="WithMaid(string, Rarity, int, int, TraitId[])"/> replaces the three
    /// openers that <c>NewGame</c> rolled, so a crafted roster holds exactly the maids it names,
    /// with ids from 1 and an empty lineup. Each maid comes out of <c>Lootbox.Mint</c> with one
    /// <c>Rng</c> seeded with <see cref="SessionFactory.MintSeed"/>, in the order the maids are
    /// added; then her name and traits are set, her level is reached by granting its XP and her
    /// bond by giving its gift points, so her stats are the ones play would give her. She joins the
    /// roster without <c>Profile.Add</c>'s count of legendaries found, so the records stay what
    /// <see cref="WithRecords"/> says.
    /// </remarks>
    public sealed class ProfileBuilder
    {
        private readonly Rng _rng = new Rng(SessionFactory.MintSeed);

        /// <summary>Whether the roster is the crafted one rather than the openers <c>NewGame</c> rolled.</summary>
        private bool _crafted;

        internal ProfileBuilder(uint seed, int cafeLevel)
        {
            Assert.IsTrue(cafeLevel >= 1 && cafeLevel <= CafeLevelTable.MaxLevel,
                "SessionFactory.NewProfile: there is no café level " + cafeLevel + "; the levels run from 1 to " +
                CafeLevelTable.MaxLevel);

            Profile = Profile.NewGame(seed);
            var wallet = Profile.Wallet;
            wallet.Restore(wallet.Money, CafeLevelTable.ReputationForLevel(cafeLevel), wallet.Points);
            Profile.ApplyUnlocks();

            Assert.AreEqual(cafeLevel, Profile.CafeLevel,
                "SessionFactory.NewProfile: the reputation CafeLevelTable gives for café level " + cafeLevel +
                " does not reach it");
        }

        /// <summary>The profile being built, for anything the setters do not cover.</summary>
        public Profile Profile { get; }

        /// <summary>The balance, which may be negative.</summary>
        public ProfileBuilder WithMoney(int money)
        {
            var wallet = Profile.Wallet;
            wallet.Restore(money, wallet.Reputation, wallet.Points);
            return this;
        }

        public ProfileBuilder WithFichas(int fichas)
        {
            Assert.IsTrue(fichas >= 0, "SessionFactory: fichas never go under, so there cannot be " + fichas);

            var wallet = Profile.Wallet;
            wallet.Restore(wallet.Money, wallet.Reputation, fichas);
            return this;
        }

        /// <summary>The day the business is on, from 1.</summary>
        public ProfileBuilder WithDay(int day)
        {
            Assert.IsTrue(day >= 1, "SessionFactory: the days start at 1, so there is no day " + day);

            Profile.Day = day;
            return this;
        }

        /// <summary>The bests the Menu shows, each set as given.</summary>
        public ProfileBuilder WithRecords(int bestDay, int bestRevenue, int customersServed, int daysPlayed,
            int businessesLost, int legendariesFound)
        {
            var records = Profile.Records;
            records.BestDay = bestDay;
            records.BestRevenue = bestRevenue;
            records.TotalCustomersServed = customersServed;
            records.TotalDaysPlayed = daysPlayed;
            records.BusinessesLost = businessesLost;
            records.LegendariesFound = legendariesFound;
            return this;
        }

        /// <summary>
        /// A maid minted at <paramref name="rarity"/>, with the archetype the mint rolled; her traits
        /// are exactly <paramref name="traits"/>, none when none are given.
        /// </summary>
        public ProfileBuilder WithMaid(string name, Rarity rarity, int level, int bond, params TraitId[] traits) =>
            Mint(name, null, rarity, level, bond, traits);

        /// <summary>
        /// A maid of <paramref name="archetype"/>, minted at <paramref name="rarity"/>; her traits are
        /// exactly <paramref name="traits"/>, none when none are given.
        /// </summary>
        /// <remarks>
        /// As <c>Profile.NewGame</c> rolls an opener: the archetype, the position it belongs in, and
        /// stats rolled for that archetype.
        /// </remarks>
        public ProfileBuilder WithMaid(string name, Archetype archetype, Rarity rarity, int level, int bond,
            params TraitId[] traits) =>
            Mint(name, archetype, rarity, level, bond, traits);

        /// <summary>The maid called <paramref name="name"/>; fails when there is none.</summary>
        public Maid MaidNamed(string name)
        {
            var maid = Find(name);
            if (maid != null) return maid;

            var names = new List<string>();
            foreach (var other in Profile.Maids) names.Add(other.Name);
            Assert.Fail("SessionFactory: the profile has no maid called " + name + "; it has " +
                        (names.Count > 0 ? string.Join(", ", names) : "none"));
            return null;
        }

        /// <summary>Puts the maid called <paramref name="maidName"/> in slot <paramref name="index"/> of <paramref name="role"/>.</summary>
        public ProfileBuilder Assign(Role role, int index, string maidName)
        {
            var maid = MaidNamed(maidName);
            var lineup = Profile.Lineup;
            Assert.IsTrue(lineup.Slot(role, index) != null,
                "SessionFactory: café level " + Profile.CafeLevel + " has no " + role + " slot " + index + "; its lineup has " +
                Slots(lineup.Slots));

            lineup.Assign(role, index, maid.Id);
            return this;
        }

        /// <summary>Fills the lineup from the roster, as the game does for a new business and after a box.</summary>
        public ProfileBuilder AutoFillLineup()
        {
            Profile.Lineup.AutoFill(Profile.Maids);
            return this;
        }

        /// <summary>Takes every item out of the room, which keeps the size the café level gives it.</summary>
        public ProfileBuilder EmptyRoom()
        {
            Profile.Grid = new CafeGrid(Profile.Grid.Width, Profile.Grid.Height);
            return this;
        }

        /// <summary>Places the placeable <paramref name="placeableId"/> in the room, its corner at <paramref name="x"/>, <paramref name="y"/>.</summary>
        public ProfileBuilder WithItem(string placeableId, int x, int y, bool rotated = false)
        {
            var def = PlaceableById(placeableId);
            Assert.IsTrue(def != null, "SessionFactory: no placeable has the id " + placeableId);

            var grid = Profile.Grid;
            Assert.IsTrue(grid.Place(def, x, y, rotated),
                "SessionFactory: " + placeableId + (rotated ? ", rotated," : "") + " does not fit at " + x + "," + y +
                " in the " + grid.Width + "x" + grid.Height + " room: it leaves the room, covers the door at " +
                grid.Door + " or overlaps another item");
            return this;
        }

        /// <summary>
        /// Owns exactly <paramref name="count"/> of the placeable or equipment
        /// <paramref name="itemId"/>, bought and not yet placed or worn.
        /// </summary>
        public ProfileBuilder WithStock(string itemId, int count)
        {
            Assert.IsTrue(count >= 0, "SessionFactory: there cannot be " + count + " of " + itemId + " in stock");

            List<string> stock = null;
            if (PlaceableById(itemId) != null) stock = Profile.OwnedPlaceables;
            else if (EquipmentById(itemId) != null) stock = Profile.OwnedEquipment;
            Assert.IsTrue(stock != null, "SessionFactory: no placeable or equipment has the id " + itemId);

            stock.RemoveAll(id => id == itemId);
            for (var i = 0; i < count; i++) stock.Add(itemId);
            return this;
        }

        /// <summary>The crafted profile, for <see cref="SessionFactory.Boot"/>.</summary>
        public Profile Build() => Profile;

        private ProfileBuilder Mint(string name, Archetype? archetype, Rarity rarity, int level, int bond,
            TraitId[] traits)
        {
            Assert.IsFalse(string.IsNullOrEmpty(name), "SessionFactory: a crafted maid needs a name");
            Assert.IsTrue(level >= 1 && level <= Maid.MaxLevel,
                "SessionFactory: " + name + " cannot be level " + level + "; the levels run from 1 to " + Maid.MaxLevel);
            Assert.IsTrue(bond >= 0 && bond <= BondTable.MaxLevel,
                "SessionFactory: " + name + " cannot have bond " + bond + "; bonds run from 0 to " + BondTable.MaxLevel);

            if (!_crafted)
            {
                Profile.Maids.Clear();
                Profile.Lineup.Clear();
                Profile.NextMaidId = 1;
                _crafted = true;
            }

            Assert.IsTrue(Find(name) == null, "SessionFactory: the profile already has a maid called " + name);

            var maid = Lootbox.Mint(Profile, rarity, _rng);
            maid.Name = name;

            if (archetype.HasValue)
            {
                maid.Archetype = archetype.Value;
                maid.NaturalRole = RoleTable.NaturalFor(archetype.Value);
                maid.Base = RarityTable.RollStats(rarity, archetype.Value, _rng);
            }

            maid.Traits.Clear();
            if (traits != null) maid.Traits.AddRange(traits);

            if (level > 1) MaidProgression.GrantXp(maid, MaidMath.XpForLevel(level) - maid.Xp, out _);
            if (bond > 0) MaidProgression.GiveGift(maid, BondTable.PointsForLevel(bond) - maid.BondPoints, out _);

            Assert.IsTrue(maid.Level == level && maid.Bond == bond,
                "SessionFactory: " + name + " reached level " + maid.Level + " and bond " + maid.Bond + ", not level " +
                level + " and bond " + bond);

            Profile.Maids.Add(maid);
            MaidProgression.RefreshSetBonus(maid);
            return this;
        }

        private Maid Find(string name)
        {
            foreach (var maid in Profile.Maids)
            {
                if (string.Equals(maid.Name, name, StringComparison.Ordinal)) return maid;
            }

            return null;
        }

        private static string Slots(RoleSlots slots)
        {
            var parts = new List<string>();
            foreach (Role role in Enum.GetValues(typeof(Role))) parts.Add(role + " " + slots.For(role));
            return string.Join(", ", parts);
        }

        private static PlaceableDef PlaceableById(string id)
        {
            foreach (var def in PlaceableCatalog.All)
            {
                if (def.Id == id) return def;
            }

            return null;
        }

        private static Equipment EquipmentById(string id)
        {
            foreach (var item in EquipmentCatalog.All)
            {
                if (item.Id == id) return item;
            }

            return null;
        }
    }
}

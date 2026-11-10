using System.Collections;
using System.IO;
using System;
using System.Linq;
using System.Text;
using Neko.Game.Core;
using Neko.Game.Ui;
using Neko.Game.View;
using Neko.Sim.Core;
using Neko.Sim.Economy;
using Neko.Sim.Maids;
using Neko.Sim.Save;
using Neko.Sim.Team;
using Neko.Tests.PlayMode.Layout;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace Neko.Tests.PlayMode
{
    /// <summary>
    /// A new business and the boxes it opens contain only catalog maids, and every screen shows them
    /// with their own portrait and their own walk cycle: nothing procedural is left on screen.
    /// </summary>
    /// <remarks>Captures are written when NEKO_CAPTURE_DIR is set.</remarks>
    public sealed class NewGameIllustratedTests
    {
        private static readonly Vector2Int[] Windows = { new Vector2Int(960, 600), new Vector2Int(1280, 720) };

        [TearDown]
        public void Restore()
        {
            ResolutionRig.Restore();
            SessionFactory.RestoreClock();
            SessionFactory.RestoreSave();
        }

        private static IEnumerator Shots(string name)
        {
            var directory = System.Environment.GetEnvironmentVariable("NEKO_CAPTURE_DIR");
            foreach (var window in Windows)
            {
                yield return ResolutionRig.Apply(window.x, window.y);
                yield return null;
                yield return null;
                if (string.IsNullOrEmpty(directory)) continue;

                Directory.CreateDirectory(directory);
                File.WriteAllBytes(Path.Combine(directory, name + "_" + window.x + "x" + window.y + ".png"),
                    ResolutionRig.Capture());
            }
        }

        private static void AssertEveryPortraitIsPremium(Transform screen, int expected)
        {
            var portraits = screen.GetComponentsInChildren<Image>(true)
                .Where(i => i.name == "Portrait" && i.transform.parent.name == "Window").ToList();
            Assert.GreaterOrEqual(portraits.Count, expected);
            foreach (var portrait in portraits)
            {
                Assert.IsNotNull(portrait.sprite);
                Assert.IsTrue(portrait.sprite.name.EndsWith("_head"),
                    "a card shows " + portrait.sprite.name + ", not a premium portrait");
            }
        }

        [UnityTest]
        public IEnumerator ANewGameAndItsBoxesHaveOnlyIllustratedMaidsOnEveryScreen()
        {
            // The real new game (Profile.NewGame, the three opening maids), a few levels in, with
            // maids that came out of real boxes.
            var builder = SessionFactory.NewProfile(0x2B1Du, 6).WithDay(1).WithMoney(3000);
            var profile = builder.Profile;
            profile.Wallet.AddPoints(2000);
            var rng = new Rng(21u);
            for (var i = 0; i < 6; i++)
            {
                Lootbox.Open(profile, "box_silver", rng);
            }

            profile.Lineup.AutoFill(profile.Maids);

            Assert.GreaterOrEqual(profile.Maids.Count, 5);
            foreach (var maid in profile.Maids)
            {
                Assert.IsFalse(maid.IsLegacy, maid.Name + " has no identity");
                Assert.IsNotNull(MaidAnimCatalog.Identify(maid), maid.Name + " has no walk cycle");
                Assert.IsNotNull(MaidPortraits.Head(maid), maid.Name + " has no portrait");
                Assert.IsNotNull(MaidPortraits.Detail(maid), maid.Name + " has no detail portrait");
            }

            yield return SessionFactory.Boot(profile);
            yield return Shots("1_menu");
            yield return SessionFactory.ToHub();
            yield return SessionFactory.Settle();
            AssertEveryPortraitIsPremium(GameRoot.Instance.Ui.Get<HubScreen>().transform, 1);
            yield return Shots("2_hub");

            yield return SessionFactory.ToLineup();
            yield return SessionFactory.Settle();
            AssertEveryPortraitIsPremium(GameRoot.Instance.Ui.Get<LineupScreen>().transform, 3);
            yield return Shots("3_escalacao");

            ResolutionRig.Restore();
            yield return SessionFactory.BackToHub();
            yield return SessionFactory.ToRoster();
            yield return SessionFactory.Settle();
            AssertEveryPortraitIsPremium(GameRoot.Instance.Ui.Get<RosterScreen>().transform, profile.Maids.Count);
            Assert.IsNotNull(GameObject.Find("Detail Portrait " + profile.Maids[0].Name), "the detail shows her portrait");
            yield return Shots("4_elenco");

            ResolutionRig.Restore();
            yield return SessionFactory.BackToHub();
            yield return SessionFactory.ToHall();
            yield return SessionFactory.Settle();
            yield return Shots("5_salao_edicao");

            // The room during the day: every maid on the floor walks with her own sheet.
            ResolutionRig.Restore();
            yield return SessionFactory.BackToHub();
            yield return SessionFactory.ToDay();
            for (var i = 0; i < 80; i++) yield return null;

            var views = UnityEngine.Object.FindObjectsByType<MaidView>(FindObjectsInactive.Exclude);
            Assert.GreaterOrEqual(views.Length, 3);
            foreach (var view in views)
            {
                var body = view.GetComponentsInChildren<SpriteRenderer>()
                    .FirstOrDefault(r => r.sprite != null && r.sprite.name.StartsWith("Maids/"));
                Assert.IsNotNull(body, view.Worker.Name + " is drawn procedurally");
            }

            yield return Shots("6_salao_dia");
        }
        // The save format before identities: one field less on each maid line, checksum recomputed.
        private static string AsPreIdentitySave(string payload)
        {
            var parts = payload.Split(':');
            var body = Encoding.UTF8.GetString(Convert.FromBase64String(parts[1]));
            var old = string.Join("\n", body.Split('\n').Select(line =>
                line.StartsWith("maid=", StringComparison.Ordinal) ? line.Substring(0, line.LastIndexOf('|')) : line));
            var hash = 2166136261u;
            foreach (var c in old)
            {
                hash ^= c;
                hash *= 16777619u;
            }

            return parts[0] + ":" + Convert.ToBase64String(Encoding.UTF8.GetBytes(old)) + ":" +
                   hash.ToString("x8", System.Globalization.CultureInfo.InvariantCulture);
        }

        [UnityTest]
        public IEnumerator AnOldSaveWithLegacyMaidsLoadsWithEveryoneAndOnlyTheCatalogOnesAreIllustrated()
        {
            var source = SessionFactory.NewProfile(0x1234u, 4).Profile;
            source.Maids.Clear();
            source.Lineup.Clear();
            source.Maids.Add(new Maid { Id = 1, Name = "Suzu", Archetype = Archetype.Genki, Level = 6, Xp = 120, Rarity = Rarity.Rare, NaturalRole = Role.Floor });
            source.Maids.Add(new Maid { Id = 2, Name = "Mina", Archetype = Archetype.Idol, Level = 3, Bond = 2, NaturalRole = Role.Floor });
            source.Maids.Add(new Maid { Id = 3, Name = "Aoi", Archetype = Archetype.Gourmand, Level = 2, NaturalRole = Role.Kitchen });
            source.NextMaidId = 4;

            var loaded = SaveCodec.Decode(AsPreIdentitySave(SaveCodec.Encode(source)));
            Assert.AreEqual(LoadStatus.Ok, loaded.Status, loaded.Message);
            var profile = loaded.Profile;
            Assert.AreEqual(3, profile.Maids.Count, "nobody is lost");
            Assert.AreEqual("suzu", profile.Maids[0].IdentityId);
            Assert.AreEqual(6, profile.Maids[0].Level);
            Assert.IsTrue(profile.Maids[1].IsLegacy);
            Assert.IsTrue(profile.Maids[2].IsLegacy);
            Assert.AreEqual("Mina", profile.Maids[1].Name);
            Assert.AreEqual(2, profile.Maids[1].Bond);

            yield return SessionFactory.Boot(profile);
            yield return SessionFactory.ToHub();
            yield return SessionFactory.ToRoster();
            yield return SessionFactory.Settle();

            var cards = GameRoot.Instance.Ui.Get<RosterScreen>().GetComponentsInChildren<Image>(true)
                .Where(i => i.name == "Portrait" && i.transform.parent.name == "Window").ToList();
            Assert.AreEqual(3, cards.Count, "the legacy maids are still on screen");
            Assert.AreEqual(1, cards.Count(c => c.sprite != null && c.sprite.name.EndsWith("_head")),
                "only the recognised catalog maid wears an illustrated portrait");
        }
    }
}

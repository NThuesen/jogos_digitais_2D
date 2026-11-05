using System.Collections;
using System.IO;
using System.Linq;
using Neko.Game.Core;
using Neko.Game.Ui;
using Neko.Game.View;
using Neko.Sim.Core;
using Neko.Sim.Economy;
using Neko.Sim.Maids;
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
            for (var i = 0; i < 6 && profile.Maids.Count < profile.Unlocks.RosterCap; i++)
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

            var views = Object.FindObjectsByType<MaidView>(FindObjectsInactive.Exclude);
            Assert.GreaterOrEqual(views.Length, 3);
            foreach (var view in views)
            {
                var body = view.GetComponentsInChildren<SpriteRenderer>()
                    .FirstOrDefault(r => r.sprite != null && r.sprite.name.StartsWith("Maids/"));
                Assert.IsNotNull(body, view.Worker.Name + " is drawn procedurally");
            }

            yield return Shots("6_salao_dia");
        }
    }
}

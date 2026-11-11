using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Neko.Game.Core;
using Neko.Game.Ui;
using Neko.Sim.Economy;
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
    /// UI-INT-01: the illustrated Hub and Caixinhas, at both reference windows, with real data.
    /// Frames are written only when NEKO_UI_QA_DIR is set (give it a path under Logs/).
    /// </summary>
    public sealed class UiIntegrationTests
    {
        private static readonly Vector2Int[] Windows = { new Vector2Int(1280, 720), new Vector2Int(960, 600) };

        private static readonly string[] Names = { "Reina", "Hibari", "Yuki", "Momo", "Akari", "Nene" };

        private static string Dir => System.Environment.GetEnvironmentVariable("NEKO_UI_QA_DIR");

        [TearDown]
        public void Restore() => ResolutionRig.Restore();

        private static void Save(string name, Vector2Int window)
        {
            if (string.IsNullOrEmpty(Dir)) return;
            Directory.CreateDirectory(Dir);
            File.WriteAllBytes(Path.Combine(Dir, name + "_" + window.x + "x" + window.y + ".png"),
                ResolutionRig.Capture());
        }

        private static Profile Squad(int level, int maids, int money, int fichas)
        {
            var builder = SessionFactory.NewProfile(7u, level).WithMoney(money).WithFichas(fichas);
            var rarities = new[] { Rarity.Epic, Rarity.Rare, Rarity.Common, Rarity.Rare, Rarity.Epic, Rarity.Common };
            for (var i = 0; i < maids; i++) builder.WithMaid(Names[i], rarities[i], 3, 2);
            return builder.AutoFillLineup().Build();
        }

        private static Button Named(Transform root, string name) =>
            root.GetComponentsInChildren<Button>(true).First(b => b.name == name);

        /// <summary>
        /// Every visible label of <paramref name="screen"/> is inside the Viewport and inside the drawn
        /// bounds of its plate, and no Image drawn later covers it: measured from the rendered meshes
        /// (<see cref="KitLegibility"/>), at the window the rig has applied.
        /// </summary>
        private static void AssertLegible(Transform screen, string where)
        {
            var found = new List<string>();
            var measured = KitLegibility.Check(LayoutModel.Measure(GameRoot.Instance.Ui), screen, where, found);
            Assert.Greater(measured, 5, where + ": almost no label was measured, so nothing was checked");
            Assert.IsEmpty(found, string.Join("\n", found));
        }

        /// <summary>A real click at the centre of a control, through the EventSystem; the handler that got it.</summary>
        private static GameObject ClickCentre(string name, out Clicks clicks)
        {
            clicks = new Clicks(LayoutModel.Measure(GameRoot.Instance.Ui));
            var control = clicks.Measurement.Controls.First(c => c.Component.name == name);
            var points = clicks.SamplePoints(control);
            Assert.IsTrue(points.Count > 0, name + " draws nothing, so it has no centre");
            return clicks.Click(points[0]);
        }

        [UnityTest]
        public IEnumerator TheHubShowsRealNumbersAndAnyNumberOfMaids()
        {
            foreach (var (level, maids) in new[] { (1, 1), (2, 2), (4, 3), (8, 5) })
            {
                ResolutionRig.Restore();
                var profile = Squad(level, maids, 435, 43);
                yield return SessionFactory.Boot(profile);
                yield return SessionFactory.ToHub();

                var hub = GameRoot.Instance.Ui.Get<HubScreen>();
                var filled = profile.Lineup.All.Count(s => !s.Empty);

                foreach (var window in Windows)
                {
                    yield return ResolutionRig.Apply(window.x, window.y);
                    yield return SessionFactory.Settle();

                    var cards = hub.transform.GetComponentsInChildren<Image>().Count(i => i.name == "Card");
                    Debug.Log("UI-INT-01 hub level " + level + " maids " + maids + " filled " + filled + " at " + window);
                    AssertLegible(hub.transform, "hub L" + level + " " + window);
                    Save("hub_L" + level + "_m" + filled, window);
                }

                Assert.IsTrue(hub.transform.GetComponentsInChildren<Text>().Any(t => t.text == "435 ¥"),
                    "the cash is the wallet's");
                Assert.IsTrue(hub.transform.GetComponentsInChildren<Text>().Any(t => t.text == "43"),
                    "the fichas are the wallet's");
            }
        }

        [UnityTest]
        public IEnumerator TheHubAlertCarriesALongReasonWithoutOverflowing()
        {
            var profile = Squad(1, 2, 10, 0);
            yield return SessionFactory.Boot(profile);
            yield return SessionFactory.ToHub();
            var hub = GameRoot.Instance.Ui.Get<HubScreen>();

            foreach (var window in Windows)
            {
                yield return ResolutionRig.Apply(window.x, window.y);
                yield return SessionFactory.Settle();
                AssertLegible(hub.transform, "hub alert " + window);
                Save("hub_alert", window);
            }

            Assert.IsNotNull(hub.transform.Find("WarningPlate"));
        }

        [UnityTest]
        public IEnumerator TheFiveNavigationButtonsBackAndTheCtaStillFire()
        {
            yield return SessionFactory.Boot(Squad(2, 3, 5000, 50));
            yield return SessionFactory.ToHub();
            var hub = GameRoot.Instance.Ui.Get<HubScreen>();

            var fired = new List<string>();
            hub.OnLineup = () => fired.Add("Nav_Lineup");
            hub.OnRoster = () => fired.Add("Nav_Roster");
            hub.OnBoxes = () => fired.Add("Nav_Boxes");
            hub.OnShop = () => fired.Add("Nav_Shop");
            hub.OnHall = () => fired.Add("Nav_Hall");
            hub.OnMenu = () => fired.Add("Back");
            hub.OnOpenDay = () => fired.Add("Cta_OpenDay");

            foreach (var window in Windows)
            {
                yield return ResolutionRig.Apply(window.x, window.y);
                yield return SessionFactory.Settle();

                foreach (var name in new[] { "Nav_Lineup", "Nav_Roster", "Nav_Boxes", "Nav_Shop", "Nav_Hall", "Back", "Cta_OpenDay" })
                {
                    fired.Clear();
                    var handler = ClickCentre(name, out _);
                    Assert.IsTrue(handler != null && handler.name == name, window + ": the click on " + name + " reached " +
                                                                           (handler != null ? handler.name : "nothing"));
                    CollectionAssert.AreEqual(new[] { name }, fired, window + ": the click on " + name);
                }
            }
        }

        [UnityTest]
        public IEnumerator TheHubKeepsItsLabelsReadableWithHugeFigures()
        {
            // The widest figures the wallet can print: cash at the int limit, the most fichas, a full lineup.
            yield return SessionFactory.Boot(Squad(8, 5, int.MaxValue / 2, 99999));
            yield return SessionFactory.ToHub();
            var hub = GameRoot.Instance.Ui.Get<HubScreen>();

            foreach (var window in Windows)
            {
                yield return ResolutionRig.Apply(window.x, window.y);
                yield return SessionFactory.Settle();
                AssertLegible(hub.transform, "hub huge figures " + window);
                Save("hub_huge", window);
            }

            Assert.IsTrue(hub.transform.GetComponentsInChildren<Text>().Any(t => t.text == "99999"),
                "the fichas are the wallet's");
        }

        [UnityTest]
        public IEnumerator ADisabledCtaLooksDisabledAndDoesNotFire()
        {
            // One maid only, so the day cannot open and the alert says why.
            yield return SessionFactory.Boot(Squad(1, 1, 435, 43));
            yield return SessionFactory.ToHub();
            var hub = GameRoot.Instance.Ui.Get<HubScreen>();
            var opened = 0;
            hub.OnOpenDay = () => opened++;

            Assert.IsNotNull(hub.Session.WhyNotReady(), "the fixture was meant to be unable to open");
            var cta = Named(hub.transform, "Cta_OpenDay");
            Assert.IsFalse(cta.interactable, "the CTA is disabled");

            foreach (var window in Windows)
            {
                yield return ResolutionRig.Apply(window.x, window.y);
                yield return SessionFactory.Settle();
                AssertLegible(hub.transform, "hub disabled CTA " + window);
                Save("hub_cta_disabled", window);

                var handler = ClickCentre("Cta_OpenDay", out _);
                Assert.AreEqual(0, opened, window + ": a disabled CTA fired, through " + (handler != null ? handler.name : "nothing"));
                Assert.IsTrue(hub.transform.Find("WarningPlate").gameObject.activeSelf, "the alert says why");
            }

            Assert.Less(cta.targetGraphic.canvasRenderer.GetColor().grayscale, 0.9f, "a disabled CTA is tinted down");
        }

        [UnityTest]
        public IEnumerator ClickingABoxTheFichasCannotPayForBuysNothing()
        {
            yield return SessionFactory.Boot(Squad(2, 2, 500, 43));
            var profile = GameRoot.Instance.Session.Profile;
            yield return SessionFactory.ToHub();
            yield return SessionFactory.ToBoxes();
            var screen = GameRoot.Instance.Ui.Get<BoxScreen>();

            foreach (var window in Windows)
            {
                yield return ResolutionRig.Apply(window.x, window.y);
                yield return SessionFactory.Settle();

                ClickCentre("Open_box_gold", out _);
                ClickCentre("Open_box_silver", out _);
                Assert.AreEqual(43, profile.Wallet.Points, window + ": a box the player cannot pay for took fichas");
                Assert.IsFalse(screen.transform.Find("Reveal").gameObject.activeSelf, "no reveal for a refused box");
            }
        }

        [UnityTest]
        public IEnumerator TheBoxesBackButtonReachesItsCallback()
        {
            yield return SessionFactory.Boot(Squad(2, 2, 500, 43));
            yield return SessionFactory.ToHub();
            yield return SessionFactory.ToBoxes();
            var screen = GameRoot.Instance.Ui.Get<BoxScreen>();
            var backs = 0;
            screen.OnBack = () => backs++;

            foreach (var window in Windows)
            {
                yield return ResolutionRig.Apply(window.x, window.y);
                yield return SessionFactory.Settle();
                var handler = ClickCentre("Back", out _);
                Assert.IsTrue(handler != null && handler.name == "Back", window + ": the click did not reach Voltar");
            }

            Assert.AreEqual(Windows.Length, backs);
        }

        [UnityTest]
        public IEnumerator TheBoxesShowTheRealPricesAndOddsAndEnableOnlyWhatIsAffordable()
        {
            yield return SessionFactory.Boot(Squad(2, 2, 500, 43));
            var profile = GameRoot.Instance.Session.Profile;
            yield return SessionFactory.ToHub();
            yield return SessionFactory.ToBoxes();
            var screen = GameRoot.Instance.Ui.Get<BoxScreen>();

            foreach (var window in Windows)
            {
                yield return ResolutionRig.Apply(window.x, window.y);
                yield return SessionFactory.Settle();
                AssertLegible(screen.transform, "boxes " + window);
                Save("boxes_43", window);
            }

            var texts = screen.transform.GetComponentsInChildren<Text>().Select(t => t.text).ToList();
            CollectionAssert.Contains(texts, "43 Fichas");

            foreach (var box in Lootbox.All)
            {
                CollectionAssert.Contains(texts, box.DisplayName);
                CollectionAssert.Contains(texts, "Abrir " + box.PricePoints);
                var button = Named(screen.transform, "Open_" + box.Id);
                Assert.AreEqual(profile.Wallet.Points >= box.PricePoints, button.interactable, box.Id);
            }

            Assert.IsTrue(texts.Any(t => t == "78% Comum") && texts.Any(t => t == "3,6% Épica") &&
                          texts.Any(t => t == "0,4% Lendária"), "the simple box prints its real chances");
            Assert.IsTrue(texts.Any(t => t == "78% Épica") && texts.Any(t => t == "22% Lendária"),
                "the gold box prints its real chances");
        }

        [UnityTest]
        public IEnumerator ARealPurchaseSpendsTheFichasAndRevealsTheCards()
        {
            yield return SessionFactory.Boot(Squad(2, 2, 500, 700));
            var profile = GameRoot.Instance.Session.Profile;
            yield return SessionFactory.ToHub();
            yield return SessionFactory.ToBoxes();
            var screen = GameRoot.Instance.Ui.Get<BoxScreen>();

            foreach (var window in Windows)
            {
                yield return ResolutionRig.Apply(window.x, window.y);
                yield return SessionFactory.Settle();
                Save("boxes_700", window);
            }

            var box = Lootbox.ById("box_silver");
            Assert.IsTrue(Named(screen.transform, "Open_box_silver").interactable);
            var before = profile.Wallet.Points;
            var maidsBefore = profile.Maids.Count;
            var bought = ClickCentre("Open_box_silver", out _);
            Assert.IsTrue(bought != null && bought.name == "Open_box_silver", "the click reached the Prata button");

            // Duplicates give fichas back, so the balance is between "price spent" and "nothing refunded beyond it".
            Assert.Less(profile.Wallet.Points, before, "the price was spent");
            Assert.GreaterOrEqual(profile.Wallet.Points, before - box.PricePoints, "never more than the price was spent");
            Assert.IsTrue(screen.transform.Find("Reveal").gameObject.activeSelf, "the reveal is up");

            foreach (var window in Windows)
            {
                yield return ResolutionRig.Apply(window.x, window.y);
                for (var i = 0; i < 40; i++) yield return null;
                Save("boxes_reveal", window);
            }

            Assert.GreaterOrEqual(profile.Maids.Count, maidsBefore);
        }
    }
}

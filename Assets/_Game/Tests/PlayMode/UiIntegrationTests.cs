using System.Collections;
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

        private static IEnumerable Texts(Transform root) => root.GetComponentsInChildren<Text>(false);

        /// <summary>Every visible text must lie inside the window and not be clipped by its own rect.</summary>
        private static void AssertTextsFit(Transform root, string where)
        {
            foreach (Text text in Texts(root))
            {
                if (string.IsNullOrEmpty(text.text) || !text.isActiveAndEnabled) continue;
                Assert.LessOrEqual(text.cachedTextGenerator.lineCount, text.resizeTextForBestFit ? 2 : 4,
                    where + ": '" + text.text + "' wraps into too many lines for its plate");
            }
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
                    AssertTextsFit(hub.transform, "hub L" + level + " " + window);
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
                AssertTextsFit(hub.transform, "hub alert " + window);
                Save("hub_alert", window);
            }

            Assert.IsNotNull(hub.transform.Find("WarningPlate"));
        }

        [UnityTest]
        public IEnumerator TheFiveNavigationButtonsBackAndTheCtaStillFire()
        {
            var profile = Squad(2, 3, 5000, 50);
            yield return SessionFactory.Boot(profile);
            yield return SessionFactory.ToHub();
            var hub = GameRoot.Instance.Ui.Get<HubScreen>();

            var fired = new System.Collections.Generic.List<string>();
            hub.OnLineup = () => fired.Add("lineup");
            hub.OnRoster = () => fired.Add("roster");
            hub.OnBoxes = () => fired.Add("boxes");
            hub.OnShop = () => fired.Add("shop");
            hub.OnHall = () => fired.Add("hall");
            hub.OnMenu = () => fired.Add("menu");
            hub.OnOpenDay = () => fired.Add("open");

            foreach (var name in new[] { "Nav_Lineup", "Nav_Roster", "Nav_Boxes", "Nav_Shop", "Nav_Hall", "Cta_OpenDay" })
            {
                var button = Named(hub.transform, name);
                Assert.IsTrue(button.interactable, name);
                button.onClick.Invoke();
            }

            hub.transform.GetComponentsInChildren<Button>().First(b => b.name == "KitButton").onClick.Invoke();
            CollectionAssert.AreEqual(new[] { "lineup", "roster", "boxes", "shop", "hall", "open", "menu" }, fired);
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
                AssertTextsFit(screen.transform, "boxes " + window);
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
            Named(screen.transform, "Open_box_silver").onClick.Invoke();

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

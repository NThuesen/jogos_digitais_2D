using System.Collections;
using System.Collections.Generic;
using Neko.Game.Core;
using Neko.Game.Ui;
using Neko.Sim.Team;
using Neko.Tests.PlayMode.Layout;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace Neko.Tests.PlayMode
{
    /// <summary>
    /// The approved modular sprites on the Menu and the Shop: the art is a skin, the behaviour is not.
    /// </summary>
    /// <remarks>
    /// Layout_Check already measures both screens at 960×600 and 1280×720; this checks what a
    /// layout rule cannot: that the sprites actually loaded with their approved import settings,
    /// the Shop still buys, tabs still switch and mark the selected one, the DISABLED state is
    /// Unity-driven as well as drawn, and the Menu keeps its controls. Pointer input is simulated.
    /// </remarks>
    public sealed class UiArtIntegrationTests
    {
        private static readonly string[] Approved =
        {
            "ui012_menu_background", "ui020_logo_wordmark", "ui017_shop_row",
            "btn2_normal", "btn2_hover", "btn2_pressed", "btn2_disabled",
            "btn1_normal", "btn1_hover", "btn1_pressed", "btn1_disabled",
            "tab_inactive", "tab_hover", "tab_active",
            "icon_coin", "icon_gift", "icon_cat_paw",
        };

        [TearDown]
        public void Restore()
        {
            SessionFactory.RestoreClock();
            SessionFactory.RestoreSave();
        }

        [Test]
        public void ApprovedSpritesLoadWithTheirImportSettings()
        {
            foreach (var name in Approved)
            {
                var sprite = UiArt.Get(name);
                Assert.IsNotNull(sprite, name + " is missing from Resources/UiArt");
                Assert.AreEqual(100f, sprite.pixelsPerUnit, name + " PPU");
                var format = sprite.texture.format;
                Assert.IsTrue(format == TextureFormat.RGBA32 || format == TextureFormat.RGB24,
                    name + " is compressed: " + format);
                Assert.AreEqual(FilterMode.Bilinear, sprite.texture.filterMode, name);
                Assert.AreEqual(TextureWrapMode.Clamp, sprite.texture.wrapMode, name);
            }

            Assert.AreEqual(new Vector4(85f, 45f, 85f, 45f), UiArt.Get("ui017_shop_row").border);
            Assert.Greater(UiArt.Get("btn1_normal").border.x, 50f);
            Assert.Greater(UiArt.Get("btn2_normal").border.x, 150f);
        }

        [UnityTest]
        public IEnumerator ShopKeepsBuyingTabsAndTheDisabledState()
        {
            var builder = SessionFactory.NewProfile(7u, 10).WithDay(1).WithMoney(100);
            builder.WithMaid("Hana", Rarity.Rare, 5, 1, Neko.Sim.Team.TraitId.Lucky);
            yield return SessionFactory.Boot(builder.Build());
            yield return SessionFactory.ToHub();
            yield return SessionFactory.ToShop();
            yield return SessionFactory.Settle();

            var root = GameRoot.Instance;
            var shop = root.Ui.Get<ShopScreen>();
            var backPressed = false;
            shop.OnBack = () => backPressed = true;

            // Tabs: exactly one selected mark, and it follows the selection.
            var tabs = shop.GetComponentsInChildren<Button>(true).ToList("ArtTab");
            Assert.AreEqual(3, tabs.Count, "three art tabs");
            Assert.AreEqual(1, ActiveMarks(tabs));
            Assert.IsTrue(MarkOn(tabs[0]), "Móveis is the starting tab");

            tabs[2].onClick.Invoke();
            yield return SessionFactory.Settle();
            Assert.IsTrue(MarkOn(tabs[2]) && !MarkOn(tabs[0]), "the mark moves to Presentes");
            Assert.AreEqual(UiArt.Get("tab_active"), ((Image)tabs[2].targetGraphic).sprite);
            Assert.AreEqual(UiArt.Get("tab_inactive"), ((Image)tabs[0].targetGraphic).sprite);

            tabs[0].onClick.Invoke();
            yield return SessionFactory.Settle();

            // Rows: affordable buy buttons are live, unaffordable ones are disabled in Unity too.
            var buys = shop.GetComponentsInChildren<Button>(false).ToList("ArtButton");
            var live = buys.Find(b => b.interactable && b.GetComponentInChildren<Text>().text != "Voltar");
            var dead = buys.Find(b => !b.interactable);
            Assert.IsNotNull(live, "an item the player can afford");
            Assert.IsNotNull(dead, "an item the player cannot afford");
            Assert.Less(dead.GetComponentInChildren<Text>().color.a, 1f, "disabled text is dimmed in Unity");

            // SpriteSwap follows the pointer.
            var pointer = new PointerEventData(EventSystem.current);
            ExecuteEvents.Execute(live.gameObject, pointer, ExecuteEvents.pointerEnterHandler);
            yield return null;
            Assert.AreEqual(UiArt.Get("btn1_hover"), ((Image)live.targetGraphic).overrideSprite);
            ExecuteEvents.Execute(live.gameObject, pointer, ExecuteEvents.pointerDownHandler);
            yield return null;
            Assert.AreEqual(UiArt.Get("btn1_pressed"), ((Image)live.targetGraphic).overrideSprite);
            ExecuteEvents.Execute(live.gameObject, pointer, ExecuteEvents.pointerUpHandler);
            ExecuteEvents.Execute(live.gameObject, pointer, ExecuteEvents.pointerExitHandler);

            // Buying still spends the money and changes the stock; a disabled button does nothing.
            var before = root.Session.Profile.Wallet.Money;
            var owned = root.Session.Profile.OwnedPlaceables.Count;
            ExecuteEvents.Execute(dead.gameObject, pointer, ExecuteEvents.pointerClickHandler);
            Assert.AreEqual(before, root.Session.Profile.Wallet.Money, "a disabled buy must not spend");

            live.onClick.Invoke();
            yield return SessionFactory.Settle();
            Assert.Less(root.Session.Profile.Wallet.Money, before, "buying spends money");
            Assert.AreEqual(owned + 1, root.Session.Profile.OwnedPlaceables.Count, "and gives the item");

            // The back control is the art button and still calls OnBack.
            var back = shop.GetComponentsInChildren<Button>(false).ToList("ArtButton")
                .Find(b => b.GetComponentInChildren<Text>().text == Neko.Game.Ui.Strings.Get("dialog.back"));
            Assert.IsNotNull(back);
            back.onClick.Invoke();
            Assert.IsTrue(backPressed);
        }

        [UnityTest]
        public IEnumerator MenuShowsTheApprovedArtAndKeepsItsControls()
        {
            var builder = SessionFactory.NewProfile(9u, 1).WithDay(1).WithMoney(600);
            yield return SessionFactory.Boot(builder.Build());
            // The wordmark sways on purpose, so the menu never "settles"; a few frames are enough.
            for (var i = 0; i < 5; i++) yield return null;

            var menu = GameRoot.Instance.Ui.Get<MenuScreen>();
            var backdrop = menu.transform.Find("Scenery/Cafe");
            Assert.IsNotNull(backdrop, "the approved café sits behind the menu");
            Assert.AreEqual(UiArt.Get("ui012_menu_background"), backdrop.GetComponent<Image>().sprite);
            Assert.IsFalse(backdrop.GetComponent<Image>().raycastTarget);

            var wordmark = menu.transform.Find("ui020_logo_wordmark");
            Assert.IsNotNull(wordmark, "the approved wordmark replaces the procedural sign");
            Assert.IsFalse(wordmark.GetComponent<Image>().raycastTarget);

            var played = false;
            menu.OnPlay = () => played = true;
            var planks = new List<Button>(menu.GetComponentsInChildren<Button>(false));
            Assert.AreEqual(2, planks.Count, "Play and New game are still the only controls");
            planks[0].onClick.Invoke();
            Assert.IsTrue(played);
        }

        private static int ActiveMarks(List<Button> tabs)
        {
            var count = 0;
            foreach (var tab in tabs) if (MarkOn(tab)) count++;
            return count;
        }

        private static bool MarkOn(Button tab) =>
            tab.transform.Find("SelectedMark").gameObject.activeSelf;
    }

    internal static class ButtonListExtensions
    {
        public static List<Button> ToList(this Button[] buttons, string objectName)
        {
            var list = new List<Button>();
            foreach (var b in buttons) if (b.name == objectName) list.Add(b);
            return list;
        }
    }
}

using System;
using Neko.Game.Art;
using UnityEngine;
using UnityEngine.UI;

namespace Neko.Game.Ui
{
    /// <summary>
    /// The room the menus are set in.
    /// </summary>
    /// <remarks>
    /// A flat colour behind an interface is the single clearest tell that nobody designed it.
    /// This is six rectangles — a papered wall, a picture rail, a wainscot, a skirting, a
    /// floor and a pool of lamplight — and it costs one draw call each, but every screen
    /// after it reads as a place rather than as a dialog box.
    /// </remarks>
    public static class Scenery
    {
        public static readonly Color Wall = Palette.Hex("#6E4F3C");
        public static readonly Color WallLit = Palette.Hex("#8A6A52");
        public static readonly Color Wainscot = Palette.Hex("#5A3F30");
        public static readonly Color Rail = Palette.Hex("#D9A24A");
        public static readonly Color Floor = Palette.Hex("#9A7352");
        public static readonly Color FloorDeep = Palette.Hex("#7E5B41");

        /// <summary>Fraction of the screen the floor takes at the bottom.</summary>
        public const float FloorHeight = 0.26f;

        public static RectTransform Build(Transform parent, bool withFloor = true)
        {
            var root = UiKit.Root("Scenery", parent);
            root.SetAsFirstSibling();

            var wall = UiKit.Panel(root, Vector2.one * 100f, Wall, 0f);
            UiKit.Stretch(wall);

            // A soft pool of light behind where the sign hangs. The only thing in the whole
            // backdrop that is not a straight edge, and it is what stops it reading as a flag.
            var glow = new GameObject("Lamplight", typeof(RectTransform)).AddComponent<Image>();
            glow.transform.SetParent(root, false);
            // Big enough that its edge is off the screen on every aspect the game supports.
            // At 1500 wide the circle ended inside the frame and the backdrop read as a
            // brown rectangle with a lighter oval painted on it.
            glow.sprite = UiKit.Sprites.Get(
                new ShapeSpec(ShapeId.Circle, Palette.WithAlpha(WallLit, 0.5f)), 256);
            glow.raycastTarget = false;
            glow.rectTransform.sizeDelta = new Vector2(2600f, 1900f);
            UiKit.Anchor(glow.rectTransform, new Vector2(0.5f, 1f), new Vector2(0f, 180f));

            Paper(root);

            if (!withFloor) return root;

            var floor = UiKit.Panel(root, Vector2.one * 100f, Floor, 0f);
            floor.anchorMin = Vector2.zero;
            floor.anchorMax = new Vector2(1f, FloorHeight);
            floor.offsetMin = Vector2.zero;
            floor.offsetMax = Vector2.zero;

            Boards(floor);

            var skirting = UiKit.Panel(root, new Vector2(0f, 26f), Wainscot, 0.12f);
            skirting.anchorMin = new Vector2(0f, FloorHeight);
            skirting.anchorMax = new Vector2(1f, FloorHeight);
            skirting.pivot = new Vector2(0.5f, 0f);
            skirting.sizeDelta = new Vector2(0f, 26f);
            skirting.offsetMin = new Vector2(0f, skirting.offsetMin.y);
            skirting.offsetMax = new Vector2(0f, skirting.offsetMax.y);

            var rail = UiKit.Panel(root, new Vector2(0f, 5f), Rail, 0.5f);
            rail.anchorMin = new Vector2(0f, FloorHeight);
            rail.anchorMax = new Vector2(1f, FloorHeight);
            rail.pivot = new Vector2(0.5f, 0f);
            rail.sizeDelta = new Vector2(0f, 5f);
            rail.anchoredPosition = new Vector2(0f, 26f);

            return root;
        }

        /// <summary>Vertical stripes on the wall. Wallpaper, at four draw calls.</summary>
        private static void Paper(RectTransform root)
        {
            const int stripes = 9;

            for (var i = 0; i < stripes; i++)
            {
                var stripe = UiKit.Panel(root, new Vector2(34f, 0f),
                    Palette.WithAlpha(WallLit, 0.22f), 0f);
                stripe.anchorMin = new Vector2(i / (float)stripes, 0f);
                stripe.anchorMax = new Vector2(i / (float)stripes, 1f);
                stripe.pivot = new Vector2(0f, 0.5f);
                stripe.sizeDelta = new Vector2(34f, 0f);
                stripe.offsetMin = new Vector2(stripe.offsetMin.x, 0f);
                stripe.offsetMax = new Vector2(stripe.offsetMax.x, 0f);
            }
        }

        /// <summary>Floorboards, so the bottom strip reads as a floor and not as a bar.</summary>
        private static void Boards(RectTransform floor)
        {
            const int boards = 7;

            for (var i = 1; i < boards; i++)
            {
                var seam = UiKit.Panel(floor, new Vector2(3f, 0f),
                    Palette.WithAlpha(FloorDeep, 0.9f), 0f);
                seam.anchorMin = new Vector2(i / (float)boards, 0f);
                seam.anchorMax = new Vector2(i / (float)boards, 1f);
                seam.pivot = new Vector2(0.5f, 0.5f);
                seam.sizeDelta = new Vector2(3f, 0f);
                seam.offsetMin = new Vector2(seam.offsetMin.x, 0f);
                seam.offsetMax = new Vector2(seam.offsetMax.x, 0f);
            }
        }

        /// <summary>
        /// A card in the house style: a plate with a lighter face and a brass edge.
        /// </summary>
        /// <remarks>
        /// Used for every panel in the game that is not a button, so the whole interface looks
        /// like it came out of the same workshop as the sign.
        /// </remarks>
        public static RectTransform Plaque(Transform parent, Vector2 size, Color? face = null)
        {
            var root = UiKit.Panel(parent, size, Logo.BoardDeep, 0.2f);
            root.gameObject.name = "Plaque";

            var edge = UiKit.Panel(root, new Vector2(size.x - 6f, size.y - 6f), Rail, 0.19f);
            UiKit.Anchor(edge, new Vector2(0.5f, 0.5f), Vector2.zero);

            var inner = UiKit.Panel(root, new Vector2(size.x - 12f, size.y - 12f),
                face ?? Palette.Panel, 0.18f);
            UiKit.Anchor(inner, new Vector2(0.5f, 0.5f), Vector2.zero);

            return inner;
        }

        /// <summary>
        /// A wooden button, anchored where you asked for it.
        /// </summary>
        /// <remarks>
        /// Exists because setting anchoredPosition without setting the anchor leaves a widget
        /// hanging off the centre of its parent, and every back button in the game was built
        /// that way until the captures showed them all floating in the middle of the screen.
        /// </remarks>
        public static Button PlankAt(Transform parent, string text, Vector2 size, Vector2 anchor,
            Vector2 offset, Action onClick, int fontSize = Typeface.Body, Color? tint = null)
        {
            var button = Plank(parent, text, size, onClick, fontSize, tint);
            UiKit.Anchor(button.GetComponent<RectTransform>(), anchor, offset);
            return button;
        }

        /// <summary>A wooden button, in the same workshop as everything else.</summary>
        public static Button Plank(Transform parent, string text, Vector2 size, Action onClick,
            int fontSize = Typeface.Body, Color? tint = null)
        {
            var root = UiKit.Panel(parent, size, Logo.BoardDeep, 0.3f);
            root.gameObject.name = "Plank";
            root.GetComponent<Image>().raycastTarget = true;

            var face = UiKit.Panel(root, new Vector2(size.x - 8f, size.y - 8f),
                tint ?? Logo.Board, 0.28f);
            UiKit.Anchor(face, new Vector2(0.5f, 0.5f), Vector2.zero);
            face.GetComponent<Image>().raycastTarget = false;

            var shine = UiKit.Panel(root, new Vector2(size.x - 20f, size.y * 0.3f),
                Palette.WithAlpha(Logo.BoardLight, 0.5f), 0.5f);
            UiKit.Anchor(shine, new Vector2(0.5f, 1f), new Vector2(0f, -8f));
            shine.GetComponent<Image>().raycastTarget = false;

            var label = UiKit.Label(root, text, fontSize, Palette.Cream, bold: true);
            UiKit.Stretch(label.rectTransform, 8f);

            var drop = label.gameObject.AddComponent<Shadow>();
            drop.effectColor = new Color(0f, 0f, 0f, 0.45f);
            drop.effectDistance = new Vector2(1.5f, -2f);

            var button = root.gameObject.AddComponent<Button>();
            button.targetGraphic = root.GetComponent<Image>();

            var colours = button.colors;
            colours.highlightedColor = new Color(1.12f, 1.12f, 1.12f, 1f);
            colours.pressedColor = new Color(0.82f, 0.82f, 0.82f, 1f);
            colours.disabledColor = new Color(1f, 1f, 1f, 0.35f);
            colours.fadeDuration = 0.07f;
            button.colors = colours;

            button.onClick.AddListener(() =>
            {
                Feel.Tween.Punch(root, 0.1f, 0.18f);
                onClick?.Invoke();
            });

            return button;
        }
    }
}

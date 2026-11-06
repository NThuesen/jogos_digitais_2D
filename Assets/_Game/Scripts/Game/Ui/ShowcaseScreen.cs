using System;
using System.Collections.Generic;
using Neko.Game.Art;
using Neko.Game.View;
using Neko.Sim.Maids;
using UnityEngine;
using UnityEngine.UI;

namespace Neko.Game.Ui
{
    /// <summary>
    /// A showcase of the authored maids, for presentations: their walk cycles from the
    /// existing sheets, each with her name and archetype.
    /// </summary>
    /// <remarks>
    /// A demonstration of existing assets only. It is not part of the game flow: nothing links
    /// to it, it reads <see cref="MaidAnimCatalog"/> and changes no profile, generation rule or
    /// save. Opened with F8 on the menu (see GameRoot), left with the back button.
    /// </remarks>
    public sealed class ShowcaseScreen : Screen
    {
        private const float Fps = 12f;
        private const float Spacing = 124f;
        private const int Columns = 9;
        private const float RowPitch = 196f;
        private const float WalkerScale = 0.5f;

        private readonly List<(Image Image, Sprite[] Frames)> _walkers = new List<(Image, Sprite[])>();

        public Action OnBack;

        protected override void Build()
        {
            Cafe();

            var title = UiKit.TrackedLabel(transform, "MAIDS AUTORAIS", Typeface.Heading, Palette.Cream, 5f);
            UiKit.Anchor(title, new Vector2(0f, 1f), new Vector2(26f, -18f));

            var shade = UiKit.Panel(transform, new Vector2(560f, 34f),
                Palette.WithAlpha(Palette.Ink, 0.55f), 0.3f, true);
            UiKit.Anchor(shade, new Vector2(0f, 1f), new Vector2(26f, -66f));
            shade.GetComponent<Image>().raycastTarget = false;

            var sub = UiKit.Label(transform, "Arte ilustrada própria · ciclos de caminhada do jogo",
                Typeface.Body, Palette.WithAlpha(Palette.Cream, 0.92f), TextAnchor.MiddleLeft);
            UiKit.Anchor(sub.rectTransform, new Vector2(0f, 1f), new Vector2(30f, -68f));
            sub.rectTransform.sizeDelta = new Vector2(700f, 30f);
            sub.gameObject.AddComponent<Shadow>().effectColor = new Color(0f, 0f, 0f, 0.7f);

            var back = UiArt.StateButton(transform, Strings.Get("dialog.back"), new Vector2(120f, 44f),
                () => OnBack?.Invoke(), "btn2", Typeface.Small);
            UiKit.Anchor(back.GetComponent<RectTransform>(), new Vector2(1f, 1f), new Vector2(-26f, -18f));

            var entries = MaidAnimCatalog.All;
            // A grid of nine a row: 27 authored maids are three full rows of nine.
            var rows = (entries.Length + Columns - 1) / Columns;
            for (var i = 0; i < entries.Length; i++)
            {
                var row = i / Columns;
                var inRow = Mathf.Min(Columns, entries.Length - row * Columns);
                var x = (i % Columns - (inRow - 1) * 0.5f) * Spacing;
                var y = 170f - row * RowPitch + (3 - rows) * RowPitch * 0.5f;
                Walker(entries[i], x, y);
            }
        }

        private void Cafe()
        {
            var art = UiArt.Get("ui012_menu_background");
            if (art == null)
            {
                Backdrop(Palette.Wall);
                return;
            }

            var root = UiKit.Root("Cafe", transform);
            root.SetAsFirstSibling();

            var picture = new GameObject("Picture", typeof(RectTransform)).AddComponent<Image>();
            picture.transform.SetParent(root, false);
            picture.sprite = art;
            picture.raycastTarget = false;
            var fit = picture.gameObject.AddComponent<AspectRatioFitter>();
            fit.aspectMode = AspectRatioFitter.AspectMode.EnvelopeParent;
            fit.aspectRatio = art.rect.width / art.rect.height;

            var scrim = UiKit.Panel(root, Vector2.one * 100f, Palette.WithAlpha(Palette.Ink, 0.45f), 0f);
            UiKit.Stretch(scrim);
            scrim.GetComponent<Image>().raycastTarget = false;
        }

        private void Walker(MaidAnimCatalog.Entry entry, float x, float y)
        {
            var frames = MaidAnimCatalog.Load(UiKit.Sprites, entry);
            var scale = WalkerScale;

            var plate = UiKit.Panel(transform, new Vector2(Spacing - 10f, 56f),
                Palette.WithAlpha(Palette.Panel, 0.94f), 0.35f, true);
            Centre(plate, x, y - entry.CellHeight * scale * 0.5f - 34f);
            plate.GetComponent<Image>().raycastTarget = false;

            var name = UiKit.Label(plate, entry.name, Typeface.Body, Palette.Text, bold: true);
            Band(name.rectTransform, 0.5f, 1f);
            var kind = UiKit.Label(plate, ArchetypeTable.For(entry.ArchetypeValue).DisplayName, Typeface.Tiny,
                Palette.ForArchetype(entry.ArchetypeValue), bold: true);
            Band(kind.rectTransform, 0f, 0.5f);

            if (frames == null || frames.Length == 0) return;

            var picture = new GameObject("Walker " + entry.name, typeof(RectTransform)).AddComponent<Image>();
            picture.transform.SetParent(transform, false);
            picture.preserveAspect = true;
            picture.raycastTarget = false;
            picture.sprite = frames[0];
            picture.rectTransform.sizeDelta = new Vector2(entry.CellWidth * scale, entry.CellHeight * scale);
            Centre(picture.rectTransform, x, y);
            _walkers.Add((picture, frames));
        }

        private static void Centre(RectTransform rect, float x, float y)
        {
            rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = new Vector2(x, y);
        }

        private static void Band(RectTransform rect, float from, float to)
        {
            rect.anchorMin = new Vector2(0f, from);
            rect.anchorMax = new Vector2(1f, to);
            rect.offsetMin = rect.offsetMax = Vector2.zero;
        }

        private void Update()
        {
            var tick = (int)(Time.unscaledTime * Fps);
            for (var i = 0; i < _walkers.Count; i++)
            {
                var (image, frames) = _walkers[i];
                image.sprite = frames[(tick + i * 3) % frames.Length];
            }
        }
    }
}

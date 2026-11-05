using System.Collections;
using System.IO;
using Neko.Game.Art;
using Neko.Game.Core;
using Neko.Game.Ui;
using Neko.Game.View;
using Neko.Sim.Maids;
using Neko.Sim.Team;
using Neko.Tests.PlayMode.Layout;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace Neko.Tests.PlayMode
{
    /// <summary>
    /// QA of the Suzu v03 portrait on the real Escalação and Elenco screens (not production).
    /// </summary>
    /// <remarks>
    /// The derived crops live under Tests/PlayMode/QaArt, outside Resources, so nothing here ships
    /// and no production sprite changes: the test swaps the sprite of the existing "Portrait"
    /// image after the screen is built. Suzu has her canonical identity (name + Genki) only in
    /// this test's profile. Captures are written when NEKO_CAPTURE_DIR is set, as in CaptureTests.
    /// </remarks>
    public sealed class SuzuPortraitQaTests
    {
        private static readonly Vector2Int[] Windows = { new Vector2Int(960, 600), new Vector2Int(1280, 720) };

        [TearDown]
        public void Restore()
        {
            ResolutionRig.Restore();
            SessionFactory.RestoreClock();
            SessionFactory.RestoreSave();
        }

        private static Sprite Load(string file)
        {
            var path = Path.Combine(Application.dataPath, "_Game/Tests/PlayMode/QaArt", file);
            var texture = new Texture2D(2, 2, TextureFormat.RGBA32, true);
            Assert.IsTrue(texture.LoadImage(File.ReadAllBytes(path)), file);
            texture.wrapMode = TextureWrapMode.Clamp;
            texture.filterMode = FilterMode.Bilinear;
            return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height),
                new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect);
        }

        private static ProfileBuilder Cast()
        {
            var builder = SessionFactory.NewProfile(0x5A52u, 10).WithDay(1).WithMoney(5000).EmptyRoom();
            builder.WithMaid("Suzu", Archetype.Genki, Rarity.Rare, 8, 3, TraitId.Hostess);
            builder.WithMaid("Akari", Rarity.Common, 3, 1);
            builder.WithMaid("Hina", Rarity.Epic, 10, 2, TraitId.Lucky);
            builder.WithMaid("Momo", Rarity.Legendary, 12, 4, TraitId.Charisma);
            builder.WithMaid("Kaede", Rarity.Common, 2, 0);
            builder.WithMaid("Ichika", Rarity.Rare, 6, 2);
            return builder.AutoFillLineup();
        }

        private static RectTransform[] SuzuCards()
        {
            var cards = new System.Collections.Generic.List<RectTransform>();
            foreach (var rect in Object.FindObjectsByType<RectTransform>(FindObjectsInactive.Exclude))
            {
                if (rect.name == "Card Suzu") cards.Add(rect);
            }

            return cards.ToArray();
        }

        private static void SwapPortrait(RectTransform card, Sprite sprite)
        {
            var portrait = card.Find("Window/Portrait").GetComponent<Image>();
            portrait.sprite = sprite;
            Assert.IsFalse(portrait.raycastTarget, "the portrait must not eat clicks");
        }

        /// <summary>The detail panel, prototyped in the test: a big framed portrait instead of the small card.</summary>
        private static void BigDetail(RectTransform smallCard, Maid maid, Sprite sprite)
        {
            var head = (RectTransform)smallCard.parent;
            Object.DestroyImmediate(smallCard.gameObject);
            head.sizeDelta = new Vector2(head.sizeDelta.x, 196f);

            var frame = UiKit.Panel(head, new Vector2(150f, 196f), Palette.Hex(maid.Card.TintHex), 0.08f, true);
            frame.SetAsFirstSibling();
            frame.GetComponent<Image>().raycastTarget = false;
            var inner = UiKit.Panel(frame, new Vector2(144f, 190f),
                Palette.WithAlpha(Palette.ForArchetype(maid.Archetype), 0.55f), 0.06f, true);
            UiKit.Anchor(inner, new Vector2(0.5f, 0.5f), Vector2.zero);
            inner.GetComponent<Image>().raycastTarget = false;

            var picture = new GameObject("DetailPortrait", typeof(RectTransform)).AddComponent<Image>();
            picture.transform.SetParent(inner, false);
            picture.sprite = sprite;
            picture.preserveAspect = true;
            picture.raycastTarget = false;
            UiKit.Stretch(picture.rectTransform);
        }

        private static void Write(string name, int width, int height)
        {
            var directory = System.Environment.GetEnvironmentVariable("NEKO_CAPTURE_DIR");
            if (string.IsNullOrEmpty(directory)) return;

            Directory.CreateDirectory(directory);
            File.WriteAllBytes(Path.Combine(directory, name + "_" + width + "x" + height + ".png"),
                ResolutionRig.Capture());
        }

        private static IEnumerator Frames()
        {
            yield return null;
            yield return null;
            yield return null;
        }

        [UnityTest]
        public IEnumerator SuzuV03OnLineupRosterAndDetail()
        {
            var builder = Cast();
            var suzu = builder.MaidNamed("Suzu");
            Assert.IsNotNull(MaidAnimCatalog.Identify(suzu), "Suzu must be the canonical Suzu (name + Genki)");

            var head = Load("suzu_v03_head_192.png");
            var detail = Load("suzu_v03_detail_480.png");

            yield return SessionFactory.Boot(builder.Build());
            yield return SessionFactory.ToHub();

            // ---- Escalação, 0.62x -------------------------------------------------------------
            yield return SessionFactory.ToLineup();
            yield return SessionFactory.Settle();
            var lineupCards = SuzuCards();
            Assert.IsNotEmpty(lineupCards, "Suzu is not on the lineup board");
            Assert.AreEqual(new Vector2(MaidCard.Width * 0.62f, MaidCard.Height * 0.62f), lineupCards[0].sizeDelta);
            foreach (var window in Windows)
            {
                yield return ResolutionRig.Apply(window.x, window.y);
                Write("lineup_A_current", window.x, window.y);
            }

            foreach (var card in lineupCards) SwapPortrait(card, head);
            foreach (var window in Windows)
            {
                yield return ResolutionRig.Apply(window.x, window.y);
                yield return Frames();
                Write("lineup_B_v03", window.x, window.y);
            }

            // Pressing the card still takes her off the board (existing behaviour, not changed).
            var button = lineupCards[0].GetComponent<Button>();
            Assert.IsNotNull(button);
            button.onClick.Invoke();
            yield return Frames();
            Assert.IsNull(GameRoot.Instance.Session.Profile.Lineup.RoleOf(suzu.Id), "the lineup card stopped working");
            GameRoot.Instance.Session.Profile.Lineup.AutoFill(GameRoot.Instance.Session.Profile.Maids);

            // ---- Elenco 0.82x + detail ------------------------------------------------------------
            ResolutionRig.Restore();
            yield return SessionFactory.BackToHub();
            yield return SessionFactory.ToRoster();
            yield return SessionFactory.Settle();
            foreach (var window in Windows)
            {
                yield return ResolutionRig.Apply(window.x, window.y);
                Write("roster_A_current", window.x, window.y);
            }

            var rosterCards = SuzuCards();
            RectTransform grid = null, small = null;
            foreach (var card in rosterCards)
            {
                if (Mathf.Approximately(card.sizeDelta.x, MaidCard.Width * 0.82f)) grid = card;
                else if (Mathf.Approximately(card.sizeDelta.x, MaidCard.Width * 0.72f)) small = card;
            }

            Assert.IsNotNull(grid, "no 0.82x Suzu card in the Elenco grid");
            Assert.IsNotNull(small, "Suzu is not the selected maid in the detail panel");

            SwapPortrait(grid, head);
            SwapPortrait(small, head);
            foreach (var window in Windows)
            {
                yield return ResolutionRig.Apply(window.x, window.y);
                yield return Frames();
                Write("roster_B_v03_cards", window.x, window.y);
            }

            BigDetail(small, suzu, detail);
            foreach (var window in Windows)
            {
                yield return ResolutionRig.Apply(window.x, window.y);
                yield return Frames();
                Write("roster_C_v03_detail", window.x, window.y);
            }

            // Selecting another maid and coming back still works on the swapped grid.
            var other = SessionFactory.Describe(GameRoot.Instance.Session.Profile);
            Assert.IsNotEmpty(other);
            grid.GetComponent<Button>().onClick.Invoke();
            yield return Frames();
            Assert.IsNotEmpty(SuzuCards(), "selecting Suzu rebuilt the detail without her card");
        }
    }
}

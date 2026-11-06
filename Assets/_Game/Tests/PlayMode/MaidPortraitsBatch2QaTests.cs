using System.Collections;
using System.Collections.Generic;
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
    /// QA of the second batch of nine UI-021 portraits on the real Escalação and Elenco screens (not production).
    /// </summary>
    /// <remarks>
    /// The derived crops live under docs/art/ui-modular/portraits/batch2/derived, outside Assets, so nothing here ships
    /// and no production sprite changes: the test swaps the sprite of the existing "Portrait"
    /// image after the screen is built. Each maid is an ordinary procedural maid with her designed name and archetype (no identity). Captures are written when
    /// NEKO_CAPTURE_DIR is set, as in CaptureTests.
    /// </remarks>
    public sealed class MaidPortraitsBatch2QaTests
    {
        private static readonly Vector2Int[] Windows = { new Vector2Int(960, 600), new Vector2Int(1280, 720) };

        // Two profiles: a café's roster holds eight, so the nine go in two groups.
        private static readonly string[][] Groups =
        {
            new[] { "Mikan", "Yuki", "Kurumi", "Nonoka", "Kaguya" },
            new[] { "Mizuki", "Himari", "Anzu", "Kasumi" },
            new[] { "Nonoka" }, // group 2: the approved v02 (Scottish Fold ears), incremental QA
        };

        // The nine are not in the walk catalog or the MaidCatalog yet (no animations): the test gives them
        // their designed archetype on an ordinary procedural maid and swaps only the portrait.
        private static readonly Dictionary<string, Archetype> Archetypes = new Dictionary<string, Archetype>
        {
            ["Mikan"] = Archetype.Genki, ["Yuki"] = Archetype.Kuudere, ["Kurumi"] = Archetype.Tsundere,
            ["Nonoka"] = Archetype.Dojikko, ["Kaguya"] = Archetype.Ojousama, ["Mizuki"] = Archetype.Oneesan,
            ["Himari"] = Archetype.Idol, ["Anzu"] = Archetype.Gourmand, ["Kasumi"] = Archetype.Mukuchi,
        };

        private static readonly Rarity[] Rarities =
            { Rarity.Rare, Rarity.Epic, Rarity.Common, Rarity.Legendary, Rarity.Epic };

        private static readonly int[] GroupIndexes = { 0, 1, 2 };

        [TearDown]
        public void Restore()
        {
            ResolutionRig.Restore();
            SessionFactory.RestoreClock();
            SessionFactory.RestoreSave();
        }

        private static Sprite Load(string file)
        {
            // Derived crops stay under docs/ (never under Assets): nothing here can ship.
            var path = Path.GetFullPath(Path.Combine(Application.dataPath, "../docs/art/ui-modular/portraits/batch2/derived", file));
            var texture = new Texture2D(2, 2, TextureFormat.RGBA32, true);
            Assert.IsTrue(texture.LoadImage(File.ReadAllBytes(path)), file);
            texture.wrapMode = TextureWrapMode.Clamp;
            texture.filterMode = FilterMode.Bilinear;
            return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height),
                new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect);
        }

        private static ProfileBuilder Cast(string[] names)
        {
            var builder = SessionFactory.NewProfile(0x5A52u, 10).WithDay(1).WithMoney(5000).EmptyRoom();
            for (var i = 0; i < names.Length; i++)
            {
                builder.WithMaid(names[i], Archetypes[names[i]], Rarities[i % Rarities.Length], 4 + i * 2, i % 5);
            }

            return builder.AutoFillLineup();
        }

        private static RectTransform CardOf(string name, float width)
        {
            foreach (var rect in Object.FindObjectsByType<RectTransform>(FindObjectsInactive.Exclude))
            {
                if (rect.name == "Card " + name && Mathf.Approximately(rect.sizeDelta.x, width)) return rect;
            }

            return null;
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

            var frame = UiKit.Panel(head, new Vector2(138f, 196f), Palette.Hex(maid.Card.TintHex), 0.08f, true);
            frame.SetAsFirstSibling();
            frame.GetComponent<Image>().raycastTarget = false;
            var inner = UiKit.Panel(frame, new Vector2(132f, 190f),
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

        private static IEnumerator Shots(string name)
        {
            foreach (var window in Windows)
            {
                yield return ResolutionRig.Apply(window.x, window.y);
                yield return Frames();
                Write(name, window.x, window.y);
            }
        }

        [UnityTest]
        public IEnumerator PortraitsOnLineupRosterAndDetail([ValueSource(nameof(GroupIndexes))] int group)
        {
            var names = Groups[group];
            var builder = Cast(names);
            var heads = new Dictionary<string, Sprite>();
            var details = new Dictionary<string, Sprite>();
            foreach (var name in names)
            {
                var version = group == 2 ? "_v02" : "";
                heads[name] = Load(name.ToLowerInvariant() + version + "_head_192.png");
                details[name] = Load(name.ToLowerInvariant() + version + "_detail_480.png");
                // An identity is attached only to a released maid (by exact name + archetype); a planned one stays legacy.
                Assert.AreEqual(MaidCatalog.FindReleased(name, Archetypes[name]) != null,
                    MaidAnimCatalog.Identify(builder.MaidNamed(name)) != null, name);
            }

            var tag = "g" + group;
            yield return SessionFactory.Boot(builder.Build());
            yield return SessionFactory.ToHub();

            // ---- Escalação, 0.62x -------------------------------------------------------------
            yield return SessionFactory.ToLineup();
            yield return SessionFactory.Settle();
            yield return Shots(tag + "_lineup_A_current");

            foreach (var name in names)
            {
                var card = CardOf(name, MaidCard.Width * 0.62f);
                Assert.IsNotNull(card, name + " is not on the lineup board");
                SwapPortrait(card, heads[name]);
            }

            yield return Shots(tag + "_lineup_B_v03");

            // Pressing a card still takes her off the board (existing behaviour, not changed).
            CardOf(names[0], MaidCard.Width * 0.62f).GetComponent<Button>().onClick.Invoke();
            yield return Frames();
            Assert.IsNull(GameRoot.Instance.Session.Profile.Lineup.RoleOf(builder.MaidNamed(names[0]).Id),
                "the lineup card stopped working");
            GameRoot.Instance.Session.Profile.Lineup.AutoFill(GameRoot.Instance.Session.Profile.Maids);

            // ---- Elenco 0.82x + detail ------------------------------------------------------------
            ResolutionRig.Restore();
            yield return SessionFactory.BackToHub();
            yield return SessionFactory.ToRoster();
            yield return SessionFactory.Settle();
            yield return Shots(tag + "_roster_A_current");

            foreach (var name in names)
            {
                var card = CardOf(name, MaidCard.Width * 0.82f);
                Assert.IsNotNull(card, name + " is not in the Elenco grid");
                SwapPortrait(card, heads[name]);
            }

            yield return Shots(tag + "_roster_B_v03");

            foreach (var name in names)
            {
                CardOf(name, MaidCard.Width * 0.82f).GetComponent<Button>().onClick.Invoke();
                yield return Frames();

                // Since the integration the real detail panel already shows her large portrait
                // (PremiumPortraitsTests); the prototype only replaces the compact card of the old one.
                var small = CardOf(name, MaidCard.Width * 0.72f);
                if (small != null) BigDetail(small, builder.MaidNamed(name), details[name]);
                else Assert.IsNotNull(GameObject.Find("Detail Portrait " + name), name + " has no detail portrait");
                yield return Shots("detail_" + name.ToLowerInvariant() + (group == 2 ? "_v02" : ""));
            }
        }
    }
}

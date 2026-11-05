using System.Collections;
using System.IO;
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
    /// The premium portraits on the real screens, with nothing swapped by the test: the nine
    /// authored maids wear theirs on Maid Cards (Escalação, Elenco, Hub) and large in the Elenco
    /// detail, and a maid without a canonical identity keeps the old card and portrait.
    /// </summary>
    /// <remarks>Captures are written when NEKO_CAPTURE_DIR is set.</remarks>
    public sealed class PremiumPortraitsTests
    {
        private static readonly Vector2Int[] Windows = { new Vector2Int(960, 600), new Vector2Int(1280, 720) };

        // The roster of a café holds eight, so the nine go in two groups. Group 1 also holds two
        // maids that must NOT get a portrait: an unknown name, and "Suzu" with another archetype.
        private static readonly string[][] Groups =
        {
            new[] { "Suzu", "Hina", "Akari", "Momo", "Reina" },
            new[] { "Kaede", "Nene", "Ichika", "Yoru", "Aoi" },
        };

        private static readonly Rarity[] Rarities =
            { Rarity.Rare, Rarity.Epic, Rarity.Common, Rarity.Legendary, Rarity.Epic };

        private static readonly int[] GroupIndexes = { 0, 1 };

        [TearDown]
        public void Restore()
        {
            ResolutionRig.Restore();
            SessionFactory.RestoreClock();
            SessionFactory.RestoreSave();
        }

        private static ProfileBuilder Cast(string[] names, bool withImpostor)
        {
            var builder = SessionFactory.NewProfile(0x5A52u, 10).WithDay(1).WithMoney(5000).EmptyRoom();
            for (var i = 0; i < names.Length; i++)
            {
                MaidAnimCatalog.Entry entry = null;
                foreach (var e in MaidAnimCatalog.All)
                {
                    if (e.name == names[i]) entry = e;
                }

                var archetype = entry != null ? entry.ArchetypeValue : Archetype.Genki;
                builder.WithMaid(names[i], archetype, Rarities[i % Rarities.Length], 4 + i * 2, i % 5);
            }

            // "Suzu" by name only: the archetype is not hers, so the portrait must not apply.
            if (withImpostor) builder.WithMaid("Suzu", Archetype.Gourmand, Rarity.Common, 2, 0);
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

        private static Image PortraitOf(RectTransform card) => card.Find("Window/Portrait").GetComponent<Image>();

        private static IEnumerator Frames()
        {
            yield return null;
            yield return null;
            yield return null;
        }

        private static IEnumerator Shots(string name)
        {
            var directory = System.Environment.GetEnvironmentVariable("NEKO_CAPTURE_DIR");
            foreach (var window in Windows)
            {
                yield return ResolutionRig.Apply(window.x, window.y);
                yield return Frames();
                if (string.IsNullOrEmpty(directory)) continue;

                Directory.CreateDirectory(directory);
                File.WriteAllBytes(Path.Combine(directory, name + "_" + window.x + "x" + window.y + ".png"),
                    ResolutionRig.Capture());
            }
        }

        [UnityTest]
        public IEnumerator RealScreensUseThePremiumPortraitsAndFallBack([ValueSource(nameof(GroupIndexes))] int group)
        {
            var names = Groups[group];
            var builder = Cast(names, group == 1);
            yield return SessionFactory.Boot(builder.Build());
            var tag = "g" + group;

            // ---- Hub ------------------------------------------------------------------------------
            yield return SessionFactory.ToHub();
            yield return SessionFactory.Settle();
            yield return Shots(tag + "_hub");

            // ---- Escalação ----------------------------------------------------------------------
            yield return SessionFactory.ToLineup();
            yield return SessionFactory.Settle();
            foreach (var name in names)
            {
                var maid = builder.MaidNamed(name);
                var card = CardOf(name, MaidCard.Width * 0.62f);
                Assert.IsNotNull(card, name + " is not on the lineup board");
                AssertPortrait(maid, PortraitOf(card), name + " lineup card");
            }

            yield return Shots(tag + "_lineup");

            // ---- Elenco -------------------------------------------------------------------------
            ResolutionRig.Restore();
            yield return SessionFactory.BackToHub();
            yield return SessionFactory.ToRoster();
            yield return SessionFactory.Settle();
            yield return Shots(tag + "_roster");

            foreach (var name in names)
            {
                var maid = builder.MaidNamed(name);
                var card = CardOf(name, MaidCard.Width * 0.82f);
                Assert.IsNotNull(card, name + " is not in the Elenco grid");
                AssertPortrait(maid, PortraitOf(card), name + " grid card");

                card.GetComponent<Button>().onClick.Invoke();
                yield return Frames();

                var frame = GameObject.Find("Detail Portrait " + name);
                if (MaidAnimCatalog.Identify(maid) != null)
                {
                    Assert.IsNotNull(frame, name + " has no large detail portrait");
                    Image picture = null;
                    foreach (var image in frame.GetComponentsInChildren<Image>())
                    {
                        if (image.name == "Portrait") picture = image;
                    }

                    Assert.IsNotNull(picture, name + " detail frame has no picture");
                    Assert.AreSame(MaidPortraits.Detail(maid), picture.sprite, name + " detail sprite");
                    Assert.IsFalse(picture.raycastTarget);
                    AssertDetailFits(frame.transform);
                    yield return Shots("detail_" + name.ToLowerInvariant());
                }
                else
                {
                    Assert.IsNull(frame, name + " must keep the compact card in the detail panel");
                    Assert.IsNotNull(CardOf(name, MaidCard.Width * 0.72f), name + " lost her detail card");
                    yield return Shots("detail_" + name.ToLowerInvariant() + "_fallback");
                }
            }

            // The Suzu with another archetype is the last maid of the profile: no borrowed face.
            if (group == 1)
            {
                var impostor = builder.Profile.Maids[builder.Profile.Maids.Count - 1];
                Assert.AreEqual("Suzu", impostor.Name);
                Assert.IsNull(MaidPortraits.Head(impostor), "a Suzu with another archetype borrowed the portrait");
                Assert.IsNull(MaidPortraits.Detail(impostor));
            }
        }

        private static void AssertPortrait(Maid maid, Image portrait, string what)
        {
            if (MaidAnimCatalog.Identify(maid) != null)
            {
                Assert.IsNotNull(MaidPortraits.Head(maid), what + ": no premium portrait in Resources");
                Assert.AreSame(MaidPortraits.Head(maid), portrait.sprite, what);
            }
            else
            {
                Assert.IsNull(MaidPortraits.Head(maid), what + " must not get a premium portrait");
                Assert.IsNotNull(portrait.sprite, what + " lost its fallback portrait");
            }
        }

        /// <summary>Every text and button of the detail plaque stays inside it.</summary>
        private static void AssertDetailFits(Transform frame)
        {
            var plaque = (RectTransform)frame.parent.parent.parent;
            var corners = new Vector3[4];
            plaque.GetWorldCorners(corners);
            var bounds = new Rect(corners[0], corners[2] - corners[0]);

            foreach (var graphic in plaque.GetComponentsInChildren<Graphic>())
            {
                if (!(graphic is Text) && graphic.GetComponentInParent<Button>() == null) continue;
                graphic.rectTransform.GetWorldCorners(corners);
                var rect = new Rect(corners[0], corners[2] - corners[0]);
                Assert.IsTrue(bounds.Contains(rect.min) && bounds.Contains(rect.max),
                    graphic.name + " sticks out of the detail plaque: " + rect + " vs " + bounds);
            }
        }
    }
}

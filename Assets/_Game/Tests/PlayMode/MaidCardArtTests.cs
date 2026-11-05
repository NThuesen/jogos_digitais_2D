using System;
using Neko.Game.Ui;
using Neko.Sim.Maids;
using Neko.Sim.Team;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;

namespace Neko.Tests.PlayMode
{
    /// <summary>
    /// The approved card art on <see cref="MaidCard"/>: frame per rarity, minimal frame on the
    /// lineup board, and a card that is still one clickable object with its texts in Unity.
    /// </summary>
    public sealed class MaidCardArtTests
    {
        private GameObject _canvas;
        private Neko.Game.Art.SpriteFactory _ownSprites;

        // UiKit.Sprites is a static that only UiRoot sets, so run alone these tests had none
        // and failed in UiKit.Panel; they only passed after another test had booted the game.
        [SetUp]
        public void GiveUiKitItsSprites()
        {
            if (UiKit.Sprites != null) return;

            _ownSprites = new Neko.Game.Art.SpriteFactory();
            UiKit.Sprites = _ownSprites;
        }

        [TearDown]
        public void Cleanup()
        {
            if (_canvas != null) UnityEngine.Object.DestroyImmediate(_canvas);
            if (_ownSprites != null && UiKit.Sprites == _ownSprites) UiKit.Sprites = null;
        }

        private static Maid MaidOf(Rarity rarity)
        {
            var maid = Maid.Create(1, "Suzu", Archetype.Genki);
            maid.Rarity = rarity;
            return maid;
        }

        private RectTransform Parent()
        {
            _canvas = new GameObject("Canvas", typeof(RectTransform), typeof(Canvas));
            return (RectTransform)_canvas.transform;
        }

        [Test]
        public void EachRarityGetsItsApprovedFrameAndBadgeOnTheCompactCard()
        {
            foreach (Rarity rarity in Enum.GetValues(typeof(Rarity)))
            {
                var key = rarity.ToString().ToLowerInvariant();
                var card = MaidCard.Build(Parent(), MaidOf(rarity), 1f, exact: true);

                Assert.AreSame(UiArt.Get("card_" + key), card.Find("Frame").GetComponent<Image>().sprite, key);
                Assert.AreSame(UiArt.Get("badge_" + key), card.Find("Rarity").GetComponent<Image>().sprite, key);
                UnityEngine.Object.DestroyImmediate(_canvas);
            }
        }

        [Test]
        public void TheLineupSlotScaleUsesTheMinimalFrameWithoutStatsOrSalary()
        {
            var card = MaidCard.Build(Parent(), MaidOf(Rarity.Rare), 0.62f, showSalary: false, exact: true);

            Assert.AreSame(UiArt.Get("card_minimal"), card.Find("Frame").GetComponent<Image>().sprite);
            Assert.IsNull(card.Find("Stats"), "the minimal card shows portrait and name only");
            Assert.AreEqual(new Vector2(MaidCard.Width * 0.62f, MaidCard.Height * 0.62f), card.sizeDelta);
        }

        [Test]
        public void TheCardStaysOneClickableObjectAndKeepsItsTexts()
        {
            var maid = MaidOf(Rarity.Epic);
            var card = MaidCard.Build(Parent(), maid, 1f, showSalary: true, exact: true);

            var hit = card.GetComponent<Image>();
            Assert.IsTrue(hit.raycastTarget, "the root is the Button target on every screen");
            foreach (var graphic in card.GetComponentsInChildren<Graphic>())
            {
                if (graphic != hit) Assert.IsFalse(graphic.raycastTarget, graphic.name + " must not eat clicks");
            }

            var texts = string.Join("|", Array.ConvertAll(card.GetComponentsInChildren<Text>(), t => t.text));
            StringAssert.Contains("Suzu", texts);
            StringAssert.Contains(maid.Profile.DisplayName, texts);
            StringAssert.Contains(Strings.Money(maid.Salary), texts);
            StringAssert.Contains(maid.Total.Serve.ToString(), texts);
        }

        [Test]
        public void AnEmptySlotKeepsItsLabelOnTheMinimalFrame()
        {
            var card = MaidCard.Build(Parent(), null, 0.62f, showSalary: false, exact: true);

            Assert.AreSame(UiArt.Get("card_minimal"), card.Find("Frame").GetComponent<Image>().sprite);
            StringAssert.Contains(Strings.Get("lineup.empty"), card.GetComponentInChildren<Text>().text);
        }
    }
}

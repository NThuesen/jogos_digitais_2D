using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Neko.Sim.Core;
using Neko.Sim.Economy;
using Neko.Sim.Maids;
using Neko.Sim.Save;
using Neko.Sim.Team;
using NUnit.Framework;

namespace Neko.Tests.EditMode.Sim
{
    /// <summary>The closed catalog of 27 maids and everything that now draws from it.</summary>
    public sealed class MaidCatalogTests
    {
        private static readonly string[] ReleasedIds =
            { "suzu", "hina", "akari", "momo", "reina", "kaede", "nene", "ichika", "yoru" };

        private static Profile Rich(uint seed, int cafeLevel = 10)
        {
            var profile = Profile.NewGame(seed);
            var wallet = profile.Wallet;
            wallet.Restore(wallet.Money, CafeLevelTable.ReputationForLevel(cafeLevel), wallet.Points);
            profile.ApplyUnlocks();
            wallet.AddPoints(100000);
            return profile;
        }

        // ---- the catalog ---------------------------------------------------------------------

        [Test]
        public void TheCatalogHasTwentySevenUniqueMaidsThreePerArchetype()
        {
            Assert.AreEqual(27, MaidCatalog.All.Count);
            Assert.AreEqual(27, MaidCatalog.All.Select(i => i.Id).Distinct().Count(), "ids");
            Assert.AreEqual(27, MaidCatalog.All.Select(i => i.Name).Distinct().Count(), "names");
            foreach (var identity in MaidCatalog.All)
            {
                Assert.AreEqual(identity.Id, identity.Id.ToLowerInvariant());
                Assert.IsFalse(identity.Id.Any(c => !char.IsLetterOrDigit(c)), identity.Id + " is not a plain slug");
            }

            foreach (Archetype archetype in Enum.GetValues(typeof(Archetype)))
            {
                Assert.AreEqual(3, MaidCatalog.All.Count(i => i.Archetype == archetype), archetype.ToString());
                Assert.AreEqual(1, MaidCatalog.Released.Count(i => i.Archetype == archetype), archetype + " released");
            }
        }

        [Test]
        public void OnlyTheNineIllustratedMaidsAreReleasedAndTheOtherEighteenArePlanned()
        {
            CollectionAssert.AreEqual(ReleasedIds, MaidCatalog.Released.Select(i => i.Id).ToArray());
            Assert.AreEqual(18, MaidCatalog.All.Count(i => i.Status == IdentityStatus.Planned));
            Assert.IsNull(MaidCatalog.FindReleased("Mikan", Archetype.Genki), "planned is never drawn or recognised");
            Assert.AreSame(MaidCatalog.ById("suzu"), MaidCatalog.FindReleased("Suzu", Archetype.Genki));
            Assert.IsNull(MaidCatalog.FindReleased("Suzu", Archetype.Tsundere), "name alone is not an identity");
        }

        [Test]
        public void ACatalogMaidKeepsTheIdentityNameAndArchetype()
        {
            foreach (var identity in MaidCatalog.All)
            {
                var maid = Maid.Create(1, identity);
                Assert.AreEqual(identity.Id, maid.IdentityId);
                Assert.AreEqual(identity.Name, maid.Name);
                Assert.AreEqual(identity.Archetype, maid.Archetype);
                Assert.IsFalse(maid.IsLegacy);
            }

            Assert.IsTrue(Maid.Create(1, "Mina", Archetype.Genki).IsLegacy);
        }

        // ---- a new game ------------------------------------------------------------------------

        [Test]
        public void ANewGameStartsWithThreeDistinctReleasedMaidsOnTheThreeOpeningPositions()
        {
            for (uint seed = 1; seed <= 200; seed++)
            {
                var profile = Profile.NewGame(seed);
                Assert.AreEqual(3, profile.Maids.Count, "seed " + seed);
                Assert.AreEqual(3, profile.Maids.Select(m => m.IdentityId).Distinct().Count(), "seed " + seed);
                CollectionAssert.AreEquivalent(new[] { Role.Reception, Role.Floor, Role.Kitchen },
                    profile.Maids.Select(m => m.NaturalRole).ToArray(), "seed " + seed);
                foreach (var maid in profile.Maids)
                {
                    Assert.IsFalse(maid.IsLegacy);
                    Assert.IsTrue(MaidCatalog.ById(maid.IdentityId).Released, maid.Name);
                    Assert.AreEqual(Rarity.Common, maid.Rarity);
                    Assert.AreEqual(MaidCatalog.ById(maid.IdentityId).Name, maid.Name);
                }
            }
        }

        [Test]
        public void TheSameSeedStartsTheSameBusinessAndOtherSeedsVary()
        {
            string Describe(Profile p) => string.Join(";", p.Maids.Select(m =>
                m.IdentityId + "/" + m.Base.Serve + "." + m.Base.Cook + "." + m.Base.Charm + "." + m.Base.Stamina +
                "/" + m.House + "/" + m.Origin));

            Assert.AreEqual(Describe(Profile.NewGame(7u)), Describe(Profile.NewGame(7u)));
            Assert.Greater(Enumerable.Range(1, 40).Select(s => Describe(Profile.NewGame((uint)s))).Distinct().Count(), 20);
        }

        // ---- boxes ---------------------------------------------------------------------------------

        [Test]
        public void BoxesNeverPutTheSameIdentityOnTheRosterTwiceAndEveryCardIsACatalogMaid()
        {
            for (uint seed = 1; seed <= 60; seed++)
            {
                var profile = Rich(seed);
                var rng = new Rng(seed * 7919u);
                for (var i = 0; i < 12; i++)
                {
                    var result = Lootbox.Open(profile, "box_silver", rng);
                    Assert.IsTrue(result.Ok);
                    foreach (var card in result.Cards)
                    {
                        Assert.IsFalse(card.Maid.IsLegacy);
                        Assert.AreEqual(MaidCatalog.ById(card.Maid.IdentityId).Name, card.Maid.Name);
                    }
                }

                var ids = profile.Maids.Select(m => m.IdentityId).ToList();
                Assert.AreEqual(ids.Count, ids.Distinct().Count(), "seed " + seed);
                Assert.LessOrEqual(profile.Maids.Count, 9);
            }
        }

        [Test]
        public void WhenSheHasEveryReleasedMaidACardIsAConvertedDuplicateNeverASecondCopy()
        {
            var profile = Rich(3u);
            foreach (var identity in MaidCatalog.FreeReleased(profile.Maids))
            {
                profile.Add(Maid.Create(profile.NextMaidId++, identity));
            }

            Assert.AreEqual(9, profile.Maids.Count);
            var before = profile.Wallet.Points;
            var open = Lootbox.Open(profile, "box_gold", new Rng(11u));

            Assert.AreEqual(9, profile.Maids.Count, "a duplicate is never added");
            Assert.AreEqual(3, open.Cards.Count);
            foreach (var card in open.Cards)
            {
                Assert.IsTrue(card.Duplicate);
                Assert.AreEqual(card.Maid.Severance, card.PointsBack);
            }

            Assert.AreEqual(before - open.Box.PricePoints + open.Cards.Sum(c => c.PointsBack), profile.Wallet.Points);
        }

        [Test]
        public void TheOddsOfTheBoxesAreUnchanged()
        {
            CollectionAssert.AreEqual(new[] { 78f, 18f, 3.6f, 0.4f }, Lootbox.ById("box_simple").Weights);
            CollectionAssert.AreEqual(new[] { 0f, 62f, 32f, 6f }, Lootbox.ById("box_silver").Weights);
            CollectionAssert.AreEqual(new[] { 0f, 0f, 78f, 22f }, Lootbox.ById("box_gold").Weights);
        }

        [Test]
        public void TheSameSeedOpensTheSameBoxes()
        {
            string Run()
            {
                var profile = Rich(5u);
                var rng = new Rng(99u);
                var text = new StringBuilder();
                for (var i = 0; i < 6; i++)
                {
                    foreach (var card in Lootbox.Open(profile, "box_simple", rng).Cards)
                    {
                        text.Append(card.Maid.IdentityId).Append('/').Append(card.Maid.Rarity).Append('/')
                            .Append(card.Maid.Base.Serve).Append(card.Duplicate ? "d;" : ";");
                    }
                }

                return text.ToString();
            }

            Assert.AreEqual(Run(), Run());
        }

        [Test]
        public void ARarityStillDecidesTheCardNotTheIdentity()
        {
            var rng = new Rng(4u);
            foreach (Rarity rarity in Enum.GetValues(typeof(Rarity)))
            {
                var maid = Lootbox.Mint(null, rarity, rng);
                Assert.AreEqual(rarity, maid.Rarity);
                Assert.AreEqual(RarityTable.For(rarity).Traits, maid.Traits.Count);
                Assert.IsTrue(MaidCatalog.ById(maid.IdentityId).Released);
            }
        }

        [Test]
        public void TheRosterRefusesAnIdentityItAlreadyHas()
        {
            var profile = Profile.NewGame(2u);
            var count = profile.Maids.Count;
            profile.Add(Maid.Create(99, MaidCatalog.ById(profile.Maids[0].IdentityId)));
            Assert.AreEqual(count, profile.Maids.Count);
            Assert.IsTrue(profile.HasIdentity(profile.Maids[0].IdentityId));
            Assert.IsFalse(profile.HasIdentity(null));
            Assert.IsFalse(profile.HasIdentity(""));
        }

        // ---- hiring ------------------------------------------------------------------------------

        [Test]
        public void TheBoardOffersDistinctReleasedMaidsThatAreNotOnTheRoster()
        {
            var profile = Profile.NewGame(8u);
            var board = new ApplicantBoard();
            for (uint seed = 1; seed <= 50; seed++)
            {
                board.Refresh(new Rng(seed), 6, profile.NextMaidId, profile.Maids);
                var ids = board.Candidates.Select(c => c.Maid.IdentityId).ToList();
                Assert.AreEqual(3, ids.Count);
                Assert.AreEqual(3, ids.Distinct().Count());
                foreach (var candidate in board.Candidates)
                {
                    Assert.IsTrue(MaidCatalog.ById(candidate.Maid.IdentityId).Released);
                    Assert.IsFalse(profile.HasIdentity(candidate.Maid.IdentityId));
                }
            }
        }

        [Test]
        public void TheBoardGetsShorterWhenFewerMaidsAreFreeAndNeverOffersAnOwnedOne()
        {
            var profile = Rich(9u);
            while (MaidCatalog.FreeReleased(profile.Maids).Count > 2)
            {
                profile.Add(Maid.Create(profile.NextMaidId++, MaidCatalog.FreeReleased(profile.Maids)[0]));
            }

            var board = new ApplicantBoard();
            board.Refresh(new Rng(1u), 6, profile.NextMaidId, profile.Maids);
            Assert.AreEqual(2, board.Candidates.Count);

            profile.Add(Maid.Create(profile.NextMaidId++, MaidCatalog.FreeReleased(profile.Maids)[0]));
            profile.Add(Maid.Create(profile.NextMaidId++, MaidCatalog.FreeReleased(profile.Maids)[0]));
            board.Refresh(new Rng(1u), 6, profile.NextMaidId, profile.Maids);
            Assert.AreEqual(0, board.Candidates.Count);
        }

        [Test]
        public void HiringRefusesAnIdentityThatArrivedOnTheRosterAfterTheBoardWasDrawn()
        {
            var profile = Rich(10u);
            profile.Wallet.Earn(100000);
            var board = new ApplicantBoard();
            board.Refresh(new Rng(3u), 6, profile.NextMaidId, profile.Maids);
            var offered = board.Candidates[0].Maid;

            profile.Add(Maid.Create(profile.NextMaidId++, MaidCatalog.ById(offered.IdentityId)));
            var count = profile.Maids.Count;
            var result = board.Hire(profile, 0);

            Assert.IsFalse(result.Ok);
            StringAssert.Contains("já está no elenco", result.Refusal);
            Assert.AreEqual(count, profile.Maids.Count);
        }

        // ---- saves -------------------------------------------------------------------------------

        [Test]
        public void TheIdentitySurvivesASaveAndALoad()
        {
            var profile = Rich(12u);
            Lootbox.Open(profile, "box_silver", new Rng(5u));
            var loaded = SaveCodec.Decode(SaveCodec.Encode(profile));

            Assert.AreEqual(LoadStatus.Ok, loaded.Status);
            CollectionAssert.AreEqual(profile.Maids.Select(m => m.IdentityId).ToArray(),
                loaded.Profile.Maids.Select(m => m.IdentityId).ToArray());
        }

        // A save from before identities existed: the maid lines carry one field less, and the
        // checksum (FNV-1a over the body) is recomputed. This is the only place that knows the format.
        private static string AsPreIdentitySave(string payload)
        {
            var parts = payload.Split(':');
            var body = Encoding.UTF8.GetString(Convert.FromBase64String(parts[1]));
            var lines = body.Split('\n').Select(line =>
            {
                if (!line.StartsWith("maid=", StringComparison.Ordinal)) return line;
                var cut = line.LastIndexOf('|');
                return line.Substring(0, cut);
            });
            var old = string.Join("\n", lines);

            var hash = 2166136261u;
            foreach (var c in old)
            {
                hash ^= c;
                hash *= 16777619u;
            }

            return parts[0] + ":" + Convert.ToBase64String(Encoding.UTF8.GetBytes(old)) + ":" +
                   hash.ToString("x8", System.Globalization.CultureInfo.InvariantCulture);
        }

        [Test]
        public void AnOldSaveKeepsEveryMaidNameAndProgressAndRecognisesOnlyTheExactCatalogMaids()
        {
            var profile = Profile.NewGame(1u);
            profile.Maids.Clear();
            profile.Lineup.Clear();
            profile.Maids.Add(new Maid { Id = 1, Name = "Suzu", Archetype = Archetype.Genki, Level = 7, Xp = 400, Rarity = Rarity.Epic });
            profile.Maids.Add(new Maid { Id = 2, Name = "Suzu", Archetype = Archetype.Tsundere, Level = 3 });
            profile.Maids.Add(new Maid { Id = 3, Name = "Mina", Archetype = Archetype.Idol, Level = 2, Bond = 2 });
            profile.NextMaidId = 4;

            var loaded = SaveCodec.Decode(AsPreIdentitySave(SaveCodec.Encode(profile)));
            Assert.AreEqual(LoadStatus.Ok, loaded.Status, loaded.Message);
            var maids = loaded.Profile.Maids;

            Assert.AreEqual(3, maids.Count, "nobody is deleted");
            Assert.AreEqual("suzu", maids[0].IdentityId);
            Assert.AreEqual(7, maids[0].Level);
            Assert.AreEqual(400, maids[0].Xp);
            Assert.AreEqual(Rarity.Epic, maids[0].Rarity);

            Assert.IsTrue(maids[1].IsLegacy, "a Suzu of another archetype is not Suzu");
            Assert.AreEqual("Suzu", maids[1].Name, "nobody is renamed");
            Assert.IsTrue(maids[2].IsLegacy);
            Assert.AreEqual("Mina", maids[2].Name);
            Assert.AreEqual(2, maids[2].Bond);
        }

        [Test]
        public void ALegacyMaidSavedOnPurposeStaysLegacyEvenIfHerNameIsACatalogOne()
        {
            var profile = Profile.NewGame(1u);
            profile.Maids.Clear();
            profile.Lineup.Clear();
            profile.Maids.Add(new Maid { Id = 1, Name = "Suzu", Archetype = Archetype.Genki });

            var loaded = SaveCodec.Decode(SaveCodec.Encode(profile));
            Assert.IsTrue(loaded.Profile.Maids[0].IsLegacy);
        }

        [Test]
        public void ASaveNeverLoadsTheSameIdentityTwiceNorAnIdentityOfAnotherArchetype()
        {
            var profile = Profile.NewGame(1u);
            profile.Maids.Clear();
            profile.Lineup.Clear();
            profile.Maids.Add(new Maid { Id = 1, Name = "Suzu", Archetype = Archetype.Genki, IdentityId = "suzu" });
            profile.Maids.Add(new Maid { Id = 2, Name = "Suzu", Archetype = Archetype.Genki, IdentityId = "suzu" });
            profile.Maids.Add(new Maid { Id = 3, Name = "Hina", Archetype = Archetype.Genki, IdentityId = "hina" });
            profile.Maids.Add(new Maid { Id = 4, Name = "X", Archetype = Archetype.Genki, IdentityId = "nobody" });

            var loaded = SaveCodec.Decode(SaveCodec.Encode(profile));
            var maids = loaded.Profile.Maids;

            Assert.AreEqual(4, maids.Count);
            Assert.AreEqual("suzu", maids[0].IdentityId);
            Assert.IsTrue(maids[1].IsLegacy, "second copy becomes legacy");
            Assert.IsTrue(maids[2].IsLegacy, "Hina is Kuudere, this one is Genki");
            Assert.IsTrue(maids[3].IsLegacy, "unknown id");
        }
    }
}

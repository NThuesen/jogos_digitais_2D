using System;
using System.Linq;
using Neko.Sim.Cafe;
using Neko.Sim.Service;
using NUnit.Framework;

namespace Neko.Tests.Sim
{
    public sealed class RecipeBookTests
    {
        [Test]
        public void EveryRecipeIsConsistent()
        {
            foreach (var recipe in RecipeBook.All)
            {
                Assert.IsNotEmpty(recipe.DisplayName, recipe.Id);
                Assert.Greater(recipe.Seconds, 0f, recipe.Id);
                Assert.Greater(recipe.Price, 0, recipe.Id);
                Assert.GreaterOrEqual(recipe.UnlockCafeLevel, 1, recipe.Id);
            }
        }

        [Test]
        public void EveryRecipeIdIsUnique()
        {
            CollectionAssert.AllItemsAreUnique(RecipeBook.All.Select(r => r.Id));
        }

        [Test]
        public void TheStoveCanAlreadyCookSomethingOnDayOne()
        {
            Assert.IsNotEmpty(RecipeBook.ForStation(StationKind.Stove, 1));
        }

        [Test]
        public void EveryStationHasARecipeByTheTimeItCanBeBuilt()
        {
            foreach (StationKind kind in Enum.GetValues(typeof(StationKind)))
            {
                var station = PlaceableCatalog.All.First(d => d.Station == kind);

                Assert.IsNotEmpty(
                    RecipeBook.ForStation(kind, station.UnlockCafeLevel),
                    kind + " can be bought at café level " + station.UnlockCafeLevel +
                    " but has nothing to cook then");
            }
        }

        [Test]
        public void RecipesUnlockWithCafeLevelAndNeverLockAgain()
        {
            var previous = 0;

            for (var level = 1; level <= 10; level++)
            {
                var count = RecipeBook.UnlockedAt(level).Count;
                Assert.GreaterOrEqual(count, previous, "level " + level);
                previous = count;
            }

            Assert.Greater(RecipeBook.UnlockedAt(10).Count, RecipeBook.UnlockedAt(1).Count);
        }

        [Test]
        public void SlowerRecipesPayBetter()
        {
            // Otherwise the player would only ever cook the fastest dish and the kitchen
            // would have one decision in it.
            var ordered = RecipeBook.All.OrderBy(r => r.Seconds).ToList();

            Assert.Less(ordered.First().Price, ordered.Last().Price);
        }

        [Test]
        public void NoRecipeIsStrictlyBetterThanAnother()
        {
            foreach (var a in RecipeBook.All)
            {
                foreach (var b in RecipeBook.All)
                {
                    if (ReferenceEquals(a, b) || a.Station != b.Station) continue;

                    var dominated = a.Seconds <= b.Seconds && a.Price >= b.Price &&
                                    a.UnlockCafeLevel <= b.UnlockCafeLevel;

                    Assert.IsFalse(dominated, b.Id + " is never worth cooking next to " + a.Id);
                }
            }
        }

        [Test]
        public void ByIdRefusesAnUnknownRecipe()
        {
            Assert.Throws<System.Collections.Generic.KeyNotFoundException>(
                () => RecipeBook.ById("nao_existe"));
        }
    }
}

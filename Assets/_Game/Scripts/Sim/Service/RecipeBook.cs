using System.Collections.Generic;
using Neko.Sim.Cafe;

namespace Neko.Sim.Service
{
    /// <summary>
    /// The menu.
    /// </summary>
    /// <remarks>
    /// Within a station, price rises with cooking time and nothing is strictly better than
    /// anything else — a test enforces it. Without that rule the kitchen collapses to "always
    /// cook the fastest dish" and the station choice stops being a choice.
    /// </remarks>
    public static class RecipeBook
    {
        private static readonly Recipe[] Recipes =
        {
            // Stove — the station the café opens with.
            new Recipe("milk", "Leite morno", 3f, 18, StationKind.Stove, 1, "Circle", "#F5EFE2"),
            new Recipe("omurice", "Omurice", 8f, 55, StationKind.Stove, 1, "Teardrop", "#F2C14E"),
            new Recipe("curry", "Curry da casa", 11f, 82, StationKind.Stove, 2, "Capsule", "#B5651D"),
            new Recipe("ramen", "Lámen do chef", 14f, 112, StationKind.Stove, 6, "Ring", "#C9893F"),
            new Recipe("omurice_deluxe", "Omurice imperial", 16f, 140, StationKind.Stove, 9,
                "Teardrop", "#E8A33C"),

            // Tea bar
            new Recipe("matcha", "Matcha", 5f, 38, StationKind.TeaBar, 2, "Capsule", "#7FA86A"),
            new Recipe("bubble_tea", "Chá de bolhas", 7f, 54, StationKind.TeaBar, 2, "Capsule",
                "#C9A27E"),
            new Recipe("royal_tea", "Chá real", 11f, 92, StationKind.TeaBar, 7, "Capsule", "#8E6FA8"),

            // Dessert bench
            new Recipe("pancake", "Panqueca de gato", 6f, 46, StationKind.Dessert, 3, "Circle",
                "#E8B974"),
            new Recipe("parfait", "Parfait", 9f, 72, StationKind.Dessert, 3, "Triangle", "#F2A6C0"),
            new Recipe("sundae", "Sundae estelar", 12f, 98, StationKind.Dessert, 8, "Star",
                "#B98FD1"),
            new Recipe("parfait_royal", "Parfait real", 15f, 135, StationKind.Dessert, 10,
                "Triangle", "#D98FC0"),

            // Bakery
            new Recipe("catbun", "Pãozinho de gato", 7f, 55, StationKind.Bakery, 5, "CatEar",
                "#E0B98A"),
            new Recipe("cake", "Bolo decorado", 13f, 105, StationKind.Bakery, 5, "RoundedRect",
                "#F2C9D6"),
        };

        public static IReadOnlyList<Recipe> All => Recipes;

        public static Recipe ById(string id)
        {
            for (var i = 0; i < Recipes.Length; i++)
            {
                if (Recipes[i].Id == id) return Recipes[i];
            }

            throw new KeyNotFoundException("No recipe called '" + id + "'.");
        }

        public static IReadOnlyList<Recipe> UnlockedAt(int cafeLevel)
        {
            var found = new List<Recipe>();
            for (var i = 0; i < Recipes.Length; i++)
            {
                if (Recipes[i].UnlockCafeLevel <= cafeLevel) found.Add(Recipes[i]);
            }

            return found;
        }

        public static IReadOnlyList<Recipe> ForStation(StationKind station, int cafeLevel)
        {
            var found = new List<Recipe>();
            for (var i = 0; i < Recipes.Length; i++)
            {
                if (Recipes[i].Station == station && Recipes[i].UnlockCafeLevel <= cafeLevel)
                {
                    found.Add(Recipes[i]);
                }
            }

            return found;
        }
    }
}

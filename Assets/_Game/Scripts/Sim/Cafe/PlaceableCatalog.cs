using System.Collections.Generic;
using Neko.Sim.Core;

namespace Neko.Sim.Cafe
{
    /// <summary>
    /// Everything that can stand on the floor, with its price and the café level that opens it.
    /// </summary>
    /// <remarks>
    /// Ambience values are deliberately uneven per credit spent: the cheap pieces are the
    /// efficient ones and the expensive ones buy a theme. That is what keeps the editor a
    /// decision rather than a shopping list sorted by price.
    /// </remarks>
    public static class PlaceableCatalog
    {
        private static readonly PlaceableDef[] Defs =
        {
            // Fixture. Not for sale; the grid places it and the view draws it from here.
            new PlaceableDef("door", "Porta", PlaceableKind.Fixture, 1, 1, "Door", "#8C5A3C", 0, 1),

            // Tables
            new PlaceableDef("table_2", "Mesa para 2", PlaceableKind.Table, 2, 1, "RoundedRect",
                "#C98A5B", 120, 1, seats: 2),
            new PlaceableDef("stool", "Banqueta de balcão", PlaceableKind.Table, 1, 1, "Circle",
                "#B57A4E", 60, 2, seats: 1),
            new PlaceableDef("table_4", "Mesa para 4", PlaceableKind.Table, 2, 2, "RoundedRect",
                "#D19866", 320, 3, seats: 4),

            // Stations
            new PlaceableDef("stove", "Fogão", PlaceableKind.Station, 1, 1, "RoundedRect",
                "#6E6A66", 180, 1, station: StationKind.Stove),
            new PlaceableDef("teabar", "Bar de chá", PlaceableKind.Station, 2, 1, "RoundedRect",
                "#7E9E86", 380, 2, station: StationKind.TeaBar),
            new PlaceableDef("dessert", "Bancada de doces", PlaceableKind.Station, 1, 1,
                "RoundedRect", "#D9A7B8", 450, 3, station: StationKind.Dessert),
            new PlaceableDef("bakery", "Forno", PlaceableKind.Station, 1, 2, "RoundedRect",
                "#A3714A", 700, 5, station: StationKind.Bakery),

            // Décor — unthemed
            new PlaceableDef("plant", "Samambaia", PlaceableKind.Decor, 1, 1, "Teardrop", "#5F9E57",
                80, 1, ambience: 3f),
            new PlaceableDef("window", "Janela", PlaceableKind.Decor, 1, 1, "RoundedRect", "#BBD9E8",
                140, 2, ambience: 4f),
            new PlaceableDef("rug", "Tapete", PlaceableKind.Decor, 2, 2, "RoundedRect", "#C96F7E",
                150, 2, ambience: 5f),
            new PlaceableDef("cat_tower", "Arranhador", PlaceableKind.Decor, 1, 2, "Capsule",
                "#BFA27E", 260, 3, ambience: 8f),
            new PlaceableDef("stage", "Palco", PlaceableKind.Decor, 2, 2, "RoundedRect", "#8E6FA8",
                800, 5, ambience: 12f),

            // Décor — Sakura
            new PlaceableDef("lantern_sakura", "Lanterna de sakura", PlaceableKind.Decor, 1, 1,
                "Circle", "#F2B6C6", 180, 2, ambience: 6f, theme: ThemeTag.Sakura),
            new PlaceableDef("sakura_tree", "Cerejeira de vaso", PlaceableKind.Decor, 1, 1,
                "Triangle", "#E890AB", 420, 4, ambience: 9f, theme: ThemeTag.Sakura),

            // Décor — Gothic
            new PlaceableDef("gothic_candelabra", "Candelabro", PlaceableKind.Decor, 1, 1, "Star",
                "#6B5A78", 190, 3, ambience: 6f, theme: ThemeTag.Gothic),
            new PlaceableDef("gothic_mirror", "Espelho barroco", PlaceableKind.Decor, 1, 1, "Ring",
                "#4E4458", 430, 5, ambience: 9f, theme: ThemeTag.Gothic),

            // Décor — Starry
            new PlaceableDef("star_lamp", "Luminária estelar", PlaceableKind.Decor, 1, 1, "Star",
                "#8FA8E8", 200, 4, ambience: 6f, theme: ThemeTag.Starry),
            new PlaceableDef("planetarium", "Planetário de mesa", PlaceableKind.Decor, 1, 1,
                "Crescent", "#6C7FD1", 520, 6, ambience: 10f, theme: ThemeTag.Starry),

            // Décor — Shōwa
            new PlaceableDef("showa_poster", "Cartaz Shōwa", PlaceableKind.Decor, 1, 1,
                "RoundedRect", "#D9B04A", 160, 4, ambience: 5f, theme: ThemeTag.Showa),
            new PlaceableDef("showa_jukebox", "Jukebox", PlaceableKind.Decor, 1, 1, "Capsule",
                "#C77A3C", 540, 6, ambience: 10f, theme: ThemeTag.Showa),
        };

        public static IReadOnlyList<PlaceableDef> All => Defs;

        public static PlaceableDef ById(string id)
        {
            for (var i = 0; i < Defs.Length; i++)
            {
                if (Defs[i].Id == id) return Defs[i];
            }

            throw new KeyNotFoundException("No placeable called '" + id + "'.");
        }

        public static IReadOnlyList<PlaceableDef> UnlockedAt(int cafeLevel)
        {
            var found = new List<PlaceableDef>();
            for (var i = 0; i < Defs.Length; i++)
            {
                if (Defs[i].UnlockCafeLevel <= cafeLevel) found.Add(Defs[i]);
            }

            return found;
        }

        /// <summary>What the shop offers: everything unlocked that is actually for sale.</summary>
        public static IReadOnlyList<PlaceableDef> ForSaleAt(int cafeLevel)
        {
            var found = new List<PlaceableDef>();
            for (var i = 0; i < Defs.Length; i++)
            {
                var def = Defs[i];
                if (def.Kind != PlaceableKind.Fixture && def.UnlockCafeLevel <= cafeLevel)
                {
                    found.Add(def);
                }
            }

            return found;
        }
    }
}

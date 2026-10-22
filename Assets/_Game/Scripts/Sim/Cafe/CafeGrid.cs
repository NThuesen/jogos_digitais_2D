using System;
using System.Collections.Generic;
using Neko.Sim.Core;

namespace Neko.Sim.Cafe
{
    public readonly struct Cell
    {
        public readonly int X;
        public readonly int Y;

        public Cell(int x, int y)
        {
            X = x;
            Y = y;
        }

        /// <summary>
        /// Chebyshev distance: walking diagonally costs the same as walking straight, which is
        /// how the workers actually move.
        /// </summary>
        public int DistanceTo(Cell other) =>
            Math.Max(Math.Abs(X - other.X), Math.Abs(Y - other.Y));

        public override string ToString() => "(" + X + ", " + Y + ")";
    }

    public readonly struct StationSpot
    {
        public readonly StationKind Kind;
        public readonly Cell Cell;
        public readonly PlacedItem Item;

        public StationSpot(StationKind kind, Cell cell, PlacedItem item)
        {
            Kind = kind;
            Cell = cell;
            Item = item;
        }
    }

    /// <summary>The room, as a grid of occupied and empty cells.</summary>
    public sealed class CafeGrid
    {
        /// <summary>Below this many pieces, a shared theme is a coincidence rather than a café.</summary>
        public const int MinimumDecorForATheme = 3;

        public const float ThemeShareNeeded = 0.70f;

        private readonly List<PlacedItem> _items = new List<PlacedItem>();

        public CafeGrid(int width, int height)
        {
            Width = Math.Max(1, width);
            Height = Math.Max(1, height);
        }

        public int Width { get; private set; }

        public int Height { get; private set; }

        /// <summary>
        /// Where customers come in. Fixed to the middle of the left wall rather than placeable:
        /// a door the player can move turns every Flow comparison into a different room.
        /// </summary>
        public Cell Door => new Cell(0, Height / 2);

        public IReadOnlyList<PlacedItem> Items => _items;

        /// <summary>The room a new game starts in: small, but able to serve somebody on day one.</summary>
        public static CafeGrid StartingRoom()
        {
            var grid = new CafeGrid(7, 5);
            grid.Place(PlaceableCatalog.ById("table_2"), 3, 1);
            grid.Place(PlaceableCatalog.ById("table_2"), 3, 3);
            grid.Place(PlaceableCatalog.ById("stove"), 5, 2);
            grid.Place(PlaceableCatalog.ById("plant"), 1, 0);
            return grid;
        }

        public bool InBounds(int x, int y) => x >= 0 && y >= 0 && x < Width && y < Height;

        public PlacedItem ItemAt(int x, int y)
        {
            for (var i = 0; i < _items.Count; i++)
            {
                if (_items[i].Covers(x, y)) return _items[i];
            }

            return null;
        }

        public bool CanPlace(PlaceableDef def, int x, int y, bool rotated = false)
        {
            if (def == null) return false;

            var probe = new PlacedItem(def, x, y, rotated);
            var door = Door;

            for (var dx = 0; dx < probe.Width; dx++)
            {
                for (var dy = 0; dy < probe.Height; dy++)
                {
                    var cx = x + dx;
                    var cy = y + dy;

                    if (!InBounds(cx, cy)) return false;
                    if (cx == door.X && cy == door.Y) return false;
                    if (ItemAt(cx, cy) != null) return false;
                }
            }

            return true;
        }

        public bool Place(PlaceableDef def, int x, int y, bool rotated = false)
        {
            if (!CanPlace(def, x, y, rotated)) return false;

            _items.Add(new PlacedItem(def, x, y, rotated));
            return true;
        }

        public bool Remove(int x, int y)
        {
            var item = ItemAt(x, y);
            return item != null && _items.Remove(item);
        }

        /// <summary>
        /// Grows or shrinks the room, dropping whatever no longer fits.
        /// </summary>
        /// <remarks>
        /// Shrinking only happens in tests today, but it has to be defined: a save written by
        /// a future version with a bigger room must not place furniture outside the wall.
        /// </remarks>
        public void Resize(int width, int height)
        {
            Width = Math.Max(1, width);
            Height = Math.Max(1, height);

            var door = Door;
            for (var i = _items.Count - 1; i >= 0; i--)
            {
                var item = _items[i];
                var fits = item.X >= 0 && item.Y >= 0 &&
                           item.X + item.Width <= Width && item.Y + item.Height <= Height &&
                           !item.Covers(door.X, door.Y);

                if (!fits) _items.RemoveAt(i);
            }
        }

        /// <summary>One cell per seat, so a four-seat table yields four places to sit.</summary>
        public IReadOnlyList<Cell> SeatPositions()
        {
            var seats = new List<Cell>();

            for (var i = 0; i < _items.Count; i++)
            {
                var item = _items[i];
                if (item.Def.Kind != PlaceableKind.Table) continue;

                for (var seat = 0; seat < item.Def.Seats; seat++)
                {
                    // Seats run along the footprint and wrap; a 2x2 four-seater fills its four
                    // cells, a 2x1 two-seater fills its two.
                    var dx = seat % Math.Max(1, item.Width);
                    var dy = seat / Math.Max(1, item.Width) % Math.Max(1, item.Height);
                    seats.Add(new Cell(item.X + dx, item.Y + dy));
                }
            }

            return seats;
        }

        public IReadOnlyList<StationSpot> Stations()
        {
            var spots = new List<StationSpot>();

            for (var i = 0; i < _items.Count; i++)
            {
                var item = _items[i];
                if (item.Def.Station.HasValue)
                {
                    spots.Add(new StationSpot(item.Def.Station.Value, new Cell(item.X, item.Y), item));
                }
            }

            return spots;
        }

        public CafeMetrics Metrics()
        {
            var seats = SeatPositions();
            var stations = Stations();

            var ambience = 0f;
            var decorCount = 0;
            var byTheme = new int[Enum.GetValues(typeof(ThemeTag)).Length];

            for (var i = 0; i < _items.Count; i++)
            {
                var def = _items[i].Def;
                ambience += def.Ambience;

                if (def.Kind != PlaceableKind.Decor) continue;

                decorCount++;
                byTheme[(int)def.Theme]++;
            }

            // Divided by the square root of the area, not the area, so that expanding the room
            // dilutes what the player has bought instead of erasing it.
            ambience /= (float)Math.Sqrt(Width * Height);

            var dominant = ThemeTag.None;
            var dominantCount = 0;
            for (var i = 1; i < byTheme.Length; i++)
            {
                if (byTheme[i] <= dominantCount) continue;
                dominantCount = byTheme[i];
                dominant = (ThemeTag)i;
            }

            var themeBonus = decorCount >= MinimumDecorForATheme &&
                             dominant != ThemeTag.None &&
                             dominantCount / (float)decorCount >= ThemeShareNeeded;

            var patience = Curves.Clamp(1f + ambience * 0.05f, 1f, 2f);
            var tip = 1f + ambience * 0.07f;
            if (themeBonus) tip *= 1.25f;
            tip = Curves.Clamp(tip, 1f, 2f);

            return new CafeMetrics(ambience, Flow(seats, stations), seats.Count, stations.Count,
                dominant, themeBonus, patience, tip);
        }

        /// <summary>
        /// Mean walk over the two legs every order makes: door to seat, and seat to the station
        /// that will cook for it.
        /// </summary>
        /// <remarks>
        /// A room with no seats scores 0 rather than dividing by zero — "no walking because
        /// there is nowhere to walk" is the honest answer, and the shift refuses to start for
        /// its own reasons. A room with seats and no station charges the full width of the
        /// room for the missing leg, so adding the first station is visibly an improvement.
        /// </remarks>
        private float Flow(IReadOnlyList<Cell> seats, IReadOnlyList<StationSpot> stations)
        {
            if (seats.Count == 0) return 0f;

            var door = Door;
            var noStationPenalty = Width + Height;
            var total = 0f;

            for (var i = 0; i < seats.Count; i++)
            {
                var seat = seats[i];
                total += door.DistanceTo(seat);

                if (stations.Count == 0)
                {
                    total += noStationPenalty;
                    continue;
                }

                var nearest = int.MaxValue;
                for (var s = 0; s < stations.Count; s++)
                {
                    var distance = seat.DistanceTo(stations[s].Cell);
                    if (distance < nearest) nearest = distance;
                }

                total += nearest;
            }

            return total / seats.Count;
        }
    }
}

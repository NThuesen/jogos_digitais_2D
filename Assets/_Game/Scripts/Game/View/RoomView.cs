using System.Collections.Generic;
using Neko.Game.Art;
using Neko.Game.Core;
using Neko.Sim.Cafe;
using UnityEngine;

namespace Neko.Game.View
{
    /// <summary>
    /// Draws the floor, the walls and everything standing on them.
    /// </summary>
    /// <remarks>
    /// Rebuilt wholesale whenever the layout changes rather than diffed. The room is at most
    /// a hundred and seventeen tiles and the editor changes it a few times a minute; a diff
    /// would be more code than the thing it saves.
    /// </remarks>
    public sealed partial class RoomView : MonoBehaviour
    {
        private readonly List<GameObject> _built = new List<GameObject>();
        private readonly List<SpriteRenderer> _flowTiles = new List<SpriteRenderer>();

        private SpriteFactory _sprites;
        private CafeGrid _grid;
        private bool _flowVisible;

        public void Bind(SpriteFactory sprites) => _sprites = sprites;

        public void Build(CafeGrid grid)
        {
            _grid = grid;
            Clear();
            if (grid == null) return;

            BuildAmbience(grid);
            BuildFloor(grid);
            BuildDoor(grid);

            foreach (var item in grid.Items) BuildItem(item);
        }

        public void SetFlowOverlay(bool visible)
        {
            _flowVisible = visible;
            for (var i = 0; i < _flowTiles.Count; i++)
            {
                if (_flowTiles[i] != null) _flowTiles[i].enabled = visible;
            }
        }

        private void BuildFloor(CafeGrid grid)
        {
            var plain = _sprites.Solid(Palette.Lighten(Palette.Wood, 0.10f));
            var alternate = _sprites.Solid(Palette.Wood);
            var joint = _sprites.Solid(Palette.WithAlpha(Palette.Shadow, 0.28f));

            // A skirting line around the floor. One rectangle, and the room has walls.
            var surround = Make("Walls", new Vector3((grid.Width - 1) * 0.5f,
                    (grid.Height - 1) * 0.5f, 0f), _sprites.Solid(Palette.Shadow),
                Layers.Floor, grid.Height + 2f);
            surround.transform.localScale = new Vector3(grid.Width + 0.5f, grid.Height + 0.5f, 1f);

            for (var x = 0; x < grid.Width; x++)
            {
                for (var y = 0; y < grid.Height; y++)
                {
                    // A checker. Enough to read as a floor and to count tiles by eye, not
                    // enough to compete with anything standing on it.
                    var tile = Make("Tile", new Vector3(x, y, 0f),
                        (x + y) % 2 == 0 ? plain : alternate, Layers.Floor, y);
                    tile.transform.localScale = Vector3.one;

                    // A plank joint, staggered row to row, so the boards read as boards.
                    var seam = Make("Seam", new Vector3(x + ((y & 1) == 0 ? 0.5f : 0f), y, 0f), joint,
                        Layers.Floor, y - 0.01f);
                    seam.transform.localScale = new Vector3(0.025f, 0.98f, 1f);

                    var overlay = Make("Flow", new Vector3(x, y, 0f), _sprites.Solid(Color.white),
                        Layers.Rug, y);
                    var renderer = overlay.GetComponent<SpriteRenderer>();
                    renderer.color = FlowTint(grid, x, y);
                    renderer.enabled = _flowVisible;
                    _flowTiles.Add(renderer);
                }
            }
        }

        /// <summary>
        /// How far this tile is from the door, as a heat tint.
        /// </summary>
        /// <remarks>
        /// The editor's Flow number says a layout is bad; this says <em>where</em>. One is a
        /// score and the other is advice.
        /// </remarks>
        private static Color FlowTint(CafeGrid grid, int x, int y)
        {
            var distance = grid.Door.DistanceTo(new Cell(x, y));
            var worst = Mathf.Max(grid.Width, grid.Height);
            var t = Mathf.Clamp01(distance / (float)worst);

            return Palette.WithAlpha(Color.Lerp(Palette.Good, Palette.Alarm, t), 0.35f);
        }

        private void BuildDoor(CafeGrid grid)
        {
            var sprite = _sprites.Furniture(PlaceableCatalog.ById("door"), 1, 1);

            Make("Door", new Vector3(grid.Door.X, grid.Door.Y, 0f), sprite, Layers.Furniture,
                grid.Door.Y);
        }

        /// <summary>
        /// One piece, painted for its footprint and pivoted on the footprint's centre.
        /// </summary>
        /// <remarks>
        /// Anything solid joins the characters' sorting band, keyed by the front edge of its
        /// footprint (see <see cref="DepthSort"/>): a fern in front of a table covers the table's
        /// legs, and a maid behind the fern is covered by it. Only what lies flat — rugs, the
        /// pond's water — stays underneath everybody. Procedural art is painted at one tile per
        /// unit and is never scaled; illustrated art is placed by its own calibration.
        /// </remarks>
        private void BuildItem(PlacedItem item)
        {
            var def = item.Def;
            var centre = new Vector3(item.X + (item.Width - 1) * 0.5f,
                item.Y + (item.Height - 1) * 0.5f, 0f);

            if (!BuildIllustrated(item, centre))
            {
                var sprite = _sprites.Furniture(def, item.Width, item.Height);
                var flat = FurnitureArt.LiesFlat(def.Id);
                var piece = Make(def.Id, centre, sprite, flat ? Layers.Rug : Layers.Furniture, item.Y);
                if (!flat) SortAsSolid(piece, item);
            }

            if (def.Kind != PlaceableKind.Station) return;

            // Stations get a warm glow underneath so the kitchen reads as the kitchen.
            var glow = Make(def.Id + "Glow", centre + new Vector3(0f, -0.08f, 0f),
                _sprites.Get(new ShapeSpec(ShapeId.Circle,
                    Palette.WithAlpha(Palette.Focus, 0.22f))), Layers.Rug, item.Y);
            glow.transform.localScale = Vector3.one * (1.2f + item.Width * 0.2f);
        }

        /// <summary>
        /// Draws the piece from its illustrated PNG when it has one for this footprint.
        /// </summary>
        /// <remarks>
        /// The art's pivot is its bottom centre and it sits on the footprint's front edge. A piece
        /// with a raised layer draws the whole picture flat underneath everyone and the standing
        /// parts again, same size and same pivot, among the solids.
        /// </remarks>
        private bool BuildIllustrated(PlacedItem item, Vector3 centre)
        {
            var entry = IllustratedFurniture.For(item.Def, item.Width, item.Height);
            if (entry == null) return false;

            var whole = _sprites.Illustrated(entry, false);
            if (whole == null) return false;

            Sprite raised = null;
            if (entry.HasRaisedLayer)
            {
                raised = _sprites.Illustrated(entry, true);
                if (raised == null) return false;
            }

            var foot = new Vector3(centre.x, DepthSort.FrontEdge(item) + entry.BottomLift, 0f);

            if (entry.HasFrontLayer)
            {
                // The front layer is the piece, keyed by its front edge like any solid; the whole picture
                // goes behind it, and seated customers are keyed between the two (DepthSort.SeatedKey). A front
                // layer that does not load leaves the whole picture, drawn the ordinary way.
                var front = _sprites.IllustratedFront(entry);
                if (front != null)
                {
                    Layers.ApplyActor(Make(item.Def.Id + "Back", foot, whole, Layers.Furniture, item.Y)
                        .GetComponent<SpriteRenderer>(), DepthSort.BackKey(item));
                    Layers.ApplyActor(Make(item.Def.Id, foot, front, Layers.Furniture, item.Y)
                        .GetComponent<SpriteRenderer>(), DepthSort.SolidKey(item));
                    return true;
                }
            }

            if (raised == null)
            {
                SortAsSolid(Make(item.Def.Id, foot, whole, Layers.Furniture, item.Y), item);
                return true;
            }

            Make(item.Def.Id + "Base", foot, whole, Layers.Rug, item.Y);
            // The pond is "flat" as a whole, so its standing part is keyed here, not by IsSolid.
            var standing = Make(item.Def.Id + "Raised", foot, raised, Layers.Furniture, item.Y);
            Layers.ApplyActor(standing.GetComponent<SpriteRenderer>(), DepthSort.SolidKey(item));
            return true;
        }

        private static void SortAsSolid(GameObject piece, PlacedItem item)
        {
            if (!DepthSort.IsSolid(item.Def)) return;
            Layers.ApplyActor(piece.GetComponent<SpriteRenderer>(), DepthSort.SolidKey(item));
        }

        private GameObject Make(string name, Vector3 position, Sprite sprite, int layer, float y)
        {
            var go = new GameObject(name);
            go.transform.SetParent(transform, false);
            go.transform.position = position;

            var renderer = go.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            Layers.Apply(renderer, layer, y);

            _built.Add(go);
            return go;
        }

        private void Clear()
        {
            for (var i = 0; i < _built.Count; i++)
            {
                if (_built[i] != null) Destroy(_built[i]);
            }

            _built.Clear();
            _flowTiles.Clear();
        }

        public CafeGrid Grid => _grid;
    }
}

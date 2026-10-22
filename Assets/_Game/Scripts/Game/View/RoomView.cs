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
    public sealed class RoomView : MonoBehaviour
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
            var plain = _sprites.Solid(Palette.Lighten(Palette.Wood, 0.18f));
            var alternate = _sprites.Solid(Palette.Wood);

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
            var def = PlaceableCatalog.ById("door");
            var sprite = _sprites.Get(new ShapeSpec(Shapes.Parse(def.ShapeId),
                Palette.Hex(def.TintHex), Palette.Ink, 0.04f));

            Make("Door", new Vector3(grid.Door.X, grid.Door.Y, 0f), sprite, Layers.Furniture,
                grid.Door.Y);
        }

        private void BuildItem(PlacedItem item)
        {
            var def = item.Def;
            var tint = Palette.Hex(def.TintHex);

            var sprite = _sprites.Get(new ShapeSpec(Shapes.Parse(def.ShapeId), tint, Palette.Ink,
                0.035f, detail: def.Kind == PlaceableKind.Table ? 0.45f : 0.3f));

            var centre = new Vector3(item.X + (item.Width - 1) * 0.5f,
                item.Y + (item.Height - 1) * 0.5f, 0f);

            var go = Make(def.Id, centre, sprite,
                def.Kind == PlaceableKind.Decor ? Layers.Rug : Layers.Furniture, item.Y);

            // Shapes are authored square; the footprint decides the rest.
            go.transform.localScale = new Vector3(item.Width * 0.94f, item.Height * 0.94f, 1f);

            if (def.Kind != PlaceableKind.Station) return;

            // Stations get a warm glow underneath so the kitchen reads as the kitchen.
            var glow = Make(def.Id + "Glow", centre + new Vector3(0f, -0.08f, 0f),
                _sprites.Get(new ShapeSpec(ShapeId.Circle,
                    Palette.WithAlpha(Palette.Focus, 0.22f))), Layers.Rug, item.Y);
            glow.transform.localScale = Vector3.one * (1.2f + item.Width * 0.2f);
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

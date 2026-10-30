using Neko.Game.Art;
using Neko.Sim.Maids;
using UnityEngine;

namespace Neko.Game.View
{
    /// <summary>
    /// Everything the view needs to know about Suzu's authored walk sheet.
    /// </summary>
    /// <remarks>
    /// The numbers describe <c>Resources/Suzu/suzu_walk_sheet.png</c> and are written by
    /// <c>tools/art/suzu_walk/process.py</c> into <c>docs/art/suzu-walk/manifest.json</c>; a test
    /// keeps the two in step. Nothing here is saved or persisted: who gets the art is decided
    /// by name and archetype, a stand-in until a maid carries a visual id of her own.
    /// </remarks>
    public static class SuzuSprites
    {
        public const string Resource = "Suzu/suzu_walk_sheet";
        public const int Frames = 30;
        public const int Columns = 6;
        public const int Rows = 5;
        public const int CellWidth = 92;
        public const int CellHeight = 126;
        public const float PixelsPerUnit = 96f;

        /// <summary>Standing frame. Feet level and close together.</summary>
        public const int IdleFrame = 10;

        /// <summary>Frames with both shoes on the floor; a stopped cycle runs on to one of them.</summary>
        public static readonly int[] RestFrames = { 0, 10, 11, 12, 13, 26 };

        /// <summary>World distance covered by one full 30-frame cycle (two steps).</summary>
        public const float UnitsPerCycle = 1.2f;

        /// <summary>
        /// Puts her soles on the line the procedural maid's shoes sit on. The sprite's pivot is the
        /// sole of the planted foot, so this is the only vertical constant.
        /// </summary>
        public static readonly Vector3 BodyOffset = new Vector3(0f, -0.58f, 0f);

        /// <summary>Where the carried plate goes: at her hands rather than over the procedural tray.</summary>
        public static readonly Vector3 CarriedOffset = new Vector3(0f, -0.04f, 0f);

        /// <summary>
        /// True for the one maid these frames depict. Equipment is drawn as layers of the
        /// procedural figure, which this picture has no equivalent for, so a geared maid keeps the
        /// layered look and the pillar that gear is visible survives.
        /// </summary>
        public static bool Applies(Maid maid)
        {
            if (maid == null || maid.Name != "Suzu" || maid.Archetype != Archetype.Genki) return false;

            for (var i = 0; i < maid.Gear.Length; i++)
            {
                if (maid.Gear[i] != null) return false;
            }

            return true;
        }

        /// <summary>The frames, or null when the sheet is missing or does not match this layout.</summary>
        public static Sprite[] Load(SpriteFactory sprites) =>
            sprites.Sheet(Resource, Columns, Rows, CellWidth, CellHeight, Frames, PixelsPerUnit);
    }
}

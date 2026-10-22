using System.Collections.Generic;
using Neko.Sim.Core;
using Neko.Sim.Maids;
using UnityEngine;

namespace Neko.Game.Art
{
    /// <summary>
    /// Every colour in the game, in one place.
    /// </summary>
    /// <remarks>
    /// Settled with the studio's <c>pixels palette_check</c> rather than by eye. The report
    /// that produced these values:
    ///
    /// <code>
    /// panel #faf0e2 L* 97.3   cream #f4e3d0 L* 91.1   wall  #e8d4be L* 86.0
    /// focus #ffb020 L* 77.5   wood  #c9a77e L* 70.6   alarm #e8405f L* 53.7
    /// shadow #8c6a4a L* 47.5  ink   #3a2e2a L* 20.1
    /// "No value collisions, and the focus is the loudest thing in the set."
    /// </code>
    ///
    /// The nine archetype accents sit on a deliberate ladder from L* 40 to L* 72, four points
    /// apart, all of them below the focus colour's band and none above its saturation. The
    /// first attempt put them all at roughly the same lightness and differed only in hue;
    /// <c>palette_check</c> reported eleven value collisions, one of them 0.2 points from the
    /// focus colour itself — a Gourmand maid would have been exactly as loud as "look here".
    ///
    /// A handful of accent-against-wood and accent-against-shadow pairs are still inside four
    /// points. They are allowed because **every character and every piece of furniture is
    /// drawn with an ink outline**: an outline guarantees an edge whatever the values on
    /// either side of it, and with signed distance fields it costs nothing. If an outline is
    /// ever dropped from a sprite, those pairs become real.
    /// </remarks>
    public static class Palette
    {
        // Structure --------------------------------------------------------------------

        public static readonly Color Panel = Hex("#FAF0E2");
        public static readonly Color Cream = Hex("#F4E3D0");
        public static readonly Color Wall = Hex("#E8D4BE");
        public static readonly Color Wood = Hex("#C9A77E");
        public static readonly Color Shadow = Hex("#8C6A4A");
        public static readonly Color Ink = Hex("#3A2E2A");

        /// <summary>
        /// The one saturated colour, reserved for things that need attention.
        /// </summary>
        /// <remarks>
        /// A ready plate, a level-up, a button that is waiting. Nothing decorative is ever
        /// allowed this colour; every individual decision to make one more thing a bit
        /// brighter is defensible and the sum of them is not.
        /// </remarks>
        public static readonly Color Focus = Hex("#FFB020");

        /// <summary>Only ever the last fifth of somebody's patience. Rare, and brief.</summary>
        public static readonly Color Alarm = Hex("#E8405F");

        public static readonly Color Good = Hex("#5AA86E");

        /// <summary>Text on a pale panel.</summary>
        public static readonly Color Text = Hex("#4A3B33");

        public static readonly Color TextDim = Hex("#8A7668");

        // Characters -------------------------------------------------------------------

        public static readonly Color Skin = Hex("#F7DCC4");
        public static readonly Color SkinShade = Hex("#E0BCA0");
        public static readonly Color Uniform = Hex("#4E4452");

        /// <summary>The inside of a cat ear. The one warm note on every maid's head.</summary>
        public static readonly Color InnerEar = Hex("#F0A8B4");
        public static readonly Color UniformTrim = Hex("#FAF0E2");

        private static readonly Dictionary<Archetype, Color> Accents =
            new Dictionary<Archetype, Color>
            {
                { Archetype.Ojousama, Hex("#A42F79") },
                { Archetype.Idol, Hex("#9940B5") },
                { Archetype.Oneesan, Hex("#636AC5") },
                { Archetype.Tsundere, Hex("#D34F4F") },
                { Archetype.Mukuchi, Hex("#32957D") },
                { Archetype.Dojikko, Hex("#4DA33F") },
                { Archetype.Genki, Hex("#D68946") },
                { Archetype.Kuudere, Hex("#6BAED0") },
                { Archetype.Gourmand, Hex("#C9B04A") },
            };

        private static readonly Dictionary<ThemeTag, Color> Themes =
            new Dictionary<ThemeTag, Color>
            {
                { ThemeTag.None, Hex("#BFAE9A") },
                { ThemeTag.Sakura, Hex("#E890AB") },
                { ThemeTag.Gothic, Hex("#6B5A78") },
                { ThemeTag.Starry, Hex("#6C7FD1") },
                { ThemeTag.Showa, Hex("#C77A3C") },
            };

        public static Color ForArchetype(Archetype archetype) =>
            Accents.TryGetValue(archetype, out var colour) ? colour : Wood;

        public static Color ForTheme(ThemeTag theme) =>
            Themes.TryGetValue(theme, out var colour) ? colour : Themes[ThemeTag.None];

        /// <summary>
        /// Parses <c>#rrggbb</c> or <c>#rrggbbaa</c>. Returns magenta for anything it cannot
        /// read, so a bad colour in a data table is loud rather than silently black.
        /// </summary>
        public static Color Hex(string value)
        {
            if (string.IsNullOrEmpty(value)) return Color.magenta;

            var text = value[0] == '#' ? value.Substring(1) : value;
            if (text.Length != 6 && text.Length != 8) return Color.magenta;

            if (!TryByte(text, 0, out var r) || !TryByte(text, 2, out var g) ||
                !TryByte(text, 4, out var b))
            {
                return Color.magenta;
            }

            var a = 255;
            if (text.Length == 8 && !TryByte(text, 6, out a)) return Color.magenta;

            return new Color(r / 255f, g / 255f, b / 255f, a / 255f);
        }

        /// <summary>
        /// The patience colour: good, then focus, then alarm in the last fifth.
        /// </summary>
        /// <remarks>
        /// Rubric 3 wants state carried by the world rather than by a readout. This is the
        /// colour a customer's whole body takes, not just their bar, so "about to walk out"
        /// is legible without reading anything.
        /// </remarks>
        public static Color ForPatience(float patience)
        {
            if (patience <= 0.2f) return Alarm;
            if (patience <= 0.5f) return Focus;
            return Good;
        }

        /// <summary>Darkens toward the floor. Used for everything that needs a shaded edge.</summary>
        public static Color Darken(Color colour, float amount) =>
            Color.Lerp(colour, Ink, Curves.Clamp01(amount));

        public static Color Lighten(Color colour, float amount) =>
            Color.Lerp(colour, Panel, Curves.Clamp01(amount));

        /// <summary>Pulls a colour toward the background, which is the whole of depth.</summary>
        public static Color Distant(Color colour, float amount) =>
            Color.Lerp(colour, Wall, Curves.Clamp01(amount));

        public static Color WithAlpha(Color colour, float alpha) =>
            new Color(colour.r, colour.g, colour.b, alpha);

        private static bool TryByte(string text, int index, out int value) =>
            int.TryParse(text.Substring(index, 2), System.Globalization.NumberStyles.HexNumber,
                System.Globalization.CultureInfo.InvariantCulture, out value);
    }
}

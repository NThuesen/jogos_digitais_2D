using UnityEngine;

namespace Neko.Game.Ui
{
    /// <summary>
    /// The one font in the game, resolved once.
    /// </summary>
    /// <remarks>
    /// TextMeshPro is not used. It needs a font asset, and this project has no TMP resources
    /// in it — checked rather than assumed: the ugui package in this editor ships none, so TMP
    /// text would lay out correctly and draw nothing at all. Legacy uGUI <c>Text</c> with a
    /// built-in font draws, works in a WebGL build, and imports no asset.
    ///
    /// The chain exists because the name of the built-in font changed in Unity 2022 and a
    /// game that silently loses all of its text on a different editor is worse than one that
    /// says so at boot.
    /// </remarks>
    public static class Typeface
    {
        private static Font _font;

        public static Font Font
        {
            get
            {
                if (_font != null) return _font;

                _font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                if (_font != null) return _font;

                _font = Resources.GetBuiltinResource<Font>("Arial.ttf");
                if (_font != null) return _font;

                _font = Font.CreateDynamicFontFromOSFont(
                    new[] { "Liberation Sans", "DejaVu Sans", "Arial", "Noto Sans" }, 32);

                if (_font == null)
                {
                    Debug.LogError("Nenhuma fonte disponível: o jogo vai rodar sem texto.");
                }

                return _font;
            }
        }

        /// <summary>Point sizes, so type scale is a decision taken once.</summary>
        public const int Tiny = 14;

        public const int Small = 18;
        public const int Body = 24;
        public const int Heading = 34;
        public const int Title = 56;
    }
}

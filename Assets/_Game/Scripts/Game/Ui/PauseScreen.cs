using Neko.Game.Art;
using UnityEngine;

namespace Neko.Game.Ui
{
    /// <summary>Stops the café and asks what next.</summary>
    public sealed class PauseScreen : Screen
    {
        public System.Action OnResume;
        public System.Action OnQuit;
        public System.Action<bool> OnMusic;
        public System.Action<bool> OnSfx;

        private bool _music = true;
        private bool _sfx = true;

        protected override void Build()
        {
            Backdrop(Palette.WithAlpha(Palette.Ink, 0.6f));

            var box = UiKit.Panel(transform, new Vector2(460f, 400f), Palette.Panel);
            UiKit.Anchor(box, new Vector2(0.5f, 0.5f), Vector2.zero);

            var column = UiKit.Column(box, 14f, 28f);
            UiKit.Stretch(column);

            var title = UiKit.Label(column, Strings.Get("pause.title"), Typeface.Heading,
                Palette.Text, bold: true);
            title.rectTransform.sizeDelta = new Vector2(400f, 48f);

            UiKit.Button(column, Strings.Get("pause.resume"), new Vector2(300f, 56f),
                Palette.Focus, () => OnResume?.Invoke(), Typeface.Heading);

            var music = UiKit.Button(column, "", new Vector2(300f, 48f), Palette.Wall, null);
            var musicLabel = music.GetComponentInChildren<UnityEngine.UI.Text>();
            musicLabel.text = Label("settings.music", _music);
            music.onClick.AddListener(() =>
            {
                _music = !_music;
                musicLabel.text = Label("settings.music", _music);
                OnMusic?.Invoke(_music);
            });

            var sfx = UiKit.Button(column, "", new Vector2(300f, 48f), Palette.Wall, null);
            var sfxLabel = sfx.GetComponentInChildren<UnityEngine.UI.Text>();
            sfxLabel.text = Label("settings.sfx", _sfx);
            sfx.onClick.AddListener(() =>
            {
                _sfx = !_sfx;
                sfxLabel.text = Label("settings.sfx", _sfx);
                OnSfx?.Invoke(_sfx);
            });

            UiKit.Button(column, Strings.Get("pause.quit"), new Vector2(300f, 48f),
                Palette.Wall, () => OnQuit?.Invoke());
        }

        private static string Label(string key, bool on) =>
            Strings.Get(key) + ": " + Strings.Get(on ? "dialog.yes" : "dialog.no");
    }
}

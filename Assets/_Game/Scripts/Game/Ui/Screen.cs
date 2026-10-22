using Neko.Game.Art;
using Neko.Game.Feel;
using UnityEngine;

namespace Neko.Game.Ui
{
    /// <summary>
    /// One full-screen page of interface.
    /// </summary>
    /// <remarks>
    /// Built once on first show and then only refreshed, because rebuilding a list of twenty
    /// rows every time somebody opens the roster is how a WebGL build starts dropping frames
    /// on a menu.
    /// </remarks>
    public abstract class Screen : MonoBehaviour
    {
        private bool _built;

        public RectTransform Rect { get; private set; }

        public bool Visible => gameObject.activeSelf;

        protected UiRoot Root { get; private set; }

        public void Attach(UiRoot root)
        {
            Root = root;
            Rect = GetComponent<RectTransform>();
            UiKit.Stretch(Rect);
            gameObject.SetActive(false);
        }

        public void Show()
        {
            if (!_built)
            {
                Build();
                _built = true;
            }

            gameObject.SetActive(true);
            Refresh();

            Rect.localScale = Vector3.one * 0.985f;
            Tween.Scale(Rect, Vector3.one * 0.985f, Vector3.one, 0.18f, Ease.OutCubic);
        }

        public void Hide() => gameObject.SetActive(false);

        /// <summary>Creates the widgets. Called once.</summary>
        protected abstract void Build();

        /// <summary>Puts today's numbers into them. Called every time it is shown.</summary>
        public virtual void Refresh()
        {
        }

        protected RectTransform Backdrop(Color colour)
        {
            var panel = UiKit.Panel(transform, Vector2.one * 100f, colour, 0f);
            UiKit.Stretch(panel);
            panel.SetAsFirstSibling();
            return panel;
        }
    }
}

using System.Collections;
using Neko.Game.Core;
using Neko.Game.Ui;
using Neko.Game.View;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Neko.Tests.PlayMode
{
    /// <summary>
    /// What dressing the room costs: a budget on how many sprites the starting café draws, so a
    /// prettier wall can never quietly become a heavier one.
    /// </summary>
    public sealed class RoomCostTests
    {
        private const string ScenePath = "Assets/_Game/Scenes/Boot.unity";

        /// <summary>The starting café draws well under this many room sprites today.</summary>
        private const int Budget = 260;

        [UnityTest]
        public IEnumerator TheStartingRoomStaysInsideItsSpriteBudget()
        {
            yield return SceneManager.LoadSceneAsync(ScenePath, LoadSceneMode.Single);
            yield return null;

            var root = GameRoot.Instance;
            root.Ui.Get<MenuScreen>().OnNewGame.Invoke();
            yield return null;
            root.Ui.Get<HubScreen>().OnOpenDay.Invoke();
            yield return null;
            yield return null;

            var room = Object.FindAnyObjectByType<RoomView>();
            var renderers = room.GetComponentsInChildren<SpriteRenderer>();
            Debug.Log("[RoomCost] room sprites: " + renderers.Length);

            Assert.LessOrEqual(renderers.Length, Budget, "the room draws " + renderers.Length + " sprites");

            // Nothing in the room may be a collider or a script: it is paint, not behaviour.
            Assert.AreEqual(0, room.GetComponentsInChildren<Collider2D>().Length);

            // And dressing the wall must not shrink the people: the camera frames the room exactly
            // as it did before the wall existed (0.8 tiles of head room above the top row).
            const float originalHeadRoom = 0.8f;
            var grid = root.Presenter.Shift.Grid;
            var low = CameraRig.ShiftBottomInset / 720f;
            var high = 1f - CameraRig.ShiftTopInset / 720f;
            var top = grid.Height - 0.5f + originalHeadRoom;
            var bottom = -0.5f - CameraRig.FootRoom;
            var heightFit = (top - bottom) / (2f * (high - low));
            var camera = root.Camera.Camera;

            var allowed = Mathf.Max(heightFit, (grid.Width + CameraRig.Margin * 2f) * 0.5f / CameraRig.NarrowestAspect) + 0.01f;

            // UI-INT-06: the day is drawn in the café's illustrated opening, a window smaller than the band
            // between the old HUD bars. The same rule applies to that window: the camera is no further away
            // than the closest framing that still fits the room (and the head room) inside it.
            var hud = root.Ui.Get<DayHud>();
            if (hud != null && hud.IsKit && hud.Window != null && hud.Window.TryPixels(out var window))
            {
                var aspect = (float)camera.pixelWidth / camera.pixelHeight;
                var fw = window.width / camera.pixelWidth;
                var fh = window.height / camera.pixelHeight;
                allowed = Mathf.Max((top - bottom) / (2f * fh),
                    (grid.Width + 2f * CameraRig.SideRoom) / (2f * aspect * fw)) * 1.01f + 0.01f;
            }

            Assert.LessOrEqual(camera.orthographicSize, allowed,
                "the camera is further away than it was before the wall");
        }
    }
}

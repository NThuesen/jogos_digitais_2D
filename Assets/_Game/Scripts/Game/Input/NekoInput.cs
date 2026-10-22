using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace Neko.Game.Input
{
    /// <summary>
    /// Mouse and keyboard, as four events.
    /// </summary>
    /// <remarks>
    /// Reads the Input System devices directly rather than through an action asset. An action
    /// asset is a file, and this project builds everything in code; for four bindings the
    /// asset is more machinery than the thing it configures.
    ///
    /// Clicks that land on the interface are dropped here rather than in every handler, which
    /// is what stops pressing a button from also sending the manager across the room.
    /// </remarks>
    public sealed class NekoInput : MonoBehaviour
    {
        public event Action<Vector2> Click;
        public event Action<Vector2> RightClick;
        public event Action Pause;
        public event Action Rotate;
        public event Action ToggleFlow;

        public Vector2 Pointer { get; private set; }

        public bool Enabled { get; set; } = true;

        private void Update()
        {
            var mouse = Mouse.current;
            if (mouse != null) Pointer = mouse.position.ReadValue();

            var keyboard = Keyboard.current;
            if (keyboard != null)
            {
                if (keyboard.escapeKey.wasPressedThisFrame) Pause?.Invoke();
                if (keyboard.rKey.wasPressedThisFrame) Rotate?.Invoke();
                if (keyboard.tabKey.wasPressedThisFrame) ToggleFlow?.Invoke();
            }

            if (!Enabled || mouse == null || OverInterface()) return;

            if (mouse.leftButton.wasPressedThisFrame) Click?.Invoke(Pointer);
            if (mouse.rightButton.wasPressedThisFrame) RightClick?.Invoke(Pointer);
        }

        /// <summary>
        /// Fires a click as though the mouse had done it.
        /// </summary>
        /// <remarks>
        /// For the PlayMode test that proves a pointer cannot reach a running day. Driving a
        /// real device through the Input System in batch mode is a great deal of machinery to
        /// assert one thing about routing.
        /// </remarks>
        public void SendClickForTest(Vector2 at) => Click?.Invoke(at);

        private static bool OverInterface() =>
            EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
    }
}

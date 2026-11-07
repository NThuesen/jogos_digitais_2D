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
        }
    }
}

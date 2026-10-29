#if UNITY_EDITOR
using System;
using System.Collections;
using System.IO;
using System.Linq;
using Neko.Game.Ui;
using Neko.Game.View;
using Neko.Sim.Progress;
using Neko.Sim.Team;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Neko.Game.Core
{
    // Explicit, Editor-only evidence run. Never enabled in the player or ordinary boot.
    public sealed class SuzuCapture : MonoBehaviour
    {
        public static void Begin()
        {
            if (!Application.isBatchMode || !SaveBridge.Isolated)
                throw new InvalidOperationException("Capture requires isolated Editor batch mode.");
            EditorSceneManager.OpenScene("Assets/_Game/Scenes/Boot.unity");
            EditorApplication.isPlaying = true;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void StartCapture()
        {
            if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("NEKO_SUZU_CAPTURE_DIR"))) return;
            if (!Application.isBatchMode || !SaveBridge.Isolated)
                throw new InvalidOperationException("Refusing capture without save isolation.");
            new GameObject("Suzu evidence capture").AddComponent<SuzuCapture>().StartCoroutine(Run());
        }

        private static IEnumerator Run()
        {
            for (var i = 0; i < 12; i++) yield return null;
            var root = GameRoot.Instance;
            root.Ui.Get<MenuScreen>().OnNewGame.Invoke();
            root.Ui.Get<MenuScreen>().OnPlay.Invoke();
            var maid = root.Session.Profile.Maids.First(m => m.NaturalRole == Role.Floor);
            maid.Name = "Suzu";
            for (var i = 0; i < maid.Gear.Length; i++) maid.Gear[i] = null;
            root.Ui.Show<RosterScreen>();
            yield return null;
            GameObject.Find("Card Suzu").GetComponent<UnityEngine.UI.Button>().onClick.Invoke();
            yield return Shot("card", root.Camera.Camera);
            maid.Gear[(int)GearSlot.Accessory] = EquipmentCatalog.ById("acc_starry");
            root.Ui.RefreshCurrent();
            yield return Shot("card_accessory", root.Camera.Camera);
            root.Ui.Show<HubScreen>();
            root.Ui.Get<HubScreen>().OnOpenDay.Invoke();
            yield return new WaitForSeconds(1f);
            yield return Shot("restaurant_idle_accessory", root.Camera.Camera);
            var view = UnityEngine.Object.FindObjectsByType<MaidView>(FindObjectsInactive.Exclude)
                .First(v => v.Worker.Maid == maid);
            var deadline = Time.realtimeSinceStartup + 60f;
            while (view.transform.Find("Body").GetComponent<SpriteRenderer>().sprite.name != "Suzu_walk")
            {
                if (Time.realtimeSinceStartup > deadline)
                    throw new InvalidOperationException("No real walk observed within capture deadline.");
                yield return null;
            }
            yield return Shot("restaurant_movement", root.Camera.Camera);
            Debug.Log("SUZU_CAPTURE_COMPLETE: isolated save, real roster and shift; static poses only.");
            EditorApplication.Exit(0);
        }

        private static IEnumerator Shot(string name, Camera camera)
        {
            foreach (var size in new[] { new Vector2Int(1280, 720), new Vector2Int(960, 600) })
            {
                var target = new RenderTexture(size.x, size.y, 24);
                var texture = new Texture2D(size.x, size.y, TextureFormat.RGB24, false);
                var previous = RenderTexture.active;
                var previousTarget = camera.targetTexture;
                var previousAspect = camera.aspect;
                try
                {
                    camera.targetTexture = target;
                    camera.aspect = (float)size.x / size.y;
                    for (var i = 0; i < 12; i++) yield return null;
                    Canvas.ForceUpdateCanvases();
                    camera.Render();
                    RenderTexture.active = target;
                    texture.ReadPixels(new Rect(0, 0, size.x, size.y), 0, 0);
                    texture.Apply();
                    var dir = Environment.GetEnvironmentVariable("NEKO_SUZU_CAPTURE_DIR");
                    Directory.CreateDirectory(dir);
                    File.WriteAllBytes(Path.Combine(dir, name + "_" + size.x + "x" + size.y + ".png"), texture.EncodeToPNG());
                    foreach (var view in UnityEngine.Object.FindObjectsByType<MaidView>(FindObjectsInactive.Exclude))
                    {
                        var body = view.transform.Find("Body").GetComponent<SpriteRenderer>();
                        Debug.Log("SUZU_EVIDENCE " + name + " " + view.Worker.Name + " sprite=" + body.sprite.name + " position=" + view.transform.position);
                    }
                }
                finally
                {
                    camera.targetTexture = previousTarget;
                    camera.aspect = previousAspect;
                    RenderTexture.active = previous;
                    UnityEngine.Object.Destroy(texture);
                    UnityEngine.Object.Destroy(target);
                }
            }
        }
    }
}
#endif

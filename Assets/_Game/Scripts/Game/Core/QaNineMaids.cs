#if NEKO_QA_NINE
using System.Collections;
using System.Linq;
using Neko.Game.Art;
using Neko.Game.View;
using Neko.Game.Ui;
using Neko.Sim.Maids;
using Neko.Sim.Progress;
using Neko.Sim.Team;
using UnityEngine;

namespace Neko.Game.Core
{
    /// <summary>
    /// QA-only: starts a shift with all nine authored maids, in memory, so a browser build can be
    /// inspected without recruiting them. Compiled ONLY when the scripting define NEKO_QA_NINE is
    /// set, which only <c>WebGLBuilder.BuildQa</c> does (into Build/WebGL_QA); the itch.io build
    /// never contains it. It mirrors MaidGameplayTests.CaptureAllTogetherInTheBiggestRoom and
    /// changes no rule, economy value, RNG or saved format.
    /// </summary>
    public static class QaNineMaids
    {
        private static readonly string[] Ids =
            { "suzu", "hina", "akari", "momo", "reina", "kaede", "nene", "ichika", "yoru" };

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Boot()
        {
            var host = new GameObject("QaNineMaids");
            Object.DontDestroyOnLoad(host);
            host.AddComponent<Runner>().StartCoroutine(Run());
        }

        private sealed class Runner : MonoBehaviour { }

        private static IEnumerator Run()
        {
            while (GameRoot.Instance == null) yield return null;
            for (var i = 0; i < 12; i++) yield return null;

            var root = GameRoot.Instance;
            root.Ui.Get<MenuScreen>().OnNewGame.Invoke();
            yield return null;

            root.Ui.Get<DayHud>().OnSkip.Invoke();
            yield return null;
            root.Ui.Get<BooksScreen>().OnContinue.Invoke();

            var profile = root.Session.Profile;
            profile.Wallet.AddReputation(Neko.Sim.Economy.CafeLevelTable.ReputationForLevel(9));
            profile.ApplyUnlocks();

            var roles = new[] { Role.Reception, Role.Floor, Role.Kitchen, Role.Floor, Role.Kitchen,
                Role.Reception, Role.Floor, Role.Kitchen, Role.Floor };
            for (var n = profile.Maids.Count; n < Ids.Length; n++)
            {
                profile.Maids.Add(new Maid { Id = 900 + n, Name = "Fixture" + n, Archetype = Archetype.Genki,
                    NaturalRole = roles[n], Base = profile.Maids[0].Base, Rarity = profile.Maids[0].Rarity });
            }

            var floor = profile.Maids.FirstOrDefault(m => m.NaturalRole == Role.Floor);
            var pool = profile.Maids.OrderBy(m => m == floor ? 0 : 1).ToList();
            for (var i = 0; i < Ids.Length && i < pool.Count; i++)
            {
                var entry = MaidAnimCatalog.ById(Ids[i]);
                pool[i].Name = entry.name;
                pool[i].Archetype = entry.ArchetypeValue;
            }

            profile.Lineup.AutoFill(profile.Maids);
            root.Ui.Get<HubScreen>().OnOpenDay.Invoke();
        }
    }
}
#endif

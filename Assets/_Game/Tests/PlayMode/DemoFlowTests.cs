using System.Collections;
using System.Linq;
using Neko.Game.Core;
using Neko.Game.Ui;
using Neko.Game.View;
using Neko.Sim.Maids;
using Neko.Sim.Progress;
using Neko.Sim.Team;
using Neko.Tests.PlayMode.Layout;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace Neko.Tests.PlayMode
{
    /// <summary>
    /// The presentation path: Menu → Empresa → Loja → Elenco → Escalação, authored portraits only on
    /// the right maid, and the nine-maid showcase. Nothing here changes a rule or a save format.
    /// </summary>
    public sealed class DemoFlowTests
    {
        [TearDown]
        public void Restore()
        {
            SessionFactory.RestoreClock();
            SessionFactory.RestoreSave();
        }

        [UnityTest]
        public IEnumerator TheDemoPathWalksThroughEveryScreen()
        {
            var builder = SessionFactory.NewProfile(11u, 3).WithDay(1).WithMoney(500);
            builder.WithMaid("Hana", Rarity.Rare, 5, 1, TraitId.Lucky);
            yield return SessionFactory.Boot(builder.Build());

            var ui = GameRoot.Instance.Ui;
            Assert.IsInstanceOf<MenuScreen>(ui.Current);

            yield return SessionFactory.ToHub();
            Assert.IsInstanceOf<HubScreen>(ui.Current);

            yield return SessionFactory.ToShop();
            Assert.IsInstanceOf<ShopScreen>(ui.Current);
            yield return SessionFactory.BackToHub();

            yield return SessionFactory.ToRoster();
            Assert.IsInstanceOf<RosterScreen>(ui.Current);
            yield return SessionFactory.BackToHub();

            yield return SessionFactory.ToLineup();
            Assert.IsInstanceOf<LineupScreen>(ui.Current);
        }

        [UnityTest]
        public IEnumerator OnlyAnIdentifiedCharacterGetsHerAuthoredPortrait()
        {
            var builder = SessionFactory.NewProfile(12u, 3).WithDay(1).WithMoney(500);
            builder.WithMaid("Hana", Rarity.Rare, 5, 1, TraitId.Lucky);
            builder.WithMaid("Rei", Rarity.Common, 1, 0);
            yield return SessionFactory.Boot(builder.Build());
            yield return SessionFactory.ToHub();

            var profile = GameRoot.Instance.Session.Profile;
            var suzu = profile.Maids.First(m => m.Name == "Hana");
            suzu.Name = "Suzu";
            suzu.Archetype = Archetype.Genki;
            var impostor = profile.Maids.First(m => m.Name == "Rei");
            impostor.Name = "Suzu";                  // right name...
            impostor.Archetype = Archetype.Tsundere; // ...wrong archetype: Akari's, not hers
            suzu.Gear[0] = EquipmentCatalog.ById("head_sakura");

            Assert.IsNotNull(MaidAnimCatalog.Identify(suzu), "gear does not change who she is");
            Assert.IsNull(MaidAnimCatalog.Resolve(suzu), "the walking figure still falls back with gear");
            Assert.IsNull(MaidAnimCatalog.Identify(impostor), "name alone is never enough");

            yield return SessionFactory.ToRoster();
            yield return SessionFactory.Settle();

            var portraits = GameRoot.Instance.Ui.Get<RosterScreen>().GetComponentsInChildren<Image>(true)
                .Where(i => i.name == "Portrait" && i.transform.parent.name == "Window").ToList();
            Assert.IsTrue(portraits.Any(p => p.sprite != null && p.sprite.name == "suzu_portrait"),
                "Suzu is drawn from her own sheet");
            Assert.AreEqual(1, portraits.Where(p => p.sprite != null && p.sprite.name.EndsWith("_portrait"))
                .Select(p => p.sprite.name).Distinct().Count(), "nobody else is given an authored face");
        }

        [UnityTest]
        public IEnumerator TheShowcaseShowsTheNineAuthoredMaidsAndChangesNothing()
        {
            var builder = SessionFactory.NewProfile(13u, 1).WithDay(1).WithMoney(100);
            yield return SessionFactory.Boot(builder.Build());

            var profile = GameRoot.Instance.Session.Profile;
            var before = SessionFactory.Describe(profile);

            GameRoot.Instance.Ui.Show<ShowcaseScreen>();
            yield return SessionFactory.Settle();

            var screen = GameRoot.Instance.Ui.Get<ShowcaseScreen>();
            var walkers = screen.GetComponentsInChildren<Image>(true).Where(i => i.name.StartsWith("Walker ")).ToList();
            Assert.AreEqual(9, walkers.Count);
            foreach (var name in new[] { "Suzu", "Hina", "Akari", "Momo", "Reina", "Kaede", "Nene", "Ichika", "Yoru" })
            {
                Assert.IsTrue(walkers.Any(w => w.name == "Walker " + name), name);
                Assert.IsTrue(screen.GetComponentsInChildren<Text>(true).Any(t => t.text == name), name + " label");
            }

            Assert.AreEqual(before, SessionFactory.Describe(GameRoot.Instance.Session.Profile),
                "the showcase reads assets only");
        }
    }
}

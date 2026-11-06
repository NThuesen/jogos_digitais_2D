using System.Collections.Generic;
using Neko.Game.Art;
using Neko.Game.View;
using Neko.Sim.Cafe;
using Neko.Sim.Service;
using NUnit.Framework;
using UnityEngine;

namespace Neko.Tests.EditMode.Game
{
    public sealed class CustomerViewCacheTests
    {
        [Test]
        public void BubbleTracksNextDishAndRebindingEvenWithoutAStateChange()
        {
            var factory = new SpriteFactory();
            var first = new Recipe("a", "A", 1, 1, default(StationKind), 1, "Circle", "FF0000");
            var second = new Recipe("b", "B", 1, 1, default(StationKind), 1, "Star", "00FF00");
            var customer = new Customer { State = CustomerState.Waiting,
                Order = new Order(new List<Recipe> { first, second }) };
            var view = CustomerView.Create(null, factory, customer);
            try
            {
                var icon = view.transform.Find("Bubble/Icon").GetComponent<SpriteRenderer>();
                view.Sync();
                var original = icon.sprite;
                Assert.IsNotNull(original);
                view.Sync();
                Assert.AreSame(original, icon.sprite);
                customer.Order.Serve();
                view.Sync();
                Assert.AreNotSame(original, icon.sprite, "next dish changes while the customer remains Waiting");
                view.Bind(new Customer { State = CustomerState.Waiting,
                    Order = new Order(new List<Recipe> { first }) });
                view.Sync();
                Assert.AreSame(original, icon.sprite, "a reused view must invalidate its icon inputs");
            }
            finally
            {
                Object.DestroyImmediate(view.gameObject);
                factory.Clear();
            }
        }
    }
}

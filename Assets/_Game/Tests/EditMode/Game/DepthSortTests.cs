using Neko.Game.Art;
using Neko.Game.Core;
using Neko.Game.View;
using Neko.Sim.Cafe;
using NUnit.Framework;
using UnityEngine;

namespace Neko.Tests.Game
{
    /// <summary>
    /// Characters and solid furniture share one sorting band keyed by floor contact.
    /// </summary>
    public sealed class DepthSortTests
    {
        private static CafeGrid GridWith(string id, int x, int y)
        {
            var grid = new CafeGrid(13, 9);
            Assert.IsTrue(grid.Place(PlaceableCatalog.ById(id), x, y), "could not place " + id);
            return grid;
        }

        private static int Piece(CafeGrid grid, int x, int y) =>
            Layers.ActorOrder(DepthSort.SolidKey(grid.ItemAt(x, y)));

        [Test]
        public void ACharacterBehindASolidPieceDrawsBeneathItAtAnyY()
        {
            for (var y = 0; y <= 6; y++)
            {
                var grid = GridWith("plant", 5, y);
                var piece = Piece(grid, 5, y);

                // Clear of the cell: a tile and a bit behind, then at the back of the room.
                foreach (var feetY in new[] { y + 1.1f, y + 1.9f, 8.4f })
                {
                    if (feetY <= y + 0.5f) continue;
                    var behind = Layers.ActorOrder(DepthSort.ActorKey(grid, new Vector3(5f, feetY, 0f)));
                    Assert.Less(behind, piece, "feet at y=" + feetY + " should be behind a plant at row " + y);
                }
            }
        }

        [Test]
        public void ACharacterInFrontOfASolidPieceDrawsOverIt()
        {
            for (var y = 2; y <= 7; y++)
            {
                var grid = GridWith("plant", 5, y);
                var piece = Piece(grid, 5, y);

                var inFront = Layers.ActorOrder(DepthSort.ActorKey(grid, new Vector3(5f, y - 0.9f, 0f)));
                Assert.Greater(inFront, piece, "a character below row " + y + " should cover the plant");
            }
        }

        [Test]
        public void SomeoneStandingOnAPieceIsInFrontOfItButBehindWhoIsInFront()
        {
            var grid = GridWith("table_4", 4, 3);
            var table = Piece(grid, 4, 3);

            var back = Layers.ActorOrder(DepthSort.ActorKey(grid, new Vector3(4f, 4.42f, 0f)));
            var front = Layers.ActorOrder(DepthSort.ActorKey(grid, new Vector3(4f, 3.42f, 0f)));
            var below = Layers.ActorOrder(DepthSort.ActorKey(grid, new Vector3(4f, 1.8f, 0f)));

            Assert.Greater(back, table, "a customer seated at the table covers it");
            Assert.Greater(front, back, "the front seat of a two-row table covers the back seat");
            Assert.Greater(below, front, "somebody standing below the table covers a seated customer");
        }

        [Test]
        public void SomeoneCrossingThePondIsInFrontOfItsRocks()
        {
            var grid = GridWith("koi_pond", 4, 3);
            var rocks = Piece(grid, 4, 3);

            Assert.Greater(Layers.ActorOrder(DepthSort.ActorKey(grid, new Vector3(4.2f, 3.4f, 0f))), rocks);
        }

        [Test]
        public void TwoSolidPiecesKeepTheirOwnOrder()
        {
            var grid = GridWith("plant", 3, 5);
            Assert.IsTrue(grid.Place(PlaceableCatalog.ById("plant"), 3, 2));

            Assert.Greater(Piece(grid, 3, 2), Piece(grid, 3, 5), "the lower piece draws in front");
        }

        [Test]
        public void FlatPiecesAndTheDoorStayUnderCharacters()
        {
            Assert.IsFalse(DepthSort.IsSolid(PlaceableCatalog.ById("rug")));
            Assert.IsFalse(DepthSort.IsSolid(PlaceableCatalog.ById("koi_pond")));
            Assert.IsFalse(DepthSort.IsSolid(PlaceableCatalog.ById("door")));
            Assert.IsTrue(DepthSort.IsSolid(PlaceableCatalog.ById("stage")));

            var lowestActor = Layers.ActorOrder(Layers.ActorReach);
            Assert.Greater(lowestActor, Layers.Order(Layers.Furniture, -40f),
                "the door's band must stay below every character");
            Assert.Greater(lowestActor, Layers.Order(Layers.Rug, -40f));
        }

        [Test]
        public void TheActorBandStaysBetweenFurnitureAndInterface()
        {
            var highest = Layers.ActorOrder(-Layers.ActorReach, 1);
            Assert.Less(highest, Layers.Order(Layers.Items, 40f), "a plate must not climb above the items band");
            Assert.Less(highest, Layers.Order(Layers.Fx, 40f));
            Assert.Less(highest, Layers.InterfaceOrder);
        }

        [Test]
        public void ACarriedPlateSortsWithItsMaidNotAboveTheRoom()
        {
            var grid = GridWith("plant", 5, 4);
            var maidBehind = DepthSort.ActorKey(grid, new Vector3(5f, 5.5f, 0f));

            Assert.Less(Layers.ActorOrder(maidBehind, 1), Piece(grid, 5, 4),
                "her plate is hidden together with her");
            Assert.Greater(Layers.ActorOrder(maidBehind, 1), Layers.ActorOrder(maidBehind));
        }

        [Test]
        public void IllustratedEntriesMatchTheirFootprintOnly()
        {
            Assert.IsNotNull(IllustratedFurniture.For(PlaceableCatalog.ById("stage"), 2, 2));
            Assert.IsNotNull(IllustratedFurniture.For(PlaceableCatalog.ById("window"), 1, 1));
            Assert.IsNull(IllustratedFurniture.For(PlaceableCatalog.ById("window"), 1, 2));
            // ART-INT-02 gave table_2 pictures at 2x1 and 1x2; a footprint it has no picture for stays procedural.
            Assert.IsNull(IllustratedFurniture.For(PlaceableCatalog.ById("table_2"), 3, 1));
            Assert.IsNull(IllustratedFurniture.For(PlaceableCatalog.ById("plant"), 1, 1));

            var pond = IllustratedFurniture.For(PlaceableCatalog.ById("koi_pond"), 2, 2);
            Assert.IsTrue(pond.HasRaisedLayer);
        }

        [Test]
        public void IllustratedPngsExistAtTheCataloguedSize()
        {
            foreach (var id in new[] { "stage", "koi_pond", "window" })
            {
                var def = PlaceableCatalog.ById(id);
                var entry = IllustratedFurniture.For(def, def.Width, def.Height);
                Assert.IsNotNull(entry, id);

                foreach (var resource in new[] { entry.Resource, entry.RaisedResource })
                {
                    if (string.IsNullOrEmpty(resource)) continue;

                    var texture = Resources.Load<Texture2D>(resource);
                    Assert.IsNotNull(texture, resource + " is missing");
                    Assert.AreEqual(entry.PixelWidth, texture.width, resource);
                    Assert.AreEqual(entry.PixelHeight, texture.height, resource);
                }
            }
        }

        [Test]
        public void TheFactoryBuildsTheSpriteOnItsBottomCentreAndKeepsTheTexture()
        {
            var factory = new SpriteFactory();
            var entry = IllustratedFurniture.For(PlaceableCatalog.ById("koi_pond"), 2, 2);

            var whole = factory.Illustrated(entry, false);
            var raised = factory.Illustrated(entry, true);

            Assert.IsNotNull(whole);
            Assert.IsNotNull(raised);
            Assert.AreEqual(whole.bounds.size, raised.bounds.size, "base and raised must align");
            Assert.AreEqual(whole.pivot, raised.pivot);
            Assert.AreEqual(entry.WidthUnits, whole.bounds.size.x, 0.01f);
            Assert.AreEqual(0f, whole.bounds.min.y, 1e-4f, "pivot is the bottom of the art");

            var texture = whole.texture;
            factory.Clear();
            Assert.IsTrue(texture != null, "Clear must not destroy the importer's texture");
        }

        [Test]
        public void RoomViewDrawsTheRaisedPartAsASolidAndTheBaseFlat()
        {
            var grid = GridWith("koi_pond", 4, 3);
            var go = new GameObject("room");
            var factory = new SpriteFactory();
            try
            {
                var room = go.AddComponent<RoomView>();
                room.Bind(factory);
                room.Build(grid);

                var baseRenderer = go.transform.Find("koi_pondBase").GetComponent<SpriteRenderer>();
                var raised = go.transform.Find("koi_pondRaised").GetComponent<SpriteRenderer>();

                Assert.AreEqual(baseRenderer.transform.position, raised.transform.position);
                Assert.AreEqual(baseRenderer.sprite.bounds.size, raised.sprite.bounds.size);
                Assert.Less(baseRenderer.sortingOrder, Layers.ActorOrder(Layers.ActorReach),
                    "the water is under every character");
                Assert.AreEqual(Piece(grid, 4, 3), raised.sortingOrder);
                Assert.AreEqual(3 - 0.5f + 0.2f, raised.transform.position.y, 1e-4f, "sits on the front edge, centred by its lift");
            }
            finally
            {
                Object.DestroyImmediate(go);
                factory.Clear();
            }
        }
    }
}

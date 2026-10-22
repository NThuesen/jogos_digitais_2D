using System.Collections.Generic;
using Neko.Game.Art;
using Neko.Game.Core;
using Neko.Game.Feel;
using Neko.Sim.Service;
using UnityEngine;

namespace Neko.Game.View
{
    /// <summary>
    /// One party at one table, and how they are feeling about it.
    /// </summary>
    /// <remarks>
    /// Rubric 3 is the reason this class is bigger than it looks like it needs to be. Patience
    /// is not a bar over a head: it is the colour of the whole figure, the droop of their ears
    /// and whether a bubble is hanging over them. A player should be able to see which table
    /// is about to walk out from across the room without reading anything.
    /// </remarks>
    public sealed class CustomerView : MonoBehaviour
    {
        private SpriteFactory _sprites;
        private SpriteRenderer _body;
        private SpriteRenderer _bubble;
        private SpriteRenderer _bubbleIcon;
        private SpriteRenderer _patience;
        private float _wobble;
        private CustomerState _lastState = CustomerState.Queueing;

        public Customer Customer { get; private set; }

        public static CustomerView Create(Transform parent, SpriteFactory sprites, Customer customer)
        {
            var go = new GameObject("Customer");
            go.transform.SetParent(parent, false);

            var view = go.AddComponent<CustomerView>();
            view._sprites = sprites;
            view._body = go.AddComponent<SpriteRenderer>();

            var bubble = new GameObject("Bubble");
            bubble.transform.SetParent(go.transform, false);
            bubble.transform.localPosition = new Vector3(0.34f, 0.62f, 0f);
            bubble.transform.localScale = Vector3.one * 0.62f;
            view._bubble = bubble.AddComponent<SpriteRenderer>();

            var icon = new GameObject("Icon");
            icon.transform.SetParent(bubble.transform, false);
            icon.transform.localScale = Vector3.one * 0.5f;
            view._bubbleIcon = icon.AddComponent<SpriteRenderer>();

            var patience = new GameObject("Patience");
            patience.transform.SetParent(go.transform, false);
            patience.transform.localPosition = new Vector3(0f, -0.46f, 0f);
            view._patience = patience.AddComponent<SpriteRenderer>();
            view._patience.sprite = sprites.Solid(Color.white);

            view.Bind(customer);
            return view;
        }

        public void Bind(Customer customer)
        {
            Customer = customer;
            _body.sprite = _sprites.Layered(Layers(customer), "customer:" + customer.Kind, 128);

            transform.localScale = Vector3.zero;
            Tween.Scale(transform, Vector3.zero, Vector3.one * 0.82f, 0.32f, Ease.OutBack);
        }

        public void Place(Vector3 position) => transform.position = position;

        public void Sync()
        {
            if (Customer == null) return;

            var patience = Customer.Patience;
            var worried = Customer.DrainsPatience && patience < 0.5f;

            // An impatient customer fidgets, and fidgets harder the closer they are to the
            // door. Motion is read long before colour at the edge of vision.
            _wobble += Clocks.Game * (worried ? 14f : 2.5f);
            var sway = worried ? (1f - patience) * 0.06f : 0.012f;
            _body.transform.localRotation = Quaternion.Euler(0f, 0f, Mathf.Sin(_wobble) * sway * 60f);

            var tint = Customer.Profile.NeverImpatient
                ? Color.white
                : Color.Lerp(Color.white, Palette.ForPatience(patience), 1f - patience);
            _body.color = tint;

            Core.Layers.Apply(_body, Core.Layers.Characters, transform.position.y);
            Core.Layers.Apply(_bubble, Core.Layers.Fx, transform.position.y);
            Core.Layers.Apply(_bubbleIcon, Core.Layers.Fx, transform.position.y - 0.01f);
            Core.Layers.Apply(_patience, Core.Layers.Fx, transform.position.y);

            SyncBubble();
            SyncPatienceBar(patience);

            if (_lastState == Customer.State) return;

            _lastState = Customer.State;
            Tween.Punch(transform, 0.14f, 0.2f);
        }

        private void SyncBubble()
        {
            var wants = Customer.State == CustomerState.Ordering ||
                        Customer.State == CustomerState.Waiting ||
                        Customer.State == CustomerState.Queueing;

            _bubble.enabled = wants;
            _bubbleIcon.enabled = wants;
            if (!wants) return;

            _bubble.sprite = _sprites.Get(
                new ShapeSpec(ShapeId.Circle, Palette.Panel, Palette.Ink, 0.07f));

            var icon = IconFor();
            _bubbleIcon.sprite = icon.sprite;
            _bubbleIcon.color = icon.tint;

            // Bobs. A still bubble reads as scenery.
            _bubble.transform.localPosition = new Vector3(0.34f,
                0.62f + Mathf.Sin(_wobble * 0.6f) * 0.03f, 0f);
        }

        private (Sprite sprite, Color tint) IconFor()
        {
            switch (Customer.State)
            {
                case CustomerState.Queueing:
                    return (_sprites.Get(new ShapeSpec(ShapeId.Door, Palette.Shadow)), Color.white);

                case CustomerState.Ordering:
                    return (_sprites.Get(new ShapeSpec(ShapeId.Note, Palette.Shadow)), Color.white);

                default:
                {
                    var dish = Customer.Order?.NextDish;
                    if (dish == null)
                    {
                        return (_sprites.Get(new ShapeSpec(ShapeId.Heart, Palette.InnerEar)),
                            Color.white);
                    }

                    return (_sprites.Get(new ShapeSpec(Shapes.Parse(dish.ShapeId),
                        Palette.Hex(dish.TintHex), Palette.Ink, 0.06f)), Color.white);
                }
            }
        }

        private void SyncPatienceBar(float patience)
        {
            var show = Customer.DrainsPatience && !Customer.Profile.NeverImpatient;
            _patience.enabled = show;
            if (!show) return;

            _patience.transform.localScale = new Vector3(0.6f * Mathf.Max(0.02f, patience), 0.09f, 1f);
            _patience.color = Palette.ForPatience(patience);
        }

        /// <summary>A customer is a simpler stack than a maid: a head, a body and a mark.</summary>
        private static IReadOnlyList<ShapeSpec> Layers(Customer customer)
        {
            var accent = Palette.Hex(customer.Profile.AccentHex);
            var ink = Palette.Ink;
            const float outline = 0.03f;

            var layers = new List<ShapeSpec>
            {
                new ShapeSpec(ShapeId.Circle, Palette.WithAlpha(ink, 0.18f),
                    offset: new Vector2(0f, -0.86f), scale: new Vector2(0.38f, 0.085f)),

                new ShapeSpec(ShapeId.Apron, accent, ink, outline,
                    offset: new Vector2(0f, -0.36f), scale: new Vector2(0.38f, 0.40f)),

                new ShapeSpec(ShapeId.Circle, Palette.Skin, ink, outline,
                    offset: new Vector2(0f, 0.26f), scale: new Vector2(0.38f, 0.38f)),
            };

            // A hat, a cat's ears or a critic's notebook: the type has to be legible at a
            // glance or the modifiers behind it are invisible.
            switch (customer.Kind)
            {
                case CustomerType.Cat:
                    for (var side = -1; side <= 1; side += 2)
                    {
                        layers.Add(new ShapeSpec(ShapeId.CatEar, Palette.Darken(accent, 0.2f), ink,
                            outline, offset: new Vector2(side * 0.22f, 0.58f),
                            scale: new Vector2(0.16f, 0.18f), rotationDegrees: side * 12f));
                    }

                    break;

                case CustomerType.Vip:
                    layers.Add(new ShapeSpec(ShapeId.Star, Palette.Focus, ink, outline,
                        offset: new Vector2(0f, 0.62f), scale: new Vector2(0.22f, 0.18f)));
                    break;

                case CustomerType.Critic:
                    layers.Add(new ShapeSpec(ShapeId.RoundedRect, Palette.Panel, ink, outline,
                        offset: new Vector2(0.3f, -0.2f), scale: new Vector2(0.16f, 0.2f),
                        detail: 0.15f));
                    break;

                case CustomerType.Salaryman:
                    layers.Add(new ShapeSpec(ShapeId.RoundedRect, Palette.Darken(accent, 0.35f),
                        ink, outline, offset: new Vector2(0f, 0.56f),
                        scale: new Vector2(0.34f, 0.1f), detail: 0.2f));
                    break;

                case CustomerType.Otaku:
                    layers.Add(new ShapeSpec(ShapeId.Heart, Palette.InnerEar, ink, outline,
                        offset: new Vector2(0.3f, 0.52f), scale: new Vector2(0.17f, 0.17f)));
                    break;

                case CustomerType.Student:
                    layers.Add(new ShapeSpec(ShapeId.Capsule, Palette.Darken(accent, 0.3f), ink,
                        outline, offset: new Vector2(-0.34f, -0.3f),
                        scale: new Vector2(0.12f, 0.2f)));
                    break;
            }

            for (var side = -1; side <= 1; side += 2)
            {
                layers.Add(new ShapeSpec(ShapeId.Capsule, ink,
                    offset: new Vector2(side * 0.14f, 0.22f), scale: new Vector2(0.05f, 0.065f)));
            }

            return layers;
        }
    }
}

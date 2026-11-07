using System.Collections.Generic;
using Neko.Game.Art;
using Neko.Game.Core;
using Neko.Sim.Service;
using Neko.Sim.Shift;
using UnityEngine;

namespace Neko.Game.View
{
    /// <summary>
    /// What the owner can help with, drawn in the room: dirty tables and the kitchen gauge.
    /// </summary>
    /// <remarks>
    /// Everything here reads the simulation and changes nothing. A dirty table carries a pulsing
    /// plate so it can be spotted from across the room; a dish being cooked carries a gauge whose
    /// golden mark is the moment to tap the stove. Nothing is drawn for something that cannot be
    /// helped right now, so a quiet room stays quiet.
    /// </remarks>
    public sealed class AssistHints : MonoBehaviour
    {
        private sealed class Gauge
        {
            public GameObject Root;
            public SpriteRenderer Back;
            public SpriteRenderer Zone;
            public SpriteRenderer Needle;
            public SpriteRenderer Glow;
        }

        private const float GaugeWidth = 1.05f;
        private const float GaugeHeight = 0.17f;

        private SpriteFactory _sprites;
        private Sim.Shift.Shift _shift;
        private readonly Dictionary<int, SpriteRenderer> _mess = new Dictionary<int, SpriteRenderer>();
        private readonly Dictionary<int, Gauge> _gauges = new Dictionary<int, Gauge>();
        private readonly List<int> _stale = new List<int>();
        private float _clock;

        public void Bind(SpriteFactory sprites) => _sprites = sprites;

        public void Begin(Sim.Shift.Shift shift)
        {
            Clear();
            _shift = shift;
        }

        public void Clear()
        {
            foreach (var icon in _mess.Values)
            {
                if (icon != null) Destroy(icon.gameObject);
            }

            foreach (var gauge in _gauges.Values)
            {
                if (gauge.Root != null) Destroy(gauge.Root);
            }

            _mess.Clear();
            _gauges.Clear();
            _shift = null;
        }

        private void LateUpdate()
        {
            if (_shift == null || _sprites == null) return;

            _clock += Clocks.Game * 4f;
            SyncMess();
            SyncGauges();
        }

        private void SyncMess()
        {
            var seats = _shift.Seats;
            for (var i = 0; i < seats.Count; i++)
            {
                var seat = seats[i];
                _mess.TryGetValue(seat.Index, out var icon);

                if (!seat.Dirty || _shift.Finished)
                {
                    if (icon != null) icon.enabled = false;
                    continue;
                }

                if (icon == null)
                {
                    icon = NewIcon("Mess " + seat.Index,
                        _sprites.Get(new ShapeSpec(ShapeId.Plate, Palette.Panel, Palette.Ink, 0.12f)));
                    _mess[seat.Index] = icon;
                }

                icon.enabled = true;
                var bob = Mathf.Sin(_clock + seat.Index) * 0.05f;
                icon.transform.position = new Vector3(seat.Cell.X, seat.Cell.Y + 0.55f + bob, 0f);
                icon.transform.localScale = Vector3.one * (0.46f + 0.04f * Mathf.Sin(_clock * 1.4f));
                Core.Layers.Apply(icon, Core.Layers.Fx, seat.Cell.Y);
            }
        }

        private void SyncGauges()
        {
            var tickets = _shift.Tickets;
            _stale.Clear();
            foreach (var id in _gauges.Keys) _stale.Add(id);

            for (var i = 0; i < tickets.Count; i++)
            {
                var ticket = tickets[i];
                var progress = _shift.CookProgressOf(ticket);
                if (progress <= 0.01f || ticket.StationIndex < 0 || _shift.Finished) continue;

                _stale.Remove(ticket.Id);
                if (!_gauges.TryGetValue(ticket.Id, out var gauge))
                {
                    gauge = NewGauge();
                    _gauges[ticket.Id] = gauge;
                }

                Place(gauge, _shift.Stations[ticket.StationIndex].Cell, progress);
            }

            for (var i = 0; i < _stale.Count; i++)
            {
                if (_gauges.TryGetValue(_stale[i], out var gone) && gone.Root != null) Destroy(gone.Root);
                _gauges.Remove(_stale[i]);
            }
        }

        private void Place(Gauge gauge, Sim.Cafe.Cell cell, float progress)
        {
            var y = cell.Y + 0.95f;
            var left = cell.X - GaugeWidth * 0.5f;
            gauge.Root.transform.position = new Vector3(cell.X, y, 0f);

            var inZone = progress >= Sim.Shift.Shift.PerfectMin;
            var zoneMid = (Sim.Shift.Shift.PerfectMin + Sim.Shift.Shift.PerfectMax) * 0.5f;
            var zoneWidth = Sim.Shift.Shift.PerfectMax - Sim.Shift.Shift.PerfectMin;

            gauge.Zone.transform.position = new Vector3(left + zoneMid * GaugeWidth, y, 0f);
            gauge.Zone.transform.localScale = new Vector3(zoneWidth * GaugeWidth, GaugeHeight * 0.8f, 1f);
            gauge.Zone.color = inZone
                ? Color.Lerp(Palette.Focus, Color.white, 0.5f + 0.5f * Mathf.Sin(_clock * 3f))
                : Palette.Focus;

            gauge.Needle.transform.position = new Vector3(left + Mathf.Clamp01(progress) * GaugeWidth, y, 0f);
            gauge.Needle.transform.localScale = new Vector3(0.06f, GaugeHeight * 1.5f, 1f);

            gauge.Back.transform.position = new Vector3(cell.X, y, 0f);
            gauge.Back.transform.localScale = new Vector3(GaugeWidth + 0.1f, GaugeHeight + 0.1f, 1f);

            gauge.Glow.enabled = inZone;
            gauge.Glow.transform.position = new Vector3(cell.X, y + 0.34f, 0f);
            gauge.Glow.transform.localScale = Vector3.one * (0.34f + 0.05f * Mathf.Sin(_clock * 3f));

            Core.Layers.Apply(gauge.Back, Core.Layers.Fx, cell.Y);
            gauge.Zone.sortingOrder = gauge.Back.sortingOrder + 1;
            gauge.Needle.sortingOrder = gauge.Back.sortingOrder + 2;
            gauge.Glow.sortingOrder = gauge.Back.sortingOrder + 2;
        }

        private Gauge NewGauge()
        {
            var root = new GameObject("Gauge");
            root.transform.SetParent(transform, false);
            var gauge = new Gauge { Root = root };

            gauge.Back = Child(root.transform, "Back", _sprites.Solid(Palette.WithAlpha(Palette.Ink, 0.85f)));
            gauge.Zone = Child(root.transform, "Zone", _sprites.Solid(Palette.Focus));
            gauge.Needle = Child(root.transform, "Needle", _sprites.Solid(Color.white));
            gauge.Glow = Child(root.transform, "Glow",
                _sprites.Get(new ShapeSpec(ShapeId.Sparkle, Palette.Focus)));
            return gauge;
        }

        private static SpriteRenderer Child(Transform parent, string name, Sprite sprite)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var renderer = go.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            return renderer;
        }

        private SpriteRenderer NewIcon(string name, Sprite sprite)
        {
            var go = new GameObject(name);
            go.transform.SetParent(transform, false);
            var renderer = go.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            return renderer;
        }
    }
}

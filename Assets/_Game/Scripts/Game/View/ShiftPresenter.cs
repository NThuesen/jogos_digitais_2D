using System;
using System.Collections.Generic;
using Neko.Game.Art;
using Neko.Game.Audio;
using Neko.Game.Core;
using Neko.Game.Feel;
using Neko.Sim.Cafe;
using Neko.Sim.Service;
using Neko.Sim.Shift;
using UnityEngine;

namespace Neko.Game.View
{
    /// <summary>
    /// Runs one shift and keeps the screen in step with it.
    /// </summary>
    /// <remarks>
    /// Views are created and destroyed strictly from the simulation's own collections, so a
    /// leaked customer is a test failure rather than a ghost somebody notices three weeks
    /// later. Everything the player sees or hears comes out of the event queue, which is why
    /// a new kind of event cannot be added silently.
    /// </remarks>
    public sealed class ShiftPresenter : MonoBehaviour
    {
        private readonly Dictionary<int, CustomerView> _customers = new Dictionary<int, CustomerView>();
        private readonly Dictionary<int, MaidView> _workers = new Dictionary<int, MaidView>();
        private readonly List<int> _gone = new List<int>();

        private SpriteFactory _sprites;
        private RoomView _room;
        private Effects _effects;
        private AudioDirector _audio;
        private MusicDirector _music;
        private CameraRig _camera;
        private AssistHints _hints;
        private bool _rushMusic;

        /// <summary>The finish has been announced. Set once per day, cleared on the next.</summary>
        private bool _reported;

        public Sim.Shift.Shift Shift { get; private set; }

        public event Action<ShiftResult> Finished;

        public void Bind(SpriteFactory sprites, RoomView room, Effects effects,
            AudioDirector audio, MusicDirector music, CameraRig camera)
        {
            _sprites = sprites;
            _room = room;
            _effects = effects;
            _audio = audio;
            _music = music;
            _camera = camera;

            _hints = GetComponent<AssistHints>() ?? gameObject.AddComponent<AssistHints>();
            _hints.Bind(sprites);
        }

        public void Begin(Sim.Shift.Shift shift)
        {
            ClearViews();
            Shift = shift;
            _rushMusic = false;
            _reported = false;

            _hints?.Begin(shift);
            _room.Build(shift.Grid);
            _room.SetFlowOverlay(false);
            _music.Play(MusicBed.ShiftCalm);

            foreach (var worker in shift.Workers)
            {
                var maid = MaidView.Create(transform, _sprites, worker);
                maid.Grid = shift.Grid;
                _workers[worker.Id] = maid;
            }

            // A shift that could not start at all still has to report, or the player is left
            // staring at an empty room with no explanation.
            if (!shift.Finished) return;

            _reported = true;
            Finished?.Invoke(shift.Finish());
        }

        /// <summary>
        /// Advances the day and reports it when it ends.
        /// </summary>
        /// <remarks>
        /// The end is reported from a flag rather than from "the tick that finished it",
        /// because the skip button ticks the shift to its end outside this method — and with
        /// the old shape the next call returned at the first line, the event never fired and
        /// the game sat on a finished day forever. A hang with no error in it.
        /// </remarks>
        public void Tick()
        {
            if (Shift == null) return;

            if (!Shift.Finished)
            {
                Shift.Tick(Clocks.Game);
                SyncCustomers();
                SyncWorkers();
                SyncMusic();
            }

            DrainEvents();

            if (!Shift.Finished || _reported) return;

            _reported = true;
            _music.Play(MusicBed.Menu);
            _effects.Clear();
            Finished?.Invoke(Shift.Finish());
        }

        public Vector3 WorldOf(Cell cell) => new Vector3(cell.X, cell.Y, 0f);

        public CustomerView ViewOf(Customer customer) =>
            customer != null && _customers.TryGetValue(customer.Id, out var view) ? view : null;

        public MaidView ViewOf(Worker worker) =>
            worker != null && _workers.TryGetValue(worker.Id, out var view) ? view : null;

        // -------------------------------------------------------------------------------

        private void SyncCustomers()
        {
            foreach (var customer in Shift.Customers)
            {
                if (!_customers.TryGetValue(customer.Id, out var view))
                {
                    view = CustomerView.Create(transform, _sprites, customer);
                    view.Grid = Shift.Grid;
                    _customers[customer.Id] = view;
                }

                view.SeatCell = SeatCellOf(customer);
                view.Place(PositionOf(customer));
                view.Sync();
            }

            _gone.Clear();
            foreach (var pair in _customers)
            {
                if (Shift.CustomerById(pair.Key) == null) _gone.Add(pair.Key);
            }

            for (var i = 0; i < _gone.Count; i++)
            {
                var view = _customers[_gone[i]];
                _customers.Remove(_gone[i]);
                if (view != null) Destroy(view.gameObject);
            }
        }

        /// <summary>
        /// Where a party stands: at their seat, or in the queue just inside the door.
        /// </summary>
        /// <remarks>
        /// The queue runs down the wall beside the door in columns of four. The first version
        /// walked straight out to the left with less than half a tile between parties, which
        /// at a door capacity of five drew five customers on top of each other and off the
        /// edge of the floor — the capture made it obvious and nothing else would have.
        /// </remarks>
        private Cell? SeatCellOf(Customer customer) =>
            customer.SeatIndex >= 0 && customer.SeatIndex < Shift.Seats.Count
                ? Shift.Seats[customer.SeatIndex].Cell
                : (Cell?)null;

        private Vector2 SeatOffsetAt(Cell cell)
        {
            var item = Shift.Grid.ItemAt(cell.X, cell.Y);
            var entry = item != null ? IllustratedFurniture.For(item.Def, item.Width, item.Height) : null;
            return _sprites.DrawsIllustrated(entry) ? entry.SeatOffset(cell.X - item.X, cell.Y - item.Y) : Vector2.zero;
        }

        private Vector3 PositionOf(Customer customer)
        {
            if (customer.SeatIndex >= 0 && customer.SeatIndex < Shift.Seats.Count)
            {
                var cell = Shift.Seats[customer.SeatIndex].Cell;

                // Drawn at the art's chair when the piece has illustrated chairs; the cell is
                // where the simulation seats them and does not move.
                var offset = SeatOffsetAt(cell);
                return new Vector3(cell.X + offset.x, cell.Y + 0.42f + offset.y, 0f);
            }

            var place = 0;
            foreach (var other in Shift.Customers)
            {
                if (other.Id == customer.Id) break;
                if (other.State == CustomerState.Queueing) place++;
            }

            var door = Shift.Grid.Door;
            var column = place / QueueColumnLength;
            var row = place % QueueColumnLength;

            return new Vector3(
                door.X + 0.2f + column * 0.72f,
                Mathf.Clamp(door.Y + 1.3f - row * 0.85f, 0.2f, Shift.Grid.Height - 0.8f),
                0f);
        }

        /// <summary>How many wait in one line before the queue starts a second.</summary>
        private const int QueueColumnLength = 4;

        private void SyncWorkers()
        {
            foreach (var worker in Shift.Workers)
            {
                if (_workers.TryGetValue(worker.Id, out var view)) view.Sync();
            }
        }

        private void SyncMusic()
        {
            var busy = Shift.Occupancy >= 0.7f || Shift.TimeRemaining <= 20f;
            if (busy == _rushMusic) return;

            _rushMusic = busy;
            _music.Play(busy ? MusicBed.ShiftRush : MusicBed.ShiftCalm);
        }

        /// <summary>
        /// Plays every event the simulation has queued up.
        /// </summary>
        /// <remarks>
        /// Public because skipping a day ticks the whole thing through in one frame and then
        /// needs the sound and the money popups to land, rather than leaving several hundred
        /// events in a queue that the next day would play.
        /// </remarks>
        public void Drain() => DrainEvents();

        /// <summary>
        /// Throws the queue away without playing it.
        /// </summary>
        /// <remarks>
        /// For a skipped day. Handle and the audio director only make flourish — no event
        /// carries bookkeeping — so a day nobody watched loses nothing by not being heard.
        /// Playing it was actively wrong: a whole day's payouts land in one frame, where no
        /// popup can age out of the way of the next, and the frame that would have shown
        /// them is the frame the dashboard replaces anyway.
        /// </remarks>
        public void Discard()
        {
            Shift.Events.Clear();
            _effects.Clear();
        }

        /// <summary>Where a customer is drawn right now, or the door when it has no view yet.</summary>
        public Vector2 WorldPositionOf(Customer customer)
        {
            if (customer != null && _customers.TryGetValue(customer.Id, out var view) && view != null)
            {
                return view.transform.position;
            }

            return new Vector2(0f, 0f);
        }

        /// <summary>
        /// The owner's tap on the floor, in world coordinates. Picks the nearest thing that can
        /// be helped with: the kitchen gauge, a party, or a dirty table.
        /// </summary>
        public AssistResult HandleClick(Vector2 world)
        {
            if (Shift == null || Shift.Finished) return AssistResult.Rejected;

            const float reach = 0.85f;
            var bestDistance = float.MaxValue;
            Func<AssistResult> act = null;
            var origin = new Vector3(world.x, world.y, 0f);

            foreach (var ticket in Shift.Tickets)
            {
                if (ticket.StationIndex < 0 || Shift.CookProgressOf(ticket) <= 0.01f) continue;

                var cell = Shift.Stations[ticket.StationIndex].Cell;
                // The stove wins ties: its gauge is the only thing here that is on a clock.
                var d = Vector2.Distance(world, new Vector2(cell.X, cell.Y + 0.4f)) - 0.35f;
                if (d >= reach || d >= bestDistance) continue;

                bestDistance = d;
                var id = ticket.Id;
                act = () => Shift.TryPerfectCook(id);
            }

            foreach (var pair in _customers)
            {
                var view = pair.Value;
                if (view == null || view.Customer == null) continue;

                var d = Vector2.Distance(world, new Vector2(view.transform.position.x, view.transform.position.y));
                if (d >= reach || d >= bestDistance) continue;

                bestDistance = d;
                var customer = view.Customer;
                switch (customer.State)
                {
                    case CustomerState.Queueing:
                        act = () => Shift.TryGreet(customer.Id);
                        break;
                    case CustomerState.Ordering:
                        act = () => Shift.TryTakeOrder(customer.Id);
                        break;
                    default:
                        act = () => Shift.TryPamper(customer.Id);
                        break;
                }
            }

            foreach (var seat in Shift.Seats)
            {
                if (!seat.Dirty) continue;

                var d = Vector2.Distance(world, new Vector2(seat.Cell.X, seat.Cell.Y + 0.35f));
                if (d >= reach || d >= bestDistance) continue;

                bestDistance = d;
                var index = seat.Index;
                act = () => Shift.TryClean(index);
            }

            if (act == null) return AssistResult.Rejected;

            var result = act();
            switch (result)
            {
                case AssistResult.Cooldown:
                    _effects.Number(origin, Ui.Strings.Get("assist.busy"), Palette.TextDim);
                    break;
                case AssistResult.TooLate:
                    _effects.Number(origin, Ui.Strings.Get("assist.late"), Palette.TextDim);
                    break;
                case AssistResult.NoSeat:
                    _effects.Number(origin, Ui.Strings.Get("assist.noseat"), Palette.Alarm);
                    break;
                case AssistResult.Done:
                    if (Shift.Combo >= 2) _effects.Number(origin + Vector3.up * 0.4f, "x" + Shift.Combo, Palette.Focus);
                    break;
            }

            return result;
        }

        private void DrainEvents()
        {
            while (Shift.Events.Count > 0)
            {
                var e = Shift.Events.Dequeue();
                var at = new Vector3(e.X, e.Y, 0f);

                _audio.Handle(e);
                Handle(e, at);
            }
        }

        private void Handle(ShiftEvent e, Vector3 at)
        {
            switch (e.Kind)
            {
                case ShiftEventKind.CustomerArrived:
                    _effects.Sparkles(at, 3);
                    break;

                case ShiftEventKind.TurnedAway:
                    _effects.Anger(at);
                    _effects.Number(at, Ui.Strings.Get("shift.full"), Palette.Alarm);
                    break;

                case ShiftEventKind.Seated:
                    _effects.Hearts(at, 2);
                    break;

                case ShiftEventKind.Ordered:
                    _effects.Note(at);
                    break;

                case ShiftEventKind.CookStarted:
                    _effects.Steam(at);
                    break;

                case ShiftEventKind.Fumbled:
                    _effects.Anger(at);
                    _camera.Shake(0.05f, 0.15f);
                    break;

                case ShiftEventKind.PlateReady:
                    _effects.Steam(at);
                    _effects.Sparkles(at, 4);
                    break;

                case ShiftEventKind.PlateDropped:
                    _effects.Hearts(at, 4);
                    _camera.Shake(0.09f, 0.22f);
                    break;

                case ShiftEventKind.WrongOrder:
                    // The loudest thing on the floor, because it is the loudest line on the
                    // dashboard and the player should connect the two.
                    _effects.Anger(at);
                    _effects.Number(at, Ui.Strings.Get("day.wrong"), Palette.Alarm);
                    _camera.Shake(0.11f, 0.26f);
                    break;

                case ShiftEventKind.Served:
                    _effects.Hearts(at, 2);
                    break;

                case ShiftEventKind.Paid:
                    _effects.Coins(at, 4);
                    _effects.Money(at, e.Amount);
                    break;

                case ShiftEventKind.LeftHappy:
                    _effects.Hearts(at, 3);
                    break;

                case ShiftEventKind.LeftAngry:
                    _effects.Anger(at);
                    _camera.Shake(0.07f, 0.2f);
                    break;

                case ShiftEventKind.Bussed:
                    _effects.Sparkles(at, 3);
                    break;

                case ShiftEventKind.MoodEmpty:
                    _effects.Number(at, "zzz", Palette.TextDim);
                    break;

                case ShiftEventKind.SecondWind:
                case ShiftEventKind.BackFromBreak:
                    _effects.Sparkles(at, 6);
                    break;

                case ShiftEventKind.EncoreUsed:
                    _effects.Note(at);
                    _effects.Hearts(at, 6);
                    _camera.Shake(0.05f, 0.3f);
                    break;

                case ShiftEventKind.Pampered:
                    _effects.Hearts(at, 5);
                    _effects.Number(at, Ui.Strings.Get("assist.pampered"), Palette.InnerEar);
                    break;

                case ShiftEventKind.PerfectDish:
                    _effects.Sparkles(at, 8);
                    _effects.Number(at, Ui.Strings.Get("assist.perfect"), Palette.Focus);
                    break;

                case ShiftEventKind.TapMissed:
                    _effects.Number(at, Ui.Strings.Get("assist.early"), Palette.Alarm);
                    _camera.Shake(0.04f, 0.12f);
                    break;

                case ShiftEventKind.TicketQueued:
                    break;

                case ShiftEventKind.ClosingTime:
                case ShiftEventKind.ShiftEnded:
                    _effects.Sparkles(at, 5);
                    break;
            }
        }

        private void ClearViews()
        {
            _hints?.Clear();
            foreach (var view in _customers.Values)
            {
                if (view != null) Destroy(view.gameObject);
            }

            foreach (var view in _workers.Values)
            {
                if (view != null) Destroy(view.gameObject);
            }

            _customers.Clear();
            _workers.Clear();
        }

        private void OnDestroy() => ClearViews();
    }
}

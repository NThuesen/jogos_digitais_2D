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
        }

        public void Begin(Sim.Shift.Shift shift)
        {
            ClearViews();
            Shift = shift;
            _rushMusic = false;
            _reported = false;

            _room.Build(shift.Grid);
            _room.SetFlowOverlay(false);
            _music.Play(MusicBed.ShiftCalm);

            foreach (var worker in shift.Workers)
            {
                _workers[worker.Id] = MaidView.Create(transform, _sprites, worker);
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
                    _customers[customer.Id] = view;
                }

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
        private Vector3 PositionOf(Customer customer)
        {
            if (customer.SeatIndex >= 0 && customer.SeatIndex < Shift.Seats.Count)
            {
                var cell = Shift.Seats[customer.SeatIndex].Cell;
                return new Vector3(cell.X, cell.Y + 0.42f, 0f);
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

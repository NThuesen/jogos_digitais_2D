using System;
using Neko.Sim.Cafe;
using Neko.Sim.Core;
using Neko.Sim.Service;

namespace Neko.Sim.Shift
{
    /// <summary>What happened to a tap the player made on the floor.</summary>
    public enum AssistResult
    {
        /// <summary>Nothing there to help with (or the day is over).</summary>
        Rejected = 0,

        /// <summary>It worked.</summary>
        Done,

        /// <summary>Tapped the kitchen gauge before the golden mark: the dish slows down.</summary>
        TooEarly,

        /// <summary>Tapped after the golden mark: the dish is about to finish by itself, no bonus.</summary>
        TooLate,

        /// <summary>The same help is still recharging, or the owner's hands are busy.</summary>
        Cooldown,

        /// <summary>A queued party could be seated but there is no free table.</summary>
        NoSeat,
    }

    /// <summary>
    /// The owner's hands on the floor: the things a manager can do while the team works.
    /// </summary>
    /// <remarks>
    /// The lineup still decides how the day goes. These taps only ever add: a day played
    /// without touching anything is exactly the day it always was, so the economy keeps its
    /// baseline and the skip button keeps its meaning. A player who pays attention takes over
    /// the small jobs a maid would do (seat a party, take an order, clear a table), stops a dish
    /// on its golden mark so it is ready early and never burns, and calms a table down. A run of
    /// helpful taps builds a combo that raises tips; a mistimed tap breaks it.
    /// </remarks>
    public sealed partial class Shift
    {
        /// <summary>Where the kitchen gauge's golden mark starts and ends, as a share of the cook.</summary>
        public const float PerfectMin = 0.72f;
        public const float PerfectMax = 0.95f;

        /// <summary>A perfect dish makes the party spend this much more.</summary>
        public const float PerfectSpendBonus = 1.10f;

        /// <summary>What a mistimed tap takes off the dish's progress.</summary>
        public const float EarlyTapPenalty = 0.12f;

        /// <summary>
        /// How long one helpful job keeps the owner's hands busy. A person cannot do everything
        /// at once, and without this a click-happy player would replace the whole team.
        /// </summary>
        public const float OwnerActionSeconds = 2.0f;

        public const float PamperPatienceGain = 0.20f;
        public const float PamperCooldownSeconds = 10f;

        /// <summary>How long a combo survives without another helpful tap.</summary>
        public const float ComboWindowSeconds = 6f;

        public const int ComboCap = 10;
        public const float ComboTipPerStep = 0.01f;

        private float _comboTimer;

        public int Assists { get; private set; }
        public int PerfectDishes { get; private set; }
        public int Pampers { get; private set; }

        /// <summary>Helpful taps in a row. Falls to zero after a mistake or a quiet spell.</summary>
        public int Combo { get; private set; }

        public int BestCombo { get; private set; }

        public float PamperCooldownRemaining { get; private set; }

        /// <summary>Seconds until the owner can do another job (the golden-mark tap is exempt).</summary>
        public float OwnerBusyRemaining { get; private set; }

        /// <summary>Extra tip rate the current combo earns.</summary>
        public float ComboTipBonus => Math.Min(Combo, ComboCap) * ComboTipPerStep;

        private void AdvanceAssist(float dt)
        {
            if (PamperCooldownRemaining > 0f)
            {
                PamperCooldownRemaining = Math.Max(0f, PamperCooldownRemaining - dt);
            }

            if (OwnerBusyRemaining > 0f) OwnerBusyRemaining = Math.Max(0f, OwnerBusyRemaining - dt);

            if (Combo <= 0) return;

            _comboTimer -= dt;
            if (_comboTimer <= 0f) Combo = 0;
        }

        private void Helped(bool busy = true)
        {
            if (busy) OwnerBusyRemaining = OwnerActionSeconds;
            Assists++;
            // The combo stops at the cap, so what the screen shows is what the tips pay.
            Combo = Math.Min(Combo + 1, ComboCap);
            if (Combo > BestCombo) BestCombo = Combo;
            _comboTimer = ComboWindowSeconds;
        }

        private void ReleaseClaimsOn(TaskKind kind, int targetId)
        {
            for (var i = 0; i < _workers.Count; i++)
            {
                var claimed = _workers[i].Claimed;
                if (claimed != null && claimed.Kind == kind && claimed.TargetId == targetId)
                {
                    ReleaseWorker(_workers[i]);
                }
            }
        }

        /// <summary>Seats a party waiting at the door, as a maid on Reception would.</summary>
        public AssistResult TryGreet(int customerId)
        {
            if (Finished) return AssistResult.Rejected;

            var customer = CustomerById(customerId);
            if (customer == null || customer.State != CustomerState.Queueing) return AssistResult.Rejected;
            if (OwnerBusyRemaining > 0f) return AssistResult.Cooldown;

            var seatIndex = FindFreeSeat();
            if (seatIndex < 0) return AssistResult.NoSeat;

            var seat = _seats[seatIndex];
            seat.OccupantId = customer.Id;
            customer.SeatIndex = seatIndex;
            customer.State = CustomerState.Seating;
            customer.StateTimer = SeatingSeconds;
            ReleaseClaimsOn(TaskKind.Greet, customerId);

            Emit(ShiftEventKind.Seated, customer.Id, seat.Cell);
            Helped();
            return AssistResult.Done;
        }

        /// <summary>Takes the order of a table with a hand up.</summary>
        public AssistResult TryTakeOrder(int customerId)
        {
            if (Finished) return AssistResult.Rejected;

            var customer = CustomerById(customerId);
            if (customer == null || customer.State != CustomerState.Ordering) return AssistResult.Rejected;
            if (OwnerBusyRemaining > 0f) return AssistResult.Cooldown;

            CompleteTakeOrder(new ShiftTask(0, TaskKind.TakeOrder, customerId));
            ReleaseClaimsOn(TaskKind.TakeOrder, customerId);
            Helped();
            return AssistResult.Done;
        }

        /// <summary>Clears a dirty table so the next party can sit.</summary>
        public AssistResult TryClean(int seatIndex)
        {
            if (Finished || seatIndex < 0 || seatIndex >= _seats.Count || !_seats[seatIndex].Dirty)
            {
                return AssistResult.Rejected;
            }

            if (OwnerBusyRemaining > 0f) return AssistResult.Cooldown;

            _seats[seatIndex].Dirty = false;
            ReleaseClaimsOn(TaskKind.Bus, seatIndex);
            Emit(ShiftEventKind.Bussed, seatIndex, _seats[seatIndex].Cell);
            Helped();
            return AssistResult.Done;
        }

        /// <summary>Calms a waiting customer down: a free refill, a kind word. Recharges.</summary>
        public AssistResult TryPamper(int customerId)
        {
            if (Finished) return AssistResult.Rejected;

            var customer = CustomerById(customerId);
            if (customer == null || !customer.DrainsPatience || customer.Profile.NeverImpatient ||
                customer.Patience >= 0.95f)
            {
                return AssistResult.Rejected;
            }

            if (PamperCooldownRemaining > 0f || OwnerBusyRemaining > 0f) return AssistResult.Cooldown;

            customer.Patience = Curves.Clamp01(customer.Patience + PamperPatienceGain);
            PamperCooldownRemaining = PamperCooldownSeconds;
            Pampers++;
            Emit(ShiftEventKind.Pampered, customer.Id, SeatCellOf(customer));
            Helped();
            return AssistResult.Done;
        }

        /// <summary>The share of the cook a dish has done (0 to 1), or -1 when nobody is cooking it.</summary>
        public float CookProgressOf(Ticket ticket)
        {
            var worker = CookOf(ticket);
            return worker == null ? -1f : Math.Min(1f, worker.Progress);
        }

        private Worker CookOf(Ticket ticket)
        {
            if (ticket == null || ticket.State != TicketState.Cooking) return null;

            for (var i = 0; i < _workers.Count; i++)
            {
                var claimed = _workers[i].Claimed;
                if (claimed != null && claimed.Kind == TaskKind.Cook && claimed.TargetId == ticket.Id)
                {
                    return _workers[i];
                }
            }

            return null;
        }

        /// <summary>
        /// Stops a dish on the golden mark: ready now, never burnt, and the party spends a bit more.
        /// </summary>
        public AssistResult TryPerfectCook(int ticketId)
        {
            if (Finished) return AssistResult.Rejected;

            var ticket = TicketById(ticketId);
            var worker = CookOf(ticket);

            // Nobody is at the stove yet: there is nothing to time.
            if (worker == null || worker.Progress <= 0.01f) return AssistResult.Rejected;

            var progress = worker.Progress;
            var station = ticket.StationIndex >= 0 ? _stations[ticket.StationIndex].Cell : Grid.Door;

            // After the mark the dish finishes on its own a moment later: nothing to gain, and
            // nothing to lose, so the combo is left alone.
            if (progress > PerfectMax) return AssistResult.TooLate;

            if (progress < PerfectMin)
            {
                worker.Progress = Math.Max(0f, progress - EarlyTapPenalty);
                Combo = 0;
                Emit(ShiftEventKind.TapMissed, ticket.Id, station);
                return AssistResult.TooEarly;
            }

            var task = worker.Claimed;
            DishesCooked++;
            PerfectDishes++;
            ticket.State = TicketState.Ready;

            var customer = CustomerById(ticket.CustomerId);
            if (customer != null) customer.SpendBonus *= PerfectSpendBonus;

            AwardXp(worker, TaskKind.Cook);
            _board.Complete(task);
            worker.Drop();

            Emit(ShiftEventKind.PerfectDish, ticket.Id, station);
            Emit(ShiftEventKind.PlateReady, ticket.Id, station);
            Helped(busy: false);
            return AssistResult.Done;
        }
    }
}

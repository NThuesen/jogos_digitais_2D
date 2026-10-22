using System;
using System.Collections.Generic;
using Neko.Sim.Cafe;
using Neko.Sim.Core;
using Neko.Sim.Maids;
using Neko.Sim.Progress;
using Neko.Sim.Service;

namespace Neko.Sim.Shift
{
    /// <summary>
    /// One day of the café, as a deterministic tick.
    /// </summary>
    /// <remarks>
    /// No <c>UnityEngine</c> anywhere in here: a shift is a function of its setup and its
    /// seed, which is what lets the balance tests run two hundred of them in a unit test and
    /// compare the outcomes. Everything random goes through <see cref="Rng"/> for the same
    /// reason.
    /// </remarks>
    public sealed class Shift : ITaskContext
    {
        public const float GreetSeconds = 1.6f;
        public const float TakeOrderSeconds = 2.0f;
        public const float DeliverSeconds = 1.0f;
        public const float BusSeconds = 2.4f;

        public const float SeatingSeconds = 0.8f;
        public const float ThinkingSeconds = 2.2f;
        public const float EatSecondsPerDish = 4.5f;
        public const float LeavingSeconds = 1.2f;

        /// <summary>How far apart two maids still count as working together.</summary>
        public const float NeighbourRadius = 2f;

        public const float IdleMoodRecoveryPerSecond = 0.02f;

        /// <summary>What the Presença skill gives back per second, to everyone nearby.</summary>
        public const float PatienceRegenPerSecond = 0.03f;

        /// <summary>How far Presença reaches. Wider than the Onee-san aura: it is the skill.</summary>
        public const float PresenceRadius = 3.5f;

        /// <summary>Dojikko's apology: every dropped plate buys the room a little goodwill.</summary>
        public const float DropPatienceGift = 0.08f;

        private static readonly Dictionary<TaskKind, int> XpForTask = new Dictionary<TaskKind, int>
        {
            { TaskKind.Greet, 3 },
            { TaskKind.TakeOrder, 3 },
            { TaskKind.Cook, 6 },
            { TaskKind.Deliver, 5 },
            { TaskKind.Bus, 2 },
        };

        private readonly ShiftSetup _setup;
        private readonly Rng _rng;
        private readonly TaskBoard _board = new TaskBoard();

        private readonly List<Customer> _customers = new List<Customer>();
        private readonly List<Worker> _workers = new List<Worker>();
        private readonly List<Seat> _seats = new List<Seat>();
        private readonly List<Ticket> _tickets = new List<Ticket>();
        private readonly List<StationSpot> _stations = new List<StationSpot>();

        /// <summary>Unlocked dishes this room owns a station for. Nobody may order outside it.</summary>
        private readonly IReadOnlyList<Recipe> _servableMenu;

        /// <summary>Ticket id occupying each station, or -1. One dish per station at a time.</summary>
        private readonly int[] _stationTicket;

        private readonly Queue<ShiftEvent> _events = new Queue<ShiftEvent>();

        private float _elapsed;
        private float _arrivalAccumulator;
        private int _scriptedIndex;
        private int _nextCustomerId = 1;
        private int _nextTicketId = 1;
        private float _graceRemaining;
        private int _reputation;
        private ShiftResult _result;

        public Shift(ShiftSetup setup)
        {
            _setup = setup ?? throw new ArgumentNullException(nameof(setup));
            SkillTable.Install();
            _rng = new Rng(setup.Seed);
            Grid = setup.Grid ?? new CafeGrid(7, 5);
            TimeRemaining = setup.DurationSeconds;
            _graceRemaining = setup.ClosingGraceSeconds;

            var cells = Grid.SeatPositions();
            for (var i = 0; i < cells.Count; i++) _seats.Add(new Seat(i, cells[i]));

            _stations.AddRange(Grid.Stations());
            _stationTicket = new int[_stations.Count];
            for (var i = 0; i < _stationTicket.Length; i++) _stationTicket[i] = -1;

            Metrics = Grid.Metrics();
            _servableMenu = BuildServableMenu();
            BuildWorkers();

            var impossible = WhyThisCafeCannotServe();
            if (impossible != null) Finish(impossible);
        }

        /// <summary>
        /// The unlocked dishes this room can actually produce, given the stations standing in it.
        /// </summary>
        private IReadOnlyList<Recipe> BuildServableMenu()
        {
            var menu = new List<Recipe>();
            var unlocked = RecipeBook.UnlockedAt(_setup.CafeLevel);

            for (var r = 0; r < unlocked.Count; r++)
            {
                for (var s = 0; s < _stations.Count; s++)
                {
                    if (_stations[s].Kind != unlocked[r].Station) continue;
                    menu.Add(unlocked[r]);
                    break;
                }
            }

            return menu;
        }

        public CafeGrid Grid { get; }

        public CafeMetrics Metrics { get; private set; }

        public bool Finished { get; private set; }

        public bool ClosingTime { get; private set; }

        public float TimeRemaining { get; private set; }

        public float Elapsed => _elapsed;

        public int Earned { get; private set; }

        public int Served { get; private set; }

        public int Angry { get; private set; }

        public int TurnedAway { get; private set; }

        public int Quota => _setup.Quota;

        public int Day => _setup.Day;

        public IReadOnlyList<Customer> Customers => _customers;

        public IReadOnlyList<Worker> Workers => _workers;

        public IReadOnlyList<Seat> Seats => _seats;

        public IReadOnlyList<Ticket> Tickets => _tickets;

        public IReadOnlyList<StationSpot> Stations => _stations;

        public IReadOnlyList<ShiftTask> OpenTasks => _board.Open;

        public Queue<ShiftEvent> Events => _events;

        public Worker Manager { get; private set; }

        public float Occupancy
        {
            get
            {
                if (_seats.Count == 0) return 0f;

                var taken = 0;
                for (var i = 0; i < _seats.Count; i++)
                {
                    if (_seats[i].OccupantId >= 0) taken++;
                }

                return taken / (float)_seats.Count;
            }
        }

        public int QueueLength
        {
            get
            {
                var count = 0;
                for (var i = 0; i < _customers.Count; i++)
                {
                    if (_customers[i].State == CustomerState.Queueing) count++;
                }

                return count;
            }
        }

        // -------------------------------------------------------------------------------
        // The tick
        // -------------------------------------------------------------------------------

        public void Tick(float dt)
        {
            if (Finished || dt <= 0f) return;

            _elapsed += dt;
            AdvanceClock(dt);
            if (!ClosingTime) SpawnArrivals(dt);
            AdvanceCustomers(dt);
            RebuildTasks();
            AdvanceWorkers(dt);
            RemoveFinishedCustomers();

            if (ClosingTime && (_customers.Count == 0 || _graceRemaining <= 0f)) Finish(null);
        }

        private void AdvanceClock(float dt)
        {
            if (ClosingTime)
            {
                _graceRemaining -= dt;
                return;
            }

            TimeRemaining -= dt;
            if (TimeRemaining > 0f) return;

            TimeRemaining = 0f;
            ClosingTime = true;
            Emit(ShiftEventKind.ClosingTime, 0, Grid.Door);

            // Whoever is still at the door at closing time goes home. Leaving them queued
            // would stall the grace period on customers who can never be seated.
            for (var i = 0; i < _customers.Count; i++)
            {
                if (_customers[i].State == CustomerState.Queueing) TurnAway(_customers[i]);
            }
        }

        private void SpawnArrivals(float dt)
        {
            if (_setup.ScriptedArrivals != null)
            {
                while (_scriptedIndex < _setup.ScriptedArrivals.Count &&
                       _setup.ScriptedArrivals[_scriptedIndex].AtSeconds <= _elapsed)
                {
                    Arrive(_setup.ScriptedArrivals[_scriptedIndex].Kind);
                    _scriptedIndex++;
                }

                return;
            }

            _arrivalAccumulator += _setup.ArrivalsPerSecond * dt;
            while (_arrivalAccumulator >= 1f)
            {
                _arrivalAccumulator -= 1f;
                Arrive(CustomerTable.Roll(_rng, _setup.CafeLevel, VipWeightMultiplier()));
            }
        }

        private void Arrive(CustomerType kind)
        {
            var customer = Customer.Spawn(_nextCustomerId++, kind, _rng, _setup.CafeLevel,
                _setup.PatienceScale, _servableMenu);

            if (QueueLength >= _setup.DoorCapacity)
            {
                TurnedAway++;
                Emit(ShiftEventKind.TurnedAway, customer.Id, Grid.Door);
                return;
            }

            _customers.Add(customer);
            Emit(ShiftEventKind.CustomerArrived, customer.Id, Grid.Door);
        }

        private void AdvanceCustomers(float dt)
        {
            for (var i = 0; i < _customers.Count; i++)
            {
                var customer = _customers[i];

                if (customer.DrainsPatience)
                {
                    customer.DrainPatience(dt, Metrics.PatienceMultiplier + SeatedCatAmbience());
                    customer.Patience = Curves.Clamp01(
                        customer.Patience + PatienceRegenNear(customer) * dt);

                    if (customer.Patience <= 0f && !customer.Profile.NeverImpatient)
                    {
                        LeaveAngry(customer);
                        continue;
                    }
                }

                switch (customer.State)
                {
                    case CustomerState.Seating:
                        if (Countdown(customer, dt))
                        {
                            customer.State = CustomerState.Thinking;
                            customer.StateTimer = ThinkingSeconds;
                        }

                        break;

                    case CustomerState.Thinking:
                        if (Countdown(customer, dt)) customer.State = CustomerState.Ordering;
                        break;

                    case CustomerState.Eating:
                        if (Countdown(customer, dt)) customer.State = CustomerState.Paying;
                        break;

                    case CustomerState.Paying:
                        Pay(customer);
                        break;

                    case CustomerState.Leaving:
                    case CustomerState.Angry:
                        Countdown(customer, dt);
                        break;
                }
            }
        }

        private static bool Countdown(Customer customer, float dt)
        {
            customer.StateTimer -= dt;
            if (customer.StateTimer > 0f) return false;

            customer.StateTimer = 0f;
            return true;
        }

        private void Pay(Customer customer)
        {
            var profile = customer.Profile;

            var amount = customer.Order.BasePrice
                         * profile.SpendMultiplier
                         * customer.SpendBonus
                         * Metrics.TipMultiplier
                         * customer.ServeTipMultiplier
                         * (0.5f + 0.5f * customer.Patience);

            if (profile.WantsAnArchetype && customer.WantedArchetype.HasValue &&
                RosterHas(customer.WantedArchetype.Value))
            {
                amount *= profile.WantedArchetypeTipBonus;
                if (RosterHasSkill(customer.WantedArchetype.Value, SkillId.Photogenic)) amount *= 2f;
            }

            var paid = (int)Math.Round(amount);
            customer.Paid = paid;
            Earned += paid;
            Served++;
            _reputation += 2;

            if (profile.Judges) _reputation += customer.Patience >= 0.6f ? 15 : -15;

            Emit(ShiftEventKind.Paid, customer.Id, SeatCellOf(customer), paid);

            customer.State = CustomerState.Leaving;
            customer.StateTimer = LeavingSeconds;
            Emit(ShiftEventKind.LeftHappy, customer.Id, SeatCellOf(customer));
            FreeSeat(customer);
        }

        private void LeaveAngry(Customer customer)
        {
            Angry++;
            _reputation -= 4;
            Emit(ShiftEventKind.LeftAngry, customer.Id, SeatCellOf(customer));

            VoidTicketsOf(customer.Id);
            FreeSeat(customer);

            customer.State = CustomerState.Angry;
            customer.StateTimer = LeavingSeconds;
        }

        private void TurnAway(Customer customer)
        {
            TurnedAway++;
            _reputation -= 1;
            Emit(ShiftEventKind.TurnedAway, customer.Id, Grid.Door);
            customer.State = CustomerState.Angry;
            customer.StateTimer = 0f;
        }

        private void RemoveFinishedCustomers()
        {
            for (var i = _customers.Count - 1; i >= 0; i--)
            {
                var customer = _customers[i];
                var done = customer.State == CustomerState.Leaving || customer.State == CustomerState.Angry;
                if (done && customer.StateTimer <= 0f) _customers.RemoveAt(i);
            }
        }

        // -------------------------------------------------------------------------------
        // Tasks
        // -------------------------------------------------------------------------------

        private void RebuildTasks()
        {
            _board.Prune(StillMeaningful);

            var freeSeat = FindFreeSeat() >= 0;

            for (var i = 0; i < _customers.Count; i++)
            {
                var customer = _customers[i];

                if (customer.State == CustomerState.Queueing && freeSeat)
                {
                    _board.Add(TaskKind.Greet, customer.Id);
                }
                else if (customer.State == CustomerState.Ordering)
                {
                    _board.Add(TaskKind.TakeOrder, customer.Id);
                }
            }

            for (var i = 0; i < _tickets.Count; i++)
            {
                var ticket = _tickets[i];
                if (ticket.State == TicketState.Queued) _board.Add(TaskKind.Cook, ticket.Id);
                else if (ticket.State == TicketState.Ready) _board.Add(TaskKind.Deliver, ticket.Id);
            }

            for (var i = 0; i < _seats.Count; i++)
            {
                if (_seats[i].Dirty) _board.Add(TaskKind.Bus, _seats[i].Index);
            }
        }

        private bool StillMeaningful(ShiftTask task)
        {
            switch (task.Kind)
            {
                case TaskKind.Greet:
                {
                    var customer = CustomerById(task.TargetId);
                    return customer != null && customer.State == CustomerState.Queueing &&
                           (FindFreeSeat() >= 0 || task.Claimed);
                }

                case TaskKind.TakeOrder:
                {
                    var customer = CustomerById(task.TargetId);
                    return customer != null && customer.State == CustomerState.Ordering;
                }

                case TaskKind.Cook:
                {
                    var ticket = TicketById(task.TargetId);
                    return ticket != null &&
                           (ticket.State == TicketState.Queued || ticket.State == TicketState.Cooking);
                }

                case TaskKind.Deliver:
                {
                    var ticket = TicketById(task.TargetId);
                    return ticket != null && ticket.State == TicketState.Ready;
                }

                case TaskKind.Bus:
                    return task.TargetId < _seats.Count && _seats[task.TargetId].Dirty;

                default:
                    return false;
            }
        }

        // ITaskContext ------------------------------------------------------------------

        public Cell CellOf(ShiftTask task)
        {
            switch (task.Kind)
            {
                case TaskKind.Greet:
                    return Grid.Door;

                case TaskKind.TakeOrder:
                {
                    var customer = CustomerById(task.TargetId);
                    return customer == null ? Grid.Door : SeatCellOf(customer);
                }

                case TaskKind.Cook:
                case TaskKind.Deliver:
                {
                    var ticket = TicketById(task.TargetId);
                    if (ticket == null) return Grid.Door;
                    return ticket.StationIndex >= 0
                        ? _stations[ticket.StationIndex].Cell
                        : NearestStationCellFor(ticket.Recipe);
                }

                case TaskKind.Bus:
                    return task.TargetId < _seats.Count ? _seats[task.TargetId].Cell : Grid.Door;

                default:
                    return Grid.Door;
            }
        }

        public float UrgencyOf(ShiftTask task)
        {
            switch (task.Kind)
            {
                case TaskKind.Greet:
                case TaskKind.TakeOrder:
                {
                    var customer = CustomerById(task.TargetId);
                    return customer == null ? 0f : 1f - customer.Patience;
                }

                case TaskKind.Cook:
                case TaskKind.Deliver:
                {
                    var ticket = TicketById(task.TargetId);
                    var customer = ticket == null ? null : CustomerById(ticket.CustomerId);
                    return customer == null ? 0f : 1f - customer.Patience;
                }

                case TaskKind.Bus:
                    // A dirty table costs nothing until somebody is waiting for it, and then
                    // it costs everything.
                    return QueueLength > 0 && FindFreeSeat() < 0 ? 0.9f : 0.1f;

                default:
                    return 0f;
            }
        }

        public bool CanDo(Worker worker, ShiftTask task)
        {
            if (!worker.Available) return false;

            switch (task.Kind)
            {
                case TaskKind.Bus:
                    return worker.WillBusTables;

                case TaskKind.Cook:
                {
                    var ticket = TicketById(task.TargetId);
                    if (ticket == null) return false;
                    if (ticket.StationIndex >= 0) return true;
                    return FindFreeStation(ticket.Recipe) >= 0;
                }

                case TaskKind.Greet:
                    return FindFreeSeat() >= 0;

                default:
                    return true;
            }
        }

        // -------------------------------------------------------------------------------
        // Workers
        // -------------------------------------------------------------------------------

        private void BuildWorkers()
        {
            var door = Grid.Door;

            Manager = new Worker
            {
                Id = 0,
                Kind = WorkerKind.Manager,
                X = door.X + 1,
                Y = door.Y,
            };
            _workers.Add(Manager);

            for (var i = 0; i < _setup.Roster.Count; i++)
            {
                _workers.Add(new Worker
                {
                    Id = i + 1,
                    Kind = WorkerKind.Maid,
                    Maid = _setup.Roster[i],
                    X = door.X + 1,
                    Y = Curves.Clamp(door.Y + i - _setup.Roster.Count * 0.5f, 0, Grid.Height - 1),
                });
            }
        }

        private void AdvanceWorkers(float dt)
        {
            for (var i = 0; i < _workers.Count; i++)
            {
                var worker = _workers[i];

                if (worker.OnBreak)
                {
                    worker.BreakRemaining -= dt;
                    if (worker.BreakRemaining > 0f) continue;

                    worker.OnBreak = false;
                    worker.Maid.Mood = Worker.MoodAfterBreak;
                    Emit(ShiftEventKind.BackFromBreak, worker.Id, worker.Cell);
                    continue;
                }

                if (worker.RushRemaining > 0f) worker.RushRemaining -= dt;
                if (worker.EncoreCooldown > 0f) worker.EncoreCooldown -= dt;

                // The manager never picks up work on his own. He is the player's hands, and a
                // manager who helped out by himself would quietly do the maids' jobs — and
                // take their XP with it, which is the opposite of what the game is about.
                if (worker.Claimed == null && !worker.IsManager) TryClaim(worker);

                if (worker.Claimed == null)
                {
                    // Idle. A maid with nothing to do gets a little of herself back.
                    if (!worker.IsManager)
                    {
                        worker.Maid.Mood += IdleMoodRecoveryPerSecond * dt;
                        if (worker.Maid.Mood <= 0f) SendOnBreak(worker);
                    }

                    continue;
                }

                Work(worker, dt);
            }
        }

        private void TryClaim(Worker worker)
        {
            var task = _board.ClaimBest(worker, this);
            if (task != null) PrepareTask(worker, task);
        }

        /// <summary>
        /// Books whatever the task needs before work on it starts. Today that is a station for
        /// a Cook task, held from the moment it is claimed so two maids cannot walk to the
        /// same stove.
        /// </summary>
        private void PrepareTask(Worker worker, ShiftTask task)
        {
            if (task.Kind != TaskKind.Cook) return;

            var ticket = TicketById(task.TargetId);
            if (ticket == null)
            {
                ReleaseWorker(worker);
                return;
            }

            if (ticket.StationIndex < 0)
            {
                var station = FindFreeStation(ticket.Recipe);
                if (station < 0)
                {
                    ReleaseWorker(worker);
                    return;
                }

                ticket.StationIndex = station;
                _stationTicket[station] = ticket.Id;
            }

            ticket.State = TicketState.Cooking;
            Emit(ShiftEventKind.CookStarted, ticket.Id, _stations[ticket.StationIndex].Cell);
        }

        private void Work(Worker worker, float dt)
        {
            var task = worker.Claimed;
            var derived = Derive(worker);
            var target = TargetCellFor(worker, task);

            if (!worker.IsManager)
            {
                worker.Maid.Mood -= derived.MoodDrainPerSecond * dt;
                if (worker.Maid.Mood <= 0f && !worker.NeedsBreak)
                {
                    if (!worker.UsedSecondWind && SkillTable.Has(worker.Maid, SkillId.SecondWind))
                    {
                        worker.UsedSecondWind = true;
                        worker.Maid.Mood = 0.5f;
                        Emit(ShiftEventKind.SecondWind, worker.Id, worker.Cell);
                    }
                    else
                    {
                        worker.NeedsBreak = true;
                        Emit(ShiftEventKind.MoodEmpty, worker.Id, worker.Cell);
                    }
                }
            }

            if (worker.DistanceTo(target) > Worker.ArrivalRadius)
            {
                StepToward(worker, target, derived.MoveSpeed * dt);
                return;
            }

            // Delivering runs in two legs. Arriving at the station is the end of the first.
            if (task.Kind == TaskKind.Deliver && !worker.Carrying)
            {
                worker.Carrying = true;
                worker.Progress = 0f;
                return;
            }

            var seconds = SecondsFor(worker, task, derived);
            worker.Progress += seconds <= 0f ? 1f : dt / seconds;
            if (worker.Progress < 1f) return;

            CompleteTask(worker, task, derived);
        }

        private static void StepToward(Worker worker, Cell target, float distance)
        {
            var dx = target.X - worker.X;
            var dy = target.Y - worker.Y;
            var length = (float)Math.Sqrt(dx * dx + dy * dy);
            if (length <= 1e-4f) return;

            var step = Math.Min(distance, length);
            worker.X += dx / length * step;
            worker.Y += dy / length * step;
        }

        private float SecondsFor(Worker worker, ShiftTask task, MaidDerived derived)
        {
            switch (task.Kind)
            {
                case TaskKind.Greet:
                    return GreetSeconds / derived.GreetSpeed;

                case TaskKind.TakeOrder:
                    return TakeOrderSeconds / derived.ServeSpeed;

                case TaskKind.Cook:
                {
                    var ticket = TicketById(task.TargetId);
                    if (ticket == null) return 0f;

                    if (!worker.IsManager && !worker.UsedMiseEnPlace &&
                        SkillTable.Has(worker.Maid, SkillId.MiseEnPlace))
                    {
                        worker.UsedMiseEnPlace = true;
                        return 0f;
                    }

                    return ticket.Recipe.Seconds / derived.CookSpeed;
                }

                case TaskKind.Deliver:
                    return DeliverSeconds / derived.ServeSpeed;

                case TaskKind.Bus:
                    return BusSeconds / derived.ServeSpeed;

                default:
                    return 1f;
            }
        }

        private void CompleteTask(Worker worker, ShiftTask task, MaidDerived derived)
        {
            switch (task.Kind)
            {
                case TaskKind.Greet:
                    CompleteGreet(worker, task);
                    break;

                case TaskKind.TakeOrder:
                    CompleteTakeOrder(task);
                    break;

                case TaskKind.Cook:
                    CompleteCook(worker, task, derived);
                    break;

                case TaskKind.Deliver:
                    CompleteDeliver(worker, task, derived);
                    break;

                case TaskKind.Bus:
                    CompleteBus(task);
                    break;
            }

            AwardXp(worker, task.Kind);
            _board.Complete(task);
            worker.Drop();

            if (!worker.IsManager && worker.NeedsBreak) SendOnBreak(worker);
        }

        private void CompleteGreet(Worker worker, ShiftTask task)
        {
            var customer = CustomerById(task.TargetId);
            if (customer == null || customer.State != CustomerState.Queueing) return;

            var seatIndex = FindFreeSeat();
            if (seatIndex < 0) return;

            var seat = _seats[seatIndex];
            seat.OccupantId = customer.Id;
            customer.SeatIndex = seatIndex;
            customer.State = CustomerState.Seating;
            customer.StateTimer = SeatingSeconds;

            // The party walks with her, so the maid ends up at the table.
            worker.X = seat.Cell.X;
            worker.Y = seat.Cell.Y;

            if (!worker.IsManager)
            {
                if (SkillTable.Has(worker.Maid, SkillId.WarmWelcome))
                {
                    customer.Patience = Curves.Clamp01(customer.Patience + 0.15f);
                }

                if (SkillTable.Has(worker.Maid, SkillId.Regulars)) customer.SpendBonus *= 1.2f;
            }

            Emit(ShiftEventKind.Seated, customer.Id, seat.Cell);
        }

        private void CompleteTakeOrder(ShiftTask task)
        {
            var customer = CustomerById(task.TargetId);
            if (customer == null || customer.State != CustomerState.Ordering) return;

            customer.State = CustomerState.Waiting;
            Emit(ShiftEventKind.Ordered, customer.Id, SeatCellOf(customer));

            for (var i = 0; i < customer.Order.Dishes.Count; i++)
            {
                var ticket = new Ticket(_nextTicketId++, customer.Id, customer.Order.Dishes[i]);
                _tickets.Add(ticket);
                Emit(ShiftEventKind.TicketQueued, ticket.Id, SeatCellOf(customer));
            }
        }

        private void CompleteCook(Worker worker, ShiftTask task, MaidDerived derived)
        {
            var ticket = TicketById(task.TargetId);
            if (ticket == null) return;

            var station = ticket.StationIndex >= 0 ? _stations[ticket.StationIndex].Cell : Grid.Door;

            if (_rng.Chance(derived.FumbleChance))
            {
                Requeue(ticket);
                Emit(ShiftEventKind.Fumbled, ticket.Id, station);
                return;
            }

            // The Gourmand's rule: the dish is excellent and it never leaves the kitchen.
            if (!worker.IsManager && _rng.Chance(worker.Maid.Profile.EatChance))
            {
                Requeue(ticket);
                Emit(ShiftEventKind.Fumbled, ticket.Id, station);
                return;
            }

            ticket.State = TicketState.Ready;
            Emit(ShiftEventKind.PlateReady, ticket.Id, station);
        }

        private void CompleteDeliver(Worker worker, ShiftTask task, MaidDerived derived)
        {
            var ticket = TicketById(task.TargetId);
            if (ticket == null) return;

            var customer = CustomerById(ticket.CustomerId);

            if (_rng.Chance(derived.DropChance))
            {
                Emit(ShiftEventKind.PlateDropped, ticket.Id, worker.Cell);
                Requeue(ticket);
                GiftPatienceToTheRoom();
                return;
            }

            FreeStationOf(ticket);
            ticket.State = TicketState.Delivered;
            ticket.StationIndex = -1;

            if (customer == null) return;

            customer.ServeTipMultiplier = derived.TipMultiplier;
            customer.Order.Serve();
            Emit(ShiftEventKind.Served, customer.Id, SeatCellOf(customer));

            if (!worker.IsManager && SkillTable.Has(worker.Maid, SkillId.DoubleTray))
            {
                DeliverSecondPlate(customer);
            }

            if (!customer.Order.Complete) return;

            customer.State = CustomerState.Eating;
            customer.StateTimer = EatSecondsPerDish * customer.Order.Dishes.Count;
        }

        /// <summary>
        /// Double Tray: she had a second plate for the same table in her other hand.
        /// </summary>
        /// <remarks>
        /// Scoped to the one customer rather than to any ready plate, because the skill reads
        /// as "she carried both of your dishes at once" — which is a thing the player can see
        /// happen at their own table, not a hidden throughput number.
        /// </remarks>
        private void DeliverSecondPlate(Customer customer)
        {
            for (var i = 0; i < _tickets.Count; i++)
            {
                var other = _tickets[i];
                if (other.CustomerId != customer.Id || other.State != TicketState.Ready) continue;

                FreeStationOf(other);
                other.State = TicketState.Delivered;
                other.StationIndex = -1;
                customer.Order.Serve();
                Emit(ShiftEventKind.Served, customer.Id, SeatCellOf(customer));
                return;
            }
        }

        /// <summary>Presença: a maid standing nearby gives people back a little goodwill.</summary>
        private float PatienceRegenNear(Customer customer)
        {
            if (customer.SeatIndex < 0) return 0f;

            var seat = _seats[customer.SeatIndex].Cell;
            var regen = 0f;

            for (var i = 0; i < _workers.Count; i++)
            {
                var worker = _workers[i];
                if (worker.IsManager || worker.OnBreak) continue;
                if (!SkillTable.Has(worker.Maid, SkillId.Patience)) continue;
                if (worker.DistanceTo(seat) > PresenceRadius) continue;

                regen += PatienceRegenPerSecond;
            }

            return regen;
        }

        private bool RosterHasSkill(Archetype archetype, SkillId skill)
        {
            for (var i = 0; i < _setup.Roster.Count; i++)
            {
                var maid = _setup.Roster[i];
                if (maid.Archetype == archetype && SkillTable.Has(maid, skill)) return true;
            }

            return false;
        }

        private void CompleteBus(ShiftTask task)
        {
            if (task.TargetId >= _seats.Count) return;

            _seats[task.TargetId].Dirty = false;
            Emit(ShiftEventKind.Bussed, task.TargetId, _seats[task.TargetId].Cell);
        }

        private void AwardXp(Worker worker, TaskKind kind)
        {
            if (worker.IsManager) return;
            if (!XpForTask.TryGetValue(kind, out var xp)) return;

            var profile = worker.Maid.Profile;
            var shared = (int)Math.Round(xp * profile.XpShareFraction);

            PendingXp(worker.Maid.Id, xp - shared);
            if (shared <= 0) return;

            var student = LowestLevelMaidOtherThan(worker.Maid.Id);
            PendingXp(student?.Id ?? worker.Maid.Id, shared);
        }

        private void SendOnBreak(Worker worker)
        {
            worker.NeedsBreak = false;
            worker.OnBreak = true;
            worker.BreakRemaining = Worker.BreakSeconds;
            _board.ReleaseAllClaimedBy(worker.Id);
            worker.Drop();
        }

        private void ReleaseWorker(Worker worker)
        {
            _board.ReleaseAllClaimedBy(worker.Id);
            worker.Drop();
        }

        // -------------------------------------------------------------------------------
        // Player commands
        // -------------------------------------------------------------------------------

        /// <summary>Sends the manager to do one specific thing. This is what a click means.</summary>
        public bool CommandManager(TaskKind kind, int targetId)
        {
            if (Finished) return false;

            var task = _board.Find(kind, targetId);
            if (task == null || !_board.Assign(Manager, task, this)) return false;

            PrepareTask(Manager, task);
            return Manager.Claimed == task;
        }

        /// <summary>Shouts at a maid. Faster for six seconds, and it costs her.</summary>
        public bool Rush(int workerId)
        {
            var worker = WorkerById(workerId);
            if (Finished || worker == null || worker.IsManager || !worker.Available) return false;

            worker.RushRemaining = Worker.RushSeconds;
            worker.Maid.Mood -= Worker.RushMoodCost;
            Emit(ShiftEventKind.Rushed, worker.Id, worker.Cell);
            return true;
        }

        /// <summary>The Idol's active: the room forgets it was waiting.</summary>
        public bool UseEncore(int workerId)
        {
            var worker = WorkerById(workerId);
            if (Finished || worker == null || !worker.HasEncore || !worker.Available) return false;
            if (worker.EncoreCooldown > 0f) return false;

            var profile = worker.Maid.Profile;
            worker.EncoreCooldown = profile.EncoreCooldownSeconds;

            for (var i = 0; i < _customers.Count; i++)
            {
                var customer = _customers[i];
                if (customer.SeatIndex < 0) continue;
                customer.Patience = Math.Max(customer.Patience, profile.EncorePatienceRestored);
            }

            Emit(ShiftEventKind.EncoreUsed, worker.Id, worker.Cell);
            return true;
        }

        // -------------------------------------------------------------------------------
        // Finishing
        // -------------------------------------------------------------------------------

        public ShiftResult Finish() => Finish(null);

        private ShiftResult Finish(string failureReason)
        {
            if (_result != null) return _result;

            // Nothing may stay claimed past the end of the day, or a worker reused across
            // shifts would come back already holding yesterday's plate.
            _board.ReleaseAll();
            for (var i = 0; i < _workers.Count; i++) _workers[i].Drop();

            Finished = true;
            TimeRemaining = 0f;

            var quotaMet = failureReason == null && Earned >= _setup.Quota;

            _result = new ShiftResult
            {
                Day = _setup.Day,
                Earned = Earned,
                Quota = _setup.Quota,
                QuotaMet = quotaMet,
                Served = Served,
                Angry = Angry,
                TurnedAway = TurnedAway,
                ReputationDelta = failureReason == null ? _reputation : 0,
                SlowDay = !quotaMet,
                FailureReason = failureReason,
            };

            foreach (var pair in _pendingXp) _result.XpByMaid[pair.Key] = pair.Value;

            Emit(ShiftEventKind.ShiftEnded, 0, Grid.Door);
            return _result;
        }

        /// <summary>
        /// Why this café could not serve anybody, or null if it can.
        /// </summary>
        /// <remarks>
        /// Checked once, up front. A shift that cannot possibly produce a plate would
        /// otherwise sit there generating unclaimable tasks until the clock ran out, which is
        /// a dead end wearing a timer.
        /// </remarks>
        private string WhyThisCafeCannotServe()
        {
            if (_seats.Count == 0)
            {
                return "O café não tem nenhum lugar para sentar. Coloque uma mesa no editor.";
            }

            if (_servableMenu.Count > 0) return null;

            return "A cozinha não tem nenhuma estação que saiba fazer o cardápio de hoje.";
        }

        // -------------------------------------------------------------------------------
        // Helpers
        // -------------------------------------------------------------------------------

        private readonly Dictionary<int, int> _pendingXp = new Dictionary<int, int>();

        private void PendingXp(int maidId, int xp)
        {
            if (xp <= 0) return;

            _pendingXp.TryGetValue(maidId, out var current);
            _pendingXp[maidId] = current + xp;
        }

        private Maid LowestLevelMaidOtherThan(int maidId)
        {
            Maid lowest = null;
            for (var i = 0; i < _setup.Roster.Count; i++)
            {
                var maid = _setup.Roster[i];
                if (maid.Id == maidId) continue;
                if (lowest == null || maid.Level < lowest.Level) lowest = maid;
            }

            return lowest;
        }

        private MaidDerived Derive(Worker worker)
        {
            if (worker.IsManager) return ManagerDerived(worker);

            return MaidMath.Derive(worker.Maid, new ShiftSnapshot(
                Occupancy,
                WorkOf(worker),
                NeighbourAuraAt(worker),
                worker.RushRemaining > 0f,
                QueuedTicketCount));
        }

        /// <summary>
        /// The manager is a flat 1.0 at everything and never tires.
        /// </summary>
        /// <remarks>
        /// Deliberately a constant. He is the baseline the maids are measured against, and
        /// the whole arc of the game is the roster growing past him — which only reads if he
        /// does not grow too.
        /// </remarks>
        private MaidDerived ManagerDerived(Worker worker)
        {
            var rush = worker.RushRemaining > 0f ? 1f + MaidMath.RushBonus : 1f;
            return new MaidDerived(MaidMath.BaseWalkSpeed * rush, rush, rush, rush, 1f, 0f, 0f, 0f);
        }

        /// <summary>The shift's task kinds, in the vocabulary the maid rules speak.</summary>
        private static WorkKind WorkOf(Worker worker)
        {
            if (worker.Claimed == null) return WorkKind.None;

            switch (worker.Claimed.Kind)
            {
                case TaskKind.Greet: return WorkKind.Greet;
                case TaskKind.TakeOrder: return WorkKind.TakeOrder;
                case TaskKind.Cook: return WorkKind.Cook;
                case TaskKind.Deliver: return WorkKind.Deliver;
                case TaskKind.Bus: return WorkKind.Bus;
                default: return WorkKind.None;
            }
        }

        public int QueuedTicketCount
        {
            get
            {
                var count = 0;
                for (var i = 0; i < _tickets.Count; i++)
                {
                    if (_tickets[i].State == TicketState.Queued) count++;
                }

                return count;
            }
        }

        private float NeighbourAuraAt(Worker worker)
        {
            var aura = 0f;
            for (var i = 0; i < _workers.Count; i++)
            {
                var other = _workers[i];
                if (other.Id == worker.Id || other.IsManager || other.OnBreak) continue;

                var dx = other.X - worker.X;
                var dy = other.Y - worker.Y;
                if (dx * dx + dy * dy > NeighbourRadius * NeighbourRadius) continue;

                aura += other.Maid.Profile.NeighbourSpeedAura;
            }

            return aura;
        }

        private float VipWeightMultiplier()
        {
            var highest = 1f;
            for (var i = 0; i < _setup.Roster.Count; i++)
            {
                var multiplier = _setup.Roster[i].Profile.VipWeightMultiplier;
                if (multiplier > highest) highest = multiplier;
            }

            return highest;
        }

        private bool RosterHas(Archetype archetype)
        {
            for (var i = 0; i < _setup.Roster.Count; i++)
            {
                if (_setup.Roster[i].Archetype == archetype) return true;
            }

            return false;
        }

        /// <summary>A cat in the room is worth real patience to everybody else.</summary>
        private float SeatedCatAmbience()
        {
            var bonus = 0f;
            for (var i = 0; i < _customers.Count; i++)
            {
                var customer = _customers[i];
                if (customer.SeatIndex >= 0) bonus += customer.Profile.AmbienceWhileSeated * 0.02f;
            }

            return bonus;
        }

        private void GiftPatienceToTheRoom()
        {
            for (var i = 0; i < _customers.Count; i++)
            {
                var customer = _customers[i];
                if (customer.SeatIndex < 0) continue;
                customer.Patience = Curves.Clamp01(customer.Patience + DropPatienceGift);
            }
        }

        private void Requeue(Ticket ticket)
        {
            FreeStationOf(ticket);
            ticket.StationIndex = -1;
            ticket.Progress = 0f;
            ticket.State = TicketState.Queued;
        }

        private void VoidTicketsOf(int customerId)
        {
            for (var i = _tickets.Count - 1; i >= 0; i--)
            {
                var ticket = _tickets[i];
                if (ticket.CustomerId != customerId) continue;

                FreeStationOf(ticket);
                _tickets.RemoveAt(i);
            }
        }

        private void FreeStationOf(Ticket ticket)
        {
            if (ticket.StationIndex < 0 || ticket.StationIndex >= _stationTicket.Length) return;
            if (_stationTicket[ticket.StationIndex] == ticket.Id) _stationTicket[ticket.StationIndex] = -1;
        }

        private void FreeSeat(Customer customer)
        {
            if (customer.SeatIndex < 0) return;

            var seat = _seats[customer.SeatIndex];
            seat.OccupantId = -1;
            seat.Dirty = true;
            customer.SeatIndex = -1;
        }

        private int FindFreeSeat()
        {
            for (var i = 0; i < _seats.Count; i++)
            {
                if (_seats[i].Free) return i;
            }

            return -1;
        }

        private int FindFreeStation(Recipe recipe)
        {
            for (var i = 0; i < _stations.Count; i++)
            {
                if (_stations[i].Kind == recipe.Station && _stationTicket[i] < 0) return i;
            }

            return -1;
        }

        private Cell NearestStationCellFor(Recipe recipe)
        {
            for (var i = 0; i < _stations.Count; i++)
            {
                if (_stations[i].Kind == recipe.Station) return _stations[i].Cell;
            }

            return Grid.Door;
        }

        private Cell TargetCellFor(Worker worker, ShiftTask task)
        {
            if (task.Kind != TaskKind.Deliver || !worker.Carrying) return CellOf(task);

            var ticket = TicketById(task.TargetId);
            var customer = ticket == null ? null : CustomerById(ticket.CustomerId);
            return customer == null ? Grid.Door : SeatCellOf(customer);
        }

        private Cell SeatCellOf(Customer customer) =>
            customer.SeatIndex >= 0 && customer.SeatIndex < _seats.Count
                ? _seats[customer.SeatIndex].Cell
                : Grid.Door;

        public Customer CustomerById(int id)
        {
            for (var i = 0; i < _customers.Count; i++)
            {
                if (_customers[i].Id == id) return _customers[i];
            }

            return null;
        }

        public Ticket TicketById(int id)
        {
            for (var i = 0; i < _tickets.Count; i++)
            {
                if (_tickets[i].Id == id) return _tickets[i];
            }

            return null;
        }

        public Worker WorkerById(int id)
        {
            for (var i = 0; i < _workers.Count; i++)
            {
                if (_workers[i].Id == id) return _workers[i];
            }

            return null;
        }

        private void Emit(ShiftEventKind kind, int subjectId, Cell at, int amount = 0) =>
            _events.Enqueue(new ShiftEvent(kind, subjectId, at.X, at.Y, amount));
    }
}

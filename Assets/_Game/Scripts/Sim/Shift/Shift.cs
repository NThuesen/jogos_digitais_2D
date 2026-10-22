using System;
using System.Collections.Generic;
using Neko.Sim.Cafe;
using Neko.Sim.Core;
using Neko.Sim.Maids;
using Neko.Sim.Progress;
using Neko.Sim.Service;
using Neko.Sim.Team;

namespace Neko.Sim.Shift
{
    /// <summary>
    /// One day of the café, running on its own.
    /// </summary>
    /// <remarks>
    /// There is no input. The player's entire turn happened before this object was built, in
    /// the lineup, and from here the day is a pure function of the setup and the seed. That is
    /// what lets two hundred seeded days be run against one squad in a unit test, which is the
    /// only honest way to say whether one lineup is better than another.
    ///
    /// No UnityEngine anywhere in here, and everything random goes through <see cref="Rng"/>,
    /// for the same reason.
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

        /// <summary>
        /// Chance a Floor maid hands over the wrong plate, before anything reduces it.
        /// </summary>
        /// <remarks>
        /// Unlike a burnt dish, which costs the kitchen time, a wrong order costs the party's
        /// goodwill — so it is what a low-Serve maid standing in the wrong position does to a
        /// room, and the clearest argument on the dashboard for fixing the lineup.
        /// </remarks>
        public const float BaseMistakeChance = 0.20f;

        /// <summary>What a wrong plate takes off a party's patience, all at once.</summary>
        public const float MistakePatienceCost = 0.35f;

        /// <summary>What a Stage maid gives the whole room, per point of her Charm.</summary>
        public const float StagePatiencePerCharm = 0.0045f;

        /// <summary>What a Management maid adds to every tip, per point of her Charm.</summary>
        public const float ManagementTipPerCharm = 0.004f;

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
        private readonly Dictionary<int, int> _pendingXp = new Dictionary<int, int>();

        private float _elapsed;
        private float _arrivalAccumulator;
        private int _scriptedIndex;
        private int _nextCustomerId = 1;
        private int _nextTicketId = 1;
        private float _graceRemaining;
        private int _happyParties;
        private int _unhappyParties;
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

        public CafeGrid Grid { get; }

        public CafeMetrics Metrics { get; private set; }

        public bool Finished { get; private set; }

        public bool ClosingTime { get; private set; }

        public float TimeRemaining { get; private set; }

        public float Elapsed => _elapsed;

        public int DishRevenue { get; private set; }

        public int Tips { get; private set; }

        public int Earned => DishRevenue + Tips;

        public int Served { get; private set; }

        public int WrongOrders { get; private set; }

        public int Burnt { get; private set; }

        public int DishesCooked { get; private set; }

        public int LeftAngry { get; private set; }

        public int TurnedAway { get; private set; }

        public int Day => _setup.Day;

        public TeamChemistry Chemistry => _setup.Chemistry;

        public IReadOnlyList<Customer> Customers => _customers;

        public IReadOnlyList<Worker> Workers => _workers;

        public IReadOnlyList<Seat> Seats => _seats;

        public IReadOnlyList<Ticket> Tickets => _tickets;

        public IReadOnlyList<StationSpot> Stations => _stations;

        public IReadOnlyList<ShiftTask> OpenTasks => _board.Open;

        public Queue<ShiftEvent> Events => _events;

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

        /// <summary>Nought to one. What the fichas are paid on at the end of the day.</summary>
        public float Satisfaction
        {
            get
            {
                var parties = _happyParties + _unhappyParties;
                return parties == 0 ? 1f : _happyParties / (float)parties;
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
            var stage = StagePatienceBonus();

            for (var i = 0; i < _customers.Count; i++)
            {
                var customer = _customers[i];

                if (customer.DrainsPatience)
                {
                    customer.DrainPatience(dt,
                        Metrics.PatienceMultiplier + SeatedCatAmbience() + stage);
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
            var dishes = customer.Order.BasePrice * profile.SpendMultiplier * customer.SpendBonus;

            if (profile.WantsAnArchetype && customer.WantedArchetype.HasValue &&
                RosterHas(customer.WantedArchetype.Value))
            {
                dishes *= profile.WantedArchetypeTipBonus;
                if (RosterHasSkill(customer.WantedArchetype.Value, SkillId.Photogenic)) dishes *= 2f;
            }

            var tipRate = Metrics.TipMultiplier * customer.ServeTipMultiplier - 1f +
                          ManagementTipBonus();

            // A party who waited to the end of their patience pays for the food and almost
            // nothing on top; one who was looked after tips properly.
            var mood = 0.5f + 0.5f * customer.Patience;
            var paidForFood = (int)Math.Round(dishes * mood);
            var tip = (int)Math.Round(dishes * mood * Math.Max(0f, tipRate));

            customer.Paid = paidForFood + tip;
            DishRevenue += paidForFood;
            Tips += tip;
            Served++;

            if (customer.Patience >= 0.45f && !customer.GotTheWrongPlate) _happyParties++;
            else _unhappyParties++;

            Emit(ShiftEventKind.Paid, customer.Id, SeatCellOf(customer), customer.Paid);

            customer.State = CustomerState.Leaving;
            customer.StateTimer = LeavingSeconds;
            Emit(ShiftEventKind.LeftHappy, customer.Id, SeatCellOf(customer));
            FreeSeat(customer);
        }

        private void LeaveAngry(Customer customer)
        {
            LeftAngry++;
            _unhappyParties++;
            Emit(ShiftEventKind.LeftAngry, customer.Id, SeatCellOf(customer));

            VoidTicketsOf(customer.Id);
            FreeSeat(customer);

            customer.State = CustomerState.Angry;
            customer.StateTimer = LeavingSeconds;
        }

        private void TurnAway(Customer customer)
        {
            TurnedAway++;
            Emit(ShiftEventKind.TurnedAway, customer.Id, Grid.Door);
            customer.State = CustomerState.Angry;
            customer.StateTimer = 0f;
        }

        private void RemoveFinishedCustomers()
        {
            for (var i = _customers.Count - 1; i >= 0; i--)
            {
                var customer = _customers[i];
                var done = customer.State == CustomerState.Leaving ||
                           customer.State == CustomerState.Angry;
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
            if (!worker.Available || !worker.Allows(task.Kind)) return false;

            switch (task.Kind)
            {
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
            var lineup = _setup.Lineup;
            var id = 1;

            if (lineup == null)
            {
                // No lineup given: everybody works the floor. Only tests do this, and it is
                // better than a café that silently refuses to open.
                for (var i = 0; i < _setup.Roster.Count; i++)
                {
                    _workers.Add(Place(id++, _setup.Roster[i], Role.Floor, door, i));
                }

                return;
            }

            var index = 0;
            foreach (var slot in lineup.All)
            {
                if (slot.Empty) continue;

                var maid = FindInRoster(slot.MaidId);
                if (maid == null) continue;

                _workers.Add(Place(id++, maid, slot.Role, door, index++));
            }
        }

        private Worker Place(int id, Maid maid, Role role, Cell door, int index)
        {
            var chemistry = _setup.Chemistry.For(maid.Id);
            var home = HomeOf(role, door);

            // Spread along the row by how many of this position came before her, so two
            // cooks do not stand in exactly the same spot and read as one.
            var sameRole = 0;
            for (var i = 0; i < _workers.Count; i++)
            {
                if (_workers[i].Role == role) sameRole++;
            }

            var stationX = Curves.Clamp(home.X + sameRole * 1.15f, 0f, Grid.Width - 1f);
            var stationY = Curves.Clamp(home.Y - 0.45f + (index % 2) * 0.9f, 0f,
                Grid.Height - 1f);

            return new Worker
            {
                Id = id,
                Maid = maid,
                Role = role,
                Chemistry = chemistry?.Score ?? 0,
                InPosition = chemistry?.InPosition ?? maid.NaturalRole == role,
                StationX = stationX,
                StationY = stationY,
                X = stationX,
                Y = stationY,
            };
        }

        /// <summary>
        /// Where a position stands when there is nothing to do.
        /// </summary>
        /// <remarks>
        /// Reception by the door, kitchen by the stoves, stage and management out of the way.
        /// A café where everybody idles in the same corner reads as broken even when it is
        /// working perfectly.
        /// </remarks>
        public Cell HomeOf(Role role, Cell door)
        {
            switch (role)
            {
                case Role.Reception:
                    return new Cell(Math.Min(door.X + 1, Grid.Width - 1), door.Y);

                case Role.Kitchen:
                    return _stations.Count > 0
                        ? new Cell(Math.Max(0, _stations[0].Cell.X - 1), _stations[0].Cell.Y)
                        : new Cell(Grid.Width - 2, Grid.Height / 2);

                case Role.Stage:
                    return new Cell(Grid.Width / 2, Grid.Height - 1);

                case Role.Management:
                    return new Cell(Grid.Width - 1, 0);

                default:
                    return new Cell(Grid.Width / 2, Math.Max(0, Grid.Height / 2 - 1));
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

                if (worker.EncoreCooldown > 0f) worker.EncoreCooldown -= dt;

                // The Idol fires her own Encore now. There is nobody to press the button.
                if (worker.HasEncore && worker.EncoreCooldown <= 0f && WorthAnEncore()) Encore(worker);

                if (!worker.TakesTasks)
                {
                    PassivePosition(worker, dt);
                    continue;
                }

                if (worker.Claimed == null) TryClaim(worker);

                if (worker.Claimed == null)
                {
                    Idle(worker, dt);
                    continue;
                }

                Work(worker, dt);
            }
        }

        /// <summary>
        /// Stage and Management, who shape the day without touching a task.
        /// </summary>
        /// <remarks>
        /// They still tire, at half rate. A Stage maid who could hold the room's patience up
        /// all day for free would be a strictly correct pick every time, and a position with
        /// no cost is not a decision.
        /// </remarks>
        private void PassivePosition(Worker worker, float dt)
        {
            StepToward(worker, HomeOf(worker.Role, Grid.Door), MaidMath.BaseWalkSpeed * 0.6f * dt);

            var derived = Derive(worker);
            worker.Maid.Mood -= derived.MoodDrainPerSecond * 0.5f * dt;

            if (worker.Maid.Mood > 0f || worker.NeedsBreak) return;

            worker.NeedsBreak = true;
            Emit(ShiftEventKind.MoodEmpty, worker.Id, worker.Cell);
            SendOnBreak(worker);
        }

        private void Idle(Worker worker, float dt)
        {
            StepToward(worker, worker.StationX, worker.StationY,
                MaidMath.BaseWalkSpeed * 0.5f * dt);

            worker.Maid.Mood += IdleMoodRecoveryPerSecond * dt;
            if (worker.Maid.Mood <= 0f) SendOnBreak(worker);
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

        private static void StepToward(Worker worker, Cell target, float distance) =>
            StepToward(worker, target.X, target.Y, distance);

        private static void StepToward(Worker worker, float targetX, float targetY,
            float distance)
        {
            var dx = targetX - worker.X;
            var dy = targetY - worker.Y;
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

                    if (!worker.UsedMiseEnPlace && SkillTable.Has(worker.Maid, SkillId.MiseEnPlace))
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

            if (worker.NeedsBreak) SendOnBreak(worker);
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

            if (SkillTable.Has(worker.Maid, SkillId.WarmWelcome))
            {
                customer.Patience = Curves.Clamp01(customer.Patience + 0.15f);
            }

            if (SkillTable.Has(worker.Maid, SkillId.Regulars)) customer.SpendBonus *= 1.2f;

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
            DishesCooked++;

            if (_rng.Chance(derived.FumbleChance) ||
                _rng.Chance(worker.Maid.Profile.EatChance))
            {
                Burnt++;
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

            // The wrong plate. Costs the party's goodwill rather than the kitchen's time.
            if (customer != null && _rng.Chance(MistakeChance(worker)))
            {
                WrongOrders++;
                customer.GotTheWrongPlate = true;
                customer.Patience = Curves.Clamp01(customer.Patience - MistakePatienceCost);

                Requeue(ticket);
                Emit(ShiftEventKind.WrongOrder, customer.Id, SeatCellOf(customer));
                return;
            }

            FreeStationOf(ticket);
            ticket.State = TicketState.Delivered;
            ticket.StationIndex = -1;

            if (customer == null) return;

            customer.ServeTipMultiplier = derived.TipMultiplier;
            customer.Order.Serve();
            Emit(ShiftEventKind.Served, customer.Id, SeatCellOf(customer));

            if (SkillTable.Has(worker.Maid, SkillId.DoubleTray)) DeliverSecondPlate(customer);

            if (!customer.Order.Complete) return;

            customer.State = CustomerState.Eating;
            customer.StateTimer = EatSecondsPerDish * customer.Order.Dishes.Count;
        }

        /// <summary>
        /// How likely she is to put the wrong plate down.
        /// </summary>
        /// <remarks>
        /// Falls hard with the Serve stat and with chemistry, which is the point: a maid
        /// standing somewhere she does not belong, in a squad that does not fit together,
        /// visibly makes mistakes — and the player reads that straight off the dashboard
        /// without being told what went wrong.
        /// </remarks>
        public float MistakeChance(Worker worker)
        {
            if (worker?.Maid == null) return 0f;

            var serve = worker.Maid.Total.Normalised(StatKind.Serve);
            var chance = BaseMistakeChance * (1f - 0.9f * serve);

            if (!worker.InPosition) chance *= 1.8f;
            chance *= 2f - _setup.Chemistry.Multiplier;

            for (var i = 0; i < worker.Maid.Traits.Count; i++)
            {
                chance *= TraitTable.ById(worker.Maid.Traits[i]).MistakeMultiplier;
            }

            if (SkillTable.Has(worker.Maid, SkillId.SteadyHands)) chance *= 0.6f;

            return Curves.Clamp01(chance);
        }

        /// <summary>Double Tray: she had a second plate for the same table in her other hand.</summary>
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

        private void CompleteBus(ShiftTask task)
        {
            if (task.TargetId >= _seats.Count) return;

            _seats[task.TargetId].Dirty = false;
            Emit(ShiftEventKind.Bussed, task.TargetId, _seats[task.TargetId].Cell);
        }

        private void AwardXp(Worker worker, TaskKind kind)
        {
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

        /// <summary>
        /// The Idol decides for herself now.
        /// </summary>
        /// <remarks>
        /// Fires when enough of the room is far enough through its patience to be worth it. A
        /// player who had to press this would be a player with a control during the day, and
        /// the whole pivot was away from exactly that.
        /// </remarks>
        private bool WorthAnEncore()
        {
            var waiting = 0;
            var hurting = 0;

            for (var i = 0; i < _customers.Count; i++)
            {
                if (_customers[i].SeatIndex < 0) continue;

                waiting++;
                if (_customers[i].Patience < 0.45f) hurting++;
            }

            return waiting >= 2 && hurting >= 2;
        }

        private void Encore(Worker worker)
        {
            var profile = worker.Maid.Profile;
            worker.EncoreCooldown = profile.EncoreCooldownSeconds;

            for (var i = 0; i < _customers.Count; i++)
            {
                var customer = _customers[i];
                if (customer.SeatIndex < 0) continue;
                customer.Patience = Math.Max(customer.Patience, profile.EncorePatienceRestored);
            }

            Emit(ShiftEventKind.EncoreUsed, worker.Id, worker.Cell);
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

            _result = new ShiftResult
            {
                Day = _setup.Day,
                DishRevenue = DishRevenue,
                Tips = Tips,
                Served = Served,
                WrongOrders = WrongOrders,
                Burnt = Burnt,
                DishesCooked = DishesCooked,
                LeftAngry = LeftAngry,
                TurnedAway = TurnedAway,
                ChemistryScore = _setup.Chemistry.Score,
                Satisfaction = Satisfaction,
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
        /// Checked once, up front, and every message names the screen that fixes it. A shift
        /// that cannot possibly produce a plate would otherwise sit there generating
        /// unclaimable tasks until the clock ran out, which is a dead end wearing a timer.
        /// </remarks>
        private string WhyThisCafeCannotServe()
        {
            if (_seats.Count == 0)
            {
                return "O café não tem nenhum lugar para sentar. Compre uma mesa na loja.";
            }

            if (_servableMenu.Count == 0)
            {
                return "A cozinha não tem nenhuma estação que saiba fazer o cardápio de hoje.";
            }

            if (_workers.Count == 0)
            {
                return "Ninguém foi escalado hoje. Monte a escalação antes de abrir.";
            }

            var reception = 0;
            var floor = 0;
            var cooks = 0;
            for (var i = 0; i < _workers.Count; i++)
            {
                if (_workers[i].Role == Role.Reception) reception++;
                if (_workers[i].Role == Role.Floor) floor++;
                if (_workers[i].Role == Role.Kitchen) cooks++;
            }

            // All three are load-bearing. Without a greeter nobody is ever seated and the
            // whole day is a queue at the door, which looks like a bug rather than a lineup
            // mistake — found by a test that left the position empty by accident.
            if (reception == 0) return "Não há ninguém na recepção. Escale ao menos uma maid lá.";
            if (floor == 0) return "Não há ninguém no salão. Escale ao menos uma maid lá.";
            if (cooks == 0) return "Não há ninguém na cozinha. Escale ao menos uma maid lá.";

            return null;
        }

        /// <summary>The unlocked dishes this room can actually produce, given its stations.</summary>
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

        // -------------------------------------------------------------------------------
        // Helpers
        // -------------------------------------------------------------------------------

        private void PendingXp(int maidId, int xp)
        {
            if (xp <= 0) return;

            _pendingXp.TryGetValue(maidId, out var current);
            _pendingXp[maidId] = current + xp;
        }

        private Maid FindInRoster(int maidId)
        {
            for (var i = 0; i < _setup.Roster.Count; i++)
            {
                if (_setup.Roster[i].Id == maidId) return _setup.Roster[i];
            }

            return null;
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

        /// <summary>
        /// Her numbers for this moment, with the lineup folded in.
        /// </summary>
        /// <remarks>
        /// Three things that are not her own stats reach in here: the team's chemistry,
        /// whether she is standing where she belongs, and her traits. Together they are the
        /// entire reason the lineup screen exists.
        /// </remarks>
        public MaidDerived Derive(Worker worker)
        {
            var moment = new ShiftSnapshot(
                Occupancy, WorkOf(worker), NeighbourAuraAt(worker), false, QueuedTicketCount);

            var derived = MaidMath.Derive(worker.Maid, moment);

            var scale = _setup.Chemistry.Multiplier;
            if (!worker.InPosition) scale *= 1f - RoleTable.OutOfPositionPenalty;

            var speed = scale;
            var tip = scale;
            var drain = 1f;

            for (var i = 0; i < worker.Maid.Traits.Count; i++)
            {
                var trait = TraitTable.ById(worker.Maid.Traits[i]);
                speed *= trait.SpeedMultiplier;
                tip *= trait.TipMultiplier;
                drain *= trait.MoodDrainMultiplier;
            }

            var scaled = derived.Scaled(speed, speed, speed, speed, tip);

            return new MaidDerived(scaled.MoveSpeed, scaled.GreetSpeed, scaled.ServeSpeed,
                scaled.CookSpeed, scaled.TipMultiplier, derived.FumbleChance, derived.DropChance,
                derived.MoodDrainPerSecond * drain);
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

        private float NeighbourAuraAt(Worker worker)
        {
            var aura = 0f;
            for (var i = 0; i < _workers.Count; i++)
            {
                var other = _workers[i];
                if (other.Id == worker.Id || other.OnBreak) continue;

                var dx = other.X - worker.X;
                var dy = other.Y - worker.Y;
                if (dx * dx + dy * dy > NeighbourRadius * NeighbourRadius) continue;

                aura += other.Maid.Profile.NeighbourSpeedAura;
            }

            return aura;
        }

        /// <summary>What the stage is worth to the whole room, as extra patience.</summary>
        private float StagePatienceBonus()
        {
            var bonus = 0f;
            for (var i = 0; i < _workers.Count; i++)
            {
                var worker = _workers[i];
                if (worker.Role != Role.Stage || worker.OnBreak) continue;

                bonus += worker.Maid.Total.Charm * StagePatiencePerCharm *
                         _setup.Chemistry.Multiplier;
            }

            return bonus;
        }

        /// <summary>What the till is worth, as a fraction added to every tip.</summary>
        public float ManagementTipBonus()
        {
            var bonus = 0f;
            for (var i = 0; i < _workers.Count; i++)
            {
                var worker = _workers[i];
                if (worker.Role != Role.Management || worker.OnBreak) continue;

                bonus += worker.Maid.Total.Charm * ManagementTipPerCharm *
                         _setup.Chemistry.Multiplier;
            }

            return bonus;
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

        private bool RosterHasSkill(Archetype archetype, SkillId skill)
        {
            for (var i = 0; i < _setup.Roster.Count; i++)
            {
                var maid = _setup.Roster[i];
                if (maid.Archetype == archetype && SkillTable.Has(maid, skill)) return true;
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

        /// <summary>Presença: a maid standing nearby gives people back a little goodwill.</summary>
        private float PatienceRegenNear(Customer customer)
        {
            if (customer.SeatIndex < 0) return 0f;

            var seat = _seats[customer.SeatIndex].Cell;
            var regen = 0f;

            for (var i = 0; i < _workers.Count; i++)
            {
                var worker = _workers[i];
                if (worker.OnBreak) continue;
                if (!SkillTable.Has(worker.Maid, SkillId.Patience)) continue;
                if (worker.DistanceTo(seat) > PresenceRadius) continue;

                regen += PatienceRegenPerSecond;
            }

            return regen;
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
            if (_stationTicket[ticket.StationIndex] == ticket.Id)
            {
                _stationTicket[ticket.StationIndex] = -1;
            }
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

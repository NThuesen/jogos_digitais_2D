using Neko.Sim.Cafe;
using Neko.Sim.Service;

namespace Neko.Sim.Shift
{
    /// <summary>One place to sit, and what is going on at it.</summary>
    public sealed class Seat
    {
        public readonly int Index;
        public readonly Cell Cell;

        /// <summary>Customer id, or -1.</summary>
        public int OccupantId = -1;

        /// <summary>
        /// Left behind by anyone who leaves, happy or not. An angry customer still leaves a
        /// mess, which is what makes letting one go cost twice.
        /// </summary>
        public bool Dirty;

        public Seat(int index, Cell cell)
        {
            Index = index;
            Cell = cell;
        }

        public bool Free => OccupantId < 0 && !Dirty;
    }

    public enum TicketState
    {
        /// <summary>On the rail, waiting for a station and somebody to work it.</summary>
        Queued = 0,

        Cooking,

        /// <summary>Done and sitting at its station, waiting to be carried.</summary>
        Ready,

        Delivered,

        /// <summary>Burnt, dropped or eaten by the cook. It has to be made again.</summary>
        Lost,
    }

    /// <summary>One dish of one order, tracked from the rail to the table.</summary>
    public sealed class Ticket
    {
        public readonly int Id;
        public readonly int CustomerId;
        public readonly Recipe Recipe;

        public TicketState State = TicketState.Queued;

        /// <summary>Index into the shift's station list while cooking or waiting to be carried.</summary>
        public int StationIndex = -1;

        public float Progress;

        public Ticket(int id, int customerId, Recipe recipe)
        {
            Id = id;
            CustomerId = customerId;
            Recipe = recipe;
        }
    }
}

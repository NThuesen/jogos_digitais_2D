using Neko.Sim.Cafe;
using Neko.Sim.Service;
using Neko.Sim.Shift;
using UnityEngine;

namespace Neko.Game.Input
{
    /// <summary>
    /// Turns a point on the floor into the thing the manager should go and do there.
    /// </summary>
    /// <remarks>
    /// Resolved against the simulation rather than against colliders. There are no colliders
    /// in this game — the room is a grid, and a grid already knows what is where, so adding
    /// physics to ask it would be a second source of truth to keep in step.
    /// </remarks>
    public static class ClickTargeting
    {
        /// <summary>How close a click has to land to count as being on somebody.</summary>
        public const float PickRadius = 0.75f;

        public static bool Resolve(Sim.Shift.Shift shift, Vector2 world, out TaskKind kind,
            out int targetId)
        {
            kind = TaskKind.Greet;
            targetId = -1;
            if (shift == null || shift.Finished) return false;

            // Customers first: they are the only thing with a clock running against them.
            var best = float.MaxValue;
            Customer bestCustomer = null;

            foreach (var customer in shift.Customers)
            {
                var at = PositionOf(shift, customer);
                var distance = Vector2.Distance(world, at);
                if (distance > PickRadius || distance >= best) continue;

                best = distance;
                bestCustomer = customer;
            }

            if (bestCustomer != null && TaskForCustomer(bestCustomer, out kind))
            {
                targetId = bestCustomer.Id;
                return shift.OpenTasks.Count == 0 || Exists(shift, kind, targetId);
            }

            var cell = new Cell(Mathf.RoundToInt(world.x), Mathf.RoundToInt(world.y));

            // A station with a ticket on it: cook what is waiting, or carry what is done.
            for (var i = 0; i < shift.Stations.Count; i++)
            {
                if (shift.Stations[i].Cell.DistanceTo(cell) > 0) continue;

                foreach (var ticket in shift.Tickets)
                {
                    if (ticket.StationIndex != i) continue;

                    if (ticket.State == TicketState.Ready)
                    {
                        kind = TaskKind.Deliver;
                        targetId = ticket.Id;
                        return Exists(shift, kind, targetId);
                    }
                }
            }

            // Anything queued, if the kitchen was clicked and nothing specific was under it.
            foreach (var ticket in shift.Tickets)
            {
                if (ticket.State != TicketState.Queued) continue;

                for (var i = 0; i < shift.Stations.Count; i++)
                {
                    if (shift.Stations[i].Kind != ticket.Recipe.Station) continue;
                    if (shift.Stations[i].Cell.DistanceTo(cell) > 0) continue;

                    kind = TaskKind.Cook;
                    targetId = ticket.Id;
                    return Exists(shift, kind, targetId);
                }
            }

            // A dirty table.
            for (var i = 0; i < shift.Seats.Count; i++)
            {
                var seat = shift.Seats[i];
                if (!seat.Dirty || seat.Cell.DistanceTo(cell) > 0) continue;

                kind = TaskKind.Bus;
                targetId = seat.Index;
                return Exists(shift, kind, targetId);
            }

            return false;
        }

        /// <summary>Which maid was clicked, or null. Used by the rush command.</summary>
        public static Worker WorkerAt(Sim.Shift.Shift shift, Vector2 world)
        {
            if (shift == null) return null;

            Worker best = null;
            var distance = PickRadius;

            foreach (var worker in shift.Workers)
            {
                if (worker.IsManager) continue;

                var d = Vector2.Distance(world, new Vector2(worker.X, worker.Y));
                if (d > distance) continue;

                distance = d;
                best = worker;
            }

            return best;
        }

        private static bool TaskForCustomer(Customer customer, out TaskKind kind)
        {
            switch (customer.State)
            {
                case CustomerState.Queueing:
                    kind = TaskKind.Greet;
                    return true;

                case CustomerState.Ordering:
                    kind = TaskKind.TakeOrder;
                    return true;

                default:
                    kind = TaskKind.Greet;
                    return false;
            }
        }

        private static bool Exists(Sim.Shift.Shift shift, TaskKind kind, int targetId)
        {
            foreach (var task in shift.OpenTasks)
            {
                if (task.Kind == kind && task.TargetId == targetId) return true;
            }

            return false;
        }

        private static Vector2 PositionOf(Sim.Shift.Shift shift, Customer customer)
        {
            if (customer.SeatIndex >= 0 && customer.SeatIndex < shift.Seats.Count)
            {
                var cell = shift.Seats[customer.SeatIndex].Cell;
                return new Vector2(cell.X, cell.Y + 0.42f);
            }

            var door = shift.Grid.Door;
            return new Vector2(door.X - 0.55f, door.Y + 0.3f);
        }
    }
}

using System;
using System.Collections.Generic;
using Neko.Sim.Core;
using Neko.Sim.Maids;

namespace Neko.Sim.Team
{
    /// <summary>How many slots each position has at a given café level.</summary>
    public readonly struct RoleSlots
    {
        public readonly int Reception;
        public readonly int Floor;
        public readonly int Kitchen;
        public readonly int Stage;
        public readonly int Management;

        public RoleSlots(int reception, int floor, int kitchen, int stage, int management)
        {
            Reception = reception;
            Floor = floor;
            Kitchen = kitchen;
            Stage = stage;
            Management = management;
        }

        public int Total => Reception + Floor + Kitchen + Stage + Management;

        public int For(Role role)
        {
            switch (role)
            {
                case Role.Reception: return Reception;
                case Role.Floor: return Floor;
                case Role.Kitchen: return Kitchen;
                case Role.Stage: return Stage;
                case Role.Management: return Management;
                default: return 0;
            }
        }
    }

    /// <summary>One maid standing in one slot.</summary>
    public sealed class LineupSlot
    {
        public readonly Role Role;

        /// <summary>Which slot of this position, from zero.</summary>
        public readonly int Index;

        public int MaidId = -1;

        public LineupSlot(Role role, int index)
        {
            Role = role;
            Index = index;
        }

        public bool Empty => MaidId < 0;
    }

    /// <summary>
    /// Who plays where today.
    /// </summary>
    /// <remarks>
    /// The entire interface of the game. Everything else — the shop, the boxes, the room — is
    /// upstream of this object, and the day itself is downstream of it. Nothing the player
    /// does once the day starts touches it.
    /// </remarks>
    public sealed class Lineup
    {
        private readonly List<LineupSlot> _slots = new List<LineupSlot>();

        public Lineup(RoleSlots slots)
        {
            Slots = slots;
            Rebuild(slots);
        }

        public RoleSlots Slots { get; private set; }

        public IReadOnlyList<LineupSlot> All => _slots;

        /// <summary>
        /// Grows or shrinks the board, keeping whoever still has a slot to stand in.
        /// </summary>
        public void Rebuild(RoleSlots slots)
        {
            var kept = new List<LineupSlot>(_slots);
            _slots.Clear();
            Slots = slots;

            foreach (Role role in Enum.GetValues(typeof(Role)))
            {
                for (var i = 0; i < slots.For(role); i++)
                {
                    var slot = new LineupSlot(role, i);

                    for (var k = 0; k < kept.Count; k++)
                    {
                        if (kept[k].Role != role || kept[k].Index != i) continue;
                        slot.MaidId = kept[k].MaidId;
                        break;
                    }

                    _slots.Add(slot);
                }
            }
        }

        public LineupSlot Slot(Role role, int index)
        {
            for (var i = 0; i < _slots.Count; i++)
            {
                if (_slots[i].Role == role && _slots[i].Index == index) return _slots[i];
            }

            return null;
        }

        public Role? RoleOf(int maidId)
        {
            for (var i = 0; i < _slots.Count; i++)
            {
                if (_slots[i].MaidId == maidId) return _slots[i].Role;
            }

            return null;
        }

        public bool Contains(int maidId) => RoleOf(maidId).HasValue;

        public IReadOnlyList<int> MaidIds()
        {
            var ids = new List<int>();
            for (var i = 0; i < _slots.Count; i++)
            {
                if (!_slots[i].Empty) ids.Add(_slots[i].MaidId);
            }

            return ids;
        }

        public IReadOnlyList<int> MaidIdsIn(Role role)
        {
            var ids = new List<int>();
            for (var i = 0; i < _slots.Count; i++)
            {
                if (_slots[i].Role == role && !_slots[i].Empty) ids.Add(_slots[i].MaidId);
            }

            return ids;
        }

        /// <summary>
        /// Puts a maid in a slot, taking her out of wherever she was.
        /// </summary>
        /// <remarks>
        /// Nobody can be in two places at once, and the obvious bug if that were not enforced
        /// is a card that works three positions and makes the lineup pointless.
        /// </remarks>
        public bool Assign(Role role, int index, int maidId)
        {
            var slot = Slot(role, index);
            if (slot == null) return false;

            if (maidId >= 0) Remove(maidId);
            slot.MaidId = maidId;
            return true;
        }

        public bool Remove(int maidId)
        {
            var removed = false;
            for (var i = 0; i < _slots.Count; i++)
            {
                if (_slots[i].MaidId != maidId) continue;
                _slots[i].MaidId = -1;
                removed = true;
            }

            return removed;
        }

        public void Clear()
        {
            for (var i = 0; i < _slots.Count; i++) _slots[i].MaidId = -1;
        }

        /// <summary>
        /// Fills the board from a roster: everybody home first, then best fit.
        /// </summary>
        /// <remarks>
        /// Two passes, because one greedy pass wastes people. Walking the slots in order and
        /// taking the best available for each filled both kitchen slots before it ever
        /// reached the stage, and left the Idol — who belongs on the stage and nowhere else —
        /// standing at a stove. Placing every maid in her natural position first fixes that,
        /// and it is also what a player would do.
        ///
        /// Used when a new business starts and after a box is opened, so somebody who never
        /// touches the lineup screen still has a café that works.
        /// </remarks>
        public void AutoFill(IReadOnlyList<Maid> roster)
        {
            Clear();
            if (roster == null) return;

            var taken = new HashSet<int>();

            for (var i = 0; i < _slots.Count; i++)
            {
                var slot = _slots[i];

                for (var m = 0; m < roster.Count; m++)
                {
                    var maid = roster[m];
                    if (taken.Contains(maid.Id) || maid.NaturalRole != slot.Role) continue;

                    slot.MaidId = maid.Id;
                    taken.Add(maid.Id);
                    break;
                }
            }

            for (var i = 0; i < _slots.Count; i++)
            {
                var slot = _slots[i];
                if (!slot.Empty) continue;

                var profile = RoleTable.For(slot.Role);

                Maid best = null;
                var bestScore = float.NegativeInfinity;

                for (var m = 0; m < roster.Count; m++)
                {
                    var maid = roster[m];
                    if (taken.Contains(maid.Id)) continue;

                    var stats = maid.Total;
                    var score = stats[profile.Primary] + stats[profile.Secondary] * 0.4f;

                    if (score <= bestScore) continue;
                    bestScore = score;
                    best = maid;
                }

                if (best == null) continue;

                slot.MaidId = best.Id;
                taken.Add(best.Id);
            }
        }

        /// <summary>
        /// What the café level gives you to work with.
        /// </summary>
        /// <remarks>
        /// Reception and the two passive positions open late on purpose: the opening café is
        /// one greeter, one waitress and one cook, which is the smallest board that still has
        /// a decision in it.
        /// </remarks>
        public static RoleSlots SlotsFor(int cafeLevel)
        {
            var level = Curves.ClampInt(cafeLevel, 1, 10);

            return new RoleSlots(
                reception: level >= 6 ? 2 : 1,
                floor: level >= 7 ? 3 : level >= 3 ? 2 : 1,
                kitchen: level >= 8 ? 3 : level >= 4 ? 2 : 1,
                stage: level >= 2 ? 1 : 0,
                management: level >= 5 ? 1 : 0);
        }
    }
}

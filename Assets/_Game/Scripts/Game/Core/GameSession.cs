using System;
using Neko.Sim.Core;
using Neko.Sim.Economy;
using Neko.Sim.Save;
using Neko.Sim.Shift;

namespace Neko.Game.Core
{
    /// <summary>
    /// One business: the profile, the day about to run, and the books that closed the last one.
    /// </summary>
    /// <remarks>
    /// Sits between the screens and the simulation so that no screen ever mutates a profile
    /// directly. The rule that pays for itself is <see cref="Touch"/>: anything that changes
    /// the business saves it, so a browser tab closed mid-shop does not cost the player the
    /// card they just pulled.
    /// </remarks>
    public sealed class GameSession
    {
        private readonly Rng _rng;

        public GameSession(Profile profile)
        {
            Profile = profile ?? Profile.NewGame(1u);
            _rng = new Rng(Profile.Seed == 0 ? 1u : Profile.Seed);
        }

        public Profile Profile { get; }

        /// <summary>The books of the last day that closed. What the dashboard shows.</summary>
        public DayLedger LastLedger { get; private set; }

        public event Action Changed;

        public static GameSession Load() => new GameSession(SaveBridge.Load().Profile);

        public bool Bankrupt => Profile.Bankrupt;

        /// <summary>What it costs to open tomorrow, whatever today does.</summary>
        public int FixedCosts => Books.FixedCostsFor(Profile);

        /// <summary>The balance is thin enough that the player should be told.</summary>
        public bool InDanger => !Bankrupt &&
                                Profile.Wallet.Money < FixedCosts * Books.WarningDays;

        /// <summary>Why today cannot open, or null. Checked before the button is offered.</summary>
        public string WhyNotReady()
        {
            var lineup = Profile.Lineup;

            if (lineup.MaidIdsIn(Neko.Sim.Team.Role.Reception).Count == 0 ||
                lineup.MaidIdsIn(Neko.Sim.Team.Role.Floor).Count == 0 ||
                lineup.MaidIdsIn(Neko.Sim.Team.Role.Kitchen).Count == 0)
            {
                return Ui.Strings.Get("lineup.needall");
            }

            return null;
        }

        public Sim.Shift.Shift StartDay() =>
            new Sim.Shift.Shift(DayPlan.Build(Profile, _rng.NextUInt()));

        /// <summary>Closes the books on a finished day and saves.</summary>
        public DayLedger FinishDay(ShiftResult result)
        {
            LastLedger = Books.Close(Profile, result);
            Touch();
            return LastLedger;
        }

        public Rng Rng => _rng;

        /// <summary>Something changed. Save it and tell the screens.</summary>
        public void Touch()
        {
            SaveBridge.Save(Profile);
            Changed?.Invoke();
        }
    }
}

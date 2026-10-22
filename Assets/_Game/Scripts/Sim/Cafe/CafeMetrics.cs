using Neko.Sim.Core;

namespace Neko.Sim.Cafe
{
    /// <summary>
    /// What a layout is worth, in the four numbers the editor shows and the shift reads.
    /// </summary>
    /// <remarks>
    /// These exist so that decorating is a layout puzzle wearing decoration's clothes. If the
    /// player could not see Flow fall when they move the stove, the café editor would be a
    /// dress-up screen, which the design calls an anti-goal.
    /// </remarks>
    public readonly struct CafeMetrics
    {
        public readonly float Ambience;

        /// <summary>Mean walk, in tiles, over door-to-seat and seat-to-station. Lower is better.</summary>
        public readonly float Flow;

        public readonly int Seats;
        public readonly int StationCount;
        public readonly ThemeTag DominantTheme;
        public readonly bool ThemeBonusActive;
        public readonly float PatienceMultiplier;
        public readonly float TipMultiplier;

        public CafeMetrics(float ambience, float flow, int seats, int stationCount,
            ThemeTag dominantTheme, bool themeBonusActive, float patienceMultiplier,
            float tipMultiplier)
        {
            Ambience = ambience;
            Flow = flow;
            Seats = seats;
            StationCount = stationCount;
            DominantTheme = dominantTheme;
            ThemeBonusActive = themeBonusActive;
            PatienceMultiplier = patienceMultiplier;
            TipMultiplier = tipMultiplier;
        }
    }
}

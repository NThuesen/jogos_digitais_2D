namespace Neko.Sim.Progress
{
    public enum SkillBranch
    {
        Floor = 0,
        Kitchen,
        Charm,
    }

    /// <summary>
    /// The eighteen skills, six per branch. A maid equips at most three.
    /// </summary>
    /// <remarks>
    /// Explicit values, because a skill id is written into the save file and renumbering the
    /// enum would silently hand every saved maid somebody else's skills.
    /// </remarks>
    public enum SkillId
    {
        None = 0,

        // Floor
        DoubleTray = 100,
        QuickFeet = 101,
        Usher = 102,
        TableSense = 103,
        CleanSweep = 104,
        Multitask = 105,

        // Kitchen
        MiseEnPlace = 200,
        SteadyHands = 201,
        BatchCooking = 202,
        Prep = 203,
        HeatControl = 204,
        Plating = 205,

        // Charm
        Regulars = 300,
        WarmWelcome = 301,
        SecondWind = 302,
        Patience = 303,
        SweetTalk = 304,
        Photogenic = 305,
    }
}

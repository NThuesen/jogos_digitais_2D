namespace Neko.Sim.Core
{
    /// <summary>
    /// The decorative families shared by café furniture and maid equipment.
    /// </summary>
    /// <remarks>
    /// One enum for both on purpose: the set bonus is what makes dressing a maid and
    /// decorating the room the same decision, and two parallel enums would let them drift.
    /// </remarks>
    public enum ThemeTag
    {
        None = 0,
        Sakura,
        Gothic,
        Starry,
        Showa,
    }
}

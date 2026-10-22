namespace Neko.Game.Audio
{
    /// <summary>
    /// Every sound the game makes.
    /// </summary>
    /// <remarks>
    /// All of them synthesised and committed as WAV — nothing was recorded or downloaded, so
    /// there is no audio licence to track either. The nine "nya" variants exist because a
    /// player should be able to tell which maid just did something with their eyes on another
    /// part of the room, which is rubric 3's job done by the speakers instead of the HUD.
    /// </remarks>
    public enum SoundId
    {
        DoorBell = 0,
        OrderPop,
        TicketSlap,
        Sizzle,
        PlateChime,
        Register,
        Coin,
        Heart,
        AngryPuff,
        Drop,
        LevelUp,
        UiTick,
        UiConfirm,
        UiBack,

        NyaGenki,
        NyaKuudere,
        NyaTsundere,
        NyaDojikko,
        NyaOjousama,
        NyaOneesan,
        NyaIdol,
        NyaGourmand,
        NyaMukuchi,
    }

    public enum MusicBed
    {
        Menu = 0,
        ShiftCalm,
        ShiftRush,
    }
}

namespace Neko.Sim.Shift
{
    /// <summary>
    /// Everything the simulation does that the player should see or hear.
    /// </summary>
    /// <remarks>
    /// The view and the audio both switch exhaustively over this enum, and a test asserts
    /// that every value produces some response. That is the mechanism behind the design's
    /// rule that feedback is carried by the world rather than the HUD: a new thing the
    /// simulation can do cannot be added silently.
    /// </remarks>
    public enum ShiftEventKind
    {
        CustomerArrived = 0,
        TurnedAway,
        Seated,
        Ordered,
        TicketQueued,
        CookStarted,
        Fumbled,
        PlateReady,
        PlateDropped,
        Served,
        Paid,
        LeftAngry,
        LeftHappy,
        Bussed,
        MoodEmpty,
        SecondWind,
        BackFromBreak,
        Rushed,
        EncoreUsed,
        ClosingTime,
        ShiftEnded,
    }

    /// <summary>One thing that just happened, with enough to draw and play it.</summary>
    public readonly struct ShiftEvent
    {
        public readonly ShiftEventKind Kind;

        /// <summary>A customer id, a ticket id, a seat index or a worker id — by kind.</summary>
        public readonly int SubjectId;

        public readonly float X;
        public readonly float Y;

        /// <summary>Money for <see cref="ShiftEventKind.Paid"/>; otherwise whatever fits.</summary>
        public readonly int Amount;

        public ShiftEvent(ShiftEventKind kind, int subjectId, float x, float y, int amount = 0)
        {
            Kind = kind;
            SubjectId = subjectId;
            X = x;
            Y = y;
            Amount = amount;
        }
    }
}

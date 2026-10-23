using Neko.Game.Art;
using Neko.Game.Audio;
using Neko.Game.Feel;
using Neko.Game.Input;
using Neko.Game.Ui;
using Neko.Game.View;
using Neko.Sim.Progress;
using Neko.Sim.Save;
using UnityEngine;

namespace Neko.Game.Core
{
    /// <summary>
    /// Builds the whole game at boot and runs it.
    /// </summary>
    /// <remarks>
    /// The Boot scene holds one empty object with this on it. Everything else — the camera,
    /// the room, the interface, the audio — is created here in code. No prefabs, no scene
    /// hierarchy to merge, and a layout that can be read in a diff.
    ///
    /// Note what is missing: there is no input path from the player into a running day. The
    /// pointer only reaches the game at all on the hall screen, where the player is arranging
    /// furniture between days.
    /// </remarks>
    public sealed class GameRoot : MonoBehaviour
    {
        public static GameRoot Instance { get; private set; }

        public SpriteFactory Sprites { get; private set; }

        public CameraRig Camera { get; private set; }

        public RoomView Room { get; private set; }

        public ShiftPresenter Presenter { get; private set; }

        public Effects Effects { get; private set; }

        public AudioDirector Audio { get; private set; }

        public MusicDirector Music { get; private set; }

        public UiRoot Ui { get; private set; }

        public NekoInput Input { get; private set; }

        public AppStateMachine State { get; } = new AppStateMachine();

        public GameSession Session { get; private set; }

        private void Awake()
        {
            Instance = this;
            Application.targetFrameRate = 60;

            // The skill table installs itself into the maid maths. A shift does this too, but
            // the roster screen derives stats before any day has run.
            SkillTable.Install();

            Sprites = new SpriteFactory();
            BuildCamera();
            BuildWorld();
            BuildAudio();
            BuildUi();
            BuildInput();

            ErrorBoundary.Failed += OnFailure;

            LoadSession();
            State.Go(AppState.Menu);
            Ui.Show<MenuScreen>();
        }

        private void OnDestroy()
        {
            ErrorBoundary.Failed -= OnFailure;
            Tween.KillAll();
            Sprites?.Clear();
            if (Instance == this) Instance = null;
        }

        // -------------------------------------------------------------------------------
        // Building
        // -------------------------------------------------------------------------------

        private void BuildCamera()
        {
            var go = new GameObject("Camera", typeof(Camera));
            go.transform.SetParent(transform, false);
            go.tag = "MainCamera";
            Camera = go.AddComponent<CameraRig>();
        }

        private void BuildWorld()
        {
            var room = new GameObject("Room");
            room.transform.SetParent(transform, false);
            Room = room.AddComponent<RoomView>();
            Room.Bind(Sprites);

            var presenter = new GameObject("Day");
            presenter.transform.SetParent(transform, false);
            Presenter = presenter.AddComponent<ShiftPresenter>();

            var effects = new GameObject("Effects");
            effects.transform.SetParent(transform, false);
            Effects = effects.AddComponent<Effects>();
        }

        private void BuildAudio()
        {
            var go = new GameObject("Audio");
            go.transform.SetParent(transform, false);

            Audio = go.AddComponent<AudioDirector>();
            Audio.Load();

            Music = go.AddComponent<MusicDirector>();
            Music.Load();
        }

        private void BuildUi()
        {
            var go = new GameObject("Ui", typeof(RectTransform));
            go.transform.SetParent(transform, false);

            Ui = go.AddComponent<UiRoot>();
            Ui.Build(Sprites, Audio, Camera.Camera);

            Effects.Bind(Sprites, Ui.OverlayLayer);
            Presenter.Bind(Sprites, Room, Effects, Audio, Music, Camera);
            Presenter.Finished += OnDayFinished;

            BuildScreens();
        }

        private void BuildScreens()
        {
            var menu = Ui.Add<MenuScreen>();
            menu.OnPlay = () => ErrorBoundary.Guard("abrir a empresa", EnterHub);
            menu.OnNewGame = () => ErrorBoundary.Guard("nova empresa", NewGame);

            var hub = Ui.Add<HubScreen>();
            hub.OnOpenDay = () => ErrorBoundary.Guard("abrir o dia", StartDay);
            hub.OnLineup = () => Ui.Show<LineupScreen>();
            hub.OnRoster = () => Ui.Show<RosterScreen>();
            hub.OnShop = () => Ui.Show<ShopScreen>();
            hub.OnBoxes = () => Ui.Show<BoxScreen>();
            hub.OnHall = () => Ui.Show<HallScreen>();
            hub.OnMenu = () => ErrorBoundary.Guard("voltar ao menu", BackToMenu);

            var lineup = Ui.Add<LineupScreen>();
            lineup.OnBack = () => Ui.Show<HubScreen>();

            var roster = Ui.Add<RosterScreen>();
            roster.OnBack = () => Ui.Show<HubScreen>();

            var shop = Ui.Add<ShopScreen>();
            shop.OnBack = () => Ui.Show<HubScreen>();

            var boxes = Ui.Add<BoxScreen>();
            boxes.OnBack = () => Ui.Show<HubScreen>();

            var hall = Ui.Add<HallScreen>();
            hall.Room = Room;
            hall.Camera = Camera;
            hall.OnBack = () => ErrorBoundary.Guard("voltar", LeaveHall);

            var day = Ui.Add<DayHud>();
            day.OnSpeed = speed => Clocks.Scale = speed;
            day.OnSkip = () => ErrorBoundary.Guard("pular o dia", SkipDay);

            var books = Ui.Add<BooksScreen>();
            books.OnContinue = () => ErrorBoundary.Guard("voltar à empresa", AfterBooks);

            var bust = Ui.Add<BustScreen>();
            bust.OnRestart = () => ErrorBoundary.Guard("recomeçar", NewGame);
        }

        private void BuildInput()
        {
            Input = gameObject.AddComponent<NekoInput>();
            Input.Click += OnClick;
            Input.RightClick += OnRightClick;
            Input.Rotate += () => Ui.Get<HallScreen>()?.Rotate();
            Input.Pause += OnEscape;
        }

        // -------------------------------------------------------------------------------
        // Flow
        // -------------------------------------------------------------------------------

        private void LoadSession()
        {
            var loaded = SaveBridge.Load();
            Session = new GameSession(loaded.Profile);
            Bind(Session);

            if (loaded.Status == LoadStatus.Corrupt || loaded.Status == LoadStatus.Migrated)
            {
                Ui.Dialog(Strings.Get("error.title"), loaded.Message);
            }
        }

        private void NewGame()
        {
            // The records survive the business, which is what makes a bankruptcy a run rather
            // than a wipe.
            var records = Session?.Profile.Records;

            SaveBridge.Wipe();
            var profile = Profile.NewGame((uint)System.DateTime.Now.Ticks);

            if (records != null)
            {
                profile.Records.BestDay = records.BestDay;
                profile.Records.BestRevenue = records.BestRevenue;
                profile.Records.TotalCustomersServed = records.TotalCustomersServed;
                profile.Records.TotalDaysPlayed = records.TotalDaysPlayed;
                profile.Records.LegendariesFound = records.LegendariesFound;
                profile.Records.BusinessesLost = records.BusinessesLost +
                                                 (Session.Bankrupt ? 1 : 0);
            }

            Session = new GameSession(profile);
            Bind(Session);
            Session.Touch();
            EnterHub();
        }

        private void Bind(GameSession session)
        {
            Ui.Get<MenuScreen>()?.SetSession(session);

            Set(Ui.Get<HubScreen>(), s => s.Session = session);
            Set(Ui.Get<LineupScreen>(), s => s.Session = session);
            Set(Ui.Get<RosterScreen>(), s => s.Session = session);
            Set(Ui.Get<ShopScreen>(), s => s.Session = session);
            Set(Ui.Get<BoxScreen>(), s => s.Session = session);
            Set(Ui.Get<HallScreen>(), s => s.Session = session);
            Set(Ui.Get<BooksScreen>(), s => s.Session = session);

            session.Changed += () => Ui.RefreshCurrent();
        }

        private static void Set<T>(T screen, System.Action<T> apply) where T : class
        {
            if (screen != null) apply(screen);
        }

        private void EnterHub()
        {
            if (Session.Bankrupt)
            {
                Ui.Show<BustScreen>();
                return;
            }

            if (State.Current != AppState.Cafe) State.Go(AppState.Cafe);

            Clocks.Reset();
            Room.Build(Session.Profile.Grid);
            Room.SetFlowOverlay(false);
            Camera.FrameRoom(Session.Profile.Grid.Width, Session.Profile.Grid.Height);
            Music.Play(MusicBed.Menu);
            Ui.Show<HubScreen>();
        }

        private void LeaveHall()
        {
            Room.SetFlowOverlay(false);
            Ui.Show<HubScreen>();
        }

        private void BackToMenu()
        {
            State.Go(AppState.Menu);
            Clocks.Reset();
            Session.Touch();
            Ui.Get<MenuScreen>()?.SetSession(Session);
            Music.Play(MusicBed.Menu);
            Ui.Show<MenuScreen>();
        }

        private void StartDay()
        {
            var why = Session.WhyNotReady();
            if (why != null)
            {
                Ui.Dialog(Strings.Get("lineup.title"), why);
                return;
            }

            if (!State.Go(AppState.Shift)) return;

            Clocks.Reset();

            var shift = Session.StartDay();
            Camera.FrameRoom(shift.Grid.Width, shift.Grid.Height);
            Presenter.Begin(shift);

            // Shown first, then filled. A screen builds its widgets on its first show,
            // so putting the chips in beforehand built them onto a row that Build then
            // replaced — and the strip came up empty every single day.
            Ui.Show<DayHud>();
            Ui.Get<DayHud>().BuildChips(shift);
        }

        /// <summary>
        /// Runs the rest of the day out in one go.
        /// </summary>
        /// <remarks>
        /// Exactly the same simulation, ticked as fast as it will go. Skipping cannot change
        /// the outcome, because there is nothing the player could have done differently once
        /// the doors opened — which is the whole reason skipping is allowed at all.
        /// </remarks>
        private void SkipDay()
        {
            var shift = Presenter.Shift;
            if (shift == null) return;

            var guard = 0;
            while (!shift.Finished && guard < 40000)
            {
                shift.Tick(0.05f);
                guard++;
            }

            Presenter.Discard();
        }

        private void OnDayFinished(Sim.Shift.ShiftResult result)
        {
            var ledger = Session.FinishDay(result);
            Clocks.Reset();

            if (!State.Go(AppState.Results)) return;

            if (ledger.Bankrupt)
            {
                Music.Play(MusicBed.Menu);
                Ui.Show<BustScreen>();
                return;
            }

            Ui.Show<BooksScreen>();
        }

        private void AfterBooks()
        {
            if (Session.Bankrupt)
            {
                Ui.Show<BustScreen>();
                return;
            }

            State.Go(AppState.Cafe);
            EnterHub();
        }

        private void OnEscape()
        {
            if (Ui.DialogOpen)
            {
                Ui.CloseDialog();
                return;
            }

            if (State.Current == AppState.Cafe && !(Ui.Current is HubScreen)) Ui.Show<HubScreen>();
        }

        // -------------------------------------------------------------------------------
        // Pointer
        // -------------------------------------------------------------------------------

        private void OnClick(Vector2 screen)
        {
            if (!(Ui.Current is HallScreen hall)) return;

            var world = Camera.ScreenToWorld(screen);
            hall.PlaceAt(Mathf.RoundToInt(world.x), Mathf.RoundToInt(world.y));
        }

        private void OnRightClick(Vector2 screen)
        {
            if (!(Ui.Current is HallScreen hall)) return;

            var world = Camera.ScreenToWorld(screen);
            hall.RemoveAt(Mathf.RoundToInt(world.x), Mathf.RoundToInt(world.y));
        }

        private void Update()
        {
            Tween.TickAll();

            if (State.Current != AppState.Shift) return;

            ErrorBoundary.Guard("o dia", () => Presenter.Tick());
            Ui.Get<DayHud>()?.Sync();
        }

        /// <summary>Something threw. End the day cleanly rather than leave a café frozen.</summary>
        private void OnFailure(string what, System.Exception error)
        {
            if (State.Current != AppState.Shift) return;

            var result = Presenter.Shift?.Finish();
            if (result != null) result.FailureReason = Strings.Get("error.day");

            OnDayFinished(result);
            Ui.Dialog(Strings.Get("error.title"), Strings.Get("error.day"));
        }
    }
}

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

        private bool _paused;

        private void Awake()
        {
            Instance = this;
            Application.targetFrameRate = 60;

            // The skill table installs itself into the maid maths. A shift does this too, but
            // the roster screen derives stats before any shift has run.
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

            var presenter = new GameObject("Shift");
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
            Ui.Build(Sprites, Audio);

            Effects.Bind(Sprites, Ui.OverlayLayer);
            Presenter.Bind(Sprites, Room, Effects, Audio, Music, Camera);
            Presenter.Finished += OnShiftFinished;

            BuildScreens();
        }

        private void BuildScreens()
        {
            var menu = Ui.Add<MenuScreen>();
            menu.OnPlay = () => ErrorBoundary.Guard("abrir o café", EnterCafe);
            menu.OnNewGame = () => ErrorBoundary.Guard("novo jogo", NewGame);

            var cafe = Ui.Add<CafeScreen>();
            cafe.OnOpenDay = () => ErrorBoundary.Guard("abrir o dia", StartShift);
            cafe.OnRoster = () => Ui.Show<RosterScreen>();
            cafe.OnShop = () => Ui.Show<ShopScreen>();
            cafe.OnEditor = () => Ui.Show<EditorScreen>();
            cafe.OnMenu = () => ErrorBoundary.Guard("voltar ao menu", BackToMenu);

            var hud = Ui.Add<ShiftHud>();
            hud.OnPause = TogglePause;

            var results = Ui.Add<ResultsScreen>();
            results.OnContinue = () => ErrorBoundary.Guard("voltar ao café", BackToCafe);

            var roster = Ui.Add<RosterScreen>();
            roster.OnBack = () => Ui.Show<CafeScreen>();

            var shop = Ui.Add<ShopScreen>();
            shop.OnBack = () => Ui.Show<CafeScreen>();

            var editor = Ui.Add<EditorScreen>();
            editor.Room = Room;
            editor.Camera = Camera;
            editor.OnBack = () => Ui.Show<CafeScreen>();

            var pause = Ui.Add<PauseScreen>();
            pause.OnResume = TogglePause;
            pause.OnQuit = () => ErrorBoundary.Guard("sair", BackToMenu);
            pause.OnMusic = on => Music.Muted = !on;
            pause.OnSfx = on => Audio.Muted = !on;
        }

        private void BuildInput()
        {
            Input = gameObject.AddComponent<NekoInput>();
            Input.Click += OnClick;
            Input.RightClick += OnRightClick;
            Input.Pause += OnPauseKey;
            Input.Rotate += () => Ui.Get<EditorScreen>()?.Rotate();
            Input.ToggleFlow += () => Room.SetFlowOverlay(true);
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
            SaveBridge.Wipe();
            Session = new GameSession(Profile.NewGame((uint)System.DateTime.Now.Ticks));
            Bind(Session);
            Session.Touch();
            EnterCafe();
        }

        private void Bind(GameSession session)
        {
            Ui.Get<MenuScreen>()?.SetSession(session);

            var cafe = Ui.Get<CafeScreen>();
            if (cafe != null) cafe.Session = session;

            var roster = Ui.Get<RosterScreen>();
            if (roster != null) roster.Session = session;

            var shop = Ui.Get<ShopScreen>();
            if (shop != null) shop.Session = session;

            var editor = Ui.Get<EditorScreen>();
            if (editor != null) editor.Session = session;

            var results = Ui.Get<ResultsScreen>();
            if (results != null) results.Session = session;

            session.Changed += () => Ui.RefreshCurrent();
        }

        private void EnterCafe()
        {
            if (!State.Go(AppState.Cafe)) return;

            Clocks.Reset();
            _paused = false;
            Room.Build(Session.Profile.Grid);
            Room.SetFlowOverlay(false);
            Camera.FrameRoom(Session.Profile.Grid.Width, Session.Profile.Grid.Height);
            Music.Play(MusicBed.Menu);
            Ui.Show<CafeScreen>();
        }

        private void BackToCafe()
        {
            if (State.Current == AppState.Results || State.Current == AppState.Shift)
            {
                State.Go(AppState.Cafe);
            }

            Clocks.Reset();
            _paused = false;
            Room.Build(Session.Profile.Grid);
            Camera.FrameRoom(Session.Profile.Grid.Width, Session.Profile.Grid.Height);
            Music.Play(MusicBed.Menu);
            Ui.Show<CafeScreen>();
        }

        private void BackToMenu()
        {
            State.Go(AppState.Menu);
            Clocks.Reset();
            _paused = false;
            Session.Touch();
            Ui.Get<MenuScreen>()?.SetSession(Session);
            Music.Play(MusicBed.Menu);
            Ui.Show<MenuScreen>();
        }

        private void StartShift()
        {
            if (!State.Go(AppState.Shift)) return;

            Clocks.Reset();
            _paused = false;

            var shift = Session.StartDay();
            Camera.FrameRoom(shift.Grid.Width, shift.Grid.Height);
            Presenter.Begin(shift);

            var hud = Ui.Get<ShiftHud>();
            hud.BuildChips(shift);
            Ui.Show<ShiftHud>();
        }

        private void OnShiftFinished(Sim.Shift.ShiftResult result)
        {
            Session.FinishDay(result);

            if (!State.Go(AppState.Results)) return;

            Clocks.Reset();
            _paused = false;
            Ui.Show<ResultsScreen>();
        }

        private void TogglePause()
        {
            if (State.Current != AppState.Shift) return;

            _paused = !_paused;
            Clocks.Pause(_paused);
            Music.Duck(_paused);
            Input.Enabled = !_paused;

            if (_paused) Ui.Show<PauseScreen>();
            else Ui.Show<ShiftHud>();
        }

        private void OnPauseKey()
        {
            if (Ui.DialogOpen)
            {
                Ui.CloseDialog();
                return;
            }

            if (State.Current == AppState.Shift) TogglePause();
        }

        // -------------------------------------------------------------------------------
        // Pointer
        // -------------------------------------------------------------------------------

        private void OnClick(Vector2 screen)
        {
            var world = Camera.ScreenToWorld(screen);

            if (State.Current == AppState.Cafe && Ui.Current is EditorScreen editor)
            {
                editor.PlaceAt(Mathf.RoundToInt(world.x), Mathf.RoundToInt(world.y));
                return;
            }

            if (State.Current != AppState.Shift || _paused) return;

            if (!ClickTargeting.Resolve(Presenter.Shift, world, out var kind, out var id)) return;

            Presenter.Shift.CommandManager(kind, id);
            Audio.Play(SoundId.UiTick, 0.4f);
        }

        private void OnRightClick(Vector2 screen)
        {
            var world = Camera.ScreenToWorld(screen);

            if (State.Current == AppState.Cafe && Ui.Current is EditorScreen editor)
            {
                editor.RemoveAt(Mathf.RoundToInt(world.x), Mathf.RoundToInt(world.y));
                return;
            }

            if (State.Current != AppState.Shift || _paused) return;

            var worker = ClickTargeting.WorkerAt(Presenter.Shift, world);
            if (worker == null) return;

            if (Presenter.Shift.Rush(worker.Id)) Audio.Nya(worker.Maid.Archetype);
        }

        // -------------------------------------------------------------------------------

        private void Update()
        {
            Tween.TickAll();

            if (State.Current != AppState.Shift || _paused) return;

            ErrorBoundary.Guard("o turno", () => Presenter.Tick());
            Ui.Get<ShiftHud>()?.Sync();
        }

        /// <summary>
        /// Something threw. End the day cleanly rather than leave a café that no longer moves.
        /// </summary>
        private void OnFailure(string what, System.Exception error)
        {
            if (State.Current != AppState.Shift) return;

            var result = Presenter.Shift?.Finish();
            if (result != null) result.FailureReason = Strings.Get("error.shift");

            Session.FinishDay(result);
            State.Go(AppState.Results);
            Ui.Show<ResultsScreen>();
            Ui.Dialog(Strings.Get("error.title"), Strings.Get("error.shift"));
        }
    }
}

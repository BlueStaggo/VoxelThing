using System.Collections.ObjectModel;
using System.Runtime.InteropServices;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using PDS;
using VoxelThing.Client.Gui;
using VoxelThing.Client.Gui.Screens;
using VoxelThing.Client.Rendering;
using VoxelThing.Client.Rendering.Textures;
using VoxelThing.Client.Rendering.Worlds;
using VoxelThing.Client.Settings;
using VoxelThing.Client.Worlds;
using VoxelThing.Game.Blocks;
using VoxelThing.Game.Entities;
using VoxelThing.Game.Utils;
using VoxelThing.Game.Worlds;
using VoxelThing.Game.Worlds.Storage;
using ErrorCode = OpenTK.Graphics.OpenGL.ErrorCode;
using Monitor = OpenTK.Windowing.GraphicsLibraryFramework.Monitor;

namespace VoxelThing.Client;

public class Game : GameWindow
{
    public const string Version = "dev";
    public const string AssetsDirectory = "./assets/";
    public const int DefaultWindowWidth = ScreenDimensions.VirtualWidth * 2;
    public const int DefaultWindowHeight = ScreenDimensions.VirtualHeight * 2;
    public const int TicksPerSecond = 20;
    public const double TickRate = 1.0 / TicksPerSecond;
    public const bool EnableCursorGrab = false;
    public const bool OpenGlDebugging = true;

    public static double TimeElapsed => GLFW.GetTime();

    public static readonly string[] Skins = ["joel", "staggo", "floof", "talon"];

    public readonly Profiler Profiler = new(false);
    public int Fps { get; private set; }
    public double Delta { get; private set; }
    public double PartialTick { get; private set; }
    
    private double tickTime;
    private double fpsTimer;
    private int fpsCounter;

    public ReadOnlyCollection<KeyboardKeyEventArgs> KeysJustPressed => keysJustPressed.AsReadOnly();
    public ReadOnlyCollection<TextInputEventArgs> CharactersJustTyped => charactersJustTyped.AsReadOnly();
    public ReadOnlyCollection<MouseButtonEventArgs> MouseButtonsJustPressed => mouseButtonsJustPressed.AsReadOnly();
    public Vector2 MouseScroll { get; private set; }
    public Vector2 MouseDelta { get; private set; }

    public readonly string SaveDirectory;
    public readonly string WorldDirectory;

    public MainRenderer MainRenderer { get; private set; }
    public readonly SettingsManager Settings;

    public World? World;
    public Player? Player;
    public IPlayerController? PlayerController;
    public bool InWorld => World is not null && Player is not null;
    public BlockRaycastResult SelectionCast { get; private set; } = BlockRaycastResult.NoHit;

    public readonly Block?[] Palette = new Block?[9];
    public int HeldItem;

    public Vector2i ScaledMousePosition => MainRenderer.ScreenDimensions.ScaledMousePosition;

    public Screen? CurrentScreen
    {
        get => currentScreen;
        set
        {
            if (value is null && !InWorld)
                value = new MainMenuScreen(this);

            currentScreen?.OnClosed();
            currentScreen = value;
            CursorState = value is null && EnableCursorGrab ? CursorState.Grabbed : CursorState.Normal;
        }
    }

    private readonly Screen debugScreen;
    private readonly Screen ingameScreen;
    private readonly Screen profilerScreen;
    
    private Screen? currentScreen;
    private bool debugMenu = true;

    private readonly List<KeyboardKeyEventArgs> keysJustPressed = [];
    private readonly List<TextInputEventArgs> charactersJustTyped = [];
    private readonly List<MouseButtonEventArgs> mouseButtonsJustPressed = [];

    public Game() : base(GameWindowSettings.Default, new()
        {
            ClientSize = (DefaultWindowWidth, DefaultWindowHeight),
            Title = "Voxel Thing",
            Flags = ContextFlags.ForwardCompatible,
            Profile = ContextProfile.Core
        })
    {
        KeyDown += keysJustPressed.Add;
        TextInput += charactersJustTyped.Add;
        MouseDown += mouseButtonsJustPressed.Add;
        MouseWheel += args => MouseScroll += args.Offset;
        MouseMove += args => MouseDelta += args.Delta; 
        
        SaveDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "VoxelThing");
        WorldDirectory = Path.Combine(SaveDirectory, "worlds");

        Settings = new();
        Settings.ReadFrom(Path.Combine(SaveDirectory, "settings.dat"));

        MainRenderer = new(this);

        debugScreen = new DebugScreen(this);
        ingameScreen = new IngameScreen(this);
        profilerScreen = new ProfilerScreen(this);

        CurrentScreen = new MainMenuScreen(this);
        
        GL.ClearColor(Color4.Black);
        GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

        if (OpenGlDebugging)
        {
            Console.Error.WriteLine("OpenGL 4.3+ debugging enabled");
            GL.Enable(EnableCap.DebugOutput);
            GL.DebugMessageCallback((source, type, id, severity, length, message, param) =>
            {
                if (severity == DebugSeverity.DebugSeverityNotification) return;
                string fullMessage = $"{severity} {type} from {source} of {(ErrorCode)id}: " +
                                     $"{Marshal.PtrToStringAnsi(new IntPtr(message))}";
                Console.Error.WriteLine(fullMessage);
            }, IntPtr.Zero);
        }
    }
    
#region Frame Events
    protected override void OnUpdateFrame(FrameEventArgs args)
    {
        base.OnUpdateFrame(args);
        
        Profiler.Push("game");
        Profiler.Push("update");
        
        Delta = args.Time;
        tickTime += Delta;

        MainRenderer.ScreenDimensions.ManualScale = Settings.GuiScale;
        MainRenderer.ScreenDimensions.UpdateDimensions();

        Screen? screen = currentScreen ?? (InWorld ? ingameScreen : null);

        if (screen is null)
        {
            screen = CurrentScreen = new MainMenuScreen(this);
            CursorState = CursorState.Normal;
        }

        if (screen == ingameScreen)
            DoControls();
        screen.HandleInput();
        bool paused = screen.PausesWorld;

        if (InWorld)
        {
            if (paused)
                PartialTick = 1.0;
            else
            {
                if (Player is not null)
                {
                    Player.OnGameUpdate();
                    Player.NoClip = IsKeyDown(Keys.Q);
                }
            }
        }

        while (tickTime > TickRate)
        {
            Profiler.Push("tick");
            tickTime -= TickRate;
            currentScreen?.Tick();

            if (InWorld && !paused)
            {
                ingameScreen?.Tick();
                Player?.Tick();
            }
            Profiler.Pop();
        }

        if (!paused)
            PartialTick = tickTime / TickRate;

        if (InWorld && Player is not null)
        {
            Camera camera = MainRenderer.Camera;
            Vector3d playerPosition = Player.Position.GetInterpolatedValue(PartialTick);
            float yaw = (float)Player.Yaw;
            float pitch = (float)Player.Pitch;

            playerPosition.Y += Player.EyeLevel;
            if (Settings.ViewBobbing)
            {
                if (Settings.ThirdPerson)
                    playerPosition.Y -= Player.Velocity.GetInterpolatedValue(PartialTick).Y;
                pitch += (float)Player.FallAmount.GetInterpolatedValue(PartialTick) * 2.5f;
            }

            camera.Position = playerPosition;
            camera.Rotation = (yaw, pitch);

            if (World is not null)
            {
                Profiler.Push("player-raycast");
                SelectionCast = World.DoRaycast(camera.Position, camera.Front, 5.0f);
                Profiler.Pop();
            }

            if (Settings.ThirdPerson)
            {
                camera.Position += camera.Front * -4.0f;
            }
            else if (Settings.ViewBobbing)
            {
                double renderWalk = Player.RenderWalk.GetInterpolatedValue(PartialTick);
                camera.Position.Y += Math.Abs(renderWalk) * 0.1;
                camera.Position += (Vector3d)camera.Right * renderWalk * 0.05;
            }
        }

        Profiler.Pop();
    }
    
    private void DoControls()
    {
        if (Player is null || World is null) return;
        
        if (KeysJustPressed.Any(e => e.Key == Keys.R))
        {
            Player.Position.Value = new(
                World.Random.NextDouble(-1000.0, 1000.0),
                64.0f,
                World.Random.NextDouble(-1000.0, 1000.0)
            );
            Player.Velocity.Value = Vector3d.Zero;
        }

        if (KeysJustPressed.Any(e => e.Key == Keys.F1))
            Settings.HideHud.Value = !Settings.HideHud;

        if (KeysJustPressed.Any(e => e.Key == Keys.F3))
            debugMenu = !debugMenu;

        if (KeysJustPressed.Any(e => e.Key == Keys.F5))
            Settings.ThirdPerson.Value = !Settings.ThirdPerson.Value;

        if (KeysJustPressed.Any(e => e.Key == Keys.Escape))
            CurrentScreen = new PauseScreen(this);

        if (KeysJustPressed.Any(e => e.Key == Keys.E))
            CurrentScreen = new BlockInventory(this);
    }

    protected override void OnRenderFrame(FrameEventArgs args)
    {
        base.OnRenderFrame(args);
        
        Profiler.Push("render");
        
        MainRenderer.Draw();
        
        if (InWorld)
        {
            if (!Settings.HideHud)
            {
                Profiler.Push("draw-hud");
                if (debugMenu)
                    debugScreen.Draw();
                ingameScreen.Draw();
                Profiler.Pop();
            }
        }
        else
        {
            Texture background = MainRenderer.Textures.Get("gui/background.png");
            ScreenDimensions dimensions = MainRenderer.ScreenDimensions;
            
            MainRenderer.Draw2D.DrawQuad(new()
            {
                Size = dimensions.IntSize,
                Texture = background,
                Uv = (
                    0.0f,
                    0.0f,
                    (float)dimensions.IntWidth / background.Width,
                    (float)dimensions.IntHeight / background.Height
                )
            });
        }

        if (CurrentScreen is not null)
        {
            Profiler.Push("draw-gui");
            CurrentScreen.Draw();
            Profiler.Pop();
        }
        
        fpsTimer += Delta;
        fpsCounter++;
        if (fpsTimer >= 1.0)
        {
            fpsTimer %= 1.0;
            Fps = fpsCounter;
            fpsCounter = 0;
        }
        
        Profiler.Pop();
        Profiler.Pop();
        
        if (!Profiler.CompletelyBlockProfiler)
            profilerScreen.Draw();

        SwapBuffers();
        
        keysJustPressed.Clear();
        charactersJustTyped.Clear();
        mouseButtonsJustPressed.Clear();
        MouseScroll = Vector2.Zero;
        MouseDelta = Vector2.Zero;

        VSync = Settings.LimitFps.Value switch
        {
            0 => false,
            1 => true,
            _ => CurrentScreen is not null
        } ? VSyncMode.Adaptive : VSyncMode.Off;
    }

    protected override void OnResize(ResizeEventArgs e)
    {
        base.OnResize(e);
        GL.Viewport(0, 0, e.Width, e.Height);
    }

    protected override void OnUnload()
    {
        ExitWorld();
        MainRenderer.Dispose();
    }
#endregion

    public void StartWorld(string saveName, WorldInfo? worldInfo = null)
    {
        string path = Path.Combine(WorldDirectory, saveName);
        StartWorld(new FolderSaveHandler(path), worldInfo);
    }
    
    public void StartWorld(ISaveHandler saveHandler, WorldInfo? worldInfo = null)
    {
        ExitWorld();

        World = new SingleplayerWorld(this, saveHandler ?? EmptySaveHandler.Instance, worldInfo);
        MainRenderer.WorldRenderer.RefreshRenderers();
        PlayerController = new ClientPlayerController(this);
        Player = new Player(World, PlayerController);

        CompoundItem? playerData = saveHandler?.LoadData("player");
        if (playerData is not null)
            Player.Deserialize(playerData);

        CurrentScreen = null;
        if (EnableCursorGrab)
            CursorState = CursorState.Grabbed;
    }
    
    public void ExitWorld()
    {
        if (World is null)
            return;

        if (Player is not null)
        {
            World.SaveHandler.SaveData("player", (CompoundItem)Player.Serialize());
            Player = null;
        }

        World.Close();
        World = null;
        MainRenderer.WorldRenderer.RefreshRenderers();
        GC.Collect();
    }

    public static void Main()
    {
        using Game game = new();
        game.Run();
    }
}
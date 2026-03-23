// Decompiled with JetBrains decompiler
// Type: SonicOrca.S2HDSonicOrcaGameContext
// Assembly: S2HD, Version=2.0.1012.10521, Culture=neutral, PublicKeyToken=null
// MVID: 18631A0F-16CF-4E18-8563-1EC5E54750D6
// Assembly location: C:\Games\S2HD_2.0.1012-rc2\S2HD.exe

using S2HD;
using SonicOrca.Core;
using SonicOrca.Drawing;
using SonicOrca.Drawing.LevelRendering;
using SonicOrca.Drawing.Renderers;
using SonicOrca.Extensions;
using SonicOrca.Geometry;
using SonicOrca.Graphics;
using SonicOrca.Input;
using SonicOrca.Resources;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;

namespace SonicOrca
{

    internal class S2HDSonicOrcaGameContext(IPlatform platform) : SonicOrcaGameContext(platform)
    {
      private IGameState _rootGameState;
      private Updater _gameStateUpdater;
      private readonly IReadOnlyList<string> CharacterNames = (IReadOnlyList<string>) new string[3]
      {
        "sonic",
        "tails",
        "knuckles"
      };

      public CommandLineArguments CommandLineArguments { get; private set; }

      public LevelGameState CurrentLevelScreen { get; set; }

      public IRendererFactory RenderFactory { get; private set; }

      public S2HDSettings Settings { get; private set; }

      public override IAudioSettings AudioSettings => (IAudioSettings) this.Settings;

      public override IVideoSettings VideoSettings => (IVideoSettings) this.Settings;

      public override void Initialise()
      {
        base.Initialise();
        this.CommandLineArguments = new CommandLineArguments((IEnumerable<string>) Program.CommandLineArguments);
        this.Configuration = Program.Configuration;
        this.UserDataDirectory = Program.UserDataDirectory;
        this._canvas = this.Window.GraphicsContext.CreateFrameBuffer(1920, 1080);
        SonicOrcaGameContext.IsMaxPerformance = this.Configuration.GetPropertyBoolean("graphics", "max_performance");
        this.Audio.Volume = this.Configuration.GetPropertyDouble("audio", "volume", 1.0);
        this.Audio.MusicVolume = this.Configuration.GetPropertyDouble("audio", "music_volume", 0.2);
        this.Audio.SoundVolume = this.Configuration.GetPropertyDouble("audio", "sound_volume", 1.0);
        this.Input.IsVibrationEnabled = this.Configuration.GetPropertyBoolean("input", "vibration", true);
        this.Settings = new S2HDSettings(this.Configuration, this.Audio, this.Window);
        this.Settings.Apply();
        this.Window.WindowTitle = "Sonic 2 HD";
        this.Window.AspectRatio = new Vector2i(16 /*0x10*/, 9);
        string contentRoot = GamePaths.ContentRootDirectory;
        this.LoadResourceFiles(Path.Combine(contentRoot, "data"));
        if (bool.Parse(this.Configuration.GetProperty("general", "use_mods", "true")))
          this.LoadResourceFiles(Path.Combine(contentRoot, "mods"));
        this.RenderFactory = DefaultRendererFactory.Create(this.Window.GraphicsContext);
        this._rootGameState = (IGameState) new RootGameState(this);
        this._gameStateUpdater = new Updater(this._rootGameState.Update());
        this.Begin();
      }

      private void LoadResources(string path)
      {
        foreach (string file in Directory.GetFiles(path, "*.dat", SearchOption.AllDirectories))
          this.ResourceTree.MergeWith(new ResourceFile(file).Scan());
      }

      public override void Dispose()
      {
        this._rootGameState.Dispose();
        base.Dispose();
      }

      private void Begin()
      {
        string str = (string) null;
        LevelPrepareSettings prepareSettings = new LevelPrepareSettings()
        {
          Act = 1,
          Lives = 3
        };
        IReadOnlyList<string> commandLineArguments = Program.CommandLineArguments;
        for (int index = 0; index < ((IReadOnlyCollection<string>) commandLineArguments).Count; ++index)
        {
          bool flag = index == ((IReadOnlyCollection<string>) commandLineArguments).Count - 1;
          string argument = commandLineArguments[index];
          switch (argument.ToLower())
          {
            case "-act":
              if (!flag)
              {
                int result;
                int.TryParse(commandLineArguments[++index], out result);
                prepareSettings.Act = result;
                break;
              }
              break;
            case "-credits":
              str = "credits";
              break;
            case "-debug":
              prepareSettings.Debugging = true;
              break;
            case "-edit":
            case "-editor":
              str = "editor";
              break;
            case "-fullscreen":
              this.Window.FullScreen = true;
              break;
            case "-ghost":
              if (!flag)
              {
                prepareSettings.RecordedPlayGhostReadPath = commandLineArguments[++index];
                break;
              }
              break;
            case "-halfpipe":
              str = "halfpipe";
              break;
            case "-host":
              Trace.WriteLine("Waiting for client");
              this.NetworkManager.Host();
              while (!this.NetworkManager.AllConnected)
                this.NetworkManager.Update();
              break;
            case "-join":
              string serverHost = commandLineArguments[++index];
              Trace.WriteLine("Joining " + serverHost);
              this.NetworkManager.Join(serverHost);
              while (!this.NetworkManager.AllConnected)
                this.NetworkManager.Update();
              break;
            case "-level":
              if (!flag)
              {
                string areaResourceKey = Levels.GetAreaResourceKey(commandLineArguments[++index].ToLower());
                if (areaResourceKey != null)
                {
                  prepareSettings.AreaResourceKey = areaResourceKey;
                  str = "level";
                  break;
                }
                break;
              }
              break;
            case "-lives":
              if (!flag)
              {
                int result;
                int.TryParse(commandLineArguments[++index], out result);
                prepareSettings.Lives = MathX.Clamp(1, result, 99);
                break;
              }
              break;
            case "-night":
              if (!flag)
              {
                double result;
                double.TryParse(commandLineArguments[++index], NumberStyles.Float, (IFormatProvider) CultureInfo.InvariantCulture, out result);
                prepareSettings.NightMode = result;
                break;
              }
              break;
            case "-playback":
              if (!flag)
              {
                prepareSettings.RecordedPlayReadPath = commandLineArguments[++index];
                break;
              }
              break;
            case "-protagonist":
              if (!flag)
              {
                argument = commandLineArguments[++index].ToLower();
                prepareSettings.ProtagonistCharacter = (CharacterType) (this.CharacterNames.IndexOf<string>((Predicate<string>) (x => x.Equals(argument))) + 1);
                break;
              }
              break;
            case "-record":
              if (!flag)
              {
                prepareSettings.RecordedPlayWritePath = commandLineArguments[++index];
                break;
              }
              break;
            case "-sidekick":
              if (!flag)
              {
                argument = commandLineArguments[++index].ToLower();
                prepareSettings.SidekickCharacter = (CharacterType) (this.CharacterNames.IndexOf<string>((Predicate<string>) (x => x.Equals(argument))) + 1);
                break;
              }
              break;
            case "-startpos":
              int result1;
              int result2;
              if (!flag && int.TryParse(commandLineArguments[++index], out result1) && index != ((IReadOnlyCollection<string>) commandLineArguments).Count - 1 && int.TryParse(commandLineArguments[++index], out result2))
              {
                prepareSettings.StartPosition = new Vector2i?(new Vector2i(result1, result2));
                break;
              }
              break;
            case "-timetrial":
              prepareSettings.TimeTrial = true;
              break;
            case "-window":
              this.Window.FullScreen = false;
              break;
          }
        }
        switch (str)
        {
          case "credits":
            CreditsGameState creditsGameState = new CreditsGameState((SonicOrcaGameContext) this);
            break;
          case "level":
            new StoryPlaythroughGameState(this).StartFrom(prepareSettings);
            break;
          case "halfpipe":
            break;
          default:
            ((IEnumerable<string>) Program.CommandLineArguments).Contains<string>("-soundtrack");
            break;
        }
      }

      protected override void OnUpdate()
      {
        if (!this.Input.CurrentState.Keyboard[226] && !this.Input.CurrentState.Keyboard[230] || !this.Input.Pressed.Keyboard[40])
          return;
        this.Window.FullScreen = !this.Window.FullScreen;
      }

      protected override void OnUpdateStep()
      {
        this.Console.Update();
        this.NetworkManager.Update();
        if (!this._gameStateUpdater.Update())
          this.Finish = true;
        foreach (Controller controller in this.Controllers)
          controller.Update();
        this.Input.OutputState.GamePad = this.Output.ToArray<GamePadOutputState>();
      }

      protected override void OnDraw()
      {
        I2dRenderer obj1 = this.Renderer.Get2dRenderer();
        obj1.ClipRectangle = new Rectangle(0.0, 0.0, 1920.0, 1080.0);
        if (this.ForceHD)
          this._canvas.Activate();
        else
          this.Window.GraphicsContext.RenderToBackBuffer();
        this.Window.GraphicsContext.ClearBuffer();
        this._rootGameState.Draw();
        this.Renderer.DeativateRenderer();
        this.Console.Draw(this.Renderer);
        this.Renderer.DeativateRenderer();
        if (SonicOrcaGameContext.Singleton.Input.Pressed.Keyboard[67])
          ScreenshotRenderer.GrabScreenshot();
        if (!this.ForceHD)
          return;
        this.Window.GraphicsContext.RenderToBackBuffer();
        obj1.BlendMode = BlendMode.Opaque;
        obj1.Colour = Colours.White;
        I2dRenderer obj2 = obj1;
        Vector2i clientSize = this.Window.ClientSize;
        double x1 = (double) clientSize.X;
        clientSize = this.Window.ClientSize;
        double y1 = (double) clientSize.Y;
        Rectangle rectangle = new Rectangle(0.0, 0.0, x1, y1);
        obj2.ClipRectangle = rectangle;
        I2dRenderer obj3 = obj1;
        ITexture texture = this._canvas.Textures[0];
        clientSize = this.Window.ClientSize;
        double x2 = (double) clientSize.X;
        clientSize = this.Window.ClientSize;
        double y2 = (double) clientSize.Y;
        Rectangle destination = new Rectangle(0.0, 0.0, x2, y2);
        obj3.RenderTexture(texture, destination, flipy: true);
        obj1.Deactivate();
      }

      private void CreateDataResourceFiles(string inputDirectory, string outputDirectory)
      {
        if (!Directory.Exists(inputDirectory))
          return;
        if (!Directory.Exists(outputDirectory))
          Directory.CreateDirectory(outputDirectory);
        foreach (string directory in Directory.GetDirectories(inputDirectory))
        {
          string path2 = Path.GetFileName(directory) + ".dat";
          string path = Path.Combine(directory, "sonicorca");
          ResourceTree tree = new ResourceTree();
          ResourceFile.GetResourcesFromDirectory(tree, path);
          new ResourceFile(Path.Combine(outputDirectory, path2)).Write(tree);
        }
      }

      private void LoadResourceFiles(string inputDirectory)
      {
        if (!Directory.Exists(inputDirectory))
          return;
        foreach (string file in Directory.GetFiles(inputDirectory, "*.dat", SearchOption.AllDirectories))
          this.ResourceTree.MergeWith(new ResourceFile(file).Scan());
      }

      protected override Renderer CreateRenderer() => (Renderer) new TheRenderer(this.Window);

      protected override ILevelRenderer CreateLevelRenderer(Level level)
      {
        return (ILevelRenderer) new LevelRenderer(level, this.VideoSettings);
      }
    }
}

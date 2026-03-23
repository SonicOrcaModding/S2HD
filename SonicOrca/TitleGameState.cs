// Decompiled with JetBrains decompiler
// Type: SonicOrca.TitleGameState
// Assembly: S2HD, Version=2.0.1012.10521, Culture=neutral, PublicKeyToken=null
// MVID: 18631A0F-16CF-4E18-8563-1EC5E54750D6
// Assembly location: C:\Games\S2HD_2.0.1012-rc2\S2HD.exe

using S2HD;
using S2HD.Title;
using SonicOrca.Audio;
using SonicOrca.Core;
using SonicOrca.Geometry;
using SonicOrca.Graphics;
using SonicOrca.Resources;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SonicOrca
{

    internal class TitleGameState : IGameState, IDisposable
    {
      private static readonly EaseTimeline IntroTextOpacity = new EaseTimeline(new EaseTimeline.Entry[4]
      {
        new EaseTimeline.Entry(32 /*0x20*/, 0.0),
        new EaseTimeline.Entry(64 /*0x40*/, 1.0),
        new EaseTimeline.Entry(144 /*0x90*/, 1.0),
        new EaseTimeline.Entry(176 /*0xB0*/, 0.0)
      });
      private readonly S2HDSonicOrcaGameContext _gameContext;
      private ResourceSession _resourceSession;
      private Font _font;
      private AnimationGroup _animationGroup;
      private Sample _sparkleSample;
      private Sample _shootingStarSample;
      private Sample _musicSample;
      private SampleInstance _musicInstance;
      private IReadOnlyCollection<Tuple<int, Vector2i>> SparkleTable = (IReadOnlyCollection<Tuple<int, Vector2i>>) new Tuple<int, Vector2i>[4]
      {
        new Tuple<int, Vector2i>(204, new Vector2i(-126, -320)),
        new Tuple<int, Vector2i>(220, new Vector2i(-280, 366)),
        new Tuple<int, Vector2i>(236, new Vector2i(324, -4)),
        new Tuple<int, Vector2i>(252, new Vector2i(80 /*0x50*/, 168))
      };
      private const int BannerStartTime = 332;
      private int _ticks;
      private Vector2i _sparklePosition;
      private AnimationInstance _sparkleAnimationInstance;
      private float _fadeOutOpacity;
      private bool _fadingOut;
      private AnimationInstance _shootingStarAnimationInstance;
      private Vector2 _shootingStarPosition;
      private Vector2[] vertexPositions = new Vector2[4];
      private Vector2[] vertexUVs = new Vector2[4];
      private IMaskRenderer _maskRenderer;
      private bool _loaded;
      private Task _loadingTask;
      private CancellationTokenSource _loadingCTS;
      private Banner _banner;
      private UserInterface _userInterface;
      private string _versionText;

      public Background Background { get; private set; }

      public LevelPrepareSettings LevelPrepareSettings { get; set; }

      public TitleGameState.ResultType Result { get; set; }

      public TitleGameState(S2HDSonicOrcaGameContext gameContext)
      {
        this._gameContext = gameContext;
        Version appVersion = Program.AppVersion;
        this._versionText = $"Version {appVersion.Major}.{appVersion.Minor} Demo Build {appVersion.Build} | Decomp by dotfla_";
      }

      private void LoadResources()
      {
        this._resourceSession = new ResourceSession(this._gameContext.ResourceTree);
        this._resourceSession.PushDependencies(TitleResources.AllResourceKeys);
        this._loadingCTS = new CancellationTokenSource();
        this._loadingTask = this._resourceSession.LoadAsync(this._loadingCTS.Token);
      }

      private void GetResourceReferences()
      {
        ResourceTree resourceTree = this._gameContext.ResourceTree;
        this._font = resourceTree.GetLoadedResource<Font>("SONICORCA/FONTS/HUD");
        this._animationGroup = resourceTree.GetLoadedResource<AnimationGroup>("SONICORCA/TITLE/ANIGROUP");
        this._sparkleSample = resourceTree.GetLoadedResource<Sample>("SONICORCA/SOUND/SPARKLE");
        this._shootingStarSample = resourceTree.GetLoadedResource<Sample>("SONICORCA/SOUND/SHOOTINGSTAR");
        this._musicSample = resourceTree.GetLoadedResource<Sample>("SONICORCA/TITLE/MUSIC");
      }

      public void Dispose() => this._resourceSession.Dispose();

      public IEnumerable<UpdateResult> Update()
      {
        TitleGameState titleGameState = this;
        titleGameState._maskRenderer = titleGameState._gameContext.Renderer.GetMaskRenderer();
        titleGameState.LoadResources();
        while (!titleGameState._loadingTask.IsCompleted)
          yield return UpdateResult.Next();
        if (!titleGameState._loadingTask.IsFaulted)
        {
          titleGameState.GetResourceReferences();
          titleGameState._loaded = true;
          titleGameState.Background = new Background(titleGameState._gameContext);
          titleGameState._banner = new Banner(titleGameState._gameContext, titleGameState._maskRenderer);
          titleGameState._userInterface = new UserInterface(titleGameState._gameContext, titleGameState, titleGameState._maskRenderer);
          titleGameState.RestartEvents();
          while (true)
          {
            if (titleGameState._fadingOut)
            {
              if ((double) titleGameState._fadeOutOpacity > 0.0)
              {
                titleGameState._fadeOutOpacity -= 0.0166666675f;
                titleGameState._musicInstance.Volume = (double) titleGameState._fadeOutOpacity;
              }
              else
                break;
            }
            if (titleGameState._ticks == 268)
            {
              titleGameState._musicInstance = new SampleInstance((SonicOrcaGameContext) titleGameState._gameContext, titleGameState._musicSample);
              titleGameState._musicInstance.Play();
            }
            if (titleGameState._ticks == 458)
            {
              titleGameState.Background.Visible = true;
              titleGameState._banner.ShowStarLensFare = true;
              titleGameState._userInterface.Visible = true;
            }
            titleGameState._banner.Update();
            if (titleGameState.Background.Visible)
              titleGameState.Background.Update();
            titleGameState._userInterface.Update();
            if (titleGameState._ticks >= 662)
            {
              if (titleGameState._shootingStarAnimationInstance == null)
              {
                titleGameState._shootingStarAnimationInstance = new AnimationInstance(titleGameState._animationGroup, 9);
                titleGameState._shootingStarAnimationInstance.AdditiveBlending = true;
                titleGameState._shootingStarPosition = new Vector2(1440.0, 0.0);
                titleGameState._gameContext.Audio.PlaySound(titleGameState._shootingStarSample);
              }
              titleGameState._shootingStarAnimationInstance.Animate();
              titleGameState._shootingStarPosition += new Vector2(-16.0, 8.0);
            }
            titleGameState.UpdateSparkle();
            ++titleGameState._ticks;
            yield return UpdateResult.Next();
          }
          titleGameState._gameContext.Audio.StopAll();
        }
      }

      private void RestartEvents()
      {
        this._ticks = 0;
        this._sparkleAnimationInstance = (AnimationInstance) null;
        this._fadeOutOpacity = 1f;
        this._fadingOut = false;
        this.Background.Reset();
        this._banner.Reset();
        this._userInterface.Reset();
        this._gameContext.Audio.StopAll();
      }

      public void FadeOut()
      {
        this._fadingOut = true;
        this._banner.DoShine(210);
      }

      private void CreateSparkle(Vector2i position)
      {
        this._sparkleAnimationInstance = new AnimationInstance(this._animationGroup, 8);
        this._sparkleAnimationInstance.AdditiveBlending = true;
        this._sparklePosition = position;
        this._gameContext.Audio.PlaySound(this._sparkleSample);
      }

      private void UpdateSparkle()
      {
        foreach (Tuple<int, Vector2i> tuple in (IEnumerable<Tuple<int, Vector2i>>) this.SparkleTable)
        {
          if (tuple.Item1 == this._ticks)
            this.CreateSparkle(this._banner.Position + tuple.Item2);
        }
        if (this._sparkleAnimationInstance == null)
          return;
        this._sparkleAnimationInstance.Animate();
      }

      public void Draw()
      {
        if (!this._loaded)
          return;
        Renderer renderer = this._gameContext.Renderer;
        this.DrawIntroText(renderer);
        this.Background.Draw(renderer);
        this.DrawShootingStar(renderer);
        this._banner.Draw(renderer);
        this.DrawSparkle(renderer);
        this._userInterface.Draw(renderer);
        if (this.Background.Visible)
          this.DrawVersion(renderer);
        if ((double) this._fadeOutOpacity != 1.0)
        {
          renderer.DeativateRenderer();
          IFadeTransitionRenderer fadeTransition = SharedRenderers.FadeTransition;
          fadeTransition.Opacity = this._fadeOutOpacity - 1f;
          fadeTransition.Render();
        }
        this._banner.DrawUnfaded(renderer);
      }

      private void DrawIntroText(Renderer renderer)
      {
        double valueAt = TitleGameState.IntroTextOpacity.GetValueAt(this._ticks);
        if (valueAt <= 0.0)
          return;
        IReadOnlyList<string> stringList = (IReadOnlyList<string>) new string[4]
        {
          "SONIC",
          "AND",
          "MILES \"TAILS\" PROWER",
          "IN"
        };
        int y = 540 - ((IReadOnlyCollection<string>) stringList).Count * 128 /*0x80*/ / 2;
        Colour colour = new Colour(valueAt, 1.0, 1.0, 1.0);
        IFontRenderer fontRenderer = renderer.GetFontRenderer();
        foreach (string text in (IEnumerable<string>) stringList)
        {
          fontRenderer.RenderStringWithShadow(text, new Rectangle(0.0, (double) y, 1920.0, 0.0), FontAlignment.Centre, this._font, colour, new int?(0));
          y += 128 /*0x80*/;
        }
      }

      private void DrawSparkle(Renderer renderer)
      {
        I2dRenderer renderer1 = renderer.Get2dRenderer();
        if (this._sparkleAnimationInstance == null || this._sparkleAnimationInstance.Cycles != 0)
          return;
        this._sparkleAnimationInstance.Draw(renderer1, (Vector2) this._sparklePosition);
      }

      private void DrawShootingStar(Renderer renderer)
      {
        I2dRenderer renderer1 = renderer.Get2dRenderer();
        if (this._shootingStarAnimationInstance == null)
          return;
        this._shootingStarAnimationInstance.Draw(renderer1, this._shootingStarPosition);
      }

      private void DrawVersion(Renderer renderer)
      {
        I2dRenderer obj = renderer.Get2dRenderer();
        IFontRenderer fontRenderer = renderer.GetFontRenderer();
        Colour colour = new Colour((double) this._fadeOutOpacity / 2.0, 1.0, 1.0, 1.0);
        using (obj.BeginMatixState())
        {
          obj.ModelMatrix *= Matrix4.CreateTranslation(8.0, 1072.0);
          obj.ModelMatrix *= Matrix4.CreateScale(0.5, 0.5);
          Rectangle boundary = new Rectangle(0.0, 0.0, 0.0, 0.0);
          fontRenderer.RenderStringWithShadow(this._versionText.ToUpper(), boundary, FontAlignment.Bottom, this._font, colour, new int?(0));
        }
      }

      public enum ResultType
      {
        NewGame,
        LevelSelect,
        ShowOptions,
        ShowAchievements,
        StartDemo,
        Quit,
      }
    }
}

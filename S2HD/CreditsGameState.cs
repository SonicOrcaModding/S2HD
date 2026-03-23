// Decompiled with JetBrains decompiler
// Type: S2HD.CreditsGameState
// Assembly: S2HD, Version=2.0.1012.10521, Culture=neutral, PublicKeyToken=null
// MVID: 18631A0F-16CF-4E18-8563-1EC5E54750D6
// Assembly location: C:\Games\S2HD_2.0.1012-rc2\S2HD.exe

using SonicOrca;
using SonicOrca.Audio;
using SonicOrca.Extensions;
using SonicOrca.Geometry;
using SonicOrca.Graphics;
using SonicOrca.Graphics.V2.Video;
using SonicOrca.Input;
using SonicOrca.Resources;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace S2HD
{

    internal class CreditsGameState : IGameState, IDisposable
    {
      private readonly SonicOrcaGameContext _gameContext;
      private bool _loaded;
      private Task _loadingTask;
      private CancellationTokenSource _loadingCTS;
      private ResourceSession _resourceSession;
      [ResourcePath("SONICORCA/MUSIC/CREDITS")]
      private Sample _musicSample;
      [ResourcePath("SONICORCA/SOUND/SHOOTINGSTAR")]
      private Sample _shootingStarSample;
      [ResourcePath("SONICORCA/CREDITS/SEQUENCE/GROUP")]
      private FilmGroup _creditsFilmGroup;
      private FilmInstance _creditsFilmInstance;
      private SampleInstance _musicInstance;
      private double _fadeOpacity;

      public CreditsGameState(SonicOrcaGameContext gameContext)
      {
        this._gameContext = gameContext;
        this._resourceSession = new ResourceSession(gameContext.ResourceTree);
      }

      private void LoadResources()
      {
        this._resourceSession = new ResourceSession(this._gameContext.ResourceTree);
        this._resourceSession.PushDependenciesByAttribute((object) this);
        this._loadingCTS = new CancellationTokenSource();
        this._loadingTask = this._resourceSession.LoadAsync(this._loadingCTS.Token);
      }

      public void Dispose()
      {
        this._musicInstance?.Dispose();
        this._musicInstance = (SampleInstance) null;
        if (this._loadingTask != null && !this._loadingTask.IsCompleted)
        {
          this._loadingCTS.Cancel();
          this._loadingTask.Wait();
        }
        if (this._resourceSession == null)
          return;
        this._resourceSession.Dispose();
        this._resourceSession = (ResourceSession) null;
      }

      public IEnumerable<UpdateResult> Update()
      {
        CreditsGameState instance = this;
        instance.LoadResources();
        while (!instance._loadingTask.IsCompleted)
          yield return UpdateResult.Next();
        if (!instance._loadingTask.IsFaulted)
        {
          instance._gameContext.ResourceTree.FullfillLoadedResourcesByAttribute((object) instance);
          instance._loaded = true;
#if __ANDROID__
          instance._creditsFilmInstance = null;
          yield break;
#else
          instance._creditsFilmInstance = new FilmInstance(instance._creditsFilmGroup);
          instance._musicInstance = new SampleInstance(instance._gameContext, instance._musicSample, new int?(458980));
          instance._fadeOpacity = 0.0;
          bool playedStarSFX = false;
          bool quit = false;
          while (!instance._creditsFilmInstance.Finished)
          {
            double currentTime = instance._creditsFilmInstance.CurrentTime;
            if (currentTime < 42.0)
            {
              InputState pressed = instance._gameContext.Input.Pressed;
              if (pressed.Keyboard[40] || pressed.GamePad[0].Start)
                quit = true;
            }
            if (quit)
            {
              instance._musicInstance.Volume -= 1.0 / 120.0;
              instance._fadeOpacity += 1.0 / 120.0;
              if (instance._fadeOpacity >= 1.0)
                break;
            }
            if (!instance._musicInstance.Playing && currentTime >= 2.0)
              instance._musicInstance.Play();
            if (currentTime >= 45.0)
            {
              double num = 1.0 - Math.Min((currentTime - 45.0) / 8.0, 1.0);
              instance._musicInstance.Volume = num;
            }
            if (!playedStarSFX && currentTime >= 52.5)
            {
              playedStarSFX = true;
              instance._gameContext.Audio.PlaySound(instance._shootingStarSample);
            }
            instance._creditsFilmInstance.Animate();
            yield return UpdateResult.Next();
          }
#endif
        }
      }

      public void Draw()
      {
        if (!this._loaded)
          return;
#if __ANDROID__
        if (this._creditsFilmInstance == null)
          return;
#endif
        Renderer renderer = this._gameContext.Renderer;
        this._creditsFilmInstance.Draw(renderer);
        if (this._fadeOpacity >= 1.0)
          return;
        I2dRenderer obj = renderer.Get2dRenderer();
        Colour colour = new Colour(this._fadeOpacity, 0.0, 0.0, 0.0);
        obj.BlendMode = BlendMode.Alpha;
        obj.RenderQuad(colour, new Rectangle(0.0, 0.0, 1920.0, 1080.0));
      }

      private static class ResourceKeys
      {
        public const string RoleFont = "SONICORCA/FONTS/TITLE/S2/NAME";
        public const string NameFont = "SONICORCA/FONTS/HUD";
        public const string Logo = "SONICORCA/LOGO";
        public const string Music = "SONICORCA/MUSIC/CREDITS";
        public const string ShootingStarSample = "SONICORCA/SOUND/SHOOTINGSTAR";
      }
    }
}

// Decompiled with JetBrains decompiler
// Type: S2HD.Menu.OptionsGameState
// Assembly: S2HD, Version=2.0.1012.10521, Culture=neutral, PublicKeyToken=null
// MVID: 18631A0F-16CF-4E18-8563-1EC5E54750D6
// Assembly location: C:\Games\S2HD_2.0.1012-rc2\S2HD.exe

using SonicOrca;
using SonicOrca.Audio;
using SonicOrca.Extensions;
using SonicOrca.Geometry;
using SonicOrca.Graphics;
using SonicOrca.Resources;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;

namespace S2HD.Menu;

internal class OptionsGameState : IGameState, IDisposable
{
  [ResourcePath("SONICORCA/MUSIC/OPTIONS")]
  private SampleInfo _musicSampleInfo;
  [ResourcePath("SONICORCA/MENU/OPTIONS/MENU3")]
  private ITexture _backgroundTexture;
  private const int FadeTime = 60;
  private readonly S2HDSonicOrcaGameContext _gameContext;
  private readonly IGraphicsContext _graphicsContext;
  private readonly SettingUIResources _settingUIResources = new SettingUIResources();
  private readonly MenuViewPresenterFactory _viewPresenterFactory;
  private readonly MenuViewPresenterHost _viewPresenterHost;
  private readonly ButtonBarUI _buttonBarUI;
  private ResourceSession _resourceSession;
  private bool _initialised;
  private bool _finished;
  private SampleInstance _musicSampleInstance;
  private Rectanglei _bounds;
  private float _fadeOutOpacity;

  public OptionsGameState(S2HDSonicOrcaGameContext gameContext)
  {
    this._gameContext = gameContext;
    this._graphicsContext = this._gameContext.Window.GraphicsContext;
    this._viewPresenterFactory = new MenuViewPresenterFactory(this._gameContext, (ISettingUIResources) this._settingUIResources);
    this._viewPresenterHost = new MenuViewPresenterHost(this._viewPresenterFactory, new MenuViewFactory(this._gameContext).GetOptionsView(), (ISettingUIResources) this._settingUIResources);
    this._viewPresenterHost.NavigateNext += new EventHandler<NavigateNextEventArgs>(this.NavigateNextHandler);
    this._viewPresenterHost.NavigateBack += new EventHandler(this.NavigateBackHandler);
    this._buttonBarUI = new ButtonBarUI((IControlResources) this._settingUIResources);
  }

  public void Dispose()
  {
    this._musicSampleInstance?.Dispose();
    this._resourceSession?.Dispose();
  }

  public async Task LoadAsync(CancellationToken ct = default (CancellationToken))
  {
    OptionsGameState instance = this;
    ResourceTree resourceTree = instance._gameContext.ResourceTree;
    instance._resourceSession = new ResourceSession(resourceTree);
    instance._resourceSession.PushDependenciesByAttribute((object) instance);
    instance._settingUIResources.PushDependencies(instance._resourceSession);
    await instance._resourceSession.LoadAsync(ct);
    resourceTree.FullfillLoadedResourcesByAttribute((object) instance);
    instance._settingUIResources.FetchResources(resourceTree);
  }

  private void Initialise()
  {
    this._backgroundTexture.Wrapping = TextureWrapping.Repeat;
    this._bounds = new Rectanglei(0, 0, 1920, 1080);
    this._fadeOutOpacity = 0.0f;
    this._viewPresenterHost.Bounds = this._bounds.Inflate(new Vector2i(-512, -386));
    this._buttonBarUI.Bounds = new Rectanglei(0, this._bounds.Height - 100, this._bounds.Width, 64 /*0x40*/);
    this._buttonBarUI.Items = ((IEnumerable<ButtonBarItem>) new ButtonBarItem[2]
    {
      new ButtonBarItem()
      {
        Button = GamePadButton.A,
        Text = "APPLY"
      },
      new ButtonBarItem()
      {
        Button = GamePadButton.B,
        Text = "CANCEL"
      }
    }).ToImmutableArray<ButtonBarItem>();
    this._initialised = true;
  }

  public IEnumerable<UpdateResult> Update()
  {
    Task loadTask = this.LoadAsync();
    while (!loadTask.IsCompleted)
      yield return UpdateResult.Next();
    if (!loadTask.IsFaulted)
    {
      this.Initialise();
      this._musicSampleInstance = new SampleInstance(this._gameContext.Audio, this._musicSampleInfo);
      this._musicSampleInstance.Classification = SampleInstanceClassification.Music;
      this._musicSampleInstance.Play();
      while ((double) this._fadeOutOpacity < 1.0)
      {
        this._fadeOutOpacity += 0.0166666675f;
        this._fadeOutOpacity = Math.Min(this._fadeOutOpacity, 1f);
        this._viewPresenterHost.Update();
        yield return UpdateResult.Next();
      }
      while (!this._finished)
      {
        this._viewPresenterHost.Update();
        this._viewPresenterHost.HandleInput();
#if __ANDROID__
        AndroidOptionsMenuTouchInput.ProcessFrame(
          this._gameContext,
          out int touchH,
          out int touchV,
          out bool touchTap);
        if (touchTap && this._gameContext.Input.Released.Mouse.Left)
        {
          if (AndroidOptionsTouchBackButton.TryWindowPixelsToLogical(
            (SonicOrcaGameContext) this._gameContext,
            this._gameContext.Input.Released.Mouse.X,
            this._gameContext.Input.Released.Mouse.Y,
            out double lx,
            out double ly))
          {
            Rectangle backRect = AndroidOptionsTouchBackButton.GetBackButtonRect(
              AndroidOptionsTouchBackButton.GetLogicalViewport((SonicOrcaGameContext) this._gameContext));
            if (lx >= backRect.X && lx < backRect.Right && ly >= backRect.Y && ly < backRect.Bottom)
              touchTap = false;
          }
        }
        this._viewPresenterHost.ApplyAndroidSwipeAndTap(touchH, touchV, touchTap);
#endif
        AndroidOptionsTouchBackButton.TryHandleReleasedTap((SonicOrcaGameContext) this._gameContext, this._viewPresenterHost);
        yield return UpdateResult.Next();
      }
      while ((double) this._fadeOutOpacity > 0.0)
      {
        this._musicSampleInstance.Volume -= 0.01666666753590107;
        this._fadeOutOpacity -= 0.0166666675f;
        this._fadeOutOpacity = Math.Min(this._fadeOutOpacity, 1f);
        this._viewPresenterHost.Update();
        yield return UpdateResult.Next();
      }
      this._musicSampleInstance.Stop();
    }
  }

  private void NavigateNextHandler(object sender, NavigateNextEventArgs e)
  {
    if (!(e.Tag is int) || (int) e.Tag != 4)
      return;
    this._gameContext.Settings.Apply();
  }

  private void NavigateBackHandler(object sender, EventArgs e) => this._finished = true;

  public void Draw()
  {
    if (!this._initialised)
      return;
    Renderer renderer = this._gameContext.Renderer;
    this.DrawBackground(renderer);
    this.DrawViewPresenterHost(renderer);
    AndroidOptionsTouchBackButton.DrawBackButton(renderer, (SonicOrcaGameContext) this._gameContext);
    this.DrawButtonBar(renderer);
    this.DrawFadeOut(renderer);
  }

  private void DrawButtonBar(Renderer renderer) => this._buttonBarUI.Draw(renderer);

  private void DrawBackground(Renderer renderer)
  {
    I2dRenderer obj = renderer.Get2dRenderer();
    obj.BlendMode = BlendMode.Opaque;
    obj.Colour = Colours.White;
    obj.ModelMatrix = Matrix4.Identity;
    obj.RenderTexture(this._backgroundTexture, (Rectangle) this._bounds, (Rectangle) this._bounds);
  }

  private void DrawViewPresenterHost(Renderer renderer) => this._viewPresenterHost.Draw(renderer);

  private void DrawFadeOut(Renderer renderer)
  {
    renderer.DeativateRenderer();
    IFadeTransitionRenderer fadeTransition = SharedRenderers.FadeTransition;
    fadeTransition.Opacity = this._fadeOutOpacity - 1f;
    fadeTransition.Render();
  }
}

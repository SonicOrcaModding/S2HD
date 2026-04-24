// Decompiled with JetBrains decompiler
// Type: S2HD.Menu.MenuViewPresenterHost
// Assembly: S2HD, Version=2.0.1012.10521, Culture=neutral, PublicKeyToken=null
// MVID: 18631A0F-16CF-4E18-8563-1EC5E54750D6
// Assembly location: C:\Games\S2HD_2.0.1012-rc2\S2HD.exe

using SonicOrca;
using SonicOrca.Geometry;
using SonicOrca.Graphics;
using System;
using System.Collections.Generic;

namespace S2HD.Menu;

internal class MenuViewPresenterHost
{
  private readonly Stack<IMenuViewModel> _viewModelStack = new Stack<IMenuViewModel>();
  private readonly MenuViewPresenterFactory _viewPresenterFactory;
  private readonly ISettingUIResources _settingUIResources;
  private IMenuViewPresenter _viewPresenter;
  private IMenuViewModel _viewModel;
  private Rectanglei _bounds;

  public event EventHandler NavigateBack;

  public event EventHandler<NavigateNextEventArgs> NavigateNext;

  public Rectanglei Bounds
  {
    get => this._bounds;
    set
    {
      this._bounds = value;
      if (this._viewPresenter == null)
        return;
      this._viewPresenter.Bounds = value;
    }
  }

  public object CurrentViewModelTag => this._viewModel.Tag;

  public MenuViewPresenterHost(
    MenuViewPresenterFactory viewPresenterFactory,
    IMenuViewModel initialViewModel,
    ISettingUIResources settingUIResources)
  {
    this._viewPresenterFactory = viewPresenterFactory;
    this._settingUIResources = settingUIResources;
    this.SetViewModel(initialViewModel);
  }

  public void Update() => this._viewPresenter?.Update();

  public void HandleInput() => this._viewPresenter?.HandleInput();

  public void RequestNavigateBack() => this.NavigateBackHandler((object) this, EventArgs.Empty);

  private void SetViewModel(IMenuViewModel viewModel)
  {
    if (this._viewModel != null)
      this.PlayConfirmSound();
    this._viewModel = viewModel;
    this._viewPresenter = this._viewPresenterFactory.Create(this._viewModel);
    this._viewPresenter.Bounds = this._bounds;
    this._viewPresenter.NavigateBack += new EventHandler(this.NavigateBackHandler);
    this._viewPresenter.NavigateNext += (EventHandler<NavigateNextEventArgs>) ((s, e) => this.NavigateNextHandler(s, e));
    this._viewPresenter.Update();
  }

  private void NavigateBackHandler(object sender, EventArgs e)
  {
    if (this._viewModelStack.Count == 0)
    {
      EventHandler navigateBack = this.NavigateBack;
      if (navigateBack == null)
        return;
      navigateBack((object) this, EventArgs.Empty);
    }
    else
    {
      this.SetViewModel(this._viewModelStack.Pop());
      if (!(this.NavigateBack.Target.GetType() != typeof (OptionsGameState)))
        return;
      EventHandler navigateBack = this.NavigateBack;
      if (navigateBack == null)
        return;
      navigateBack(sender, EventArgs.Empty);
    }
  }

  private void NavigateNextHandler(object s, NavigateNextEventArgs e)
  {
    if (e.ViewModel != null)
    {
      this._viewModelStack.Push(this._viewModel);
      this.SetViewModel(e.ViewModel);
    }
    if (e.Tag != null && e.Tag.Equals((object) 0))
      this.NavigateBackHandler(s, (EventArgs) e);
    else if (e.ViewModel != null && e.Tag == null)
    {
      EventHandler<NavigateNextEventArgs> navigateNext = this.NavigateNext;
      if (navigateNext == null)
        return;
      navigateNext(s, new NavigateNextEventArgs(e.ViewModel, e.ViewModel.Tag));
    }
    else
    {
      EventHandler<NavigateNextEventArgs> navigateNext = this.NavigateNext;
      if (navigateNext == null)
        return;
      navigateNext(s, e);
    }
  }

  private void PlayNavigationSound()
  {
    if (this._settingUIResources.NavigateSample == null)
      return;
    SonicOrcaGameContext.Singleton.Audio.PlaySound(this._settingUIResources.NavigateSample);
  }

  private void PlayConfirmSound()
  {
    if (this._settingUIResources.NavigateSample == null)
      return;
    SonicOrcaGameContext.Singleton.Audio.PlaySound(this._settingUIResources.ConfirmSample);
  }

  private void PlayBackSound()
  {
    if (this._settingUIResources.NavigateSample == null)
      return;
    SonicOrcaGameContext.Singleton.Audio.PlaySound(this._settingUIResources.CancelSample);
  }

  public void Draw(Renderer renderer) => this._viewPresenter?.Draw(renderer);
}

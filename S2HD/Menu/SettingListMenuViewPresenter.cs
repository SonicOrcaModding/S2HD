// Decompiled with JetBrains decompiler
// Type: S2HD.Menu.SettingListMenuViewPresenter
// Assembly: S2HD, Version=2.0.1012.10521, Culture=neutral, PublicKeyToken=null
// MVID: 18631A0F-16CF-4E18-8563-1EC5E54750D6
// Assembly location: C:\Games\S2HD_2.0.1012-rc2\S2HD.exe

using SonicOrca;
using SonicOrca.Audio;
using SonicOrca.Geometry;
using SonicOrca.Graphics;
using SonicOrca.Input;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace S2HD.Menu;

internal class SettingListMenuViewPresenter : IMenuViewPresenter
{
  private const int Margin = 16 /*0x10*/;
  private const int ItemHeight = 64 /*0x40*/;
  private readonly S2HDSonicOrcaGameContext _gameContext;
  private readonly AudioContext _audioContext;
  private readonly ISettingUIResources _resources;
  private readonly ImmutableArray<ISetting> _settings;
  private readonly ISettingListMenuViewModel _viewModel;
  private readonly List<SettingUI> _optionSettingUIs = new List<SettingUI>();
  private Rectanglei _bounds;
  private bool _layoutDirty = true;
  private int _selectedIndex;
  private bool _tapUp;
  private bool _tapDown;
  private bool _tapLeft;
  private bool _tapRight;

  public event EventHandler NavigateBack;

  public event EventHandler<NavigateNextEventArgs> NavigateNext;

  public Rectanglei Bounds
  {
    get => this._bounds;
    set
    {
      this._bounds = value;
      this._layoutDirty = true;
    }
  }

  public SettingListMenuViewPresenter(
    S2HDSonicOrcaGameContext gameContext,
    ISettingUIResources resources,
    ISettingListMenuViewModel viewModel)
  {
    this._gameContext = gameContext;
    this._audioContext = this._gameContext.Audio;
    this._resources = resources;
    this._viewModel = viewModel;
    this._settings = viewModel.Items;
    this.Layout();
  }

  private void Layout()
  {
    int x = this.Bounds.Left + 16 /*0x10*/;
    int width = this.Bounds.Width - 32 /*0x20*/;
    int y = this.Bounds.Top + 16 /*0x10*/;
    this._optionSettingUIs.Clear();
    foreach (ISetting setting in this._settings)
    {
      this._optionSettingUIs.Add(new SettingUI(setting, this._resources)
      {
        Bounds = new Rectanglei(x, y, width, 64 /*0x40*/),
        Highlighted = false
      });
      y += 64 /*0x40*/;
    }
    if (this._optionSettingUIs.Count > 0)
    {
      this._selectedIndex = this._selectedIndex != -1 ? Math.Min(this._selectedIndex, this._optionSettingUIs.Count - 1) : 0;
      this._optionSettingUIs[this._selectedIndex].Highlighted = true;
    }
    else
      this._selectedIndex = -1;
    this._layoutDirty = false;
  }

  public void Update()
  {
    if (!this._layoutDirty)
      return;
    this.Layout();
  }

  public void HandleInput()
  {
    InputContext input = this._gameContext.Input;
    InputState pressed = this._gameContext.Input.Pressed;
    GamePadInputState gamePadInputState;
    Vector2i pov;
    Vector2 leftAxis;
    if (!input.Pressed.Keyboard[40])
    {
      gamePadInputState = pressed.GamePad[0];
      if (!gamePadInputState.Start)
      {
        gamePadInputState = pressed.GamePad[0];
        if (!gamePadInputState.South)
        {
          if (!input.Pressed.Keyboard[41])
          {
            gamePadInputState = pressed.GamePad[0];
            if (!gamePadInputState.Select)
            {
              gamePadInputState = pressed.GamePad[0];
              if (!gamePadInputState.East)
              {
                if (!input.Pressed.Keyboard[82])
                {
                  gamePadInputState = pressed.GamePad[0];
                  pov = gamePadInputState.POV;
                  if (pov.Y != -1)
                  {
                    gamePadInputState = pressed.GamePad[0];
                    leftAxis = gamePadInputState.LeftAxis;
                    if (leftAxis.Y > -0.5)
                      goto label_15;
                  }
                  if (!this._tapDown)
                    goto label_14;
label_15:
                  if (!input.Pressed.Keyboard[81])
                  {
                    gamePadInputState = pressed.GamePad[0];
                    pov = gamePadInputState.POV;
                    if (pov.Y != 1)
                    {
                      gamePadInputState = pressed.GamePad[0];
                      leftAxis = gamePadInputState.LeftAxis;
                      if (leftAxis.Y < 0.5)
                        goto label_20;
                    }
                    if (!this._tapUp)
                      goto label_19;
label_20:
                    if (!input.Pressed.Keyboard[80 /*0x50*/])
                    {
                      gamePadInputState = pressed.GamePad[0];
                      pov = gamePadInputState.POV;
                      if (pov.X != -1)
                      {
                        gamePadInputState = pressed.GamePad[0];
                        leftAxis = gamePadInputState.LeftAxis;
                        if (leftAxis.X > -0.5)
                          goto label_25;
                      }
                      if (!this._tapLeft)
                        goto label_24;
label_25:
                      if (!input.Pressed.Keyboard[79])
                      {
                        gamePadInputState = pressed.GamePad[0];
                        pov = gamePadInputState.POV;
                        if (pov.X != 1)
                        {
                          gamePadInputState = pressed.GamePad[0];
                          leftAxis = gamePadInputState.LeftAxis;
                          if (leftAxis.X < 0.5)
                            goto label_30;
                        }
                        if (this._tapRight)
                          goto label_30;
                      }
                      this.NavigateRight();
                      goto label_30;
                    }
label_24:
                    this.NavigateLeft();
                    goto label_30;
                  }
label_19:
                  this.NavigateDown();
                  goto label_30;
                }
label_14:
                this.NavigateUp();
                goto label_30;
              }
            }
          }
          EventHandler navigateBack = this.NavigateBack;
          if (navigateBack != null)
          {
            navigateBack((object) this, EventArgs.Empty);
            goto label_30;
          }
          goto label_30;
        }
      }
    }
    EventHandler<NavigateNextEventArgs> navigateNext = this.NavigateNext;
    if (navigateNext != null)
      navigateNext((object) this, new NavigateNextEventArgs((IMenuViewModel) null, (object) 4));
label_30:
    gamePadInputState = pressed.GamePad[0];
    pov = gamePadInputState.POV;
    if (pov.Y != 1)
    {
      gamePadInputState = pressed.GamePad[0];
      leftAxis = gamePadInputState.LeftAxis;
      if (leftAxis.Y < 0.5)
      {
        gamePadInputState = pressed.GamePad[0];
        pov = gamePadInputState.POV;
        if (pov.Y != -1)
        {
          gamePadInputState = pressed.GamePad[0];
          leftAxis = gamePadInputState.LeftAxis;
          if (leftAxis.Y >= -0.5)
            goto label_36;
        }
        this._tapDown = true;
        goto label_36;
      }
    }
    this._tapUp = true;
label_36:
    gamePadInputState = pressed.GamePad[0];
    pov = gamePadInputState.POV;
    if (pov.X != 1)
    {
      gamePadInputState = pressed.GamePad[0];
      leftAxis = gamePadInputState.LeftAxis;
      if (leftAxis.X < 0.5)
      {
        gamePadInputState = pressed.GamePad[0];
        pov = gamePadInputState.POV;
        if (pov.X != -1)
        {
          gamePadInputState = pressed.GamePad[0];
          leftAxis = gamePadInputState.LeftAxis;
          if (leftAxis.X >= -0.5)
            goto label_42;
        }
        this._tapLeft = true;
        goto label_42;
      }
    }
    this._tapRight = true;
label_42:
    gamePadInputState = pressed.GamePad[0];
    pov = gamePadInputState.POV;
    if (pov.Y == 0)
    {
      gamePadInputState = pressed.GamePad[0];
      leftAxis = gamePadInputState.LeftAxis;
      if (Math.Abs(leftAxis.Y) < 0.01)
      {
        this._tapUp = false;
        this._tapDown = false;
      }
    }
    gamePadInputState = pressed.GamePad[0];
    pov = gamePadInputState.POV;
    if (pov.X != 0)
      return;
    gamePadInputState = pressed.GamePad[0];
    leftAxis = gamePadInputState.LeftAxis;
    if (Math.Abs(leftAxis.X) >= 0.01)
      return;
    this._tapLeft = false;
    this._tapRight = false;
  }

  public void ApplyOrConfirm()
  {
    EventHandler<NavigateNextEventArgs> handler = this.NavigateNext;
    if (handler == null)
      return;
    handler((object) this, new NavigateNextEventArgs((IMenuViewModel) null, (object) 4));
  }

  public void NavigateUp()
  {
    if (this._selectedIndex <= 0)
      return;
    this._optionSettingUIs[this._selectedIndex].Highlighted = false;
    --this._selectedIndex;
    this._optionSettingUIs[this._selectedIndex].Highlighted = true;
    this.PlayNavigationSound();
  }

  public void NavigateDown()
  {
    if (this._selectedIndex >= this._optionSettingUIs.Count - 1)
      return;
    this._optionSettingUIs[this._selectedIndex].Highlighted = false;
    ++this._selectedIndex;
    this._optionSettingUIs[this._selectedIndex].Highlighted = true;
    this.PlayNavigationSound();
  }

  private void PlayNavigationSound()
  {
    if (this._resources.NavigateSample == null)
      return;
    this._audioContext.PlaySound(this._resources.NavigateSample);
  }

  public void NavigateLeft()
  {
    if (this._selectedIndex == -1)
      return;
    ISetting setting = this._settings[this._selectedIndex];
    switch (setting)
    {
      case IAudioSliderSetting _:
        IAudioSliderSetting audioSliderSetting = setting as IAudioSliderSetting;
        if (audioSliderSetting.Value <= 0.0)
          break;
        audioSliderSetting.Value = MathX.Snap(audioSliderSetting.Value - 0.1, 0.1);
        audioSliderSetting.Value = Math.Max(audioSliderSetting.Value, 0.0);
        this.PlayNavigationSound();
        break;
      case ISpinnerSetting _:
        ISpinnerSetting spinnerSetting = setting as ISpinnerSetting;
        if (spinnerSetting.SelectedIndex <= 0)
          break;
        --spinnerSetting.SelectedIndex;
        this.PlayNavigationSound();
        break;
    }
  }

  public void NavigateRight()
  {
    if (this._selectedIndex == -1)
      return;
    ISetting setting = this._settings[this._selectedIndex];
    switch (setting)
    {
      case IAudioSliderSetting _:
        IAudioSliderSetting audioSliderSetting = setting as IAudioSliderSetting;
        if (audioSliderSetting.Value >= 1.0)
          break;
        audioSliderSetting.Value = MathX.Snap(audioSliderSetting.Value + 0.1, 0.1);
        audioSliderSetting.Value = Math.Min(audioSliderSetting.Value, 1.0);
        this.PlayNavigationSound();
        break;
      case ISpinnerSetting _:
        ISpinnerSetting spinnerSetting = setting as ISpinnerSetting;
        int count = ((IReadOnlyCollection<string>) spinnerSetting.Values).Count;
        if (spinnerSetting.SelectedIndex >= count - 1)
          break;
        ++spinnerSetting.SelectedIndex;
        this.PlayNavigationSound();
        break;
    }
  }

  public void Draw(Renderer renderer)
  {
    I2dRenderer obj = renderer.Get2dRenderer();
    if (this._selectedIndex != -1)
    {
      Rectanglei destination = this._optionSettingUIs[this._selectedIndex].Bounds;
      destination = destination.Inflate(new Vector2i(16 /*0x10*/, 0));
      Colour colour = new Colour(0.1, 1.0, 1.0, 1.0);
      obj.BlendMode = BlendMode.Additive;
      obj.RenderQuad(colour, (Rectangle) destination);
    }
    foreach (SettingUI optionSettingUi in this._optionSettingUIs)
      optionSettingUi.Draw(renderer);
  }
}

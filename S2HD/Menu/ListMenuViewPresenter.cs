// Decompiled with JetBrains decompiler
// Type: S2HD.Menu.ListMenuViewPresenter
// Assembly: S2HD, Version=2.0.1012.10521, Culture=neutral, PublicKeyToken=null
// MVID: 18631A0F-16CF-4E18-8563-1EC5E54750D6
// Assembly location: C:\Games\S2HD_2.0.1012-rc2\S2HD.exe

using SonicOrca;
using SonicOrca.Audio;
using SonicOrca.Geometry;
using SonicOrca.Graphics;
using SonicOrca.Input;
using System;
using System.Collections.Immutable;

namespace S2HD.Menu;

internal class ListMenuViewPresenter : IMenuViewPresenter
{
  private readonly S2HDSonicOrcaGameContext _gameContext;
  private readonly AudioContext _audioContext;
  private readonly ISettingUIResources _resources;
  private readonly ImmutableArray<IMenuItem> _items;
  private readonly IListMenuViewModel _viewModel;
  private bool _tapUp;
  private bool _tapDown;

  public int HighlightedIndex
  {
    get => this._viewModel.HighlightedIndex;
    set => this._viewModel.HighlightedIndex = value;
  }

  public Rectanglei Bounds { get; set; }

  public event EventHandler NavigateBack;

  public event EventHandler<NavigateNextEventArgs> NavigateNext;

  public ListMenuViewPresenter(
    S2HDSonicOrcaGameContext gameContext,
    ISettingUIResources resources,
    IListMenuViewModel viewModel)
  {
    this._gameContext = gameContext;
    this._audioContext = gameContext.Audio;
    this._resources = resources;
    this._items = viewModel.Items;
    this._viewModel = viewModel;
  }

  public void Update()
  {
  }

  public void HandleInput()
  {
    InputContext input = this._gameContext.Input;
    InputState pressed = this._gameContext.Input.Pressed;
    GamePadInputState gamePadInputState;
    Vector2i pov;
    Vector2 leftAxis;
    if (!input.Pressed.Keyboard[41])
    {
      gamePadInputState = pressed.GamePad[0];
      if (!gamePadInputState.Select)
      {
        gamePadInputState = pressed.GamePad[0];
        if (!gamePadInputState.East)
        {
          if (!input.Pressed.Keyboard[40])
          {
            gamePadInputState = pressed.GamePad[0];
            if (!gamePadInputState.Start)
            {
              gamePadInputState = pressed.GamePad[0];
              if (!gamePadInputState.South)
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
                      goto label_13;
                  }
                  if (!this._tapDown)
                    goto label_12;
label_13:
                  if (!input.Pressed.Keyboard[81])
                  {
                    gamePadInputState = pressed.GamePad[0];
                    pov = gamePadInputState.POV;
                    if (pov.Y != 1)
                    {
                      gamePadInputState = pressed.GamePad[0];
                      leftAxis = gamePadInputState.LeftAxis;
                      if (leftAxis.Y < 0.5)
                        goto label_18;
                    }
                    if (this._tapUp)
                      goto label_18;
                  }
                  this.NavigateDown();
                  goto label_18;
                }
label_12:
                this.NavigateUp();
                goto label_18;
              }
            }
          }
          this.NavigateSelect();
          goto label_18;
        }
      }
    }
    this.NavigateBack2();
label_18:
    gamePadInputState = pressed.GamePad[0];
    pov = gamePadInputState.POV;
    if (pov.Y != 1)
    {
      gamePadInputState = pressed.GamePad[0];
      leftAxis = gamePadInputState.LeftAxis;
      if (leftAxis.Y < 0.5)
        goto label_21;
    }
    this._tapUp = true;
label_21:
    gamePadInputState = pressed.GamePad[0];
    pov = gamePadInputState.POV;
    if (pov.Y != -1)
    {
      gamePadInputState = pressed.GamePad[0];
      leftAxis = gamePadInputState.LeftAxis;
      if (leftAxis.Y >= -0.5)
      {
        gamePadInputState = pressed.GamePad[0];
        pov = gamePadInputState.POV;
        if (pov.Y != 0)
          return;
        gamePadInputState = pressed.GamePad[0];
        leftAxis = gamePadInputState.LeftAxis;
        if (Math.Abs(leftAxis.Y) >= 0.01)
          return;
        this._tapUp = false;
        this._tapDown = false;
        return;
      }
    }
    this._tapDown = true;
  }

  private void NavigateBack2()
  {
    EventHandler navigateBack = this.NavigateBack;
    if (navigateBack == null)
      return;
    navigateBack((object) this, EventArgs.Empty);
  }

  public void ActivateSelection() => this.NavigateSelect();

  private void NavigateSelect()
  {
    IMenuItem menuItem = this._items[this.HighlightedIndex];
    NavigateNextEventArgs e = new NavigateNextEventArgs(menuItem.Next, menuItem.Tag);
    EventHandler<NavigateNextEventArgs> navigateNext = this.NavigateNext;
    if (navigateNext == null)
      return;
    navigateNext((object) this, e);
  }

  public void NavigateUp()
  {
    if (this.HighlightedIndex <= 0)
      return;
    --this.HighlightedIndex;
    this.PlayNavigationSound();
  }

  public void NavigateDown()
  {
    if (this.HighlightedIndex >= this._items.Length - 1)
      return;
    ++this.HighlightedIndex;
    this.PlayNavigationSound();
  }

  private void PlayNavigationSound()
  {
    if (this._resources.NavigateSample == null)
      return;
    this._audioContext.PlaySound(this._resources.NavigateSample);
  }

  private void PlayConfirmSound()
  {
    if (this._resources.NavigateSample == null)
      return;
    this._audioContext.PlaySound(this._resources.NavigateSample);
  }

  public void Draw(Renderer renderer)
  {
    I2dRenderer obj = renderer.Get2dRenderer();
    IFontRenderer fontRenderer = renderer.GetFontRenderer();
    int num = this.Bounds.Centre.Y - this._items.Length * 64 /*0x40*/ / 2;
    Rectanglei bounds1 = this.Bounds;
    int x1 = bounds1.X;
    int y1 = num;
    bounds1 = this.Bounds;
    int width1 = bounds1.Width;
    Rectanglei rectanglei1 = new Rectanglei(x1, y1, width1, 64 /*0x40*/);
    Rectanglei rectanglei2 = new Rectanglei();
    ref Rectanglei local = ref rectanglei2;
    Rectanglei bounds2 = this.Bounds;
    int x2 = bounds2.X;
    int y2 = num;
    bounds2 = this.Bounds;
    int width2 = bounds2.Width;
    local = new Rectanglei(x2, y2, width2, 64 /*0x40*/);
    for (int index = 0; index < this._items.Length; ++index)
    {
      IMenuItem menuItem = this._items[index];
      int overlay = 0;
      if (index == this.HighlightedIndex)
      {
        obj.BlendMode = BlendMode.Additive;
        obj.Colour = new Colour(0.3, 1.0, 1.0, 1.0);
        obj.RenderTexture(this._resources.SelectionBar, (Rectangle) rectanglei2);
        overlay = 1;
      }
      fontRenderer.RenderStringWithShadow(menuItem.Text, (Rectangle) rectanglei2, FontAlignment.Centre, this._resources.Font, overlay);
      rectanglei2.Y += rectanglei2.Height;
    }
  }
}

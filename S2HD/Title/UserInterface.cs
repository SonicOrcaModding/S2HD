using System;
using System.Collections.Generic;
using System.Linq;
using SonicOrca;
using SonicOrca.Audio;
using SonicOrca.Core;
using SonicOrca.Extensions;
using SonicOrca.Geometry;
using SonicOrca.Graphics;
using SonicOrca.Input;
using SonicOrca.Resources;

namespace S2HD.Title
{
    // Token: 0x020000A8 RID: 168
    internal class UserInterface
    {
        // Token: 0x17000079 RID: 121
        // (get) Token: 0x060003EB RID: 1003 RVA: 0x0001B729 File Offset: 0x00019929
        // (set) Token: 0x060003EC RID: 1004 RVA: 0x0001B731 File Offset: 0x00019931
        public bool Visible { get; set; }

        // Token: 0x1700007A RID: 122
        // (get) Token: 0x060003ED RID: 1005 RVA: 0x0001B73A File Offset: 0x0001993A
        private bool IsSonicActive
        {
            get
            {
                return this._characterSelectionIndex == 0 || this._characterSelectionIndex == 1;
            }
        }

        // Token: 0x1700007B RID: 123
        // (get) Token: 0x060003EE RID: 1006 RVA: 0x0001B74F File Offset: 0x0001994F
        private bool IsTailsActive
        {
            get
            {
                return this._characterSelectionIndex == 0 || this._characterSelectionIndex == 2;
            }
        }

        // Token: 0x060003EF RID: 1007 RVA: 0x0001B764 File Offset: 0x00019964
        public UserInterface(S2HDSonicOrcaGameContext gameContext, TitleGameState titleGameState, IMaskRenderer maskRenderer)
        {
            this._gameContext = gameContext;
            this._titleGameState = titleGameState;
            this._maskRenderer = maskRenderer;
            ResourceTree resourceTree = this._gameContext.ResourceTree;
            resourceTree.FullfillLoadedResourcesByAttribute(this);
            this._miniSonicAniInstance = new AnimationInstance(resourceTree, "SONICORCA/TITLE/ANIGROUP", 11);
            this._miniTailsAniInstance = new AnimationInstance(resourceTree, "SONICORCA/TITLE/ANIGROUP", 13);
            this.InitialiseLevelSelect();
        }

        // Token: 0x060003F0 RID: 1008 RVA: 0x0001B7E4 File Offset: 0x000199E4
        public void Reset()
        {
            this._ticks = 0;
            this._pressStartActive = true;
            this._pressStartScale = new Vector2(1.0);
            this._pressStartOpacity = 1.0;
            this._pressStartWhiteAdditive = 0.0;
            this._textWhiteAdditive = 0.0;
            this._selectionIndex = 0;
            this._levelSelectInputState = 0;
            this._levelSelectEnabled = false;
            this._levelSelectSelectionIndex = 0;
            this._effectEventManager.Clear();
            this._demoTimeout = new int?(720);
            this._characterSelectTimer = new int?(60);
            this.InitialiseMenuItemWidgets();
        }

        // Token: 0x060003F1 RID: 1009 RVA: 0x0001B88C File Offset: 0x00019A8C
        public void Update()
        {
            if (!this.Visible)
            {
                return;
            }
            if (this._characterSelectActive)
            {
                this._miniSonicAniInstance.Animate();
                this._miniTailsAniInstance.Animate();
                this._characterSelectOpacity = MathX.GoTowards(this._characterSelectOpacity, 1.0, 0.06666666666666667);
                this._textOpacity = 1.0 - this._characterSelectOpacity;
            }
            else
            {
                this._characterSelectOpacity = MathX.GoTowards(this._characterSelectOpacity, 0.0, 0.06666666666666667);
                this._textOpacity = 1.0 - this._characterSelectOpacity;
            }
            this._effectEventManager.Update();
            this.HandleInput();
            this._ticks++;
            if (this._demoTimeout != null)
            {
                this._demoTimeout--;
                if (this._demoTimeout <= 0)
                {
                    this._demoTimeout = null;
                    this.StartDemo();
                }
            }
            if (this._characterSelected)
            {
                this._characterSelectTimer--;
                if (this._characterSelectTimer == 0)
                {
                    this.OnSelectCharacter();
                }
            }
        }

        // Token: 0x060003F2 RID: 1010 RVA: 0x0001BA1C File Offset: 0x00019C1C
        private void HandleInput()
        {
            Controller controller = this._gameContext.Pressed[0];
            InputState pressed = this._gameContext.Input.Pressed;
            if (this._busy)
            {
                return;
            }
            if (!this._levelSelectEnabled)
            {
                int levelSelectControllerInput = this.GetLevelSelectControllerInput(controller);
                if (levelSelectControllerInput != -1)
                {
                    if (this._levelSelectInputState <= 3)
                    {
                        if (this._levelSelectInputState != levelSelectControllerInput)
                        {
                            this._levelSelectInputState = 0;
                        }
                        else
                        {
                            this._levelSelectInputState++;
                        }
                    }
                    else if (levelSelectControllerInput == 4 || levelSelectControllerInput == 7)
                    {
                        if (this._gameContext.Current[0].Action1 && levelSelectControllerInput == 7)
                        {
                            this._levelSelectInputState = 0;
                            this._levelSelectEnabled = true;
                            this._gameContext.Audio.PlaySound(this._sampleNavigateYes);
                            return;
                        }
                    }
                    else
                    {
                        this._levelSelectInputState = 0;
                    }
                }
            }
            if (this._levelSelectEnabled)
            {
                if (Math.Abs(controller.DirectionLeft.Y) >= 0.5)
                {
                    if (Math.Sign(controller.DirectionLeft.Y) < 0)
                    {
                        this._levelSelectSelectionIndex = UserInterface.NegMod(this._levelSelectSelectionIndex - 1, this._levelSelectItems.Length);
                        this._gameContext.Audio.PlaySound(this._sampleNavigateCursor);
                    }
                    else
                    {
                        this._levelSelectSelectionIndex = UserInterface.NegMod(this._levelSelectSelectionIndex + 1, this._levelSelectItems.Length);
                        this._gameContext.Audio.PlaySound(this._sampleNavigateCursor);
                    }
                }
                if (controller.Start)
                {
                    this._gameContext.Audio.PlaySound(this._sampleNavigateYes);
                    this.OnLevelSelectStart();
                }
                if (UserInterface.BackPressed(pressed))
                {
                    this._gameContext.Audio.PlaySound(this._sampleNavigateBack);
                    this._levelSelectEnabled = false;
                    return;
                }
            }
            else if (this._pressStartActive)
            {
                if (controller.Start)
                {
                    this._effectEventManager.BeginEvent(this.EffectPressStart());
                    this._pressStartActive = false;
                    this._demoTimeout = null;
                    this._gameContext.Audio.PlaySound(this._sampleNavigateYes);
                    return;
                }
            }
            else if (this._characterSelectActive)
            {
                if (Math.Abs(controller.DirectionLeft.X) >= 0.5)
                {
                    if (Math.Sign(controller.DirectionLeft.X) < 0)
                    {
                        this._characterSelectionIndex = (this._characterSelectionIndex - 1 + 3) % 3;
                    }
                    else
                    {
                        this._characterSelectionIndex = (this._characterSelectionIndex + 1) % 3;
                    }
                    this._miniSonicAniInstance.Index = 11;
                    this._miniTailsAniInstance.Index = 13;
                    this._gameContext.Audio.PlaySound(this._sampleNavigateCursor);
                }
                if (UserInterface.BackPressed(pressed))
                {
                    this._characterSelectActive = false;
                    this._gameContext.Audio.PlaySound(this._sampleNavigateBack);
                    return;
                }
                if (controller.Start)
                {
                    this._gameContext.Audio.PlaySound(this._sampleNavigateYes);
                    this._miniSonicAniInstance.Index = (this.IsSonicActive ? 12 : 11);
                    this._miniTailsAniInstance.Index = (this.IsTailsActive ? 14 : 13);
                    this._characterSelected = true;
                    return;
                }
            }
            else
            {
                if (UserInterface.BackPressed(pressed))
                {
                    this._demoTimeout = new int?(720);
                    this._pressStartActive = true;
                    this._pressStartOpacity = 1.0;
                    this._pressStartScale = new Vector2(1.0);
                    this._pressStartWhiteAdditive = 0.0;
                    this._selectionIndex = 0;
                    this.InitialiseMenuItemWidgets();
                    this._gameContext.Audio.PlaySound(this._sampleNavigateBack);
                }
                if (Math.Abs(controller.DirectionLeft.X) >= 0.5)
                {
                    int num = Math.Sign(controller.DirectionLeft.X);
                    if (num < 0)
                    {
                        this._selectionIndex = (this._selectionIndex - 1 + this._menuItems.Length) % this._menuItems.Length;
                    }
                    else
                    {
                        this._selectionIndex = (this._selectionIndex + 1) % this._menuItems.Length;
                    }
                    this._effectEventManager.BeginEvent(this.EffectNavigateMenu(num));
                    this._gameContext.Audio.PlaySound(this._sampleNavigateCursor);
                }
                if (controller.Start)
                {
                    Action action = this._menuItems[this._selectionIndex].Action;
                    if (action != null)
                    {
                        action();
                    }
                    this._gameContext.Audio.PlaySound(this._sampleNavigateYes);
                }
            }
        }

        // Token: 0x060003F3 RID: 1011 RVA: 0x0001BE84 File Offset: 0x0001A084
        private static bool BackPressed(InputState inputState)
        {
            GamePadInputState gamePadInputState = inputState.GamePad[0];
            return inputState.Keyboard[41] || gamePadInputState.Select || gamePadInputState.East;
        }

        // Token: 0x060003F4 RID: 1012 RVA: 0x0001BEC0 File Offset: 0x0001A0C0
        private int GetLevelSelectControllerInput(Controller pressed)
        {
            if (pressed.DirectionLeft.Y <= -0.5)
            {
                return 0;
            }
            if (pressed.DirectionLeft.Y >= 0.5)
            {
                return 1;
            }
            if (pressed.DirectionLeft.X <= -0.5)
            {
                return 2;
            }
            if (pressed.DirectionLeft.X >= 0.5)
            {
                return 3;
            }
            if (pressed.Action1)
            {
                return 4;
            }
            if (pressed.Action2)
            {
                return 5;
            }
            if (pressed.Action3)
            {
                return 6;
            }
            if (pressed.Start)
            {
                return 7;
            }
            return -1;
        }

        // Token: 0x060003F5 RID: 1013 RVA: 0x0001BF64 File Offset: 0x0001A164
        private void InitialiseMenuItemWidgets()
        {
            this._menuItems = new UserInterface.MenuItem[]
            {
                new UserInterface.MenuItem
                {
                    Text = "NEW GAME",
                    Action = new Action(this.OnSelectNewGame)
                },
                new UserInterface.MenuItem
                {
                    Text = "OPTIONS",
                    Action = new Action(this.OnSelectOptions)
                },
                new UserInterface.MenuItem
                {
                    Text = "QUIT",
                    Action = new Action(this.OnSelectQuit)
                },
            };
            this._menuItemWidgets = new UserInterface.MenuItemWidget[5];
            for (int i = -2; i <= 2; i++)
            {
                int num = (this._selectionIndex + i + this._menuItems.Length) % this._menuItems.Length;
                UserInterface.MenuItem menuItem = this._menuItems[num];
                UserInterface.MenuItemWidget menuItemWidget = new UserInterface.MenuItemWidget
                {
                    MenuItemIndex = num,
                    OriginOffset = i,
                    X = (float)(960 + 400 * i),
                    Scale = new Vector2(1.0)
                };
                menuItemWidget.Opacity = (float)UserInterface.MenuItemOpacityEaseTimeline.GetValueAt((int)menuItemWidget.X);
                this._menuItemWidgets[i + 2] = menuItemWidget;
            }
            this.SetSelectionMarkerPositions();
        }

        // Token: 0x060003F6 RID: 1014 RVA: 0x0001C094 File Offset: 0x0001A294
        private void SetSelectionMarkerPositions()
        {
            UserInterface.MenuItemWidget menuItemWidget = this._menuItemWidgets.First((UserInterface.MenuItemWidget x) => x.OriginOffset == 0);
            int markerOffset = this.GetMarkerOffset(this._selectionIndex);
            this._selectedMenuItemMarkerPositions[0] = new Vector2((double)(menuItemWidget.X - (float)markerOffset), 900.0);
            this._selectedMenuItemMarkerPositions[1] = new Vector2((double)(menuItemWidget.X + (float)markerOffset), 900.0);
        }

        // Token: 0x060003F7 RID: 1015 RVA: 0x0001C124 File Offset: 0x0001A324
        private int GetMenuItemWidth(int index)
        {
            return (int)this._fontImpactRegular.MeasureString(this._menuItems[index].Text).Width;
        }

        // Token: 0x060003F8 RID: 1016 RVA: 0x0001C152 File Offset: 0x0001A352
        private int GetMarkerOffset(int index)
        {
            return this.GetMenuItemWidth(index) / 2 + 48;
        }

        // Token: 0x060003F9 RID: 1017 RVA: 0x0001C160 File Offset: 0x0001A360
        private void InitialiseLevelSelect()
        {
            List<UserInterface.LevelSelectItem> list = new List<UserInterface.LevelSelectItem>();
            int num = 1;
            foreach (LevelInfo levelInfo in Levels.LevelList)
            {
                if (!levelInfo.Unreleased)
                {
                    for (int i = 1; i <= levelInfo.Acts; i++)
                    {
                        string text = string.Format("{0} ACT {1}", levelInfo.Name, i).ToUpper();
                        list.Add(new UserInterface.LevelSelectItem
                        {
                            Text = text,
                            Mnemonic = levelInfo.Mnemonic,
                            Act = i,
                            Number = num
                        });
                    }
                    num++;
                }
            }
            this._levelSelectItems = list.ToArray();
        }

        // Token: 0x060003FA RID: 1018 RVA: 0x0001C228 File Offset: 0x0001A428
        private void OnSelectNewGame()
        {
            this._characterSelectActive = true;
        }

        // Token: 0x060003FB RID: 1019 RVA: 0x0001C234 File Offset: 0x0001A434
        private void OnSelectCharacter()
        {
            this._busy = true;
            this._effectEventManager.BeginEvent(this.EffectFadeOut());
            this._titleGameState.Result = TitleGameState.ResultType.NewGame;
            LevelPrepareSettings levelPrepareSettings = new LevelPrepareSettings
            {
                AreaResourceKey = Levels.GetAreaResourceKey("ehz"),
                Act = 1,
                LevelNumber = 1
            };
            this.ApplyCharacterSelection(levelPrepareSettings);
            this._titleGameState.LevelPrepareSettings = levelPrepareSettings;
        }

        // Token: 0x060003FC RID: 1020 RVA: 0x0001C29C File Offset: 0x0001A49C
        private void OnSelectOptions()
        {
            this._busy = true;
            this._effectEventManager.BeginEvent(this.EffectFadeOut());
            this._titleGameState.Result = TitleGameState.ResultType.ShowOptions;
        }

        // Token: 0x060003FD RID: 1021 RVA: 0x0001C2C2 File Offset: 0x0001A4C2
        private void OnSelectQuit()
        {
            this._busy = true;
            this._effectEventManager.BeginEvent(this.EffectFadeOut());
            this._titleGameState.Result = TitleGameState.ResultType.Quit;
        }

        // Token: 0x060003FE RID: 1022 RVA: 0x0001C2E8 File Offset: 0x0001A4E8
        private void StartDemo()
        {
            this._busy = true;
            this._effectEventManager.BeginEvent(this.EffectFadeOut());
            this._titleGameState.Result = TitleGameState.ResultType.StartDemo;
        }

        // Token: 0x060003FF RID: 1023 RVA: 0x0001C310 File Offset: 0x0001A510
        private void OnLevelSelectStart()
        {
            this._busy = true;
            UserInterface.LevelSelectItem levelSelectItem = this._levelSelectItems[this._levelSelectSelectionIndex];
            LevelPrepareSettings levelPrepareSettings = new LevelPrepareSettings
            {
                AreaResourceKey = Levels.GetAreaResourceKey(levelSelectItem.Mnemonic),
                Act = levelSelectItem.Act,
                LevelNumber = levelSelectItem.Number
            };
            this.ApplyCharacterSelection(levelPrepareSettings);
            this._titleGameState.LevelPrepareSettings = levelPrepareSettings;
            this._titleGameState.FadeOut();
            this._titleGameState.Result = TitleGameState.ResultType.LevelSelect;
        }

        // Token: 0x06000400 RID: 1024 RVA: 0x0001C38C File Offset: 0x0001A58C
        private void ApplyCharacterSelection(LevelPrepareSettings lps)
        {
            switch (this._characterSelectionIndex)
            {
                case 0:
                    lps.ProtagonistCharacter = CharacterType.Sonic;
                    lps.SidekickCharacter = CharacterType.Tails;
                    return;
                case 1:
                    lps.ProtagonistCharacter = CharacterType.Sonic;
                    lps.SidekickCharacter = CharacterType.Null;
                    return;
                case 2:
                    lps.ProtagonistCharacter = CharacterType.Tails;
                    lps.SidekickCharacter = CharacterType.Null;
                    return;
                default:
                    return;
            }
        }

        // Token: 0x06000401 RID: 1025 RVA: 0x0001C3DF File Offset: 0x0001A5DF
        private IEnumerable<UpdateResult> EffectPressStart()
        {
            this._busy = true;
            int num;
            for (int t = 0; t <= 50; t = num + 1)
            {
                this._pressStartScale = new Vector2(UserInterface.PressStartScaleXTimeline.GetValueAt(t), UserInterface.PressStartScaleYTimeline.GetValueAt(t));
                this._pressStartWhiteAdditive = UserInterface.PressStartWhiteAdditiveTimeline.GetValueAt(t);
                this._pressStartOpacity = UserInterface.PressStartOpacityTimeline.GetValueAt(t);
                this._textOpacity = UserInterface.TextOpacityTimeline.GetValueAt(t);
                this._textWhiteAdditive = UserInterface.TextWhiteAdditiveTimeline.GetValueAt(t);
                yield return UpdateResult.Next();
                num = t;
            }
            this._busy = false;
            yield break;
        }

        // Token: 0x06000402 RID: 1026 RVA: 0x0001C3EF File Offset: 0x0001A5EF
        private IEnumerable<UpdateResult> EffectNavigateMenu(int direction)
        {
            this._busy = true;
            int markerOffset = this.GetMarkerOffset(UserInterface.NegMod(this._selectionIndex - direction, this._menuItems.Length));
            int markerOffset2 = this.GetMarkerOffset(this._selectionIndex);
            double markerVelocityX = (double)((markerOffset2 - markerOffset) / 7);
            double markerVelocityY = 7.0;
            float velocity = (float)(direction * -1) * 26.666666f;
            int i;
            for (int t = 0; t < 15; t = i + 1)
            {
                foreach (UserInterface.MenuItemWidget menuItemWidget in this._menuItemWidgets)
                {
                    menuItemWidget.X += velocity;
                    menuItemWidget.Opacity = (float)UserInterface.MenuItemOpacityEaseTimeline.GetValueAt((int)menuItemWidget.X);
                }
                if (t <= 7)
                {
                    for (int j = 0; j < this._selectedMenuItemMarkerPositions.Length; j++)
                    {
                        Vector2[] selectedMenuItemMarkerPositions = this._selectedMenuItemMarkerPositions;
                        int num = j;
                        selectedMenuItemMarkerPositions[num].Y = selectedMenuItemMarkerPositions[num].Y - markerVelocityY;
                    }
                }
                else
                {
                    Vector2[] selectedMenuItemMarkerPositions2 = this._selectedMenuItemMarkerPositions;
                    int num2 = 0;
                    selectedMenuItemMarkerPositions2[num2].X = selectedMenuItemMarkerPositions2[num2].X - markerVelocityX;
                    Vector2[] selectedMenuItemMarkerPositions3 = this._selectedMenuItemMarkerPositions;
                    int num3 = 1;
                    selectedMenuItemMarkerPositions3[num3].X = selectedMenuItemMarkerPositions3[num3].X + markerVelocityX;
                    Vector2[] selectedMenuItemMarkerPositions4 = this._selectedMenuItemMarkerPositions;
                    int num4 = 0;
                    selectedMenuItemMarkerPositions4[num4].Y = selectedMenuItemMarkerPositions4[num4].Y + markerVelocityY;
                    Vector2[] selectedMenuItemMarkerPositions5 = this._selectedMenuItemMarkerPositions;
                    int num5 = 1;
                    selectedMenuItemMarkerPositions5[num5].Y = selectedMenuItemMarkerPositions5[num5].Y + markerVelocityY;
                }
                yield return UpdateResult.Next();
                i = t;
            }
            foreach (UserInterface.MenuItemWidget menuItemWidget2 in this._menuItemWidgets)
            {
                int num6 = menuItemWidget2.OriginOffset - direction;
                if (num6 == -3)
                {
                    num6 = 2;
                    menuItemWidget2.MenuItemIndex = UserInterface.NegMod(menuItemWidget2.MenuItemIndex - 1, this._menuItems.Length);
                }
                else if (num6 == 3)
                {
                    num6 = -2;
                    menuItemWidget2.MenuItemIndex = UserInterface.NegMod(menuItemWidget2.MenuItemIndex + 1, this._menuItems.Length);
                }
                menuItemWidget2.X = (float)(960 + 400 * num6);
                menuItemWidget2.Opacity = (float)UserInterface.MenuItemOpacityEaseTimeline.GetValueAt((int)menuItemWidget2.X);
                menuItemWidget2.OriginOffset = num6;
            }
            this.SetSelectionMarkerPositions();
            this._busy = false;
            yield break;
        }

        // Token: 0x06000403 RID: 1027 RVA: 0x0001C406 File Offset: 0x0001A606
        private IEnumerable<UpdateResult> EffectFadeOut()
        {
            UserInterface.MenuItemWidget widget = this._menuItemWidgets.First((UserInterface.MenuItemWidget x) => x.MenuItemIndex == this._selectionIndex);
            this._titleGameState.Background.WipeOut();
            int num;
            for (int t = 0; t <= 30; t = num + 1)
            {
                if (t == 15)
                {
                    this._titleGameState.FadeOut();
                }
                widget.Scale = new Vector2(UserInterface.ActivatedTextScaleTimeline.GetValueAt(t));
                this._textOpacity = (double)((float)UserInterface.ActivatedTextOpacityTimeline.GetValueAt(t));
                yield return UpdateResult.Next();
                num = t;
            }
            yield break;
        }

        // Token: 0x06000404 RID: 1028 RVA: 0x0001C418 File Offset: 0x0001A618
        public void Draw(Renderer renderer)
        {
            if (!this.Visible)
            {
                return;
            }
            this.DrawPressStart(renderer);
            if (!this._pressStartActive)
            {
                if (this._characterSelectOpacity != 0.0)
                {
                    this.DrawCharacterSelect(renderer);
                }
                if (this._characterSelectOpacity != 1.0)
                {
                    this.DrawMenuItems(renderer);
                }
            }
            if (this._levelSelectEnabled)
            {
                this.DrawLevelSelect(renderer);
            }
        }

        // Token: 0x06000405 RID: 1029 RVA: 0x0001C47C File Offset: 0x0001A67C
        private void DrawPressStart(Renderer renderer)
        {
            I2dRenderer i2dRenderer = renderer.Get2dRenderer();
            IFontRenderer fontRenderer = renderer.GetFontRenderer();
            Vector2i vector2i = new Vector2i(960, 900);
            if (this._pressStartOpacity == 0.0)
            {
                return;
            }
            i2dRenderer.BlendMode = BlendMode.Alpha;
            using (i2dRenderer.BeginMatixState())
            {
                i2dRenderer.ModelMatrix = i2dRenderer.ModelMatrix.Scale(this._pressStartScale.X, this._pressStartScale.Y, 1.0);
                i2dRenderer.ModelMatrix = i2dRenderer.ModelMatrix.Translate((double)vector2i.X, (double)vector2i.Y, 0.0);
                i2dRenderer.AdditiveColour = new Colour(this._pressStartWhiteAdditive);
                fontRenderer.RenderStringWithShadow("PRESS START", default(Rectangle), FontAlignment.Centre, this._fontImpactItalic, new Colour(this._pressStartOpacity, 1.0, 1.0, 1.0), new int?(0));
                i2dRenderer.AdditiveColour = Colours.Transparent;
            }
            int num = (int)this._fontImpactItalic.MeasureString("PRESS START").Width;
            int num2 = 79;
            int num3 = 16;
            Vector2i vector2i2 = new Vector2i(num / 2 + num3 + num2 / 2, 0);
            int width = this._textureZigZag.Width;
            int num4 = this._ticks / 2;
            int wrapOffsetX = width - num4 % width;
            int wrapOffsetX2 = num4 % width;
            this.DrawZigZag(i2dRenderer, new Rectanglei(vector2i.X - vector2i2.X - num2 / 2, vector2i.Y, num2, 0), wrapOffsetX);
            this.DrawZigZag(i2dRenderer, new Rectanglei(vector2i.X + vector2i2.X - num2 / 2, vector2i.Y, num2, 0), wrapOffsetX2);
        }

        // Token: 0x06000406 RID: 1030 RVA: 0x0001C664 File Offset: 0x0001A864
        private void DrawZigZag(I2dRenderer g, Rectanglei rect, int wrapOffsetX)
        {
            rect.Y -= this._textureZigZag.Height / 2;
            rect.Height = this._textureZigZag.Height;
            Rectangle clipRectangle = g.ClipRectangle;
            g.ClipRectangle = rect;
            Rectanglei r = rect;
            r.X = rect.X - wrapOffsetX;
            r.Width = this._textureZigZag.Width;
            while (r.X < rect.Right)
            {
                g.RenderTexture(this._textureZigZag, r, false, false);
                r.X += r.Width;
            }
            g.ClipRectangle = clipRectangle;
        }

        // Token: 0x06000407 RID: 1031 RVA: 0x0001C718 File Offset: 0x0001A918
        private void DrawMenuItems(Renderer renderer)
        {
            I2dRenderer i2dRenderer = renderer.Get2dRenderer();
            renderer.GetFontRenderer();
            int y = 900;
            foreach (UserInterface.MenuItemWidget menuItemWidget in this._menuItemWidgets)
            {
                UserInterface.MenuItem menuItem = this._menuItems[menuItemWidget.MenuItemIndex];
                bool selected = menuItemWidget.MenuItemIndex == this._selectionIndex;
                this.DrawMenuItem(renderer, menuItem.Text, new Vector2i((int)menuItemWidget.X, y), (double)menuItemWidget.Opacity, menuItemWidget.Scale, selected);
            }
            i2dRenderer.Colour = new Colour(this._textOpacity, 1.0, 1.0, 1.0);
            i2dRenderer.AdditiveColour = new Colour(this._textWhiteAdditive);
            foreach (Vector2 v in this._selectedMenuItemMarkerPositions)
            {
                i2dRenderer.RenderTexture(this._textureSelectionMarker, (Vector2i)v, false, false);
            }
            i2dRenderer.AdditiveColour = Colours.Transparent;
        }

        // Token: 0x06000408 RID: 1032 RVA: 0x0001C824 File Offset: 0x0001AA24
        private void DrawMenuItem(Renderer renderer, string text, Vector2i position, double opacity, Vector2 scale, bool selected = false)
        {
            I2dRenderer i2dRenderer = renderer.Get2dRenderer();
            IFontRenderer fontRenderer = renderer.GetFontRenderer();
            int value = 1;
            if (!selected)
            {
                value = 0;
            }
            if (this._textOpacity < 1.0)
            {
                opacity *= this._textOpacity;
            }
            double a = opacity * opacity * opacity;
            Colour colour = new Colour(opacity, 1.0, 1.0, 1.0);
            Colour shadowColour = new Colour(a, 0.0, 0.0, 0.0);
            i2dRenderer.AdditiveColour = new Colour(this._textWhiteAdditive);
            using (i2dRenderer.BeginMatixState())
            {
                i2dRenderer.ModelMatrix = i2dRenderer.ModelMatrix.Scale(scale);
                i2dRenderer.ModelMatrix = i2dRenderer.ModelMatrix.Translate(position);
                FontAlignment fontAlignment = FontAlignment.Centre;
                Font fontImpactRegular = this._fontImpactRegular;
                fontRenderer.RenderStringWithShadow(text, default(Rectangle), fontAlignment, fontImpactRegular, colour, new int?(value), fontImpactRegular.DefaultShadow, shadowColour, null);
            }
            i2dRenderer.AdditiveColour = Colours.Transparent;
        }

        // Token: 0x06000409 RID: 1033 RVA: 0x0001C964 File Offset: 0x0001AB64
        private void DrawCharacterSelect(Renderer renderer)
        {
            double characterSelectOpacity = this._characterSelectOpacity;
            Colour colour = new Colour(characterSelectOpacity, 0.25, 0.25, 0.25);
            Colour colour2 = new Colour(characterSelectOpacity, 1.0, 1.0, 1.0);
            string text = (new string[]
            {
                "SONIC & TAILS",
                "SONIC",
                "TAILS"
            })[this._characterSelectionIndex];
            I2dRenderer i2dRenderer = renderer.Get2dRenderer();
            IFontRenderer fontRenderer = renderer.GetFontRenderer();
            Rectangle rectangle = new Rectangle(0.0, 950.0, 1920.0, 60.0);
            i2dRenderer.BlendMode = BlendMode.Alpha;
            i2dRenderer.RenderQuad(new Colour(0.3 * characterSelectOpacity, 0.0, 0.0, 0.0), rectangle);
            fontRenderer.RenderStringWithShadow(text, rectangle, FontAlignment.Centre, this._fontImpactRegular, Colour.FromOpacity(characterSelectOpacity), new int?(1));
            i2dRenderer.Colour = Colour.FromOpacity(characterSelectOpacity);
            Rectangle rectangle2 = this._fontImpactRegular.MeasureString(text, rectangle, FontAlignment.Centre);
            i2dRenderer.RenderTexture(this._textureLeftArrow, new Vector2(rectangle2.Left - 50.0, rectangle2.CentreY), false, false);
            i2dRenderer.RenderTexture(this._textureRightArrow, new Vector2(rectangle2.Right + 50.0, rectangle2.CentreY), false, false);
            Colour colour3 = this.IsSonicActive ? colour2 : colour;
            this._miniSonicAniInstance.Draw(i2dRenderer, colour3, new Vector2i(910, 880), false, false);
            colour3 = (this.IsTailsActive ? colour2 : colour);
            this._miniTailsAniInstance.Draw(i2dRenderer, colour3, new Vector2i(1010, 880), false, false);
        }

        // Token: 0x0600040A RID: 1034 RVA: 0x0001CB50 File Offset: 0x0001AD50
        private void DrawLevelSelect(Renderer renderer)
        {
            I2dRenderer i2dRenderer = renderer.Get2dRenderer();
            IFontRenderer fontRenderer = renderer.GetFontRenderer();
            i2dRenderer.BlendMode = BlendMode.Alpha;
            i2dRenderer.RenderQuad(new Colour(0.8, 0.0, 0.0, 0.0), new Rectangle(0.0, 0.0, 1920.0, 1080.0));
            int num = 660;
            int num2 = 400;
            int num3 = 0;
            fontRenderer.RenderStringWithShadow("LEVEL SELECT", new Rectangle((double)num, 330.0, 0.0, 0.0), FontAlignment.MiddleY, this._fontImpactRegular, 0);
            foreach (UserInterface.LevelSelectItem levelSelectItem in this._levelSelectItems)
            {
                bool flag = this._levelSelectSelectionIndex == num3;
                int value = flag ? 1 : 0;
                Colour colour = flag ? Colours.White : new Colour(0.5, 1.0, 1.0, 1.0);
                fontRenderer.RenderStringWithShadow(levelSelectItem.Text, new Rectangle((double)num, (double)num2, 0.0, 0.0), FontAlignment.MiddleY, this._fontImpactRegular, colour, new int?(value));
                num2 += 50;
                num3++;
            }
        }

        // Token: 0x0600040B RID: 1035 RVA: 0x0001CCBA File Offset: 0x0001AEBA
        private static int NegMod(int x, int divisor)
        {
            while (x < 0)
            {
                x += divisor;
            }
            return x % divisor;
        }

        // Token: 0x0600040C RID: 1036 RVA: 0x0001CCCC File Offset: 0x0001AECC
        // Note: this type is marked as 'beforefieldinit'.
        static UserInterface()
        {
        }

        // Token: 0x040004B8 RID: 1208
        private readonly EffectEventManager _effectEventManager = new EffectEventManager();

        // Token: 0x040004B9 RID: 1209
        private readonly S2HDSonicOrcaGameContext _gameContext;

        // Token: 0x040004BA RID: 1210
        private readonly TitleGameState _titleGameState;

        // Token: 0x040004BB RID: 1211
        private readonly IMaskRenderer _maskRenderer;

        // Token: 0x040004BC RID: 1212
        [ResourcePath("SONICORCA/TITLE/SELECTIONMARKER")]
        private ITexture _textureSelectionMarker;

        // Token: 0x040004BD RID: 1213
        [ResourcePath("SONICORCA/TITLE/ZIGZAG")]
        private ITexture _textureZigZag;

        // Token: 0x040004BE RID: 1214
        [ResourcePath("SONICORCA/MENU/LEFT")]
        private ITexture _textureLeftArrow;

        // Token: 0x040004BF RID: 1215
        [ResourcePath("SONICORCA/MENU/RIGHT")]
        private ITexture _textureRightArrow;

        // Token: 0x040004C0 RID: 1216
        [ResourcePath("SONICORCA/FONTS/IMPACT/REGULAR")]
        private Font _fontImpactRegular;

        // Token: 0x040004C1 RID: 1217
        [ResourcePath("SONICORCA/FONTS/IMPACT/ITALIC")]
        private Font _fontImpactItalic;

        // Token: 0x040004C2 RID: 1218
        [ResourcePath("SONICORCA/SOUND/NAVIGATE/CURSOR")]
        private Sample _sampleNavigateCursor;

        // Token: 0x040004C3 RID: 1219
        [ResourcePath("SONICORCA/SOUND/NAVIGATE/BACK")]
        private Sample _sampleNavigateBack;

        // Token: 0x040004C4 RID: 1220
        [ResourcePath("SONICORCA/SOUND/NAVIGATE/YES")]
        private Sample _sampleNavigateYes;

        // Token: 0x040004C5 RID: 1221
        private int _ticks;

        // Token: 0x040004C6 RID: 1222
        private bool _pressStartActive;

        // Token: 0x040004C7 RID: 1223
        private int _selectionIndex;

        // Token: 0x040004C8 RID: 1224
        private bool _busy;

        // Token: 0x040004C9 RID: 1225
        private Vector2 _pressStartScale;

        // Token: 0x040004CA RID: 1226
        private double _pressStartOpacity;

        // Token: 0x040004CB RID: 1227
        private double _pressStartWhiteAdditive;

        // Token: 0x040004CC RID: 1228
        private double _textOpacity;

        // Token: 0x040004CD RID: 1229
        private double _textWhiteAdditive;

        // Token: 0x040004CE RID: 1230
        private readonly Vector2[] _selectedMenuItemMarkerPositions = new Vector2[2];

        // Token: 0x040004CF RID: 1231
        private int _levelSelectInputState;

        // Token: 0x040004D0 RID: 1232
        private bool _levelSelectEnabled;

        // Token: 0x040004D1 RID: 1233
        private int _levelSelectSelectionIndex;

        // Token: 0x040004D2 RID: 1234
        private UserInterface.LevelSelectItem[] _levelSelectItems;

        // Token: 0x040004D4 RID: 1236
        private UserInterface.MenuItem[] _menuItems;

        // Token: 0x040004D5 RID: 1237
        private UserInterface.MenuItemWidget[] _menuItemWidgets;

        // Token: 0x040004D6 RID: 1238
        private AnimationInstance _miniSonicAniInstance;

        // Token: 0x040004D7 RID: 1239
        private AnimationInstance _miniTailsAniInstance;

        // Token: 0x040004D8 RID: 1240
        private int _characterSelectionIndex;

        // Token: 0x040004D9 RID: 1241
        private bool _characterSelectActive;

        // Token: 0x040004DA RID: 1242
        private double _characterSelectOpacity;

        // Token: 0x040004DB RID: 1243
        private const int DemoInitialTimeout = 720;

        // Token: 0x040004DC RID: 1244
        private int? _demoTimeout;

        // Token: 0x040004DD RID: 1245
        private const int CharacterSelectTime = 60;

        // Token: 0x040004DE RID: 1246
        private int? _characterSelectTimer;

        // Token: 0x040004DF RID: 1247
        private bool _characterSelected;

        // Token: 0x040004E0 RID: 1248
        private static readonly EaseTimeline PressStartScaleXTimeline = new EaseTimeline(new EaseTimeline.Entry[]
        {
            new EaseTimeline.Entry(5, 1.0),
            new EaseTimeline.Entry(10, 1.2),
            new EaseTimeline.Entry(20, 0.0)
        });

        // Token: 0x040004E1 RID: 1249
        private static readonly EaseTimeline PressStartScaleYTimeline = new EaseTimeline(new EaseTimeline.Entry[]
        {
            new EaseTimeline.Entry(0, 1.0),
            new EaseTimeline.Entry(5, 1.45),
            new EaseTimeline.Entry(10, 1.0),
            new EaseTimeline.Entry(20, 0.0)
        });

        // Token: 0x040004E2 RID: 1250
        private static readonly EaseTimeline PressStartWhiteAdditiveTimeline = new EaseTimeline(new EaseTimeline.Entry[]
        {
            new EaseTimeline.Entry(0, 0.0),
            new EaseTimeline.Entry(10, 1.0)
        });

        // Token: 0x040004E3 RID: 1251
        private static readonly EaseTimeline PressStartOpacityTimeline = new EaseTimeline(new EaseTimeline.Entry[]
        {
            new EaseTimeline.Entry(10, 1.0),
            new EaseTimeline.Entry(20, 0.0)
        });

        // Token: 0x040004E4 RID: 1252
        private static readonly EaseTimeline TextOpacityTimeline = new EaseTimeline(new EaseTimeline.Entry[]
        {
            new EaseTimeline.Entry(20, 0.0),
            new EaseTimeline.Entry(30, 1.0)
        });

        // Token: 0x040004E5 RID: 1253
        private static readonly EaseTimeline TextWhiteAdditiveTimeline = new EaseTimeline(new EaseTimeline.Entry[]
        {
            new EaseTimeline.Entry(20, 1.0),
            new EaseTimeline.Entry(30, 0.0)
        });

        // Token: 0x040004E6 RID: 1254
        private static readonly EaseTimeline MenuItemOpacityEaseTimeline = new EaseTimeline(new EaseTimeline.Entry[]
        {
            new EaseTimeline.Entry(260, 0.0),
            new EaseTimeline.Entry(560, 0.5),
            new EaseTimeline.Entry(960, 1.0),
            new EaseTimeline.Entry(1360, 0.5),
            new EaseTimeline.Entry(1660, 0.0)
        });

        // Token: 0x040004E7 RID: 1255
        private static readonly EaseTimeline ActivatedTextScaleTimeline = new EaseTimeline(new EaseTimeline.Entry[]
        {
            new EaseTimeline.Entry(0, 1.0),
            new EaseTimeline.Entry(15, 0.8),
            new EaseTimeline.Entry(30, 1.4)
        });

        // Token: 0x040004E8 RID: 1256
        private static readonly EaseTimeline ActivatedTextOpacityTimeline = new EaseTimeline(new EaseTimeline.Entry[]
        {
            new EaseTimeline.Entry(15, 1.0),
            new EaseTimeline.Entry(30, 0.0)
        });

        // Token: 0x02000120 RID: 288
        private class MenuItem
        {
            // Token: 0x17000119 RID: 281
            // (get) Token: 0x060006DD RID: 1757 RVA: 0x00028943 File Offset: 0x00026B43
            // (set) Token: 0x060006DE RID: 1758 RVA: 0x0002894B File Offset: 0x00026B4B
            public string Text { get; set; }

            // Token: 0x1700011A RID: 282
            // (get) Token: 0x060006DF RID: 1759 RVA: 0x00028954 File Offset: 0x00026B54
            // (set) Token: 0x060006E0 RID: 1760 RVA: 0x0002895C File Offset: 0x00026B5C
            public Action Action { get; set; }

            // Token: 0x060006E1 RID: 1761 RVA: 0x00016B22 File Offset: 0x00014D22
            public MenuItem()
            {
            }
        }

        // Token: 0x02000121 RID: 289
        private class MenuItemWidget
        {
            // Token: 0x1700011B RID: 283
            // (get) Token: 0x060006E2 RID: 1762 RVA: 0x00028965 File Offset: 0x00026B65
            // (set) Token: 0x060006E3 RID: 1763 RVA: 0x0002896D File Offset: 0x00026B6D
            public int OriginOffset { get; set; }

            // Token: 0x1700011C RID: 284
            // (get) Token: 0x060006E4 RID: 1764 RVA: 0x00028976 File Offset: 0x00026B76
            // (set) Token: 0x060006E5 RID: 1765 RVA: 0x0002897E File Offset: 0x00026B7E
            public int MenuItemIndex { get; set; }

            // Token: 0x1700011D RID: 285
            // (get) Token: 0x060006E6 RID: 1766 RVA: 0x00028987 File Offset: 0x00026B87
            // (set) Token: 0x060006E7 RID: 1767 RVA: 0x0002898F File Offset: 0x00026B8F
            public float Opacity { get; set; }

            // Token: 0x1700011E RID: 286
            // (get) Token: 0x060006E8 RID: 1768 RVA: 0x00028998 File Offset: 0x00026B98
            // (set) Token: 0x060006E9 RID: 1769 RVA: 0x000289A0 File Offset: 0x00026BA0
            public float X { get; set; }

            // Token: 0x1700011F RID: 287
            // (get) Token: 0x060006EA RID: 1770 RVA: 0x000289A9 File Offset: 0x00026BA9
            // (set) Token: 0x060006EB RID: 1771 RVA: 0x000289B1 File Offset: 0x00026BB1
            public Vector2 Scale { get; set; }

            // Token: 0x060006EC RID: 1772 RVA: 0x00016B22 File Offset: 0x00014D22
            public MenuItemWidget()
            {
            }
        }

        // Token: 0x02000122 RID: 290
        private class LevelSelectItem
        {
            // Token: 0x17000120 RID: 288
            // (get) Token: 0x060006ED RID: 1773 RVA: 0x000289BA File Offset: 0x00026BBA
            // (set) Token: 0x060006EE RID: 1774 RVA: 0x000289C2 File Offset: 0x00026BC2
            public string Text { get; set; }

            // Token: 0x17000121 RID: 289
            // (get) Token: 0x060006EF RID: 1775 RVA: 0x000289CB File Offset: 0x00026BCB
            // (set) Token: 0x060006F0 RID: 1776 RVA: 0x000289D3 File Offset: 0x00026BD3
            public string Mnemonic { get; set; }

            // Token: 0x17000122 RID: 290
            // (get) Token: 0x060006F1 RID: 1777 RVA: 0x000289DC File Offset: 0x00026BDC
            // (set) Token: 0x060006F2 RID: 1778 RVA: 0x000289E4 File Offset: 0x00026BE4
            public int Act { get; set; }

            // Token: 0x17000123 RID: 291
            // (get) Token: 0x060006F3 RID: 1779 RVA: 0x000289ED File Offset: 0x00026BED
            // (set) Token: 0x060006F4 RID: 1780 RVA: 0x000289F5 File Offset: 0x00026BF5
            public int Number { get; set; }

            // Token: 0x060006F5 RID: 1781 RVA: 0x00016B22 File Offset: 0x00014D22
            public LevelSelectItem()
            {
            }
        }
    }
}

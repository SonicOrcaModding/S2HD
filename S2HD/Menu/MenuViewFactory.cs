// Decompiled with JetBrains decompiler
// Type: S2HD.Menu.MenuViewFactory
// Assembly: S2HD, Version=2.0.1012.10521, Culture=neutral, PublicKeyToken=null
// MVID: 18631A0F-16CF-4E18-8563-1EC5E54750D6
// Assembly location: C:\Games\S2HD_2.0.1012-rc2\S2HD.exe

using SonicOrca;
using SonicOrca.Graphics;
using System;
using System.Collections.Generic;

namespace S2HD.Menu;

internal class MenuViewFactory
{
  private S2HDSonicOrcaGameContext _gameContext;

  public MenuViewFactory(S2HDSonicOrcaGameContext gameContext) => this._gameContext = gameContext;

  public IListMenuViewModel GetInGameOptionsView(bool canRestart)
  {
    List<IMenuItem> items = new List<IMenuItem>(4);
    items.Add((IMenuItem) new MenuItem("RESUME", tag: (object) 3));
    if (canRestart)
      items.Add((IMenuItem) new MenuItem("RESTART", (IMenuViewModel) this.GetConfirmMenu((object) 2)));
    items.Add((IMenuItem) new MenuItem("OPTIONS", this.GetOptionsView()));
    items.Add((IMenuItem) new MenuItem("QUIT", (IMenuViewModel) this.GetConfirmMenu((object) 1)));
    return (IListMenuViewModel) new ListMenuViewModel((IEnumerable<IMenuItem>) items);
  }

  public IListMenuViewModel GetConfirmMenu(object confirmTag)
  {
    return (IListMenuViewModel) new ListMenuViewModel((IEnumerable<IMenuItem>) new IMenuItem[2]
    {
      (IMenuItem) new MenuItem("NO", tag: (object) 0),
      (IMenuItem) new MenuItem("YES", tag: confirmTag)
    });
  }

  public IMenuViewModel GetOptionsView()
  {
    List<IMenuItem> optionItems = new List<IMenuItem>(3)
    {
      (IMenuItem) new MenuItem("AUDIO", (IMenuViewModel) this.GetAudioOptions(), (object) 6),
      (IMenuItem) new MenuItem("VIDEO", (IMenuViewModel) this.GetVideoOptions(), (object) 7)
    };
#if __ANDROID__
    optionItems.Add((IMenuItem) new MenuItem("MOBILE", (IMenuViewModel) this.GetMobileOptions(), (object) 8));
#endif
    return (IMenuViewModel) new ListMenuViewModel((IEnumerable<IMenuItem>) optionItems, (object) 5);
  }

  public ISettingListMenuViewModel GetAudioOptions()
  {
    return (ISettingListMenuViewModel) new SettingListMenuViewModel((IEnumerable<ISetting>) new ISetting[2]
    {
      (ISetting) new MenuViewFactory.MusicSetting(this._gameContext.Settings),
      (ISetting) new MenuViewFactory.SoundSetting(this._gameContext.Settings)
    }, (object) 5);
  }

  public ISettingListMenuViewModel GetMobileOptions()
  {
    S2HDSettings settings = this._gameContext.Settings;
    return (ISettingListMenuViewModel) new SettingListMenuViewModel((IEnumerable<ISetting>) new ISetting[1]
    {
      (ISetting) new OnOffOptionSetting("WIDESCREEN", (Func<bool>) (() => settings.AndroidWidescreen), (Action<bool>) (value => settings.AndroidWidescreen = value))
    }, (object) 5);
  }

  public ISettingListMenuViewModel GetVideoOptions()
  {
    S2HDSettings settings = this._gameContext.Settings;
    return (ISettingListMenuViewModel) new SettingListMenuViewModel((IEnumerable<ISetting>) new ISetting[5]
    {
      (ISetting) new StandardOptionSetting("MODE", new string[3]
      {
        "WINDOW",
        "FULLSCREEN",
        "BORDERLESS WINDOWED"
      }, (Func<int>) (() => (int) settings.Mode), (Action<int>) (value => settings.Mode = (VideoMode) value)),
      (ISetting) new StandardOptionSetting("RESOLUTION", new string[1]
      {
        "1920 X 1080"
      }, (Func<int>) (() => 0), (Action<int>) (value => { })),
      (ISetting) new OnOffOptionSetting("SHADOWS", (Func<bool>) (() => settings.EnableShadows), (Action<bool>) (value => settings.EnableShadows = value)),
      (ISetting) new OnOffOptionSetting("WATER EFFECTS", (Func<bool>) (() => settings.EnableWaterEffects), (Action<bool>) (value => settings.EnableWaterEffects = value)),
      (ISetting) new OnOffOptionSetting("HEAT EFFECTS", (Func<bool>) (() => settings.EnableHeatEffects), (Action<bool>) (value => settings.EnableHeatEffects = value))
    }, (object) 5);
  }

  private class MusicSetting : IAudioSliderSetting, ISetting
  {
    private readonly S2HDSettings _settings;

    public string Name => "MUSIC";

    public double Value
    {
      get => this._settings.MusicVolume;
      set => this._settings.MusicVolume = value;
    }

    public MusicSetting(S2HDSettings settings) => this._settings = settings;
  }

  private class SoundSetting : IAudioSliderSetting, ISetting
  {
    private readonly S2HDSettings _settings;

    public string Name => "SFX";

    public double Value
    {
      get => this._settings.SoundVolume;
      set => this._settings.SoundVolume = value;
    }

    public SoundSetting(S2HDSettings settings) => this._settings = settings;
  }
}

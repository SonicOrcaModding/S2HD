#if __ANDROID__ || __IOS__
using System;
using SonicOrca;
using SonicOrca.Input;

namespace S2HD.Menu;

internal static class AndroidOptionsMenuTouchInput
{
  private const int SwipeThresholdPx = 96;
  private const int TapMoveThresholdPx = 24;
  private static bool _touchTracking;
  private static int _touchStartX, _touchStartY;
  private static int _touchMaxDistanceSquared;

  public static void ProcessFrame(
    S2HDSonicOrcaGameContext gameContext,
    out int horizontal,
    out int vertical,
    out bool tap)
  {
    horizontal = 0;
    vertical = 0;
    tap = false;
    MouseState currentMouse = gameContext.Input.CurrentState.Mouse;
    MouseState pressedMouse = gameContext.Input.Pressed.Mouse;
    MouseState releasedMouse = gameContext.Input.Released.Mouse;
    if (pressedMouse.Left)
    {
      _touchTracking = true;
      _touchStartX = currentMouse.X;
      _touchStartY = currentMouse.Y;
      _touchMaxDistanceSquared = 0;
    }
    if (_touchTracking && currentMouse.Left)
    {
      int dx = currentMouse.X - _touchStartX;
      int dy = currentMouse.Y - _touchStartY;
      int distanceSquared = dx * dx + dy * dy;
      if (distanceSquared > _touchMaxDistanceSquared)
        _touchMaxDistanceSquared = distanceSquared;
    }
    if (!_touchTracking || !releasedMouse.Left)
      return;
    int releaseDx = currentMouse.X - _touchStartX;
    int releaseDy = currentMouse.Y - _touchStartY;
    _touchTracking = false;
    int absX = Math.Abs(releaseDx);
    int absY = Math.Abs(releaseDy);
    int swipeThresholdSquared = SwipeThresholdPx * SwipeThresholdPx;
    bool isSwipe = absX >= SwipeThresholdPx || absY >= SwipeThresholdPx || _touchMaxDistanceSquared >= swipeThresholdSquared;
    if (!isSwipe)
    {
      if (_touchMaxDistanceSquared <= TapMoveThresholdPx * TapMoveThresholdPx)
        tap = true;
      return;
    }
    if (absX >= absY && absX >= SwipeThresholdPx)
    {
      horizontal = releaseDx > 0 ? 1 : -1;
      return;
    }
    if (absY >= SwipeThresholdPx)
      vertical = releaseDy > 0 ? 1 : -1;
  }
}
#endif

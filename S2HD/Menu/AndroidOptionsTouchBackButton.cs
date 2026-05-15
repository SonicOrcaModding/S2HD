using SonicOrca;
using SonicOrca.Drawing.Renderers;
#if __ANDROID__
using SonicOrca.Geometry;
using SonicOrca.Graphics;
#endif

namespace S2HD.Menu;

internal static class AndroidOptionsTouchBackButton
{
#if __ANDROID__
  private const double TouchPauseSizeFactor = 72.0 / 320.0;

  public static Rectangle GetLogicalViewport(SonicOrcaGameContext gameContext)
  {
    Rectangle surface = new Rectangle(0.0, 0.0, 1920.0, 1080.0);
    Rectangle viewport = surface;
    Vector2i clientSize = gameContext.Window.ClientSize;
    Vector2i aspectRatio = gameContext.Window.AspectRatio;
    if (clientSize.X > 0 && clientSize.Y > 0 && aspectRatio.X > 0 && aspectRatio.Y > 0)
    {
      double targetAspect = (double) aspectRatio.X / (double) aspectRatio.Y;
      double surfaceAspect = (double) clientSize.X / (double) clientSize.Y;
      if (surfaceAspect > targetAspect)
      {
        double width = surface.Height * targetAspect;
        double x = (surface.Width - width) / 2.0;
        viewport = new Rectangle(x, 0.0, width, surface.Height);
      }
      else if (surfaceAspect < targetAspect)
      {
        double height = surface.Width / targetAspect;
        double y = (surface.Height - height) / 2.0;
        viewport = new Rectangle(0.0, y, surface.Width, height);
      }
    }
    return viewport;
  }

  public static Rectangle GetBackButtonRect(Rectangle logicalViewport)
  {
    double pauseSize = System.Math.Min(logicalViewport.Width, logicalViewport.Height) * TouchPauseSizeFactor;
    return new Rectangle(logicalViewport.X + 24.0, logicalViewport.Y + 24.0, pauseSize, pauseSize);
  }

  public static bool TryWindowPixelsToLogical(
    SonicOrcaGameContext gameContext,
    int windowX,
    int windowY,
    out double logicalX,
    out double logicalY)
  {
    logicalX = 0.0;
    logicalY = 0.0;
    Vector2i clientSize = gameContext.Window.ClientSize;
    int windowWidth = clientSize.X;
    int windowHeight = clientSize.Y;
    if (windowWidth <= 0 || windowHeight <= 0)
      return false;
    int viewportX = 0;
    int viewportY = 0;
    int viewportWidth = windowWidth;
    int viewportHeight = windowHeight;
    Vector2i aspectRatio = gameContext.Window.AspectRatio;
    if (aspectRatio.X > 0 && aspectRatio.Y > 0)
    {
      double targetAspect = (double) aspectRatio.X / (double) aspectRatio.Y;
      double surfaceAspect = (double) windowWidth / (double) windowHeight;
      if (surfaceAspect > targetAspect)
      {
        viewportWidth = (int) System.Math.Round((double) windowHeight * targetAspect);
        viewportX = (windowWidth - viewportWidth) / 2;
      }
      else if (surfaceAspect < targetAspect)
      {
        viewportHeight = (int) System.Math.Round((double) windowWidth / targetAspect);
        viewportY = (windowHeight - viewportHeight) / 2;
      }
    }
    if (windowX < viewportX || windowX >= viewportX + viewportWidth || windowY < viewportY || windowY >= viewportY + viewportHeight)
      return false;
    Rectangle logicalVp = GetLogicalViewport(gameContext);
    logicalX = logicalVp.X + (double) (windowX - viewportX) * logicalVp.Width / (double) viewportWidth;
    logicalY = logicalVp.Y + (double) (windowY - viewportY) * logicalVp.Height / (double) viewportHeight;
    return true;
  }

  public static void TryHandleReleasedTap(SonicOrcaGameContext gameContext, MenuViewPresenterHost host)
  {
    if (host == null || !gameContext.Input.Released.Mouse.Left)
      return;
    int mx = gameContext.Input.Released.Mouse.X;
    int my = gameContext.Input.Released.Mouse.Y;
    if (!TryWindowPixelsToLogical(gameContext, mx, my, out double lx, out double ly))
      return;
    Rectangle back = GetBackButtonRect(GetLogicalViewport(gameContext));
    if (lx >= back.X && lx < back.Right && ly >= back.Y && ly < back.Bottom)
      host.RequestNavigateBack();
  }

  public static void DrawBackButton(Renderer renderer, SonicOrcaGameContext gameContext)
  {
    I2dRenderer g = renderer.Get2dRenderer();
    Rectangle logicalVp = GetLogicalViewport(gameContext);
    Rectangle backRect = GetBackButtonRect(logicalVp);
    AndroidTouchControlAssets.EnsureLoaded(gameContext);
    if (AndroidTouchControlAssets.Available && AndroidTouchControlAssets.ButtonBack != null)
    {
      g.BlendMode = BlendMode.Alpha;
      g.Colour = Colours.White;
      g.RenderTexture(AndroidTouchControlAssets.ButtonBack, backRect);
    }
    else
    {
      g.RenderEllipse(new Colour(88, 255, 255, 255), backRect.Centre, backRect.Width * 0.35, backRect.Width * 0.5, 48);
      Vector2 c = backRect.Centre;
      double w = backRect.Width;
      double h = backRect.Height;
      g.RenderLine(new Colour(170, 255, 255, 255), new Vector2(c.X + w * 0.12, c.Y - h * 0.16), new Vector2(c.X - w * 0.08, c.Y), 10.0);
      g.RenderLine(new Colour(170, 255, 255, 255), new Vector2(c.X - w * 0.08, c.Y), new Vector2(c.X + w * 0.12, c.Y + h * 0.16), 10.0);
    }
  }
#else
  public static void TryHandleReleasedTap(SonicOrcaGameContext gameContext, MenuViewPresenterHost host)
  {
  }

  public static void DrawBackButton(Renderer renderer, SonicOrcaGameContext gameContext)
  {
  }
#endif
}

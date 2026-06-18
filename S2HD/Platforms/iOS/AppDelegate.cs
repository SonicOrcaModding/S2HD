using Foundation;
using SonicOrca;
using SonicOrca.SDL2;
using System;
using System.IO;
using System.Threading;
using UIKit;

namespace S2HD
{
    [Register("AppDelegate")]
    public class AppDelegate : UIApplicationDelegate
    {
        private bool _gameThreadStarted;

        public override bool FinishedLaunching(UIApplication application, NSDictionary launchOptions)
        {
            if (_gameThreadStarted)
                return true;
            _gameThreadStarted = true;

            AppDomain.CurrentDomain.UnhandledException += (_, e) =>
                ShowFatalError("Unhandled exception:\n" + e.ExceptionObject);

            string userData = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                "SonicOrca");
            Program.SetIOSUserDataDirectory(userData);

            UIApplication.SharedApplication.IdleTimerDisabled = true;

            string contentRoot =
                Path.GetDirectoryName(typeof(AppDelegate).Assembly.Location)
                ?? NSBundle.MainBundle.BundlePath;
            GamePaths.ContentRootDirectory = contentRoot;

            string dataFile = Path.Combine(contentRoot, "data", "sonicorca.dat");
            string shadersDir = Path.Combine(contentRoot, "shaders");
            if (!File.Exists(dataFile) || !Directory.Exists(shadersDir))
            {
                ShowFatalError(
                    "Game files not found.\n\n" +
                    "data/sonicorca.dat: " + (File.Exists(dataFile) ? "OK" : "MISSING") + "\n" +
                    "shaders/: " + (Directory.Exists(shadersDir) ? "OK" : "MISSING") + "\n\n" +
                    "Content root:\n" + contentRoot);
                return true;
            }

            var thread = new Thread(() =>
            {
                try
                {
                    Program.StartFromiOS();
                }
                catch (Exception ex)
                {
                    ShowFatalError(ex.ToString());
                }
                finally
                {
                    Environment.Exit(0);
                }
            }) { Name = "S2HDGame" };
            thread.Start();

            return true;
        }

        internal static void ShowFatalError(string message)
        {
            var done = new ManualResetEventSlim(false);
            UIApplication.SharedApplication.InvokeOnMainThread(() =>
            {
                try
                {
                    var window = new UIWindow(UIScreen.MainScreen.Bounds);
                    window.WindowLevel = 1000;
                    window.RootViewController = new UIViewController();
                    window.MakeKeyAndVisible();

                    string truncated = message.Length > 3500
                        ? message.Substring(0, 3500) + "…" : message;
                    var alert = UIAlertController.Create(
                        "S2HD Error", truncated, UIAlertControllerStyle.Alert);
                    alert.AddAction(UIAlertAction.Create("Copy", UIAlertActionStyle.Default, _ =>
                    {
                        UIPasteboard.General.String = message;
                        window.Hidden = true;
                        done.Set();
                    }));
                    alert.AddAction(UIAlertAction.Create("OK", UIAlertActionStyle.Cancel, _ =>
                    {
                        window.Hidden = true;
                        done.Set();
                    }));
                    window.RootViewController.PresentViewController(alert, true, null);
                }
                catch
                {
                    done.Set();
                }
            });
            done.Wait(TimeSpan.FromSeconds(60));
        }

        public override void DidEnterBackground(UIApplication application)
        {
            SDL2WindowContext.iOSSuspended = true;
        }

        public override void WillEnterForeground(UIApplication application)
        {
            SDL2WindowContext.iOSSuspended = false;
        }
    }

    public static class IOSApplication
    {
        static void Main(string[] args)
        {
            UIApplication.Main(args, null, "AppDelegate");
        }
    }
}

using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using Android.Util;
using SonicOrca;
using System;
using System.IO;
using System.Threading;

namespace S2HD
{
    [Activity(
        Label = "S2HD",
        Theme = "@android:style/Theme.Black.NoTitleBar.Fullscreen",
        ScreenOrientation = ScreenOrientation.SensorLandscape,
        MainLauncher = true,
        LaunchMode = LaunchMode.SingleInstance,
        ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.ScreenSize | ConfigChanges.KeyboardHidden)]
    public class MainActivity : Org.Libsdl.App.SDLActivity
    {
        private bool _gameThreadStarted;

        protected MainActivity(IntPtr javaReference, JniHandleOwnership transfer)
            : base(javaReference, transfer)
        {
        }

        public MainActivity()
        {
        }

        protected override void OnStart()
        {
            base.OnStart();

            if (_gameThreadStarted)
                return;
            _gameThreadStarted = true;

            string userData = Path.Combine(FilesDir!.AbsolutePath, "SonicOrca");
            Program.SetAndroidUserDataDirectory(userData);
            Log.Info("S2HD", "Set user data dir (sonicorca.cfg + sonicorca.log): {0}", userData);

            EnsureBundledContentExtracted();

            var thread = new Thread(RunGameThread) { Name = "S2HDGame" };
            thread.Start();
        }

        private void EnsureBundledContentExtracted()
        {
            string contentRoot = Path.Combine(FilesDir!.AbsolutePath, "SonicOrca", "Content");
            string dataSentinel = Path.Combine(contentRoot, "data", "sonicorca.dat");
            string shaderSentinel = Path.Combine(contentRoot, "shaders", "greyscale_filter.shader");
            if (!File.Exists(dataSentinel) || !File.Exists(shaderSentinel))
            {
                ExtractAssetDirectory("data", Path.Combine(contentRoot, "data"));
                ExtractAssetDirectory("shaders", Path.Combine(contentRoot, "shaders"));
            }

            GamePaths.ContentRootDirectory = contentRoot;
            Log.Info("S2HD", "Bundled content root: {0}", contentRoot);
        }

        private void ExtractAssetDirectory(string assetDir, string targetDir)
        {
            Directory.CreateDirectory(targetDir);

            string[]? entries = Assets?.List(assetDir);
            if (entries == null)
                return;

            foreach (string entry in entries)
            {
                string assetPath = assetDir.TrimEnd('/') + "/" + entry;
                string targetPath = Path.Combine(targetDir, entry);

                string[]? children = Assets?.List(assetPath);
                if (children != null && children.Length > 0)
                {
                    ExtractAssetDirectory(assetPath, targetPath);
                    continue;
                }

                using (Stream input = Assets!.Open(assetPath))
                using (FileStream output = File.Create(targetPath))
                    input.CopyTo(output);
            }
        }

        private static void RunGameThread()
        {
            try
            {
                Program.StartFromAndroid();
            }
            catch (Exception ex)
            {
                Log.Error("S2HD", "Managed exception on game thread: " + ex);
            }
        }
    }
}

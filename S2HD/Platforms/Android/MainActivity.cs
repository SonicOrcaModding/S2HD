using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using Android.Util;
using SonicOrca;
using SonicOrca.SDL2;
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

            string userData = Path.Combine(GetAndroidAppFilesRoot(), "SonicOrca");
            Program.SetAndroidUserDataDirectory(userData);
            Log.Info("S2HD", "Set user data dir (sonicorca.cfg + sonicorca.log): {0}", userData);

            EnsureBundledContentExtracted();

            var thread = new Thread(() =>
            {
                try
                {
                    Program.StartFromAndroid();
                    RunOnUiThread(() => FinishAffinity());
                }
                catch (Exception ex)
                {
                    Log.Error("S2HD", "Managed exception on game thread: " + ex);
                }
            }) { Name = "S2HDGame" };
            thread.Start();
        }

        private void EnsureBundledContentExtracted()
        {
            string contentRoot = Path.Combine(GetAndroidAppFilesRoot(), "SonicOrca", "Content");
            ExtractAssetDirectory("data", Path.Combine(contentRoot, "data"));
            ExtractAssetDirectory("shaders", Path.Combine(contentRoot, "shaders"));
            ExtractAssetDirectory("mobile_assets", Path.Combine(contentRoot, "data", "mobile_assets"));

            GamePaths.ContentRootDirectory = contentRoot;
            Log.Info("S2HD", "Bundled content root: {0}", contentRoot);
        }

        private string GetAndroidAppFilesRoot()
        {
            string? externalFiles = GetExternalFilesDir(null)?.AbsolutePath;
            return !string.IsNullOrEmpty(externalFiles) ? externalFiles : FilesDir!.AbsolutePath;
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

        protected override void OnPause()
        {
            base.OnPause();
            SDL2WindowContext.AndroidSuspended = true;
        }

        protected override void OnResume()
        {
            base.OnResume();
        }

        public override void OnWindowFocusChanged(bool hasFocus)
        {
            base.OnWindowFocusChanged(hasFocus);
            if (hasFocus)
                SDL2WindowContext.AndroidSuspended = false;
            else
                SDL2WindowContext.AndroidSuspended = true;
        }
    }
}

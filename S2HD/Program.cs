// Decompiled with JetBrains decompiler
// Type: S2HD.Program
// Assembly: S2HD, Version=2.0.1012.10521, Culture=neutral, PublicKeyToken=null
// MVID: 18631A0F-16CF-4E18-8563-1EC5E54750D6
// Assembly location: C:\Games\S2HD_2.0.1012-rc2\S2HD.exe

using SonicOrca;
using SonicOrca.SDL2;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;

namespace S2HD
{

    public static class Program
    {
      public const string BuildConfigurationName = "RELEASE";
      public static Version AppVersion = Assembly.GetExecutingAssembly().GetName().Version;
      public static string AppArchitecture = Environment.Is64BitProcess ? "x64" : "x86";
      public static Version AppMinOpenGLVersion = new Version(3, 3);
      private static string IniConfigurationPath = "sonicorca.cfg";

      public static IniConfiguration Configuration { get; private set; }

      public static string AppBaseDirectory => AppContext.BaseDirectory;

      public static string UserDataDirectory
      {
#if MONO_NX
        get => Path.Combine(Program.AppBaseDirectory, "userdata");
#else
        get => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "SonicOrca");
#endif
      }

      private static string LogPath => Path.Combine(Program.UserDataDirectory, "sonicorca.log");

      public static IReadOnlyList<string> CommandLineArguments { get; private set; }

      private static void Main(string[] args)
      {
        Program.CommandLineArguments = (IReadOnlyList<string>) args;
        Program.EnsureUserDataDirectoryExists();
        Program.WriteLogHeader();
        Program.LoadConfiguration();
        Program.RunOrFocusGame();
      }

      private static void EnsureUserDataDirectoryExists()
      {
        if (Directory.Exists(Program.UserDataDirectory))
          return;
        Directory.CreateDirectory(Program.UserDataDirectory);
      }

      private static void LoadConfiguration()
      {
        string path = Path.Combine(Program.UserDataDirectory, Program.IniConfigurationPath);
        if (File.Exists(path))
        {
          Program.Configuration = new IniConfiguration(path);
        }
        else
        {
          Program.Configuration = new IniConfiguration();
          Program.Configuration.SetProperty("video", "fullscreen", "0");
          Program.Configuration.SetProperty("audio", "volume", "1.0");
          Program.Configuration.SetProperty("audio", "music_volume", "0.5");
          Program.Configuration.SetProperty("audio", "sound_volume", "1.0");
          Program.Configuration.Save(path);
        }
      }

      private static void RunOrFocusGame()
      {
#if MONO_NX
        Program.RunGame();
#else
        bool createdNew;
        using (new Mutex(true, "SonicOrca", out createdNew))
        {
          if (createdNew || Program.Configuration.GetPropertyBoolean("debug", "allow_multiple_instances"))
            Program.RunGame();
          else
            Program.FocusGame();
        }
#endif
      }

      private static IPlatform GetPlatform() => (IPlatform) SDL2Platform.Instance;

      private static void RunGame()
      {
#if MONO_NX
        SonicOrcaGameContext.IsMaxPerformance = true;
#endif
        using (IPlatform platform = Program.GetPlatform())
        {
          try
          {
            platform.Initialise();
          }
          catch (Exception ex)
          {
            Program.LogException(ex);
            Program.ShowErrorMessageBox(ex.Message);
            return;
          }
          if (!Program.CheckOpenGL(platform))
            return;
          using (S2HDSonicOrcaGameContext sonicOrcaGameContext = new S2HDSonicOrcaGameContext(platform))
          {
            try
            {
              Trace.WriteLine("Initialising game");
              Trace.Indent();
              sonicOrcaGameContext.Initialise();
              Trace.Unindent();
              Trace.WriteLine("Running game");
              Trace.Indent();
              sonicOrcaGameContext.Run();
            }
            catch (Exception ex)
            {
              if (Debugger.IsAttached)
                throw;
              Program.LogException(ex);
              Program.ShowErrorMessageBox(ex.Message);
            }
            finally
            {
              Trace.Unindent();
            }
          }
        }
        Trace.Unindent();
        Trace.WriteLine(new string('-', 80 /*0x50*/));
        Trace.WriteLine(string.Empty);
        Trace.Flush();
      }

      private static void FocusGame()
      {
        Process current = Process.GetCurrentProcess();
        Process process = ((IEnumerable<Process>) Process.GetProcessesByName(current.ProcessName)).Where<Process>((Func<Process, bool>) (x => x.Id != current.Id)).FirstOrDefault<Process>();
        if (process == null)
          return;
#if WINDOWS_MESSAGE_BOX
        WindowsShell.SetForegroundWindow(process.MainWindowHandle);
#endif
      }

      private static bool CheckOpenGL(IPlatform platform)
      {
#if MONO_NX
        return true;
#else
        Trace.WriteLine("Verifying OpenGL version");
        Version openGlVersion = platform.GetOpenGLVersion();
        Trace.WriteLine($"OpenGL {openGlVersion.Major}.{openGlVersion.Minor}");
        if (!(openGlVersion < Program.AppMinOpenGLVersion))
          return true;
        Trace.WriteLine("OpenGL version too low");
        Program.ShowErrorMessageBox($"OpenGL {Program.AppMinOpenGLVersion.Major}.{Program.AppMinOpenGLVersion.Minor} or later is required.");
        return false;
#endif
      }

      public static void ShowErrorMessageBox(string text)
      {
        Trace.WriteLine("ERROR: " + text);
        Console.Error.WriteLine(text);
#if WINDOWS_MESSAGE_BOX
        WindowsShell.ShowMessageBox(text, "SonicOrca");
#endif
      }

      private static void WriteLogHeader()
      {
#if MONO_NX
        Trace.AutoFlush = false;
#else
        Trace.AutoFlush = true;
#endif
        Trace.Listeners.Add((TraceListener) new TextWriterTraceListener(Program.LogPath));
#if !MONO_NX
        Trace.Listeners.Add((TraceListener) new ConsoleTraceListener());
#endif
        Trace.WriteLine((object) Environment.OSVersion);
        Trace.WriteLine($"SonicOrca {Program.AppVersion} [{"RELEASE"} {Program.AppArchitecture}]");
        Trace.WriteLine(DateTime.Now.ToString("dd MMMM yyyy @ hh:mm tt"));
        Trace.WriteLine(new string('-', 80 /*0x50*/));
        Trace.Indent();
      }

      public static void LogException(Exception ex, bool logStackTrace = true)
      {
        string[] strArray1 = ex.Message.Split(Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
        if (strArray1.Length <= 1)
        {
          Trace.WriteLine("EXCEPTION: " + strArray1[0]);
        }
        else
        {
          Trace.WriteLine("EXCEPTION:");
          Trace.Indent();
          foreach (string message in strArray1)
            Trace.WriteLine(message);
          Trace.Unindent();
        }
        if (ex.InnerException != null)
        {
          Trace.Indent();
          Program.LogException(ex.InnerException);
          Trace.Unindent();
        }
        if (!logStackTrace || string.IsNullOrEmpty(ex.StackTrace))
        {
          Trace.Flush();
          return;
        }
        string[] strArray2 = ex.StackTrace.Split(Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
        Trace.WriteLine("STACK TRACE:");
        Trace.Indent();
        foreach (string str in strArray2)
          Trace.WriteLine(str.Trim());
        Trace.Unindent();
        Trace.Flush();
      }
    }
}

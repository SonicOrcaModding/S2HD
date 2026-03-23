#if WINDOWS_MESSAGE_BOX
using System;
using System.Runtime.InteropServices;

namespace S2HD
{
    internal static class WindowsShell
    {
        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        private static extern int MessageBox(IntPtr hwnd, string text, string caption, uint type);

        [DllImport("user32.dll")]
        internal static extern int SetForegroundWindow(IntPtr hWnd);

        internal static void ShowMessageBox(string text, string caption)
        {
            MessageBox(IntPtr.Zero, text, caption, 48U);
        }
    }
}
#endif

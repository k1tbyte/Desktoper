using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Automation;

// ReSharper disable InconsistentNaming

namespace Desktoper.Services
{
    public static class Win32Window
    {
        public static event Action<Process, IntPtr> OnWindowOpened;
        private static bool _isHooked = false;
        
        public static void HookOnWindowOpened()
        {
            if (_isHooked)
            {
                return;
            }
            
            _isHooked = true;
            Automation.AddAutomationEventHandler(
                WindowPattern.WindowOpenedEvent, AutomationElement.RootElement, TreeScope.Children, WindowOpenedEvent
            );
        }

        public static void UnhookOnWindowOpened()
        {
            Automation.RemoveAutomationEventHandler(
                WindowPattern.WindowOpenedEvent, AutomationElement.RootElement, WindowOpenedEvent
            );
            _isHooked = false;
        }

        private static void WindowOpenedEvent(object sender, AutomationEventArgs e)
        {
            var window = (AutomationElement)sender;
            if (IsOverlayStyleWindow((IntPtr)window.Current.NativeWindowHandle))
            {
                return;
            }
            var process = Process.GetProcessById(window.Current.ProcessId);
            OnWindowOpened?.Invoke(process,(IntPtr) window.Current.NativeWindowHandle);
        }
        
        public static bool IsOverlayStyleWindow(IntPtr hWnd)
        {
            int exStyle = GetWindowLong(hWnd, GWL_EXSTYLE);
            return (exStyle & WS_EX_LAYERED) == WS_EX_LAYERED || (exStyle & WS_EX_TOOLWINDOW) == WS_EX_TOOLWINDOW;
        }
        
        public static HashSet<string> GetOpenedWindowProcesses()
        {
            IntPtr shellWindow = GetShellWindow();
            var processes = new HashSet<string>();

            EnumWindows(delegate(IntPtr hWnd, int lParam)
            {
                if (hWnd == shellWindow || !IsWindowVisible(hWnd) 
                                        || IsOverlayStyleWindow(hWnd)
                                        || GetWindowTextLength(hWnd) == 0)
                {
                    return true;
                }
                
                uint processId;
                GetWindowThreadProcessId(hWnd, out processId);
                var processName = Process.GetProcessById((int)processId).ProcessName;
                processes.Add(processName);
                
                return true;

            }, 0);

            return processes;
        }

        #region Imports

        
        [DllImport("user32.dll")]
        public static extern int GetWindowLong(IntPtr hWnd, int nIndex);
        
        private delegate bool EnumWindowsProc(IntPtr hWnd, int lParam);

        [DllImport("user32.dll")]
        private static extern bool EnumWindows(EnumWindowsProc enumFunc, int lParam);

        [DllImport("user32.dll")]
        private static extern int GetWindowTextLength(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern bool IsWindowVisible(IntPtr hWnd);
        
        [DllImport("user32.dll")]
        public static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);
        
        [DllImport("user32.dll")]
        private static extern IntPtr GetShellWindow();
        
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

        public struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }
        
        public const int GWL_EXSTYLE = -20;
        public const int WS_EX_LAYERED = 0x00080000; // Прозрачное окно
        public const int WS_EX_TOOLWINDOW = 0x00000080;

        #endregion
    }
}
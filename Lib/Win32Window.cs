using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Automation;

// ReSharper disable InconsistentNaming

namespace Desktoper.Lib
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

        public static void MoveWindow(IntPtr hWnd, int? x = null, int? y = null, int? width=null, int? height=null)
        {
            if (!x.HasValue && !y.HasValue && !width.HasValue && !height.HasValue)
            {
                return;
            }

            if (!GetWindowRect(hWnd, out var rect))
            {
                return;
            }
            
            SetWindowPos(hWnd, 
                IntPtr.Zero, 
                x ?? rect.Left,  
                y ?? rect.Top, 
                width ?? rect.Right - rect.Left, 
                height ?? rect.Bottom - rect.Top, 
                SWP_NOZORDER | SWP_SHOWWINDOW);
        }

        public static Rectangle? GetWindowSize(IntPtr hWnd)
        {
            if (!GetWindowRect(hWnd, out Rect rect))
            {
                return null;
            }
            
            return new Rectangle(rect.Left, rect.Top, rect.Right - rect.Left, rect.Bottom - rect.Top);
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
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);
        
        [DllImport("user32.dll")]
        private static extern IntPtr GetShellWindow();
        
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetWindowRect(IntPtr hWnd, out Rect lpRect);
        
        [DllImport("user32.dll")]
        private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);
        
        [DllImport("user32.dll")]
        private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        public struct Rect
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }
        
        private const int SWP_NOSIZE = 0x0001;
        private const int SWP_NOMOVE = 0x0002;
        private const int SWP_NOZORDER = 0x0004;
        private const int SWP_SHOWWINDOW = 0x0040;
        
        public const int GWL_EXSTYLE = -20;
        public const int WS_EX_LAYERED = 0x00080000; 
        public const int WS_EX_TOOLWINDOW = 0x00000080;

        #endregion
    }
}
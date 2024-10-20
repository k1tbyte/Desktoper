using System;
using System.Collections.Generic;
using System.Diagnostics;
using Desktoper.Lib;
using Desktoper.MVVM.Model;

namespace Desktoper.Services
{
    public sealed class WindowManager : IDisposable
    {
        private bool _isListening ;
        
        
        private void OnWindowOpenedCallback(Process process, IntPtr windowHandle)
        {
            if (process.MainWindowHandle != windowHandle ||
                !Config.ListeningProcesses.TryGetValue(process.ProcessName, out var window) ||
                window.IsEnabled == false || window.IsAligned)
            {
                return;
            }
            
            ReplicateDesktops(window.DesktopIndex ,0,
                (o, _) => o.MoveWindow(process.MainWindowHandle)
            );
            
            Win32Window.MoveWindow(windowHandle, window.X, window.Y, window.Width, window.Height);

            if (window.IsEnabled == null)
            {
                window.IsAligned = true;
            }
        }
        
        public static void ReplicateDesktops(int start,
            int end = 0, 
            Action<VirtualDesktopManager.Desktop, int> callback = null)
        {
            var total = VirtualDesktopManager.Desktop.Count;
            for (int i = total > start ? start : total; i <= (end == 0 ? start : end); i++)
            {
                var desktop = i < total ? VirtualDesktopManager.Desktop.FromIndex(i) : VirtualDesktopManager.Desktop.Create();
                ReplicateName(desktop, i);
                callback?.Invoke(desktop,i);
            }
            
            return;

            void ReplicateName(VirtualDesktopManager.Desktop desktop, int i)
            {
                if (!string.IsNullOrEmpty(Config.Current.Desktops[i].Name))
                {
                    desktop.SetName(Config.Current.Desktops[i].Name);
                }
            }
        }

        public static void ReplicateApps()
        {
            if (Config.Current.Desktops.Count == 0)
            {
                return;
            }

            var desktops = new Dictionary<int, VirtualDesktopManager.Desktop>();
            ReplicateDesktops(0, Config.Current.Desktops.Count-1, (desktop, i) => desktops.Add(i, desktop));
            
            foreach (var process in Process.GetProcesses())
            {
                if (process.MainWindowHandle == IntPtr.Zero || 
                    !Config.ListeningProcesses.TryGetValue(process.ProcessName, out var window))
                {
                    continue;
                }
                desktops[window.DesktopIndex].MoveWindow(process.MainWindowHandle);
                Win32Window.MoveWindow(process.MainWindowHandle, window.X, window.Y, window.Width, window.Height);
            }
        }

        public void Execute(bool state)
        {
            switch (state)
            {
                case true when _isListening:
                    return;
                case true when !_isListening:
                    Win32Window.OnWindowOpened += OnWindowOpenedCallback;
                    Win32Window.HookOnWindowOpened();
                    _isListening = true;
                    return;
                default:
                    Dispose();
                    break;
            }
        }


        public void Dispose()
        {
            if (!_isListening)
            {
                return;
            }
            
            Win32Window.OnWindowOpened -= OnWindowOpenedCallback;
            Win32Window.UnhookOnWindowOpened();
            _isListening = false;
        }
    }
}
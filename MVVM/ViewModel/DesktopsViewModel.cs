using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Desktoper.Lib;
using Desktoper.MVVM.Core;
using Desktoper.MVVM.Model;
using Desktoper.MVVM.View;
using Desktoper.Services;

namespace Desktoper.MVVM.ViewModel
{
    public sealed class DesktopsViewModel : ObservableObject
    {
        private Desktop _selectedDesktop = null;
        private int _selectedDesktopIndex;
        private string[] _availableProcesses;

        public Desktop SelectedDesktop
        {
            get => _selectedDesktop;
            set => SetProperty(ref _selectedDesktop, value);
        }

        public int SelectedDesktopIndex
        {
            get => _selectedDesktopIndex;
            set => SetProperty(ref _selectedDesktopIndex, value + 1);
        }

        public string[] AvailableProcesses
        {
            get => _availableProcesses;
            set => SetProperty(ref _availableProcesses, value);
        }
        
        public DesktopsView View { get; set; }

        public ObservableCollection<Desktop> Desktops => Config.Current.Desktops;
        
        
        public RelayCommand AddNewDesktopCommand { get; private set; }
        public RelayCommand RemoveDesktopCommand { get; private set; }
        public RelayCommand OpenAddProcessContextCommand { get; private set; }
        public RelayCommand AttachWindowToDesktopCommand { get; private set; }
        public RelayCommand GetCurrentWindowPropertiesCommand { get; private set; }

        private void AddNewDesktop(object parameter)
        {
            var newDesktop = new Desktop();
            Desktops.Add(newDesktop);
            SelectedDesktop = newDesktop;
            View.DesktopsListBox.ScrollIntoView(newDesktop);
            Config.Current.Save();
        }

        private void RemoveDesktop(object parameter)
        {
            var desktop = (Desktop)parameter;
            Desktops.Remove(desktop);
            if (desktop == _selectedDesktop)
            {
                _selectedDesktop = null;
            }
            Config.Current.Save();
        }

        private void RenameCurrentDesktop(object sender, RoutedEventArgs e)
        {
            var text = (sender as TextBox).Text;
            if (text == _selectedDesktop.Name ||
                (string.IsNullOrEmpty(text) && string.IsNullOrEmpty(SelectedDesktop.Name)))
            {
                return;
            }

            _selectedDesktop.Name = text;
            WindowManager.ReplicateDesktops(_selectedDesktopIndex - 1);
            Config.Current.Save();
        }

        public void Initialize()
        {
            View.DesktopNameTextBox.LostFocus += RenameCurrentDesktop;
        }

        private void OpenAddProcessContext(object parameter)
        {
            var processes = Win32Window.GetOpenedWindowProcesses();
            AvailableProcesses = processes
                .Except(SelectedDesktop.Windows.Select(o => o.ProcessName))
                .Except(Config.ListeningProcesses.Keys)
                .ToArray();
            
            if (AvailableProcesses.Length == 0)
            {
                return;
            }
            
            var menu = (parameter as Button).ContextMenu;
            menu.DataContext = this;
            menu.IsOpen = true;
        }

        private void AttachWindowToDesktop(object parameter)
        {
            SelectedDesktop.Windows.Add(
                new DesktopWindow
                {
                    ProcessName = (string)parameter, 
                    DesktopIndex = SelectedDesktopIndex-1,
                    IsEnabled = true
                });
            Config.OrganizeListeningProcesses();
            Config.Current.Save();
        }

        private void GetCurrentWindowProperties(object parameter)
        {
            Rectangle? size = null;
            var desktop = (DesktopWindow)parameter;
            var hWnd = Process.GetProcessesByName(desktop.ProcessName)
                .FirstOrDefault(o => o.MainWindowHandle != IntPtr.Zero)?.MainWindowHandle;

            if (hWnd == null || hWnd == IntPtr.Zero || (size = Win32Window.GetWindowSize(hWnd.Value)) == null)
            {
                return;
            }

            desktop.X = size.Value.X;
            desktop.Y = size.Value.Y;
            desktop.Width = size.Value.Width;
            desktop.Height = size.Value.Height;
        }
        
        public DesktopsViewModel()
        {
            AddNewDesktopCommand = new RelayCommand(AddNewDesktop);
            RemoveDesktopCommand = new RelayCommand(RemoveDesktop);
            OpenAddProcessContextCommand = new RelayCommand(OpenAddProcessContext);
            AttachWindowToDesktopCommand = new RelayCommand(AttachWindowToDesktop);
            GetCurrentWindowPropertiesCommand = new RelayCommand(GetCurrentWindowProperties);
        }
    }
}
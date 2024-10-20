using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Desktoper.MVVM.Model;
using Desktoper.MVVM.ViewModel;

namespace Desktoper.MVVM.View
{
    public partial class DesktopsView : Grid
    {
        public DesktopsView()
        {
            InitializeComponent();
            var context = DataContext as DesktopsViewModel;
            // ReSharper disable once PossibleNullReferenceException
            context.View = this;
            context.Initialize();
        }
        
        private void NumberFilter(object sender, TextCompositionEventArgs e)
        {
            if (!int.TryParse(e.Text, out _))
            {
                e.Handled = true;
            }
        }
        
        private void ValidateSize(object sender, KeyboardFocusChangedEventArgs e)
        {
            var textBox = (TextBox)sender;

            if (!int.TryParse(textBox.Text, out var size))
            {
                return;
            }
            var reset = false;
            
            switch (textBox.Name)
            {
                case "PosX" when size > SystemParameters.WorkArea.Width - 1:
                case "PosY" when size > SystemParameters.WorkArea.Height - 1:
                case "Width" when size < 1 || size > SystemParameters.WorkArea.Width:
                case "Height" when size < 1 || size > SystemParameters.WorkArea.Height:
                    reset = true;
                    break;
            }

            if (reset)
            {
                textBox.Text = "";
                e.Handled = true;
            }
        }

        private void Card_OnContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            var border = (Border)sender;
            var window = border.DataContext as DesktopWindow;

            var process = Process.GetProcessesByName(window.ProcessName).FirstOrDefault(o => o.MainWindowHandle != IntPtr.Zero);
            if (process == null || process.MainWindowHandle == IntPtr.Zero)
            {
                e.Handled = true;
            }
        }
    }
}
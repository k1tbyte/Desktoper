using System.Windows;
using System.Windows.Input;
using Desktoper.MVVM.View;
using Desktoper.Services;

namespace Desktoper
{
    public partial class MainWindow
    {
        public Config Config => Config.Current;

        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = this;
        }

        public void OpenPopup(object content, string title = null)
        {
            this.PopupHost.PopupContent = content;
            PopupHost.Title.Text = title;
            PopupHost.IsOpen = true;
        }

        private void OnDragMove(object sender, MouseButtonEventArgs e) => this.DragMove();

        private void OnClosing(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void ListeningStateMouseDown(object sender, MouseButtonEventArgs e)
        {
            Config.Current.IsListening = !Config.Current.IsListening;
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            OpenPopup(new SettingsView(), "Settings");
        }
    }
}
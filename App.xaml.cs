using System;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Automation;
using Desktoper.Services;
using System.Windows.Forms;
using Newtonsoft.Json;

namespace Desktoper
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        internal static readonly string WorkingDir;
        internal static readonly string ConfigPath;
        internal const string Name = "Desktoper";
        internal static JsonSerializerSettings JsonSettings = new JsonSerializerSettings
            { NullValueHandling = NullValueHandling.Ignore, };
        private NotifyIcon TrayIcon;
        

        static App()
        {
            WorkingDir      = Path.GetDirectoryName(Assembly.GetExecutingAssembly()?.Location) ?? throw new ArgumentNullException(nameof(WorkingDir));
            ConfigPath      = Path.Combine(WorkingDir, "config.json");
        }
        public static WindowManager WindowManager { get; } = new WindowManager();
        protected override void OnStartup(StartupEventArgs e)
        {
            this.ShutdownMode = ShutdownMode.OnExplicitShutdown;
            base.OnStartup(e);
            
            InitConfig();
            
            var trayMenu = new ContextMenu();
            trayMenu.MenuItems.Add("Replicate apps", (sender, args) => WindowManager.ReplicateApps());
            trayMenu.MenuItems.Add("Exit", (sender, args) => this.Shutdown());
            
            TrayIcon = new NotifyIcon()
            {
                Text    = "Desktoper",
                Icon    = System.Drawing.Icon.ExtractAssociatedIcon(Assembly.GetExecutingAssembly().Location),
                ContextMenu = trayMenu,
                Visible = true,
            };

            TrayIcon.DoubleClick += (sender, args) =>
            {
                if (this.MainWindow != null)
                {
                    this.MainWindow.Close();
                    this.MainWindow = null;
                    return;
                }
                this.MainWindow = new MainWindow();
                this.MainWindow.Show();
            };
        }

        private void InitConfig()
        {
            Config.Load();
            WindowManager.Execute(Config.Current.IsListening);
            
            Config.Current.PropertyChanged += (sender, args) =>
            {
                if(args.PropertyName != nameof(Config.Current.IsListening)) return;
                WindowManager.Execute(Config.Current.IsListening);
            };

            if (!Config.Current.StartMinimized)
            {
                this.MainWindow = new MainWindow();
                this.MainWindow.Show();
            }

            if (Config.Current.ReplicatePresetOnStartup)
            {
                WindowManager.ReplicateApps();
            }

        }

        protected override void OnExit(ExitEventArgs e)
        {
            WindowManager.Dispose();
            TrayIcon.Dispose();
            base.OnExit(e);
        }
        
    }
}
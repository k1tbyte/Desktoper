using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Desktoper.MVVM.Core;
using Desktoper.MVVM.Model;
using Newtonsoft.Json;

namespace Desktoper.Services
{
    public sealed class Config : ObservableConfig
    {
        public static Config Current { get; private set;  } 
        public static Dictionary<string, DesktopWindow> ListeningProcesses { get; } = new Dictionary<string, DesktopWindow>();
        private static CancellationTokenSource _saveTokenSource;
        private static bool _isLoading = false;
        
        
        
        private bool _isListening = true;
        private bool _startMinimized;
        private bool _replicatePresetOnStartup;

        public ObservableCollection<Desktop> Desktops { get; set; } = new ObservableCollection<Desktop>();

        public bool StartMinimized
        {
            get => _startMinimized;
            set => SetProperty(ref _startMinimized, value, nameof(StartMinimized));
        }

        public bool ReplicatePresetOnStartup
        {
            get => _replicatePresetOnStartup;
            set => SetProperty(ref _replicatePresetOnStartup, value, nameof(ReplicatePresetOnStartup));
        }

        public bool IsListening
        {
            get => _isListening;
            set => SetProperty(ref _isListening, value, nameof(IsListening));
        }

        public static void OrganizeListeningProcesses()
        {
            ListeningProcesses.Clear();
            foreach (var desktop in Current.Desktops)
            {
                foreach (var window in desktop.Windows)
                {
                    ListeningProcesses.Add(window.ProcessName, window);
                }
            }
        }

        public static void Load()
        {
            try
            {
                _isLoading = true;
                if (!File.Exists(App.ConfigPath))
                {
                    Current = new Config();
                    return;
                }
                Current = JsonConvert.DeserializeObject<Config>(File.ReadAllText(App.ConfigPath)) ?? new Config();
                OrganizeListeningProcesses();
            }
            finally
            {
                _isLoading = false;
            }

        }
        
        public void Save()
        {
            if (_isLoading)
            {
                return;
            }
            
            if (_saveTokenSource?.IsCancellationRequested == false)
            {
                _saveTokenSource.Cancel(false);
            }
            
            _saveTokenSource = new CancellationTokenSource();
            Task.Delay(3000, _saveTokenSource.Token).ContinueWith(task =>
            {
                File.WriteAllText(App.ConfigPath, 
                    JsonConvert.SerializeObject(this, App.JsonSettings));
            }, _saveTokenSource.Token);
        }
    }
}
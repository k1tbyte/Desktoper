using System.Reflection;
using Desktoper.Lib;
using Desktoper.MVVM.Core;
using Desktoper.Services;
using Microsoft.Win32;

namespace Desktoper.MVVM.ViewModel
{
    public sealed class SettingsViewModel : ObservableObject
    {
        private const string RegistryAutoStartup = "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run";
        public Config Config => Config.Current;
        
        public bool InAutoStartup
        {
            get =>
                !string.IsNullOrEmpty(RegistryUtils.GetUserRegistryValue(RegistryAutoStartup, App.Name) as string);

            set
            {
                if(value &&
                   RegistryUtils.SetUserRegistryValue(RegistryAutoStartup, App.Name, Assembly.GetExecutingAssembly().Location, RegistryValueKind.String))
                {
                    return;
                }

                RegistryUtils.DeleteRegistryValue(RegistryAutoStartup, App.Name);
            }
        }
    }
}
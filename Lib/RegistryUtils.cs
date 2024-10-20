namespace Desktoper.Lib
{
    public static class Registry
    {
        public static object GetUserRegistryValue(string path, string valueName)
        {
            try
            {
                using (var registryKey = Registry.CurrentUser.OpenSubKey(path, false))
                {
                    return registryKey.GetValue(valueName);
                }
            }
            catch (Exception e)
            {
                App.Logger.Value.LogHandledException(e);
            }

            return null;
        }

        public static bool SetUserRegistryValue(string path, string valueName, object value, RegistryValueKind kind)
        {
            try
            {
                using (var registryKey = Registry.CurrentUser.OpenSubKey(path, true))
                {
                    registryKey.SetValue(valueName,value, kind);
                }
                return true;
            }
            catch (Exception e)
            {
                App.Logger.Value.LogHandledException(e);
            }

            return false;
        }

        public static bool DeleteRegistryValue(string path, string valueName)
        {
            try
            {
                using (var registryKey = Registry.CurrentUser.OpenSubKey(path, true))
                {
                    registryKey.DeleteValue(valueName);
                }
                return true;
            }
            catch (Exception e)
            {
                App.Logger.Value.LogHandledException(e);
            }

            return false;
        }
    }
}
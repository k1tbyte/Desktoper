using Desktoper.MVVM.Core;

namespace Desktoper.Services
{
    public class ObservableConfig : ObservableObject
    {
        protected override bool SetProperty<T>(ref T field, T value, string propertyName = null)
        {
            var result = base.SetProperty(ref field, value, propertyName);
            if (result)
            {
                Config.Current?.Save();
            }
            return result;
        }
    }
}
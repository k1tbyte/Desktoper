using System;
using Desktoper.Services;
using Newtonsoft.Json;

namespace Desktoper.MVVM.Model
{
    public sealed class DesktopWindow : ObservableConfig
    {
        private bool? _isEnabled = true;
        private int? _height;
        private int? _width;
        private int? _y;
        private int? _x;
        public string ProcessName { get; set; }
        public int DesktopIndex { get; set; }

        public int? X
        {
            get => _x;
            set => SetProperty(ref _x, value, nameof(X));
        }

        public int? Y
        {
            get => _y;
            set => SetProperty(ref _y, value, nameof(Y));
        }

        public int? Width
        {
            get => _width;
            set => SetProperty(ref _width, value, nameof(Width));
        }

        public int? Height
        {
            get => _height;
            set => SetProperty(ref _height, value, nameof(Height));
        }

        public bool? IsEnabled
        {
            get => _isEnabled;
            set => SetProperty(ref _isEnabled, value, nameof(IsEnabled));
        }

        [JsonIgnore]
        public bool IsAligned { get; set; }
    }
}
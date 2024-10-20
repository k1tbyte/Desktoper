using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Desktoper.Services;

namespace Desktoper.MVVM.Model
{
    public sealed class Desktop
    {
        public string Name { get; set; }
        public ObservableCollection<DesktopWindow> Windows { get; set; } = new ObservableCollection<DesktopWindow>();
    }
}
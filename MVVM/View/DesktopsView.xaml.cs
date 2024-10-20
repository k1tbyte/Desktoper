using System.Collections.Generic;
using System.Windows.Controls;

namespace Desktoper.MVVM.View
{
    public partial class DesktopListView : Grid
    {
        public List<string> Desktops { get; set; } = new List<string>{ "1", "2", "3", "4", "5", "6", "7", "8" };
        public DesktopListView()
        {
            InitializeComponent();
            this.DataContext = this;
        }
    }
}
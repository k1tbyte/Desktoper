﻿using System.Windows;

namespace Desktoper.MVVM.Converters
{
    internal sealed class ToVisibilityConverter : BaseBooleanConverter<Visibility>
    {
        public ToVisibilityConverter() :
            base(Visibility.Visible, Visibility.Collapsed)
        { }
    }

    internal sealed class ToVisibilityInvertedConverter : BaseBooleanConverter<Visibility>
    {
        public ToVisibilityInvertedConverter() :
            base(Visibility.Collapsed, Visibility.Visible)
        { }
    }
}
using System;
using System.Globalization;
using Avalonia;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Markup.Xaml;

namespace VirtualizationImages;

public class DoubleToMatrixConverter : MarkupExtension, IValueConverter
{
    private static IValueConverter _converter;
    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        return _converter ??= new DoubleToMatrixConverter();
    }

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        Matrix bitmap;
        if (value == null)
            return BindingOperations.DoNothing;
        if (value is double zoom &&
            zoom > 0)
        {
            bitmap = new Matrix(zoom, 0, 0, zoom, 0, 0);
            return bitmap;
        }
        return BindingOperations.DoNothing;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return BindingOperations.DoNothing;
    }
}

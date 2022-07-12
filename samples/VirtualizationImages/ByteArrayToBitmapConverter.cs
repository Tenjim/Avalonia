using System;
using System.Globalization;
using System.IO;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Markup.Xaml;
using Avalonia.Media.Imaging;

namespace VirtualizationImages
{
    public class ByteArrayToBitmapConverter : MarkupExtension, IValueConverter
    {
        private static IValueConverter _converter;
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return _converter ??= new ByteArrayToBitmapConverter();
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Bitmap bitmap=null;
            if (value == null)
                return BindingOperations.DoNothing;
            if (value is byte[] array &&
                array.Length > 0)
            {
                using MemoryStream ms = new MemoryStream(array);
                bitmap = new Bitmap(ms);
            }

            return bitmap;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return BindingOperations.DoNothing;
        }
    }
}

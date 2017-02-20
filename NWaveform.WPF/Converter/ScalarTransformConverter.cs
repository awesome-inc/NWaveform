using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace NWaveform.Converter
{
    public class ScalarTransformConverter : IMultiValueConverter, IValueConverter
    {
        public Transform Transform { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return DoTransform((double)value, parameter);
        }

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values == null) throw new ArgumentNullException(nameof(values));
            if (values.Length < 1) throw new ArgumentException("values requires at least one element", nameof(values));

            return DoTransform((double)values[0], parameter);
        }

        private object DoTransform(double scalar, object parameter)
        {
            if (Transform == null) return null;
            var isY = (parameter != null) && System.Convert.ToBoolean(parameter, CultureInfo.CurrentCulture);
            var point = new Point(isY ? 0 : scalar, isY ? scalar : 0);
            var transformed = Transform.Transform(point);
            return isY ? transformed.Y : transformed.X;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
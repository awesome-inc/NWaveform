using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows.Media;

namespace NWaveform.Converter
{
    /* simple trick to make converter even more accessible by deriving from markup extension
     * cf.: http://drwpf.com/blog/2009/03/17/tips-and-tricks-making-value-converters-more-accessible-in-markup/
     */
    // ReSharper disable once UnusedMember.Global
    public class InverseTransformConverter : MarkupExtension, IValueConverter
    {
        private static readonly InverseTransformConverter Converter = new InverseTransformConverter();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ((Transform)value).Inverse;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return Converter;
        }
    }
}
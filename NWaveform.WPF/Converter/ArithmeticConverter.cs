using System;
using System.Globalization;
using System.Linq;
using System.Windows.Data;

namespace NWaveform.Converter
{
    public class ArithmeticConverter : IValueConverter, IMultiValueConverter
    {
        private ArithmeticOperation _operation;
        private Func<double, double, double> _func;

        public ArithmeticConverter()
        {
            Operation = ArithmeticOperation.Addition;
            OperandIsLeft = false;
        }

        public ArithmeticOperation Operation
        {
            get => _operation;
            set
            {
                _operation = value;
                _func = ToFunc(_operation);
            }
        }

        public double Operand { get; set; }
        public bool OperandIsLeft { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return OperandIsLeft 
                ? _func(Operand, (double)value)
                : _func((double)value, Operand);
        }

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            var d = values.OfType<double>().ToArray();
            if (d.Any()) return d.Aggregate(_func);
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            return null;
        }

        public enum ArithmeticOperation
        {
            Addition,
            Subtraction,
            Multiplication,
            Division,
            Power,
            Log,
            Max,
            Min
        }

        private static Func<double, double, double> ToFunc(ArithmeticOperation operation)
        {
            switch (operation)
            {
                case ArithmeticOperation.Addition: return (a, b) => a + b;
                case ArithmeticOperation.Subtraction: return (a, b) => a - b;
                case ArithmeticOperation.Multiplication: return (a, b) => a * b;
                case ArithmeticOperation.Division: return (a, b) => a / b;
                case ArithmeticOperation.Power: return Math.Pow;
                case ArithmeticOperation.Log: return Math.Log;
                case ArithmeticOperation.Max: return Math.Max;
                case ArithmeticOperation.Min: return Math.Min;
                    // ReSharper disable once LocalizableElement
                default: throw new ArgumentException("Unsupported arithmetic operation: " + operation, nameof(operation));
            }
        }
    }
}

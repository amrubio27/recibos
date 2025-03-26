using System.Globalization;

namespace recibos.core.converters {
    public class BooleanConverter : IValueConverter {
        public bool Invert { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (value is bool boolValue) {
                return Invert ? !boolValue : boolValue;
            }

            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            if (value is bool boolValue) {
                return Invert ? !boolValue : boolValue;
            }

            return false;
        }
    }
}
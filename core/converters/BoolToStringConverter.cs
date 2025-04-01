using System.Globalization;

namespace recibos.core.converters;

public class BoolToStringConverter : IValueConverter {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
        if (value is not bool boolValue || parameter is not string paramString)
            return string.Empty;

        string[] values = paramString.Split(';');
        if (values.Length != 2)
            return boolValue.ToString();

        return boolValue ? values[1] : values[0];
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
        throw new NotImplementedException();
    }
}
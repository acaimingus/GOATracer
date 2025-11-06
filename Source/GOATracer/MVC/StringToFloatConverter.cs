using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace GOATracer.MVC;

public class StringToFloatConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is float floatValue)
        {
            return floatValue.ToString(CultureInfo.InvariantCulture);
        }

        return "0.0";
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        // Check if it is a string
        if (value is string stringValue)
        {
            // Replace , with .
            var cleanedValue = stringValue.Replace(',', '.');

            // Empty string
            if (string.IsNullOrWhiteSpace(cleanedValue))
            {
                return 0.0f;
            }

            // Actual value
            if (float.TryParse(cleanedValue, NumberStyles.Float, CultureInfo.InvariantCulture, out var floatValue))
            {
                return floatValue;
            }
        }
        // Invalid, make 0.0f
        return 0.0f;
    }
}
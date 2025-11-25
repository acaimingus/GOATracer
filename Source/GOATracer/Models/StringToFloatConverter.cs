using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace GOATracer.Models;

/// <summary>
/// Custom converter for adapting strings to floats; Used to avoid errors showing up in the UI textboxes
/// </summary>
public class StringToFloatConverter : IValueConverter
{
    /// <summary>
    /// Converts a float value to its string representation.
    /// </summary>
    /// <param name="value">The float value to convert.</param>
    /// <param name="targetType">The type of the binding target property.</param>
    /// <param name="parameter">The converter parameter to use.</param>
    /// <param name="culture">The culture to use in the converter.</param>
    /// <returns>A string representation of the float value.</returns>
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        // Get the value of the float if available
        if (value is float floatValue)
        {
            return floatValue.ToString(CultureInfo.InvariantCulture);
        }

        // Else return 0.0f as a fallback
        return "0.0";
    }

    /// <summary>
    /// Converts a string representation of a number to a float.
    /// </summary>
    /// <param name="value">The string value to convert.</param>
    /// <param name="targetType">The type of the binding target property.</param>
    /// <param name="parameter">The converter parameter to use.</param>
    /// <param name="culture">The culture to use in the converter.</param>
    /// <returns>A float value, or 0.0f if conversion fails.</returns>
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
using Avalonia.Data.Converters;
using Avalonia.Media;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace ElGamalGUI.Converters;

public class StatusColorConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var text = value as string;
        if (string.IsNullOrEmpty(text)) return Brushes.Transparent;

        string lowerText = text.ToLower();

        if (lowerText.Contains("valid") && !lowerText.Contains("invalid"))
            return Brushes.Green;

        if (lowerText.Contains("invalid") ||
            lowerText.Contains("error") ||
            lowerText.Contains("fail") ||
            lowerText.Contains("compromised") ||
            lowerText.Contains("missing"))
            return Brushes.Tomato;

        return Brushes.White;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

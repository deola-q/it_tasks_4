using System;
using System.Globalization;
using System.Linq;
using Avalonia.Data.Converters;
using System.Reflection;

namespace Geometric.Reflection.Converters
{
    public class StringFormatConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is null) return null;
        
        if (value is ParameterInfo[] parameters)
        {
            return string.Join(", ", parameters.Select(p => $"{p.ParameterType.Name} {p.Name}"));
        }
        
        // Добавляем обработку MethodInfo
        if (value is MethodInfo method)
        {
            var methodParams = method.GetParameters();
            return string.Join(", ", methodParams.Select(p => $"{p.ParameterType.Name} {p.Name}"));
        }

        return string.Format(parameter?.ToString() ?? "{0}", value);
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
}
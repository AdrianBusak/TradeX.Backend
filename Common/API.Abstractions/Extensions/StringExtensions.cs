using Newtonsoft.Json;
using System.Globalization;

namespace API.Abstractions.Extensions
{
    public static class StringExtensions
    {
        public static T? GetQueryParameter<T>(this string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return default;

            if (typeof(T) == typeof(string))
                return (T)(object)value;

            if (typeof(T) == typeof(int) && int.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var intVal))
                return (T)(object)intVal;

            if (typeof(T) == typeof(double) && double.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var doubleVal))
                return (T)(object)doubleVal;

            return JsonConvert.DeserializeObject<T>(value, new JsonSerializerSettings());
        }
    }
}

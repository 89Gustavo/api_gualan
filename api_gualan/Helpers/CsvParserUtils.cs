using System;
using System.Globalization;

namespace api_gualan.Helpers
{
    public static class CsvParserUtils
    {
        public static string? Clean(string value)
        {
            return string.IsNullOrWhiteSpace(value)
                ? null
                : value.Trim().Replace("\"", "");
        }

        public static decimal? ParseDecimal(string value)
        {
            value = Clean(value);

            if (decimal.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal result))
                return result;

            return null;
        }

        public static int? ParseInt(string value)
        {
            value = Clean(value);

            if (int.TryParse(value, out int result))
                return result;

            return null;
        }

        public static DateTime? ParseDate(string value)
        {
            value = Clean(value);

            if (DateTime.TryParse(value, out DateTime date))
                return date;

            return null;
        }
    }
}

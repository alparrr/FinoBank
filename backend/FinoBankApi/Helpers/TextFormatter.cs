using System.Globalization;

namespace FinoBankApi.Helpers
{
    public static class TextFormatter
    {
        public static string Capitalize(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return input;
            
            return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(input.Trim().ToLower());
        }

        public static string FormatAddress(string input)
        {
             if (string.IsNullOrWhiteSpace(input)) return input;
             return input.Trim();
        }
    }
}
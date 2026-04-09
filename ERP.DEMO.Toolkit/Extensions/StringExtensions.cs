
namespace ERP.DEMO.Toolkit.Extensions
{
    public static class StringExtensions
    {
        public static bool IsJsonNullOrEmpty(this string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return true;
            else if (value == "[]")
                return true;
            else
                return false;
        }

        public static string RemoveDiacritics(this string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return value;

            var tempBytes = System.Text.Encoding.GetEncoding("ISO-8859-15").GetBytes(value);
            string asciiStr = System.Text.Encoding.UTF8.GetString(tempBytes);
            var s = System.Text.RegularExpressions.Regex.Replace(asciiStr, "[^a-zA-Z0-9]", "");
            return s;
        }

        public static string SubStringTo(this string thatString, int limit)
        {
            if (!string.IsNullOrEmpty(thatString))
            {
                if (thatString.Length > limit)
                {
                    return thatString.Substring(0, limit) + "...";
                }
                return thatString;
            }
            return string.Empty;
        }
    }
}

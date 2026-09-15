using System.Globalization;
using System.Text.RegularExpressions;

namespace MAUIBUDGET.Models
{
    public static class AmountParser
    {
        const string CurrencyPattern = @"(?:(?<![A-Za-z])[A-Z]{3}(?![A-Za-z])|Kč|€|\$|£)";

        static readonly Regex NumberRegex = new(@"(?<!\d)(?<int>\d{1,3}(?:[   .,]\d{3})+|\d+)(?:[.,](?<dec>\d{1,2}))?(?!\d)");
        static readonly Regex CurrencyBeforeRegex = new(CurrencyPattern + @"\s*$");
        static readonly Regex CurrencyAfterRegex = new(@"^\s*" + CurrencyPattern);
        static readonly Regex NonDigitRegex = new(@"\D");

        public static double? Parse(string? text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return null;

            Match? amount = NumberRegex.Matches(text).FirstOrDefault(m => HasAdjacentCurrency(text, m));
            if (amount == null)
                return null;

            string integerPart = NonDigitRegex.Replace(amount.Groups["int"].Value, "");
            string decimalPart = amount.Groups["dec"].Success ? amount.Groups["dec"].Value : "0";

            return double.Parse($"{integerPart}.{decimalPart}", CultureInfo.InvariantCulture);
        }

        static bool HasAdjacentCurrency(string text, Match match)
        {
            string before = text[..match.Index];
            string after = text[(match.Index + match.Length)..];
            return CurrencyBeforeRegex.IsMatch(before) || CurrencyAfterRegex.IsMatch(after);
        }
    }
}

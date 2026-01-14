using System.Text.RegularExpressions;

namespace CycleManager.Tests.Integration.Helpers
{
    public static class TokenHelper
    {
        public static string ExtractAntiForgeryToken(string html)
        {
            if (string.IsNullOrWhiteSpace(html))
                throw new ArgumentException("HTML cannot be null or empty", nameof(html));

            var match = Regex.Match(html,
                @"<input name=""__RequestVerificationToken"" type=""hidden"" value=""([^""]+)"" />");

            if (!match.Success)
                throw new InvalidOperationException("Anti-forgery token not found in HTML.");

            return match.Groups[1].Value;
        }
    }
}

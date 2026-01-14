namespace CycleManager.Services.Helpers
{
    public static class SplitNamesHelper
    {
        private static readonly HashSet<string> LastnamePrefixes = new HashSet<string>(
            StringComparer.OrdinalIgnoreCase)
        {
            "van", "der", "de", "den", "del", "della",
             "la", "le", "di", "da", "dos", "das", "von"
        };

        public static (string FirstName, string LastName) SplitName(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return ("Unknown", "Unknown");

            var parts = input.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries).ToList();
            if (parts.Count == 0) return ("", "");
            if (parts.Count == 1) return (parts[0], "");

            List<string> lastNameTokens = new List<string>();
            int i = 0;

            // Start met eerste token in achternaam
            lastNameTokens.Add(parts[i]);
            i++;

            // Voeg tokens toe als ze een prefix zijn of als ze na een prefix staan
            while (i < parts.Count - 1)
            {
                lastNameTokens.Add(parts[i]);
                i++;
            }

            string lastName = string.Join(" ", lastNameTokens);

            // Alles na de achternaam → voornaam
            string firstName = string.Join(" ", parts.Skip(i));

            return (firstName, lastName);
        }

        public static string FormatPart(string part)
        {
            if (string.IsNullOrWhiteSpace(part))
                return "";

            part = part.ToLower();

            if (LastnamePrefixes.Contains(part))
                return part;

            // Normale capitalisatie
            return char.ToUpper(part[0]) + part.Substring(1);
        }

        public static string FormatName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return "";

            var parts = name.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            return string.Join(" ",
                parts.Select(p => FormatPart(p))
            );
        }
    }
}

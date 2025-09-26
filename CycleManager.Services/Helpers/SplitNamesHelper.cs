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
            var parts = input.Split(' ', StringSplitOptions.RemoveEmptyEntries).ToList();
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
    }
}

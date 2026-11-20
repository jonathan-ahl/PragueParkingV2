using System.Text.Encodings.Web;
using System.Text.Json;

namespace PragueParking.Data
{
    // Klassbiblioteket för dataåtkomst. All läsning och skrivning av filer sker här,
    // så att resten av programmet inte behöver veta något om filer.
    //
    // Metoderna är generiska (<T>): samma metod kan spara en lista med P-platser,
    // ett konfigurationsobjekt eller vad som helst annat.
    public static class MinaFiler
    {
        private static readonly JsonSerializerOptions Installningar = new JsonSerializerOptions
        {
            WriteIndented = true,                                  // snygg, läsbar JSON med radbrytningar
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping  // skriv Å, Ä och Ö som vanliga tecken
        };

        public static bool FilFinns(string sokvag)
        {
            return File.Exists(sokvag);
        }

        // Läser en JSON-fil och gör om den till ett objekt av typen T.
        // Returnerar null om filen inte finns.
        public static T? LasJson<T>(string sokvag)
        {
            if (!File.Exists(sokvag))
            {
                return default;
            }

            string json = File.ReadAllText(sokvag);
            return JsonSerializer.Deserialize<T>(json, Installningar);
        }

        // Gör om ett objekt till JSON och skriver det till fil (skriver över filen).
        public static void SparaJson<T>(string sokvag, T data)
        {
            string json = JsonSerializer.Serialize(data, Installningar);
            File.WriteAllText(sokvag, json);
        }

        // Läser en vanlig textfil rad för rad (används för prislistan).
        public static string[] LasRader(string sokvag)
        {
            if (!File.Exists(sokvag))
            {
                return new string[0];
            }

            return File.ReadAllLines(sokvag);
        }
    }
}

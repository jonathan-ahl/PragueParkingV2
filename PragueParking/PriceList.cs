namespace PragueParking
{
    // Prislistan, inläst från prislista.txt.
    public class PriceList
    {
        // Pris per påbörjad timme för varje fordonstyp, t.ex. "Bil" -> 20
        public Dictionary<string, int> PrisPerTimme { get; } = new Dictionary<string, int>();
        public int GratisMinuter { get; set; } = 10;

        // Gör om raderna i textfilen till en prislista.
        // Format per rad:  Namn = värde   # kommentar
        public static PriceList FranRader(string[] rader)
        {
            PriceList lista = new PriceList();

            foreach (string rad in rader)
            {
                string text = rad.Split('#')[0].Trim(); // allt efter # är kommentar
                if (text == "")
                {
                    continue; // tom rad eller bara kommentar
                }

                string[] delar = text.Split('=');
                if (delar.Length != 2)
                {
                    continue; // felaktig rad hoppas över
                }

                string namn = delar[0].Trim();
                if (!int.TryParse(delar[1].Trim(), out int varde) || varde < 0)
                {
                    continue;
                }

                if (namn == "GratisMinuter")
                {
                    lista.GratisMinuter = varde;
                }
                else
                {
                    lista.PrisPerTimme[namn] = varde;
                }
            }

            return lista;
        }

        public int BeraknaPris(string typnamn, TimeSpan parkeradTid)
        {
            if (parkeradTid.TotalMinutes <= GratisMinuter)
            {
                return 0;
            }

            // Math.Ceiling avrundar uppåt: 61 minuter = 2 påbörjade timmar
            int paborjadeTimmar = (int)Math.Ceiling(parkeradTid.TotalHours);

            int timpris = 0;
            if (PrisPerTimme.ContainsKey(typnamn))
            {
                timpris = PrisPerTimme[typnamn];
            }

            return paborjadeTimmar * timpris;
        }
    }
}

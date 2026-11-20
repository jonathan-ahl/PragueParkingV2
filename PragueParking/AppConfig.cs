namespace PragueParking
{
    // Innehållet i config.json. Egenskaperna har samma namn som i filen.
    public class AppConfig
    {
        public int AntalPlatser { get; set; } = 100;
        public int PlatsStorlek { get; set; } = 4;
        public List<VehicleType> Fordonstyper { get; set; } = new List<VehicleType>();

        // Standardinställningar, om config.json saknas.
        public static AppConfig Standard()
        {
            AppConfig config = new AppConfig();
            config.Fordonstyper.Add(new VehicleType { Namn = "Bil", AntalPerPlats = 1 });
            config.Fordonstyper.Add(new VehicleType { Namn = "MC", AntalPerPlats = 2 });
            return config;
        }

        // Räknar ut hur stor en fordonstyp är.
        // Exempel: platsstorlek 4 och 2 MC per plats -> en MC har storlek 2.
        public int StorlekFor(string typnamn)
        {
            foreach (VehicleType typ in Fordonstyper)
            {
                if (typ.Namn == typnamn && typ.AntalPerPlats > 0)
                {
                    return PlatsStorlek / typ.AntalPerPlats;
                }
            }
            return PlatsStorlek; // okänd typ tar en hel plats
        }
    }

    // En fordonstyp i config.json, t.ex. { "Namn": "MC", "AntalPerPlats": 2 }
    public class VehicleType
    {
        public string Namn { get; set; } = "";
        public int AntalPerPlats { get; set; } = 1;
    }
}

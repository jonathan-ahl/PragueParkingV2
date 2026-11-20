using System.Text.Json.Serialization;

namespace PragueParking
{
    // Basklassen för alla fordon.
    // abstract betyder att man inte kan skapa ett "bara Vehicle" – bara en Car eller en MC.
    //
    // Attributen ovanför klassen behövs för JSON: när en lista med fordon sparas skrivs
    // "Typ": "Bil" eller "Typ": "MC" in i filen, så att rätt subklass skapas igen vid inläsning.
    [JsonPolymorphic(TypeDiscriminatorPropertyName = "Typ")]
    [JsonDerivedType(typeof(Car), "Bil")]
    [JsonDerivedType(typeof(MC), "MC")]
    public abstract class Vehicle
    {
        public string RegNr { get; set; } = "";

        // Hur mycket plats fordonet tar. En P-ruta har storlek 4: en bil tar 4, en MC tar 2.
        // Värdet räknas fram från konfigurationsfilen när fordonet skapas.
        public int Storlek { get; set; }

        // Tidpunkt när fordonet parkerades. Sätts automatiskt i konstruktorn.
        public DateTime Ankomst { get; set; }

        // Varje subklass talar själv om vad den heter ("Bil" eller "MC").
        // [JsonIgnore] = sparas inte i filen, eftersom det alltid går att räkna ut.
        [JsonIgnore]
        public abstract string Typnamn { get; }

        // Tom konstruktor – används när fordonet läses in från JSON-filen.
        protected Vehicle()
        {
        }

        // Används när ett nytt fordon parkeras.
        protected Vehicle(string regNr, int storlek)
        {
            RegNr = regNr;
            Storlek = storlek;
            Ankomst = DateTime.Now;
        }

        public TimeSpan ParkeradTid(DateTime nu)
        {
            return nu - Ankomst;
        }
    }
}

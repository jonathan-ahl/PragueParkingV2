namespace PragueParking
{
    // P-huset. Innehåller en lista med P-platser och all logik för att
    // parkera, söka, flytta och checka ut fordon.
    // Här finns inga Console-anrop – användargränssnittet ligger i Program.cs.
    public class ParkingGarage
    {
        public List<ParkingSpot> Platser { get; } = new List<ParkingSpot>();

        public ParkingGarage(int antalPlatser, int platsStorlek)
        {
            for (int nummer = 1; nummer <= antalPlatser; nummer++)
            {
                Platser.Add(new ParkingSpot(nummer, platsStorlek));
            }
        }

        // Parkerar fordonet på första lämpliga plats och returnerar platsen,
        // eller null om det inte finns plats.
        public ParkingSpot? Parkera(Vehicle fordon)
        {
            // 1. Försök först på en plats där det redan står något men där det finns plats kvar
            //    (t.ex. en MC bredvid en annan MC). Då sparas hela platser åt bilarna.
            foreach (ParkingSpot plats in Platser)
            {
                if (!plats.ArTom() && plats.Rymmer(fordon))
                {
                    plats.Fordon.Add(fordon);
                    return plats;
                }
            }

            // 2. Annars första tomma plats.
            foreach (ParkingSpot plats in Platser)
            {
                if (plats.Rymmer(fordon))
                {
                    plats.Fordon.Add(fordon);
                    return plats;
                }
            }

            return null;
        }

        // Söker efter ett fordon. Platsen och fordonet skickas tillbaka via out-parametrar.
        public bool HittaFordon(string regNr, out ParkingSpot? hittadPlats, out Vehicle? hittatFordon)
        {
            foreach (ParkingSpot plats in Platser)
            {
                foreach (Vehicle fordon in plats.Fordon)
                {
                    if (fordon.RegNr == regNr)
                    {
                        hittadPlats = plats;
                        hittatFordon = fordon;
                        return true;
                    }
                }
            }

            hittadPlats = null;
            hittatFordon = null;
            return false;
        }

        // Tar bort fordonet från P-huset och returnerar det (null om det inte fanns).
        public Vehicle? CheckaUt(string regNr)
        {
            if (HittaFordon(regNr, out ParkingSpot? plats, out Vehicle? fordon) && plats != null && fordon != null)
            {
                plats.Fordon.Remove(fordon);
                return fordon;
            }
            return null;
        }

        // Flyttar ett fordon. Returnerar true om det gick. meddelande innehåller
        // arbetsordern eller förklaringen till varför det inte gick.
        public bool Flytta(string regNr, int tillNummer, out string meddelande)
        {
            if (!HittaFordon(regNr, out ParkingSpot? franPlats, out Vehicle? fordon) || franPlats == null || fordon == null)
            {
                meddelande = $"Hittade inget fordon med registreringsnummer {regNr}.";
                return false;
            }

            ParkingSpot? tillPlats = HamtaPlats(tillNummer);
            if (tillPlats == null)
            {
                meddelande = $"Plats {tillNummer} finns inte.";
                return false;
            }

            if (tillPlats == franPlats)
            {
                meddelande = "Fordonet står redan på den platsen.";
                return false;
            }

            if (!tillPlats.Rymmer(fordon))
            {
                meddelande = $"Det finns inte plats för {fordon.Typnamn} {regNr} på plats {tillNummer}.";
                return false;
            }

            franPlats.Fordon.Remove(fordon);
            tillPlats.Fordon.Add(fordon);
            meddelande = $"Flytta {fordon.Typnamn} {regNr} från plats {franPlats.Nummer} till plats {tillPlats.Nummer}.";
            return true;
        }

        // Platsnummer 1–100 för personalen, index 0–99 i listan.
        public ParkingSpot? HamtaPlats(int nummer)
        {
            if (nummer < 1 || nummer > Platser.Count)
            {
                return null;
            }
            return Platser[nummer - 1];
        }

        // De platser där det står fordon. Det är dessa som sparas i datafilen.
        public List<ParkingSpot> UpptagnaPlatser()
        {
            List<ParkingSpot> upptagna = new List<ParkingSpot>();
            foreach (ParkingSpot plats in Platser)
            {
                if (!plats.ArTom())
                {
                    upptagna.Add(plats);
                }
            }
            return upptagna;
        }

        // Lägger tillbaka fordon som har lästs in från datafilen, på samma plats som förut.
        // Om platsen inte längre finns (t.ex. om antalet platser har minskats i
        // konfigurationsfilen) parkeras fordonet på första lediga plats i stället.
        public void LaggTillSparadeFordon(List<ParkingSpot> sparadePlatser)
        {
            foreach (ParkingSpot sparad in sparadePlatser)
            {
                ParkingSpot? plats = HamtaPlats(sparad.Nummer);
                foreach (Vehicle fordon in sparad.Fordon)
                {
                    if (plats != null && plats.Rymmer(fordon))
                    {
                        plats.Fordon.Add(fordon);
                    }
                    else
                    {
                        Parkera(fordon);
                    }
                }
            }
        }
    }
}

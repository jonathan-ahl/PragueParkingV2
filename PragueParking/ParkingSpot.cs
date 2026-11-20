namespace PragueParking
{
    // En P-ruta. Den har ett nummer, en storlek och en lista med fordonen som står där.
    // I stället för strängar som "MC#ABC|MC#DEF" (version 1) har vi nu riktiga objekt.
    public class ParkingSpot
    {
        public int Nummer { get; set; }
        public int Storlek { get; set; }
        public List<Vehicle> Fordon { get; set; } = new List<Vehicle>();

        // Tom konstruktor – används när P-platsen läses in från JSON-filen.
        public ParkingSpot()
        {
        }

        public ParkingSpot(int nummer, int storlek)
        {
            Nummer = nummer;
            Storlek = storlek;
        }

        public int UpptagenYta()
        {
            int summa = 0;
            foreach (Vehicle fordon in Fordon)
            {
                summa += fordon.Storlek;
            }
            return summa;
        }

        public int LedigYta()
        {
            return Storlek - UpptagenYta();
        }

        public bool Rymmer(Vehicle fordon)
        {
            return fordon.Storlek <= LedigYta();
        }

        public bool ArTom()
        {
            return Fordon.Count == 0;
        }

        public bool ArFull()
        {
            return LedigYta() <= 0;
        }
    }
}

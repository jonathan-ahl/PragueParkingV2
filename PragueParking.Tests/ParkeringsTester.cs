using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace PragueParking.Tests
{
    // Enhetstester med MSTest. Kör dem via Test -> Run All Tests i Visual Studio.
    // Varje test följer mönstret: förbered (Arrange), gör (Act), kontrollera (Assert).
    [TestClass]
    public class ParkeringsTester
    {
        [TestMethod]
        public void TvaMcDelarSammaPlats()
        {
            // Arrange
            ParkingGarage garage = new ParkingGarage(10, 4);

            // Act
            garage.Parkera(new MC("MC111", 2));
            ParkingSpot? plats = garage.Parkera(new MC("MC222", 2));

            // Assert
            Assert.IsNotNull(plats);
            Assert.AreEqual(1, plats.Nummer);
            Assert.AreEqual(2, plats.Fordon.Count);
        }

        [TestMethod]
        public void BilParkerasInteBredvidMc()
        {
            ParkingGarage garage = new ParkingGarage(10, 4);
            garage.Parkera(new MC("MC111", 2));

            ParkingSpot? plats = garage.Parkera(new Car("BIL111", 4));

            Assert.IsNotNull(plats);
            Assert.AreEqual(2, plats.Nummer);
        }

        [TestMethod]
        public void DeForstaTioMinuternaArGratis()
        {
            PriceList prislista = PriceList.FranRader(new string[] { "Bil = 20", "GratisMinuter = 10" });

            int pris = prislista.BeraknaPris("Bil", TimeSpan.FromMinutes(9));

            Assert.AreEqual(0, pris);
        }

        [TestMethod]
        public void BilBetalarPerPaborjadTimme()
        {
            PriceList prislista = PriceList.FranRader(new string[] { "Bil = 20", "GratisMinuter = 10" });

            int pris = prislista.BeraknaPris("Bil", TimeSpan.FromMinutes(61)); // 2 påbörjade timmar

            Assert.AreEqual(40, pris);
        }

        [TestMethod]
        public void KommentarerIPrislistanIgnoreras()
        {
            string[] rader = { "# Detta är en kommentar", "MC = 10   # per timme", "" };

            PriceList prislista = PriceList.FranRader(rader);

            Assert.AreEqual(10, prislista.PrisPerTimme["MC"]);
        }

        [TestMethod]
        public void FordonSparasOchLasesInMedRattTyp()
        {
            // Arrange – ett P-hus med en bil och en MC, sparat till en tillfällig fil
            ParkingGarage garage = new ParkingGarage(10, 4);
            garage.Parkera(new Car("BIL111", 4));
            garage.Parkera(new MC("MC111", 2));
            string fil = Path.Combine(Path.GetTempPath(), $"pp-test-{Guid.NewGuid()}.json");

            try
            {
                // Act – spara och läs in igen, som när programmet startas om
                PragueParking.Data.MinaFiler.SparaJson(fil, garage.UpptagnaPlatser());
                List<ParkingSpot>? inlast = PragueParking.Data.MinaFiler.LasJson<List<ParkingSpot>>(fil);
                ParkingGarage nyttGarage = new ParkingGarage(10, 4);
                nyttGarage.LaggTillSparadeFordon(inlast!);

                // Assert – fordonen är tillbaka på samma platser och är fortfarande Car respektive MC
                Assert.IsTrue(nyttGarage.HittaFordon("BIL111", out ParkingSpot? bilPlats, out Vehicle? bil));
                Assert.IsInstanceOfType(bil, typeof(Car));
                Assert.AreEqual(1, bilPlats!.Nummer);

                Assert.IsTrue(nyttGarage.HittaFordon("MC111", out ParkingSpot? mcPlats, out Vehicle? mc));
                Assert.IsInstanceOfType(mc, typeof(MC));
                Assert.AreEqual(2, mcPlats!.Nummer);
            }
            finally
            {
                File.Delete(fil);
            }
        }

        [TestMethod]
        public void BilKanInteFlyttasTillPlatsMedMc()
        {
            ParkingGarage garage = new ParkingGarage(10, 4);
            garage.Parkera(new MC("MC111", 2));      // plats 1
            garage.Parkera(new Car("BIL111", 4));    // plats 2

            bool gick = garage.Flytta("BIL111", 1, out string meddelande);

            Assert.IsFalse(gick);
            Assert.IsTrue(garage.HittaFordon("BIL111", out ParkingSpot? plats, out _));
            Assert.AreEqual(2, plats!.Nummer); // bilen står kvar
        }
    }
}

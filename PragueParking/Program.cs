using System.Text;
using PragueParking.Data;
using Spectre.Console;

namespace PragueParking
{
    // Användargränssnittet. Här finns menyn och all kommunikation med personalen.
    // Logiken ligger i klasserna ParkingGarage, ParkingSpot, PriceList osv.
    internal class Program
    {
        // Filerna ligger i samma mapp som programmet (bin/Debug/net10.0)
        static readonly string ConfigFil = Path.Combine(AppContext.BaseDirectory, "config.json");
        static readonly string PrisFil = Path.Combine(AppContext.BaseDirectory, "prislista.txt");
        static readonly string DataFil = Path.Combine(AppContext.BaseDirectory, "parkering.json");

        const string MenyParkera = "Parkera fordon";
        const string MenyHamtaUt = "Hämta ut fordon";
        const string MenyFlytta = "Flytta fordon";
        const string MenySok = "Sök fordon";
        const string MenyLista = "Lista alla fordon";
        const string MenyPrislista = "Läs in prislistan på nytt";
        const string MenyAvsluta = "Avsluta";

        // Visas i varje meny så att man förstår hur man väljer utan att läsa README.
        const string MenyHjalp = "[grey](välj med ↑ ↓ och tryck Enter)[/]";

        static AppConfig config = AppConfig.Standard();
        static PriceList prislista = new PriceList();
        static ParkingGarage garage = new ParkingGarage(0, 4);

        static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;

            // Vid uppstart: inställningar, prislista och parkerade fordon
            LasInConfig();
            LasInPrislista();
            LasInParkering();

            bool programmetKors = true;
            while (programmetKors)
            {
                AnsiConsole.Clear();
                AnsiConsole.Write(new Rule("[orange1]PRAGUE PARKING 2.0[/]"));
                VisaKarta();

                string val = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title($"Vad vill du göra? {MenyHjalp}")
                        .AddChoices(MenyParkera, MenyHamtaUt, MenyFlytta, MenySok, MenyLista, MenyPrislista, MenyAvsluta));

                switch (val)
                {
                    case MenyParkera: ParkeraFordon(); break;
                    case MenyHamtaUt: HamtaUtFordon(); break;
                    case MenyFlytta: FlyttaFordon(); break;
                    case MenySok: SokFordon(); break;
                    case MenyLista: ListaFordon(); break;
                    case MenyPrislista: LasInPrislista(); VisaPrislista(); break;
                    case MenyAvsluta: programmetKors = false; break;
                }

                if (programmetKors)
                {
                    AnsiConsole.MarkupLine("");
                    AnsiConsole.MarkupLine("[grey]Tryck på valfri tangent för att återgå till menyn...[/]");
                    Console.ReadKey(true);
                }
            }
        }

        // ---------------------------------------------------------------
        // MENYVAL
        // ---------------------------------------------------------------

        static void ParkeraFordon()
        {
            // Fordonstyperna hämtas från config.json (de som programmet har en klass för).
            List<string> typer = TillgangligaFordonstyper();
            if (typer.Count == 0)
            {
                AnsiConsole.MarkupLine("[red]Inga kända fordonstyper (Bil/MC) finns i config.json.[/]");
                return;
            }

            string typ = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title($"Vilken fordonstyp? {MenyHjalp}")
                    .AddChoices(typer));

            string regNr = FragaRegNr();

            if (garage.HittaFordon(regNr, out ParkingSpot? befintligPlats, out Vehicle? befintligtFordon) && befintligPlats != null)
            {
                AnsiConsole.MarkupLine($"[red]{regNr} står redan på plats {befintligPlats.Nummer}.[/]");
                return;
            }

            Vehicle fordon = SkapaFordon(typ, regNr);
            ParkingSpot? plats = garage.Parkera(fordon);

            if (plats == null)
            {
                AnsiConsole.MarkupLine("[red]Tyvärr, det finns ingen ledig plats för det fordonet.[/]");
                return;
            }

            SparaParkering();
            AnsiConsole.MarkupLine($"[green]Kör {fordon.Typnamn} {regNr} till plats {plats.Nummer}.[/]");
        }

        static void HamtaUtFordon()
        {
            string regNr = FragaRegNr();

            if (!garage.HittaFordon(regNr, out ParkingSpot? plats, out Vehicle? fordon) || plats == null || fordon == null)
            {
                AnsiConsole.MarkupLine($"[red]Hittade inget fordon med registreringsnummer {regNr}.[/]");
                return;
            }

            garage.CheckaUt(regNr);
            SparaParkering();

            TimeSpan tid = fordon.ParkeradTid(DateTime.Now);
            int pris = prislista.BeraknaPris(fordon.Typnamn, tid);

            Table kvitto = new Table();
            kvitto.AddColumn("Fordon");
            kvitto.AddColumn("Plats");
            kvitto.AddColumn("Ankomst");
            kvitto.AddColumn("Parkerad tid");
            kvitto.AddColumn("Att betala");
            kvitto.AddRow($"{fordon.Typnamn} {regNr}", plats.Nummer.ToString(), fordon.Ankomst.ToString("yyyy-MM-dd HH:mm"), FormateraTid(tid), $"{pris} CZK");
            AnsiConsole.Write(kvitto);
        }

        static void FlyttaFordon()
        {
            string regNr = FragaRegNr();

            // Kontrollera först att fordonet finns, så att man inte behöver ange en plats i onödan.
            if (!garage.HittaFordon(regNr, out ParkingSpot? franPlats, out Vehicle? fordon) || franPlats == null || fordon == null)
            {
                AnsiConsole.MarkupLine($"[red]Hittade inget fordon med registreringsnummer {regNr}.[/]");
                return;
            }

            AnsiConsole.MarkupLine($"{fordon.Typnamn} {regNr} står på plats [green]{franPlats.Nummer}[/].");
            int tillNummer = FragaPlatsnummer("Till vilken plats?");

            if (garage.Flytta(regNr, tillNummer, out string meddelande))
            {
                SparaParkering();
                AnsiConsole.MarkupLine($"[green]{meddelande}[/]");
            }
            else
            {
                AnsiConsole.MarkupLine($"[red]{meddelande}[/]");
            }
        }

        static void SokFordon()
        {
            string regNr = FragaRegNr();

            if (garage.HittaFordon(regNr, out ParkingSpot? plats, out Vehicle? fordon) && plats != null && fordon != null)
            {
                AnsiConsole.MarkupLine($"{fordon.Typnamn} {regNr} står på plats [green]{plats.Nummer}[/] sedan {fordon.Ankomst:yyyy-MM-dd HH:mm}.");
            }
            else
            {
                AnsiConsole.MarkupLine($"[red]Hittade inget fordon med registreringsnummer {regNr}.[/]");
            }
        }

        static void ListaFordon()
        {
            Table tabell = new Table();
            tabell.AddColumn("Plats");
            tabell.AddColumn("Typ");
            tabell.AddColumn("Reg.nr");
            tabell.AddColumn("Ankomst");

            foreach (ParkingSpot plats in garage.UpptagnaPlatser())
            {
                foreach (Vehicle fordon in plats.Fordon)
                {
                    tabell.AddRow(plats.Nummer.ToString(), fordon.Typnamn, fordon.RegNr, fordon.Ankomst.ToString("yyyy-MM-dd HH:mm"));
                }
            }

            AnsiConsole.Write(tabell);
        }

        static void VisaPrislista()
        {
            Table tabell = new Table();
            tabell.AddColumn("Fordonstyp");
            tabell.AddColumn("Pris per påbörjad timme");

            foreach (KeyValuePair<string, int> rad in prislista.PrisPerTimme)
            {
                tabell.AddRow(rad.Key, $"{rad.Value} CZK");
            }

            AnsiConsole.MarkupLine("[green]Prislistan är inläst.[/]");
            AnsiConsole.Write(tabell);
            AnsiConsole.MarkupLine($"De första {prislista.GratisMinuter} minuterna är gratis.");
        }

        // ---------------------------------------------------------------
        // KARTA ÖVER P-HUSET
        // Grön = ledig, gul = delvis full, röd = full. Tio platser per rad.
        // ---------------------------------------------------------------

        static void VisaKarta()
        {
            int lediga = 0;
            int delvis = 0;
            int fulla = 0;
            StringBuilder rad = new StringBuilder();

            for (int i = 0; i < garage.Platser.Count; i++)
            {
                ParkingSpot plats = garage.Platser[i];
                string farg;

                if (plats.ArTom())
                {
                    farg = "black on green";
                    lediga++;
                }
                else if (plats.ArFull())
                {
                    farg = "white on red";
                    fulla++;
                }
                else
                {
                    farg = "black on yellow";
                    delvis++;
                }

                rad.Append($"[{farg}] {plats.Nummer.ToString("000")} [/] ");

                // Ny rad efter var tionde plats (och efter sista platsen)
                if ((i + 1) % 10 == 0 || i == garage.Platser.Count - 1)
                {
                    AnsiConsole.MarkupLine(rad.ToString());
                    rad.Clear();
                }
            }

            AnsiConsole.MarkupLine("");
            AnsiConsole.MarkupLine($"[green]Lediga: {lediga}[/]    [yellow]Delvis fulla: {delvis}[/]    [red]Fulla: {fulla}[/]");
            AnsiConsole.MarkupLine("");
        }

        // ---------------------------------------------------------------
        // INMATNING – Spectre frågar om tills värdet är giltigt
        // ---------------------------------------------------------------

        static string FragaRegNr()
        {
            string svar = AnsiConsole.Prompt(
                new TextPrompt<string>("Registreringsnummer:")
                    .Validate(text =>
                    {
                        if (ArGiltigtRegNr(text.Trim().ToUpper()))
                        {
                            return ValidationResult.Success();
                        }
                        return ValidationResult.Error("[red]1–10 bokstäver eller siffror, inga mellanslag.[/]");
                    }));

            return svar.Trim().ToUpper();
        }

        static int FragaPlatsnummer(string fraga)
        {
            return AnsiConsole.Prompt(
                new TextPrompt<int>(fraga)
                    .Validate(nummer =>
                    {
                        if (nummer >= 1 && nummer <= garage.Platser.Count)
                        {
                            return ValidationResult.Success();
                        }
                        return ValidationResult.Error($"[red]Välj en plats mellan 1 och {garage.Platser.Count}.[/]");
                    }));
        }

        // Bara bokstäver (även Å, Ä, Ö, Ü ...) och siffror, max 10 tecken.
        // Det skyddar också mot konstiga tecken i datafilen och i Spectre-utskrifterna.
        public static bool ArGiltigtRegNr(string regNr)
        {
            if (regNr.Length < 1 || regNr.Length > 10)
            {
                return false;
            }

            foreach (char tecken in regNr)
            {
                if (!char.IsLetterOrDigit(tecken))
                {
                    return false;
                }
            }
            return true;
        }

        // ---------------------------------------------------------------
        // HJÄLPMETODER
        // ---------------------------------------------------------------

        static Vehicle SkapaFordon(string typ, string regNr)
        {
            int storlek = config.StorlekFor(typ);
            if (typ == "MC")
            {
                return new MC(regNr, storlek);
            }
            return new Car(regNr, storlek);
        }

        // De fordonstyper i config.json som programmet har en klass för.
        // Nya typer (t.ex. buss) kan läggas i filen, men kräver också en ny klass i koden.
        static List<string> TillgangligaFordonstyper()
        {
            List<string> typer = new List<string>();
            foreach (VehicleType typ in config.Fordonstyper)
            {
                if (typ.Namn == "Bil" || typ.Namn == "MC")
                {
                    typer.Add(typ.Namn);
                }
            }
            return typer;
        }

        static string FormateraTid(TimeSpan tid)
        {
            return $"{(int)tid.TotalDays} d {tid.Hours} h {tid.Minutes} min";
        }

        // ---------------------------------------------------------------
        // FILER – själva läsningen och skrivningen sker i klassbiblioteket (MinaFiler)
        // ---------------------------------------------------------------

        static void LasInConfig()
        {
            try
            {
                AppConfig? inlast = MinaFiler.LasJson<AppConfig>(ConfigFil);
                if (inlast == null)
                {
                    config = AppConfig.Standard();
                    MinaFiler.SparaJson(ConfigFil, config); // skapa filen om den saknas
                }
                else
                {
                    config = inlast;
                }
            }
            catch (Exception)
            {
                AnsiConsole.MarkupLine("[red]Kunde inte läsa config.json, standardinställningar används.[/]");
                config = AppConfig.Standard();
                Console.ReadKey(true);
            }
        }

        static void LasInPrislista()
        {
            prislista = PriceList.FranRader(MinaFiler.LasRader(PrisFil));

            if (prislista.PrisPerTimme.Count == 0)
            {
                AnsiConsole.MarkupLine("[red]prislista.txt saknas eller innehåller inga priser – parkeringen blir gratis tills filen är rättad.[/]");
                Console.ReadKey(true);
            }
        }

        static void LasInParkering()
        {
            garage = new ParkingGarage(config.AntalPlatser, config.PlatsStorlek);

            try
            {
                List<ParkingSpot>? sparade = MinaFiler.LasJson<List<ParkingSpot>>(DataFil);
                if (sparade != null)
                {
                    garage.LaggTillSparadeFordon(sparade);
                }
            }
            catch (Exception)
            {
                AnsiConsole.MarkupLine("[red]Kunde inte läsa parkering.json, P-huset startar tomt.[/]");
                Console.ReadKey(true);
            }
        }

        // Anropas efter varje förändring: parkering, utcheckning och flytt.
        static void SparaParkering()
        {
            MinaFiler.SparaJson(DataFil, garage.UpptagnaPlatser());
        }
    }
}

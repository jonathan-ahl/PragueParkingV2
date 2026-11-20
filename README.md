# Prague Parking 2.0

Konsolprogram med Spectre.Console för valet parking vid slottet i Prag. Fordonen hanteras som objekt, data sparas i JSON och priserna läses från en textfil.

## Köra programmet

1. Öppna `PragueParking.sln` i **Visual Studio 2026 Community**.
2. Tryck **F5**.

**PragueParking** är redan startprojekt (det ligger först i `.sln`-filen). Om Visual Studio ändå skulle visa ett annat projekt i fetstil: högerklicka på **PragueParking** → *Set as Startup Project*.

Projektet använder **.NET 10**, som följer med Visual Studio 2026. Spectre.Console och MSTest hämtas automatiskt från NuGet första gången lösningen byggs, så datorn behöver vara uppkopplad mot internet.

Utan Visual Studio (kräver .NET 10 SDK):

```
dotnet run --project PragueParking
dotnet test
```

## Använda programmet

- I menyerna väljer man med **piltangenterna ↑ ↓** och bekräftar med **Enter**. (Det står även i programmet.)
- Registreringsnummer och platsnummer skrivs in och bekräftas med Enter. Är värdet ogiltigt frågar programmet igen.
- Kartan överst visar beläggningen: **grön** = ledig, **gul** = delvis full, **röd** = full.

## Köra testerna

*Test → Run All Tests* (eller Test Explorer).

## Lösningens delar

| Projekt | Innehåll |
|---|---|
| `PragueParking` | Konsolprogrammet: klasserna `Vehicle`, `Car`, `MC`, `ParkingSpot`, `ParkingGarage`, `AppConfig`, `PriceList` och menyn i `Program.cs` |
| `PragueParking.Data` | Klassbibliotek för dataåtkomst: `MinaFiler` läser och sparar filer |
| `PragueParking.Tests` | Enhetstester med MSTest |

## Filer

Filerna kopieras till programmets mapp (`PragueParking/bin/Debug/net10.0/`) när programmet byggs, och det är där de läses och sparas.

| Fil | Innehåll |
|---|---|
| `config.json` | Antal P-platser, platsstorlek, fordonstyper och antal fordon per plats. Läses vid start. |
| `prislista.txt` | Pris per påbörjad timme och antal gratisminuter. Kan ändras medan programmet körs och läsas in på nytt från menyn. |
| `parkering.json` | De parkerade fordonen. Läses vid start och sparas efter varje parkering, utcheckning och flytt. Följer med testdata. |

Vill man börja om med testdatan: välj *Build → Rebuild Solution* efter att ha ändrat `PragueParking/parkering.json`, eller radera filen i `bin/Debug/net10.0/` och bygg igen.

Fordonstyperna i menyn hämtas från `config.json`. Programmet har klasser för `Bil` och `MC`; en ny typ (t.ex. buss) behöver även en ny klass i koden.

## Prisregler

- Bil: 20 CZK per påbörjad timme
- MC: 10 CZK per påbörjad timme
- De första 10 minuterna är gratis

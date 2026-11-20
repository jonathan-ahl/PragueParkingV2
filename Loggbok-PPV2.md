# Loggbok Prague Parking 2.0


**Namn:** Jonathan Ahl
**Projekt:** Prague Parking 2.0 – OOP-version (Inlämningsuppgift 2)
**Period:** 2026-10-12 – 2026-11-20
**Repo:** https://github.com/jonathan-ahl/PragueParkingV2

---

## 2026-11-15
**Tid:** ca 1,5 h

**Gjort**
- Läste uppgiften och gjorde en checklista över G-kraven. Delade upp dem i tre högar:
  1. **Klasser:** `Vehicle`, `Car`, `MC`, `ParkingSpot`, `ParkingGarage`
  2. **Filer:** `config.json`, `prislista.txt`, `parkering.json`, klassbibliotek för dataåtkomst
  3. **Övrigt:** Spectre.Console, karta, MSTest-projekt med minst två tester, README
- Ritade klasserna på papper: ett `ParkingGarage` har en `List<ParkingSpot>`, en `ParkingSpot` har en `List<Vehicle>`.

**Tankar**
- I V1 var det strängarna `"MC#ABC|MC#DEF"` som krånglade mest. Nu blir det objekt i listor i stället – det borde göra mycket av V1-koden enklare.

---

## 2026-11-16 – Lösning med tre projekt
**Tid:** ca 4 h

**Gjort**
- Skapade en solution med tre projekt: `PragueParking` (konsolapp), `PragueParking.Data` (klassbibliotek) och `PragueParking.Tests` (MSTest).
- Lade till projektreferenser: appen → Data, Tests → appen.
- Installerade Spectre.Console via NuGet.


**Problem och lösning**
- *Problem:* `MinaFiler` syntes inte i `Program.cs`.
- *Orsak:* Projektreferensen fanns, men jag hade glömt `using PragueParking.Data;` och klassen var `internal`.
- *Lösning:* Gjorde klassen `public` och lade till `using`.

---

## 2026-11-16 – Klasserna
**Tid:** ca 4 h

**Gjort**
- `Vehicle` blev en `abstract` basklass med `RegNr`, `Storlek` och `Ankomst`. `Car` och `MC` ärver och talar bara om sitt `Typnamn` (`override`).
- `ParkingSpot`: `Nummer`, `Storlek` och `List<Vehicle> Fordon`, plus `Rymmer(fordon)`, `ArTom()` och `ArFull()`.
- `ParkingGarage`: `Parkera`, `HittaFordon`, `CheckaUt`, `Flytta`. Logiken från V1 gick att återanvända, men blev mycket kortare.

**Problem och lösning**
- *Problem:* Hur ska en P-ruta veta att den rymmer 2 MC men bara 1 bil?
- *Lösning:* Varje fordon har en storlek och rutan har en storlek (4). Bil = 4, MC = 2. `Rymmer` kollar bara om fordonets storlek ≤ ledig yta. Det gör det enkelt att senare lägga till t.ex. cyklar (storlek 1).

---

## 2026-11-17 – Spara och läsa JSON
**Tid:** ca 4 h

**Gjort**
- `MinaFiler.SparaJson<T>` och `LasJson<T>` med `System.Text.Json`.
- Programmet sparar efter varje parkering, utcheckning och flytt.

**Problem och lösning**
- *Problem:* Efter omstart kraschade programmet: *"Deserialization of interface or abstract types is not supported"*.
- *Orsak:* Listan innehåller `Vehicle`, som är abstrakt. JSON-filen sa inte om fordonet var en bil eller en MC.
- *Lösning:* Lade till `[JsonPolymorphic]` och `[JsonDerivedType]` på `Vehicle`. Nu skrivs `"Typ": "Bil"` eller `"Typ": "MC"` in i filen och rätt klass skapas vid inläsning.
- *Problem 2:* Å, Ä och Ö sparades som koder i filen, t.ex. `\u00C5` i stället för Å.
- *Lösning:* `Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping` i inställningarna.

---

## 2026-11-18 – Konfigurationsfil och prislista
**Tid:** ca 3,5 h

**Gjort**
- `config.json` med antal platser, platsstorlek och fordonstyper (`Namn`, `AntalPerPlats`). Fordonets storlek räknas ut som platsstorlek / antal per plats.
- `prislista.txt` i formatet `Bil = 20`. Allt efter `#` är kommentar, så instruktionerna står i själva filen.
- Menyval för att läsa in prislistan på nytt.
- Prisberäkning: gratis de första 10 minuterna, sedan per påbörjad timme (`Math.Ceiling`).

**Problem och lösning**
- *Problem:* Ändringar i `config.json` syntes inte när jag körde programmet.
- *Orsak:* Programmet läser filerna från `bin/Debug/net10.0/`, inte från projektmappen.
- *Lösning:* *Copy to Output Directory = Copy if newer* på filerna, och läser dem via `AppContext.BaseDirectory`. Skrev in det i README.

---

## 2026-11-19 – Spectre.Console och kartan
**Tid:** ca 5 h

**Gjort**
- Bytte ut sifferstyrd meny mot `SelectionPrompt` (piltangenter + Enter).
- Inmatning med `TextPrompt` och `.Validate(...)` – Spectre frågar själv om när värdet är fel.
- Karta: tio platser per rad, grön = ledig, gul = delvis full, röd = full, och summering under.
- Kvitto vid utcheckning som tabell: ankomst, parkerad tid och pris.

**Problem och lösning**
- *Problem:* Programmet kraschade med *"Could not find color or style"* när ett regnr innehöll `[`.
- *Orsak:* Spectre tolkar `[...]` som färgkoder.
- *Lösning:* Regnr får bara innehålla bokstäver och siffror – då kan det aldrig hända. (Annars: `Markup.Escape`.)

---

## 2026-11-19 – Enhetstester
**Tid:** ca 3 h

**Gjort**
- Skrev MSTest-tester enligt Arrange–Act–Assert:
  - två MC delar samma plats
  - en bil parkeras inte bredvid en MC
  - de första tio minuterna är gratis
  - 61 minuter = 2 påbörjade timmar
  - kommentarer i prislistan ignoreras
**Problem och lösning**
- **Problem:** Test Explorer hittade inga tester.
- **Orsak:** Metoden saknade `[TestMethod]` och klassen var inte `public`.
- **Lösning:** Lade till attributen. Alla tester gröna.

---

## 2026-11-20 – Säkerhet och felhantering
**Tid:** ca 2 h

**Gjort** 
- `try/catch` runt inläsning av `config.json` och `parkering.json`. Är filen trasig startar programmet med standardvärden i stället för att krascha.
- Varning om `prislista.txt` saknas eller är tom.
- Validering av all inmatning: platsnummer måste finnas i P-huset, regnr 1–10 tecken.

---

## 2026-11-20 – Granskning, F5-fel och förbättrad menyhjälp
**Tid:** ca 4 h

Gick igenom hela lösningen mot G-kraven i uppgiftsbeskrivningen, med hjälp av en vän för granskning och testkörning.

### F5 startade fel projekt på en annan dator
- **Problem:** När lösningen öppnades utan `.vs`-mappen (som när läraren klonar repot) valde Visual Studio `PragueParking.Data` som startprojekt. F5 gav då felet att ett klassbibliotek inte kan startas.
- **orsak:** I `.slnx`-filen sorterar Visual Studio projekten efter sökväg, och `PragueParking.Data/` hamnar före `PragueParking/`. Startprojektet sparas bara lokalt i `.vs`, som inte ligger i repot.
- **Lösning:** Bytte `PragueParking.slnx` mot `PragueParking.sln` där `PragueParking` står först – i en `.sln` blir första projektet startprojekt.  Öppnade en kopia utan `.vs` i Visual Studio → `PragueParking` var startprojekt och programmet startade med Start/F5.

### Menyn förklarade inte hur man väljer (UX)
- **problem:** I V1 valde man med siffror. I V2 används Spectre.Consoles `SelectionPrompt`, där man väljer med piltangenterna och Enter. Det stod ingenstans – varken i programmet eller i README – så en ny användare förstår inte direkt hur menyn fungerar.
- **Lösning:**
  - Lade till konstanten `MenyHjalp` i `Program.cs` med texten *"(välj med ↑ ↓ och tryck Enter)"* i grått, och visar den i rubriken på både huvudmenyn och frågan om fordonstyp.
  - Nytt avsnitt *Använda programmet* i README: piltangenter, inmatning och kartans färger.
- **Alternativ som övervägdes:** Byta tillbaka till sifferval som i V1. Valdes bort eftersom piltangentsmenyn är Spectres standard och fungerar bra – problemet var att den inte förklarades, inte själva menyn.
- **Ändrade filer:** `PragueParking/Program.cs`, `README.md`


---

## 2026-11-20 – README och städning
**Tid:** ca 2,5 h

**Gjort**
- Använde mig av hjälp för att hitta eventuella förbättringar.
- README: hur man startar, vad filerna gör, hur man kör testerna och att NuGet-paketen kräver internet första gången.
- Fordonstyperna i menyn hämtas nu från `config.json` i stället för att vara hårdkodade.
- Gick igenom checklistan jag gjorde tidigare – alla G-krav uppfyllda.
- Lade till `.gitignore`.
- "Flytta fordon" kontrollerar först att fordonet finns och visar nuvarande plats innan programmet frågar efter ny plats.
- Två nya enhetstester: fordon sparas och läses in som rätt typ (`Car`/`MC`), och en bil kan inte flyttas till en plats där det står en MC.
- README: `.sln` i stället för `.slnx`, .NET 10, `dotnet run`/`dotnet test`, hur man återställer testdatan.

---

## Sammanfattning
- **Total tid:** ca 36 h
- **Svårast:** Att spara en lista med en abstrakt basklass till JSON, och att få programmet att fungera på en annan dator (filsökvägar och startprojekt).
- **Viktigaste lärdomen:** Med klasser och arv blev koden kortare och lättare att förstå än strängarna i V1. Att lägga filhanteringen i ett eget klassbibliotek gjorde att resten av programmet inte behöver bry sig om filer, och att logiken utan Console-anrop gick att enhetstesta.

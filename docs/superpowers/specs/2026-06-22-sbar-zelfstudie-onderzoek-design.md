# SBAR Zelfstandig Onderzoek — Design Spec
**Datum:** 2026-06-22
**Project:** SBAR VR Trainingsomgeving — Mevrouw De Groot
**Scope:** Uitbreiding van scène 4 (Vitalen) naar vrije zelfstandige onderzoeksfase

---

## Samenvatting

Scène 4 wordt omgebouwd van een vaste, gestuurde vitalenmeting naar een vrije onderzoeksfase. De student beslist zelf welke objecten in de kamer hij onderzoekt en in welke volgorde. De HUD geeft alleen een globale hint. Gemiste bevindingen hebben geen tussentijdse consequenties — alleen de eindfeedback (scène 7) toont wat ontbrak.

---

## Architectuur

### Nieuwe componenten

**`IInvestigatable` (interface, `SBAR.Interaction`)**
```
void OnInspect(InvestigationResult result)
```
Aparte interface van `IInteractable`. Objecten kunnen beide implementeren.

**`InvestigationResult` (dataklasse, `SBAR.Core`)**
```
string id            // uniek per object, bijv. "saturatiemeter"
SBARCategory cat     // Situation / Background / Assessment / Recommendation
string finding       // tekst die in notitieboekje verschijnt
bool isCritical      // telt mee in eindscore feedback
```

**`InvestigationTracker` (singleton, `SBAR.Core`)**
```
List<InvestigationResult> all    // alle mogelijke bevindingen (bij Start ingevuld)
List<string> found               // geïnspecteerde id's
void Register(InvestigationResult result)
List<InvestigationResult> GetMissed()
List<InvestigationResult> GetFound()
```
Geïnitialiseerd door `SBARManager` bij start van scène `Vitalen`.

---

## Kamerobjecten

### Bestaande objecten (krijgen `IInvestigatable` erbij)

| Object | Bevinding | Categorie | Kritiek |
|--------|-----------|-----------|---------|
| Saturatiemeter | SpO₂ 88% ⚠ afwijkend | Assessment | ja |
| Bloeddrukband | 155/95 ⚠ afwijkend | Assessment | ja |
| Thermometer | 37,2 °C normaal | Assessment | nee |

### Nieuwe objecten (primitives + label)

| Object | Type | Bevinding | Categorie | Kritiek |
|--------|------|-----------|-----------|---------|
| Dossier | Cube op tafel | Bekend met hartfalen, diureticum | Background | ja |
| Medicatielijst | Quad/papier | Furosemide 40mg, laatste dosis vanmorgen | Background | nee |
| Zuurstofmeter aan muur | Cube aan muur | Kamerlucht, geen O₂ actief | Assessment | nee |
| Infuuszak | Capsule aan standaard | NaCl 0,9%, loopsnelheid hoog | Situation | nee |
| Verpleegdagboek | Cube op tafel | Gisteren ook benauwd, niet gerapporteerd | Background | ja |

Kritieke bevindingen: **4** (saturatiemeter, bloeddrukband, dossier, verpleegdagboek).

---

## Scèneflow — scène 4 (Vitalen)

1. `SBARManager.AdvanceTo(Vitalen)` initialiseert `InvestigationTracker` met alle `InvestigationResult`-definities.
2. HUD toont: *"Onderzoek de kamer. Druk op N voor notitieboekje."*
3. Alle `IInvestigatable`-objecten zijn actief. Student beweegt vrij door kamer.
4. Bij interactie met object:
   - Ondertiteling toont bevinding (`SubtitleSystem`)
   - Notitieboekje krijgt automatisch regel (met categorie-icoontje)
   - `InvestigationTracker.Register(result)` aangeroepen
5. Knop "Klaar met onderzoeken" (rechtsonder) triggert `AdvanceTo(Voorbereiden)`.
6. Geen blokkade op ontbrekende bevindingen.

---

## Wijzigingen bestaande systemen

### `SBARManager`
- Scène `Vitalen`: start vrije fase i.p.v. vaste volgorde
- Voegt "Klaar met onderzoeken"-knop toe
- Initialiseert `InvestigationTracker`

### `SBARNotebook`
- Regels komen via `InvestigationTracker`-events (i.p.v. hardcoded per scène)
- Elk item toont categorie-icoontje (📋 dossier, 💊 medicatie, 📊 vitalen) — OE-D3

### `FeedbackSystem` (scène 7)
- Roept `InvestigationTracker.GetMissed()` en `GetFound()` aan
- Kritieke gemiste bevindingen: rode rij `"⚠ Niet onderzocht: [naam] — [wat je had kunnen vinden]"`
- Gevonden: groene rij `"✓ [naam]"`
- Topscore: `X van 4 kritieke bevindingen gevonden`
- Bestaande SBAR-onderdeel feedback blijft eronder

### Ongewijzigd
`InteractionRaycaster`, `SubtitleSystem`, `PlayerController`, scènes 1–3, 5–6.

---

## Ontwerpeisen

| Eis | Hoe geborgd |
|-----|-------------|
| OE-B1: besturing vanzelfsprekend | HUD-hint, geen verplichte volgorde |
| OE-B3: rustige omgeving | Geen timer, geen druk |
| OE-B4: notitieboekje altijd oproepbaar | Toets N, gevuld via tracker-events |
| OE-D1: ondertiteling | `SubtitleSystem` bij elke interactie |
| OE-D3: kleur niet enige drager | Icoontje + tekst "afwijkend" naast kleur |
| OE-D5/D6: herhaalbaar | Scène herspeelbaar, tracker reset bij herspeel |
| OE-C1/C3: feedback ondersteunend | Eindfeedback toont gemist + gevonden |

---

## Uitbreidingshaken (niet uitwerken nu)

- `// TODO VR:` XR-controller vervangt `InteractionRaycaster` — `IInvestigatable` ongewijzigd
- `// TODO Spraak:` `IVoiceInput` als alternatief naast klikken op objecten
- `// TODO Timer:` optionele tijdsdruk per scène als moeilijkheidsgraad
- `// TODO Hints:` knop "Geef hint" toont pijl naar ongezien kritiek object

---

## Scripts aan te maken

| Script | Namespace | Doel |
|--------|-----------|------|
| `IInvestigatable.cs` | SBAR.Interaction | Interface |
| `InvestigationResult.cs` | SBAR.Core | Dataklasse |
| `InvestigationTracker.cs` | SBAR.Core | Singleton tracker |
| `InvestigatableObject.cs` | SBAR.Interaction | MonoBehaviour voor kamerobjecten |

Wijzigingen in: `SBARManager.cs`, `SBARNotebook.cs`, `FeedbackSystem.cs`.

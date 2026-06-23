# SBAR Unity Trainingsprototype

Een interactief trainingsscenario in Unity waarin verpleegkundestudenten de **SBAR-methodiek** (Situation, Background, Assessment, Recommendation) oefenen aan de hand van een klinische casus.

## Casus

**Mevrouw De Groot, 74 jaar** — opgenomen na een heupoperatie, bekend met hartfalen. Tijdens de dienst wordt zij plotseling kortademig. De student onderzoekt de patiënt, verzamelt bevindingen en draagt deze gestructureerd (SBAR) over aan de arts.

## Spelverloop

De simulatie doorloopt zeven fasen:

1. **Briefing** — een collega vraagt je bij kamer 7 te kijken.
2. **Binnenkomst** — de patiënt is benauwd; het saturatie-alarm gaat af.
3. **Vragen** — stel relevante vragen aan de patiënt.
4. **Onderzoeksfase** — beweeg vrij door de kamer en onderzoek objecten (dossier, medicatielijst, meetapparaten, infuus, verpleegdagboek). Kritieke bevindingen tellen mee in de score.
5. **Aanbeveling** — kies een passende vervolgactie.
6. **Overdracht** — draag elk SBAR-onderdeel over aan de arts.
7. **Feedback** — beoordeling van volledigheid en gemiste kritieke bevindingen.

## Besturing

| Toets / muis | Actie |
|--------------|-------|
| **WASD** | Lopen |
| **Rechtermuisknop ingedrukt** | Rondkijken |
| **E** of **linkerklik** | Onderzoeken / interactie |
| **N** | Notitieboekje openen/sluiten |

## Openen

- **Unity-versie:** 6000.4.7f1 (Unity 6)
- **Render pipeline:** Universal Render Pipeline (URP)
1. Clone de repo.
2. Open de map als project in Unity Hub (juiste versie).
3. Laat Unity de packages oplossen (o.a. `com.unity.cloud.gltfast` voor `.glb`-modellen).
4. Open de scène in `Assets/Scenes/` en druk op **Play**.

## Projectstructuur

```
Assets/
├── Scripts/
│   ├── Core/          # SBARManager, Notebook, InvestigationTracker, ...
│   ├── Interaction/   # PlayerController, raycaster, interacteerbare objecten
│   ├── UI/            # HUD, ondertitels, notitieboek, feedbackscherm
│   └── Data/          # DialogueData (ScriptableObject met casusteksten)
├── Editor/            # Hulpscripts onder het 'SBAR'-menu (scene bouwen/koppelen)
├── Imported/          # Externe assets (modellen, audio, UI)
└── Scenes/
```

### SBAR-menu (Editor-hulpscripts)

In de Unity-menubalk onder **SBAR**:

- **Build Entire Scene** — bouwt de volledige scène van nul (destructief).
- **Setup Scene** — oudere scene-setup.
- **Setup Onderzoeksobjecten** — plaatst/koppelt de onderzoeksobjecten van de vrije fase.
- **Plaats Echte Assets** — vervangt placeholders door geïmporteerde modellen.
- **Plaats Achtergrondgeluid** — koppelt het ziekenhuis-sfeergeluid + AudioListener.
- **Fix HUD Layout** — past de HUD-posities aan.

## Status

Werkend prototype voor gebruikerstests. De volledige SBAR-flow is speelbaar met echte modellen, audio en feedbackscoring.

### Bekende punten / vervolg

- Collega en arts gebruiken (nog) hetzelfde personagemodel.
- Modelschaal/positie van enkele assets kan verder bijgesteld worden (TUNE-constants in de Editor-scripts).
- Thermometer is nog een placeholder.
- VR-rig nog niet ingebouwd (hooks staan klaar in `PlayerController` / `InteractionRaycaster`).

## Licenties

Geïmporteerde assets (modellen, audio, UI) zijn afkomstig van externe bronnen (o.a. Mixamo, Sketchfab, Kenney, Freesound) en vallen onder hun eigen licenties. Controleer per asset de bijbehorende licentievoorwaarden voordat je het project publiek of commercieel gebruikt.

# SBAR Unity Trainingsprototype

Een interactief **VR-trainingsscenario** in Unity waarin verpleegkundestudenten de **SBAR-methodiek** (Situation, Background, Assessment, Recommendation) oefenen aan de hand van een klinische casus. Speelbaar met een Meta Quest-headset, in de XR Device Simulator én met muis/toetsenbord op een laptop.

## Casus

**Mevrouw De Groot, 74 jaar** — opgenomen na een heupoperatie, bekend met hartfalen. Tijdens de dienst wordt zij plotseling kortademig. De student onderzoekt de patiënt, verzamelt bevindingen en draagt deze gestructureerd (SBAR) over aan de arts.

## Spelverloop

De simulatie doorloopt zeven fasen:

1. **Briefing** — een collega vraagt je bij kamer 7 te kijken.
2. **Binnenkomst** — de patiënt is benauwd; het saturatie-alarm gaat af.
3. **Vragen** — stel relevante vragen aan de patiënt.
4. **Onderzoeksfase** — beweeg vrij door de kamer en onderzoek objecten (dossier, medicatielijst, meetapparaten, infuuszak, verpleegdagboek). Kritieke bevindingen tellen mee in de score. Tijdens deze fase loopt een **tijdslimiet** (richtwaarde 2 min) als zachte druk — bij 0:00 verschijnt een melding, maar de speler mag doorgaan.
5. **Aanbeveling** — kies een passende vervolgactie.
6. **Overdracht** — draag elk SBAR-onderdeel over aan de arts.
7. **Feedback** — beoordeling van volledigheid en gemiste kritieke bevindingen.

De verzamelde informatie komt terecht in een **SBAR-notitieboek** in het gezichtsveld (HUD), met tabs per letter (S/B/A/R) zodat het overzicht niet dichtslibt.

## Besturing

**VR (Meta Quest):** controller-ray om objecten/UI aan te wijzen, trigger om te onderzoeken/kiezen. Bewegen via teleportatie.

**Laptop / XR Device Simulator:**

| Toets / muis | Actie |
|--------------|-------|
| **WASD** | Lopen |
| **Rechtermuisknop ingedrukt** | Rondkijken |
| **E** of **linkerklik** (object in beeldmidden) | Onderzoeken / interactie |
| **Muisklik** (op knop) | UI-knoppen, keuzes, notitieboek-tabs |
| **N** | Notitieboek openen/sluiten |
| **H** of de **? Hulp**-knop | Besturings- en doeluitleg |

> Wereld-objecten interacteren vanaf het beeldmidden (kruis): kijk het object aan en druk **E**. UI-knoppen klik je direct met de muis aan.

## VR & toegankelijkheid

- **XR:** XR Interaction Toolkit 3.5.1 + OpenXR, Meta Quest feature-set en Oculus Touch controller-profielen. Testen kan zonder headset via de **XR Device Simulator**.
- **World-space HUD** die met de blik mee-draait (yaw-follow), inclusief notitieboek, ondertitels en hulp.
- **Toegankelijkheid** (uit de ontwerpeisen):
  - Ondertiteling bij alle gesproken tekst.
  - Knop-/menualternatief naast spraak; kleur wordt nooit als enige informatiedrager gebruikt (afwijkende waarden ook in tekst).
  - Oproepbare hulp via toets én HUD-knop, en een zachte tijdslimiet i.p.v. afstraffende feedback.

## Openen

- **Unity-versie:** 6000.4.7f1 (Unity 6)
- **Render pipeline:** Universal Render Pipeline (URP)
- **Build-doel:** standalone Meta Quest `.apk` (Android, IL2CPP, ARM64, min. API 32). Configuratie staat in het project; bouwen via `BuildPipeline` / Build Profiles.
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

Werkend VR-prototype voor gebruikerstests. De volledige SBAR-flow is speelbaar met echte modellen, audio, world-space UI, een SBAR-notitieboek met tabs, tijdslimiet en feedbackscoring. Interactie werkt zowel met VR-controllers als met muis/toetsenbord in de simulator.

### Bekende punten / vervolg

- Nog niet op fysieke Quest-hardware getest; controller-interactie kan na de eerste headset-test bijgesteld worden.
- De geïnstalleerde `.apk` moet opnieuw gebouwd worden om al het recente werk te bevatten.
- Collega en arts gebruiken (nog) hetzelfde personagemodel.
- Spraakinteractie (`IVoiceInput`) is voorbereid maar nog niet geïmplementeerd; momenteel werkt alles via knop-/ray-interactie.
- Modelschaal/positie van enkele assets kan verder bijgesteld worden (TUNE-constants in de Editor-scripts).

## Licenties

Geïmporteerde assets (modellen, audio, UI) zijn afkomstig van externe bronnen (o.a. Mixamo, Sketchfab, Kenney, Freesound) en vallen onder hun eigen licenties. Controleer per asset de bijbehorende licentievoorwaarden voordat je het project publiek of commercieel gebruikt.

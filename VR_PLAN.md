# VR-ombouw plan — SBAR (standalone Meta Quest, .apk)

Target: standalone Quest, Android-build, OpenXR + XR Interaction Toolkit. URP (al aanwezig). Input System 1.19 al aanwezig.

## Prereqs (USER doet, buiten MCP)
- [ ] Unity Hub → Installs → 6000.4.7f1 → Add Modules → **Android Build Support** (+ OpenJDK + Android SDK & NDK Tools)
- [ ] Quest: Developer Mode aan (Meta Horizon app) + USB-debugging toestaan
- [ ] (test) Quest via USB aan PC, of build .apk en sideload (SideQuest/adb)

## Stap 1 — Packages (CLAUDE via MCP) — DONE
- [x] com.unity.xr.interaction.toolkit 3.5.1 (XRI) — trekt xr.management + core-utils mee
- [x] com.unity.xr.openxr (OpenXR)
- [ ] (later evt.) Starter Assets sample voor kant-en-klare controller-prefab + UI input module

## Milestones
- **M1 = rondkijken op headset** (head-tracking) → valideert build-pipeline. Scene-werk DONE.
- **M2 = controllers + interactie** (VRInteractor staat klaar) + locomotion (teleport).
- **M3 = UI naar world-space** (HUD/menus/feedback).

## DONE in scene (M1 + M2-foundation) — getest in XR Device Simulator, werkt
- [x] Volledige rig = Starter Assets prefab "XR Origin (XR Rig)" op (2,0,0). Camera + 2 controllers (Near-Far interactors) + InputModalityManager + gaze.
- [x] Kale XR Origin (VR) verwijderd.
- [x] XR Device Simulator prefab in scene → test in Editor zonder headset (muis/kb).
- [x] Oude Player-rig conflicten uit: PlayerCamera untagged + Camera/AudioListener/InteractionRaycaster disabled, PlayerController disabled. Player-object blijft active (SBARManager-refs intact).
- [x] VRInteractor.cs op **Right Controller** (-64856): ray vanaf controller → IInteractable.OnInteract, trigger-binding `<XRController>{RightHand}/triggerPressed`.
- [x] Play-test simulator: kamer rendert vanuit VR-camera, controllers zichtbaar, 0 errors.

## DONE — M2 + M3 (getest in simulator)
- [x] M2 teleport: XR Interaction Manager toegevoegd; TeleportationArea op Vloer (dubbele verwijderd); rig heeft TeleportationProvider + 2 teleport-interactors.
- [x] M3 UI: HUD_Canvas → world-space, geparent aan Main Camera (head-locked), local pos (0,-0.25,2), rot (0,0,0), scale 0.0016. + TrackedDeviceGraphicRaycaster (controller-ray klikt knoppen). Tekst leesbaar in VR getest.
      → Overdracht/Feedback/keuzemenu/briefing zijn children van HUD_Canvas = automatisch mee-geconverteerd.
- [x] 0 console-errors.

## NOG TE DOEN
- [ ] M2: verifieer trigger→OnInteract echt vuurt (USER in sim/headset).
- [ ] HUD-comfort: nu head-locked (volgt hoofd). Evt. naar zachte body-lock/follow voor minder misselijk. WorldCanvas (69186) bestaat al, ongebruikt onderzoeken.
- [ ] SBARManager input: N-toets → VR-knop (menu-knop controller).
- [ ] Android build-config (XR Plug-in Mgmt OpenXR+Meta Quest, IL2CPP/ARM64/API32, Linear) + .apk (USER, bij headset).

## Stap 2 — Project config (CLAUDE waar kan, USER waar GUI nodig)
- [ ] Switch active platform → Android
- [ ] Player Settings: scripting backend IL2CPP, target arch ARM64, min API 32 (Quest), color space Linear
- [ ] XR Plug-in Management → Android tab → OpenXR aan + Meta Quest feature group
- [ ] OpenXR interaction profiles: Oculus Touch Controller Profile
- [ ] URP: mobile-vriendelijk (MSAA, geen dure post-fx) — later tunen

## Stap 3 — XR Rig in scene (CLAUDE)
- [ ] XR Origin (camera + 2 controllers) vervangt/naast Player
- [ ] Locomotion: teleport + snap-turn (comfort) of continu
- [ ] Controllers: Ray Interactor (interactie) + XR UI Input Module + ray voor world-UI
- [ ] Behoud bestaande PlayerController als desktop-fallback? (optie)

## Stap 4 — Interactie-adapter (CLAUDE)
- [ ] InteractionRaycaster → XR Ray Interactor roept `IInteractable.OnInteract()`
  - bridge-component: op select-event → GetComponentInParent<IInteractable>().OnInteract()
- [ ] Investigatables hebben al colliders → check interaction layer mask

## Stap 5 — Input migratie (CLAUDE)
- [ ] SBARManager N-toets → XR-knop (of menu-knop op controller)
- [ ] PlayerController Input.GetAxis/Mouse → XR (of vervangen door XR locomotion)
- [ ] HUDController input

## Stap 6 — UI naar world-space (CLAUDE) — grootste brok
- [x] Notitieboek = al world-space (op klembord)
- [ ] HUDController (instructie/ondertitel) → world-space (pols-menu of vast paneel)
- [ ] Keuzemenu → world-space + TrackedDeviceGraphicRaycaster
- [ ] OverdrachtPanel → world-space
- [ ] FeedbackScreen → world-space
- [ ] Alle canvases: EventSystem met XR UI Input Module

## Stap 7 — Build + test (USER)
- [ ] Build .apk, sideload, test op Quest
- [ ] Meld wat misstaat → CLAUDE bijstellen via MCP

## Notities
- MCP werkt nu (CoplayDev, port 6400). Zie [[unity-mcp-setup]].
- Caveman-modus aan.
- .apk-builds traag; CLAUDE kan NIET bouwen/testen — user doet build-loop.

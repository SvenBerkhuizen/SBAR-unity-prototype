using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using SBAR.Interaction;
using SBAR.UI;

namespace SBAR.Core
{
    public class SBARManager : MonoBehaviour
    {
        public static SBARManager Instance { get; private set; }

        [Header("Dialoogdata")]
        public DialogueData dialogue;

        [Header("Besturing")]
        public InteractionRaycaster raycaster;
        public PlayerController player;

        [Header("UI-referenties")]
        public HUDController hud;
        public SubtitleSystem subtitles;
        public NotebookUI notebookUI;
        public ChoiceMenu choiceMenu;
        public OverdrachtPanel overdrachtPanel;
        public FeedbackScreen feedbackScreen;

        [Header("Scene-objecten")]
        public MeasurementDevice saturatiemeter;
        public MeasurementDevice bloeddrukband;
        public MeasurementDevice thermometer;
        public NPCInteractable collega;
        public NPCInteractable arts;

        [Header("Onderzoeksobjecten (vrije fase)")]
        public List<InvestigatableObject> onderzoeksobjecten = new List<InvestigatableObject>();
        public InvestigationTracker investigationTracker;

        public SBARNotebook Notebook { get; private set; }
        public SBARScene CurrentScene { get; private set; }

        private bool[] _vragenGesteld;
        private int _relevantVragenGesteld;
        private int _irrelevanteVragenGesteld;
        private readonly HashSet<MeasurementDevice> _gemeten = new HashSet<MeasurementDevice>();
        private int _gekozenAanbeveling = -1;
        private InvestigationResult _satResult, _bpResult, _tempResult;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;

            Notebook = new SBARNotebook();
            if (dialogue == null) dialogue = DialogueData.CreateDefault();
        }

        private void Start()
        {
            if (saturatiemeter != null)
                saturatiemeter.Configure(dialogue.saturatiemeterNaam, dialogue.saturatiemeterReadings);
            if (bloeddrukband != null)
                bloeddrukband.Configure(dialogue.bloeddrukbandNaam, dialogue.bloeddrukbandReadings);
            if (thermometer != null)
                thermometer.Configure(dialogue.thermometerNaam, dialogue.thermometerReadings);

            if (saturatiemeter != null) saturatiemeter.Measured += OnDeviceMeasured;
            if (bloeddrukband != null) bloeddrukband.Measured += OnDeviceMeasured;
            if (thermometer != null) thermometer.Measured += OnDeviceMeasured;
            if (collega != null) collega.Interacted += _ => subtitles?.Show(dialogue.collegaNaam, dialogue.collegaBriefingLijn);
            if (arts != null) arts.Interacted += _ => subtitles?.Show(dialogue.artsNaam, dialogue.artsBegroeting);

            if (notebookUI != null) notebookUI.SetNotebook(Notebook);

            AdvanceTo(SBARScene.Briefing);
        }

        private void Update()
        {
            // Notitieboek-toggle centraal: NotebookUI zit op het paneel zelf en stopt
            // met Update() zodra het paneel inactief is. SBARManager blijft altijd actief.
            if (Input.GetKeyDown(KeyCode.N)) notebookUI?.Toggle();
        }

        public void AdvanceTo(SBARScene next)
        {
            CurrentScene = next;
            switch (next)
            {
                case SBARScene.Briefing:     EnterBriefing();     break;
                case SBARScene.Binnenkomst:  EnterBinnenkomst();  break;
                case SBARScene.Vragen:       EnterVragen();       break;
                case SBARScene.Vitalen:      EnterVitalen();      break;
                case SBARScene.Voorbereiden: EnterVoorbereiden(); break;
                case SBARScene.Overdracht:   EnterOverdracht();   break;
                case SBARScene.Feedback:     EnterFeedback();     break;
            }
        }

        public void AdvanceToNext()
        {
            int volgende = (int)CurrentScene + 1;
            if (volgende <= (int)SBARScene.Feedback)
                AdvanceTo((SBARScene)volgende);
        }

        public void RepeatCurrent()
        {
            AdvanceTo(CurrentScene);
        }

        public void Restart()
        {
            Notebook.Clear();
            _vragenGesteld = null;
            _relevantVragenGesteld = 0;
            _irrelevanteVragenGesteld = 0;
            _gemeten.Clear();
            _gekozenAanbeveling = -1;
            investigationTracker?.Reset();
            feedbackScreen?.Hide();
            AdvanceTo(SBARScene.Briefing);
        }

        public void Quit()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        private void EnterBriefing()
        {
            SetWorldInput(true);
            SetDevicesInteractable(false);
            SetInvestigatableObjectsActive(false);
            collega?.SetInteractable(true);
            arts?.SetInteractable(false);

            hud?.SetInstruction(dialogue.instructieBriefing);
            hud?.ShowBriefingCard(dialogue.briefingKaart);
            subtitles?.Show(dialogue.collegaNaam, dialogue.collegaBriefingLijn);

            foreach (var l in dialogue.baseBackground) Notebook.Add(SBARPart.Background, l);

            choiceMenu?.Hide();
            overdrachtPanel?.Hide();
            feedbackScreen?.Hide();

            hud?.SetNextButton(true, true, dialogue.startKnopLabel, AdvanceToNext);
            hud?.SetRepeatButton(false, null);
        }

        private void EnterBinnenkomst()
        {
            SetWorldInput(true);
            SetDevicesInteractable(false);
            collega?.SetInteractable(false);

            hud?.HideBriefingCard();
            hud?.SetInstruction(dialogue.instructieBinnenkomst);
            subtitles?.Show(dialogue.patientNaam, dialogue.patientBinnenkomstLijn);

            saturatiemeter?.StartAlarmBlink();
            Notebook.Add(SBARPart.Situation, dialogue.binnenkomstSituatie);

            hud?.SetNextButton(true, true, "Volgende stap", AdvanceToNext);
            hud?.SetRepeatButton(true, RepeatCurrent);
        }

        private void EnterVragen()
        {
            SetWorldInput(false);
            SetDevicesInteractable(false);
            hud?.SetInstruction(dialogue.instructieVragen);

            _vragenGesteld = new bool[dialogue.vragen.Count];
            _relevantVragenGesteld = 0;
            _irrelevanteVragenGesteld = 0;

            var opties = new List<string>();
            foreach (var qa in dialogue.vragen) opties.Add(qa.question);
            choiceMenu?.Show(dialogue.vragenPrompt, opties, AskQuestion);

            // Vragen zijn optioneel: speler mag altijd door, ook zonder (of met 'foute') vragen.
            // Feedback achteraf rekent met aantal relevante vragen → ruimte voor fouten.
            hud?.SetNextButton(true, true, "Volgende stap", AdvanceToNext);
            hud?.SetRepeatButton(true, RepeatCurrent);
        }

        public void AskQuestion(int index)
        {
            if (_vragenGesteld == null || index < 0 || index >= dialogue.vragen.Count || _vragenGesteld[index]) return;
            _vragenGesteld[index] = true;

            var qa = dialogue.vragen[index];
            subtitles?.Show(dialogue.patientNaam, qa.answer);
            // Alleen relevante vragen leveren een notitie op; niet-relevante vragen
            // zijn 'fout' en voegen niets toe aan het SBAR-dossier.
            if (qa.isRelevant && !string.IsNullOrEmpty(qa.notebookLine))
                Notebook.Add(qa.targetPart, qa.notebookLine);
            choiceMenu?.SetOptionInteractable(index, false);
            if (qa.isRelevant) _relevantVragenGesteld++;
            else _irrelevanteVragenGesteld++;

            int aantalGesteld = _vragenGesteld.Count(b => b);
            if (aantalGesteld >= dialogue.minVragenVoorVervolgStap)
                hud?.SetNextButton(true, true, "Volgende stap", AdvanceToNext);
        }

        private void EnterVitalen()
        {
            choiceMenu?.Hide();
            SetWorldInput(true);

            _gemeten.Clear();
            saturatiemeter?.Configure(dialogue.saturatiemeterNaam, dialogue.saturatiemeterReadings);
            bloeddrukband?.Configure(dialogue.bloeddrukbandNaam, dialogue.bloeddrukbandReadings);
            thermometer?.Configure(dialogue.thermometerNaam, dialogue.thermometerReadings);
            SetDevicesInteractable(true);
            SetInvestigatableObjectsActive(true);

            // Bouw InvestigationResults voor meetapparaten
            _satResult  = new InvestigationResult("saturatiemeter", SBARPart.Assessment, "SpO2 88% ! afwijkend", isCritical: true);
            _bpResult   = new InvestigationResult("bloeddrukband",  SBARPart.Assessment, "Bloeddruk 155/95 ! afwijkend", isCritical: true);
            _tempResult = new InvestigationResult("thermometer",     SBARPart.Assessment, "Temperatuur 37,2 °C normaal", isCritical: false);

            var alleResultaten = new List<InvestigationResult> { _satResult, _bpResult, _tempResult };
            foreach (var obj in onderzoeksobjecten)
                if (obj != null) alleResultaten.Add(obj.GetResult());
            investigationTracker?.Initialize(alleResultaten);

            hud?.SetInstruction(dialogue.vitalenInstructie);
            hud?.SetNextButton(true, true, "Klaar met onderzoeken", AdvanceToNext);
            hud?.SetRepeatButton(true, RepeatCurrent);
        }

        private void OnDeviceMeasured(MeasurementDevice device)
        {
            if (CurrentScene != SBARScene.Vitalen) return;
            _gemeten.Add(device);

            foreach (var r in device.Readings)
                Notebook.Add(SBARPart.Assessment, r.ToLine());

            subtitles?.Show("", $"{device.deviceNaam} gemeten en genoteerd.", 2.5f);

            // Registreer meting in onderzoekstracker
            InvestigationResult devResult = null;
            if (device == saturatiemeter) devResult = _satResult;
            else if (device == bloeddrukband) devResult = _bpResult;
            else if (device == thermometer)   devResult = _tempResult;
            if (devResult != null) investigationTracker?.Register(devResult);
        }

        private void EnterVoorbereiden()
        {
            SetWorldInput(false);
            SetDevicesInteractable(false);
            SetInvestigatableObjectsActive(false);
            saturatiemeter?.StopAlarmBlink();

            hud?.SetInstruction(dialogue.instructieVoorbereiden);
            choiceMenu?.Show(dialogue.voorbereidenPrompt, dialogue.aanbevelingen, ChooseRecommendation);
            hud?.SetNextButton(true, false, "Volgende stap", AdvanceToNext);
            hud?.SetRepeatButton(true, RepeatCurrent);
        }

        public void ChooseRecommendation(int index)
        {
            if (index < 0 || index >= dialogue.aanbevelingen.Count) return;
            _gekozenAanbeveling = index;
            Notebook.Add(SBARPart.Recommendation, dialogue.aanbevelingen[index]);
            subtitles?.Show("", $"Aanbeveling gekozen: {dialogue.aanbevelingen[index]}", 3f);
            choiceMenu?.Hide();
            hud?.SetNextButton(true, true, "Volgende stap", AdvanceToNext);
        }

        private void EnterOverdracht()
        {
            choiceMenu?.Hide();
            SetWorldInput(false);
            SetDevicesInteractable(false);

            arts?.SetInteractable(true);
            hud?.SetInstruction(dialogue.overdrachtInstructie);
            subtitles?.Show(dialogue.artsNaam, dialogue.artsBegroeting);

            notebookUI?.Show();
            overdrachtPanel?.Setup(Notebook, OnAllHandedOver);
            overdrachtPanel?.Show();

            hud?.SetNextButton(true, false, "Volgende stap", AdvanceToNext);
            hud?.SetRepeatButton(true, RepeatCurrent);
        }

        private void OnAllHandedOver()
        {
            hud?.SetNextButton(true, true, "Naar feedback", AdvanceToNext);
        }

        private void EnterFeedback()
        {
            overdrachtPanel?.Hide();
            SetWorldInput(false);
            hud?.SetInstruction(dialogue.instructieFeedback);
            hud?.SetNextButton(false, false, "", null);
            hud?.SetRepeatButton(false, null);

            var lijnen = BuildFeedback();
            feedbackScreen?.Show(dialogue.feedbackTitel, lijnen, dialogue.herspeelLabel,
                                 dialogue.afsluitenLabel, Restart, Quit);
        }

        public List<FeedbackLine> BuildFeedback()
        {
            var lijnen = new List<FeedbackLine>();

            // Onderzoeksfase-evaluatie
            if (investigationTracker != null)
            {
                int gevonden = investigationTracker.CriticalFound;
                int totaal   = investigationTracker.CriticalCount;
                FeedbackStatus invStatus = gevonden == totaal ? FeedbackStatus.Goed
                    : gevonden > 0 ? FeedbackStatus.Gedeeltelijk
                    : FeedbackStatus.Onvoldoende;
                lijnen.Add(new FeedbackLine("Onderzoek", $"{gevonden} van {totaal} kritieke bevindingen gevonden", invStatus));

                foreach (var gemist in investigationTracker.GetMissed())
                    if (gemist.isCritical)
                        lijnen.Add(new FeedbackLine("! Gemist", $"{gemist.id}: {gemist.finding}", FeedbackStatus.Onvoldoende));
            }

            // Vragen-evaluatie
            int totaalRelevant = dialogue.vragen.Count(qa => qa.isRelevant);
            if (_relevantVragenGesteld == 0)
                lijnen.Add(new FeedbackLine("Vragen", "geen relevante vragen gesteld", FeedbackStatus.Onvoldoende));
            else if (_relevantVragenGesteld < totaalRelevant)
                lijnen.Add(new FeedbackLine("Vragen", $"{_relevantVragenGesteld} van {totaalRelevant} relevante vragen gesteld", FeedbackStatus.Gedeeltelijk));
            else
                lijnen.Add(new FeedbackLine("Vragen", "alle relevante vragen gesteld", FeedbackStatus.Goed));

            // Niet-relevante vragen = foutkeuze: kost tijd, minder gericht uitvragen.
            if (_irrelevanteVragenGesteld > 0)
                lijnen.Add(new FeedbackLine("Vraagkeuze",
                    $"{_irrelevanteVragenGesteld} niet-relevante vraag/vragen gesteld — vraag gerichter uit",
                    FeedbackStatus.Gedeeltelijk));

            lijnen.Add(Notebook.HasContent(SBARPart.Situation)
                ? new FeedbackLine("Situation", "volledig", FeedbackStatus.Goed)
                : new FeedbackLine("Situation", "geen situatie beschreven", FeedbackStatus.Onvoldoende));

            lijnen.Add(Notebook.HasContent(SBARPart.Background)
                ? new FeedbackLine("Background", "volledig", FeedbackStatus.Goed)
                : new FeedbackLine("Background", "voorgeschiedenis ontbreekt", FeedbackStatus.Onvoldoende));

            bool saturatieVermeld = false;
            foreach (var r in Notebook.Assessment)
                if (r.Contains("SpO2") || r.ToLower().Contains("saturat")) saturatieVermeld = true;
            int aantal = Notebook.Assessment.Count;
            if (aantal == 0)
                lijnen.Add(new FeedbackLine("Assessment", "geen metingen vastgelegd", FeedbackStatus.Onvoldoende));
            else if (!saturatieVermeld)
                lijnen.Add(new FeedbackLine("Assessment", "gedeeltelijk (saturatie niet vermeld)", FeedbackStatus.Gedeeltelijk));
            else
                lijnen.Add(new FeedbackLine("Assessment", "volledig", FeedbackStatus.Goed));

            bool passend = _gekozenAanbeveling >= 0 &&
                           dialogue.passendeAanbevelingen.Contains(_gekozenAanbeveling);
            if (_gekozenAanbeveling < 0)
                lijnen.Add(new FeedbackLine("Recommendation", "geen aanbeveling gekozen", FeedbackStatus.Onvoldoende));
            else if (passend)
                lijnen.Add(new FeedbackLine("Recommendation", "passend", FeedbackStatus.Goed));
            else
                lijnen.Add(new FeedbackLine("Recommendation", "niet passend bij deze verslechtering", FeedbackStatus.Gedeeltelijk));

            return lijnen;
        }

        private int TelApparaten()
        {
            int n = 0;
            if (saturatiemeter != null) n++;
            if (bloeddrukband != null) n++;
            if (thermometer != null) n++;
            return n;
        }

        private void SetDevicesInteractable(bool actief)
        {
            saturatiemeter?.SetInteractable(actief);
            bloeddrukband?.SetInteractable(actief);
            thermometer?.SetInteractable(actief);
        }

        private void SetInvestigatableObjectsActive(bool actief)
        {
            foreach (var obj in onderzoeksobjecten)
                obj?.SetInteractable(actief);
        }

        public void SetWorldInput(bool actief)
        {
            if (raycaster != null) raycaster.InputEnabled = actief;
            if (player != null) player.LookEnabled = true;
        }
    }
}

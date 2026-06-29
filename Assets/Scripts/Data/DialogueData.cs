using System;
using System.Collections.Generic;
using UnityEngine;

namespace SBAR.Core
{
    [Serializable]
    public class VitalReading
    {
        [Tooltip("Bijv. 'SpO2', 'Bloeddruk', 'Hartslag', 'Temperatuur'.")]
        public string label = "Waarde";

        [Tooltip("Bijv. '88%', '155/95', '102', '37,2 °C'.")]
        public string value = "";

        [Tooltip("Aanvinken als deze waarde afwijkend (rood + waarschuwing) is.")]
        public bool afwijkend = false;

        public string ToLine()
        {
            // OE-D3: afwijkend nooit alleen via kleur tonen; tekst-marker toevoegen.
            return afwijkend ? $"{label}: {value}  (!) afwijkend" : $"{label}: {value}";
        }
    }

    [Serializable]
    public class QuestionAnswer
    {
        [Tooltip("De vraag die de student kan stellen.")]
        public string question = "";

        [Tooltip("Het antwoord van de patiënt (ondertiteling).")]
        public string answer = "";

        [Tooltip("In welk SBAR-onderdeel het antwoord genoteerd wordt.")]
        public SBARPart targetPart = SBARPart.Situation;

        [Tooltip("De regel die in het notitieboekje belandt.")]
        public string notebookLine = "";

        [Tooltip("Aanvinken als dit een relevante vraag is voor de SBAR-overdracht (voor feedback).")]
        public bool isRelevant = true;
    }

    [CreateAssetMenu(fileName = "DialogueData", menuName = "SBAR/Dialoogdata", order = 0)]
    public class DialogueData : ScriptableObject
    {
        [Header("Patiënt & collega")]
        public string patientNaam = "Mw. De Groot";
        public string collegaNaam = "Collega";
        public string artsNaam = "Arts";

        [Header("Tijdslimiet (in kaart brengen)")]
        [Tooltip("Toon een aflopende klok tijdens het in kaart brengen van de SBAR-situatie.")]
        public bool tijdlimietActief = true;
        [Tooltip("Aantal seconden om de situatie in kaart te brengen (richtwaarde 120s = 2 min).")]
        public float tijdlimietSeconden = 120f;
        [Tooltip("Melding wanneer de tijd op is (niet-bestraffend; speler mag doorgaan).")]
        public string tijdOpMelding = "Tijd om — rond af en draag over aan de arts.";

        [Header("1. Briefing")]
        [TextArea] public string briefingKaart =
            "Mw. De Groot, 74 jaar, opgenomen na heupoperatie, bekend met hartfalen.";
        [TextArea] public string collegaBriefingLijn =
            "Kun jij even bij kamer 7 kijken? Mevrouw De Groot belde aan, ik kom zo snel mogelijk.";
        public string startKnopLabel = "Start";
        public List<string> baseBackground = new List<string>
        {
            "74 jaar, opgenomen na heupoperatie.",
            "Bekend met hartfalen (cardiale voorgeschiedenis)."
        };

        [Header("2. Binnenkomst")]
        [TextArea] public string patientBinnenkomstLijn =
            "Ik voel me niet goed, ik ben zo benauwd...";
        public string binnenkomstSituatie =
            "Patiënt is kortademig/benauwd bij binnenkomst.";

        [Header("3. Vragen")]
        public string vragenPrompt = "Wat wil je de patiënt vragen?";
        [Tooltip("Minimaal aantal vragen dat gesteld moet worden voordat 'Volgende stap' beschikbaar wordt.")]
        public int minVragenVoorVervolgStap = 3;
        public List<QuestionAnswer> vragen = new List<QuestionAnswer>
        {
            new QuestionAnswer {
                question = "Hoe lang heeft u dit al?",
                answer = "Al een half uur zo.",
                targetPart = SBARPart.Situation,
                notebookLine = "Klachten bestaan sinds ongeveer een half uur.",
                isRelevant = true
            },
            new QuestionAnswer {
                question = "Heeft u pijn op de borst?",
                answer = "Een beetje druk op mijn borst.",
                targetPart = SBARPart.Situation,
                notebookLine = "Patiënt ervaart druk op de borst.",
                isRelevant = true
            },
            new QuestionAnswer {
                question = "Heeft u medicatie ingenomen?",
                answer = "Mijn bloedverdunner heb ik vanmorgen genomen.",
                targetPart = SBARPart.Background,
                notebookLine = "Heeft vanmorgen bloedverdunner ingenomen.",
                isRelevant = true
            },
            new QuestionAnswer {
                question = "Heeft u eerder hartklachten gehad?",
                answer = "Ja, al een paar jaar hartfalen, maar zo erg als nu nooit.",
                targetPart = SBARPart.Background,
                notebookLine = "Eerder hartfalen; nooit eerder zo ernstig als nu.",
                isRelevant = true
            },
            new QuestionAnswer {
                question = "Voelt u pijn in uw arm of kaak?",
                answer = "Niet echt, maar ik heb wel wat druk hier op mijn borst.",
                targetPart = SBARPart.Situation,
                notebookLine = "Geen uitstralende pijn arm/kaak; wel thoracale druk.",
                isRelevant = true
            },
            new QuestionAnswer {
                question = "Heeft u familie die wij kunnen bellen?",
                answer = "Ja, mijn dochter. Haar nummer staat in het dossier.",
                targetPart = SBARPart.Situation,
                notebookLine = "",
                isRelevant = false
            },
            new QuestionAnswer {
                question = "Heeft u vandaag goed gegeten?",
                answer = "Niet zo veel, ik had geen honger.",
                targetPart = SBARPart.Situation,
                notebookLine = "",
                isRelevant = false
            },
            new QuestionAnswer {
                question = "Wilt u iets te drinken?",
                answer = "Nee, dank je, niet nu.",
                targetPart = SBARPart.Situation,
                notebookLine = "",
                isRelevant = false
            },
            new QuestionAnswer {
                question = "Heeft u vannacht goed geslapen?",
                answer = "Gaat wel, maar daar gaat het nu niet om hè.",
                targetPart = SBARPart.Situation,
                notebookLine = "",
                isRelevant = false
            }
        };

        [Header("4. Vitalen — apparaten en metingen")]
        public string vitalenInstructie =
            "Onderzoek de kamer. Meet vitalen en bekijk documenten. Druk op N voor notitieboekje.";
        public string saturatiemeterNaam = "Saturatiemeter";
        public List<VitalReading> saturatiemeterReadings = new List<VitalReading>
        {
            new VitalReading { label = "SpO2", value = "88%", afwijkend = true }
        };
        public string bloeddrukbandNaam = "Bloeddrukband";
        public List<VitalReading> bloeddrukbandReadings = new List<VitalReading>
        {
            new VitalReading { label = "Bloeddruk", value = "155/95 mmHg", afwijkend = true },
            new VitalReading { label = "Hartslag", value = "102/min", afwijkend = true }
        };
        public string thermometerNaam = "Thermometer";
        public List<VitalReading> thermometerReadings = new List<VitalReading>
        {
            new VitalReading { label = "Temperatuur", value = "37,2 °C", afwijkend = false }
        };

        [Header("5. Voorbereiden — aanbevelingen")]
        public string voorbereidenPrompt = "Welke aanbeveling doe je?";
        public List<int> passendeAanbevelingen = new List<int> { 0, 1 };
        public List<string> aanbevelingen = new List<string>
        {
            "Arts bellen voor direct consult",
            "Zuurstof toedienen en arts bellen",
            "Observeren en over 15 minuten terug melden."
        };

        [Header("6. Overdracht")]
        [TextArea] public string artsBegroeting =
            "Vertel het maar, wat is er met mevrouw De Groot aan de hand?";
        public string overdrachtInstructie =
            "Draag elk SBAR-onderdeel over aan de arts.";

        [Header("7. Feedback")]
        public string feedbackTitel = "Feedback op je SBAR-overdracht";
        public string herspeelLabel = "Herspeel";
        public string afsluitenLabel = "Afsluiten";

        [Header("HUD-instructies per fase (leeg = geen instructie getoond)")]
        public string instructieBriefing = "";
        public string instructieBinnenkomst = "";
        public string instructieVragen = "";
        public string instructieVoorbereiden = "";
        public string instructieOverdracht = "";
        public string instructieFeedback = "";

        public static DialogueData CreateDefault()
        {
            return CreateInstance<DialogueData>();
        }
    }
}

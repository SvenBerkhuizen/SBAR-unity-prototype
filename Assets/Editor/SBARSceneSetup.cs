using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;
using SBAR.Core;
using SBAR.Interaction;
using SBAR.UI;

public class SBARSceneSetup : EditorWindow
{
    [MenuItem("SBAR/Setup Scene")]
    public static void SetupScene()
    {
        // === PLAYER ===
        var player = GameObject.Find("Player");
        if (player == null) { Debug.LogError("Player object niet gevonden!"); return; }

        var cc = player.GetComponent<CharacterController>();
        if (cc == null) cc = player.AddComponent<CharacterController>();
        cc.center = new Vector3(0, 0, 0);
        cc.height = 1.8f;
        cc.radius = 0.3f;

        var pc = player.GetComponent<PlayerController>();
        if (pc == null) pc = player.AddComponent<PlayerController>();

        var playerCamera = GameObject.Find("PlayerCamera");
        if (playerCamera != null)
        {
            var so = new SerializedObject(pc);
            so.FindProperty("cameraPivot").objectReferenceValue = playerCamera.transform;
            so.ApplyModifiedProperties();
        }

        // === INTERACTION RAYCASTER op PlayerCamera ===
        if (playerCamera != null)
        {
            var ir = playerCamera.GetComponent<InteractionRaycaster>();
            if (ir == null) ir = playerCamera.AddComponent<InteractionRaycaster>();
            var so = new SerializedObject(ir);
            so.FindProperty("sourceCamera").objectReferenceValue = playerCamera.GetComponent<Camera>();
            so.FindProperty("maxDistance").floatValue = 4f;
            so.ApplyModifiedProperties();
        }

        // === MEETAPPARATEN ===
        SetupDevice("Saturatiemeter", "Saturatiemeter");
        SetupDevice("Bloeddrukband", "Bloeddrukband");
        SetupDevice("Thermometer", "Thermometer");

        // === NPC's ===
        SetupNPC("Collega", "Collega");
        SetupNPC("Arts", "Arts");

        // === HUD CONTROLLER ===
        var hudCanvas = GameObject.Find("HUD_Canvas");
        if (hudCanvas != null)
        {
            var hud = hudCanvas.GetComponent<HUDController>();
            if (hud == null) hud = hudCanvas.AddComponent<HUDController>();
            var so = new SerializedObject(hud);
            SetTMP(so, "instructionLabel", "InstructieTekst");
            SetGO(so, "briefingPanel", "BriefingPaneel");
            SetTMP(so, "briefingLabel", "BriefingTekst");
            SetButton(so, "nextButton", "VolgendeStapKnop");
            SetTMP(so, "nextButtonLabel", "VolgendeStapLabel");
            SetButton(so, "repeatButton", "HerhaalStapKnop");
            SetTMP(so, "controlsHint", "BesturingsHint");
            SetGO(so, "helpPanel", "HelpPaneel");
            so.ApplyModifiedProperties();
        }

        // === SUBTITLE SYSTEM ===
        var ondertitelPaneel = GameObject.Find("OndertitelPaneel");
        if (ondertitelPaneel != null)
        {
            var ss = ondertitelPaneel.GetComponent<SubtitleSystem>();
            if (ss == null) ss = ondertitelPaneel.AddComponent<SubtitleSystem>();
            var so = new SerializedObject(ss);
            so.FindProperty("panel").objectReferenceValue = ondertitelPaneel;
            SetTMP(so, "label", "OndertitelTekst");
            so.ApplyModifiedProperties();
        }

        // === NOTEBOOK UI ===
        var notitiePaneel = GameObject.Find("NotitiePaneel");
        if (notitiePaneel != null)
        {
            var nb = notitiePaneel.GetComponent<NotebookUI>();
            if (nb == null) nb = notitiePaneel.AddComponent<NotebookUI>();
            var so = new SerializedObject(nb);
            so.FindProperty("panel").objectReferenceValue = notitiePaneel;
            SetTMP(so, "contentLabel", "NotitieTekst");
            so.ApplyModifiedProperties();
        }

        // === CHOICE MENU ===
        var keuzeMenuPaneel = GameObject.Find("KeuzeMenuPaneel");
        if (keuzeMenuPaneel != null)
        {
            var cm = keuzeMenuPaneel.GetComponent<ChoiceMenu>();
            if (cm == null) cm = keuzeMenuPaneel.AddComponent<ChoiceMenu>();
            var so = new SerializedObject(cm);
            so.FindProperty("panel").objectReferenceValue = keuzeMenuPaneel;
            SetTMP(so, "promptLabel", "KeuzePrompt");

            var btnsProp = so.FindProperty("buttons");
            btnsProp.arraySize = 3;
            string[] btnNames = { "KeuzeKnop1", "KeuzeKnop2", "KeuzeKnop3" };
            for (int i = 0; i < 3; i++)
            {
                var go = GameObject.Find(btnNames[i]);
                if (go != null) btnsProp.GetArrayElementAtIndex(i).objectReferenceValue = go.GetComponent<Button>();
            }

            var lblsProp = so.FindProperty("buttonLabels");
            lblsProp.arraySize = 3;
            for (int i = 0; i < 3; i++)
            {
                var go = GameObject.Find(btnNames[i]);
                if (go != null)
                {
                    var lbl = go.GetComponentInChildren<TMP_Text>();
                    lblsProp.GetArrayElementAtIndex(i).objectReferenceValue = lbl;
                }
            }
            so.ApplyModifiedProperties();
        }

        // === OVERDRACHT PANEL ===
        var overdrachtPaneel = GameObject.Find("OverdrachtPaneel");
        if (overdrachtPaneel != null)
        {
            var op = overdrachtPaneel.GetComponent<OverdrachtPanel>();
            if (op == null) op = overdrachtPaneel.AddComponent<OverdrachtPanel>();
            var so = new SerializedObject(op);
            so.FindProperty("panel").objectReferenceValue = overdrachtPaneel;

            var btnsProp = so.FindProperty("partButtons");
            btnsProp.arraySize = 4;
            string[] geefNames = { "GeefS", "GeefB", "GeefA", "GeefR" };
            for (int i = 0; i < 4; i++)
            {
                var go = GameObject.Find(geefNames[i]);
                if (go != null) btnsProp.GetArrayElementAtIndex(i).objectReferenceValue = go.GetComponent<Button>();
            }

            var statusProp = so.FindProperty("statusLabels");
            statusProp.arraySize = 4;
            string[] statusNames = { "StatusS", "StatusB", "StatusA", "StatusR" };
            for (int i = 0; i < 4; i++)
            {
                var go = GameObject.Find(statusNames[i]);
                if (go != null) statusProp.GetArrayElementAtIndex(i).objectReferenceValue = go.GetComponent<TMP_Text>();
            }
            so.ApplyModifiedProperties();
        }

        // === FEEDBACK SCREEN ===
        var feedbackPaneel = GameObject.Find("FeedbackPaneel");
        if (feedbackPaneel != null)
        {
            var fs = feedbackPaneel.GetComponent<FeedbackScreen>();
            if (fs == null) fs = feedbackPaneel.AddComponent<FeedbackScreen>();
            var so = new SerializedObject(fs);
            so.FindProperty("panel").objectReferenceValue = feedbackPaneel;
            SetTMP(so, "titleLabel", "FeedbackTitel");
            SetTMP(so, "contentLabel", "FeedbackInhoud");
            SetButton(so, "replayButton", "HerspeelKnop");
            SetButton(so, "quitButton", "AfsluitenKnop");

            var herspeelKnop = GameObject.Find("HerspeelKnop");
            if (herspeelKnop != null)
                so.FindProperty("replayLabel").objectReferenceValue = herspeelKnop.GetComponentInChildren<TMP_Text>();
            var afsluitenKnop = GameObject.Find("AfsluitenKnop");
            if (afsluitenKnop != null)
                so.FindProperty("quitLabel").objectReferenceValue = afsluitenKnop.GetComponentInChildren<TMP_Text>();

            so.ApplyModifiedProperties();
        }

        // === SBAR MANAGER ===
        var sbarManager = GameObject.Find("SBARManager");
        if (sbarManager != null)
        {
            var mgr = sbarManager.GetComponent<SBARManager>();
            if (mgr == null) mgr = sbarManager.AddComponent<SBARManager>();
            var so = new SerializedObject(mgr);

            if (playerCamera != null)
                so.FindProperty("raycaster").objectReferenceValue = playerCamera.GetComponent<InteractionRaycaster>();
            so.FindProperty("player").objectReferenceValue = player.GetComponent<PlayerController>();

            if (hudCanvas != null)
                so.FindProperty("hud").objectReferenceValue = hudCanvas.GetComponent<HUDController>();
            if (ondertitelPaneel != null)
                so.FindProperty("subtitles").objectReferenceValue = ondertitelPaneel.GetComponent<SubtitleSystem>();
            if (notitiePaneel != null)
                so.FindProperty("notebookUI").objectReferenceValue = notitiePaneel.GetComponent<NotebookUI>();
            if (keuzeMenuPaneel != null)
                so.FindProperty("choiceMenu").objectReferenceValue = keuzeMenuPaneel.GetComponent<ChoiceMenu>();
            if (overdrachtPaneel != null)
                so.FindProperty("overdrachtPanel").objectReferenceValue = overdrachtPaneel.GetComponent<OverdrachtPanel>();
            if (feedbackPaneel != null)
                so.FindProperty("feedbackScreen").objectReferenceValue = feedbackPaneel.GetComponent<FeedbackScreen>();

            SetDevice(so, "saturatiemeter", "Saturatiemeter");
            SetDevice(so, "bloeddrukband", "Bloeddrukband");
            SetDevice(so, "thermometer", "Thermometer");
            SetNPC(so, "collega", "Collega");
            SetNPC(so, "arts", "Arts");

            so.ApplyModifiedProperties();
        }

        EditorUtility.SetDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects()[0]);
        UnityEditor.SceneManagement.EditorSceneManager.MarkAllScenesDirty();
        Debug.Log("SBAR Scene Setup compleet!");
    }

    private static void SetupDevice(string goName, string deviceName)
    {
        var go = GameObject.Find(goName);
        if (go == null) { Debug.LogWarning($"{goName} niet gevonden"); return; }
        var md = go.GetComponent<MeasurementDevice>();
        if (md == null) md = go.AddComponent<MeasurementDevice>();
        var so = new SerializedObject(md);
        so.FindProperty("deviceNaam").stringValue = deviceName;
        var label = go.GetComponentInChildren<TMP_Text>();
        if (label != null) so.FindProperty("waardeLabel").objectReferenceValue = label;
        so.ApplyModifiedProperties();
    }

    private static void SetupNPC(string goName, string npcName)
    {
        var go = GameObject.Find(goName);
        if (go == null) { Debug.LogWarning($"{goName} niet gevonden"); return; }
        var npc = go.GetComponent<NPCInteractable>();
        if (npc == null) npc = go.AddComponent<NPCInteractable>();
        var so = new SerializedObject(npc);
        so.FindProperty("personageNaam").stringValue = npcName;
        so.ApplyModifiedProperties();
    }

    private static void SetTMP(SerializedObject so, string field, string goName)
    {
        var go = GameObject.Find(goName);
        if (go != null) so.FindProperty(field).objectReferenceValue = go.GetComponent<TMP_Text>();
    }

    private static void SetGO(SerializedObject so, string field, string goName)
    {
        var go = GameObject.Find(goName);
        if (go != null) so.FindProperty(field).objectReferenceValue = go;
    }

    private static void SetButton(SerializedObject so, string field, string goName)
    {
        var go = GameObject.Find(goName);
        if (go != null) so.FindProperty(field).objectReferenceValue = go.GetComponent<Button>();
    }

    private static void SetDevice(SerializedObject so, string field, string goName)
    {
        var go = GameObject.Find(goName);
        if (go != null) so.FindProperty(field).objectReferenceValue = go.GetComponent<MeasurementDevice>();
    }

    private static void SetNPC(SerializedObject so, string field, string goName)
    {
        var go = GameObject.Find(goName);
        if (go != null) so.FindProperty(field).objectReferenceValue = go.GetComponent<NPCInteractable>();
    }
}

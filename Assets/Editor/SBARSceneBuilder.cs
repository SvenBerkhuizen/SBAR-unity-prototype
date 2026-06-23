using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using SBAR.Core;
using SBAR.Interaction;
using SBAR.UI;

public class SBARSceneBuilder : EditorWindow
{
    [MenuItem("SBAR/Build Entire Scene")]
    public static void BuildScene()
    {
        // Verwijder oude SBAR-objecten zodat het script veilig herhaalbaar is
        DestroyIfExists("Kamer");
        DestroyIfExists("Bed");
        DestroyIfExists("Apparatentafel");
        DestroyIfExists("DeurZone");
        DestroyIfExists("Player");
        DestroyIfExists("HUD_Canvas");
        DestroyIfExists("WorldCanvas");
        DestroyIfExists("SBARManager");
        DestroyIfExists("EventSystem");

        // ── MATERIALEN ────────────────────────────────────────────────
        var matVloer    = GetOrCreateMaterial("Mat_Vloer",    new Color(0.847f, 0.824f, 0.780f));
        var matMuur     = GetOrCreateMaterial("Mat_Muur",     new Color(0.788f, 0.824f, 0.839f));
        var matPlafond  = GetOrCreateMaterial("Mat_Plafond",  new Color(0.925f, 0.925f, 0.925f));
        var matMeubel   = GetOrCreateMaterial("Mat_Meubel",   new Color(0.749f, 0.659f, 0.545f));
        var matApparaat = GetOrCreateMaterial("Mat_Apparaat", new Color(0.604f, 0.627f, 0.651f));
        var matPatient  = GetOrCreateMaterial("Mat_Patient",  new Color(0.851f, 0.761f, 0.690f));
        var matDeur     = GetOrCreateMaterial("Mat_Deur",     new Color(0.663f, 0.718f, 0.753f));

        // ── 4.1 KAMER ─────────────────────────────────────────────────
        var kamer = new GameObject("Kamer");
        CreatePrimitive(kamer, "Vloer",   PrimitiveType.Plane,  new Vector3(0,0,0),      Vector3.zero,        new Vector3(1.2f,1,1.2f), matVloer);
        CreatePrimitive(kamer, "Plafond", PrimitiveType.Plane,  new Vector3(0,3,0),      new Vector3(180,0,0),new Vector3(1.2f,1,1.2f), matPlafond);
        CreatePrimitive(kamer, "Muur_N",  PrimitiveType.Cube,   new Vector3(0,1.5f,6),   Vector3.zero,        new Vector3(12,3,0.2f),   matMuur);
        CreatePrimitive(kamer, "Muur_Z",  PrimitiveType.Cube,   new Vector3(0,1.5f,-6),  Vector3.zero,        new Vector3(12,3,0.2f),   matMuur);
        CreatePrimitive(kamer, "Muur_O",  PrimitiveType.Cube,   new Vector3(6,1.5f,0),   new Vector3(0,90,0), new Vector3(12,3,0.2f),   matMuur);
        CreatePrimitive(kamer, "Muur_W",  PrimitiveType.Cube,   new Vector3(-6,1.5f,0),  new Vector3(0,90,0), new Vector3(12,3,0.2f),   matMuur);

        // ── 4.2 BED ───────────────────────────────────────────────────
        var bed = new GameObject("Bed");
        CreatePrimitive(bed, "Matras",         PrimitiveType.Cube,    new Vector3(-2.5f,0.5f,2), Vector3.zero, new Vector3(1,0.5f,2.2f),   matMeubel);
        CreatePrimitive(bed, "Patient_DeGroot",PrimitiveType.Capsule, new Vector3(-2.5f,0.9f,2), new Vector3(90,0,0), new Vector3(0.8f,0.8f,0.8f), matPatient);

        // ── 4.3 APPARATENTAFEL ────────────────────────────────────────
        var tafel = new GameObject("Apparatentafel");
        CreatePrimitive(tafel, "Tafelblad", PrimitiveType.Cube, new Vector3(-0.5f,0.9f,2), Vector3.zero, new Vector3(1.2f,0.1f,0.6f), matMeubel);

        var sat  = CreateDevice(tafel, "Saturatiemeter", new Vector3(-0.85f,1.1f,2), matApparaat);
        var bloed = CreateDevice(tafel, "Bloeddrukband", new Vector3(-0.5f, 1.1f,2), matApparaat);
        var therm = CreateDevice(tafel, "Thermometer",   new Vector3(-0.15f,1.1f,2), matApparaat);

        // ── 4.4 DEURZONE ──────────────────────────────────────────────
        var deurzone = new GameObject("DeurZone");
        CreatePrimitive(deurzone, "Deur",   PrimitiveType.Cube,    new Vector3(0,1.05f,-5.8f), Vector3.zero, new Vector3(1,2.1f,0.1f), matDeur);
        var collegaGO = CreatePrimitive(deurzone, "Collega", PrimitiveType.Capsule, new Vector3(1,1,-5), Vector3.zero, Vector3.one, matPatient);
        var artsGO    = CreatePrimitive(deurzone, "Arts",    PrimitiveType.Capsule, new Vector3(2,1,-5), Vector3.zero, Vector3.one, matPatient);

        // ── 4.5 PLAYER ────────────────────────────────────────────────
        var playerGO = new GameObject("Player");
        playerGO.transform.position = new Vector3(2,1,0);

        var camGO = new GameObject("PlayerCamera");
        camGO.transform.SetParent(playerGO.transform);
        camGO.transform.localPosition = new Vector3(0,0.7f,0);
        var cam = camGO.AddComponent<Camera>();
        camGO.tag = "MainCamera";

        // Verwijder de oude Main Camera uit de scene
        var oldCam = GameObject.FindWithTag("MainCamera");
        if (oldCam != null && oldCam != camGO) Object.DestroyImmediate(oldCam);

        var cc = playerGO.AddComponent<CharacterController>();
        cc.height = 1.8f; cc.radius = 0.3f;

        var pc = playerGO.AddComponent<PlayerController>();
        var pcSO = new SerializedObject(pc);
        pcSO.FindProperty("cameraPivot").objectReferenceValue = camGO.transform;
        pcSO.ApplyModifiedProperties();

        var ir = camGO.AddComponent<InteractionRaycaster>();
        var irSO = new SerializedObject(ir);
        irSO.FindProperty("sourceCamera").objectReferenceValue = cam;
        irSO.FindProperty("maxDistance").floatValue = 4f;
        irSO.ApplyModifiedProperties();

        // ── COMPONENTEN OP MEETAPPARATEN ─────────────────────────────
        var satComp   = AddDevice(sat,   "Saturatiemeter");
        var bloedComp = AddDevice(bloed, "Bloeddrukband");
        var thermComp = AddDevice(therm, "Thermometer");

        // ── COMPONENTEN OP NPC's ──────────────────────────────────────
        var collegaComp = collegaGO.AddComponent<NPCInteractable>();
        var collegaSO = new SerializedObject(collegaComp);
        collegaSO.FindProperty("personageNaam").stringValue = "Collega";
        collegaSO.ApplyModifiedProperties();

        var artsComp = artsGO.AddComponent<NPCInteractable>();
        var artsSO = new SerializedObject(artsComp);
        artsSO.FindProperty("personageNaam").stringValue = "Arts";
        artsSO.ApplyModifiedProperties();

        // ── 4.7 WORLD CANVAS ──────────────────────────────────────────
        var worldCanvas = new GameObject("WorldCanvas");
        var wc = worldCanvas.AddComponent<Canvas>();
        wc.renderMode = RenderMode.WorldSpace;
        worldCanvas.transform.position = new Vector3(-2.5f,1.8f,2);
        worldCanvas.transform.localScale = new Vector3(0.01f,0.01f,0.01f);
        worldCanvas.AddComponent<CanvasScaler>();
        worldCanvas.AddComponent<GraphicRaycaster>();
        var wcText = CreateTMPText(worldCanvas, "WorldLabel", "Mw. De Groot — Kamer 7", new Vector2(0,0), new Vector2(400,80));

        // ── 4.6 HUD CANVAS ────────────────────────────────────────────
        var hudGO = new GameObject("HUD_Canvas");
        var hudCanvas2 = hudGO.AddComponent<Canvas>();
        hudCanvas2.renderMode = RenderMode.ScreenSpaceOverlay;
        hudGO.AddComponent<CanvasScaler>();
        hudGO.AddComponent<GraphicRaycaster>();

        // EventSystem
        var es = new GameObject("EventSystem");
        es.AddComponent<EventSystem>();
        es.AddComponent<StandaloneInputModule>();

        // UI-elementen aanmaken
        var instructieTekst   = CreateTMPText(hudGO, "InstructieTekst",  "Instructie...", new Vector2(0,180),   new Vector2(700,50));
        var besturingsHint    = CreateTMPText(hudGO, "BesturingsHint",   "",              new Vector2(0,-250),  new Vector2(900,30));
        var helpPaneel        = CreatePanel(hudGO,  "HelpPaneel",        new Vector2(0,0), new Vector2(600,300));
        var briefingPaneel    = CreatePanel(hudGO,  "BriefingPaneel",    new Vector2(0,50),new Vector2(500,200));
        var briefingTekst     = CreateTMPText(briefingPaneel, "BriefingTekst", "", new Vector2(0,0), new Vector2(460,160));

        var volgendeKnop      = CreateButton(hudGO, "VolgendeStapKnop",  "Start",         new Vector2(250,-200),new Vector2(200,50));
        var herhaalKnop       = CreateButton(hudGO, "HerhaalStapKnop",   "Herhaal stap",  new Vector2(0,-200),  new Vector2(180,50));
        var volgendeLabel     = volgendeKnop.GetComponentInChildren<TMP_Text>();

        var ondertitelPaneel  = CreatePanel(hudGO,  "OndertitelPaneel",  new Vector2(0,-180),new Vector2(700,60));
        var ondertitelTekst   = CreateTMPText(ondertitelPaneel, "OndertitelTekst", "", new Vector2(0,0), new Vector2(680,50));

        var notitiePaneel     = CreatePanel(hudGO,  "NotitiePaneel",     new Vector2(350,0), new Vector2(320,400));
        var notitieTekst      = CreateTMPText(notitiePaneel, "NotitieTekst", "", new Vector2(0,0), new Vector2(300,380));

        var keuzeMenuPaneel   = CreatePanel(hudGO,  "KeuzeMenuPaneel",   new Vector2(0,-30), new Vector2(520,390));
        var keuzePrompt       = CreateTMPText(keuzeMenuPaneel, "KeuzePrompt", "Kies:", new Vector2(0,170), new Vector2(480,36));
        var keuzeKnop1        = CreateButton(keuzeMenuPaneel, "KeuzeKnop1", "Vraag 1", new Vector2(0,127),  new Vector2(480,36));
        var keuzeKnop2        = CreateButton(keuzeMenuPaneel, "KeuzeKnop2", "Vraag 2", new Vector2(0, 83),  new Vector2(480,36));
        var keuzeKnop3        = CreateButton(keuzeMenuPaneel, "KeuzeKnop3", "Vraag 3", new Vector2(0, 39),  new Vector2(480,36));
        var keuzeKnop4        = CreateButton(keuzeMenuPaneel, "KeuzeKnop4", "Vraag 4", new Vector2(0, -5),  new Vector2(480,36));
        var keuzeKnop5        = CreateButton(keuzeMenuPaneel, "KeuzeKnop5", "Vraag 5", new Vector2(0,-49),  new Vector2(480,36));
        var keuzeKnop6        = CreateButton(keuzeMenuPaneel, "KeuzeKnop6", "Vraag 6", new Vector2(0,-93),  new Vector2(480,36));
        var keuzeKnop7        = CreateButton(keuzeMenuPaneel, "KeuzeKnop7", "Vraag 7", new Vector2(0,-137), new Vector2(480,36));

        var overdrachtPaneel  = CreatePanel(hudGO,  "OverdrachtPaneel",  new Vector2(-200,0),new Vector2(360,320));
        var geefS  = CreateButton(overdrachtPaneel, "GeefS", "Geef Situation",     new Vector2(0,110), new Vector2(320,40));
        var geefB  = CreateButton(overdrachtPaneel, "GeefB", "Geef Background",    new Vector2(0,60),  new Vector2(320,40));
        var geefA  = CreateButton(overdrachtPaneel, "GeefA", "Geef Assessment",    new Vector2(0,10),  new Vector2(320,40));
        var geefR  = CreateButton(overdrachtPaneel, "GeefR", "Geef Recommendation",new Vector2(0,-40), new Vector2(320,40));
        var statusS = CreateTMPText(overdrachtPaneel, "StatusS", "? Situation",    new Vector2(0, 85), new Vector2(320,22));
        var statusB = CreateTMPText(overdrachtPaneel, "StatusB", "? Background",   new Vector2(0, 35), new Vector2(320,22));
        var statusA = CreateTMPText(overdrachtPaneel, "StatusA", "? Assessment",   new Vector2(0,-15), new Vector2(320,22));
        var statusR = CreateTMPText(overdrachtPaneel, "StatusR", "? Recommendation",new Vector2(0,-65),new Vector2(320,22));

        var feedbackPaneel    = CreatePanel(hudGO,  "FeedbackPaneel",    new Vector2(0,0),   new Vector2(600,400));
        var feedbackTitel     = CreateTMPText(feedbackPaneel, "FeedbackTitel",   "Feedback", new Vector2(0,160), new Vector2(560,50));
        var feedbackInhoud    = CreateTMPText(feedbackPaneel, "FeedbackInhoud",  "",         new Vector2(0,30),  new Vector2(560,200));
        var herspeelKnop      = CreateButton(feedbackPaneel,  "HerspeelKnop",   "Herspeel", new Vector2(-100,-150), new Vector2(160,45));
        var afsluitenKnop     = CreateButton(feedbackPaneel,  "AfsluitenKnop",  "Afsluiten",new Vector2(100,-150),  new Vector2(160,45));

        // ── HUD CONTROLLER ────────────────────────────────────────────
        var hud = hudGO.AddComponent<HUDController>();
        var hudSO = new SerializedObject(hud);
        hudSO.FindProperty("instructionLabel").objectReferenceValue  = instructieTekst;
        hudSO.FindProperty("briefingPanel").objectReferenceValue     = briefingPaneel;
        hudSO.FindProperty("briefingLabel").objectReferenceValue     = briefingTekst;
        hudSO.FindProperty("nextButton").objectReferenceValue        = volgendeKnop;
        hudSO.FindProperty("nextButtonLabel").objectReferenceValue   = volgendeLabel;
        hudSO.FindProperty("repeatButton").objectReferenceValue      = herhaalKnop;
        hudSO.FindProperty("controlsHint").objectReferenceValue      = besturingsHint;
        hudSO.FindProperty("helpPanel").objectReferenceValue         = helpPaneel;
        hudSO.ApplyModifiedProperties();

        // ── SUBTITLE SYSTEM ───────────────────────────────────────────
        var ss = ondertitelPaneel.AddComponent<SubtitleSystem>();
        var ssSO = new SerializedObject(ss);
        ssSO.FindProperty("panel").objectReferenceValue  = ondertitelPaneel;
        ssSO.FindProperty("label").objectReferenceValue  = ondertitelTekst;
        ssSO.ApplyModifiedProperties();

        // ── NOTEBOOK UI ───────────────────────────────────────────────
        var nb = notitiePaneel.AddComponent<NotebookUI>();
        var nbSO = new SerializedObject(nb);
        nbSO.FindProperty("panel").objectReferenceValue        = notitiePaneel;
        nbSO.FindProperty("contentLabel").objectReferenceValue = notitieTekst;
        nbSO.ApplyModifiedProperties();

        // ── CHOICE MENU ───────────────────────────────────────────────
        var cm = keuzeMenuPaneel.AddComponent<ChoiceMenu>();
        var cmSO = new SerializedObject(cm);
        cmSO.FindProperty("panel").objectReferenceValue       = keuzeMenuPaneel;
        cmSO.FindProperty("promptLabel").objectReferenceValue = keuzePrompt;
        var btnsProp = cmSO.FindProperty("buttons");
        btnsProp.arraySize = 7;
        btnsProp.GetArrayElementAtIndex(0).objectReferenceValue = keuzeKnop1;
        btnsProp.GetArrayElementAtIndex(1).objectReferenceValue = keuzeKnop2;
        btnsProp.GetArrayElementAtIndex(2).objectReferenceValue = keuzeKnop3;
        btnsProp.GetArrayElementAtIndex(3).objectReferenceValue = keuzeKnop4;
        btnsProp.GetArrayElementAtIndex(4).objectReferenceValue = keuzeKnop5;
        btnsProp.GetArrayElementAtIndex(5).objectReferenceValue = keuzeKnop6;
        btnsProp.GetArrayElementAtIndex(6).objectReferenceValue = keuzeKnop7;
        var lblsProp = cmSO.FindProperty("buttonLabels");
        lblsProp.arraySize = 7;
        lblsProp.GetArrayElementAtIndex(0).objectReferenceValue = keuzeKnop1.GetComponentInChildren<TMP_Text>();
        lblsProp.GetArrayElementAtIndex(1).objectReferenceValue = keuzeKnop2.GetComponentInChildren<TMP_Text>();
        lblsProp.GetArrayElementAtIndex(2).objectReferenceValue = keuzeKnop3.GetComponentInChildren<TMP_Text>();
        lblsProp.GetArrayElementAtIndex(3).objectReferenceValue = keuzeKnop4.GetComponentInChildren<TMP_Text>();
        lblsProp.GetArrayElementAtIndex(4).objectReferenceValue = keuzeKnop5.GetComponentInChildren<TMP_Text>();
        lblsProp.GetArrayElementAtIndex(5).objectReferenceValue = keuzeKnop6.GetComponentInChildren<TMP_Text>();
        lblsProp.GetArrayElementAtIndex(6).objectReferenceValue = keuzeKnop7.GetComponentInChildren<TMP_Text>();
        cmSO.ApplyModifiedProperties();

        // ── OVERDRACHT PANEL ──────────────────────────────────────────
        var op = overdrachtPaneel.AddComponent<OverdrachtPanel>();
        var opSO = new SerializedObject(op);
        opSO.FindProperty("panel").objectReferenceValue = overdrachtPaneel;
        var partBtns = opSO.FindProperty("partButtons");
        partBtns.arraySize = 4;
        partBtns.GetArrayElementAtIndex(0).objectReferenceValue = geefS;
        partBtns.GetArrayElementAtIndex(1).objectReferenceValue = geefB;
        partBtns.GetArrayElementAtIndex(2).objectReferenceValue = geefA;
        partBtns.GetArrayElementAtIndex(3).objectReferenceValue = geefR;
        var statusLbls = opSO.FindProperty("statusLabels");
        statusLbls.arraySize = 4;
        statusLbls.GetArrayElementAtIndex(0).objectReferenceValue = statusS;
        statusLbls.GetArrayElementAtIndex(1).objectReferenceValue = statusB;
        statusLbls.GetArrayElementAtIndex(2).objectReferenceValue = statusA;
        statusLbls.GetArrayElementAtIndex(3).objectReferenceValue = statusR;
        opSO.ApplyModifiedProperties();

        // ── FEEDBACK SCREEN ───────────────────────────────────────────
        var fs = feedbackPaneel.AddComponent<FeedbackScreen>();
        var fsSO = new SerializedObject(fs);
        fsSO.FindProperty("panel").objectReferenceValue        = feedbackPaneel;
        fsSO.FindProperty("titleLabel").objectReferenceValue   = feedbackTitel;
        fsSO.FindProperty("contentLabel").objectReferenceValue = feedbackInhoud;
        fsSO.FindProperty("replayButton").objectReferenceValue = herspeelKnop;
        fsSO.FindProperty("replayLabel").objectReferenceValue  = herspeelKnop.GetComponentInChildren<TMP_Text>();
        fsSO.FindProperty("quitButton").objectReferenceValue   = afsluitenKnop;
        fsSO.FindProperty("quitLabel").objectReferenceValue    = afsluitenKnop.GetComponentInChildren<TMP_Text>();
        fsSO.ApplyModifiedProperties();

        // ── SBAR MANAGER ──────────────────────────────────────────────
        var mgrGO = new GameObject("SBARManager");
        var mgr = mgrGO.AddComponent<SBARManager>();
        var mgrSO = new SerializedObject(mgr);
        mgrSO.FindProperty("raycaster").objectReferenceValue     = ir;
        mgrSO.FindProperty("player").objectReferenceValue        = pc;
        mgrSO.FindProperty("hud").objectReferenceValue           = hud;
        mgrSO.FindProperty("subtitles").objectReferenceValue     = ss;
        mgrSO.FindProperty("notebookUI").objectReferenceValue    = nb;
        mgrSO.FindProperty("choiceMenu").objectReferenceValue    = cm;
        mgrSO.FindProperty("overdrachtPanel").objectReferenceValue = op;
        mgrSO.FindProperty("feedbackScreen").objectReferenceValue  = fs;
        mgrSO.FindProperty("saturatiemeter").objectReferenceValue  = satComp;
        mgrSO.FindProperty("bloeddrukband").objectReferenceValue   = bloedComp;
        mgrSO.FindProperty("thermometer").objectReferenceValue     = thermComp;
        mgrSO.FindProperty("collega").objectReferenceValue         = collegaComp;
        mgrSO.FindProperty("arts").objectReferenceValue            = artsComp;
        mgrSO.ApplyModifiedProperties();

        UnityEditor.SceneManagement.EditorSceneManager.MarkAllScenesDirty();
        Debug.Log("✓ SBAR scene volledig gebouwd en gekoppeld!");
    }

    // ── HELPERS ──────────────────────────────────────────────────────

    private static Material GetOrCreateMaterial(string name, Color color)
    {
        string path = $"Assets/Materials/{name}.mat";
        var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (mat == null)
        {
            System.IO.Directory.CreateDirectory("Assets/Materials");
            mat = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
            mat.name = name;
            AssetDatabase.CreateAsset(mat, path);
        }
        if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", color);
        else mat.color = color;
        EditorUtility.SetDirty(mat);
        return mat;
    }

    private static GameObject CreatePrimitive(GameObject parent, string name, PrimitiveType type,
        Vector3 pos, Vector3 rot, Vector3 scale, Material mat)
    {
        var go = GameObject.CreatePrimitive(type);
        go.name = name;
        go.transform.SetParent(parent.transform);
        go.transform.position = pos;
        go.transform.eulerAngles = rot;
        go.transform.localScale = scale;
        if (mat != null) go.GetComponent<Renderer>().sharedMaterial = mat;
        return go;
    }

    private static GameObject CreateDevice(GameObject parent, string name, Vector3 pos, Material mat)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = name;
        go.transform.SetParent(parent.transform);
        go.transform.position = pos;
        go.transform.localScale = new Vector3(0.25f, 0.25f, 0.25f);
        if (mat != null) go.GetComponent<Renderer>().sharedMaterial = mat;

        // 3D tekstlabel als kind
        var labelGO = new GameObject("Label");
        labelGO.transform.SetParent(go.transform);
        labelGO.transform.localPosition = new Vector3(0, 2.5f, 0);
        labelGO.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
        var tmp = labelGO.AddComponent<TextMeshPro>();
        tmp.text = name + "\n[E / klik]";
        tmp.fontSize = 14;
        tmp.alignment = TextAlignmentOptions.Center;

        return go;
    }

    private static MeasurementDevice AddDevice(GameObject go, string naam)
    {
        var md = go.AddComponent<MeasurementDevice>();
        var so = new SerializedObject(md);
        so.FindProperty("deviceNaam").stringValue = naam;
        var label = go.GetComponentInChildren<TextMeshPro>();
        if (label != null) so.FindProperty("waardeLabel").objectReferenceValue = label;
        so.ApplyModifiedProperties();
        return md;
    }

    private static GameObject CreatePanel(GameObject parent, string name, Vector2 anchoredPos, Vector2 size)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent.transform, false);
        var img = go.AddComponent<Image>();
        img.color = new Color(0, 0, 0, 0.7f);
        var rt = go.GetComponent<RectTransform>();
        rt.anchoredPosition = anchoredPos;
        rt.sizeDelta = size;
        return go;
    }

    private static TMP_Text CreateTMPText(GameObject parent, string name, string text, Vector2 anchoredPos, Vector2 size)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent.transform, false);
        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = 16;
        tmp.color = Color.white;
        tmp.alignment = TextAlignmentOptions.Center;
        var rt = go.GetComponent<RectTransform>();
        rt.anchoredPosition = anchoredPos;
        rt.sizeDelta = size;
        return tmp;
    }

    private static Button CreateButton(GameObject parent, string name, string label, Vector2 anchoredPos, Vector2 size)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent.transform, false);
        var img = go.AddComponent<Image>();
        img.color = new Color(0.2f, 0.5f, 0.8f, 1f);
        var btn = go.AddComponent<Button>();
        var rt = go.GetComponent<RectTransform>();
        rt.anchoredPosition = anchoredPos;
        rt.sizeDelta = size;

        var lblGO = new GameObject("Label");
        lblGO.transform.SetParent(go.transform, false);
        var tmp = lblGO.AddComponent<TextMeshProUGUI>();
        tmp.text = label;
        tmp.fontSize = 14;
        tmp.color = Color.white;
        tmp.alignment = TextAlignmentOptions.Center;
        var lblRT = lblGO.GetComponent<RectTransform>();
        lblRT.anchorMin = Vector2.zero;
        lblRT.anchorMax = Vector2.one;
        lblRT.offsetMin = Vector2.zero;
        lblRT.offsetMax = Vector2.zero;

        return btn;
    }

    private static void DestroyIfExists(string name)
    {
        var go = GameObject.Find(name);
        if (go != null) Object.DestroyImmediate(go);
    }
}

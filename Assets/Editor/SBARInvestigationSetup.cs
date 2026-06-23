using UnityEngine;
using UnityEditor;
using TMPro;
using SBAR.Core;
using SBAR.Interaction;

public static class SBARInvestigationSetup
{
    private struct ObjDef
    {
        public string   id;
        public int      cat;       // SBARPart enum int
        public string   finding;
        public bool     critical;
        public string   label;
        public Vector3  pos;
    }

    [MenuItem("SBAR/Setup Onderzoeksobjecten")]
    static void Setup()
    {
        SBARManager manager = Object.FindObjectOfType<SBARManager>();
        if (manager == null)
        {
            Debug.LogError("[SBAR] SBARManager niet gevonden in scène. Open SBAR_Testscene eerst.");
            return;
        }

        // --- InvestigationTracker ---
        InvestigationTracker tracker = Object.FindObjectOfType<InvestigationTracker>();
        if (tracker == null)
        {
            var go = new GameObject("InvestigationTracker");
            tracker = go.AddComponent<InvestigationTracker>();
            Undo.RegisterCreatedObjectUndo(go, "Create InvestigationTracker");
            Debug.Log("[SBAR] InvestigationTracker aangemaakt.");
        }

        // --- Oude onderzoeksobjecten opruimen (idempotent re-run) ---
        foreach (var oud in Object.FindObjectsOfType<InvestigatableObject>())
            Undo.DestroyObjectImmediate(oud.gameObject);

        // --- Objectdefinities ---
        // cat: Situation=0, Background=1, Assessment=2
        var defs = new ObjDef[]
        {
            new ObjDef { id = "dossier",         cat = 1, finding = "Bekend met hartfalen, diureticum",              critical = true,  label = "Patiëntdossier",  pos = new Vector3(-2.5f, 1.05f, 0.6f) },
            new ObjDef { id = "medicatielijst",   cat = 1, finding = "Furosemide 40mg, laatste dosis vanmorgen",     critical = false, label = "Medicatielijst",   pos = new Vector3(-1.9f, 1.05f, 0.6f) },
            new ObjDef { id = "zuurstofmeter",    cat = 2, finding = "Kamerlucht, geen O₂ actief",             critical = false, label = "Zuurstofmeter",    pos = new Vector3(-3.3f, 0.45f, 3.2f) },
            new ObjDef { id = "infuuszak",        cat = 0, finding = "NaCl 0,9%, loopsnelheid hoog",                critical = false, label = "Infuuszak",        pos = new Vector3(-1.9f, 1.70f, 3.4f) },
            new ObjDef { id = "verpleegdagboek",  cat = 1, finding = "Gisteren ook benauwd, niet gerapporteerd",    critical = true,  label = "Verpleegdagboek",  pos = new Vector3(-1.9f, 1.05f, 3.0f) },
        };

        // --- Maak objecten + koppel ---
        SerializedObject managerSO = new SerializedObject(manager);
        SerializedProperty trackerProp   = managerSO.FindProperty("investigationTracker");
        SerializedProperty lijstProp     = managerSO.FindProperty("onderzoeksobjecten");

        trackerProp.objectReferenceValue = tracker;
        lijstProp.ClearArray();

        for (int i = 0; i < defs.Length; i++)
        {
            ObjDef def = defs[i];

            // GameObject + collider
            var prim = GameObject.CreatePrimitive(PrimitiveType.Cube);
            prim.name = "Investigatable_" + def.id;
            prim.transform.localScale = new Vector3(0.25f, 0.15f, 0.35f);
            prim.transform.position   = def.pos;
            Undo.RegisterCreatedObjectUndo(prim, "Create " + prim.name);

            // Kleur zodat ze opvallen
            var rend = prim.GetComponent<Renderer>();
            if (rend != null)
            {
                var mat = new Material(rend.sharedMaterial);
                mat.color = new Color(0.55f, 0.75f, 0.9f); // lichtblauw
                rend.material = mat;
            }

            // Component
            InvestigatableObject comp = prim.AddComponent<InvestigatableObject>();

            // 3D-label boven het object (counter-scale tegen cube-scale → uniform in wereld)
            var labelGO = new GameObject("Label");
            labelGO.transform.SetParent(prim.transform, false);
            labelGO.transform.localPosition = new Vector3(0f, 3.3f, 0f);   // ~0.5m boven cube
            labelGO.transform.localScale    = new Vector3(0.72f, 1.2f, 0.514f); // → wereldscale ~0.18
            var tmp = labelGO.AddComponent<TextMeshPro>();
            tmp.text      = $"{def.label}\n[onderzoek: E / klik]";
            tmp.fontSize  = 14;
            tmp.color     = new Color(0.1f, 0.1f, 0.1f);
            tmp.alignment = TextAlignmentOptions.Center;

            // Serialized velden zetten
            SerializedObject compSO = new SerializedObject(comp);
            compSO.FindProperty("objectId").stringValue     = def.id;
            compSO.FindProperty("category").enumValueIndex  = def.cat;
            compSO.FindProperty("finding").stringValue      = def.finding;
            compSO.FindProperty("isCritical").boolValue     = def.critical;
            compSO.FindProperty("displayLabel").stringValue = def.label;
            compSO.FindProperty("labelText").objectReferenceValue = tmp;
            compSO.ApplyModifiedProperties();

            // Toevoegen aan lijst
            lijstProp.InsertArrayElementAtIndex(i);
            lijstProp.GetArrayElementAtIndex(i).objectReferenceValue = comp;
        }

        managerSO.ApplyModifiedProperties();

        Debug.Log($"[SBAR] Klaar: {defs.Length} onderzoeksobjecten aangemaakt en gekoppeld aan SBARManager.");
        EditorUtility.SetDirty(manager);
    }
}

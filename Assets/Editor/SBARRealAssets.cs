using UnityEngine;
using UnityEditor;

/// <summary>
/// Vervangt primitieve placeholders door echte gedownloade modellen (Assets/Imported).
/// Behoudt root-GameObjects → NPCInteractable / MeasurementDevice / SBARManager-refs blijven intact.
/// Verbergt de primitieve mesh, hangt het echte model als visuele child "ModelVisual".
/// Idempotent: re-run vervangt vorige ModelVisual.
///
/// Schaal/positie/rotatie van externe modellen klopt zelden meteen → pas de TUNE-constants
/// hieronder aan en draai 'SBAR/Plaats Echte Assets' opnieuw.
/// </summary>
public static class SBARRealAssets
{
    // ── ASSET-PADEN ────────────────────────────────────────────────
    const string P_GRANDMA   = "Assets/Imported/Patient_Grandma/source/2.fbx";
    const string P_CH16      = "Assets/Imported/Character_Ch16/Ch16_nonPBR.fbx";
    const string P_BED       = "Assets/Imported/Bed/hospital_bed.glb";
    const string P_MONITOR   = "Assets/Imported/Monitor/source/monitor.fbx";
    const string P_SPHYGMO   = "Assets/Imported/Sphygmomanometer/source/ketuatukei3.glb";
    const string P_CLIPBOARD = "Assets/Imported/Clipboard/clipboard.glb";
    const string P_INFUUS    = "Assets/Imported/Misc/15.glb";
    const string P_AUDIO     = "Assets/Imported/Audio/hospital-ambient.mp3";

    // ── TUNE HIER (positie = lokaal t.o.v. root; euler = graden; scale) ──
    // Patiënt (grandma) — child van 'Bed', ligt op matras
    static readonly Vector3 GRANDMA_POS   = new Vector3(-2.5f, 0.95f, 2f);
    static readonly Vector3 GRANDMA_EULER = new Vector3(-90f, 0f, 0f);   // liggend op rug
    static readonly Vector3 GRANDMA_SCALE = new Vector3(1f, 1f, 1f);

    // NPC's (Ch16) — child van Collega/Arts root (root-center op y=1, voeten op vloer)
    static readonly Vector3 NPC_POS   = new Vector3(0f, -1f, 0f);
    static readonly Vector3 NPC_EULER = new Vector3(0f, 180f, 0f);       // kijkt de kamer in
    static readonly Vector3 NPC_SCALE = new Vector3(1f, 1f, 1f);
    static readonly Color   TINT_COLLEGA = new Color(0.80f, 1.00f, 1.00f); // subtiel cyaan
    static readonly Color   TINT_ARTS    = new Color(1.00f, 1.00f, 0.95f); // subtiel warm

    // Bed-model — child van 'Bed'
    static readonly Vector3 BED_POS   = new Vector3(-2.5f, 0f, 2f);
    static readonly Vector3 BED_EULER = new Vector3(0f, 90f, 0f);
    static readonly Vector3 BED_SCALE = new Vector3(1f, 1f, 1f);

    // Monitor — child van 'Saturatiemeter' device
    static readonly Vector3 MON_POS   = new Vector3(0f, 0f, 0f);
    static readonly Vector3 MON_EULER = new Vector3(0f, 180f, 0f);
    static readonly Vector3 MON_SCALE = new Vector3(0.4f, 0.4f, 0.4f);

    // Bloeddrukmeter — child van 'Bloeddrukband' device
    static readonly Vector3 BP_POS   = new Vector3(0f, 0f, 0f);
    static readonly Vector3 BP_EULER = new Vector3(0f, 0f, 0f);
    static readonly Vector3 BP_SCALE = new Vector3(1f, 1f, 1f);

    // Losse props (wereld-positie, onder holder 'ImportedProps')
    static readonly Vector3 INFUUS_POS   = new Vector3(-1.9f, 0f, 3.2f);
    static readonly Vector3 INFUUS_EULER = Vector3.zero;
    static readonly Vector3 INFUUS_SCALE = new Vector3(1f, 1f, 1f);

    static readonly Vector3 CLIP_POS   = new Vector3(-0.5f, 1.0f, 2.0f);
    static readonly Vector3 CLIP_EULER = Vector3.zero;
    static readonly Vector3 CLIP_SCALE = new Vector3(1f, 1f, 1f);

    [MenuItem("SBAR/Plaats Echte Assets")]
    static void Place()
    {
        // NPC's: Ch16 op collega + arts
        SwapVisual("Collega", P_CH16, NPC_POS, NPC_EULER, NPC_SCALE, TINT_COLLEGA);
        SwapVisual("Arts",    P_CH16, NPC_POS, NPC_EULER, NPC_SCALE, TINT_ARTS);

        // Patiënt: verberg capsule, grandma onder 'Bed'
        var bed = GameObject.Find("Bed");
        if (bed != null)
        {
            HidePrimitive(bed.transform.Find("Patient_DeGroot"));
            ReplaceChild(bed.transform, "ModelVisual_Patient", P_GRANDMA, GRANDMA_POS, GRANDMA_EULER, GRANDMA_SCALE, null, world:true);
            HidePrimitive(bed.transform.Find("Matras"));
            ReplaceChild(bed.transform, "ModelVisual_Bed", P_BED, BED_POS, BED_EULER, BED_SCALE, null, world:true);
        }
        else Debug.LogWarning("[SBAR] 'Bed' niet gevonden.");

        // Meetapparaten: model over de cube (component + Label + collider blijven)
        SwapVisual("Saturatiemeter", P_MONITOR, MON_POS, MON_EULER, MON_SCALE, null);
        SwapVisual("Bloeddrukband",  P_SPHYGMO, BP_POS,  BP_EULER,  BP_SCALE,  null);

        // Losse props
        var holder = GameObject.Find("ImportedProps") ?? new GameObject("ImportedProps");
        ClearChildren(holder.transform);
        Place(Load(P_INFUUS),    holder.transform, INFUUS_POS, INFUUS_EULER, INFUUS_SCALE, "Infuusstandaard", world:true);
        Place(Load(P_CLIPBOARD), holder.transform, CLIP_POS,   CLIP_EULER,   CLIP_SCALE,   "Klembord",        world:true);

        // Sfeergeluid op Player
        AddAmbientAudio();

        UnityEditor.SceneManagement.EditorSceneManager.MarkAllScenesDirty();
        Debug.Log("[SBAR] Echte assets geplaatst. Pas TUNE-constants aan en herhaal indien schaal/positie afwijkt.");
    }

    // ── Helpers ────────────────────────────────────────────────────

    static void SwapVisual(string rootName, string modelPath, Vector3 pos, Vector3 euler, Vector3 scale, Color? tint)
    {
        var root = GameObject.Find(rootName);
        if (root == null) { Debug.LogWarning($"[SBAR] '{rootName}' niet gevonden."); return; }

        Undo.RegisterFullObjectHierarchyUndo(root, "Plaats model " + rootName);

        // Oude visuals opruimen (idempotent)
        DestroyChild(root.transform, "PuppetBody");
        DestroyChild(root.transform, "ModelVisual");

        // Primitieve mesh van root verbergen (collider blijft voor interactie)
        var mr = root.GetComponent<MeshRenderer>();
        if (mr != null) Object.DestroyImmediate(mr);
        var mf = root.GetComponent<MeshFilter>();
        if (mf != null) Object.DestroyImmediate(mf);

        var inst = Place(Load(modelPath), root.transform, pos, euler, scale, "ModelVisual", world:false);
        if (inst != null)
        {
            FixRenderersURP(inst);
            if (tint.HasValue) ApplyTint(inst, tint.Value);
        }
    }

    static void ReplaceChild(Transform parent, string childName, string modelPath,
                             Vector3 pos, Vector3 euler, Vector3 scale, Color? tint, bool world)
    {
        DestroyChild(parent, childName);
        var inst = Place(Load(modelPath), parent, pos, euler, scale, childName, world);
        if (inst != null) { FixRenderersURP(inst); if (tint.HasValue) ApplyTint(inst, tint.Value); }
    }

    static GameObject Load(string path)
    {
        var go = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (go == null) Debug.LogWarning($"[SBAR] Model niet gevonden/geïmporteerd: {path}");
        return go;
    }

    static GameObject Place(GameObject model, Transform parent, Vector3 pos, Vector3 euler, Vector3 scale, string name, bool world)
    {
        if (model == null) return null;
        var inst = (GameObject)PrefabUtility.InstantiatePrefab(model);
        inst.name = name;
        inst.transform.SetParent(parent, false);
        if (world)
        {
            inst.transform.position      = pos;
            inst.transform.eulerAngles   = euler;
            inst.transform.localScale    = scale;
        }
        else
        {
            inst.transform.localPosition    = pos;
            inst.transform.localEulerAngles = euler;
            inst.transform.localScale       = scale;
        }
        return inst;
    }

    static void FixRenderersURP(GameObject go)
    {
        var urp = Shader.Find("Universal Render Pipeline/Lit");
        if (urp == null) return;
        foreach (var r in go.GetComponentsInChildren<Renderer>(true))
        {
            var mats = r.sharedMaterials;
            bool changed = false;
            for (int i = 0; i < mats.Length; i++)
            {
                var m = mats[i];
                if (m == null) continue;
                // Al URP of glTFast → laat staan
                if (m.shader != null && (m.shader.name.Contains("Universal") || m.shader.name.Contains("glTF"))) continue;

                var nm = new Material(urp);
                Texture tex = m.HasProperty("_MainTex") ? m.GetTexture("_MainTex")
                            : m.HasProperty("_BaseMap") ? m.GetTexture("_BaseMap") : null;
                if (tex != null) nm.SetTexture("_BaseMap", tex);
                Color col = m.HasProperty("_Color") ? m.GetColor("_Color")
                          : m.HasProperty("_BaseColor") ? m.GetColor("_BaseColor") : Color.white;
                nm.SetColor("_BaseColor", col);
                mats[i] = nm;
                changed = true;
            }
            if (changed) r.sharedMaterials = mats;
        }
    }

    static void ApplyTint(GameObject go, Color tint)
    {
        foreach (var r in go.GetComponentsInChildren<Renderer>(true))
        {
            var mpb = new MaterialPropertyBlock();
            r.GetPropertyBlock(mpb);
            mpb.SetColor("_BaseColor", tint);
            r.SetPropertyBlock(mpb);
        }
    }

    static void HidePrimitive(Transform t)
    {
        if (t == null) return;
        var mr = t.GetComponent<MeshRenderer>();
        if (mr != null) mr.enabled = false;
    }

    static void DestroyChild(Transform parent, string name)
    {
        var c = parent.Find(name);
        if (c != null) Object.DestroyImmediate(c.gameObject);
    }

    static void ClearChildren(Transform parent)
    {
        for (int i = parent.childCount - 1; i >= 0; i--)
            Object.DestroyImmediate(parent.GetChild(i).gameObject);
    }

    static void AddAmbientAudio()
    {
        var player = GameObject.Find("Player");
        if (player == null) return;
        var clip = AssetDatabase.LoadAssetAtPath<AudioClip>(P_AUDIO);
        if (clip == null) { Debug.LogWarning("[SBAR] Audio niet gevonden: " + P_AUDIO); return; }
        var src = player.GetComponent<AudioSource>() ?? player.AddComponent<AudioSource>();
        src.clip = clip;
        src.loop = true;
        src.playOnAwake = true;
        src.volume = 0.25f;
        src.spatialBlend = 0f; // 2D ambient
    }
}

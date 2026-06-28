using UnityEngine;
using UnityEditor;

/// <summary>
/// Past de HUD-layout aan in de REEDS gebouwde scène (zonder rebuild, assets blijven staan).
/// Vragenmenu omhoog zodat het niet meer over de ondertitel/knoppen valt.
/// Pas de constants aan + draai 'SBAR/Fix HUD Layout' opnieuw bij verdere bijstelling.
/// </summary>
public static class SBARHudFix
{
    // TUNE — anchoredPosition (x,y) in UI-punten
    static readonly Vector2 INSTRUCTIE_POS = new Vector2(0f, 230f);  // instructie hoger weg van keuzemenu
    static readonly Vector2 KEUZE_POS      = new Vector2(0f, 90f);   // was (0,-30): nu hoger
    static readonly Vector2 ONDERTITEL_POS = new Vector2(0f, -210f); // antwoord-balk lager weg van knoppen
    static readonly Vector2 VOLGENDE_POS   = new Vector2(250f, -250f);
    static readonly Vector2 HERHAAL_POS    = new Vector2(0f, -250f);

    [MenuItem("SBAR/Fix HUD Layout")]
    static void Fix()
    {
        Move("InstructieTekst",  INSTRUCTIE_POS);
        Move("KeuzeMenuPaneel", KEUZE_POS);
        Move("OndertitelPaneel", ONDERTITEL_POS);
        Move("VolgendeStapKnop", VOLGENDE_POS);
        Move("HerhaalStapKnop",  HERHAAL_POS);

        UnityEditor.SceneManagement.EditorSceneManager.MarkAllScenesDirty();
        Debug.Log("[SBAR] HUD-layout aangepast.");
    }

    static void Move(string name, Vector2 pos)
    {
        var go = GameObject.Find(name);
        if (go == null) { Debug.LogWarning($"[SBAR] '{name}' niet gevonden."); return; }
        var rt = go.GetComponent<RectTransform>();
        if (rt == null) { Debug.LogWarning($"[SBAR] '{name}' heeft geen RectTransform."); return; }
        Undo.RecordObject(rt, "Move " + name);
        rt.anchoredPosition = pos;
        EditorUtility.SetDirty(rt);
    }
}

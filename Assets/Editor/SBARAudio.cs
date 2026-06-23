using UnityEngine;
using UnityEditor;

/// <summary>
/// Zet het ziekenhuis-achtergrondgeluid als loopende 2D-AudioSource op de Player.
/// Draai 'SBAR/Plaats Achtergrondgeluid'. Let op: geluid speelt alleen in Play-mode.
/// </summary>
public static class SBARAudio
{
    const string P_AUDIO = "Assets/Imported/Audio/hospital-ambient.mp3";
    const float VOLUME = 0.25f;

    [MenuItem("SBAR/Plaats Achtergrondgeluid")]
    static void Place()
    {
        var clip = AssetDatabase.LoadAssetAtPath<AudioClip>(P_AUDIO);
        if (clip == null)
        {
            Debug.LogError("[SBAR] AudioClip niet gevonden/geïmporteerd: " + P_AUDIO);
            return;
        }

        // Bij voorkeur op Player; anders op SBARManager
        var host = GameObject.Find("Player") ?? GameObject.Find("SBARManager");
        if (host == null)
        {
            Debug.LogError("[SBAR] Geen Player/SBARManager om geluid op te zetten.");
            return;
        }

        Undo.RegisterFullObjectHierarchyUndo(host, "Plaats achtergrondgeluid");

        var src = host.GetComponent<AudioSource>();
        if (src == null) src = Undo.AddComponent<AudioSource>(host);

        src.clip          = clip;
        src.loop          = true;
        src.playOnAwake   = true;
        src.volume        = VOLUME;
        src.spatialBlend  = 0f;   // 2D, overal even hard
        src.priority      = 200;  // achtergrond

        EditorUtility.SetDirty(host);

        // AudioListener verplicht — builder verwijderde de oude Main Camera met listener
        EnsureAudioListener();

        UnityEditor.SceneManagement.EditorSceneManager.MarkAllScenesDirty();
        Debug.Log($"[SBAR] Achtergrondgeluid gezet op '{host.name}' (volume {VOLUME}). Speelt in Play-mode.");
    }

    static void EnsureAudioListener()
    {
        if (Object.FindObjectOfType<AudioListener>() != null) return;

        // Bij voorkeur op de camera, anders op Player
        var cam = GameObject.Find("PlayerCamera")
                  ?? (Camera.main != null ? Camera.main.gameObject : null)
                  ?? GameObject.Find("Player");
        if (cam == null)
        {
            Debug.LogWarning("[SBAR] Geen camera/Player gevonden voor AudioListener.");
            return;
        }
        Undo.AddComponent<AudioListener>(cam);
        EditorUtility.SetDirty(cam);
        Debug.Log($"[SBAR] AudioListener toegevoegd aan '{cam.name}'.");
    }
}

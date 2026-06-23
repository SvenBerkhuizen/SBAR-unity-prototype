using System.Collections;
using TMPro;
using UnityEngine;

namespace SBAR.UI
{
    public class SubtitleSystem : MonoBehaviour
    {
        [SerializeField] private GameObject panel;
        [SerializeField] private TMP_Text label;
        [SerializeField] private AudioSource audioSource;

        private Coroutine _hideRoutine;

        private void Awake()
        {
            Hide();
        }

        public void Show(string speaker, string line, float duration = 0f, AudioClip clip = null)
        {
            if (panel != null) panel.SetActive(true);
            if (label != null)
                label.text = string.IsNullOrEmpty(speaker) ? line : $"<b>{speaker}:</b> {line}";

            if (clip != null && audioSource != null) audioSource.PlayOneShot(clip);

            if (_hideRoutine != null) StopCoroutine(_hideRoutine);
            if (duration > 0f) _hideRoutine = StartCoroutine(HideAfter(duration));
        }

        public void Hide()
        {
            if (panel != null) panel.SetActive(false);
        }

        private IEnumerator HideAfter(float seconds)
        {
            yield return new WaitForSeconds(seconds);
            Hide();
        }
    }
}

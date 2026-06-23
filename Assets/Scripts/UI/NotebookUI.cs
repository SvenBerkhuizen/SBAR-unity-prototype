using System.Text;
using TMPro;
using UnityEngine;
using SBAR.Core;

namespace SBAR.UI
{
    public class NotebookUI : MonoBehaviour
    {
        [SerializeField] private GameObject panel;
        [SerializeField] private TMP_Text contentLabel;

        private SBARNotebook _notebook;

        // N-toets wordt afgehandeld door SBARManager (altijd actief). Dit component
        // zit op het paneel zelf; als het paneel inactief is draait Update() hier niet meer.

        public void SetNotebook(SBARNotebook notebook)
        {
            if (_notebook != null) _notebook.Changed -= Refresh;
            _notebook = notebook;
            if (_notebook != null) _notebook.Changed += Refresh;
            Refresh();
        }

        public void Toggle()
        {
            if (panel != null) panel.SetActive(!panel.activeSelf);
        }

        public void Show()
        {
            if (panel != null) panel.SetActive(true);
        }

        public void Hide()
        {
            if (panel != null) panel.SetActive(false);
        }

        public void Refresh()
        {
            if (contentLabel == null || _notebook == null) return;
            var sb = new StringBuilder();
            sb.AppendLine("<b>NOTITIEBOEKJE — SBAR</b>");
            AppendSection(sb, "S — Situation", _notebook.Situation);
            AppendSection(sb, "B — Background", _notebook.Background);
            AppendSection(sb, "A — Assessment", _notebook.Assessment);
            AppendSection(sb, "R — Recommendation", _notebook.Recommendation);
            contentLabel.text = sb.ToString().TrimEnd();
        }

        private static void AppendSection(StringBuilder sb, string titel, System.Collections.Generic.List<string> lijnen)
        {
            sb.AppendLine();
            sb.AppendLine($"<b>{titel}</b>");
            if (lijnen == null || lijnen.Count == 0)
            {
                sb.AppendLine("  (nog leeg)");
                return;
            }
            foreach (var l in lijnen) sb.AppendLine($"  • {l}");
        }
    }
}

using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using SBAR.Core;

namespace SBAR.UI
{
    /// <summary>
    /// Toont het SBAR-notitieboek in het gezichtsveld (HUD). Om clutter te vermijden
    /// kies je per tab (S/B/A/R) welk SBAR-onderdeel je bekijkt; "Alles" toont het
    /// volledige overzicht.
    /// </summary>
    public class NotebookUI : MonoBehaviour
    {
        [SerializeField] private GameObject panel;
        [SerializeField] private TMP_Text contentLabel;

        [Header("Tabs (optioneel)")]
        [SerializeField] private Button tabAlles;
        [SerializeField] private Button tabS;
        [SerializeField] private Button tabB;
        [SerializeField] private Button tabA;
        [SerializeField] private Button tabR;
        [SerializeField] private Color tabActief = new Color(0.20f, 0.45f, 0.75f, 1f);
        [SerializeField] private Color tabInactief = new Color(0.15f, 0.18f, 0.22f, 0.9f);

        private SBARNotebook _notebook;
        private bool _toonAlles = true;
        private SBARPart _geselecteerd = SBARPart.Situation;

        private void Awake()
        {
            WireTab(tabAlles, () => SelecteerAlles());
            WireTab(tabS, () => SelecteerDeel(SBARPart.Situation));
            WireTab(tabB, () => SelecteerDeel(SBARPart.Background));
            WireTab(tabA, () => SelecteerDeel(SBARPart.Assessment));
            WireTab(tabR, () => SelecteerDeel(SBARPart.Recommendation));
        }

        private void WireTab(Button b, UnityEngine.Events.UnityAction action)
        {
            if (b == null) return;
            b.onClick.RemoveAllListeners();
            b.onClick.AddListener(action);
        }

        public void SetNotebook(SBARNotebook notebook)
        {
            if (_notebook != null) _notebook.Changed -= Refresh;
            _notebook = notebook;
            if (_notebook != null) _notebook.Changed += Refresh;
            if (contentLabel != null)
            {
                contentLabel.lineSpacing = 10f;
                contentLabel.paragraphSpacing = 16f;
            }
            Refresh();
        }

        public void SelecteerAlles()
        {
            _toonAlles = true;
            UpdateTabKleuren();
            Refresh();
        }

        public void SelecteerDeel(SBARPart part)
        {
            _toonAlles = false;
            _geselecteerd = part;
            UpdateTabKleuren();
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

            if (_toonAlles)
            {
                sb.AppendLine("<b>NOTITIEBOEK — SBAR</b>");
                AppendSection(sb, "S — Situation", _notebook.Situation);
                AppendSection(sb, "B — Background", _notebook.Background);
                AppendSection(sb, "A — Assessment", _notebook.Assessment);
                AppendSection(sb, "R — Recommendation", _notebook.Recommendation);
            }
            else
            {
                sb.AppendLine($"<b>{Titel(_geselecteerd)}</b>");
                var lijnen = _notebook.GetLines(_geselecteerd);
                if (lijnen == null || lijnen.Count == 0) sb.AppendLine("  (nog leeg)");
                else foreach (var l in lijnen) sb.AppendLine($"  • {l}");
            }

            contentLabel.text = sb.ToString().TrimEnd();
        }

        private void UpdateTabKleuren()
        {
            SetTabKleur(tabAlles, _toonAlles);
            SetTabKleur(tabS, !_toonAlles && _geselecteerd == SBARPart.Situation);
            SetTabKleur(tabB, !_toonAlles && _geselecteerd == SBARPart.Background);
            SetTabKleur(tabA, !_toonAlles && _geselecteerd == SBARPart.Assessment);
            SetTabKleur(tabR, !_toonAlles && _geselecteerd == SBARPart.Recommendation);
        }

        private void SetTabKleur(Button b, bool actief)
        {
            if (b == null) return;
            var img = b.GetComponent<Image>();
            if (img != null) img.color = actief ? tabActief : tabInactief;
        }

        private static string Titel(SBARPart part)
        {
            switch (part)
            {
                case SBARPart.Situation:      return "S — Situation";
                case SBARPart.Background:      return "B — Background";
                case SBARPart.Assessment:      return "A — Assessment";
                case SBARPart.Recommendation:  return "R — Recommendation";
                default:                       return "SBAR";
            }
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

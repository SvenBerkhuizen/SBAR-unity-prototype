using System;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SBAR.UI
{
    public enum FeedbackStatus
    {
        Goed,
        Gedeeltelijk,
        Onvoldoende
    }

    public class FeedbackLine
    {
        public string label;
        public string oordeel;
        public FeedbackStatus status;

        public FeedbackLine(string label, string oordeel, FeedbackStatus status)
        {
            this.label = label;
            this.oordeel = oordeel;
            this.status = status;
        }
    }

    public class FeedbackScreen : MonoBehaviour
    {
        [SerializeField] private GameObject panel;
        [SerializeField] private TMP_Text titleLabel;
        [SerializeField] private TMP_Text contentLabel;
        [SerializeField] private Button replayButton;
        [SerializeField] private TMP_Text replayLabel;
        [SerializeField] private Button quitButton;
        [SerializeField] private TMP_Text quitLabel;

        public void Show(string titel, IList<FeedbackLine> lines, string replayTekst,
                         string quitTekst, Action onReplay, Action onQuit)
        {
            if (panel != null) panel.SetActive(true);
            if (titleLabel != null) titleLabel.text = titel;
            if (contentLabel != null) contentLabel.text = BuildContent(lines);

            if (replayLabel != null) replayLabel.text = replayTekst;
            if (quitLabel != null) quitLabel.text = quitTekst;

            if (replayButton != null)
            {
                replayButton.onClick.RemoveAllListeners();
                if (onReplay != null) replayButton.onClick.AddListener(() => onReplay());
            }
            if (quitButton != null)
            {
                quitButton.onClick.RemoveAllListeners();
                if (onQuit != null) quitButton.onClick.AddListener(() => onQuit());
            }
        }

        public void Hide()
        {
            if (panel != null) panel.SetActive(false);
        }

        private static string BuildContent(IList<FeedbackLine> lines)
        {
            if (lines == null) return string.Empty;
            var sb = new StringBuilder();
            foreach (var l in lines)
            {
                string icoon;
                Color kleur;
                switch (l.status)
                {
                    case FeedbackStatus.Goed:
                        icoon = "[v]"; kleur = new Color(0.2f, 0.7f, 0.3f); break;
                    case FeedbackStatus.Gedeeltelijk:
                        icoon = "?"; kleur = new Color(0.85f, 0.7f, 0.1f); break;
                    default:
                        icoon = "[x]"; kleur = new Color(0.85f, 0.25f, 0.2f); break;
                }
                string hex = ColorUtility.ToHtmlStringRGB(kleur);
                sb.AppendLine($"<color=#{hex}>{icoon}</color> <b>{l.label}:</b> {l.oordeel}");
            }
            return sb.ToString().TrimEnd();
        }
    }
}

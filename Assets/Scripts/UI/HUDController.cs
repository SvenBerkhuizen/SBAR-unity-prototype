using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SBAR.UI
{
    public class HUDController : MonoBehaviour
    {
        [Header("Instructie")]
        [SerializeField] private TMP_Text instructionLabel;

        [Header("Briefingkaart")]
        [SerializeField] private GameObject briefingPanel;
        [SerializeField] private TMP_Text briefingLabel;

        [Header("Knoppen")]
        [SerializeField] private Button nextButton;
        [SerializeField] private TMP_Text nextButtonLabel;
        [SerializeField] private Button repeatButton;

        [Header("Besturingsoverlay")]
        [SerializeField] private TMP_Text controlsHint;
        [SerializeField] private GameObject helpPanel;
        [SerializeField] private KeyCode helpKey = KeyCode.H;

        private const string ControlsText =
            "WASD: lopen   |   Rechtermuis vasthouden: kijken   |   E/linkermuis: interactie   |   N: notitieboekje   |   H: hulp";

        private void Awake()
        {
            if (controlsHint != null) controlsHint.text = ControlsText;
            if (helpPanel != null) helpPanel.SetActive(false);
            if (briefingPanel != null) briefingPanel.SetActive(false);
        }

        private void Update()
        {
            if (Input.GetKeyDown(helpKey) && helpPanel != null)
                helpPanel.SetActive(!helpPanel.activeSelf);
        }

        public void SetInstruction(string text)
        {
            if (instructionLabel != null) instructionLabel.text = text;
        }

        public void ShowBriefingCard(string text)
        {
            if (briefingPanel != null) briefingPanel.SetActive(true);
            if (briefingLabel != null) briefingLabel.text = text;
        }

        public void HideBriefingCard()
        {
            if (briefingPanel != null) briefingPanel.SetActive(false);
        }

        public void SetNextButton(bool visible, bool interactable, string label, Action onClick)
        {
            if (nextButton == null) return;
            nextButton.gameObject.SetActive(visible);
            nextButton.interactable = interactable;
            if (nextButtonLabel != null) nextButtonLabel.text = label;
            nextButton.onClick.RemoveAllListeners();
            if (onClick != null) nextButton.onClick.AddListener(() => onClick());
        }

        public void SetRepeatButton(bool visible, Action onClick)
        {
            if (repeatButton == null) return;
            repeatButton.gameObject.SetActive(visible);
            repeatButton.onClick.RemoveAllListeners();
            if (onClick != null) repeatButton.onClick.AddListener(() => onClick());
        }
    }
}

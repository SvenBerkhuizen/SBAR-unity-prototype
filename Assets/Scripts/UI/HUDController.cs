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

        [Header("Tijdslimiet")]
        [SerializeField] private GameObject timerPanel;
        [SerializeField] private TMP_Text timerLabel;
        [SerializeField] private Color timerNormal = new Color(0.85f, 0.92f, 1f);
        [SerializeField] private Color timerWaarschuwing = new Color(1f, 0.6f, 0.1f);
        [SerializeField] private Color timerKritiek = new Color(1f, 0.25f, 0.2f);

        [Header("Besturingsoverlay")]
        [SerializeField] private TMP_Text controlsHint;
        [SerializeField] private GameObject helpPanel;
        [SerializeField] private KeyCode helpKey = KeyCode.H;
        [Tooltip("Oproepbare hulp-knop (OE-D6): werkt met controller-ray, geen toetsenbord nodig.")]
        [SerializeField] private Button helpButton;

        private float _timeLeft;
        private bool _timerRunning;
        private bool _timerExpired;
        private Action _onTimerExpire;

        private const string ControlsText =
            "WASD: lopen   |   Rechtermuis vasthouden: kijken   |   E/linkermuis: interactie   |   N: notitieboekje   |   H: hulp";

        private void Awake()
        {
            if (controlsHint != null) controlsHint.text = ControlsText;
            if (helpPanel != null) helpPanel.SetActive(false);
            if (briefingPanel != null) briefingPanel.SetActive(false);
            if (timerPanel != null) timerPanel.SetActive(false);
            if (helpButton != null)
            {
                helpButton.onClick.RemoveAllListeners();
                helpButton.onClick.AddListener(ToggleHelp);
            }
        }

        private void Update()
        {
            if (Input.GetKeyDown(helpKey)) ToggleHelp();

            if (_timerRunning) TickTimer();
        }

        // OE-D6: oproepbare hulp — via H-toets (desktop) of HUD-knop (VR-controller).
        public void ToggleHelp()
        {
            if (helpPanel != null) helpPanel.SetActive(!helpPanel.activeSelf);
        }

        // --- Tijdslimiet ---

        public void StartTimer(float seconds, Action onExpire)
        {
            _timeLeft = Mathf.Max(0f, seconds);
            _onTimerExpire = onExpire;
            _timerRunning = true;
            _timerExpired = false;
            if (timerPanel != null) timerPanel.SetActive(true);
            RenderTimer();
        }

        public void StopTimer()
        {
            _timerRunning = false;
            if (timerPanel != null) timerPanel.SetActive(false);
        }

        private void TickTimer()
        {
            _timeLeft -= Time.deltaTime;
            if (_timeLeft <= 0f)
            {
                _timeLeft = 0f;
                _timerRunning = false;
                RenderTimer();
                if (!_timerExpired)
                {
                    _timerExpired = true;
                    if (_onTimerExpire != null) _onTimerExpire();
                }
                return;
            }
            RenderTimer();
        }

        private void RenderTimer()
        {
            if (timerLabel == null) return;
            int totaal = Mathf.CeilToInt(_timeLeft);
            int min = totaal / 60;
            int sec = totaal % 60;
            timerLabel.text = $"{min}:{sec:00}";
            timerLabel.color = _timeLeft <= 10f ? timerKritiek
                             : _timeLeft <= 30f ? timerWaarschuwing
                             : timerNormal;
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

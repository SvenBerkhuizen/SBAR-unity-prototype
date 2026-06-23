using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SBAR.UI
{
    public class ChoiceMenu : MonoBehaviour
    {
        [SerializeField] private GameObject panel;
        [SerializeField] private TMP_Text promptLabel;
        [SerializeField] private List<Button> buttons = new List<Button>();
        [SerializeField] private List<TMP_Text> buttonLabels = new List<TMP_Text>();

        private Action<int> _onChosen;

        private void Awake()
        {
            Hide();
        }

        public void Show(string prompt, IList<string> options, Action<int> onChosen)
        {
            _onChosen = onChosen;
            if (panel != null) panel.SetActive(true);
            if (promptLabel != null) promptLabel.text = prompt;

            for (int i = 0; i < buttons.Count; i++)
            {
                int index = i;
                bool active = options != null && i < options.Count;
                if (buttons[i] != null)
                {
                    buttons[i].gameObject.SetActive(active);
                    buttons[i].interactable = true;
                    buttons[i].onClick.RemoveAllListeners();
                    if (active)
                        buttons[i].onClick.AddListener(() => _onChosen?.Invoke(index));
                }
                if (active && i < buttonLabels.Count && buttonLabels[i] != null)
                    buttonLabels[i].text = options[i];
            }
        }

        public void SetOptionInteractable(int index, bool interactable)
        {
            if (index >= 0 && index < buttons.Count && buttons[index] != null)
                buttons[index].interactable = interactable;
        }

        public void Hide()
        {
            if (panel != null) panel.SetActive(false);
        }
    }
}

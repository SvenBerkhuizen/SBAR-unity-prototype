using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using SBAR.Core;

namespace SBAR.UI
{
    public class OverdrachtPanel : MonoBehaviour
    {
        [SerializeField] private GameObject panel;
        [SerializeField] private List<Button> partButtons = new List<Button>();
        [SerializeField] private List<TMP_Text> statusLabels = new List<TMP_Text>();

        private static readonly Color GroenCompleet = new Color(0.2f, 0.7f, 0.3f);
        private static readonly Color GeelOntbreekt = new Color(0.85f, 0.7f, 0.1f);
        private static readonly SBARPart[] Volgorde =
        {
            SBARPart.Situation, SBARPart.Background, SBARPart.Assessment, SBARPart.Recommendation
        };

        private SBARNotebook _notebook;
        private readonly HashSet<SBARPart> _overgedragen = new HashSet<SBARPart>();
        private Action _onAllHandedOver;

        public void Setup(SBARNotebook notebook, Action onAllHandedOver)
        {
            _notebook = notebook;
            _onAllHandedOver = onAllHandedOver;
            _overgedragen.Clear();

            for (int i = 0; i < partButtons.Count && i < Volgorde.Length; i++)
            {
                SBARPart part = Volgorde[i];
                int idx = i;
                if (partButtons[i] != null)
                {
                    var txt = partButtons[i].GetComponentInChildren<TMP_Text>();
                    if (txt != null) txt.text = $"Geef {part} door";
                    partButtons[i].interactable = true;
                    partButtons[i].onClick.RemoveAllListeners();
                    partButtons[i].onClick.AddListener(() => HandOver(idx));
                }
                UpdateStatus(i, part);
            }
        }

        public void Show()
        {
            if (panel != null) panel.SetActive(true);
        }

        public void Hide()
        {
            if (panel != null) panel.SetActive(false);
        }

        public void HandOver(int index)
        {
            if (index < 0 || index >= Volgorde.Length) return;
            SBARPart part = Volgorde[index];
            _overgedragen.Add(part);
            if (index < partButtons.Count && partButtons[index] != null)
                partButtons[index].interactable = false;
            UpdateStatus(index, part);

            if (_overgedragen.Count >= Volgorde.Length)
                _onAllHandedOver?.Invoke();
        }

        private void UpdateStatus(int index, SBARPart part)
        {
            if (index >= statusLabels.Count || statusLabels[index] == null) return;
            bool compleet = _notebook != null && _notebook.HasContent(part);
            bool gedaan = _overgedragen.Contains(part);

            string icoon = compleet ? "[v]" : "?";
            string staat = compleet ? "compleet" : "ontbrekend";
            string doorgegeven = gedaan ? "  — doorgegeven" : "";
            statusLabels[index].text = $"{icoon} {part}: {staat}{doorgegeven}";
            statusLabels[index].color = compleet ? GroenCompleet : GeelOntbreekt;
        }
    }
}

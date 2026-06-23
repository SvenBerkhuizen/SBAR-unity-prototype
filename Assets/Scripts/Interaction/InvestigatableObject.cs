using TMPro;
using UnityEngine;
using SBAR.Core;

namespace SBAR.Interaction
{
    /// <summary>
    /// Kamerobject dat de student vrij kan onderzoeken (dossier, medicatielijst, etc.).
    /// Extend InteractableBase voor hover-highlight; implementeert IInvestigatable.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class InvestigatableObject : InteractableBase, IInvestigatable
    {
        [Header("Bevinding")]
        [SerializeField] private string   objectId;
        [SerializeField] private SBARPart category;
        [SerializeField] private string   finding;
        [SerializeField] private bool     isCritical;

        [Header("Label")]
        [SerializeField] private string   displayLabel = "Object";
        [SerializeField] private TMP_Text labelText;

        private Collider           _col;
        private InvestigationResult _result;

        protected override void Awake()
        {
            base.Awake();
            _col = GetComponent<Collider>();
            _col.enabled = false; // start uitgeschakeld; SBARManager activeert in onderzoeksfase
            _result = new InvestigationResult(objectId, category, finding, isCritical);
            RefreshLabel(false);
        }

        /// <summary>Geeft het InvestigationResult van dit object terug (voor tracker-initialisatie).</summary>
        public InvestigationResult GetResult() => _result;

        /// <summary>Activeer of deactiveer dit object als interacteerbaar.</summary>
        public void SetInteractable(bool actief)
        {
            if (_col != null) _col.enabled = actief;
        }

        public override void OnInteract() => OnInspect();

        public void OnInspect()
        {
            bool nieuw = InvestigationTracker.Instance != null &&
                         InvestigationTracker.Instance.Register(_result);
            if (!nieuw) return;

            SBARManager.Instance?.Notebook.Add(category, finding);
            SBARManager.Instance?.subtitles?.Show("", finding, 3.5f);
            RefreshLabel(true);
        }

        private void RefreshLabel(bool gevonden)
        {
            if (labelText == null) return;
            labelText.text = gevonden
                ? $"{displayLabel}\n<color=#55CC66>✓ onderzocht</color>"
                : $"{displayLabel}\n[onderzoek: E / klik]";
        }
    }
}

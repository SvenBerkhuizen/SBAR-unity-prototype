using System;
using UnityEngine;

namespace SBAR.Interaction
{
    public class NPCInteractable : InteractableBase
    {
        [Tooltip("Weergavenaam van het personage (bijv. 'Collega' of 'Arts').")]
        public string personageNaam = "Collega";

        private Collider _collider;

        public event Action<NPCInteractable> Interacted;

        protected override void Awake()
        {
            base.Awake();
            _collider = GetComponent<Collider>();
        }

        public void SetInteractable(bool actief)
        {
            if (_collider != null) _collider.enabled = actief;
        }

        public override void OnInteract()
        {
            Interacted?.Invoke(this);
        }
    }
}

using UnityEngine;
using SBAR.UI;

namespace SBAR.Interaction
{
    /// <summary>
    /// Maakt een wereld-voorwerp (klembord) interactief: E/klik opent of sluit het
    /// notitieboekje-paneel. Werkt naast de N-toets (die blijft als snelkoppeling).
    /// </summary>
    public class NotebookProp : InteractableBase
    {
        [SerializeField] private NotebookUI notebookUI;

        public void SetNotebook(NotebookUI ui) => notebookUI = ui;

        public override void OnInteract()
        {
            notebookUI?.Toggle();
        }
    }
}

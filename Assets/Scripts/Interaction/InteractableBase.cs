using UnityEngine;
using TMPro;

namespace SBAR.Interaction
{
    [RequireComponent(typeof(Collider))]
    public abstract class InteractableBase : MonoBehaviour, IInteractable
    {
        [Header("Highlight")]
        [SerializeField] private Color highlightTint = new Color(1f, 0.95f, 0.4f);

        private Renderer _renderer;
        private MaterialPropertyBlock _mpb;
        private bool _hovered;
        private TMP_Text _hoverLabel;
        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
        private static readonly int LegacyColorId = Shader.PropertyToID("_Color");

        protected Color BaseColor { get; private set; } = Color.gray;
        protected Color IdleColor { get; set; } = Color.gray;

        /// <summary>Subklassen die hun label altijd zichtbaar willen (bv. notitieboek) zetten dit op false.</summary>
        protected virtual bool UseHoverLabel => true;

        protected virtual void Awake()
        {
            _renderer = GetComponentInChildren<Renderer>();
            _mpb = new MaterialPropertyBlock();
            if (_renderer != null && _renderer.sharedMaterial != null)
            {
                var mat = _renderer.sharedMaterial;
                if (mat.HasProperty(BaseColorId)) BaseColor = mat.GetColor(BaseColorId);
                else if (mat.HasProperty(LegacyColorId)) BaseColor = mat.GetColor(LegacyColorId);
            }
            IdleColor = BaseColor;
            ApplyColor(IdleColor);

            // Label (hint/waarde) start verborgen; alleen tonen bij hover → geen muur-clutter.
            if (UseHoverLabel)
            {
                _hoverLabel = GetComponentInChildren<TMP_Text>(true);
                if (_hoverLabel != null) _hoverLabel.gameObject.SetActive(false);
            }
        }

        public virtual void OnHoverEnter()
        {
            _hovered = true;
            if (_hoverLabel != null) _hoverLabel.gameObject.SetActive(true);
            Refresh();
        }

        public virtual void OnHoverExit()
        {
            _hovered = false;
            if (_hoverLabel != null) _hoverLabel.gameObject.SetActive(false);
            Refresh();
        }

        public abstract void OnInteract();

        protected void Refresh()
        {
            Color target = _hovered ? Color.Lerp(IdleColor, highlightTint, 0.6f) : IdleColor;
            ApplyColor(target);
        }

        private void ApplyColor(Color c)
        {
            if (_renderer == null) return;
            _renderer.GetPropertyBlock(_mpb);
            _mpb.SetColor(BaseColorId, c);
            _mpb.SetColor(LegacyColorId, c);
            _renderer.SetPropertyBlock(_mpb);
        }
    }
}

using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace SBAR.Interaction
{
    /// <summary>
    /// Maakt een prop (bijv. het klembord/notitieboek) grijpbaar in VR en laat het
    /// na loslaten soepel terugkeren naar zijn beginpositie, zodat het altijd op een
    /// vaste, vindbare plek terug ligt. Werkt met een kinematische Rigidbody.
    /// </summary>
    [RequireComponent(typeof(XRGrabInteractable))]
    public class GrabReturnToStart : MonoBehaviour
    {
        [Tooltip("Hoe snel het prop terugkeert na loslaten (hoger = sneller).")]
        [SerializeField] private float terugkeerSnelheid = 5f;

        private XRGrabInteractable _grab;
        private Vector3 _startPos;
        private Quaternion _startRot;
        private bool _keertTerug;

        private void Awake()
        {
            _grab = GetComponent<XRGrabInteractable>();
            _startPos = transform.position;
            _startRot = transform.rotation;
            _grab.selectEntered.AddListener(_ => _keertTerug = false);
            _grab.selectExited.AddListener(_ => _keertTerug = true);
        }

        private void Update()
        {
            if (!_keertTerug) return;

            transform.position = Vector3.Lerp(transform.position, _startPos, Time.deltaTime * terugkeerSnelheid);
            transform.rotation = Quaternion.Slerp(transform.rotation, _startRot, Time.deltaTime * terugkeerSnelheid);

            if (Vector3.Distance(transform.position, _startPos) < 0.002f)
            {
                transform.position = _startPos;
                transform.rotation = _startRot;
                _keertTerug = false;
            }
        }
    }
}

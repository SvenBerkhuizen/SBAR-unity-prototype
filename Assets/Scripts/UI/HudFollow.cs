using UnityEngine;

namespace SBAR.UI
{
    /// <summary>
    /// Houdt een world-space HUD comfortabel voor de speler in VR: volgt de
    /// horizontale kijkrichting (yaw) en positie, maar NIET pitch/roll. Daardoor
    /// kantelt het paneel niet mee als je omhoog/omlaag kijkt (geen clipping door
    /// vloer/tafel) en blijft het op vaste ooghoogte rechtop staan.
    /// </summary>
    public class HudFollow : MonoBehaviour
    {
        [Tooltip("De headset-camera. Leeg = Camera.main.")]
        public Transform cam;

        [Tooltip("Afstand vóór de speler (meter).")]
        public float distance = 1.1f;

        [Tooltip("Hoogte-offset t.o.v. de camera (meter, negatief = iets lager).")]
        public float heightOffset = -0.15f;

        [Tooltip("Volg-snelheid (hoger = strakker).")]
        public float smooth = 10f;

        private void Awake()
        {
            if (cam == null && Camera.main != null) cam = Camera.main.transform;
        }

        private void LateUpdate()
        {
            if (cam == null)
            {
                if (Camera.main == null) return;
                cam = Camera.main.transform;
            }

            // Horizontale kijkrichting (pitch eruit).
            Vector3 fwd = cam.forward;
            fwd.y = 0f;
            if (fwd.sqrMagnitude < 0.0001f) fwd = transform.forward;
            fwd.Normalize();

            Vector3 targetPos = cam.position + fwd * distance;
            targetPos.y = cam.position.y + heightOffset;

            transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime * smooth);

            // Paneel-voorzijde wijst van de camera weg (leesbare kant naar speler), rechtop.
            Vector3 lookDir = transform.position - cam.position;
            lookDir.y = 0f;
            if (lookDir.sqrMagnitude > 0.0001f)
            {
                Quaternion targetRot = Quaternion.LookRotation(lookDir, Vector3.up);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * smooth);
            }
        }
    }
}

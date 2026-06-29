using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using SBAR.Core;

namespace SBAR.Interaction
{
    public class MeasurementDevice : InteractableBase
    {
        [Header("Apparaat")]
        public string deviceNaam = "Apparaat";

        [SerializeField] private TMP_Text waardeLabel;

        [Header("Waarschuwing")]
        [SerializeField] private Color alarmKleur = new Color(1f, 0.5f, 0f);
        [SerializeField] private float knipperSnelheid = 2f;

        private readonly List<VitalReading> _readings = new List<VitalReading>();
        private Coroutine _blinkRoutine;
        private Collider _collider;
        private bool _alarm;

        public bool Gemeten { get; private set; }
        public IReadOnlyList<VitalReading> Readings => _readings;
        public event Action<MeasurementDevice> Measured;

        protected override void Awake()
        {
            base.Awake();
            _collider = GetComponent<Collider>();
            if (waardeLabel != null) waardeLabel.lineSpacing = 25f; // regels uit elkaar
            ShowName();
        }

        public void Configure(string naam, IEnumerable<VitalReading> readings)
        {
            deviceNaam = naam;
            _readings.Clear();
            if (readings != null) _readings.AddRange(readings);
            Gemeten = false;
            ShowName();
        }

        public void SetInteractable(bool actief)
        {
            if (_collider != null) _collider.enabled = actief;
        }

        public override void OnInteract()
        {
            bool eerste = !Gemeten;
            Gemeten = true;          // vóór StopAlarmBlink: voorkomt dat ShowName() de readings overschrijft
            StopAlarmBlink();
            ShowReadings();
            if (eerste) Measured?.Invoke(this);
        }

        public void StartAlarmBlink()
        {
            _alarm = true;
            if (!Gemeten) ShowName();
            if (_blinkRoutine == null)
                _blinkRoutine = StartCoroutine(BlinkRoutine());
        }

        public void StopAlarmBlink()
        {
            _alarm = false;
            if (_blinkRoutine != null)
            {
                StopCoroutine(_blinkRoutine);
                _blinkRoutine = null;
            }
            IdleColor = BaseColor;
            Refresh();
            if (!Gemeten) ShowName();
        }

        private IEnumerator BlinkRoutine()
        {
            while (true)
            {
                float t = Mathf.PingPong(Time.time * knipperSnelheid, 1f);
                IdleColor = Color.Lerp(BaseColor, alarmKleur, t);
                Refresh();
                yield return null;
            }
        }

        private void ShowName()
        {
            if (waardeLabel == null) return;
            // OE-D3: alarmstatus ook in tekst, niet alleen via knipperkleur.
            string alarm = _alarm ? "<b>⚠ ALARM — meet direct</b>\n" : "";
            waardeLabel.text = $"{alarm}<b>{deviceNaam}</b>\n\n[meet: E / klik]";
        }

        private void ShowReadings()
        {
            if (waardeLabel == null) return;
            var sb = new System.Text.StringBuilder();
            sb.AppendLine($"<b>{deviceNaam}</b>");
            sb.AppendLine();
            foreach (var r in _readings) sb.AppendLine(r.ToLine());
            waardeLabel.text = sb.ToString().TrimEnd();
        }
    }
}

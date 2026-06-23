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

        public bool Gemeten { get; private set; }
        public IReadOnlyList<VitalReading> Readings => _readings;
        public event Action<MeasurementDevice> Measured;

        protected override void Awake()
        {
            base.Awake();
            _collider = GetComponent<Collider>();
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
            ShowReadings();
            StopAlarmBlink();
            if (!Gemeten)
            {
                Gemeten = true;
                Measured?.Invoke(this);
            }
        }

        public void StartAlarmBlink()
        {
            if (_blinkRoutine == null)
                _blinkRoutine = StartCoroutine(BlinkRoutine());
        }

        public void StopAlarmBlink()
        {
            if (_blinkRoutine != null)
            {
                StopCoroutine(_blinkRoutine);
                _blinkRoutine = null;
            }
            IdleColor = BaseColor;
            Refresh();
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
            if (waardeLabel != null) waardeLabel.text = deviceNaam + "\n[meet: E / klik]";
        }

        private void ShowReadings()
        {
            if (waardeLabel == null) return;
            var sb = new System.Text.StringBuilder();
            sb.AppendLine(deviceNaam);
            foreach (var r in _readings) sb.AppendLine(r.ToLine());
            waardeLabel.text = sb.ToString().TrimEnd();
        }
    }
}

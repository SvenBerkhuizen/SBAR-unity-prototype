using System;
using System.Collections.Generic;
using UnityEngine;

namespace SBAR.Core
{
    public class InvestigationTracker : MonoBehaviour
    {
        public static InvestigationTracker Instance { get; private set; }

        private readonly List<InvestigationResult> _all   = new List<InvestigationResult>();
        private readonly HashSet<string>            _found = new HashSet<string>();

        public event Action<InvestigationResult> Registered;

        public int CriticalCount { get; private set; }
        public int CriticalFound { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        /// <summary>Laad alle mogelijke bevindingen voor de onderzoeksfase. Reset gevonden-lijst.</summary>
        public void Initialize(IEnumerable<InvestigationResult> allResults)
        {
            _all.Clear();
            _found.Clear();
            CriticalCount = 0;
            CriticalFound = 0;
            if (allResults == null) return;
            foreach (var r in allResults)
            {
                _all.Add(r);
                if (r.isCritical) CriticalCount++;
            }
        }

        /// <summary>Alleen gevonden-lijst resetten (bij herspelen).</summary>
        public void Reset()
        {
            _found.Clear();
            CriticalFound = 0;
        }

        /// <summary>Registreer een gevonden bevinding. Geeft true als het nieuw is.</summary>
        public bool Register(InvestigationResult result)
        {
            if (result == null || _found.Contains(result.id)) return false;
            _found.Add(result.id);
            if (result.isCritical) CriticalFound++;
            Registered?.Invoke(result);
            return true;
        }

        /// <summary>Alle bevindingen die de student heeft onderzocht.</summary>
        public List<InvestigationResult> GetFound() => _all.FindAll(r => _found.Contains(r.id));

        /// <summary>Alle bevindingen die de student heeft gemist.</summary>
        public List<InvestigationResult> GetMissed() => _all.FindAll(r => !_found.Contains(r.id));
    }
}

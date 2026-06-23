using System;

namespace SBAR.Core
{
    /// <summary>Bevinding die een student kan ontdekken tijdens de vrije onderzoeksfase.</summary>
    [Serializable]
    public class InvestigationResult
    {
        public string id;
        public SBARPart category;
        public string finding;
        public bool isCritical;

        public InvestigationResult(string id, SBARPart category, string finding, bool isCritical = false)
        {
            this.id       = id;
            this.category = category;
            this.finding  = finding;
            this.isCritical = isCritical;
        }
    }
}

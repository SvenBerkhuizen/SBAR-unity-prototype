using System;
using System.Collections.Generic;

namespace SBAR.Core
{
    [Serializable]
    public class SBARNotebook
    {
        public List<string> Situation { get; private set; } = new List<string>();
        public List<string> Background { get; private set; } = new List<string>();
        public List<string> Assessment { get; private set; } = new List<string>();
        public List<string> Recommendation { get; private set; } = new List<string>();

        public event Action Changed;

        public List<string> GetLines(SBARPart part)
        {
            switch (part)
            {
                case SBARPart.Situation: return Situation;
                case SBARPart.Background: return Background;
                case SBARPart.Assessment: return Assessment;
                case SBARPart.Recommendation: return Recommendation;
                default: return Situation;
            }
        }

        public bool Add(SBARPart part, string line)
        {
            if (string.IsNullOrWhiteSpace(line)) return false;
            var lines = GetLines(part);
            if (lines.Contains(line)) return false;
            lines.Add(line);
            Changed?.Invoke();
            return true;
        }

        public bool HasContent(SBARPart part)
        {
            return GetLines(part).Count > 0;
        }

        public void Clear()
        {
            Situation.Clear();
            Background.Clear();
            Assessment.Clear();
            Recommendation.Clear();
            Changed?.Invoke();
        }
    }
}

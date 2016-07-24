using System.Collections.Generic;

namespace Gardiner.XsltTools.ErrorList
{
    internal class AccessibilityResult
    {
        public string Url { get; set; }
        public string Project { get; set; }

        public IEnumerable<Rule> Violations { get; set; } = new List<Rule>();
        public IEnumerable<Rule> Passes { get; set; } = new List<Rule>();
    }
}
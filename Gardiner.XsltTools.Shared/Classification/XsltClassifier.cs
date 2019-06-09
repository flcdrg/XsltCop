using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.RegularExpressions;

using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;

namespace Gardiner.XsltTools.Classification
{
    internal class XsltClassifier : IClassifier
    {
        private readonly IClassificationType _attributeValue;

        public XsltClassifier(IClassificationType[] types)
        {
            _attributeValue = types[0];
        }

        internal static readonly Regex Regex = new Regex(@"<xsl:\w*\s+href=(['""])(?<path>[^'""]+)\1(\s*/>)?");

        //private static readonly Regex regex = new Regex(@"David");

        public IList<ClassificationSpan> GetClassificationSpans(SnapshotSpan span)
        {
            var list = new List<ClassificationSpan>();

            string text = span.GetText();

            var matches = Regex.Matches(text);
            foreach (Match match in matches)
            {
                var group = match.Groups["path"];
                //if (index == -1 || match.Index < index)
                {
                   
                    var result = new SnapshotSpan(span.Snapshot, span.Start + group.Index, group.Length);

                    Debug.WriteLine($"Adding classifier for {group.Value}");


                    list.Add(new ClassificationSpan(result, _attributeValue));
                }
            }

            return list;
        }

        public event EventHandler<ClassificationChangedEventArgs> ClassificationChanged;

        public virtual void OnClassificationChanged(ClassificationChangedEventArgs e)
        {
            ClassificationChanged?.Invoke(this, e);
        }
    }
}
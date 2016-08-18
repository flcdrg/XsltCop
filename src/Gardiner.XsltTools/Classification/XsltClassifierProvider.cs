using System.ComponentModel.Composition;

using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Utilities;

namespace Gardiner.XsltTools.Classification
{
    [Export(typeof(IClassifierProvider))]
    [ContentType("XSLT")]
    public class XsltClassifierProvider : IClassifierProvider
    {
        private readonly IClassificationType[] _types;

        [ImportingConstructor]
        public XsltClassifierProvider(IClassificationTypeRegistryService classificationTypeRegistryService)
        {
            _types = new[]
            {
                classificationTypeRegistryService.GetClassificationType(ClassificationTypeNames.XsltHrefAttributeName)
            };
        }

        public IClassifier GetClassifier(ITextBuffer textBuffer)
        {
            return textBuffer.Properties.GetOrCreateSingletonProperty(() => new XsltClassifier(_types));
        }
    }
}
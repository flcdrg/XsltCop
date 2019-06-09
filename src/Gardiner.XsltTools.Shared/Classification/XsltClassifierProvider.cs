using System;
using System.ComponentModel.Composition;

using JetBrains.Annotations;

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
        public XsltClassifierProvider([NotNull] IClassificationTypeRegistryService classificationTypeRegistryService)
        {
            if (classificationTypeRegistryService == null)
            {
                throw new ArgumentNullException(nameof(classificationTypeRegistryService));
            }

            _types = new[]
            {
                classificationTypeRegistryService.GetClassificationType(ClassificationTypeNames.XsltHrefAttributeName)
            };
        }

        public IClassifier GetClassifier([NotNull] ITextBuffer textBuffer)
        {
            if (textBuffer == null)
            {
                throw new ArgumentNullException(nameof(textBuffer));
            }

            return textBuffer.Properties.GetOrCreateSingletonProperty(() => new XsltClassifier(_types));
        }
    }
}
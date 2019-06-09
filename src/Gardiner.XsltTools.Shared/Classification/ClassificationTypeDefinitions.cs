using System.ComponentModel.Composition;
using System.Windows.Media;

using Microsoft.VisualStudio.Language.StandardClassification;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Utilities;

namespace Gardiner.XsltTools.Classification
{
    public class ClassificationTypeDefinitions
    {
#pragma warning disable CS0649
        [Export]
        [Name(ClassificationTypeNames.XsltHrefAttributeName)]
        [BaseDefinition(Microsoft.Language.Xml.ClassificationTypeNames.XmlAttributeValue)]
        internal readonly ClassificationTypeDefinition XslLiteralImportHrefTypeDefinition;

        [Export(typeof(EditorFormatDefinition))]
        [ClassificationType(ClassificationTypeNames = ClassificationTypeNames.XsltHrefAttributeName)]
        [Name(ClassificationTypeNames.XsltHrefAttributeName)]
        [Order(After = LanguagePriority.NaturalLanguage, Before = LanguagePriority.FormalLanguage)]
        //[Order( Before = Priority.Default)]
        [UserVisible(true)]
        private class XslLiteralImportHrefFormatDefinition : ClassificationFormatDefinition
        {
            private XslLiteralImportHrefFormatDefinition()
            {
                DisplayName = "XSLT Hyperlink";
                ForegroundColor = Colors.DarkRed; // HC_LIGHTBLUE
                TextDecorations = System.Windows.TextDecorations.Underline;
            }
        }
    }
}
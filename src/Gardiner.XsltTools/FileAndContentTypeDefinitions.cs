using System.ComponentModel.Composition;

using Microsoft.VisualStudio.Utilities;

namespace Gardiner.XsltTools
{
#pragma warning disable CS0649 // Fields get initialized by MEF
    internal static class FileAndContentTypeDefinitions
    {
        [Export]
        [Name("XSLT")]
        [BaseDefinition("xml")]
        internal static ContentTypeDefinition hidingContentTypeDefinition;

        [Export]
        [FileExtension(".xsl")]
        [ContentType("XSLT")]
        internal static FileExtensionToContentTypeDefinition hiddenFileExtensionDefinition;

        [Export]
        [FileExtension(".xslt")]
        [ContentType("XSLT")]
        internal static FileExtensionToContentTypeDefinition hiddenFileExtensionDefinition2;
    }
}
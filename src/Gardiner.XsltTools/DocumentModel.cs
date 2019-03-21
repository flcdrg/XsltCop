using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace Gardiner.XsltTools
{
    public sealed class DocumentModel
    {
        private XDocument _document;

        private List<XDocument> _dependencies;

        private DocumentModel(XDocument document)
        {
            _document = document;
        }

        public static DocumentModel FromFile(string filename)
        {
            if (!File.Exists(filename))
                return null;

            var text = File.ReadAllText(filename);

            var doc = XDocument.Parse(text, LoadOptions.SetLineInfo | LoadOptions.PreserveWhitespace);

            const string xsl = "http://www.w3.org/1999/XSL/Transform";

            // find imports and includes
            var variables =
                doc.Root.Descendants(XName.Get("variable", xsl)).Attributes("name").Select(a => a.Value).ToList();


            var model = new DocumentModel(doc);

            return model;
        }
    }
}
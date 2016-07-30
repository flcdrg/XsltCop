using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;

using CodePlex.XPathParser;

using Gardiner.XsltTools.ErrorList;

namespace Gardiner.XsltTools
{
    internal class XsltChecker
    {
        public AccessibilityResult CheckFile(string filename)
        {
            var violations = new List<Rule>();

            Debug.WriteLine(filename);
            var text = File.ReadAllText(filename);
            try
            {
                var doc = XDocument.Parse(text, LoadOptions.SetLineInfo | LoadOptions.PreserveWhitespace);

                const string xsl = "http://www.w3.org/1999/XSL/Transform";

                var variables =
                    doc.Root.Descendants(XName.Get("variable", xsl)).Attributes("name").Select(a => a.Value).ToList();

                // find parameters 
                var parameters =
                    doc.Root.Descendants(XName.Get("param", xsl))
                        .Attributes("name")
                        .Select(a => a.Value)
                        .Union(variables)
                        .ToList();

                // places to look
                var things =
                    doc.Root.Descendants().Attributes("select").Union(doc.Root.Descendants().Attributes("test")).ToList();

                foreach (var attribute in things)
                {
                    var lineInfo = (IXmlLineInfo)attribute;

                    try
                    {
                        var xpath = new XPathParser<XElement>().Parse(attribute.Value, new XPathTreeBuilder());

                        foreach (var parameter in parameters)
                        {
                            var notVariables =
                                xpath.Descendants()
                                    .Attributes("name")
                                    .Where(a => a.Parent.Name != "variable")
                                    .Select(x => x.Value)
                                    .ToList();

                            if (notVariables.Contains(parameter))
                            {
                                Debug.WriteLine($"\tWarning: {parameter} at ({lineInfo.LineNumber},{lineInfo.LinePosition})");

                                AddRule(filename, violations, lineInfo, $"{parameter} used without a $ prefix", "XSLT001");
                            }
                        }
                    }
                    catch (XPathParserException ex)
                    {
                        AddRule(filename, violations, lineInfo, $"Error parsing XPath expression: {ex.Message}",
                            "XSLT002");
                    }
                }
            }
            catch (XmlException)
            {
                // Must have been a weird XML document. Just ignore
            }


            return new AccessibilityResult
            {
                Project = "My Project",
                Url = new UriBuilder(filename).Uri.AbsolutePath.ToUpperInvariant(),
                Violations = violations
            };
        }

        private static void AddRule(string filename, ICollection<Rule> violations, IXmlLineInfo lineInfo, string description, string id)
        {
            violations.Add(new Rule
            {
                FileName = filename,
                Column = lineInfo.LinePosition,
                Line = lineInfo.LineNumber - 1,
                Impact = "moderate",
                Description = description,
                Id = id,
                Help = string.Empty,
                HelpUrl = string.Empty,
                Html = string.Empty
            });
        }
    }
}
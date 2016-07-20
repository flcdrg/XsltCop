using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using CodePlex.XPathParser;
using ConsoleApplication3;
using Gardiner.XsltTools.ErrorList;

namespace Gardiner.XsltTools
{
    class XsltChecker
    {
        public AccessibilityResult CheckFile(string filename)
        {
            var violations = new List<Rule>();

            Console.WriteLine(filename);
            var text = File.ReadAllText(filename);
            var doc = XDocument.Parse(text, LoadOptions.SetLineInfo | LoadOptions.PreserveWhitespace);

            const string xsl = "http://www.w3.org/1999/XSL/Transform";

            var variables = doc.Root.Descendants(XName.Get("variable", xsl)).Attributes("name").Select(a => a.Value).ToList();

            // find parameters 
            var parameters = doc.Root.Descendants(XName.Get("param", xsl)).Attributes("name").Select(a => a.Value).Union(variables).ToList();


            // places to look
            var things =
                doc.Root.Descendants().Attributes("select").Union(doc.Root.Descendants().Attributes("test")).ToList();

            foreach (var parameter in parameters)
            {
                foreach (var attribute in things)
                {
                    var xpath = new XPathParser<XElement>().Parse(attribute.Value, new XPathTreeBuilder());

                    var notVariables = xpath.Descendants().Attributes("name").Where(a => a.Parent.Name != "variable").Select(x => x.Value).ToList();

                    if (notVariables.Contains(parameter))
                    {
                        var lineInfo = (IXmlLineInfo)attribute;
                        Debug.WriteLine($"\tWarning: {parameter} at ({lineInfo.LineNumber},{lineInfo.LinePosition})");

                        violations.Add(new Rule()
                        {
                            FileName = filename, Column = lineInfo.LinePosition, Line = lineInfo.LineNumber - 1, Impact = "moderate", Description = $"{parameter} used without a $ prefix", Id = "XSLT001", Help = string.Empty, HelpUrl = string.Empty, Html = string.Empty
                        });
                    }
                }
            }

            return new AccessibilityResult()
            {
                Project = "My Project",
                Url = new UriBuilder(filename).Uri.AbsolutePath,
                Violations = violations
            };
        }
    }
}
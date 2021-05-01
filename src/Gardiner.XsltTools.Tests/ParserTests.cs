using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;

using Microsoft.Language.Xml;

using NUnit.Framework;

namespace Gardiner.XsltTools.Tests
{
    [TestFixture]
    public class ParserTests
    {
        [Test]
        public void Thing()
        {
            var localPath = Path.GetDirectoryName(new Uri(Assembly.GetExecutingAssembly().Location).LocalPath);
            var text = File.ReadAllText(Path.Combine(localPath, @"..\..\..\..\Demo\MyXslProject\Second.xslt"));
            XmlDocumentSyntax syntax = Parser.ParseText(text);
            //ClassifierVisitor.Visit(syntax, 0, text.Length, ResultCollector);

            GetChildren(syntax);
        }

        private void GetChildren(SyntaxNode node)
        {
            if (node.Kind == SyntaxKind.XmlName)
            {
                var name = (XmlNameSyntax) node;

                if (name.Name == "template")
                {
                    var mode = name.ParentElement.Attributes.FirstOrDefault(a => a.Key == "mode");
                    Debug.WriteLine(mode.Value);
                }
            }

            foreach (var childNode in node.ChildNodes)
            {
                GetChildren(childNode);
            }
        }

        private void ResultCollector(int i, int i1, SyntaxNode arg3, XmlClassificationTypes arg4)
        {
            if (arg3.Kind == SyntaxKind.XmlName)
            {
                var name = (XmlNameSyntax) arg3;

                if (name.Name == "template")
                {
                    var mode = name.ChildNodes
                        .Where(n => n.Kind == SyntaxKind.XmlAttribute)
                        .OfType<XmlAttributeSyntax>()
                        .FirstOrDefault(a => a.Name == "mode");

                    if (mode != null)
                    {
                        Debug.WriteLine(mode.Value);
                    }
                }
            }

            Debug.WriteLine($"{i}, {i1}, {arg3.GetType()} {arg3.Kind} {arg4}");
        }
    }
}
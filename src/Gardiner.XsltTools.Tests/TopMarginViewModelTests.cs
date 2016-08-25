using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;

using Gardiner.XsltTools.Margins;

using Microsoft.Language.Xml;

using NUnit.Framework;

namespace Gardiner.XsltTools.Tests
{
    [TestFixture]
    public class TopMarginViewModelTests
    {
        private readonly string _text;

        public TopMarginViewModelTests()
        {
            var localPath = Path.GetDirectoryName(new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath);
            _text = File.ReadAllText(Path.Combine(localPath, @"..\..\..\..\Demo\MyXslProject\Second.xslt"));
        }

        [Test]
        public void FindPosition()
        {
            var sut = new TopMarginViewModel();

            XmlDocumentSyntax syntax = Parser.ParseText(_text);

            var elts = TopMarginViewModel.GetDescendants(syntax).OfType<XmlElementSyntax>().Where(n => n.Name == "template");

            foreach (var elt in elts)
            {
                Debug.WriteLine($"{elt.Name} {elt.Kind} {elt.Start} {elt.Attributes.Where(a => a.Key == "mode").Select(a => a.Value).FirstOrDefault()}");
            }
            //var result = sut.FindPosition(syntax.Body, 600);
            //Debug.WriteLine($"Got result: {result}");
        }

        [Test]
        public void Recursing()
        {
            Recurse(Parser.ParseText(_text));
        }

        void Recurse(SyntaxNode node, int indent = 0)
        {
            Debug.WriteLine(new string(' ', indent) + $"\t{node.Kind}, {node.GetType()}, {node}");

            {
                foreach (var childNode in node.ChildNodes)
                {
                    Recurse(childNode, indent + 1);
                }
            }
        }

    }
}
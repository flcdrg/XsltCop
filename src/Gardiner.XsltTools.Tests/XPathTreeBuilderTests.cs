using System;
using System.Xml.Linq;

using ApprovalTests;
using ApprovalTests.Reporters;

using CodePlex.XPathParser;

using NUnit.Framework;

namespace Gardiner.XsltTools.Tests
{
    [TestFixture]
    [UseReporter(typeof(BeyondCompareReporter))]
    public class XPathTreeBuilderTests
    {
        [Test]
        public void AllFredElements()
        {
            var result = new XPathParser<XElement>().Parse("//fred", new XPathTreeBuilder());

            Approvals.VerifyXml(result.ToString());
        }

        [Test]
        public void MismatchedSquareBrackets()
        {
            Assert.Throws<XPathParserException>( () => new XPathParser<XElement>().Parse("[@attr='blah']", new XPathTreeBuilder()));
        }
    }
}

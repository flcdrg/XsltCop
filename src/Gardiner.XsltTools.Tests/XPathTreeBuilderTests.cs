using System;
using System.Xml.Linq;

using ApprovalTests;
using ApprovalTests.Reporters;

using CodePlex.XPathParser;

using NUnit.Framework;

namespace Gardiner.XsltTools.Tests
{
    [TestFixture]
    public class XPathTreeBuilderTests
    {
        [Test]
        [UseReporter(typeof(BeyondCompareReporter))]
        public void AllFredElements()
        {
            var result = new XPathParser<XElement>().Parse("//fred", new XPathTreeBuilder());

            Approvals.VerifyXml(result.ToString());
        }
    }
}

using Microsoft.XmlDiffPatch;
using System;
using System.Diagnostics;
using System.IO;
using System.Xml;
using Xunit;

namespace UnitTests
{
    public class UnitTest1
    {
        [Fact]
        public void Test1()
        {
            XmlDocument doc1 = new XmlDocument();
            doc1.LoadXml("<a></a>");
            XmlDocument doc2 = new XmlDocument();
            doc2.LoadXml("<a><b/></a>");

            XmlDiff diff = new XmlDiff();

            var sw = new StringWriter();
            var settings = new XmlWriterSettings() { OmitXmlDeclaration = true, Indent = false };
            using (var writer = XmlWriter.Create(sw, settings))
            {
                Assert.False(diff.Compare(doc1, doc2, writer));                
            }

            XmlDocument result = new XmlDocument();
            result.LoadXml(sw.ToString());

            var inner = result.DocumentElement.InnerXml; // avoid comparing the hash.
            var expected = "<xd:node match=\"1\" xmlns:xd=\"http://schemas.microsoft.com/xmltools/2002/xmldiff\"><xd:add><b /></xd:add></xd:node>";
            Assert.Equal(inner, expected);
        }
    }
}

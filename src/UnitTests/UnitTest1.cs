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
        public void SimpleAddition()
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

            var inner = ToComparibleString(result);
            var expected = "<xd:xmldiff version=\"1.0\" options=\"None\" fragments=\"no\" xmlns:xd=\"http://schemas.microsoft.com/xmltools/2002/xmldiff\"><xd:node match=\"1\"><xd:add><b /></xd:add></xd:node></xd:xmldiff>";
            Assert.Equal(inner, expected);
        }

        [Fact]
        public void SimpleFragment()
        {
            using (var col = new TempFileCollection())
            {
                var file1 = col.CreateTempFile("<a></a><b></b>");
                var file2 = col.CreateTempFile("<a></a><b></b><c></c>");

                XmlDiff diff = new XmlDiff();

                var sw = new StringWriter();
                var settings = new XmlWriterSettings() { OmitXmlDeclaration = true, Indent = false };
                using (var writer = XmlWriter.Create(sw, settings))
                {
                    Assert.False(diff.Compare(file1, file2, true, writer));
                }

                XmlDocument result = new XmlDocument();
                result.LoadXml(sw.ToString());

                var inner = ToComparibleString(result);
                var expected = "<xd:xmldiff version=\"1.0\" options=\"None\" fragments=\"yes\" xmlns:xd=\"http://schemas.microsoft.com/xmltools/2002/xmldiff\"><xd:node match=\"2\" /><xd:add><c /></xd:add></xd:xmldiff>";
                Assert.Equal(inner, expected);
            }
        }

        [Fact]
        public void SideBySideDiffView()
        {
            using (var col = new TempFileCollection())
            {
                var xmlSource = col.CreateTempFile("<root><a></a><b><temp/></b></root>");
                var xmlChanged = col.CreateTempFile("<root><a id='1'></a><b></b><c></c></root>");

                var view = new XmlDiffView();
                var options = XmlDiffOptions.IgnoreChildOrder | XmlDiffOptions.IgnoreWhitespace;
                var results = view.DifferencesSideBySideAsHtml(xmlSource, xmlChanged, false, options);

                var diff = results.ReadToEnd();
                Assert.Contains("<span class=\"remove\">&lt;temp</span>", diff);
                Assert.Contains("<span class=\"add\">id=\"1\"</span>", diff);
                Assert.Contains("<span class=\"add\">&lt;c</span>", diff);
            }
        }

        [Fact]
        public void RegressionMix()
        {
            using (var col = new TempFileCollection())
            {
                var xmlSource = col.CreateTempFile("<Profile><field><field>Approval_Workflow__c.Description__c</field></field><field><editable>false</editable><field>Approval_Workflow__c.Migration_Id__c</field><readable>false</readable></field><field><field>Approval_Workflow__c.Reject_PO_Substage__c</field></field><field><field>Location.VisitorAddressId</field></field><flow><flow>TestAccListApexFlow</flow></flow><layout><layout>Lead-LeadLayout</layout></layout><layout><layout>Macro-MacroLayout</layout></layout><layout><layout>OFAC__SDN_Match__c-OFAC__MatchLayout</layout></layout><layout><layout>OFAC__SDN_Search__c-OFAC__OFACRequestLayout</layout></layout><layout><layout>Oak_Office__c-OakOfficeLayout</layout></layout><objectPermissions><allowCreate>false</allowCreate><allowDelete>false</allowDelete><allowEdit>false</allowEdit><allowRead>true</allowRead><modifyAllRecords>false</modifyAllRecords></objectPermissions><objectPermissions><object>Asset</object><viewAllRecords>false</viewAllRecords></objectPermissions></Profile>");
                var xmlChanged = col.CreateTempFile("<Profile><field><field>Approval_Workflow__c.Description__c</field></field><field><editable>true</editable><field>Approval_Workflow__c.Migration_Id__c</field><readable>true</readable></field><field><field>Approval_Workflow__c.Reject_PO_Substage__c</field></field><field><field>Location.VisitorAddressId</field></field><layout><layout>Lead-LeadLayout</layout></layout><layout><layout>Location-LocationLayout</layout></layout><layout><layout>Macro-MacroLayout</layout></layout><layout><layout>Oak_Office__c-OakOfficeLayout</layout></layout><objectPermissions><allowCreate>false</allowCreate><allowDelete>false</allowDelete><allowEdit>false</allowEdit><allowRead>true</allowRead><modifyAllRecords>false</modifyAllRecords><object>Address</object><viewAllRecords>false</viewAllRecords></objectPermissions><objectPermissions><allowCreate>false</allowCreate><allowDelete>false</allowDelete><allowEdit>false</allowEdit><allowRead>true</allowRead><modifyAllRecords>false</modifyAllRecords><object>Asset</object><viewAllRecords>false</viewAllRecords></objectPermissions></Profile>");

                var view = new XmlDiffView();
                var options = XmlDiffOptions.IgnoreChildOrder | XmlDiffOptions.IgnoreWhitespace;
                var results = view.DifferencesSideBySideAsHtml(xmlSource, xmlChanged, false, options);

                var diff = results.ReadToEnd();
                Assert.Contains("<span class=\"remove\">&lt;temp</span>", diff);
                Assert.Contains("<span class=\"add\">id=\"1\"</span>", diff);
                Assert.Contains("<span class=\"add\">&lt;c</span>", diff);
            }
        }

        [Fact]
        public void SideBySideDiffViewCompact()
        {
            using (var col = new TempFileCollection())
            {
                var xmlSource = col.CreateTempFile("<root id='3'><apple></apple><banana><temp/></banana><elephant size='big'/></root>");
                var xmlChanged = col.CreateTempFile("<root id='3'><apple></apple><banana></banana><carrot/><elephant size='big'/></root>");

                var view = new XmlDiffView();
                var options = XmlDiffOptions.IgnoreChildOrder | XmlDiffOptions.IgnoreWhitespace;
                var results = view.DifferencesSideBySideAsHtml(xmlSource, xmlChanged, false, options, true);

                var diff = results.ReadToEnd();
                Assert.Contains("root", diff);
                Assert.Contains("banana", diff);
                Assert.Contains("&lt;temp", diff);
                Assert.DoesNotContain("apple", diff);
                Assert.DoesNotContain("elephant", diff);
            }
        }

        private string ToComparibleString(XmlDocument doc)
        {
            // avoid comparing the hash.
            var s = doc.OuterXml;
            int pos = s.IndexOf("srcDocHash");
            int end = s.IndexOf("\"", pos + 12) + 2;
            s = s.Substring(0, pos) + s.Substring(end);
            return s;
        }
    }
}

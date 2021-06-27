// Copyright (c)  Allan Nielsen.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Xunit;

namespace OurPresence.Modeller.Liquid.Tests.Tags.Html
{
    public class TableRowTests
    {
        [Fact]
        public void TestHtmlTable()
        {
            Helper.AssertTemplateResult(
                string.Format("<tr class=\"row1\">{0}<td class=\"col1\"> 1 </td><td class=\"col2\"> 2 </td><td class=\"col3\"> 3 </td></tr>{0}<tr class=\"row2\"><td class=\"col1\"> 4 </td><td class=\"col2\"> 5 </td><td class=\"col3\"> 6 </td></tr>{0}", Environment.NewLine),
                "{% tablerow n in numbers cols:3%} {{n}} {% endtablerow %}",
                Hash.FromAnonymousObject(new { numbers = new[] { 1, 2, 3, 4, 5, 6 } }));

            Helper.AssertTemplateResult(string.Format("<tr class=\"row1\">{0}</tr>{0}", Environment.NewLine),
                "{% tablerow n in numbers cols:3%} {{n}} {% endtablerow %}",
                Hash.FromAnonymousObject(new { numbers = new int[] { } }));
        }

        [Fact]
        public void TestHtmlTableWithDifferentCols()
        {
            Helper.AssertTemplateResult(
                string.Format("<tr class=\"row1\">{0}<td class=\"col1\"> 1 </td><td class=\"col2\"> 2 </td><td class=\"col3\"> 3 </td><td class=\"col4\"> 4 </td><td class=\"col5\"> 5 </td></tr>{0}<tr class=\"row2\"><td class=\"col1\"> 6 </td></tr>{0}", Environment.NewLine),
                "{% tablerow n in numbers cols:5%} {{n}} {% endtablerow %}",
                Hash.FromAnonymousObject(new { numbers = new[] { 1, 2, 3, 4, 5, 6 } }));
        }

        [Fact]
        public void TestHtmlColCounter()
        {
            Helper.AssertTemplateResult(
                string.Format("<tr class=\"row1\">{0}<td class=\"col1\">1</td><td class=\"col2\">2</td></tr>{0}<tr class=\"row2\"><td class=\"col1\">1</td><td class=\"col2\">2</td></tr>{0}<tr class=\"row3\"><td class=\"col1\">1</td><td class=\"col2\">2</td></tr>{0}", Environment.NewLine),
                "{% tablerow n in numbers cols:2%}{{tablerowloop.col}}{% endtablerow %}",
                Hash.FromAnonymousObject(new { numbers = new[] { 1, 2, 3, 4, 5, 6 } }));
        }

        [Fact]
        public void TestHtmlOffsetLimit()
        {
            Helper.AssertTemplateResult(
                string.Format("<tr class=\"row1\">{0}<td class=\"col1\">2</td><td class=\"col2\">3</td></tr>{0}<tr class=\"row2\"><td class=\"col1\">4</td></tr>{0}", Environment.NewLine),
                "{% tablerow n in numbers cols:2 offset:1 limit:3 %}{{n}}{% endtablerow %}",
                Hash.FromAnonymousObject(new { numbers = new[] { 1, 2, 3, 4, 5, 6 } }));
        }
    }
}

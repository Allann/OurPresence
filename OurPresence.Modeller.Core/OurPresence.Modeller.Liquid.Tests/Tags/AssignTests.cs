// Copyright (c)  Allan Nielsen.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Globalization;
using Xunit;

namespace OurPresence.Modeller.Liquid.Tests.Tags
{
    public class AssignTests
    {
        [Fact]
        public void TestAssignedVariable()
        {
            Helper.AssertTemplateResult(".foo.", "{% assign foo = values %}.{{ foo[0] }}.",
                Hash.FromAnonymousObject(new { values = new[] { "foo", "bar", "baz" } }));
            Helper.AssertTemplateResult(".bar.", "{% assign foo = values %}.{{ foo[1] }}.",
                Hash.FromAnonymousObject(new { values = new[] { "foo", "bar", "baz" } }));
        }

        [Fact]
        public void TestAssignDecimal()
        {
            Helper.AssertTemplateResult(string.Format("10{0}05", CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator),
                "{% assign foo = decimal %}{{ foo }}",
                Hash.FromAnonymousObject(new { @decimal = 10.05d }));
        }

        [Fact]
        public void TestAssignDecimalAndPlus()
        {
            Helper.AssertTemplateResult(
                expected: string.Format("20{0}05", CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator),
                template: "{% assign foo = decimal %}{% assign foo = foo | plus:10 %}{{ foo }}",
                localVariables: Hash.FromAnonymousObject(new { @decimal = 10.05d }));
            Helper.AssertTemplateResult(
                expected: string.Format("148397{0}77", CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator),
                template: "{% assign foo = decimal %}{% assign foo = foo | plus:10 %}{{ foo }}",
                localVariables: Hash.FromAnonymousObject(new { @decimal = 148387.77d }));
        }

        [Fact]
        public void TestAssignDoubleWithoutVariable()
        {
            Helper.AssertTemplateResult(string.Format("1{0}2345678912345", CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator),
                "{% assign foo = 1.2345678912345 %}{{ foo }}");
        }

        [Fact]
        public void TestAssignDoubleAndPlus()
        {
            Helper.AssertTemplateResult(string.Format("11{0}2345678912345", CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator),
                "{% assign foo = 1.2345678912345 %}{% assign foo = foo | plus:10 %}{{ foo }}");
        }

        [Fact]
        public void TestAssignDecimalInlineWithEnglishDecimalSeparator()
        {
            using (CultureHelper.SetCulture("en-GB"))
            {
                Helper.AssertTemplateResult(string.Format("2{0}5", CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator),
                    "{% assign foo = 2.5 %}{{ foo }}");
            }
        }

        [Fact]
        public void TestAssignDecimalInlineWithEnglishGroupSeparator()
        {
            using (CultureHelper.SetCulture("en-GB"))
            {
                Helper.AssertTemplateResult("2500",
                    "{% assign foo = 2,500 %}{{ foo }}");
            }
        }

        [Fact]
        public void TestAssignDecimalInlineWithFrenchDecimalSeparator()
        {
            using (CultureHelper.SetCulture("fr-FR"))
            {
                Helper.AssertTemplateResult(string.Format("2{0}5", CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator),
                    "{% assign foo = 2,5 %}{{ foo }}");
            }
        }

        [Fact]
        public void TestAssignDecimalInlineWithInvariantDecimalSeparatorInFrenchCulture()
        {
            using (CultureHelper.SetCulture("fr-FR"))
            {
                Helper.AssertTemplateResult(string.Format("2{0}5", CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator),
                    "{% assign foo = 2.5 %}{{ foo }}");
            }
        }

        [Fact]
        public void TestAssignWithFilter()
        {
            Helper.AssertTemplateResult(".bar.", "{% assign foo = values | split: ',' %}.{{ foo[1] }}.",
                Hash.FromAnonymousObject(new { values = "foo,bar,baz" }));
        }

        private class AssignDrop : Drop
        {
            public AssignDrop(Template template) : base(template)
            {
            }

            public string MyProperty
            {
                get { return "MyValue"; }
            }
        }

        [Fact]
        public void TestAssignWithDrop()
        {
            var template = new Template();

            Helper.AssertTemplateResult(".MyValue.", @"{% assign foo = value %}.{{ foo.my_property }}.",
                Hash.FromAnonymousObject(new { value = new AssignDrop(template) }));
        }
    }
}

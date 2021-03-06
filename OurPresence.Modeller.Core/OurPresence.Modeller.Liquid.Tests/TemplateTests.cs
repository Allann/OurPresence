// Copyright (c)  Allan Nielsen.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using FluentAssertions;
using System.Globalization;
using System.IO;
using System.Net;
using Xunit;

namespace OurPresence.Modeller.Liquid.Tests
{
    public class TemplateTests
    {
        [Fact]
        public void TestTokenizeStrings()
        {
            Template.Tokenize(" ").Should().BeEquivalentTo(new[] { " " });
            Template.Tokenize("hello world").Should().BeEquivalentTo(new[] { "hello world" });
        }

        [Fact]
        public void TestTokenizeVariables()
        {
            Template.Tokenize("{{funk}}").Should().BeEquivalentTo(new[] { "{{funk}}" });
            Template.Tokenize(" {{funk}} ").Should().BeEquivalentTo(new[] { " ", "{{funk}}", " " });
            Template.Tokenize(" {{funk}} {{so}} {{brother}} ").Should().BeEquivalentTo(new[] { " ", "{{funk}}", " ", "{{so}}", " ", "{{brother}}", " " });
            Template.Tokenize(" {{  funk  }} ").Should().BeEquivalentTo(new[] { " ", "{{  funk  }}", " " });
        }

        [Fact]
        public void TestTokenizeBlocks()
        {
            Template.Tokenize("{%comment%}").Should().BeEquivalentTo(new[] { "{%comment%}" });
            Template.Tokenize(" {%comment%} ").Should().BeEquivalentTo(new[] { " ", "{%comment%}", " " });
            
            Template.Tokenize(" {%comment%} {%endcomment%} ").Should().BeEquivalentTo(new[] { " ", "{%comment%}", " ", "{%endcomment%}", " " });
            Template.Tokenize("  {% comment %} {% endcomment %} ").Should().BeEquivalentTo(new[] { "  ", "{% comment %}", " ", "{% endcomment %}", " " });
        }

        [Fact]
        public void TestInstanceAssignsPersistOnSameTemplateObjectBetweenParses()
        {
            Template t = new Template();
            Assert.Equal("from instance assigns", t.ParseInternal("{% assign foo = 'from instance assigns' %}{{ foo }}").Render());
            Assert.Equal("from instance assigns", t.ParseInternal("{{ foo }}").Render());
        }

        [Fact]
        public void TestThreadSafeInstanceAssignsNotPersistOnSameTemplateObjectBetweenParses()
        {
            Template t = new Template();
            Assert.Equal("from instance assigns", t.ParseInternal("{% assign foo = 'from instance assigns' %}{{ foo }}").Render());
            Assert.Equal("", t.ParseInternal("{{ foo }}").Render());
        }

        [Fact]
        public void TestInstanceAssignsPersistOnSameTemplateParsingBetweenRenders()
        {
            Template t = Template.Parse("{{ foo }}{% assign foo = 'foo' %}{{ foo }}");
            Assert.Equal("foo", t.Render());
            Assert.Equal("foofoo", t.Render());
        }

        [Fact]
        public void TestThreadSafeInstanceAssignsNotPersistOnSameTemplateParsingBetweenRenders()
        {
            Template t = Template.Parse("{{ foo }}{% assign foo = 'foo' %}{{ foo }}");
            Assert.Equal("foo", t.Render());
            Assert.Equal("foo", t.Render());
        }

        [Fact]
        public void TestCustomAssignsDoNotPersistOnSameTemplate()
        {
            Template t = new Template();
            Assert.Equal("from custom assigns", t.ParseInternal("{{ foo }}").Render(Hash.FromAnonymousObject(new { foo = "from custom assigns" })));
            Assert.Equal("", t.ParseInternal("{{ foo }}").Render());
        }

        [Fact]
        public void TestCustomAssignsSquashInstanceAssigns()
        {
            Template t = new Template();
            Assert.Equal("from instance assigns", t.ParseInternal("{% assign foo = 'from instance assigns' %}{{ foo }}").Render());
            Assert.Equal("from custom assigns", t.ParseInternal("{{ foo }}").Render(Hash.FromAnonymousObject(new { foo = "from custom assigns" })));
        }

        [Fact]
        public void TestPersistentAssignsSquashInstanceAssigns()
        {
            Template t = new Template();
            Assert.Equal("from instance assigns",
                t.ParseInternal("{% assign foo = 'from instance assigns' %}{{ foo }}").Render());
            t.Assigns["foo"] = "from persistent assigns";
            Assert.Equal("from persistent assigns", t.ParseInternal("{{ foo }}").Render());
        }

        [Fact]
        public void TestLambdaIsCalledOnceFromPersistentAssignsOverMultipleParsesAndRenders()
        {
            Template t = new Template();
            int global = 0;
            t.Assigns["number"] = (Proc) (c => ++global);
            Assert.Equal("1", t.ParseInternal("{{number}}").Render());
            Assert.Equal("1", t.ParseInternal("{{number}}").Render());
            Assert.Equal("1", t.Render());
        }

        [Fact]
        public void TestLambdaIsCalledOnceFromCustomAssignsOverMultipleParsesAndRenders()
        {
            Template t = new Template();
            int global = 0;
            Hash assigns = Hash.FromAnonymousObject(new { number = (Proc) (c => ++global) });
            Assert.Equal("1", t.ParseInternal("{{number}}").Render(assigns));
            Assert.Equal("1", t.ParseInternal("{{number}}").Render(assigns));
            Assert.Equal("1", t.Render(assigns));
        }

        [Fact]
        public void TestErbLikeTrimmingLeadingWhitespace()
        {
            Template t = Template.Parse("foo\n\t  {%- if true %}hi tobi{% endif %}");
            Assert.Equal("foo\nhi tobi", t.Render());
        }

        [Fact]
        public void TestErbLikeTrimmingTrailingWhitespace()
        {
            Template t = Template.Parse("{% if true -%}\nhi tobi\n{% endif %}");
            Assert.Equal("hi tobi\n", t.Render());
        }

        [Fact]
        public void TestErbLikeTrimmingLeadingAndTrailingWhitespace()
        {
            Template t = Template.Parse(@"<ul>
{% for item in tasks -%}
    {%- if true -%}
    <li>{{ item }}</li>
    {%- endif -%}
{% endfor -%}
</ul>");
            Assert.Equal(@"<ul>
    <li>foo</li>
    <li>bar</li>
    <li>baz</li>
</ul>", t.Render(Hash.FromAnonymousObject(new { tasks = new [] { "foo", "bar", "baz" } })));
        }

        [Fact]
        public void TestRenderToStreamWriter()
        {
            Template template = Template.Parse("{{test}}");

            using (TextWriter writer = new StringWriter(CultureInfo.InvariantCulture))
            {
                template.Render(writer, new RenderParameters(CultureInfo.InvariantCulture) { LocalVariables = Hash.FromAnonymousObject(new { test = "worked" }) });

                Assert.Equal("worked", writer.ToString());
            }
        }

        [Fact]
        public void TestRenderToStream()
        {
            Template template = Template.Parse("{{test}}");

            var output = new MemoryStream();
            template.Render(output, new RenderParameters(CultureInfo.InvariantCulture) { LocalVariables = Hash.FromAnonymousObject(new { test = "worked" }) });

            output.Seek(0, SeekOrigin.Begin);

            using (TextReader reader = new StreamReader(output))
            {
                Assert.Equal("worked", reader.ReadToEnd());
            }
        }

        public class MySimpleType
        {
            public string Name { get; set; }

            public override string ToString()
            {
                return "Foo";
            }
        }

        [Fact]
        public void TestRegisterSimpleType()
        {
            var template = Template.Parse("{{context.Name}}");
            template.RegisterSafeType(typeof(MySimpleType), new[] { "Name" });

            var output = template.Render(Hash.FromAnonymousObject(new { context = new MySimpleType() { Name = "worked" } }));

            Assert.Equal("worked", output);
        }

        [Fact]
        public void TestRegisterSimpleTypeToString()
        {
            Template template = Template.Parse("{{context}}");
            template.RegisterSafeType(typeof(MySimpleType), new[] { "ToString" });

            var output = template.Render(Hash.FromAnonymousObject(new { context = new MySimpleType() }));

            // Doesn't automatically call ToString().
            Assert.Equal(string.Empty, output);
        }

        [Fact]
        public void TestRegisterSimpleTypeToStringWhenTransformReturnsComplexType()
        {
            Template template = Template.Parse("{{context}}");
            template.RegisterSafeType(typeof(MySimpleType), o =>
            {
                return o;
            });

            var output = template.Render(Hash.FromAnonymousObject(new { context = new MySimpleType() }));

            // Does automatically call ToString because Variable.Render calls ToString on objects during rendering.
            Assert.Equal("Foo", output);
        }

        [Fact]
        public void TestRegisterSimpleTypeTransformer()
        {
            Template template = Template.Parse("{{context}}");
            template.RegisterSafeType(typeof(MySimpleType), o => o.ToString());

            var output = template.Render(Hash.FromAnonymousObject(new { context = new MySimpleType() }));

            // Uses safe type transformer.
            Assert.Equal("Foo", output);
        }

        [Fact]
        public void TestRegisterRegisterSafeTypeWithValueTypeTransformer()
        {
            Template template = Template.Parse("{{context}}{{context.Name}}"); //
            template.RegisterSafeType(typeof(MySimpleType), new[] { "Name" }, m => m.ToString());

            var output = template.Render(Hash.FromAnonymousObject(new { context = new MySimpleType() { Name = "Bar" } }));

            // Uses safe type transformer.
            Assert.Equal("FooBar", output);
        }

        public class NestedMySimpleType
        {
            public string Name { get; set; }

            public NestedMySimpleType Nested { get; set; }

            public override string ToString()
            {
                return "Foo";
            }
        }

        [Fact]
        public void TestNestedRegisterRegisterSafeTypeWithValueTypeTransformer()
        {
            Template template = Template.Parse("{{context}}{{context.Name}} {{context.Nested}}{{context.Nested.Name}}"); //
            template.RegisterSafeType(typeof(NestedMySimpleType), new[] { "Name", "Nested" }, m => m.ToString());

            var inner = new NestedMySimpleType() { Name = "Bar2" };

            var output = template.Render(Hash.FromAnonymousObject(new { context = new NestedMySimpleType() { Nested = inner, Name = "Bar" } }));

            // Uses safe type transformer.
            Assert.Equal("FooBar FooBar2", output);
        }

        [Fact]
        public void TestOverrideDefaultBoolRenderingWithValueTypeTransformer()
        {
            Template template = Template.Parse("{{var1}} {{var2}}");
            template.RegisterValueTypeTransformer(typeof(bool), m => (bool)m ? "Win" : "Fail");

            var output = template.Render(Hash.FromAnonymousObject(new { var1 = true, var2 = false }));

            Assert.Equal("Win Fail", output);
        }

        [Fact]
        public void TestHtmlEncodingFilter()
        {
            Template template = Template.Parse("{{var1}} {{var2}}");
            template.RegisterValueTypeTransformer(typeof(string), m => WebUtility.HtmlEncode((string) m));

            var output = template.Render(Hash.FromAnonymousObject(new { var1 = "<html>", var2 = "Some <b>bold</b> text." }));

            Assert.Equal("&lt;html&gt; Some &lt;b&gt;bold&lt;/b&gt; text.", output);
        }

        public interface IMySimpleInterface2
        {
            string Name { get; }
        }

        public class MySimpleType2 : IMySimpleInterface2
        {
            public string Name { get; set; }
        }

        [Fact]
        public void TestRegisterSimpleTypeTransformIntoAnonymousType()
        {
            // specify a transform function
            Template template = Template.Parse("{{context.Name}}");
            template.RegisterSafeType(typeof(MySimpleType2), x => new { Name = ((MySimpleType2)x).Name } );

            var output = template.Render(Hash.FromAnonymousObject(new { context = new MySimpleType2 { Name = "worked" } }));

            Assert.Equal("worked", output);
        }

        [Fact]
        public void TestRegisterInterfaceTransformIntoAnonymousType()
        {
            // specify a transform function
            Template template = Template.Parse("{{context.Name}}");
            template.RegisterSafeType(typeof(IMySimpleInterface2), x => new { Name = ((IMySimpleInterface2) x).Name });

            var output = template.Render(Hash.FromAnonymousObject(new { context = new MySimpleType2 { Name = "worked" } }));

            Assert.Equal("worked", output);
        }

        public class MyUnsafeType2
        {
            public string Name { get; set; }
        }

        [Fact]
        public void TestRegisterSimpleTypeTransformIntoUnsafeType()
        {
            // specify a transform function
            Template template = Template.Parse("{{context.Name}}");
            template.RegisterSafeType(typeof(MySimpleType2), x => new MyUnsafeType2 { Name = ((MySimpleType2)x).Name });

            var output = template.Render(Hash.FromAnonymousObject(new { context = new MySimpleType2 { Name = "worked" } }));

            Assert.Equal("", output);
        }

        public interface MyGenericInterface<T>
        {
            T Value { get; set; }
        }

        public class MyGenericImpl<T> : MyGenericInterface<T>
        {
            public T Value { get; set; }
        }

        [Fact]
        public void TestRegisterGenericInterface()
        {
            Template template = Template.Parse("{{context.Value}}");
            template.RegisterSafeType(typeof(MyGenericInterface<>), new[] { "Value" });

            var output = template.Render(Hash.FromAnonymousObject(new { context = new MyGenericImpl<string> { Value = "worked" } }));

            Assert.Equal("worked", output);
        }

        [Fact]
        public void TestFirstAndLastOfObjectArray()
        {
            var template = new Template();
            template.RegisterSafeType(typeof(MySimpleType), new[] { "Name" });

            var array = new
            {
                People = new[] {
                    new MySimpleType { Name = "Jane" },
                    new MySimpleType { Name = "Mike" },
                }
            };

            Helper.AssertTemplateResult(
                expected: "Jane",
                template: "{{ People.first.Name }}",
                localVariables: Hash.FromAnonymousObject(array));

            Helper.AssertTemplateResult(
                expected: "Mike",
                template: "{{ People.last.Name }}",
                localVariables: Hash.FromAnonymousObject(array));
        }
    }
}

using Xunit;

namespace OurPresence.Modeller.Liquid.Tests.Tags
{
    public class RawTests
    {
        [Fact]
        public void TestTagInRaw ()
        {
            Helper.AssertTemplateResult ("{% comment %} test {% endcomment %}",
                "{% raw %}{% comment %} test {% endcomment %}{% endraw %}");
        }

        [Fact]
        public void TestOutputInRaw ()
        {
            Helper.AssertTemplateResult ("{{ test }}",
                "{% raw %}{{ test }}{% endraw %}");
        }
    }
}

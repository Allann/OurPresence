using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using OurPresence.Liquid.Exceptions;
using OurPresence.Liquid.Util;

namespace OurPresence.Liquid.Tags
{
    /// <summary>
    /// If is the conditional block
    ///
    /// {% if user.admin %}
    ///   Admin user!
    /// {% else %}
    ///   Not admin user
    /// {% endif %}
    ///
    ///  There are {% if count &lt; 5 %} less {% else %} more {% endif %} items than you need.
    /// </summary>
    public class If : OurPresence.Liquid.Block
    {
        private string SyntaxHelp = "Syntax Error in 'if' tag - Valid syntax: if [expression]";
        private string TooMuchConditionsHelp = "Syntax Error in 'if' tag - max 500 conditions are allowed";
        private static readonly Regex s_syntax = R.B(R.Q(@"({0})\s*([=!<>a-zA-Z_]+)?\s*({0})?"), Liquid.QuotedFragment);

        private static readonly string s_expressionsAndOperators = string.Format(R.Q(@"(?:\b(?:\s?and\s?|\s?or\s?)\b|(?:\s*(?!\b(?:\s?and\s?|\s?or\s?)\b)(?:{0}|\S+)\s*)+)"), Liquid.QuotedFragment);
        private static readonly Regex s_expressionsAndOperatorsRegex = R.C(s_expressionsAndOperators);

        protected List<Condition> Blocks { get; private set; }

        public override void Initialize(string tagName, string markup, List<string> tokens)
        {
            Blocks = new List<Condition>();
            PushBlock("if", markup);
            base.Initialize(tagName, markup, tokens);
        }

        public override void UnknownTag(string tag, string markup, List<string> tokens)
        {
            // Ruby version did not include "elseif", but I've added that to make it more C#-friendly.
            if (tag == "elsif" || tag == "elseif" || tag == "else")
                PushBlock(tag, markup);
            else
                base.UnknownTag(tag, markup, tokens);
        }

        public override void Render(Context context, TextWriter result)
        {
            context.Stack(() =>
            {
                foreach (var block in Blocks)
                {
                    if (block.Evaluate(context, result.FormatProvider))
                    {
                        RenderAll(block.Attachment, context, result);
                        return;
                    }
                }
            });
        }

        private void PushBlock(string tag, string markup)
        {
            Condition block;
            if (tag == "else")
            {
                block = new ElseCondition();
            }
            else
            {
                var expressions = R.Scan(markup, s_expressionsAndOperatorsRegex);

                // last item in list
                var syntax = expressions.TryGetAtIndexReverse(0);

                if (string.IsNullOrEmpty(syntax))
                    throw new SyntaxException(SyntaxHelp);
                var syntaxMatch = s_syntax.Match(syntax);
                if (!syntaxMatch.Success)
                    throw new SyntaxException(SyntaxHelp);

                var condition = new Condition(syntaxMatch.Groups[1].Value,
                    syntaxMatch.Groups[2].Value, syntaxMatch.Groups[3].Value);

                var conditionCount = 1;
                // continue to process remaining items in the list backwards, in pairs
                for (var i = 1; i < expressions.Count; i = i + 2)
                {
                    var @operator = expressions.TryGetAtIndexReverse(i).Trim();

                    var expressionMatch = s_syntax.Match(expressions.TryGetAtIndexReverse(i + 1));
                    if (!expressionMatch.Success)
                        throw new SyntaxException(SyntaxHelp);

                    if(++conditionCount > 500)
                    {
                        throw new SyntaxException(TooMuchConditionsHelp);
                    }

                    var newCondition = new Condition(expressionMatch.Groups[1].Value,
                        expressionMatch.Groups[2].Value, expressionMatch.Groups[3].Value);
                    switch (@operator)
                    {
                        case "and":
                            newCondition.And(condition);
                            break;
                        case "or":
                            newCondition.Or(condition);
                            break;
                    }
                    condition = newCondition;
                }
                block = condition;
            }

            Blocks.Add(block);
            NodeList = block.Attach(new List<object>());
        }
    }
}

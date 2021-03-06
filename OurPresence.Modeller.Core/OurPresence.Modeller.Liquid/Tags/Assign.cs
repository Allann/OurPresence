// Copyright (c)  Allan Nielsen.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using OurPresence.Modeller.Liquid.Exceptions;
using OurPresence.Modeller.Liquid.Util;

namespace OurPresence.Modeller.Liquid.Tags
{
    /// <summary>
    /// Assign sets a variable in your template.
    ///
    /// {% assign foo = 'monkey' %}
    ///
    /// You can then use the variable later in the page.
    ///
    /// {{ foo }}
    /// </summary>
    public class Assign : Tag
    {
        private static readonly Regex s_syntax = R.B(R.Q(@"({0}+)\s*=\s*(.*)\s*"), Liquid.VariableSignature);
        private string _to;
        private Variable _from;

        /// <summary>
        ///
        /// </summary>
        /// <param name="template"></param>
        /// <param name="tagName"></param>
        /// <param name="markup"></param>
        public Assign(Template template, string tagName, string markup)
            : base(template, tagName, markup)
        { }

        /// <summary>
        ///
        /// </summary>
        /// <param name="tokens"></param>
        public override void Initialize(IEnumerable<string> tokens)
        {
            var syntaxMatch = s_syntax.Match(Markup);
            if (syntaxMatch.Success)
            {
                _to = syntaxMatch.Groups[1].Value;
                _from = new Variable(Template, syntaxMatch.Groups[2].Value);
            }
            else
            {
                throw new SyntaxException(Liquid.ResourceManager.GetString("AssignTagSyntaxException"));
            }

            base.Initialize(tokens);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="context"></param>
        /// <param name="result"></param>
        public override void Render(Context context, TextWriter result)
        {
            context.Scopes.Last()[_to] = _from.Render(context);
        }
    }
}

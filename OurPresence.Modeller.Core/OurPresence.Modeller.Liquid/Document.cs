﻿// Copyright (c)  Allan Nielsen.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.IO;
using OurPresence.Modeller.Liquid.Exceptions;

namespace OurPresence.Modeller.Liquid
{
    /// <summary>
    /// Represents the Liquid template
    /// </summary>
    public class Document : Block
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="template"></param>
        /// <param name="tagName"></param>
        /// <param name="markup"></param>
        public Document(Template template, string tagName, string markup)
            :base(template, tagName, markup)
        { }

        /// <summary>
        /// We don't need markup to open this block
        /// </summary>
        /// <param name="tokens"></param>
        public override void Initialize(IEnumerable<string> tokens)
        {
            Parse(tokens);
        }

        /// <summary>
        /// There isn't a real delimiter
        /// </summary>
        protected override string BlockDelimiter
        {
            get { return string.Empty; }
        }

        /// <summary>
        /// Document blocks don't need to be terminated since they are not actually opened
        /// </summary>
        protected override void AssertMissingDelimitation()
        { }

        /// <summary>
        /// Renders the Document
        /// </summary>
        /// <param name="context"></param>
        /// <param name="result"></param>
        public override void Render(Context context, TextWriter result)
        {
            try
            {
                base.Render(context, result);
            }
            catch (BreakInterrupt)
            {
                // BreakInterrupt exceptions are used to interrupt a rendering
            }
            catch (ContinueInterrupt)
            {
                // ContinueInterrupt exceptions are used to interrupt a rendering
            }
        }
    }
}

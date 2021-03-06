﻿// Copyright (c)  Allan Nielsen.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace OurPresence.Modeller.Liquid
{
    /// <summary>
    /// Tag factory using System.Activator to instanciate the tag.
    /// </summary>
    public class ActivatorTagFactory : ITagFactory
    {
        private readonly Type _tagType;
        private readonly string _tagName;

        /// <summary>
        /// Instanciates a new ActivatorTagFactory
        /// </summary>
        /// <param name="tagType">Name of the tag</param>
        /// <param name="tagName">Type of the tag. must inherit from OurPresence.Modeller.Liquid.Tag.</param>
        public ActivatorTagFactory(Type tagType, string tagName)
        {
            _tagType = tagType;
            _tagName = tagName;
        }

        /// <summary>
        ///
        /// </summary>
        public string TagName => _tagName;

        /// <summary>
        /// Creates the tag
        /// </summary>
        /// <returns></returns>
        public Tag Create(Template template,string markup)
        {
            return Activator.CreateInstance(_tagType, template, _tagName, markup) as Tag;
        }
    }
}

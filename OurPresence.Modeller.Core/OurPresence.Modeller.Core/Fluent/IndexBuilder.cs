﻿using System;
using System.ComponentModel;

namespace OurPresence.Modeller.Fluent
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class IndexBuilder : FluentBase
    {
        public IndexBuilder(ModelBuilder model, Domain.Index index)
        {
            Build = model ?? throw new ArgumentNullException(nameof(model));
            Instance = index;
        }

        public ModuleBuilder ModuleBuild { get; }
        public ModelBuilder Build { get; }
        public Domain.Index Instance { get; }

        public IndexBuilder IsClustered()
        {
            Instance.IsClustered = true;
            return this;
        }

        public IndexBuilder IsUnique()
        {
            Instance.IsUnique = true;
            return this;
        }

        public IndexFieldBuilder AddField(string name)
        {
            var field = Fluent.IndexField.Create(this, name);
            Instance.Fields.Add(field.Instance);
            return field;
        }
    }
}

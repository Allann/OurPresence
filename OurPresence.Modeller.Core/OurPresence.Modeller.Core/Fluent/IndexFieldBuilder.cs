﻿// Copyright (c)  Allan Nielsen.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.ComponentModel;

namespace OurPresence.Modeller.Fluent
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class IndexFieldBuilder
    {
        public IndexFieldBuilder(IndexBuilder keyBuilder, Domain.IndexField field)
        {
            Build = keyBuilder ?? throw new ArgumentNullException(nameof(keyBuilder));
            Instance = field ?? throw new ArgumentNullException(nameof(field));
        }

        public IndexBuilder Build { get; }

        public Domain.IndexField Instance { get; }

        public IndexFieldBuilder Sort(ListSortDirection sort)
        {
            Instance.Sort = sort;
            return this;
        }
    }
}

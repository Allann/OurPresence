﻿using System;

namespace OurPresence.Modeller.Fluent
{
    public static class Index
    {
        public static IndexBuilder Create(ModelBuilder model, string name)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("message", nameof(name));
            var index = new Domain.Index(name);
            return new IndexBuilder(model, index);
        }
    }
}

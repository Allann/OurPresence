﻿// Copyright (c)  Allan Nielsen.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using OurPresence.Modeller.Generator;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace OurPresence.Modeller.Tests.TestFiles
{
    public class SimpleTestGeneratorDetails : MetadataBase
    {
        public override string Name => "SimpleTestGenerator";
        public override string Description => "A simple test generator";
        public override Type EntryPoint => typeof(SimpleTestGenerator);
        public override IEnumerable<Type> SubGenerators => new Collection<Type>();
    }
}

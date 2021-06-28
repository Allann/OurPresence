﻿using OurPresence.Modeller.Generator;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace BusinessLogicBehaviour
{
    public class GeneratorDetails : MetadataBase
    {
        public GeneratorDetails() : base("1.0.0")
        { }

        public override string Name => "Business Logic Behaviour";

        public override string Description => "Build a Business Logic Behaviour file group";

        public override Type EntryPoint => typeof(Generator);

        public override IEnumerable<Type> SubGenerators => new Collection<Type>() { typeof(Property.Generator), typeof(Header.Generator) };
    }
}
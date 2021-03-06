﻿// Copyright (c)  Allan Nielsen.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Text;
using OurPresence.Modeller.Domain;
using OurPresence.Modeller.Domain.Extensions;
using OurPresence.Modeller.Generator;
using OurPresence.Modeller.Interfaces;

namespace EntityFrameworkClass
{
    public class ConfigGenerated : IGenerator
    {
        private readonly Module _module;
        private readonly Model _model;

        public ConfigGenerated(ISettings settings, Module module, Model model)
        {
            Settings = settings ?? throw new ArgumentNullException(nameof(settings));
            _module = module ?? throw new ArgumentNullException(nameof(module));
            _model = model ?? throw new ArgumentNullException(nameof(module));
        }

        public IOutput Create()
        {
            var sb = new StringBuilder();
            sb.Al($"namespace {_module.Namespace}.Data.Configurations");
            sb.Al("{");
            sb.I(1).Al($"public partial class {_model.Name}Configuration : IEntityTypeConfiguration<{_model.Name}>");
            sb.I(1).Al("{");
            sb.I(2).Al($"void IEntityTypeConfiguration<{_model.Name}>.Configure(EntityTypeBuilder<{_model.Name}> builder)");
            sb.I(2).Al("{");
            sb.I(3).Al($"builder.ToTable(\"{_model.Name}\");");

            foreach (var field in _model.Fields)
            {
                sb.I(3).A($"builder.Property(p => p.{field.Name})");
                if (!field.Nullable)
                {
                    sb.B();
                    sb.I(4).A($".IsRequired()");
                }
                if (field.MaxLength.HasValue)
                {
                    sb.B();
                    sb.I(4).A($".HasMaxLength({field.MaxLength.Value})");
                }
                if (!string.IsNullOrWhiteSpace(field.Default))
                {
                    sb.B();
                    sb.I(4).A($".HasDefaultValueSql(\"{field.Default}\")");
                }
                sb.Al(";");
            }
            if (_model.HasBusinessKey() != null)
            {
                sb.I(3).A($"builder.HasIndex(i => i.{_model.HasBusinessKey()?.Name})");
                sb.I(4).A($".IsUnique()");
                sb.I(4).A($".IsClustered(); ");
            }

            if (Settings.SupportRegen)
            {
                sb.I(3).Al("OnConfigurePartial(builder);");
                sb.I(2).Al("}");
                sb.I(2).Al($"partial void OnConfigurePartial(EntityTypeBuilder<{_model.Name}> builder);");
            }
            else
            {
                sb.I(2).Al("}");
            }
            sb.I(1).Al("}");
            sb.Al("}");

            return new File($"{_model.Name}Configuration.cs", path:"Configurations", content: sb.ToString(), canOverwrite:Settings.SupportRegen);
        }

        public ISettings Settings { get; }
    }
}

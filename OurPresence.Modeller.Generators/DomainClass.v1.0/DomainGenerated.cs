﻿using OurPresence.Modeller.Domain;
using OurPresence.Modeller.Domain.Extensions;
using OurPresence.Modeller.Generator;
using OurPresence.Modeller.Interfaces;
using System;
using System.Linq;
using System.Text;

namespace DomainClass
{
    public class DomainGenerated : IGenerator
    {
        private readonly Module _module;
        private readonly Model _model;

        public DomainGenerated(ISettings settings, Module module, Model model)
        {
            Settings = settings ?? throw new ArgumentNullException(nameof(settings));
            _module = module ?? throw new ArgumentNullException(nameof(module));
            _model = model ?? throw new ArgumentNullException(nameof(model));
        }

        public IOutput Create()
        {
            var bk = _model.HasBusinessKey();
            var isEntity = _model.IsEntity();

            var sb = new StringBuilder();
            if (Settings.SupportRegen)
            {
                sb.al(((ISnippet)new OverwriteHeader.Generator(Settings, new GeneratorDetails()).Create()).Content);
            }
            else
            {
                sb.al(((ISnippet)new Header.Generator(Settings, new GeneratorDetails()).Create()).Content);
            }
            sb.al("using System;");
            sb.al("using OurPresence.Core.Models;");
            sb.b();
            sb.al($"namespace {_module.Namespace}");
            sb.al("{");

            if (Settings.SupportRegen)
            {
                sb.i(1).a($"partial class {_model.Name}");
            }
            else
            {
                sb.i(1).a($"public class {_model.Name}");
            }
            var entity = string.Empty;
            if (isEntity)
            {
                entity += $" : BaseAggregateRoot<{_model.Name}, ";
                if(_model.Key.Fields.Count==1)
                {
                    entity += $"{_model.Key.Fields.First().GetDataType()}>";
                }
                else
                {
                    entity += "{ ";
                    foreach (var item in _model.Key.Fields)
                    {
                        entity += item.GetDataType() + ",";
                    }
                    entity.TrimEnd(',');
                    entity += " }>;";
                }
            }
            sb.al(entity);
            sb.i(1).al("{");

            if (!Settings.SupportRegen)
            {
                sb.i(2).a($"public {_model.Name}(");
                foreach (var field in _model.Fields)
                {
                    sb.a($"{field.GetDataType()} {field.Name.Singular.LocalVariable}, ");
                }
                sb.TrimEnd(", ");
                sb.al(")");
                sb.i(2).al("{");
                foreach (var field in _model.Fields.Where(f => !f.Nullable && f.DataType == DataTypes.String))
                {
                    sb.i(3).al($"if(string.IsNullOrWhiteSpace({field.Name.Singular.LocalVariable}))");
                    sb.i(4).al($"throw new ArgumentException(\"Must include a value for {field.Name.Singular.Display}\");");
                }

                foreach (var field in _model.Fields)
                {
                    sb.i(3).al($"{field.Name.Value} = {field.Name.Singular.LocalVariable};");
                }
                sb.i(2).al("}");
                sb.b();
            }

            foreach (var item in _model.Key.Fields)
            {
                if (isEntity && item.Name.ToString() == "Id")
                    continue;
                var property = (ISnippet)new Property.Generator(item, setScope: Property.PropertyScope.@private).Create();
                sb.al(property.Content);
            }
            foreach (var item in _model.Fields)
            {
                var property = (ISnippet)new Property.Generator(item, setScope: Property.PropertyScope.@private).Create();
                sb.al(property.Content);
            }

            foreach (var item in _model.Behaviours)
            {
                if (Settings.SupportRegen)
                {
                    sb.i(2).a($"public partial void {item.Name}(");
                    foreach (var field in item.Fields)
                    {
                        sb.a($"{field.GetDataType()} {field.Name.Singular.LocalVariable}, ");
                    }
                    sb.TrimEnd(", ");
                    sb.al(");");
                }
                else
                {
                    sb.i(2).a($"public void {item.Name}(");
                    foreach (var field in item.Fields)
                    {
                        sb.a($"{field.GetDataType()} {field.Name.Singular.LocalVariable}, ");
                    }
                    sb.TrimEnd(", ");
                    sb.al(")");
                    sb.i(2).al("{");
                    sb.i(3).al($"// todo: Add {item.Name.Singular.Display} behaviour here");
                    sb.i(2).al("}");
                }
                sb.b();
            }

            if (!Settings.SupportRegen)
            {
                sb.i(2).al($"protected override void Apply(IDomainEvent<Guid> @event)");
                sb.i(2).al("{");
                sb.i(3).al($"// todo: Apply events");
                sb.i(2).al("}");
            }

            sb.TrimEnd(Environment.NewLine);
            sb.b();

            sb.i(1).al("}");
            sb.al("}");

            var filename = _model.Name.ToString();
            if (Settings.SupportRegen)
            {
                filename += ".generated";
            }
            filename += ".cs";

            return new File(filename, sb.ToString(), path: "Domain", canOverwrite: Settings.SupportRegen);
        }

        public ISettings Settings { get; }
    }
}
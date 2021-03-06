﻿// Copyright (c)  Allan Nielsen.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using FluentValidation.Results;
using OurPresence.Modeller.Generator.Validators;
using System.Linq;
using OurPresence.Modeller.Interfaces;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace OurPresence.Modeller.Generator
{
    public class Context : IContext
    {
        //private readonly IGeneratorConfiguration _configuration;
        private readonly ILoader<ISettings> _settingsLoader;
        private readonly ILoader<IEnumerable<INamedElement>> _moduleLoader;
        private readonly ILoader<IEnumerable<IGeneratorItem>> _generatorLoader;
        private readonly IPackageService _packageService;
        private readonly ILogger<IContext> _logger;

        private INamedElement? _module;
        private IGeneratorItem? _generator;
        private ISettings _settings = null!;
        private INamedElement? _model;

        public Context(
            ILoader<ISettings> settingsLoader,
            ILoader<IEnumerable<INamedElement>> moduleLoader,
            ILoader<IEnumerable<IGeneratorItem>> generatorLoader,
            IPackageService packageService,
            ILogger<Context> logger)
        {
            _settingsLoader = settingsLoader;
            _moduleLoader = moduleLoader;
            _generatorLoader = generatorLoader;
            _packageService = packageService;
            _logger = logger;
        }

        public IGeneratorItem? Generator => _generator;

        public ISettings Settings => _settings;

        public INamedElement? Module => _module;

        public INamedElement? Model => _model;

        private ISettings GetSettings(IGeneratorConfiguration configuration)
        {
            if(_settingsLoader.TryLoad(configuration.SettingsFile, out var settings))
            {
                _settings = settings;
                _logger.LogInformation("Registered {Packages} packages", _settings.Packages.Count.ToString());
            }
            return _settings;
        }

        private IGeneratorItem? GetGenerator(IGeneratorConfiguration configuration)
        {
            var generators = _generatorLoader.Load(configuration.LocalFolder);
            var name = configuration.GeneratorName.ToLowerInvariant();
            if (string.IsNullOrWhiteSpace(name))
            {
                return null;
            }

            var matches = generators.Where(g =>
                    g.Metadata.Name.ToLowerInvariant() == name || g.AbbreviatedFileName.ToLowerInvariant() == name)
                .ToList();
            var exact = matches.SingleOrDefault(m => m.Metadata.Version == configuration.Version);
            var item = exact ?? matches.OrderByDescending(k => k.Metadata.Version).FirstOrDefault();

            _logger.LogInformation("Generator Loaded: {S}",
                exact is null ? matches.Count() + " matches found" : "Exact match found");

            return item;
        }

        public override string ToString()
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine(_settings.ToString());
            return sb.ToString();
        }

        public ValidationResult ValidateConfiguration(IGeneratorConfiguration configuration)
        {
            var result = new ValidationResult();

            _settings = GetSettings(configuration);
            if(_settings==null)
            {
                _settings = new Settings(configuration, _packageService);                
            }
            _generator = GetGenerator(configuration);
            if (!string.IsNullOrEmpty(_settings.SourceModel))
            {
                var items = _moduleLoader.Load(_settings.SourceModel).ToList();
                if (!items.Any())
                {
                    result.Errors.Add(new ValidationFailure("SourceModel", "No module found in the source model."));
                }
                else if (items.Count() == 1)
                {
                    _module = items.First();
                }
                else
                {
                    result.Errors.Add(new ValidationFailure("SourceModel", "More than one module was found."));
                }
            }

            if (_module != null && string.IsNullOrEmpty(_settings.ModelName))
                _model = ((Domain.Module) _module).Models.FirstOrDefault(m => m.Name.Value == configuration.ModelName);

            var configValidator = new ContextValidator();
            var results = configValidator.Validate(this);
            if (!results.IsValid) return result;
            foreach (var item in results.Errors)
            {
                _logger.LogError("{Error}",item.ErrorMessage);
                result.Errors.Add(item);
            }

            return result;
        }
    }
}

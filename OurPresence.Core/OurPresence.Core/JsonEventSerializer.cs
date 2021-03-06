﻿// Copyright (c)  Allan Nielsen.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using OurPresence.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace OurPresence.Core
{
    public class JsonEventSerializer : IEventSerializer
    {
        private readonly IEnumerable<Assembly> _assemblies;

        private static readonly Newtonsoft.Json.JsonSerializerSettings s_jsonSerializerSettings = new()
        {
            ConstructorHandling = Newtonsoft.Json.ConstructorHandling.AllowNonPublicDefaultConstructor,
            ContractResolver = new PrivateSetterContractResolver()
        };

        public JsonEventSerializer(IEnumerable<Assembly> assemblies)
        {
            _assemblies = assemblies ?? new[] { Assembly.GetExecutingAssembly() };
        }

        public IDomainEvent<TKey> Deserialize<TKey>(string type, byte[] data)
        {
            var jsonData = Encoding.UTF8.GetString(data);
            return this.Deserialize<TKey>(type, jsonData);
        }

        public IDomainEvent<TKey> Deserialize<TKey>(string type, string data)
        {
            //TODO: cache types
            var eventType = _assemblies.Select(a => a.GetType(type, false))
                                .FirstOrDefault(t => t != null) ?? Type.GetType(type);
            if (null == eventType)
                throw new ArgumentOutOfRangeException(nameof(type), $"invalid event type: {type}");

            // as of 01/10/2020, "Deserialization to reference types without a parameterless constructor isn't supported."
            // https://docs.microsoft.com/en-us/dotnet/standard/serialization/system-text-json-how-to
            // apparently it's being worked on: https://github.com/dotnet/runtime/issues/29895

            var result = Newtonsoft.Json.JsonConvert.DeserializeObject(data, eventType, s_jsonSerializerSettings);

            return (IDomainEvent<TKey>)result;
        }

        public byte[] Serialize<TKey>(IDomainEvent<TKey> @event)
        {
            var json = System.Text.Json.JsonSerializer.Serialize((dynamic)@event);
            var data = Encoding.UTF8.GetBytes(json);
            return data;
        }
    }
}

﻿// Copyright (c)  Allan Nielsen.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace OurPresence.Modeller.Domain.Extensions
{
    public static class JsonExtensions
    {
        private static JsonSerializerSettings GetSettings(bool includeNull = true)
        {
            return new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
                Converters = new JsonConverter[] { new StringEnumConverter(), new VersionConverter(), new NameConverter() },
                NullValueHandling = includeNull ? NullValueHandling.Include : NullValueHandling.Ignore,
                ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor
            };
        }

        public static string ToJson<T>(this T obj, bool includeNull = true)
        {
            var settings = GetSettings(includeNull);
            return JsonConvert.SerializeObject(obj, settings);
        }

        public static T? FromJson<T>(this string json)
        {
            var settings = GetSettings();
            return JsonConvert.DeserializeObject<T>(json, settings);
        }
    }

}

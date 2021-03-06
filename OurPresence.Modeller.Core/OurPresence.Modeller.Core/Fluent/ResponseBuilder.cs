﻿// Copyright (c)  Allan Nielsen.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using OurPresence.Modeller.Domain;
using System;
using System.ComponentModel;

namespace OurPresence.Modeller.Fluent
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class ResponseBuilder : FluentBase
    {
        public ResponseBuilder(RequestBuilder request, Response response)
        {
            Build = request ?? throw new ArgumentNullException(nameof(request));
            Instance = response ?? throw new ArgumentNullException(nameof(response));
        }

        public RequestBuilder Build { get; }

        public Response Instance { get; }
        
        public FieldBuilder<ResponseBuilder> AddField(string name)
        {
            var field = Fluent.Field<ResponseBuilder>.Create(this, name);
            Instance.Fields.Add(field.Instance);
            return field;
        }
    }
}

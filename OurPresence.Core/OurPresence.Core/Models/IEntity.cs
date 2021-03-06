﻿// Copyright (c)  Allan Nielsen.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace OurPresence.Core.Models
{
    public interface IEntity<out TKey>
    {
        TKey Id { get; }
    }
}

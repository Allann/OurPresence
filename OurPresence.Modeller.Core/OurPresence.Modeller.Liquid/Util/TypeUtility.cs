// Copyright (c)  Allan Nielsen.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace OurPresence.Modeller.Liquid.Util
{
    internal static class TypeUtility
    {
        private const TypeAttributes AnonymousTypeAttributes = TypeAttributes.NotPublic;

        public static bool IsAnonymousType(Type t)
        {
            
            return t.GetTypeInfo().GetCustomAttribute<CompilerGeneratedAttribute>() != null
                && t.GetTypeInfo().IsGenericType
                    && (t.Name.Contains("AnonymousType") || t.Name.Contains("AnonType"))
                        && (t.Name.StartsWith("<>") || t.Name.StartsWith("VB$"))
                            && (t.GetTypeInfo().Attributes & AnonymousTypeAttributes) == AnonymousTypeAttributes;
        }
    }
}

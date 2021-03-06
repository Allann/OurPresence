﻿// Copyright (c)  Allan Nielsen.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using OurPresence.Modeller.Liquid.Util;

namespace OurPresence.Modeller.Liquid
{
    /// <summary>
    ///     Configurable typing metadata collection
    /// </summary>
    internal class TypeResolution
    {
        public Dictionary<string, MethodInfo> CachedMethods { get; private set; }

        public Dictionary<string, PropertyInfo> CachedProperties { get; private set; }
        public Template Template { get; }

        public TypeResolution(Template template, Type type, Func<MemberInfo, bool> filterMemberCallback)
        {
            // Cache all methods and properties of this object, but don't include those
            // defined at or above the base Drop class.
            CachedMethods = GetMemberDictionary(GetMethodsWithoutDuplicateNames(type, mi => mi.GetParameters().Length == 0),
                                                mi => filterMemberCallback(mi));

            CachedProperties = GetMemberDictionary(GetPropertiesWithoutDuplicateNames(type), mi => filterMemberCallback(mi));
            Template = template;
        }

        private Dictionary<string, T> GetMemberDictionary<T>(IEnumerable<T> members, Func<T, bool> filterMemberCallback) where T : MemberInfo
        {
            return members.Where(filterMemberCallback).ToDictionary(mi => mi.Name);
        }

        /// <summary>
        ///     Gets all of the properties for a type, filtering out properties with duplicate names by choosing the property with
        ///     the most derived declaring type.
        /// </summary>
        /// <param name="type">Type to get properties for</param>
        /// <param name="predicate">Any additional filtering on properties</param>
        /// <returns>Filtered properties</returns>
        private static IEnumerable<PropertyInfo> GetPropertiesWithoutDuplicateNames(Type type, Func<PropertyInfo, bool> predicate = null)
        {
            IList<MemberInfo> properties = predicate != null
                                               ? type.GetRuntimeProperties()
                                                     .Where(p => p.CanRead && p.GetMethod.IsPublic && !p.GetMethod.IsStatic)
                                                     .Where(predicate)
                                                     .Cast<MemberInfo>()
                                                     .ToList()
                                               : type.GetRuntimeProperties()
                                                     .Where(p => p.CanRead && p.GetMethod.IsPublic && !p.GetMethod.IsStatic)
                                                     .Cast<MemberInfo>()
                                                     .ToList();

            return GetMembersWithoutDuplicateNames(properties)
                .Cast<PropertyInfo>();
        }

        /// <summary>
        ///     Gets all of the methods for a type, filtering out methods with duplicate names by choosing the method with the most
        ///     derived declaring type.
        /// </summary>
        /// <param name="type">Type to get methods for</param>
        /// <param name="predicate">Any additional filtering on methods</param>
        /// <returns>Filtered methods</returns>
        private static IEnumerable<MethodInfo> GetMethodsWithoutDuplicateNames(Type type, Func<MethodInfo, bool> predicate = null)
        {
            IList<MemberInfo> methods = predicate != null
                                            ? type
                                                  .GetRuntimeMethods()
                                                  .Where(m => m.IsPublic && !m.IsStatic)
                                                  .Where(predicate)
                                                  .Cast<MemberInfo>()
                                                  .ToList()
                                            : type
                                                  .GetRuntimeMethods()
                                                  .Where(m => m.IsPublic && !m.IsStatic)
                                                  .Cast<MemberInfo>()
                                                  .ToList();

            return GetMembersWithoutDuplicateNames(methods)
                .Cast<MethodInfo>();
        }

        /// <summary>
        ///     Filters a collection of MemberInfos by removing MemberInfos with duplicate names. If duplicate names exist, the
        ///     MemberInfo with the most derived DeclaringType will be chosen.
        /// </summary>
        /// <param name="members">Collection of MemberInfos to filter</param>
        /// <returns>Filtered MemberInfos</returns>
        private static IEnumerable<MemberInfo> GetMembersWithoutDuplicateNames(ICollection<MemberInfo> members)
        {
            var duplicatesGroupings = members.GroupBy(x => x.Name)
                                             .Where(g => g.Count() > 1);

            foreach (var duplicatesGrouping in duplicatesGroupings)
            {
                var duplicates = duplicatesGrouping.Select(g => g)
                                                   .ToList();
                var declaringTypes = duplicates.Select(d => d.DeclaringType)
                                               .ToList();

                var mostDerived = declaringTypes.Single(t => !declaringTypes.Any(o => t.GetTypeInfo().IsAssignableFrom(o.GetTypeInfo()) && o != t));

                foreach (var duplicate in duplicates)
                {
                    if (duplicate.DeclaringType != mostDerived)
                    {
                        members.Remove(duplicate);
                    }
                }
            }

            return members;
        }
    }

    internal static class TypeResolutionCache
    {
        [ThreadStatic]
        private static WeakTable<Type, TypeResolution> s_cache;

        public static WeakTable<Type, TypeResolution> Instance
        {
            get { return s_cache ??= new WeakTable<Type, TypeResolution>(32); }
        }
    }

    /// <summary>
    /// A drop in liquid is a class which allows you to to export DOM like things to liquid
    /// Methods of drops are callable.
    /// The main use for liquid drops is the implement lazy loaded objects.
    /// If you would like to make data available to the web designers which you don't want loaded unless needed then
    /// a drop is a great way to do that
    ///     Example:
    ///     class ProductDrop &lt; Liquid::Drop
    ///     def top_sales
    ///     Shop.current.products.find(:all, :order => 'sales', :limit => 10 )
    ///     end
    ///     end
    ///     tmpl = Liquid::Template.parse( ' {% for product in product.top_sales %} {{ product.name }} {%endfor%} ' )
    ///     tmpl.render('product' => ProductDrop.new ) # will invoke top_sales query.
    ///
    /// Your drop can either implement the methods sans any parameters or implement the before_method(name) method which is a
    /// catch all
    /// </summary>
    public abstract class DropBase : ILiquidizable, IIndexable, IContextAware
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="template"></param>
        protected DropBase(Template template)
        {
            Template = template;
        }

        internal TypeResolution TypeResolution
        {
            get
            {
                var dropType = GetObject().GetType();
                if (!TypeResolutionCache.Instance.TryGetValue(dropType, out var resolution))
                {
                    TypeResolutionCache.Instance[dropType] = resolution = CreateTypeResolution(dropType);
                }
                return resolution;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public Context Context { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public Template Template { get; }

        /// <summary>
        /// Just an alias for InvokeDrop - but the presence of the indexer
        /// means that Liquid will access Drop objects as though they are
        /// dictionaries or hashes.
        /// </summary>
        /// <param name="method"></param>
        /// <returns></returns>
        public virtual object this[object method]
        {
            get { return InvokeDrop(method); }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public virtual bool ContainsKey(object name) { return true; }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public virtual object ToLiquid() { return this; }

        internal abstract object GetObject();

        internal abstract TypeResolution CreateTypeResolution(Type type);

        /// <summary>
        /// Catch all for the method
        /// </summary>
        /// <param name="method"></param>
        /// <returns></returns>
        public virtual object BeforeMethod(string method)
        {
            return null;
        }

        /// <summary>
        ///     Called by liquid to invoke a drop
        /// </summary>
        /// <param name="name"></param>
        public object InvokeDrop(object name)
        {
            var method = (string)name;

            if (TypeResolution.CachedMethods.TryGetValue(method, out var mi))
            {
                return mi.Invoke(GetObject(), null);
            }

            if (TypeResolution.CachedProperties.TryGetValue(method, out var pi))
            {
                return pi.GetValue(GetObject(), null);
            }

            return BeforeMethod(method);
        }
    }
    /// <summary>
    /// 
    /// </summary>
    public abstract class Drop : DropBase
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="template"></param>
        protected Drop(Template template)
            :base(template)
        {
        }

        internal override object GetObject() { return this; }

        internal override TypeResolution CreateTypeResolution(Type type) { return new TypeResolution(Template, type, mi => mi.DeclaringType.GetTypeInfo().BaseType != null && typeof(Drop).GetTypeInfo().IsAssignableFrom(mi.DeclaringType.GetTypeInfo().BaseType.GetTypeInfo())); }
    }

    /// <summary>
    /// Proxy for types not derived from DropBase
    /// </summary>
    public class DropProxy : DropBase, IValueTypeConvertible
    {
        private readonly string[] _allowedMembers;
        private readonly object _proxiedObject;
        private readonly Func<object, object> _value;
        private readonly bool _allowAllMembers;

        /// <summary>
        /// Create a new DropProxy object
        /// </summary>
        /// <param name="template"></param>
        /// <param name="obj">The object to create a proxy for</param>
        /// <param name="allowedMembers">An array of property and method names that are allowed to be called on the object.</param>
        public DropProxy(Template template, object obj, string[] allowedMembers)
            :base(template)
        {
            _proxiedObject = obj;
            _allowedMembers = allowedMembers;
            // Allow all member if the list of allowed members is size 1 and is a wild card
            _allowAllMembers = _allowedMembers?.Length == 1 && _allowedMembers[0] == "*";
        }

        /// <summary>
        /// Create a new DropProxy object
        /// </summary>
        /// <param name="template"></param>
        /// <param name="obj">The object to create a proxy for</param>
        /// <param name="allowedMembers">An array of property and method names that are allowed to be called on the object.</param>
        /// <param name="value">Function that converts the specified type into a Liquid Drop-compatible object (eg, implements ILiquidizable)</param>
        public DropProxy(Template template, object obj, string[] allowedMembers, Func<object, object> value)
            : this(template,obj, allowedMembers)
        {
            _value = value;
        }

        #region IValueTypeConvertible
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public virtual object ConvertToValueType()
        {
            if (_value == null)
            {
                return null;
            }

            return _value(_proxiedObject);
        }

        #endregion IValueTypeConvertible

        internal override object GetObject() { return _proxiedObject; }

        internal override TypeResolution CreateTypeResolution(Type type) { return new TypeResolution(Template, type, mi => _allowAllMembers || _allowedMembers.Contains(mi.Name)); }
    }
}

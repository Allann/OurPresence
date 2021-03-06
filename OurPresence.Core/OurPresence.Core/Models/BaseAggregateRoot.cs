﻿// Copyright (c)  Allan Nielsen.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;

namespace OurPresence.Core.Models
{
    public abstract class BaseAggregateRoot<TA, TKey> : BaseEntity<TKey>, IAggregateRoot<TKey>
        where TA : class, IAggregateRoot<TKey>
    {
        private readonly Queue<IDomainEvent<TKey>> _events = new Queue<IDomainEvent<TKey>>();

        protected BaseAggregateRoot() { }

        protected BaseAggregateRoot(TKey id) : base(id)
        {
        }

        public IReadOnlyCollection<IDomainEvent<TKey>> Events => _events.ToImmutableArray();

        public long Version { get; private set; }

        public void ClearEvents()
        {
            _events.Clear();
        }

        protected void AddEvent(IDomainEvent<TKey> @event)
        {
            _events.Enqueue(@event);

            this.Apply(@event);

            this.Version++;
        }

        protected abstract void Apply(IDomainEvent<TKey> @event);

        #region Factory

        private static readonly ConstructorInfo CTor;

        static BaseAggregateRoot()
        {
            var aggregateType = typeof(TA);
            CTor = aggregateType.GetConstructor(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public,
                null, Array.Empty<Type>(), Array.Empty<ParameterModifier>());
            if (null == CTor)
                throw new InvalidOperationException($"Unable to find required private parameterless constructor for Aggregate of type '{aggregateType.Name}'");
        }

        public static TA Create(IEnumerable<IDomainEvent<TKey>> events)
        {
            if (null == events || !events.Any())
                throw new ArgumentNullException(nameof(events));
            var result = (TA)CTor.Invoke(new object[0]);

            var baseAggregate = result as BaseAggregateRoot<TA, TKey>;
            if (baseAggregate != null)
                foreach (var @event in events)
                    baseAggregate.AddEvent(@event);

            result.ClearEvents();

            return result;
        }

        #endregion Factory
    }
}

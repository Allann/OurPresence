﻿// Copyright (c)  Allan Nielsen.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace OurPresence.Core.Money
{
    /// <summary>Defines a custom currency that is new or based on another currency.</summary>
    public class CurrencyBuilder
    {
        /// <summary>Initializes a new instance of the <see cref="CurrencyBuilder"/> class.</summary>
        /// <param name="code">The code of the currency, normally the three-character ISO 4217 currency code.</param>
        /// <param name="namespace">The namespace for the currency.</param>
        /// <exception cref="ArgumentNullException"><paramref name="code"/> or <paramref name="namespace"/> is <see langword="null" /> or empty.</exception>
        public CurrencyBuilder(string code, string @namespace)
        {
            if (string.IsNullOrWhiteSpace(code))
            {
                throw new ArgumentNullException(nameof(code));
            }

            if (string.IsNullOrWhiteSpace(@namespace))
            {
                throw new ArgumentNullException(nameof(@namespace));
            }

            Code = code;
            Namespace = @namespace;
        }

        /// <summary>Gets or sets the english name of the currency.</summary>
        public string? EnglishName { get; set; }

        /// <summary>Gets or sets the currency sign.</summary>
        public string? Symbol { get; set; }

        /// <summary>Gets or sets the numeric ISO 4217 currency code.</summary>
        // ReSharper disable once InconsistentNaming
        public string? ISONumber { get; set; }

        /// <summary>Gets or sets the number of digits after the decimal separator.</summary>
        public double DecimalDigits { get; set; }

        /// <summary>Gets the namespace of the currency.</summary>
        public string Namespace { get; }

        /// <summary>Gets the code of the currency, normally a three-character ISO 4217 currency code.</summary>
        public string Code { get; }

        /// <summary>Gets or sets the date when the currency is valid from.</summary>
        /// <value>The from date when the currency is valid.</value>
        public DateTime? ValidFrom { get; set; }

        /// <summary>Gets or sets the date when the currency is valid to.</summary>
        /// <value>The to date when the currency is valid.</value>
        public DateTime? ValidTo { get; set; }

        /// <summary>Unregisters the specified currency code from the current AppDomain and returns it.</summary>
        /// <param name="code">The name of the currency to unregister.</param>
        /// <param name="namespace">The namespace of the currency to unregister from.</param>
        /// <returns>An instance of the type <see cref="Currency"/>.</returns>
        /// <exception cref="ArgumentException">code specifies a currency that is not found in the given namespace.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="code" /> or <paramref name="namespace" /> is <see langword="null" /> or empty.</exception>
        public static Currency Unregister(string code, string @namespace)
        {
            if (string.IsNullOrWhiteSpace(code))
            {
                throw new ArgumentNullException(nameof(code));
            }

            if (string.IsNullOrWhiteSpace(@namespace))
            {
                throw new ArgumentNullException(nameof(@namespace));
            }

            return !Currency.Registry.TryRemove(code, @namespace, out var currency)
                ? throw new ArgumentException($"code {code} specifies a currency that is not found in the {nameof(@namespace)} {@namespace}.")
                : currency;
        }

        /// <summary>Builds the current <see cref="CurrencyBuilder"/> object as a custom currency.</summary>
        /// <returns>A <see cref="Currency"/> instance that is build.</returns>
        /// <exception cref="InvalidOperationException">The current CurrencyBuilder object has a property that must be set before the currency can be registered.</exception>
        public Currency Build()
        {
            if (string.IsNullOrWhiteSpace(Symbol))
            {
                Symbol = Currency.GenericCurrencySign;
            }

            return new Currency(Code, ISONumber, DecimalDigits, EnglishName, Symbol, Namespace, ValidTo, ValidFrom);
        }

        /// <summary>Registers the current <see cref="CurrencyBuilder"/> object as a custom currency for the current AppDomain.</summary>
        /// <returns>A <see cref="Currency"/> instance that is build and registered.</returns>
        /// <exception cref="InvalidOperationException">
        ///     <para>The custom currency is already registered.</para>
        ///     <para>-or-</para>
        ///     <para>The current CurrencyBuilder object has a property that must be set before the currency can be registered.</para>
        /// </exception>
        public Currency Register()
        {
            var currency = Build();
            return !Currency.Registry.TryAdd(Code, Namespace, currency)
                ? throw new InvalidOperationException("The custom currency is already registered.")
                : currency;
        }

        /// <summary>Sets the properties of the current <see cref="CurrencyBuilder"/> object with the corresponding properties of
        /// the specified <see cref="Currency"/> object, except for the code and namespace.</summary>
        /// <param name="currency">The object whose properties will be used.</param>
        public void LoadDataFromCurrency(Currency currency)
        {
            EnglishName = currency.EnglishName;
            Symbol = currency.Symbol;
            ISONumber = currency.Number;
            DecimalDigits = currency.DecimalDigits;
            ValidFrom = currency.ValidFrom;
            ValidTo = currency.ValidTo;
        }
    }
}

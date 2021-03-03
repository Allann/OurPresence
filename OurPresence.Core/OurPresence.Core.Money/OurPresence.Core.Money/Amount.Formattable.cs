﻿using System;
using System.Globalization;

namespace OurPresence.Core.Money
{
    /// <summary>Represents Money, an amount defined in a specific Currency.</summary>
    public partial struct Amount : IFormattable
    {
        /// <summary>Converts this <see cref="Amount"/> instance to its equivalent <see cref="string"/> representation.</summary>
        /// <returns>A string that represents this <see cref="Amount"/> instance.</returns>
        /// <remarks>
        /// Converting will use the <see cref="NumberFormatInfo"/> object for the current culture if this has the same
        /// ISOCurrencySymbol, otherwise the <see cref="NumberFormatInfo"/> from the <see cref="Currency"/> will be used.
        /// </remarks>
        public override string? ToString()
            => ConvertToString(null, null);

        /// <summary>Converts the <see cref="Amount"/> value of this instance to its equivalent <see cref="string"/> representation
        /// using the specified format.</summary>
        /// <param name="format">A numeric format string.</param>
        /// <returns>The string representation of this <see cref="Amount"/> instance as specified by the format.</returns>
        public string ToString(string format)
        {
            return ConvertToString(format, null);
        }

        /// <summary>Converts this <see cref="Amount"/> instance to its equivalent <see cref="string"/> representation using the
        /// specified culture-specific format information.</summary>
        /// <param name="formatProvider">An <see cref="IFormatProvider"/> that supplies culture-specific formatting information.</param>
        /// <returns>The string representation of this <see cref="Amount"/> instance as specified by formatProvider.</returns>
        public string ToString(IFormatProvider formatProvider)
        {
            return ConvertToString(null, formatProvider);
        }

        /// <summary>Converts the <see cref="Amount"/> value of this instance to its equivalent <see cref="string"/> representation
        /// using the specified format and culture-specific format information.</summary>
        /// <param name="format">A numeric format string.</param>
        /// <param name="formatProvider">An <see cref="IFormatProvider"/> that supplies culture-specific formatting information.</param>
        /// <returns>The string representation of this <see cref="Amount"/> instance as specified by the format and formatProvider.</returns>
        public string ToString(string? format, IFormatProvider? formatProvider)
        {
            return ConvertToString(format, formatProvider);
        }

        private static IFormatProvider GetFormatProvider(Currency? currency, IFormatProvider? formatProvider, bool useCode = false)
        {
            var cc = CultureInfo.CurrentCulture;

            var numberFormatInfo = (NumberFormatInfo)cc.NumberFormat.Clone();
            if (!currency.HasValue)
            {
                return numberFormatInfo;
            }

            if (formatProvider != null)
            {
                numberFormatInfo = formatProvider switch
                {
                    CultureInfo ci => (NumberFormatInfo)ci.NumberFormat.Clone(),
                    NumberFormatInfo nfi => (NumberFormatInfo)nfi.Clone(),
                    _ => numberFormatInfo
                };
            }

            numberFormatInfo.CurrencyDecimalDigits = (int)currency.Value.DecimalDigits;
            numberFormatInfo.CurrencySymbol = currency.Value.Symbol ?? string.Empty;

            if (!useCode)
            {
                return numberFormatInfo;
            }

            // Replace symbol with the code
            numberFormatInfo.CurrencySymbol = currency.Value.Code;

            // Add spacing to PositivePattern and NegativePattern
            if (numberFormatInfo.CurrencyPositivePattern == 0) // $n
            {
                numberFormatInfo.CurrencyPositivePattern = 2; // $ n
            }

            if (numberFormatInfo.CurrencyPositivePattern == 1) // n$
            {
                numberFormatInfo.CurrencyPositivePattern = 3; // n $
            }

            switch (numberFormatInfo.CurrencyNegativePattern)
            {
                case 0: // ($n)
                    numberFormatInfo.CurrencyNegativePattern = 14; // ($ n)
                    break;
                case 1: // -$n
                    numberFormatInfo.CurrencyNegativePattern = 9; // -$ n
                    break;
                case 2: // $-n
                    numberFormatInfo.CurrencyNegativePattern = 12; // $ -n
                    break;
                case 3: // $n-
                    numberFormatInfo.CurrencyNegativePattern = 11; // $ n-
                    break;
                case 4: // (n$)
                    numberFormatInfo.CurrencyNegativePattern = 15; // (n $)
                    break;
                case 5: // -n$
                    numberFormatInfo.CurrencyNegativePattern = 8; // -n $
                    break;
                case 6: // n-$
                    numberFormatInfo.CurrencyNegativePattern = 13; // n- $
                    break;
                case 7: // n$-
                    numberFormatInfo.CurrencyNegativePattern = 10; // n $-
                    break;
                default:
                    break;
            }

            return numberFormatInfo;
        }

        private string ConvertToString(string? format, IFormatProvider? formatProvider)
        {
            // TODO: ICustomFormat : http://msdn.microsoft.com/query/dev12.query?appId=Dev12IDEF1&l=EN-US&k=k(System.IFormatProvider);k(TargetFrameworkMoniker-.NETPortable,Version%3Dv4.6);k(DevLang-csharp)&rd=true

            // TODO: Hacked solution, solve with better implementation
            IFormatProvider provider;
            if (!string.IsNullOrWhiteSpace(format) && format.StartsWith("I", StringComparison.Ordinal) && format.Length >= 1 && format.Length <= 2)
            {
                format = format.Replace("I", "C");
                provider = GetFormatProvider(Currency, formatProvider, true);
            }
            else
            {
                provider = GetFormatProvider(Currency, formatProvider);
            }

            if (format == null || format == "G")
            {
                format = "C";
            }

            if (!format.StartsWith("F", StringComparison.Ordinal))
            {
                return Value.ToString(format, provider);
            }

            format = format.Replace("F", "N");
            if (format.Length == 1)
            {
                format += Currency.DecimalDigits;
            }

            return $"{Value.ToString(format, provider)} {Currency.EnglishName}";

        }
    }
}

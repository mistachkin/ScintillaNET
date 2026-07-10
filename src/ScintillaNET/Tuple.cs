/*
 * Tuple.cs --
 *
 * Copyright (c) 2017 Jacob Slusser, https://github.com/jacobslusser
 * Copyright (c) 2019-2026 by Joe Mistachkin.  All rights reserved.
 *
 * This file is part of ScintillaNET, which is distributed under the MIT
 * License; see the file "LICENSE" for full terms and a DISCLAIMER OF ALL
 * WARRANTIES.
 *
 * RCS: @(#) $Id: $
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;

namespace ScintillaNET
{
    /// <summary>
    /// Provides static methods for creating two-element tuple instances.
    /// </summary>
    [ObjectId("d9b0bad5-7fce-4068-b04d-a803b0abea59")]
    static class Tuple
    {
        #region Public Static Methods
        /// <summary>
        /// Creates a new two-element tuple using the specified component
        /// values.
        /// </summary>
        /// <typeparam name="T1">
        /// The type of the first component of the tuple.
        /// </typeparam>
        /// <typeparam name="T2">
        /// The type of the second component of the tuple.
        /// </typeparam>
        /// <param name="item1">
        /// The value of the first component of the tuple.
        /// </param>
        /// <param name="item2">
        /// The value of the second component of the tuple.
        /// </param>
        /// <returns>
        /// A new <see cref="Tuple{T1, T2}" /> whose components are the
        /// specified values.
        /// </returns>
        public static Tuple<T1, T2> Create<T1, T2>(
            T1 item1, /* in */
            T2 item2  /* in */
            )
        {
            return new Tuple<T1, T2>(item1, item2);
        }
        #endregion
    }

    ///////////////////////////////////////////////////////////////////////////

    /// <summary>
    /// Represents a two-element tuple.  This is a lightweight replacement
    /// for the framework tuple type on platforms where it is unavailable.
    /// </summary>
    /// <typeparam name="T1">
    /// The type of the first component of the tuple.
    /// </typeparam>
    /// <typeparam name="T2">
    /// The type of the second component of the tuple.
    /// </typeparam>
    [DebuggerDisplay("Item1={Item1};Item2={Item2}")]
    [ObjectId("466b18c3-c21d-45d6-a7b5-040313a6d66a")]
    class Tuple<T1, T2> : IFormattable
    {
        #region Private Static Data
        /// <summary>
        /// The equality comparer used to compare and hash values of the first
        /// component type.
        /// </summary>
        private static readonly IEqualityComparer<T1> Item1Comparer =
            EqualityComparer<T1>.Default;

        /// <summary>
        /// The equality comparer used to compare and hash values of the
        /// second component type.
        /// </summary>
        private static readonly IEqualityComparer<T2> Item2Comparer =
            EqualityComparer<T2>.Default;
        #endregion

        ///////////////////////////////////////////////////////////////////////

        #region Private Data
        /// <summary>
        /// The value of the first component of the tuple.
        /// </summary>
        private T1 item1;

        /// <summary>
        /// The value of the second component of the tuple.
        /// </summary>
        private T2 item2;
        #endregion

        ///////////////////////////////////////////////////////////////////////

        #region Public Constructors
        /// <summary>
        /// Constructs an instance of this class using the specified component
        /// values.
        /// </summary>
        /// <param name="item1">
        /// The value of the first component of the tuple.
        /// </param>
        /// <param name="item2">
        /// The value of the second component of the tuple.
        /// </param>
        public Tuple(
            T1 item1, /* in */
            T2 item2  /* in */
            )
        {
            this.item1 = item1;
            this.item2 = item2;
        }
        #endregion

        ///////////////////////////////////////////////////////////////////////

        #region Public Properties
        /// <summary>
        /// Gets the value of the first component of the tuple.
        /// </summary>
        public T1 Item1
        {
            get { return this.item1; }
            private set { this.item1 = value; }
        }

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Gets the value of the second component of the tuple.
        /// </summary>
        public T2 Item2
        {
            get { return this.item2; }
            private set { this.item2 = value; }
        }
        #endregion

        ///////////////////////////////////////////////////////////////////////

        #region IFormattable Members
        /// <summary>
        /// Formats the value of this tuple using the specified format string
        /// and format provider.
        /// </summary>
        /// <param name="format">
        /// The format string to use, or null to use a default format that
        /// combines both components separated by a comma.
        /// </param>
        /// <param name="formatProvider">
        /// The provider to use to format the value, or null to use the
        /// formatting information from the current culture.
        /// </param>
        /// <returns>
        /// The value of this tuple formatted using the specified format and
        /// provider.
        /// </returns>
        public string ToString(
            string format,                 /* in */
            IFormatProvider formatProvider /* in */
            )
        {
            return string.Format(
                formatProvider, format ?? "{0},{1}", Item1, Item2);
        }
        #endregion

        ///////////////////////////////////////////////////////////////////////

        #region System.Object Overrides
        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        /// <returns>
        /// A 32-bit signed integer hash code.
        /// </returns>
        public override int GetHashCode()
        {
            int hc = 0;

            if (!object.ReferenceEquals(Item1, null))
                hc = Item1Comparer.GetHashCode(Item1);

            if (!object.ReferenceEquals(Item2, null))
                hc = (hc << 3) ^ Item2Comparer.GetHashCode(Item2);

            return hc;
        }

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Returns a value indicating whether this instance is equal to a
        /// specified object.
        /// </summary>
        /// <param name="obj">
        /// An object to compare with this instance or null.
        /// </param>
        /// <returns>
        /// true if <paramref name="obj" /> is a <see cref="Tuple{T1, T2}" />
        /// whose components are equal to those of this instance; otherwise,
        /// false.
        /// </returns>
        public override bool Equals(
            object obj /* in */
            )
        {
            Tuple<T1, T2> other = obj as Tuple<T1, T2>;

            if (object.ReferenceEquals(other, null))
                return false;
            else
                return Item1Comparer.Equals(Item1, other.Item1) &&
                    Item2Comparer.Equals(Item2, other.Item2);
        }

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Returns the string representation of this tuple using the current
        /// culture.
        /// </summary>
        /// <returns>
        /// The string representation of this tuple.
        /// </returns>
        public override string ToString()
        {
            return ToString(null, CultureInfo.CurrentCulture);
        }
        #endregion
    }
}

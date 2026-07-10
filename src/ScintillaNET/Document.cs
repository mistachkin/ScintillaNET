/*
 * Document.cs --
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

namespace ScintillaNET
{
    /// <summary>
    /// A <see cref="Scintilla" /> document.
    /// </summary>
    /// <remarks>
    /// This is an opaque type, meaning it can be used by a
    /// <see cref="Scintilla" /> control but otherwise has no public members
    /// of its own.
    /// </remarks>
    [ObjectId("6916e188-8702-4d20-a6da-48347153619e")]
    public struct Document
    {
        #region Internal Data
        /// <summary>
        /// The native handle that identifies the underlying Scintilla
        /// document.
        /// </summary>
        internal IntPtr Value;
        #endregion

        ///////////////////////////////////////////////////////////////////////

        #region Public Static Data
        /// <summary>
        /// A read-only field that represents an uninitialized document.
        /// </summary>
        public static readonly Document Empty;
        #endregion

        ///////////////////////////////////////////////////////////////////////

        #region Internal Constructors
        /// <summary>
        /// Constructs an instance of this structure using the specified
        /// native document handle.
        /// </summary>
        /// <param name="value">
        /// The native handle that identifies the underlying Scintilla
        /// document.
        /// </param>
        internal Document(
            IntPtr value /* in */
            )
        {
            this.Value = value;
        }
        #endregion

        ///////////////////////////////////////////////////////////////////////

        #region Operators
        /// <summary>
        /// Determines whether two specified instances of
        /// <see cref="Document" /> are equal.
        /// </summary>
        /// <param name="a">
        /// The first document to compare.
        /// </param>
        /// <param name="b">
        /// The second document to compare.
        /// </param>
        /// <returns>
        /// true if <paramref name="a" /> equals <paramref name="b" />;
        /// otherwise, false.
        /// </returns>
        public static bool operator ==(
            Document a, /* in */
            Document b  /* in */
            )
        {
            return a.Value == b.Value;
        }

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Determines whether two specified instances of
        /// <see cref="Document" /> are not equal.
        /// </summary>
        /// <param name="a">
        /// The first document to compare.
        /// </param>
        /// <param name="b">
        /// The second document to compare.
        /// </param>
        /// <returns>
        /// true if <paramref name="a" /> does not equal
        /// <paramref name="b" />; otherwise, false.
        /// </returns>
        public static bool operator !=(
            Document a, /* in */
            Document b  /* in */
            )
        {
            return a.Value != b.Value;
        }
        #endregion

        ///////////////////////////////////////////////////////////////////////

        #region System.Object Overrides
        /// <summary>
        /// Returns a value indicating whether this instance is equal to a
        /// specified object.
        /// </summary>
        /// <param name="obj">
        /// An object to compare with this instance or null.
        /// </param>
        /// <returns>
        /// true if <paramref name="obj" /> is an instance of
        /// <see cref="Document" /> and equals the value of this instance;
        /// otherwise, false.
        /// </returns>
        public override bool Equals(
            object obj /* in */
            )
        {
            return (obj is Document) && Value == ((Document)obj).Value;
        }

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        /// <returns>
        /// A 32-bit signed integer hash code.
        /// </returns>
        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }
        #endregion
    }
}

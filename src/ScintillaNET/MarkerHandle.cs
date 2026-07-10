/*
 * MarkerHandle.cs --
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
    /// A <see cref="Marker" /> handle.
    /// </summary>
    /// <remarks>
    /// This is an opaque type, meaning it can be used by a
    /// <see cref="Scintilla" /> control but otherwise has no public members
    /// of its own.
    /// </remarks>
    [ObjectId("96c76e38-5a27-4aa4-b4e1-27f1b7e43a3f")]
    public struct MarkerHandle
    {
        #region Internal Data
        /// <summary>
        /// The native handle that identifies the underlying Scintilla marker.
        /// </summary>
        internal IntPtr Value;
        #endregion

        ///////////////////////////////////////////////////////////////////////

        #region Public Static Data
        /// <summary>
        /// A read-only field that represents an uninitialized handle.
        /// </summary>
        public static readonly MarkerHandle Zero;
        #endregion

        ///////////////////////////////////////////////////////////////////////

        #region Internal Constructors
        /// <summary>
        /// Constructs an instance of this structure using the specified
        /// native marker handle.
        /// </summary>
        /// <param name="value">
        /// The native handle that identifies the underlying Scintilla marker.
        /// </param>
        internal MarkerHandle(
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
        /// <see cref="MarkerHandle" /> are equal.
        /// </summary>
        /// <param name="a">
        /// The first handle to compare.
        /// </param>
        /// <param name="b">
        /// The second handle to compare.
        /// </param>
        /// <returns>
        /// true if <paramref name="a" /> equals <paramref name="b" />;
        /// otherwise, false.
        /// </returns>
        public static bool operator ==(
            MarkerHandle a, /* in */
            MarkerHandle b  /* in */
            )
        {
            return a.Value == b.Value;
        }

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Determines whether two specified instances of
        /// <see cref="MarkerHandle" /> are not equal.
        /// </summary>
        /// <param name="a">
        /// The first handle to compare.
        /// </param>
        /// <param name="b">
        /// The second handle to compare.
        /// </param>
        /// <returns>
        /// true if <paramref name="a" /> does not equal
        /// <paramref name="b" />; otherwise, false.
        /// </returns>
        public static bool operator !=(
            MarkerHandle a, /* in */
            MarkerHandle b  /* in */
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
        /// <see cref="MarkerHandle" /> and equals the value of this instance;
        /// otherwise, false.
        /// </returns>
        public override bool Equals(
            object obj /* in */
            )
        {
            return (obj is IntPtr) && Value == ((MarkerHandle)obj).Value;
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

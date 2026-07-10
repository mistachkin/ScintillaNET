/*
 * ObjectId.cs --
 *
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
    /// This class implements an attribute used to associate a stable,
    /// globally-unique identifier with the type or member it marks.  The
    /// identifier remains constant across builds, mirroring the Eagle
    /// <c>ObjectId</c> convention so that ScintillaNET types can be recognized
    /// regardless of their names.
    /// </summary>
    [AttributeUsage(AttributeTargets.All, Inherited = false)]
    [ObjectId("8fdc6ecd-9cbc-4917-a20e-7fe34e0e69e5")]
    internal sealed class ObjectIdAttribute : Attribute
    {
        #region Private Data
        /// <summary>
        /// The unique identifier associated with the marked type or member.
        /// </summary>
        private Guid id;
        #endregion

        ///////////////////////////////////////////////////////////////////////

        #region Public Constructors
        /// <summary>
        /// Constructs an instance of this attribute using the specified unique
        /// identifier in its string form.
        /// </summary>
        /// <param name="value">
        /// The string representation of the unique identifier to associate with
        /// the marked type or member.  An exception is thrown if this value
        /// cannot be parsed as a valid identifier.
        /// </param>
        public ObjectIdAttribute(
            string value /* in */
            )
        {
            this.id = new Guid(value); /* throw */
        }
        #endregion

        ///////////////////////////////////////////////////////////////////////

        #region Public Properties
        /// <summary>
        /// Gets the unique identifier associated with the marked type or
        /// member.
        /// </summary>
        public Guid Id
        {
            get { return this.id; }
        }
        #endregion
    }
}

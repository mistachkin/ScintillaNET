/*
 * ChangeAnnotationEventArgs.cs --
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
    /// Provides data for the <see cref="Scintilla.ChangeAnnotation" /> event.
    /// </summary>
    [ObjectId("c9692035-5a76-4ca1-9ec3-581a9ac69ac0")]
    public class ChangeAnnotationEventArgs : EventArgs
    {
        #region Private Data
        /// <summary>
        /// The zero-based line index where the annotation change occurred.
        /// </summary>
        private long line;
        #endregion

        ///////////////////////////////////////////////////////////////////////

        #region Public Constructors
        /// <summary>
        /// Constructs an instance of this class using the specified line
        /// index.
        /// </summary>
        /// <param name="line">
        /// The zero-based line index of the annotation that changed.
        /// </param>
        public ChangeAnnotationEventArgs(
            long line /* in */
            )
        {
            this.line = line;
        }
        #endregion

        ///////////////////////////////////////////////////////////////////////

        #region Public Properties
        /// <summary>
        /// Gets the line index where the annotation changed.
        /// </summary>
        public long Line
        {
            get { return this.line; }
        }
        #endregion
    }
}

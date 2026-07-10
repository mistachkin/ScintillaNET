/*
 * CharAddedEventArgs.cs --
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
    /// Provides the data for the <see cref="Scintilla.CharAdded" /> event.
    /// </summary>
    [ObjectId("7a7012fe-b7d9-4a43-ae75-2e50a6a984bd")]
    public class CharAddedEventArgs : EventArgs
    {
        #region Private Data
        /// <summary>
        /// The text character that was added to the control.
        /// </summary>
        private int ch;
        #endregion

        ///////////////////////////////////////////////////////////////////////

        #region Public Constructors
        /// <summary>
        /// Constructs an instance of this class using the specified character.
        /// </summary>
        /// <param name="ch">
        /// The text character that was added to the control.
        /// </param>
        public CharAddedEventArgs(
            int ch /* in */
            )
        {
            this.ch = ch;
        }
        #endregion

        ///////////////////////////////////////////////////////////////////////

        #region Public Properties
        /// <summary>
        /// Gets the text character added to a <see cref="Scintilla" />
        /// control.
        /// </summary>
        public int Char
        {
            get { return this.ch; }
        }
        #endregion
    }
}

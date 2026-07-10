/*
 * UpdateUIEventArgs.cs --
 *
 * Copyright (c) 2017 Jacob Slusser, https://github.com/jacobslusser
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
    /// Provides data for the <see cref="Scintilla.UpdateUI" /> event.
    /// </summary>
    [ObjectId("878d9557-ea3c-421b-8bc5-ec1562eb29d9")]
    public class UpdateUIEventArgs : EventArgs
    {
        #region Private Data
        /// <summary>
        /// The UI update that occurred.
        /// </summary>
        private UpdateChange change;
        #endregion

        ///////////////////////////////////////////////////////////////////////

        #region Public Constructors
        /// <summary>
        /// Constructs an instance of this class using the specified UI update.
        /// </summary>
        /// <param name="change">
        /// A bitwise combination of <see cref="UpdateChange" /> values
        /// specifying the reason to update the UI.
        /// </param>
        public UpdateUIEventArgs(
            UpdateChange change /* in */
            )
        {
            this.change = change;
        }
        #endregion

        ///////////////////////////////////////////////////////////////////////

        #region Public Properties
        /// <summary>
        /// Gets the UI update that occurred.
        /// </summary>
        public UpdateChange Change
        {
            get { return this.change; }
        }
        #endregion
    }
}

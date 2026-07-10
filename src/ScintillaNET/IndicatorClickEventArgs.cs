/*
 * IndicatorClickEventArgs.cs --
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

using System.Windows.Forms;

namespace ScintillaNET
{
    /// <summary>
    /// Provides data for the <see cref="Scintilla.IndicatorClick" /> event.
    /// </summary>
    [ObjectId("6ac0a093-2154-4c37-a7b0-76ccb0ff75be")]
    public class IndicatorClickEventArgs : IndicatorReleaseEventArgs
    {
        #region Private Data
        /// <summary>
        /// The modifier keys (SHIFT, CTRL, ALT) held down when clicked.
        /// </summary>
        private Keys modifiers;
        #endregion

        ///////////////////////////////////////////////////////////////////////

        #region Public Constructors
        /// <summary>
        /// Constructs an instance of this class using the specified
        /// control, modifier keys, and byte position.
        /// </summary>
        /// <param name="scintilla">
        /// The <see cref="Scintilla" /> control that generated this event.
        /// </param>
        /// <param name="modifiers">
        /// The modifier keys that were held down at the time of the click.
        /// </param>
        /// <param name="bytePosition">
        /// The zero-based byte position of the clicked text.
        /// </param>
        public IndicatorClickEventArgs(
            Scintilla scintilla, /* in */
            Keys modifiers,      /* in */
            long bytePosition    /* in */
            )
            : base(scintilla, bytePosition)
        {
            this.modifiers = modifiers;
        }
        #endregion

        ///////////////////////////////////////////////////////////////////////

        #region Public Properties
        /// <summary>
        /// Gets the modifier keys (SHIFT, CTRL, ALT) held down when clicked.
        /// </summary>
        public Keys Modifiers
        {
            get { return this.modifiers; }
        }
        #endregion
    }
}

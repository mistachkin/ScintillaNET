/*
 * StyleNeededEventArgs.cs --
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
    /// Provides the data for the <see cref="Scintilla.StyleNeeded" /> event.
    /// </summary>
    [ObjectId("f71ca391-63d7-42d4-8b95-3d70cb4637d0")]
    public class StyleNeededEventArgs : EventArgs
    {
        #region Private Data
        /// <summary>
        /// The <see cref="Scintilla" /> control that generated this event.
        /// </summary>
        private readonly Scintilla scintilla;

        /// <summary>
        /// The zero-based byte position within the document to stop styling.
        /// </summary>
        private readonly long bytePosition;

        /// <summary>
        /// The cached zero-based document (character) position up to which
        /// styling should be performed, or null if it has not yet been
        /// computed.
        /// </summary>
        private long? position;
        #endregion

        ///////////////////////////////////////////////////////////////////////

        #region Public Constructors
        /// <summary>
        /// Constructs an instance of this class using the specified control
        /// and byte position.
        /// </summary>
        /// <param name="scintilla">
        /// The <see cref="Scintilla" /> control that generated this event.
        /// </param>
        /// <param name="bytePosition">
        /// The zero-based byte position within the document to stop styling.
        /// </param>
        public StyleNeededEventArgs(
            Scintilla scintilla, /* in */
            long bytePosition    /* in */
            )
        {
            this.scintilla = scintilla;
            this.bytePosition = bytePosition;
        }
        #endregion

        ///////////////////////////////////////////////////////////////////////

        #region Public Properties
        /// <summary>
        /// Gets the zero-based document position where styling should end.
        /// The <see cref="Scintilla.GetEndStyled" /> method indicates the last
        /// position styled correctly and the starting place for where styling
        /// should begin.
        /// </summary>
        public long Position
        {
            get
            {
                if (this.position == null)
                    this.position = this.scintilla.Lines.ByteToCharPosition(
                        this.bytePosition);

                return (long)this.position;
            }
        }
        #endregion
    }
}

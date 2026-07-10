/*
 * IndicatorReleaseEventArgs.cs --
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
    /// Provides the data for the <see cref="Scintilla.IndicatorRelease" />
    /// event.
    /// </summary>
    [ObjectId("8cfb9262-cfee-412e-a423-73cb306ee191")]
    public class IndicatorReleaseEventArgs : EventArgs
    {
        #region Private Data
        /// <summary>
        /// The <see cref="Scintilla" /> control that generated this event.
        /// </summary>
        private readonly Scintilla scintilla;

        /// <summary>
        /// The zero-based byte position of the clicked text.
        /// </summary>
        private readonly long bytePosition;

        /// <summary>
        /// The cached zero-based document (character) position of the clicked
        /// text, or null if it has not yet been computed.
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
        /// The zero-based byte position of the clicked text.
        /// </param>
        public IndicatorReleaseEventArgs(
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
        /// Gets the zero-based document (character) position of the text that
        /// was clicked.
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

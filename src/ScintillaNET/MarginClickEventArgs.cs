/*
 * MarginClickEventArgs.cs --
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
using System.Windows.Forms;

namespace ScintillaNET
{
    /// <summary>
    /// Provides data for the <see cref="Scintilla.MarginClick" /> event.
    /// </summary>
    [ObjectId("8c03746f-df50-4609-85d5-6035209b30f3")]
    public class MarginClickEventArgs : EventArgs
    {
        #region Private Data
        /// <summary>
        /// The <see cref="Scintilla" /> control that generated this event.
        /// </summary>
        private readonly Scintilla scintilla;

        /// <summary>
        /// The zero-based byte position within the document where the line
        /// adjacent to the clicked margin starts.
        /// </summary>
        private readonly long bytePosition;

        /// <summary>
        /// The zero-based character position within the document, computed on
        /// demand from the byte position and cached here; otherwise, null if
        /// it has not yet been computed.
        /// </summary>
        private long? position;

        /// <summary>
        /// The zero-based index of the clicked margin.
        /// </summary>
        private int margin;

        /// <summary>
        /// The modifier keys (SHIFT, CTRL, ALT) held down when the margin
        /// was clicked.
        /// </summary>
        private Keys modifiers;
        #endregion

        ///////////////////////////////////////////////////////////////////////

        #region Public Constructors
        /// <summary>
        /// Constructs an instance of this class using the specified control,
        /// modifier keys, byte position, and margin index.
        /// </summary>
        /// <param name="scintilla">
        /// The <see cref="Scintilla" /> control that generated this event.
        /// </param>
        /// <param name="modifiers">
        /// The modifier keys that were held down at the time of the margin
        /// click.
        /// </param>
        /// <param name="bytePosition">
        /// The zero-based byte position within the document where the line
        /// adjacent to the clicked margin starts.
        /// </param>
        /// <param name="margin">
        /// The zero-based index of the clicked margin.
        /// </param>
        public MarginClickEventArgs(
            Scintilla scintilla, /* in */
            Keys modifiers,      /* in */
            long bytePosition,   /* in */
            int margin           /* in */
            )
        {
            this.scintilla = scintilla;
            this.bytePosition = bytePosition;
            this.modifiers = modifiers;
            this.margin = margin;
        }
        #endregion

        ///////////////////////////////////////////////////////////////////////

        #region Public Properties
        /// <summary>
        /// Gets the margin clicked.
        /// </summary>
        public int Margin
        {
            get { return this.margin; }
        }

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Gets the modifier keys (SHIFT, CTRL, ALT) held down when the
        /// margin was clicked.
        /// </summary>
        public Keys Modifiers
        {
            get { return this.modifiers; }
        }

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Gets the zero-based document position where the line adjacent to
        /// the clicked margin starts.
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

/*
 * DoubleClickEventArgs.cs --
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
    /// Provides data for the <see cref="Scintilla.DoubleClick" /> event.
    /// </summary>
    [ObjectId("72ae4a7d-56af-4f3d-b2b1-0b65a20f8e31")]
    public class DoubleClickEventArgs : EventArgs
    {
        #region Private Data
        /// <summary>
        /// The <see cref="Scintilla" /> control that generated this event.
        /// </summary>
        private readonly Scintilla scintilla;

        /// <summary>
        /// The zero-based byte position of the double clicked text.
        /// </summary>
        private readonly long bytePosition;

        /// <summary>
        /// The zero-based character position within the document, computed on
        /// demand from the byte position and cached here; otherwise, null if
        /// it has not yet been computed.
        /// </summary>
        private long? position;

        /// <summary>
        /// The zero-based index of the double clicked line.
        /// </summary>
        private long line;

        /// <summary>
        /// The modifier keys (SHIFT, CTRL, ALT) held down when double
        /// clicked.
        /// </summary>
        private Keys modifiers;
        #endregion

        ///////////////////////////////////////////////////////////////////////

        #region Public Constructors
        /// <summary>
        /// Constructs an instance of this class using the specified control,
        /// modifier keys, byte position, and line index.
        /// </summary>
        /// <param name="scintilla">
        /// The <see cref="Scintilla" /> control that generated this event.
        /// </param>
        /// <param name="modifiers">
        /// The modifier keys that were held down at the time of the double
        /// click.
        /// </param>
        /// <param name="bytePosition">
        /// The zero-based byte position of the double clicked text.
        /// </param>
        /// <param name="line">
        /// The zero-based line index of the double clicked text.
        /// </param>
        public DoubleClickEventArgs(
            Scintilla scintilla, /* in */
            Keys modifiers,      /* in */
            long bytePosition,   /* in */
            long line            /* in */
            )
        {
            this.scintilla = scintilla;
            this.bytePosition = bytePosition;
            this.modifiers = modifiers;
            this.line = line;

            if (bytePosition == -1)
                this.position = -1;
        }
        #endregion

        ///////////////////////////////////////////////////////////////////////

        #region Public Properties
        /// <summary>
        /// Gets the line double clicked.
        /// </summary>
        public long Line
        {
            get { return this.line; }
        }

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Gets the modifier keys (SHIFT, CTRL, ALT) held down when double
        /// clicked.
        /// </summary>
        public Keys Modifiers
        {
            get { return this.modifiers; }
        }

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Gets the zero-based document position of the text double clicked.
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

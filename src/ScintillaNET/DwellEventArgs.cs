/*
 * DwellEventArgs.cs --
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
    /// Provides data for the <see cref="Scintilla.DwellStart" /> and
    /// <see cref="Scintilla.DwellEnd" /> events.
    /// </summary>
    [ObjectId("071e0ca5-2e33-43a3-b973-e1663938175b")]
    public class DwellEventArgs : EventArgs
    {
        #region Private Data
        /// <summary>
        /// The <see cref="Scintilla" /> control that generated this event.
        /// </summary>
        private readonly Scintilla scintilla;

        /// <summary>
        /// The zero-based byte position within the document where the mouse
        /// pointer was lingering.
        /// </summary>
        private readonly long bytePosition;

        /// <summary>
        /// The zero-based character position within the document, computed on
        /// demand from the byte position and cached here; otherwise, null if
        /// it has not yet been computed.
        /// </summary>
        private long? position;

        /// <summary>
        /// The x-coordinate of the mouse pointer relative to the
        /// <see cref="Scintilla" /> control.
        /// </summary>
        private int x;

        /// <summary>
        /// The y-coordinate of the mouse pointer relative to the
        /// <see cref="Scintilla" /> control.
        /// </summary>
        private int y;
        #endregion

        ///////////////////////////////////////////////////////////////////////

        #region Public Constructors
        /// <summary>
        /// Constructs an instance of this class using the specified control,
        /// byte position, and mouse pointer coordinates.
        /// </summary>
        /// <param name="scintilla">
        /// The <see cref="Scintilla" /> control that generated this event.
        /// </param>
        /// <param name="bytePosition">
        /// The zero-based byte position within the document where the mouse
        /// pointer was lingering.
        /// </param>
        /// <param name="x">
        /// The x-coordinate of the mouse pointer relative to the
        /// <see cref="Scintilla" /> control.
        /// </param>
        /// <param name="y">
        /// The y-coordinate of the mouse pointer relative to the
        /// <see cref="Scintilla" /> control.
        /// </param>
        public DwellEventArgs(
            Scintilla scintilla, /* in */
            long bytePosition,   /* in */
            int x,               /* in */
            int y                /* in */
            )
        {
            this.scintilla = scintilla;
            this.bytePosition = bytePosition;
            this.x = x;
            this.y = y;

            //
            // NOTE: The position is not over text.
            //
            if (bytePosition < 0)
                this.position = bytePosition;
        }
        #endregion

        ///////////////////////////////////////////////////////////////////////

        #region Public Properties
        /// <summary>
        /// Gets the zero-based document position where the mouse pointer was
        /// lingering.
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

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Gets the x-coordinate of the mouse pointer.
        /// </summary>
        public int X
        {
            get { return this.x; }
        }

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Gets the y-coordinate of the mouse pointer.
        /// </summary>
        public int Y
        {
            get { return this.y; }
        }
        #endregion
    }
}

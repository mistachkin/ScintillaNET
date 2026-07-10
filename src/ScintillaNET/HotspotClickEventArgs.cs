/*
 * HotspotClickEventArgs.cs --
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
    /// Provides data for the <see cref="Scintilla.HotspotClick" />,
    /// <see cref="Scintilla.HotspotDoubleClick" />, and
    /// <see cref="Scintilla.HotspotReleaseClick" /> events.
    /// </summary>
    [ObjectId("f7dcadac-ecd8-4262-87f7-ce6962bfa293")]
    public class HotspotClickEventArgs : EventArgs
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
        /// The zero-based character position within the document, computed on
        /// demand from the byte position and cached here; otherwise, null if
        /// it has not yet been computed.
        /// </summary>
        private long? position;

        /// <summary>
        /// The modifier keys (SHIFT, CTRL, ALT) held down when clicked.
        /// </summary>
        private Keys modifiers;
        #endregion

        ///////////////////////////////////////////////////////////////////////

        #region Public Constructors
        /// <summary>
        /// Constructs an instance of this class using the specified control,
        /// modifier keys, and byte position.
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
        public HotspotClickEventArgs(
            Scintilla scintilla, /* in */
            Keys modifiers,      /* in */
            long bytePosition    /* in */
            )
        {
            this.scintilla = scintilla;
            this.bytePosition = bytePosition;
            this.modifiers = modifiers;
        }
        #endregion

        ///////////////////////////////////////////////////////////////////////

        #region Public Properties
        /// <summary>
        /// Gets the modifier keys (SHIFT, CTRL, ALT) held down when clicked.
        /// </summary>
        /// <remarks>
        /// Only the state of the CTRL key is reported in the
        /// <see cref="Scintilla.HotspotReleaseClick" /> event.
        /// </remarks>
        public Keys Modifiers
        {
            get { return this.modifiers; }
        }

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Gets the zero-based document position of the text clicked.
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

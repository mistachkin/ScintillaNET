/*
 * NeedShownEventArgs.cs --
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
    /// Provides the data for the <see cref="Scintilla.NeedShown" /> event.
    /// </summary>
    [ObjectId("e0260f1b-cb0f-49f5-93a9-edfb912054ed")]
    public class NeedShownEventArgs : EventArgs
    {
        #region Private Data
        /// <summary>
        /// The <see cref="Scintilla" /> control that generated this event.
        /// </summary>
        private readonly Scintilla scintilla;

        /// <summary>
        /// The zero-based byte position within the document where text needs
        /// to be shown.
        /// </summary>
        private readonly long bytePosition;

        /// <summary>
        /// The length in bytes of the text that needs to be shown.
        /// </summary>
        private readonly long byteLength;

        /// <summary>
        /// The cached zero-based document position where the range of text to
        /// be shown starts, or null if it has not yet been computed.
        /// </summary>
        private long? position;

        /// <summary>
        /// The cached length, in characters, of the text that needs to be
        /// shown, or null if it has not yet been computed.
        /// </summary>
        private long? length;
        #endregion

        ///////////////////////////////////////////////////////////////////////

        #region Public Constructors
        /// <summary>
        /// Constructs an instance of this class using the specified control,
        /// byte position, and byte length.
        /// </summary>
        /// <param name="scintilla">
        /// The <see cref="Scintilla" /> control that generated this event.
        /// </param>
        /// <param name="bytePosition">
        /// The zero-based byte position within the document where text needs
        /// to be shown.
        /// </param>
        /// <param name="byteLength">
        /// The length in bytes of the text that needs to be shown.
        /// </param>
        public NeedShownEventArgs(
            Scintilla scintilla, /* in */
            long bytePosition,   /* in */
            long byteLength      /* in */
            )
        {
            this.scintilla = scintilla;
            this.bytePosition = bytePosition;
            this.byteLength = byteLength;
        }
        #endregion

        ///////////////////////////////////////////////////////////////////////

        #region Public Properties
        /// <summary>
        /// Gets the length of the text, starting at
        /// <see cref="Position" />, that needs to be shown.
        /// </summary>
        public long Length
        {
            get
            {
                if (this.length == null)
                {
                    long endBytePosition =
                        (this.bytePosition + this.byteLength);
                    long endPosition = this.scintilla.Lines.ByteToCharPosition(
                        endBytePosition);

                    this.length = (endPosition - Position);
                }

                return (long)this.length;
            }
        }

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Gets the zero-based document position where text needs to be shown.
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

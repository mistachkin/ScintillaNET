/*
 * InsertCheckEventArgs.cs --
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
    /// Provides the data for the <see cref="Scintilla.InsertCheck" /> event.
    /// </summary>
    [ObjectId("8ec7ac33-cc1e-4449-8c13-440f667b043b")]
    public class InsertCheckEventArgs : EventArgs
    {
        #region Private Data
        /// <summary>
        /// The <see cref="Scintilla" /> control that generated this event.
        /// </summary>
        private readonly Scintilla scintilla;

        /// <summary>
        /// The zero-based byte position within the document where text is
        /// being inserted.
        /// </summary>
        private readonly long bytePosition;

        /// <summary>
        /// The length, in bytes, of the inserted text.
        /// </summary>
        private readonly int byteLength;

        /// <summary>
        /// A pointer to the text being inserted.
        /// </summary>
        private readonly IntPtr textPtr;

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// The lazily-computed character position corresponding to the byte
        /// position, or null if it has not yet been computed.
        /// </summary>
        private long? cachedPosition;

        /// <summary>
        /// The lazily-retrieved or replacement text being inserted, or null
        /// if it has not yet been retrieved.
        /// </summary>
        private string cachedText;
        #endregion

        ///////////////////////////////////////////////////////////////////////

        #region Public Constructors
        /// <summary>
        /// Constructs an instance of this class using the specified
        /// <see cref="Scintilla" /> control, byte position, byte length,
        /// and text pointer.
        /// </summary>
        /// <param name="scintilla">
        /// The <see cref="Scintilla" /> control that generated this event.
        /// </param>
        /// <param name="bytePosition">
        /// The zero-based byte position within the document where text is
        /// being inserted.
        /// </param>
        /// <param name="byteLength">
        /// The length, in bytes, of the inserted text.
        /// </param>
        /// <param name="text">
        /// A pointer to the text being inserted.
        /// </param>
        public InsertCheckEventArgs(
            Scintilla scintilla, /* in */
            long bytePosition,   /* in */
            int byteLength,      /* in */
            IntPtr text          /* in */
            )
        {
            this.scintilla = scintilla;
            this.bytePosition = bytePosition;
            this.byteLength = byteLength;
            this.textPtr = text;
        }
        #endregion

        ///////////////////////////////////////////////////////////////////////

        #region Public Properties
        /// <summary>
        /// Gets the zero-based character position within the document where
        /// text will be inserted.
        /// </summary>
        public long Position
        {
            get
            {
                if (this.CachedPosition == null)
                {
                    this.CachedPosition =
                        this.scintilla.Lines.ByteToCharPosition(
                            this.bytePosition);
                }

                return (long)this.CachedPosition;
            }
        }

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Gets or sets the text being inserted into the document.
        /// </summary>
        public unsafe string Text
        {
            get
            {
                if (this.CachedText == null)
                {
                    this.CachedText = Helpers.GetString(
                        this.textPtr, this.byteLength,
                        this.scintilla.Encoding);
                }

                return this.CachedText;
            }
            set
            {
                this.CachedText = value ?? string.Empty;

                byte[] bytes = Helpers.GetBytes(
                    this.CachedText, this.scintilla.Encoding, false);

                fixed (byte* bp = bytes)
                {
                    this.scintilla.DirectMessage(
                        NativeMethods.SCI_CHANGEINSERTION,
                        new IntPtr(bytes.Length), new IntPtr(bp));
                }
            }
        }
        #endregion

        ///////////////////////////////////////////////////////////////////////

        #region Internal Properties
        /// <summary>
        /// Gets or sets the cached character position corresponding to the
        /// byte position, shared among the related modification events to
        /// avoid recomputing it.
        /// </summary>
        internal long? CachedPosition
        {
            get { return this.cachedPosition; }
            set { this.cachedPosition = value; }
        }

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Gets or sets the cached text being inserted, shared among the
        /// related modification events to avoid re-reading it.
        /// </summary>
        internal string CachedText
        {
            get { return this.cachedText; }
            set { this.cachedText = value; }
        }
        #endregion
    }
}

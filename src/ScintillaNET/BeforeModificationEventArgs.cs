/*
 * BeforeModificationEventArgs.cs --
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
    /// Provides the data for the <see cref="Scintilla.BeforeInsert" /> and
    /// <see cref="Scintilla.BeforeDelete" /> events.
    /// </summary>
    [ObjectId("e951eaf1-24ed-4953-af9c-c7d3de501848")]
    public class BeforeModificationEventArgs : EventArgs
    {
        #region Private Data
        /// <summary>
        /// The <see cref="Scintilla" /> control that generated this event.
        /// </summary>
        private readonly Scintilla scintilla;

        /// <summary>
        /// The source of the modification.
        /// </summary>
        private ModificationSource source;

        /// <summary>
        /// The zero-based byte position within the document where text is
        /// being modified.
        /// </summary>
        private readonly long bytePosition;

        /// <summary>
        /// The length, in bytes, of the text being modified.
        /// </summary>
        private readonly int byteLength;

        /// <summary>
        /// A pointer to the text being inserted, or IntPtr.Zero when the text
        /// must be read from the document (for example, during a delete).
        /// </summary>
        private readonly IntPtr textPtr;

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// The lazily-computed character position corresponding to the byte
        /// position, or null if it has not yet been computed.
        /// </summary>
        private long? cachedPosition;

        /// <summary>
        /// The lazily-retrieved text being inserted or deleted, or null if it
        /// has not yet been retrieved.
        /// </summary>
        private string cachedText;
        #endregion

        ///////////////////////////////////////////////////////////////////////

        #region Public Constructors
        /// <summary>
        /// Constructs an instance of this class using the specified
        /// <see cref="Scintilla" /> control, modification source, byte
        /// position, byte length, and text pointer.
        /// </summary>
        /// <param name="scintilla">
        /// The <see cref="Scintilla" /> control that generated this event.
        /// </param>
        /// <param name="source">
        /// The source of the modification.
        /// </param>
        /// <param name="bytePosition">
        /// The zero-based byte position within the document where text is
        /// being modified.
        /// </param>
        /// <param name="byteLength">
        /// The length, in bytes, of the text being modified.
        /// </param>
        /// <param name="text">
        /// A pointer to the text being inserted.
        /// </param>
        public BeforeModificationEventArgs(
            Scintilla scintilla,       /* in */
            ModificationSource source, /* in */
            long bytePosition,         /* in */
            int byteLength,            /* in */
            IntPtr text                /* in */
            )
        {
            this.scintilla = scintilla;
            this.source = source;
            this.bytePosition = bytePosition;
            this.byteLength = byteLength;
            this.textPtr = text;
        }
        #endregion

        ///////////////////////////////////////////////////////////////////////

        #region Public Properties
        /// <summary>
        /// Gets the zero-based character position within the document where
        /// text will be inserted or deleted.
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
        /// Gets the source of the modification, one of the
        /// <see cref="ModificationSource" /> enumeration values.
        /// </summary>
        public ModificationSource Source
        {
            get { return this.source; }
        }

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Gets the text about to be inserted or deleted.  This value is
        /// null when the source of the modification is an undo or redo
        /// operation (i.e. when <see cref="Source" /> is
        /// <see cref="ModificationSource.Undo" /> or
        /// <see cref="ModificationSource.Redo" />).
        /// </summary>
        public unsafe virtual string Text
        {
            get
            {
                if (this.Source != ModificationSource.User)
                    return null;

                if (this.CachedText == null)
                {
                    //
                    // NOTE: For some reason the Scintilla overlords do not
                    //       provide text in SC_MOD_BEFOREDELETE... but we
                    //       can get it from the document.
                    //
                    if (this.textPtr == IntPtr.Zero)
                    {
                        IntPtr ptr = this.scintilla.DirectMessage(
                            NativeMethods.SCI_GETRANGEPOINTER,
                            new IntPtr(this.bytePosition),
                            new IntPtr(this.byteLength));

                        this.CachedText = new string(
                            (sbyte*)ptr, 0, this.byteLength,
                            this.scintilla.Encoding);
                    }
                    else
                    {
                        this.CachedText = Helpers.GetString(
                            this.textPtr, this.byteLength,
                            this.scintilla.Encoding);
                    }
                }

                return this.CachedText;
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
        /// Gets or sets the cached text being inserted or deleted, shared
        /// among the related modification events to avoid re-reading it.
        /// </summary>
        internal string CachedText
        {
            get { return this.cachedText; }
            set { this.cachedText = value; }
        }
        #endregion
    }
}

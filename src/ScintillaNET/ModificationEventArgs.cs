/*
 * ModificationEventArgs.cs --
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
    /// Provides the data for the <see cref="Scintilla.Insert" /> and
    /// <see cref="Scintilla.Delete" /> events.
    /// </summary>
    [ObjectId("858b475c-0f9f-4789-baf6-bf6ecf628467")]
    public class ModificationEventArgs : BeforeModificationEventArgs
    {
        #region Private Data
        /// <summary>
        /// The <see cref="Scintilla" /> control that generated this event.
        /// </summary>
        private readonly Scintilla scintilla;

        /// <summary>
        /// The zero-based byte position within the document where text was
        /// modified.
        /// </summary>
        private readonly long bytePosition;

        /// <summary>
        /// The length in bytes of the inserted or deleted text.
        /// </summary>
        private readonly int byteLength;

        /// <summary>
        /// A pointer to the text that was inserted or deleted.
        /// </summary>
        private readonly IntPtr textPtr;

        /// <summary>
        /// The number of lines added or removed by the modification.  This
        /// value is negative when lines are removed from the document.
        /// </summary>
        private long linesAdded;
        #endregion

        ///////////////////////////////////////////////////////////////////////

        #region Public Constructors
        /// <summary>
        /// Constructs an instance of this class using the specified control,
        /// modification source, byte position, byte length, text pointer, and
        /// number of lines added or removed.
        /// </summary>
        /// <param name="scintilla">
        /// The <see cref="Scintilla" /> control that generated this event.
        /// </param>
        /// <param name="source">
        /// The source of the modification.
        /// </param>
        /// <param name="bytePosition">
        /// The zero-based byte position within the document where text was
        /// modified.
        /// </param>
        /// <param name="byteLength">
        /// The length in bytes of the inserted or deleted text.
        /// </param>
        /// <param name="text">
        /// A pointer to the text that was inserted or deleted.
        /// </param>
        /// <param name="linesAdded">
        /// The number of lines added or removed (delta).
        /// </param>
        public ModificationEventArgs(
            Scintilla scintilla,       /* in */
            ModificationSource source, /* in */
            long bytePosition,         /* in */
            int byteLength,            /* in */
            IntPtr text,               /* in */
            long linesAdded            /* in */
            )
            : base(scintilla, source, bytePosition, byteLength, text)
        {
            this.scintilla = scintilla;
            this.bytePosition = bytePosition;
            this.byteLength = byteLength;
            this.textPtr = text;

            this.linesAdded = linesAdded;
        }
        #endregion

        ///////////////////////////////////////////////////////////////////////

        #region Public Properties
        /// <summary>
        /// Gets the number of lines added or removed by the modification.
        /// This value is negative when lines are removed from the document.
        /// </summary>
        public long LinesAdded
        {
            get { return this.linesAdded; }
        }

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Gets the text that was inserted or deleted.
        /// </summary>
        public override unsafe string Text
        {
            get
            {
                if (CachedText == null)
                    CachedText = Helpers.GetString(
                        this.textPtr, this.byteLength,
                        this.scintilla.Encoding);

                return CachedText;
            }
        }
        #endregion
    }
}

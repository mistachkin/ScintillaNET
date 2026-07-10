/*
 * AutoCSelectionEventArgs.cs --
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
    /// Provides the data for the <see cref="Scintilla.AutoCSelection" />
    /// event.
    /// </summary>
    [ObjectId("96bd4bfd-9450-41e6-ac27-d87f9fc1419b")]
    public class AutoCSelectionEventArgs : EventArgs
    {
        #region Private Data
        /// <summary>
        /// The <see cref="Scintilla" /> control that generated this event.
        /// </summary>
        private readonly Scintilla scintilla;

        /// <summary>
        /// The zero-based byte position within the document of the word being
        /// completed.
        /// </summary>
        private readonly long bytePosition;

        /// <summary>
        /// A pointer to the selected autocompletion text.
        /// </summary>
        private readonly IntPtr textPtr;

        /// <summary>
        /// The character that caused the completion.
        /// </summary>
        private int ch;

        /// <summary>
        /// A value indicating the way in which the completion occurred.
        /// </summary>
        private ListCompletionMethod listCompletionMethod;

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// The lazily-computed character position of the word being completed,
        /// or null if it has not yet been computed.
        /// </summary>
        private long? position;

        /// <summary>
        /// The lazily-retrieved text of the selected autocompletion item, or
        /// null if it has not yet been retrieved.
        /// </summary>
        private string text;
        #endregion

        ///////////////////////////////////////////////////////////////////////

        #region Public Constructors
        /// <summary>
        /// Constructs an instance of this class using the specified
        /// <see cref="Scintilla" /> control, byte position, text pointer,
        /// character, and list completion method.
        /// </summary>
        /// <param name="scintilla">
        /// The <see cref="Scintilla" /> control that generated this event.
        /// </param>
        /// <param name="bytePosition">
        /// The zero-based byte position within the document of the word
        /// being completed.
        /// </param>
        /// <param name="text">
        /// A pointer to the selected autocompletion text.
        /// </param>
        /// <param name="ch">
        /// The character that caused the completion.
        /// </param>
        /// <param name="listCompletionMethod">
        /// A value indicating the way in which the completion occurred.
        /// </param>
        public AutoCSelectionEventArgs(
            Scintilla scintilla,                       /* in */
            long bytePosition,                         /* in */
            IntPtr text,                               /* in */
            int ch,                                    /* in */
            ListCompletionMethod listCompletionMethod  /* in */
            )
        {
            this.scintilla = scintilla;
            this.bytePosition = bytePosition;
            this.textPtr = text;
            this.ch = ch;
            this.listCompletionMethod = listCompletionMethod;
        }
        #endregion

        ///////////////////////////////////////////////////////////////////////

        #region Public Properties
        /// <summary>
        /// Gets the fillup character that caused the completion, or 0 when
        /// no fillup character was involved.  Only a
        /// <see cref="ListCompletionMethod" /> of
        /// <see cref="ScintillaNET.ListCompletionMethod.FillUp" /> will
        /// return a non-zero character.
        /// </summary>
        public int Char
        {
            get { return this.ch; }
        }

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Gets a value indicating how the completion occurred, one of the
        /// <see cref="ScintillaNET.ListCompletionMethod" /> enumeration
        /// values.
        /// </summary>
        public ListCompletionMethod ListCompletionMethod
        {
            get { return this.listCompletionMethod; }
        }

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Gets the zero-based document position of the word being
        /// completed.
        /// </summary>
        public long Position
        {
            get
            {
                if (this.position == null)
                {
                    this.position =
                        this.scintilla.Lines.ByteToCharPosition(
                            this.bytePosition);
                }

                return (long)this.position;
            }
        }

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Gets the text of the selected autocompletion item.
        /// </summary>
        public unsafe string Text
        {
            get
            {
                if (this.text == null)
                {
                    int len = 0;

                    while (((byte*)this.textPtr)[len] != 0)
                        len++;

                    this.text = Helpers.GetString(
                        this.textPtr, len, this.scintilla.Encoding);
                }

                return this.text;
            }
        }
        #endregion
    }
}

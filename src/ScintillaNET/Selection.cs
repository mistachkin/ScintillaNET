/*
 * Selection.cs --
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
    /// Represents a selection when there are multiple active selections in
    /// a <see cref="Scintilla" /> control.
    /// </summary>
    [ObjectId("9c49f192-4dd5-42c8-bba4-36ce7a240a06")]
    public class Selection
    {
        #region Private Data
        /// <summary>
        /// The <see cref="Scintilla" /> control that created this selection.
        /// </summary>
        private readonly Scintilla scintilla;

        /// <summary>
        /// The zero-based selection index within the
        /// <see cref="SelectionCollection" /> that created it.
        /// </summary>
        private readonly int index;
        #endregion

        ///////////////////////////////////////////////////////////////////////

        #region Public Constructors
        /// <summary>
        /// Constructs an instance of this class using the specified
        /// <see cref="Scintilla" /> control and selection index.
        /// </summary>
        /// <param name="scintilla">
        /// The <see cref="Scintilla" /> control that created this selection.
        /// </param>
        /// <param name="index">
        /// The index of this selection within the
        /// <see cref="SelectionCollection" /> that created it.
        /// </param>
        public Selection(
            Scintilla scintilla, /* in */
            int index            /* in */
            )
        {
            this.scintilla = scintilla;
            this.index = index;
        }
        #endregion

        ///////////////////////////////////////////////////////////////////////

        #region Public Properties
        /// <summary>
        /// Gets or sets the zero-based document position of the selection
        /// anchor.
        /// </summary>
        public long Anchor
        {
            get
            {
                long pos = this.scintilla.DirectMessage(
                    NativeMethods.SCI_GETSELECTIONNANCHOR,
                    new IntPtr(this.index)).ToInt64();

                if (pos <= 0)
                    return pos;

                return this.scintilla.Lines.ByteToCharPosition(pos);
            }
            set
            {
                value = Helpers.Clamp(value, 0, this.scintilla.TextLength);
                value = this.scintilla.Lines.CharToBytePosition(value);

                this.scintilla.DirectMessage(
                    NativeMethods.SCI_SETSELECTIONNANCHOR,
                    new IntPtr(this.index), new IntPtr(value));
            }
        }

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Gets or sets the amount of virtual space past the end of the line
        /// offsetting the selection anchor.
        /// </summary>
        public long AnchorVirtualSpace
        {
            get
            {
                return this.scintilla.DirectMessage(
                    NativeMethods.SCI_GETSELECTIONNANCHORVIRTUALSPACE,
                    new IntPtr(this.index)).ToInt64();
            }
            set
            {
                value = Helpers.ClampMin(value, 0);

                this.scintilla.DirectMessage(
                    NativeMethods.SCI_SETSELECTIONNANCHORVIRTUALSPACE,
                    new IntPtr(this.index), new IntPtr(value));
            }
        }

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Gets or sets the zero-based document position of the selection
        /// caret.
        /// </summary>
        public long Caret
        {
            get
            {
                long pos = this.scintilla.DirectMessage(
                    NativeMethods.SCI_GETSELECTIONNCARET,
                    new IntPtr(this.index)).ToInt64();

                if (pos <= 0)
                    return pos;

                return this.scintilla.Lines.ByteToCharPosition(pos);
            }
            set
            {
                value = Helpers.Clamp(value, 0, this.scintilla.TextLength);
                value = this.scintilla.Lines.CharToBytePosition(value);

                this.scintilla.DirectMessage(
                    NativeMethods.SCI_SETSELECTIONNCARET,
                    new IntPtr(this.index), new IntPtr(value));
            }
        }

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Gets or sets the amount of virtual space past the end of the line
        /// offsetting the selection caret.
        /// </summary>
        public long CaretVirtualSpace
        {
            get
            {
                return this.scintilla.DirectMessage(
                    NativeMethods.SCI_GETSELECTIONNCARETVIRTUALSPACE,
                    new IntPtr(this.index)).ToInt64();
            }
            set
            {
                value = Helpers.ClampMin(value, 0);

                this.scintilla.DirectMessage(
                    NativeMethods.SCI_SETSELECTIONNCARETVIRTUALSPACE,
                    new IntPtr(this.index), new IntPtr(value));
            }
        }

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Gets or sets the zero-based document position where the selection
        /// ends.
        /// </summary>
        public long End
        {
            get
            {
                long pos = this.scintilla.DirectMessage(
                    NativeMethods.SCI_GETSELECTIONNEND,
                    new IntPtr(this.index)).ToInt64();

                if (pos <= 0)
                    return pos;

                return this.scintilla.Lines.ByteToCharPosition(pos);
            }
            set
            {
                value = Helpers.Clamp(value, 0, this.scintilla.TextLength);
                value = this.scintilla.Lines.CharToBytePosition(value);

                this.scintilla.DirectMessage(
                    NativeMethods.SCI_SETSELECTIONNEND,
                    new IntPtr(this.index), new IntPtr(value));
            }
        }

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Gets the zero-based selection index within the
        /// <see cref="SelectionCollection" /> that created it.
        /// </summary>
        public int Index
        {
            get { return this.index; }
        }

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Gets or sets the zero-based document position where the selection
        /// starts.
        /// </summary>
        public long Start
        {
            get
            {
                long pos = this.scintilla.DirectMessage(
                    NativeMethods.SCI_GETSELECTIONNSTART,
                    new IntPtr(this.index)).ToInt64();

                if (pos <= 0)
                    return pos;

                return this.scintilla.Lines.ByteToCharPosition(pos);
            }
            set
            {
                value = Helpers.Clamp(value, 0, this.scintilla.TextLength);
                value = this.scintilla.Lines.CharToBytePosition(value);

                this.scintilla.DirectMessage(
                    NativeMethods.SCI_SETSELECTIONNSTART,
                    new IntPtr(this.index), new IntPtr(value));
            }
        }
        #endregion
    }
}

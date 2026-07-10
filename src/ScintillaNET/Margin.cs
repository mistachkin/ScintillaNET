/*
 * Margin.cs --
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
using System.Drawing;

namespace ScintillaNET
{
    /// <summary>
    /// Represents a margin displayed on the left edge of a
    /// <see cref="Scintilla" /> control.
    /// </summary>
    [ObjectId("9d8917c6-77e9-47a7-bc36-329b88c88e75")]
    public class Margin
    {
        #region Private Data
        /// <summary>
        /// The <see cref="Scintilla" /> control that created this margin.
        /// </summary>
        private readonly Scintilla scintilla;

        /// <summary>
        /// The zero-based margin index within the
        /// <see cref="MarginCollection" /> that created it.
        /// </summary>
        private readonly int index;
        #endregion

        ///////////////////////////////////////////////////////////////////////

        #region Public Constructors
        /// <summary>
        /// Constructs an instance of this class using the specified
        /// <see cref="Scintilla" /> control and margin index.
        /// </summary>
        /// <param name="scintilla">
        /// The <see cref="Scintilla" /> control that created this margin.
        /// </param>
        /// <param name="index">
        /// The index of this margin within the
        /// <see cref="MarginCollection" /> that created it.
        /// </param>
        public Margin(
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
        /// Gets or sets the background color of the margin when the
        /// <see cref="Type" /> property is set to
        /// <see cref="MarginType.Color" />.  The default is black.
        /// </summary>
        /// <remarks>
        /// Alpha color values are ignored.
        /// </remarks>
        public Color BackColor
        {
            get
            {
                int color = this.scintilla.DirectMessage(
                    NativeMethods.SCI_GETMARGINBACKN,
                    new IntPtr(this.index)).ToInt32();

                return ColorTranslator.FromWin32(color);
            }
            set
            {
                if (value.IsEmpty)
                    value = Color.Black;

                int color = ColorTranslator.ToWin32(value);

                this.scintilla.DirectMessage(
                    NativeMethods.SCI_SETMARGINBACKN,
                    new IntPtr(this.index), new IntPtr(color));
            }
        }

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Gets or sets the mouse cursor style when over the margin, one of
        /// the <see cref="MarginCursor" /> enumeration values.  The default
        /// is <see cref="MarginCursor.Arrow" />.
        /// </summary>
        public MarginCursor Cursor
        {
            get
            {
                return (MarginCursor)this.scintilla.DirectMessage(
                    NativeMethods.SCI_GETMARGINCURSORN,
                    new IntPtr(this.index));
            }
            set
            {
                int cursor = (int)value;

                this.scintilla.DirectMessage(
                    NativeMethods.SCI_SETMARGINCURSORN,
                    new IntPtr(this.index), new IntPtr(cursor));
            }
        }

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Gets the zero-based margin index this object represents within the
        /// <see cref="MarginCollection" />.
        /// </summary>
        public int Index
        {
            get { return this.index; }
        }

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Gets or sets whether the margin is sensitive to mouse clicks.  The
        /// default is false.
        /// </summary>
        public bool Sensitive
        {
            get
            {
                return (this.scintilla.DirectMessage(
                    NativeMethods.SCI_GETMARGINSENSITIVEN,
                    new IntPtr(this.index)) != IntPtr.Zero);
            }
            set
            {
                IntPtr sensitive = (value ? new IntPtr(1) : IntPtr.Zero);

                this.scintilla.DirectMessage(
                    NativeMethods.SCI_SETMARGINSENSITIVEN,
                    new IntPtr(this.index), sensitive);
            }
        }

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Gets or sets the margin type, one of the <see cref="MarginType" />
        /// enumeration values.  The default is
        /// <see cref="MarginType.Symbol" />.
        /// </summary>
        public MarginType Type
        {
            get
            {
                return (MarginType)(this.scintilla.DirectMessage(
                    NativeMethods.SCI_GETMARGINTYPEN,
                    new IntPtr(this.index)));
            }
            set
            {
                int type = (int)value;

                this.scintilla.DirectMessage(
                    NativeMethods.SCI_SETMARGINTYPEN,
                    new IntPtr(this.index), new IntPtr(type));
            }
        }

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Gets or sets the width in pixels of the margin.
        /// </summary>
        /// <remarks>
        /// Scintilla assigns various default widths.
        /// </remarks>
        public int Width
        {
            get
            {
                return this.scintilla.DirectMessage(
                    NativeMethods.SCI_GETMARGINWIDTHN,
                    new IntPtr(this.index)).ToInt32();
            }
            set
            {
                value = Helpers.ClampMin(value, 0);

                this.scintilla.DirectMessage(
                    NativeMethods.SCI_SETMARGINWIDTHN,
                    new IntPtr(this.index), new IntPtr(value));
            }
        }

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Gets or sets a mask indicating which markers this margin can
        /// display.  This is an unsigned 32-bit value with each bit
        /// corresponding to one of the 32 zero-based <see cref="Margin" />
        /// indexes.  The default is 0x1FFFFFF, which is every marker except
        /// folder markers (i.e. 0 through 24).
        /// </summary>
        /// <remarks>
        /// For example, the mask for marker index 10 is 1 shifted left 10
        /// times (1 &lt;&lt; 10).  <see cref="Marker.MaskFolders" /> is a
        /// useful constant for working with just folder margin indexes.
        /// </remarks>
        public uint Mask
        {
            get
            {
                return unchecked((uint)this.scintilla.DirectMessage(
                    NativeMethods.SCI_GETMARGINMASKN,
                    new IntPtr(this.index)).ToInt32());
            }
            set
            {
                int mask = unchecked((int)value);

                this.scintilla.DirectMessage(
                    NativeMethods.SCI_SETMARGINMASKN,
                    new IntPtr(this.index), new IntPtr(mask));
            }
        }
        #endregion
    }
}

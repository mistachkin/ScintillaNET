/*
 * Marker.cs --
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
    /// Represents a margin marker in a <see cref="Scintilla" /> control.
    /// </summary>
    [ObjectId("7d452ffc-0c83-42bc-9b05-c5feb55b7429")]
    public class Marker
    {
        #region Public Constants
        /// <summary>
        /// An unsigned 32-bit mask of all <see cref="Margin" /> indexes where
        /// each bit corresponds to a margin index.
        /// </summary>
        public const uint MaskAll = unchecked((uint)-1);

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// An unsigned 32-bit mask of folder <see cref="Margin" /> indexes
        /// (25 through 31) where each bit corresponds to a margin index.
        /// </summary>
        public const uint MaskFolders = NativeMethods.SC_MASK_FOLDERS;

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Folder end marker index.  This marker is typically configured to
        /// display the <see cref="MarkerSymbol.BoxPlusConnected" /> symbol.
        /// </summary>
        public const int FolderEnd = NativeMethods.SC_MARKNUM_FOLDEREND;

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Folder open marker index.  This marker is typically configured to
        /// display the <see cref="MarkerSymbol.BoxMinusConnected" /> symbol.
        /// </summary>
        public const int FolderOpenMid =
            NativeMethods.SC_MARKNUM_FOLDEROPENMID;

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Folder mid tail marker index.  This marker is typically configured
        /// to display the <see cref="MarkerSymbol.TCorner" /> symbol.
        /// </summary>
        public const int FolderMidTail =
            NativeMethods.SC_MARKNUM_FOLDERMIDTAIL;

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Folder tail marker index.  This marker is typically configured to
        /// display the <see cref="MarkerSymbol.LCorner" /> symbol.
        /// </summary>
        public const int FolderTail = NativeMethods.SC_MARKNUM_FOLDERTAIL;

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Folder sub marker index.  This marker is typically configured to
        /// display the <see cref="MarkerSymbol.VLine" /> symbol.
        /// </summary>
        public const int FolderSub = NativeMethods.SC_MARKNUM_FOLDERSUB;

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Folder marker index.  This marker is typically configured to
        /// display the <see cref="MarkerSymbol.BoxPlus" /> symbol.
        /// </summary>
        public const int Folder = NativeMethods.SC_MARKNUM_FOLDER;

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Folder open marker index.  This marker is typically configured to
        /// display the <see cref="MarkerSymbol.BoxMinus" /> symbol.
        /// </summary>
        public const int FolderOpen = NativeMethods.SC_MARKNUM_FOLDEROPEN;
        #endregion

        ///////////////////////////////////////////////////////////////////////

        #region Private Data
        /// <summary>
        /// The <see cref="Scintilla" /> control that created this marker.
        /// </summary>
        private readonly Scintilla scintilla;

        /// <summary>
        /// The zero-based marker index within the
        /// <see cref="MarkerCollection" /> that created it.
        /// </summary>
        private readonly int index;
        #endregion

        ///////////////////////////////////////////////////////////////////////

        #region Public Constructors
        /// <summary>
        /// Constructs an instance of this class using the specified
        /// <see cref="Scintilla" /> control and marker index.
        /// </summary>
        /// <param name="scintilla">
        /// The <see cref="Scintilla" /> control that created this marker.
        /// </param>
        /// <param name="index">
        /// The index of this marker within the
        /// <see cref="MarkerCollection" /> that created it.
        /// </param>
        public Marker(
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
        /// Gets the zero-based marker index this object represents within the
        /// <see cref="MarkerCollection" />.
        /// </summary>
        public int Index
        {
            get { return this.index; }
        }

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Gets or sets the marker symbol, one of the
        /// <see cref="MarkerSymbol" /> enumeration values.  The default is
        /// <see cref="MarkerSymbol.Circle" />.
        /// </summary>
        public MarkerSymbol Symbol
        {
            get
            {
                return (MarkerSymbol)this.scintilla.DirectMessage(
                    NativeMethods.SCI_MARKERSYMBOLDEFINED,
                    new IntPtr(this.index));
            }
            set
            {
                int markerSymbol = (int)value;

                this.scintilla.DirectMessage(
                    NativeMethods.SCI_MARKERDEFINE,
                    new IntPtr(this.index), new IntPtr(markerSymbol));
            }
        }
        #endregion

        ///////////////////////////////////////////////////////////////////////

        #region Public Methods
        /// <summary>
        /// Sets the marker symbol to a custom image.
        /// </summary>
        /// <param name="image">
        /// The <see cref="Bitmap" /> to use as a marker symbol.
        /// </param>
        /// <remarks>
        /// Calling this method will also update the <see cref="Symbol" />
        /// property to <see cref="MarkerSymbol.RgbaImage" />.
        /// </remarks>
        public unsafe void DefineRgbaImage(
            Bitmap image /* in */
            )
        {
            if (image == null)
                return;

            this.scintilla.DirectMessage(
                NativeMethods.SCI_RGBAIMAGESETWIDTH,
                new IntPtr(image.Width));

            this.scintilla.DirectMessage(
                NativeMethods.SCI_RGBAIMAGESETHEIGHT,
                new IntPtr(image.Height));

            byte[] bytes = Helpers.BitmapToArgb(image);

            fixed (byte* bp = bytes)
            {
                this.scintilla.DirectMessage(
                    NativeMethods.SCI_MARKERDEFINERGBAIMAGE,
                    new IntPtr(this.index), new IntPtr(bp));
            }
        }

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Removes this marker from all lines.
        /// </summary>
        public void DeleteAll()
        {
            this.scintilla.MarkerDeleteAll(this.index);
        }

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Sets the foreground alpha transparency for markers that are drawn
        /// in the content area.
        /// </summary>
        /// <param name="alpha">
        /// The alpha transparency ranging from 0 (completely transparent) to
        /// 255 (no transparency).
        /// </param>
        /// <remarks>
        /// See the remarks on the <see cref="SetBackColor" /> method for a
        /// full explanation of when a marker can be drawn in the content area.
        /// </remarks>
        public void SetAlpha(
            int alpha /* in */
            )
        {
            alpha = Helpers.Clamp(alpha, 0, 255);

            this.scintilla.DirectMessage(
                NativeMethods.SCI_MARKERSETALPHA,
                new IntPtr(this.index), new IntPtr(alpha));
        }

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Sets the background color of the marker.
        /// </summary>
        /// <param name="color">
        /// The <see cref="Marker" /> background color.  The default is white.
        /// </param>
        /// <remarks>
        /// The background color of the whole line will be drawn in the
        /// <paramref name="color" /> specified when the marker is not visible
        /// because it is hidden by a <see cref="Margin.Mask" /> or the
        /// <see cref="Margin.Width" /> is zero.
        /// </remarks>
        public void SetBackColor(
            Color color /* in */
            )
        {
            int colour = ColorTranslator.ToWin32(color);

            this.scintilla.DirectMessage(
                NativeMethods.SCI_MARKERSETBACK,
                new IntPtr(this.index), new IntPtr(colour));
        }

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Sets the foreground color of the marker.
        /// </summary>
        /// <param name="color">
        /// The <see cref="Marker" /> foreground color.  The default is black.
        /// </param>
        public void SetForeColor(
            Color color /* in */
            )
        {
            int colour = ColorTranslator.ToWin32(color);

            this.scintilla.DirectMessage(
                NativeMethods.SCI_MARKERSETFORE,
                new IntPtr(this.index), new IntPtr(colour));
        }
        #endregion
    }
}

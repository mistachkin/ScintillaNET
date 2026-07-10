/*
 * Line.cs --
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
    /// Represents a line of text in a <see cref="Scintilla" /> control.
    /// </summary>
    [ObjectId("83508ea1-b441-4dc0-83f8-e20ad89f2d6e")]
    public class Line
    {
        #region Private Data
        /// <summary>
        /// The <see cref="Scintilla" /> control that created this line.
        /// </summary>
        private readonly Scintilla scintilla;

        /// <summary>
        /// The zero-based line index within the <see cref="LineCollection" />
        /// that created this line.
        /// </summary>
        private long index;
        #endregion

        ///////////////////////////////////////////////////////////////////////

        #region Public Constructors
        /// <summary>
        /// Constructs an instance of this class using the specified
        /// <see cref="Scintilla" /> control and line index.
        /// </summary>
        /// <param name="scintilla">
        /// The <see cref="Scintilla" /> control that created this line.
        /// </param>
        /// <param name="index">
        /// The index of this line within the <see cref="LineCollection" />
        /// that created it.
        /// </param>
        public Line(
            Scintilla scintilla, /* in */
            long index           /* in */
            )
        {
            this.scintilla = scintilla;
            this.Index = index;
        }
        #endregion

        ///////////////////////////////////////////////////////////////////////

        #region Public Properties
        /// <summary>
        /// Gets the number of annotation lines of text.
        /// </summary>
        public int AnnotationLines
        {
            get
            {
                return this.scintilla.DirectMessage(
                    NativeMethods.SCI_ANNOTATIONGETLINES,
                    new IntPtr(this.Index)).ToInt32();
            }
        }

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Gets or sets the style of the annotation text.  This is the
        /// zero-based index of the annotation text <see cref="Style" />, or
        /// 256 when <see cref="AnnotationStyles" /> has been used to set
        /// individual character styles.
        /// </summary>
        public int AnnotationStyle
        {
            get
            {
                return this.scintilla.DirectMessage(
                    NativeMethods.SCI_ANNOTATIONGETSTYLE,
                    new IntPtr(this.Index)).ToInt32();
            }
            set
            {
                value = Helpers.Clamp(
                    value, 0, this.scintilla.Styles.Count - 1);

                this.scintilla.DirectMessage(
                    NativeMethods.SCI_ANNOTATIONSETSTYLE,
                    new IntPtr(this.Index), new IntPtr(value));
            }
        }

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Gets or sets an array of style indexes corresponding to each
        /// character in the <see cref="AnnotationText" /> so that each
        /// character may be individually styled.
        /// <see cref="AnnotationText" /> must be set prior to setting this
        /// property, and the array should have a length equal to the
        /// <see cref="AnnotationText" /> length to properly style all
        /// characters.
        /// </summary>
        public unsafe byte[] AnnotationStyles
        {
            get
            {
                int length = this.scintilla.DirectMessage(
                    NativeMethods.SCI_ANNOTATIONGETTEXT,
                    new IntPtr(this.Index)).ToInt32();

                if (length == 0)
                    return new byte[0];

                byte[] text = new byte[length + 1];
                byte[] styles = new byte[length + 1];

                fixed (byte* textPtr = text)
                fixed (byte* stylePtr = styles)
                {
                    this.scintilla.DirectMessage(
                        NativeMethods.SCI_ANNOTATIONGETTEXT,
                        new IntPtr(this.Index), new IntPtr(textPtr));

                    this.scintilla.DirectMessage(
                        NativeMethods.SCI_ANNOTATIONGETSTYLES,
                        new IntPtr(this.Index), new IntPtr(stylePtr));

                    return Helpers.ByteToCharStyles(
                        stylePtr, textPtr, length, this.scintilla.Encoding);
                }
            }
            set
            {
                int length = this.scintilla.DirectMessage(
                    NativeMethods.SCI_ANNOTATIONGETTEXT,
                    new IntPtr(this.Index)).ToInt32();

                if (length == 0)
                    return;

                byte[] text = new byte[length + 1];

                fixed (byte* textPtr = text)
                {
                    this.scintilla.DirectMessage(
                        NativeMethods.SCI_ANNOTATIONGETTEXT,
                        new IntPtr(this.Index), new IntPtr(textPtr));

                    byte[] styles = Helpers.CharToByteStyles(
                        value ?? new byte[0], textPtr, length,
                        this.scintilla.Encoding);

                    fixed (byte* stylePtr = styles)
                        this.scintilla.DirectMessage(
                            NativeMethods.SCI_ANNOTATIONSETSTYLES,
                            new IntPtr(this.Index), new IntPtr(stylePtr));
                }
            }
        }

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Gets or sets the line annotation text.
        /// </summary>
        public unsafe string AnnotationText
        {
            get
            {
                int length = this.scintilla.DirectMessage(
                    NativeMethods.SCI_ANNOTATIONGETTEXT,
                    new IntPtr(this.Index)).ToInt32();

                if (length == 0)
                    return string.Empty;

                byte[] bytes = new byte[length + 1];

                fixed (byte* bp = bytes)
                {
                    this.scintilla.DirectMessage(
                        NativeMethods.SCI_ANNOTATIONGETTEXT,
                        new IntPtr(this.Index), new IntPtr(bp));

                    return Helpers.GetString(
                        new IntPtr(bp), length, this.scintilla.Encoding);
                }
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    //
                    // NOTE: Scintilla docs suggest that setting to NULL
                    //       rather than an empty string will free memory.
                    //
                    this.scintilla.DirectMessage(
                        NativeMethods.SCI_ANNOTATIONSETTEXT,
                        new IntPtr(this.Index), IntPtr.Zero);
                }
                else
                {
                    byte[] bytes = Helpers.GetBytes(
                        value, this.scintilla.Encoding, true);

                    fixed (byte* bp = bytes)
                        this.scintilla.DirectMessage(
                            NativeMethods.SCI_ANNOTATIONSETTEXT,
                            new IntPtr(this.Index), new IntPtr(bp));
                }
            }
        }

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Searches from the current line to find the zero-based index of the
        /// next contracted fold header.  If the current line is contracted
        /// the current line index is returned.
        /// </summary>
        public long ContractedFoldNext
        {
            get
            {
                return this.scintilla.DirectMessage(
                    NativeMethods.SCI_CONTRACTEDFOLDNEXT,
                    new IntPtr(this.Index)).ToInt64();
            }
        }

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Gets the zero-based index of the line as displayed in a
        /// <see cref="Scintilla" /> control, taking into consideration folded
        /// (hidden) lines.
        /// </summary>
        public long DisplayIndex
        {
            get
            {
                return this.scintilla.DirectMessage(
                    NativeMethods.SCI_VISIBLEFROMDOCLINE,
                    new IntPtr(this.Index)).ToInt64();
            }
        }

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Gets the zero-based character position in the document where the
        /// line ends (exclusive).  This is the equivalent of
        /// <see cref="Position" /> + <see cref="Length" />.
        /// </summary>
        public long EndPosition
        {
            get
            {
                return this.Position + this.Length;
            }
        }

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Gets or sets the expanded state (not the visible state) of the
        /// line.  To toggle the fold state of a single line the
        /// <see cref="ToggleFold" /> method should be used; this property is
        /// useful for toggling the state of many folds without updating the
        /// display until finished.
        /// </summary>
        public bool Expanded
        {
            get
            {
                return (this.scintilla.DirectMessage(
                    NativeMethods.SCI_GETFOLDEXPANDED,
                    new IntPtr(this.Index)) != IntPtr.Zero);
            }
            set
            {
                IntPtr expanded = (value ? new IntPtr(1) : IntPtr.Zero);

                this.scintilla.DirectMessage(
                    NativeMethods.SCI_SETFOLDEXPANDED,
                    new IntPtr(this.Index), expanded);
            }
        }

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Gets or sets the fold level of the line, ranging from 0 to 4095.
        /// The default is 1024.
        /// </summary>
        public int FoldLevel
        {
            get
            {
                int level = this.scintilla.DirectMessage(
                    NativeMethods.SCI_GETFOLDLEVEL,
                    new IntPtr(this.Index)).ToInt32();

                return (level & NativeMethods.SC_FOLDLEVELNUMBERMASK);
            }
            set
            {
                int bits = (int)this.FoldLevelFlags;
                bits |= value;

                this.scintilla.DirectMessage(
                    NativeMethods.SCI_SETFOLDLEVEL,
                    new IntPtr(this.Index), new IntPtr(bits));
            }
        }

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Gets or sets the fold level flags, a bitwise combination of the
        /// <see cref="ScintillaNET.FoldLevelFlags" /> enumeration values.
        /// </summary>
        public FoldLevelFlags FoldLevelFlags
        {
            get
            {
                int flags = this.scintilla.DirectMessage(
                    NativeMethods.SCI_GETFOLDLEVEL,
                    new IntPtr(this.Index)).ToInt32();

                return (FoldLevelFlags)(
                    flags & ~NativeMethods.SC_FOLDLEVELNUMBERMASK);
            }
            set
            {
                int bits = this.FoldLevel;
                bits |= (int)value;

                this.scintilla.DirectMessage(
                    NativeMethods.SCI_SETFOLDLEVEL,
                    new IntPtr(this.Index), new IntPtr(bits));
            }
        }

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Gets the zero-based line index of the first line before the
        /// current line that is marked as
        /// <see cref="ScintillaNET.FoldLevelFlags.Header" /> and has a
        /// <see cref="FoldLevel" /> less than the current line, or -1 if
        /// there is no such line.
        /// </summary>
        public long FoldParent
        {
            get
            {
                return this.scintilla.DirectMessage(
                    NativeMethods.SCI_GETFOLDPARENT,
                    new IntPtr(this.Index)).ToInt64();
            }
        }

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Gets the height of the line in pixels.  Currently all lines are
        /// the same height.
        /// </summary>
        public int Height
        {
            get
            {
                return this.scintilla.DirectMessage(
                    NativeMethods.SCI_TEXTHEIGHT,
                    new IntPtr(this.Index)).ToInt32();
            }
        }

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Gets the zero-based line index within the
        /// <see cref="LineCollection" /> that created it.
        /// </summary>
        public long Index
        {
            get { return this.index; }
            private set { this.index = value; }
        }

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Gets the length of the line, the number of characters in the line
        /// including any end of line characters.
        /// </summary>
        public long Length
        {
            get
            {
                return this.scintilla.Lines.CharLineLength(this.Index);
            }
        }

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Gets or sets the style of the margin text in a
        /// <see cref="MarginType.Text" /> or
        /// <see cref="MarginType.RightText" /> margin.  This is the zero-based
        /// index of the margin text <see cref="Style" />, or 256 when
        /// <see cref="MarginStyles" /> has been used to set individual
        /// character styles.
        /// </summary>
        public int MarginStyle
        {
            get
            {
                return this.scintilla.DirectMessage(
                    NativeMethods.SCI_MARGINGETSTYLE,
                    new IntPtr(this.Index)).ToInt32();
            }
            set
            {
                value = Helpers.Clamp(
                    value, 0, this.scintilla.Styles.Count - 1);

                this.scintilla.DirectMessage(
                    NativeMethods.SCI_MARGINSETSTYLE,
                    new IntPtr(this.Index), new IntPtr(value));
            }
        }

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Gets or sets an array of style indexes corresponding to each
        /// character in the <see cref="MarginText" /> so that each character
        /// may be individually styled.  <see cref="MarginText" /> must be set
        /// prior to setting this property, and the array should have a
        /// length equal to the <see cref="MarginText" /> length to properly
        /// style all characters.
        /// </summary>
        public unsafe byte[] MarginStyles
        {
            get
            {
                int length = this.scintilla.DirectMessage(
                    NativeMethods.SCI_MARGINGETTEXT,
                    new IntPtr(this.Index)).ToInt32();

                if (length == 0)
                    return new byte[0];

                byte[] text = new byte[length + 1];
                byte[] styles = new byte[length + 1];

                fixed (byte* textPtr = text)
                fixed (byte* stylePtr = styles)
                {
                    this.scintilla.DirectMessage(
                        NativeMethods.SCI_MARGINGETTEXT,
                        new IntPtr(this.Index), new IntPtr(textPtr));

                    this.scintilla.DirectMessage(
                        NativeMethods.SCI_MARGINGETSTYLES,
                        new IntPtr(this.Index), new IntPtr(stylePtr));

                    return Helpers.ByteToCharStyles(
                        stylePtr, textPtr, length, this.scintilla.Encoding);
                }
            }
            set
            {
                int length = this.scintilla.DirectMessage(
                    NativeMethods.SCI_MARGINGETTEXT,
                    new IntPtr(this.Index)).ToInt32();

                if (length == 0)
                    return;

                byte[] text = new byte[length + 1];

                fixed (byte* textPtr = text)
                {
                    this.scintilla.DirectMessage(
                        NativeMethods.SCI_MARGINGETTEXT,
                        new IntPtr(this.Index), new IntPtr(textPtr));

                    byte[] styles = Helpers.CharToByteStyles(
                        value ?? new byte[0], textPtr, length,
                        this.scintilla.Encoding);

                    fixed (byte* stylePtr = styles)
                        this.scintilla.DirectMessage(
                            NativeMethods.SCI_MARGINSETSTYLES,
                            new IntPtr(this.Index), new IntPtr(stylePtr));
                }
            }
        }

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Gets or sets the text displayed in the line margin when the margin
        /// type is <see cref="MarginType.Text" /> or
        /// <see cref="MarginType.RightText" />.
        /// </summary>
        public unsafe string MarginText
        {
            get
            {
                int length = this.scintilla.DirectMessage(
                    NativeMethods.SCI_MARGINGETTEXT,
                    new IntPtr(this.Index)).ToInt32();

                if (length == 0)
                    return string.Empty;

                byte[] bytes = new byte[length + 1];

                fixed (byte* bp = bytes)
                {
                    this.scintilla.DirectMessage(
                        NativeMethods.SCI_MARGINGETTEXT,
                        new IntPtr(this.Index), new IntPtr(bp));

                    return Helpers.GetString(
                        new IntPtr(bp), length, this.scintilla.Encoding);
                }
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    //
                    // NOTE: Scintilla docs suggest that setting to NULL
                    //       rather than an empty string will free memory.
                    //
                    this.scintilla.DirectMessage(
                        NativeMethods.SCI_MARGINSETTEXT,
                        new IntPtr(this.Index), IntPtr.Zero);
                }
                else
                {
                    byte[] bytes = Helpers.GetBytes(
                        value, this.scintilla.Encoding, true);

                    fixed (byte* bp = bytes)
                        this.scintilla.DirectMessage(
                            NativeMethods.SCI_MARGINSETTEXT,
                            new IntPtr(this.Index), new IntPtr(bp));
                }
            }
        }

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Gets the zero-based character position in the document where the
        /// line begins.
        /// </summary>
        public long Position
        {
            get
            {
                return this.scintilla.Lines.CharPositionFromLine(this.Index);
            }
        }

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Gets the line text, including any end of line characters.
        /// </summary>
        public unsafe string Text
        {
            get
            {
                IntPtr start = this.scintilla.DirectMessage(
                    NativeMethods.SCI_POSITIONFROMLINE,
                    new IntPtr(this.Index));

                IntPtr length = this.scintilla.DirectMessage(
                    NativeMethods.SCI_LINELENGTH, new IntPtr(this.Index));

                IntPtr ptr = this.scintilla.DirectMessage(
                    NativeMethods.SCI_GETRANGEPOINTER, start, length);

                if (ptr == IntPtr.Zero)
                    return string.Empty;

                string text = new string(
                    (sbyte*)ptr, 0, length.ToInt32(), this.scintilla.Encoding);

                return text;
            }
        }

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Gets or sets the line indentation, measured in character columns
        /// (corresponding to the width of space characters).
        /// </summary>
        public int Indentation
        {
            get
            {
                return (this.scintilla.DirectMessage(
                    NativeMethods.SCI_GETLINEINDENTATION,
                    new IntPtr(this.Index)).ToInt32());
            }
            set
            {
                this.scintilla.DirectMessage(
                    NativeMethods.SCI_SETLINEINDENTATION,
                    new IntPtr(this.Index), new IntPtr(value));
            }
        }

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Gets a value indicating whether the line is visible; true if the
        /// line is visible, otherwise false.
        /// </summary>
        public bool Visible
        {
            get
            {
                return (this.scintilla.DirectMessage(
                    NativeMethods.SCI_GETLINEVISIBLE,
                    new IntPtr(this.Index)) != IntPtr.Zero);
            }
        }

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Gets the number of display lines this line would occupy when
        /// wrapping is enabled.
        /// </summary>
        public long WrapCount
        {
            get
            {
                return this.scintilla.DirectMessage(
                    NativeMethods.SCI_WRAPCOUNT,
                    new IntPtr(this.Index)).ToInt64();
            }
        }
        #endregion

        ///////////////////////////////////////////////////////////////////////

        #region Public Methods
        /// <summary>
        /// Expands any parent folds to ensure the line is visible.
        /// </summary>
        public void EnsureVisible()
        {
            this.scintilla.DirectMessage(
                NativeMethods.SCI_ENSUREVISIBLE, new IntPtr(this.Index));
        }

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Performs the specified fold action on the current line and all
        /// child lines.
        /// </summary>
        /// <param name="action">
        /// One of the <see cref="FoldAction" /> enumeration values.
        /// </param>
        public void FoldChildren(
            FoldAction action /* in */
            )
        {
            this.scintilla.DirectMessage(
                NativeMethods.SCI_FOLDCHILDREN,
                new IntPtr(this.Index), new IntPtr((int)action));
        }

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Performs the specified fold action on the current line.
        /// </summary>
        /// <param name="action">
        /// One of the <see cref="FoldAction" /> enumeration values.
        /// </param>
        public void FoldLine(
            FoldAction action /* in */
            )
        {
            this.scintilla.DirectMessage(
                NativeMethods.SCI_FOLDLINE,
                new IntPtr(this.Index), new IntPtr((int)action));
        }

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Searches for the next line that has a folding level that is less
        /// than or equal to <paramref name="level" /> and returns the
        /// previous line index.
        /// </summary>
        /// <param name="level">
        /// The level of the line to search for.  A value of -1 will use the
        /// current line <see cref="FoldLevel" />.
        /// </param>
        /// <returns>
        /// The zero-based index of the next line that has a
        /// <see cref="FoldLevel" /> less than or equal to
        /// <paramref name="level" />.  If the current line is a fold point
        /// and <paramref name="level" /> is -1 the index returned is the last
        /// line that would be made visible or hidden by toggling the fold
        /// state.
        /// </returns>
        public long GetLastChild(
            int level /* in */
            )
        {
            return this.scintilla.DirectMessage(
                NativeMethods.SCI_GETLASTCHILD,
                new IntPtr(this.Index), new IntPtr(level)).ToInt64();
        }

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Navigates the caret to the start of the line, discarding any
        /// selection.
        /// </summary>
        public void Goto()
        {
            this.scintilla.DirectMessage(
                NativeMethods.SCI_GOTOLINE, new IntPtr(this.Index));
        }

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Adds the specified <see cref="Marker" /> to the line.  This method
        /// does not check whether the line already contains the
        /// <paramref name="marker" />.
        /// </summary>
        /// <param name="marker">
        /// The zero-based index of the marker to add to the line.
        /// </param>
        /// <returns>
        /// A <see cref="MarkerHandle" /> which can be used to track the line.
        /// </returns>
        public MarkerHandle MarkerAdd(
            int marker /* in */
            )
        {
            marker = Helpers.Clamp(
                marker, 0, this.scintilla.Markers.Count - 1);

            IntPtr handle = this.scintilla.DirectMessage(
                NativeMethods.SCI_MARKERADD,
                new IntPtr(this.Index), new IntPtr(marker));

            return new MarkerHandle(handle);
        }

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Adds one or more markers to the line in a single call using a bit
        /// mask.
        /// </summary>
        /// <param name="markerMask">
        /// An unsigned 32-bit value with each bit corresponding to one of the
        /// 32 zero-based <see cref="Margin" /> indexes to add.
        /// </param>
        public void MarkerAddSet(
            uint markerMask /* in */
            )
        {
            int mask = unchecked((int)markerMask);

            this.scintilla.DirectMessage(
                NativeMethods.SCI_MARKERADDSET,
                new IntPtr(this.Index), new IntPtr(mask));
        }

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Removes the specified <see cref="Marker" /> from the line.  If the
        /// same marker has been added to the line more than once, this
        /// deletes one copy each time it is used.
        /// </summary>
        /// <param name="marker">
        /// The zero-based index of the marker to remove from the line, or -1
        /// to delete all markers from the line.
        /// </param>
        public void MarkerDelete(
            int marker /* in */
            )
        {
            marker = Helpers.Clamp(
                marker, -1, this.scintilla.Markers.Count - 1);

            this.scintilla.DirectMessage(
                NativeMethods.SCI_MARKERDELETE,
                new IntPtr(this.Index), new IntPtr(marker));
        }

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Returns a bit mask indicating which markers are present on the
        /// line.
        /// </summary>
        /// <returns>
        /// An unsigned 32-bit value with each bit corresponding to one of the
        /// 32 zero-based <see cref="Marker" /> indexes.
        /// </returns>
        public uint MarkerGet()
        {
            int mask = this.scintilla.DirectMessage(
                NativeMethods.SCI_MARKERGET, new IntPtr(this.Index)).ToInt32();

            return unchecked((uint)mask);
        }

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Efficiently searches from the current line forward to the end of
        /// the document for the specified markers.  For example, the mask for
        /// marker index 10 is 1 shifted left 10 times (1 &lt;&lt; 10).
        /// </summary>
        /// <param name="markerMask">
        /// An unsigned 32-bit value with each bit corresponding to one of the
        /// 32 zero-based <see cref="Margin" /> indexes.
        /// </param>
        /// <returns>
        /// If found, the zero-based line index containing one of the markers
        /// in <paramref name="markerMask" />; otherwise, -1.
        /// </returns>
        public long MarkerNext(
            uint markerMask /* in */
            )
        {
            int mask = unchecked((int)markerMask);

            return this.scintilla.DirectMessage(
                NativeMethods.SCI_MARKERNEXT,
                new IntPtr(this.Index), new IntPtr(mask)).ToInt64();
        }

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Efficiently searches from the current line backward to the start
        /// of the document for the specified markers.  For example, the mask
        /// for marker index 10 is 1 shifted left 10 times (1 &lt;&lt; 10).
        /// </summary>
        /// <param name="markerMask">
        /// An unsigned 32-bit value with each bit corresponding to one of the
        /// 32 zero-based <see cref="Margin" /> indexes.
        /// </param>
        /// <returns>
        /// If found, the zero-based line index containing one of the markers
        /// in <paramref name="markerMask" />; otherwise, -1.
        /// </returns>
        public long MarkerPrevious(
            uint markerMask /* in */
            )
        {
            int mask = unchecked((int)markerMask);

            return this.scintilla.DirectMessage(
                NativeMethods.SCI_MARKERPREVIOUS,
                new IntPtr(this.Index), new IntPtr(mask)).ToInt64();
        }

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Toggles the folding state of the line, expanding or contracting
        /// all child lines.  The line must be marked as a
        /// <see cref="ScintillaNET.FoldLevelFlags.Header" />.
        /// </summary>
        public void ToggleFold()
        {
            this.scintilla.DirectMessage(
                NativeMethods.SCI_TOGGLEFOLD, new IntPtr(this.Index));
        }

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Toggles the folding state of the line, expanding or contracting
        /// all child lines, and specifies the text tag to display to the
        /// right of the fold.  The display of fold text tags is determined by
        /// the <see cref="Scintilla.FoldDisplayTextSetStyle" /> method.
        /// </summary>
        /// <param name="text">
        /// The text tag to show to the right of the folded text.
        /// </param>
        public unsafe void ToggleFoldShowText(
            string text /* in */
            )
        {
            if (string.IsNullOrEmpty(text))
            {
                this.scintilla.DirectMessage(
                    NativeMethods.SCI_TOGGLEFOLDSHOWTEXT,
                    new IntPtr(this.Index), IntPtr.Zero);
            }
            else
            {
                byte[] bytes = Helpers.GetBytes(
                    text, this.scintilla.Encoding, true);

                fixed (byte* bp = bytes)
                    this.scintilla.DirectMessage(
                        NativeMethods.SCI_TOGGLEFOLDSHOWTEXT,
                        new IntPtr(this.Index), new IntPtr(bp));
            }
        }
        #endregion
    }
}

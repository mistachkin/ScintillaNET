/*
 * Indicator.cs --
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
    /// Represents an indicator in a <see cref="Scintilla" /> control.
    /// </summary>
    [ObjectId("862977b0-4018-4523-880f-9706993e3b5d")]
    public class Indicator
    {
        #region Public Constants
        /// <summary>
        /// An OR mask to use with <see cref="Scintilla.IndicatorValue" /> and
        /// <see cref="IndicatorFlags.ValueFore" /> to indicate that the
        /// user-defined indicator value should be treated as a RGB color.
        /// </summary>
        public const int ValueBit = NativeMethods.SC_INDICVALUEBIT;

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// An AND mask to use with <see cref="Indicator.ValueAt" /> to
        /// retrieve the user-defined value as a RGB color when being treated
        /// as such.
        /// </summary>
        public const int ValueMask = NativeMethods.SC_INDICVALUEMASK;
        #endregion

        ///////////////////////////////////////////////////////////////////////

        #region Private Data
        /// <summary>
        /// The <see cref="Scintilla" /> control that created this indicator.
        /// </summary>
        private readonly Scintilla scintilla;

        /// <summary>
        /// The zero-based indicator index within the
        /// <see cref="IndicatorCollection" /> that created it.
        /// </summary>
        private readonly int index;
        #endregion

        ///////////////////////////////////////////////////////////////////////

        #region Public Constructors
        /// <summary>
        /// Constructs an instance of this class using the specified
        /// <see cref="Scintilla" /> control and indicator index.
        /// </summary>
        /// <param name="scintilla">
        /// The <see cref="Scintilla" /> control that created this indicator.
        /// </param>
        /// <param name="index">
        /// The index of this indicator within the
        /// <see cref="IndicatorCollection" /> that created it.
        /// </param>
        public Indicator(
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
        /// Gets or sets the alpha transparency of the indicator, ranging from
        /// 0 (completely transparent) to 255 (no transparency).  The default
        /// is 30.
        /// </summary>
        public int Alpha
        {
            get
            {
                return this.scintilla.DirectMessage(
                    NativeMethods.SCI_INDICGETALPHA,
                    new IntPtr(this.index)).ToInt32();
            }
            set
            {
                value = Helpers.Clamp(value, 0, 255);

                this.scintilla.DirectMessage(
                    NativeMethods.SCI_INDICSETALPHA,
                    new IntPtr(this.index), new IntPtr(value));
            }
        }

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Gets or sets the indicator flags, a bitwise combination of the
        /// <see cref="IndicatorFlags" /> enumeration.  The default is
        /// <see cref="IndicatorFlags.None" />.
        /// </summary>
        public IndicatorFlags Flags
        {
            get
            {
                return (IndicatorFlags)this.scintilla.DirectMessage(
                    NativeMethods.SCI_INDICGETFLAGS,
                    new IntPtr(this.index));
            }
            set
            {
                int flags = (int)value;

                this.scintilla.DirectMessage(
                    NativeMethods.SCI_INDICSETFLAGS,
                    new IntPtr(this.index), new IntPtr(flags));
            }
        }

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Gets or sets the color used to draw an indicator.  The default
        /// varies.
        /// </summary>
        /// <remarks>
        /// Changing the <see cref="ForeColor" /> property will reset the
        /// <see cref="HoverForeColor" />.
        /// </remarks>
        public Color ForeColor
        {
            get
            {
                int color = this.scintilla.DirectMessage(
                    NativeMethods.SCI_INDICGETFORE,
                    new IntPtr(this.index)).ToInt32();

                return ColorTranslator.FromWin32(color);
            }
            set
            {
                int color = ColorTranslator.ToWin32(value);

                this.scintilla.DirectMessage(
                    NativeMethods.SCI_INDICSETFORE,
                    new IntPtr(this.index), new IntPtr(color));
            }
        }

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Gets or sets the color used to draw an indicator when the mouse or
        /// caret is over an indicator.  By default, the hover style is equal
        /// to the regular <see cref="ForeColor" />.
        /// </summary>
        /// <remarks>
        /// Changing the <see cref="ForeColor" /> property will reset the
        /// <see cref="HoverForeColor" />.
        /// </remarks>
        public Color HoverForeColor
        {
            get
            {
                int color = this.scintilla.DirectMessage(
                    NativeMethods.SCI_INDICGETHOVERFORE,
                    new IntPtr(this.index)).ToInt32();

                return ColorTranslator.FromWin32(color);
            }
            set
            {
                int color = ColorTranslator.ToWin32(value);

                this.scintilla.DirectMessage(
                    NativeMethods.SCI_INDICSETHOVERFORE,
                    new IntPtr(this.index), new IntPtr(color));
            }
        }

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Gets or sets the indicator style used when the mouse or caret is
        /// over an indicator, one of the <see cref="IndicatorStyle" />
        /// enumeration values.  By default, the hover style is equal to the
        /// regular <see cref="Style" />.
        /// </summary>
        /// <remarks>
        /// Changing the <see cref="Style" /> property will reset the
        /// <see cref="HoverStyle" />.
        /// </remarks>
        public IndicatorStyle HoverStyle
        {
            get
            {
                return (IndicatorStyle)this.scintilla.DirectMessage(
                    NativeMethods.SCI_INDICGETHOVERSTYLE,
                    new IntPtr(this.index));
            }
            set
            {
                int style = (int)value;

                this.scintilla.DirectMessage(
                    NativeMethods.SCI_INDICSETHOVERSTYLE,
                    new IntPtr(this.index), new IntPtr(style));
            }
        }

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Gets the zero-based indicator index this object represents within
        /// the <see cref="IndicatorCollection" />.
        /// </summary>
        public int Index
        {
            get { return this.index; }
        }

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Gets or sets the alpha transparency of the indicator outline,
        /// ranging from 0 (completely transparent) to 255 (no transparency).
        /// The default is 50.
        /// </summary>
        public int OutlineAlpha
        {
            get
            {
                return this.scintilla.DirectMessage(
                    NativeMethods.SCI_INDICGETOUTLINEALPHA,
                    new IntPtr(this.index)).ToInt32();
            }
            set
            {
                value = Helpers.Clamp(value, 0, 255);

                this.scintilla.DirectMessage(
                    NativeMethods.SCI_INDICSETOUTLINEALPHA,
                    new IntPtr(this.index), new IntPtr(value));
            }
        }

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Gets or sets the indicator style, one of the
        /// <see cref="IndicatorStyle" /> enumeration values.  The default
        /// varies.
        /// </summary>
        /// <remarks>
        /// Changing the <see cref="Style" /> property will reset the
        /// <see cref="HoverStyle" />.
        /// </remarks>
        public IndicatorStyle Style
        {
            get
            {
                return (IndicatorStyle)this.scintilla.DirectMessage(
                    NativeMethods.SCI_INDICGETSTYLE,
                    new IntPtr(this.index));
            }
            set
            {
                int style = (int)value;

                this.scintilla.DirectMessage(
                    NativeMethods.SCI_INDICSETSTYLE,
                    new IntPtr(this.index), new IntPtr(style));
            }
        }

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Gets or sets whether indicators are drawn under or over text.  The
        /// default is false.
        /// </summary>
        /// <remarks>
        /// Drawing indicators under text requires <see cref="Phases.One" /> or
        /// <see cref="Phases.Multiple" /> drawing.
        /// </remarks>
        public bool Under
        {
            get
            {
                return (this.scintilla.DirectMessage(
                    NativeMethods.SCI_INDICGETUNDER,
                    new IntPtr(this.index)) != IntPtr.Zero);
            }
            set
            {
                IntPtr under = (value ? new IntPtr(1) : IntPtr.Zero);

                this.scintilla.DirectMessage(
                    NativeMethods.SCI_INDICSETUNDER,
                    new IntPtr(this.index), under);
            }
        }
        #endregion

        ///////////////////////////////////////////////////////////////////////

        #region Public Methods
        /// <summary>
        /// Given a document position which is filled with this indicator, will
        /// return the document position where the use of this indicator ends.
        /// </summary>
        /// <param name="position">
        /// A zero-based document position using this indicator.
        /// </param>
        /// <returns>
        /// The zero-based document position where the use of this indicator
        /// ends.
        /// </returns>
        /// <remarks>
        /// Specifying a <paramref name="position" /> which is not filled with
        /// this indicator will cause this method to return the end position of
        /// the range where this indicator is not in use (the negative space).
        /// If this indicator is not in use anywhere within the document the
        /// return value will be 0.
        /// </remarks>
        public long End(
            long position /* in */
            )
        {
            position = Helpers.Clamp(position, 0, this.scintilla.TextLength);
            position = this.scintilla.Lines.CharToBytePosition(position);

            position = this.scintilla.DirectMessage(
                NativeMethods.SCI_INDICATOREND,
                new IntPtr(this.index), new IntPtr(position)).ToInt64();

            return this.scintilla.Lines.ByteToCharPosition(position);
        }

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Given a document position which is filled with this indicator, will
        /// return the document position where the use of this indicator
        /// starts.
        /// </summary>
        /// <param name="position">
        /// A zero-based document position using this indicator.
        /// </param>
        /// <returns>
        /// The zero-based document position where the use of this indicator
        /// starts.
        /// </returns>
        /// <remarks>
        /// Specifying a <paramref name="position" /> which is not filled with
        /// this indicator will cause this method to return the start position
        /// of the range where this indicator is not in use (the negative
        /// space).  If this indicator is not in use anywhere within the
        /// document the return value will be 0.
        /// </remarks>
        public long Start(
            long position /* in */
            )
        {
            position = Helpers.Clamp(position, 0, this.scintilla.TextLength);
            position = this.scintilla.Lines.CharToBytePosition(position);

            position = this.scintilla.DirectMessage(
                NativeMethods.SCI_INDICATORSTART,
                new IntPtr(this.index), new IntPtr(position)).ToInt64();

            return this.scintilla.Lines.ByteToCharPosition(position);
        }

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Returns the user-defined value for the indicator at the specified
        /// position.
        /// </summary>
        /// <param name="position">
        /// The zero-based document position to get the indicator value for.
        /// </param>
        /// <returns>
        /// The user-defined value at the specified
        /// <paramref name="position" />.
        /// </returns>
        public int ValueAt(
            long position /* in */
            )
        {
            position = Helpers.Clamp(position, 0, this.scintilla.TextLength);
            position = this.scintilla.Lines.CharToBytePosition(position);

            return this.scintilla.DirectMessage(
                NativeMethods.SCI_INDICATORVALUEAT,
                new IntPtr(this.index), new IntPtr(position)).ToInt32();
        }
        #endregion
    }
}

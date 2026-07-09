/*
 * MarginType.cs --
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

namespace ScintillaNET
{
    /// <summary>
    /// The behavior and appearance of a margin.
    /// </summary>
    [ObjectId("d815cde2-87fd-4d5a-90b7-fa272f698abc")]
    public enum MarginType
    {
        /// <summary>
        /// Margin can display symbols.
        /// </summary>
        Symbol = NativeMethods.SC_MARGIN_SYMBOL,

        /// <summary>
        /// Margin displays line numbers.
        /// </summary>
        Number = NativeMethods.SC_MARGIN_NUMBER,

        /// <summary>
        /// Margin can display symbols and has a background color equivalent to <see cref="Style.Default" /> background color.
        /// </summary>
        BackColor = NativeMethods.SC_MARGIN_BACK,

        /// <summary>
        /// Margin can display symbols and has a background color equivalent to <see cref="Style.Default"/> foreground color.
        /// </summary>
        ForeColor = NativeMethods.SC_MARGIN_FORE,

        /// <summary>
        /// Margin can display application defined text.
        /// </summary>
        Text = NativeMethods.SC_MARGIN_TEXT,

        /// <summary>
        /// Margin can display application defined text right-justified.
        /// </summary>
        RightText = NativeMethods.SC_MARGIN_RTEXT,

        /// <summary>
        /// Margin can display symbols and has a background color specified using the <see cref="Margin.BackColor" /> property.
        /// </summary>
        Color = NativeMethods.SC_MARGIN_COLOUR
    }
}

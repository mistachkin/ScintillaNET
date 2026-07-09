/*
 * WhitespaceMode.cs --
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
    /// Specifies the display mode of whitespace characters.
    /// </summary>
    [ObjectId("44722689-09e2-40d3-878f-1d6988db5fd0")]
    public enum WhitespaceMode
    {
        /// <summary>
        /// The normal display mode with whitespace displayed as an empty background color.
        /// </summary>
        Invisible = NativeMethods.SCWS_INVISIBLE,

        /// <summary>
        /// Whitespace characters are drawn as dots and arrows.
        /// </summary>
        VisibleAlways = NativeMethods.SCWS_VISIBLEALWAYS,

        /// <summary>
        /// Whitespace used for indentation is displayed normally but after the first visible character,
        /// it is shown as dots and arrows.
        /// </summary>
        VisibleAfterIndent = NativeMethods.SCWS_VISIBLEAFTERINDENT,

        /// <summary>
        /// Whitespace used for indentation is displayed as dots and arrows.
        /// </summary>
        VisibleOnlyIndent = NativeMethods.SCWS_VISIBLEONLYININDENT
    }
}

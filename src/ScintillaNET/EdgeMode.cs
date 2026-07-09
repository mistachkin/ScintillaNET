/*
 * EdgeMode.cs --
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
    /// The long line edge display mode.
    /// </summary>
    [ObjectId("5af5c033-3b83-4bcc-94d5-0a85f7ac6965")]
    public enum EdgeMode
    {
        /// <summary>
        /// Long lines are not indicated. This is the default.
        /// </summary>
        None = NativeMethods.EDGE_NONE,

        /// <summary>
        /// Long lines are indicated with a vertical line.
        /// </summary>
        Line = NativeMethods.EDGE_LINE,

        /// <summary>
        /// Long lines are indicated with a background color.
        /// </summary>
        Background = NativeMethods.EDGE_BACKGROUND,

        /// <summary>
        /// Similar to <see cref="Line" /> except allows for multiple vertical lines to be visible using the <see cref="Scintilla.MultiEdgeAddLine" /> method.
        /// </summary>
        /// <remarks><see cref="Line" /> and <see cref="Scintilla.EdgeColumn" /> are completely independant of this mode.</remarks>
        MultiLine = NativeMethods.EDGE_MULTILINE
    }
}

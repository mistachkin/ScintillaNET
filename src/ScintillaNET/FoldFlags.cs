/*
 * FoldFlags.cs --
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
    /// Additional display options for folds.
    /// </summary>
    [Flags]
    [ObjectId("6d03f52c-022a-4ffe-9648-9f956af7485f")]
    public enum FoldFlags
    {
        /// <summary>
        /// A line is drawn above if expanded.
        /// </summary>
        LineBeforeExpanded = NativeMethods.SC_FOLDFLAG_LINEBEFORE_EXPANDED,

        /// <summary>
        /// A line is drawn above if not expanded.
        /// </summary>
        LineBeforeContracted = NativeMethods.SC_FOLDFLAG_LINEBEFORE_CONTRACTED,

        /// <summary>
        /// A line is drawn below if expanded.
        /// </summary>
        LineAfterExpanded = NativeMethods.SC_FOLDFLAG_LINEAFTER_EXPANDED,

        /// <summary>
        /// A line is drawn below if not expanded.
        /// </summary>
        LineAfterContracted = NativeMethods.SC_FOLDFLAG_LINEAFTER_CONTRACTED,

        /// <summary>
        /// Displays the hexadecimal fold levels in the margin to aid with debugging.
        /// This feature may change in the future.
        /// </summary>
        LevelNumbers = NativeMethods.SC_FOLDFLAG_LEVELNUMBERS,

        /// <summary>
        /// Displays the hexadecimal line state in the margin to aid with debugging. This flag
        /// cannot be used at the same time as the <see cref="LevelNumbers" /> flag.
        /// </summary>
        LineState = NativeMethods.SC_FOLDFLAG_LINESTATE
    }
}

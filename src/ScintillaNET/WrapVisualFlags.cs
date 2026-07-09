/*
 * WrapVisualFlags.cs --
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
    /// The visual indicator used on a wrapped line.
    /// </summary>
    [Flags]
    [ObjectId("18da9043-29da-430f-8a84-b51302e9be88")]
    public enum WrapVisualFlags
    {
        /// <summary>
        /// No visual indicator is displayed. This the default.
        /// </summary>
        None = NativeMethods.SC_WRAPVISUALFLAG_NONE,

        /// <summary>
        /// A visual indicator is displayed at th end of a wrapped subline.
        /// </summary>
        End = NativeMethods.SC_WRAPVISUALFLAG_END,

        /// <summary>
        /// A visual indicator is displayed at the beginning of a subline.
        /// The subline is indented by 1 pixel to make room for the display.
        /// </summary>
        Start = NativeMethods.SC_WRAPVISUALFLAG_START,

        /// <summary>
        /// A visual indicator is displayed in the number margin.
        /// </summary>
        Margin = NativeMethods.SC_WRAPVISUALFLAG_MARGIN
    }
}

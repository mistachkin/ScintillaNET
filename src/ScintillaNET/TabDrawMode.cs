/*
 * TabDrawMode.cs --
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
    /// Specifies how tab characters are drawn when whitespace is visible.
    /// </summary>
    [ObjectId("34001327-b2c8-4d62-bef4-791aa7fab89e")]
    public enum TabDrawMode
    {
        /// <summary>
        /// The default mode of an arrow stretching until the tabstop.
        /// </summary>
        LongArrow = NativeMethods.SCTD_LONGARROW,

        /// <summary>
        /// A horizontal line stretching until the tabstop.
        /// </summary>
        Strikeout = NativeMethods.SCTD_STRIKEOUT
    }
}

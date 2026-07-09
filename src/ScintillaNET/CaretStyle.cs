/*
 * CaretStyle.cs --
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
    /// The caret visual style.
    /// </summary>
    [ObjectId("16171b4e-d55c-4b91-b624-f2a120780ccb")]
    public enum CaretStyle
    {
        /// <summary>
        /// The caret is not displayed.
        /// </summary>
        Invisible = NativeMethods.CARETSTYLE_INVISIBLE,

        /// <summary>
        /// The caret is drawn as a vertical line.
        /// </summary>
        Line = NativeMethods.CARETSTYLE_LINE,

        /// <summary>
        /// The caret is drawn as a block.
        /// </summary>
        Block = NativeMethods.CARETSTYLE_BLOCK
    }
}

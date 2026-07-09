/*
 * MultiPaste.cs --
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
    /// Specifies the behavior of pasting into multiple selections.
    /// </summary>
    [ObjectId("525e0f61-fed7-4fbf-b718-f5eebc43dc2d")]
    public enum MultiPaste
    {
        /// <summary>
        /// Pasting into multiple selections only pastes to the main selection. This is the default.
        /// </summary>
        Once = NativeMethods.SC_MULTIPASTE_ONCE,

        /// <summary>
        /// Pasting into multiple selections pastes into each selection.
        /// </summary>
        Each = NativeMethods.SC_MULTIPASTE_EACH
    }
}

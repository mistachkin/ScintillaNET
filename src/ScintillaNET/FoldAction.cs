/*
 * FoldAction.cs --
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
    /// Fold actions.
    /// </summary>
    [ObjectId("1a00a860-a7ad-4312-b055-1daedc09ac1d")]
    public enum FoldAction
    {
        /// <summary>
        /// Contract the fold.
        /// </summary>
        Contract = NativeMethods.SC_FOLDACTION_CONTRACT,

        /// <summary>
        /// Expand the fold.
        /// </summary>
        Expand = NativeMethods.SC_FOLDACTION_EXPAND,

        /// <summary>
        /// Toggle between contracted and expanded.
        /// </summary>
        Toggle = NativeMethods.SC_FOLDACTION_TOGGLE
    }
}

/*
 * FoldDisplayText.cs --
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
    /// Display options for fold text tags.
    /// </summary>
    [ObjectId("145502b2-40c2-40ac-9995-9311f07b1acd")]
    public enum FoldDisplayText
    {
        /// <summary>
        /// Do not display the text tags. This is the default.
        /// </summary>
        Hidden = NativeMethods.SC_FOLDDISPLAYTEXT_HIDDEN,

        /// <summary>
        /// Display the text tags.
        /// </summary>
        Standard = NativeMethods.SC_FOLDDISPLAYTEXT_STANDARD,

        /// <summary>
        /// Display the text tags with a box drawn around them.
        /// </summary>
        Boxed = NativeMethods.SC_FOLDDISPLAYTEXT_BOXED
    }
}

/*
 * MarginCursor.cs --
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
    /// The display of a cursor when over a margin.
    /// </summary>
    [ObjectId("d9d65920-d31e-4187-ad5b-77955880a11f")]
    public enum MarginCursor
    {
        /// <summary>
        /// A normal arrow.
        /// </summary>
        Arrow = NativeMethods.SC_CURSORARROW,

        /// <summary>
        /// A reversed arrow.
        /// </summary>
        ReverseArrow = NativeMethods.SC_CURSORREVERSEARROW
    }
}

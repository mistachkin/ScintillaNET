/*
 * Eol.cs --
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
    /// End-of-line format.
    /// </summary>
    [ObjectId("95f438aa-f55d-4923-a83a-cf229795f1f3")]
    public enum Eol
    {
        /// <summary>
        /// Carriage Return, Line Feed pair "\r\n" (0x0D0A).
        /// </summary>
        CrLf = NativeMethods.SC_EOL_CRLF,

        /// <summary>
        /// Carriage Return '\r' (0x0D).
        /// </summary>
        Cr = NativeMethods.SC_EOL_CR,

        /// <summary>
        /// Line Feed '\n' (0x0A).
        /// </summary>
        Lf = NativeMethods.SC_EOL_LF
    }
}

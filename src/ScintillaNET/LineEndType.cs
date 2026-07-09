/*
 * LineEndType.cs --
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
    /// Line endings types supported by lexers and allowed by a <see cref="Scintilla" /> control.
    /// </summary>
    /// <seealso cref="Scintilla.LineEndTypesSupported" />
    /// <seealso cref="Scintilla.LineEndTypesAllowed" />
    /// <seealso cref="Scintilla.LineEndTypesActive" />
    [Flags]
    [ObjectId("37db90f8-04e1-412f-a8fc-e12395f8bb63")]
    public enum LineEndType
    {
        /// <summary>
        /// ASCII line endings. Carriage Return, Line Feed pair "\r\n" (0x0D0A); Carriage Return '\r' (0x0D); Line Feed '\n' (0x0A).
        /// </summary>
        Default = NativeMethods.SC_LINE_END_TYPE_DEFAULT,

        /// <summary>
        /// Unicode line endings. Next Line (0x0085); Line Separator (0x2028); Paragraph Separator (0x2029).
        /// </summary>
        Unicode = NativeMethods.SC_LINE_END_TYPE_UNICODE
    }
}

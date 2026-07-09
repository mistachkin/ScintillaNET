/*
 * WrapIndentMode.cs --
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
    /// Indenting behavior of wrapped sublines.
    /// </summary>
    [ObjectId("f672ed36-4d1f-48d6-a4c7-e6bd18fca7fd")]
    public enum WrapIndentMode
    {
        /// <summary>
        /// Wrapped sublines aligned to left of window plus the amount set by <see cref="ScintillaNET.Scintilla.WrapStartIndent" />.
        /// This is the default.
        /// </summary>
        Fixed,

        /// <summary>
        /// Wrapped sublines are aligned to first subline indent.
        /// </summary>
        Same,

        /// <summary>
        /// Wrapped sublines are aligned to first subline indent plus one more level of indentation.
        /// </summary>
        Indent = NativeMethods.SC_WRAPINDENT_INDENT
    }
}

/*
 * CopyFormat.cs --
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
    /// Specifies the clipboard formats to copy.
    /// </summary>
    [Flags]
    [ObjectId("bc9be78f-dd9a-4f26-8ea0-7971b5175354")]
    public enum CopyFormat
    {
        /// <summary>
        /// Copies text to the clipboard in Unicode format.
        /// </summary>
        Text = 1 << 0,

        /// <summary>
        /// Copies text to the clipboard in Rich Text Format (RTF).
        /// </summary>
        Rtf = 1 << 1,

        /// <summary>
        /// Copies text to the clipboard in HyperText Markup Language (HTML) format.
        /// </summary>
        Html = 1 << 2
    }
}

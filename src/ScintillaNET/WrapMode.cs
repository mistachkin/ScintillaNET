/*
 * WrapMode.cs --
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
    /// The line wrapping strategy.
    /// </summary>
    [ObjectId("01142c09-0264-43ff-be0d-d4978b525bd4")]
    public enum WrapMode
    {
        /// <summary>
        /// Line wrapping is disabled. This is the default.
        /// </summary>
        None = NativeMethods.SC_WRAP_NONE,

        /// <summary>
        /// Lines are wrapped on word or style boundaries.
        /// </summary>
        Word = NativeMethods.SC_WRAP_WORD,

        /// <summary>
        /// Lines are wrapped between any character.
        /// </summary>
        Char = NativeMethods.SC_WRAP_CHAR,

        /// <summary>
        /// Lines are wrapped on whitespace.
        /// </summary>
        Whitespace = NativeMethods.SC_WRAP_WHITESPACE
    }
}

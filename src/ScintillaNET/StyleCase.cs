/*
 * StyleCase.cs --
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
    /// The possible casing styles of a style.
    /// </summary>
    [ObjectId("d74f5cf8-3da5-46be-944e-d036d715a89e")]
    public enum StyleCase
    {
        /// <summary>
        /// Display the text normally.
        /// </summary>
        Mixed = NativeMethods.SC_CASE_MIXED,

        /// <summary>
        /// Display the text in upper case.
        /// </summary>
        Upper = NativeMethods.SC_CASE_UPPER,

        /// <summary>
        /// Display the text in lower case.
        /// </summary>
        Lower = NativeMethods.SC_CASE_LOWER,

        /// <summary>
        /// Display the text in camel case.
        /// </summary>
        Camel = NativeMethods.SC_CASE_CAMEL
    }
}

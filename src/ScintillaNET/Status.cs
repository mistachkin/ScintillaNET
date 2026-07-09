/*
 * Status.cs --
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
    /// Possible status codes returned by the <see cref="Scintilla.Status" /> property.
    /// </summary>
    [ObjectId("01a19a53-8f4a-4e02-9d43-d70a84ad972d")]
    public enum Status
    {
        /// <summary>
        /// No failures.
        /// </summary>
        Ok = NativeMethods.SC_STATUS_OK,

        /// <summary>
        /// Generic failure.
        /// </summary>
        Failure = NativeMethods.SC_STATUS_FAILURE,

        /// <summary>
        /// Memory is exhausted.
        /// </summary>
        BadAlloc = NativeMethods.SC_STATUS_BADALLOC,

        /// <summary>
        /// Regular expression is invalid.
        /// </summary>
        WarnRegex = NativeMethods.SC_STATUS_WARN_REGEX
    }
}

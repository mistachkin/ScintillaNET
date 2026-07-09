/*
 * ModificationSource.cs --
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
    /// The source of a modification
    /// </summary>
    [ObjectId("f0b58fb9-4f6c-4918-929f-5c8bdf7c1055")]
    public enum ModificationSource
    {
        /// <summary>
        /// Modification is the result of a user operation.
        /// </summary>
        User = NativeMethods.SC_PERFORMED_USER,

        /// <summary>
        /// Modification is the result of an undo operation.
        /// </summary>
        Undo = NativeMethods.SC_PERFORMED_UNDO,

        /// <summary>
        /// Modification is the result of a redo operation.
        /// </summary>
        Redo = NativeMethods.SC_PERFORMED_REDO
    }
}

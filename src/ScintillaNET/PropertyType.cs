/*
 * PropertyType.cs --
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
    /// Lexer property types.
    /// </summary>
    [ObjectId("35ec7017-0f6e-416b-907a-d80d2105f166")]
    public enum PropertyType
    {
        /// <summary>
        /// A Boolean property. This is the default.
        /// </summary>
        Boolean = NativeMethods.SC_TYPE_BOOLEAN,

        /// <summary>
        /// An integer property.
        /// </summary>
        Integer = NativeMethods.SC_TYPE_INTEGER,

        /// <summary>
        /// A string property.
        /// </summary>
        String = NativeMethods.SC_TYPE_STRING
    }
}

/*
 * Technology.cs --
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
    /// The rendering technology used in a <see cref="Scintilla" /> control.
    /// </summary>
    [ObjectId("83c24fad-d425-4988-85e9-6a2b2799a144")]
    public enum Technology
    {
        /// <summary>
        /// Renders text using GDI. This is the default.
        /// </summary>
        Default = NativeMethods.SC_TECHNOLOGY_DEFAULT,

        /// <summary>
        /// Renders text using Direct2D/DirectWrite. Since Direct2D buffers drawing,
        /// Scintilla's buffering can be turned off with <see cref="Scintilla.BufferedDraw" />.
        /// </summary>
        DirectWrite = NativeMethods.SC_TECHNOLOGY_DIRECTWRITE
    }
}

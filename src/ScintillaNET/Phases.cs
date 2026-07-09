/*
 * Phases.cs --
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
    /// The number of phases used when drawing.
    /// </summary>
    [ObjectId("d9c1f6f9-cada-408d-ad56-2fd6f8c97d54")]
    public enum Phases
    {
        /// <summary>
        /// Drawing is done in a single phase. This is the fastest but provides no support for kerning.
        /// </summary>
        One = NativeMethods.SC_PHASES_ONE,

        /// <summary>
        /// Drawing is done in two phases; the background first and then the text. This is the default.
        /// </summary>
        Two = NativeMethods.SC_PHASES_TWO,

        /// <summary>
        /// Drawing is done in multiple phases; once for each feature. This is the slowest but allows
        /// extreme ascenders and descenders to overflow into adjacent lines.
        /// </summary>
        Multiple = NativeMethods.SC_PHASES_MULTIPLE
    }
}

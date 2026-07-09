/*
 * IdleStyling.cs --
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
    /// Possible strategies for styling text using application idle time.
    /// </summary>
    /// <seealso cref="Scintilla.IdleStyling" />
    [ObjectId("5df23924-17a9-43a8-ac8a-7257d951d06e")]
    public enum IdleStyling
    {
        /// <summary>
        /// Syntax styling is performed for all the currently visible text before displaying it.
        /// This is the default.
        /// </summary>
        None = NativeMethods.SC_IDLESTYLING_NONE,

        /// <summary>
        /// A small amount of styling is performed before display and then further styling is performed incrementally in the background as an idle-time task.
        /// This can improve initial display/scroll performance, but may result in the text initially appearing uncolored and then, some time later, it is colored.
        /// </summary>
        ToVisible = NativeMethods.SC_IDLESTYLING_TOVISIBLE,

        /// <summary>
        /// Text after the currently visible portion may be styled as an idle-time task.
        /// This will not improve initial display/scroll performance, but may improve subsequent display/scroll performance.
        /// </summary>
        AfterVisible = NativeMethods.SC_IDLESTYLING_AFTERVISIBLE,

        /// <summary>
        /// Text before and after the current visible text.
        /// This is a combination of <see cref="ToVisible" /> and <see cref="AfterVisible" />.
        /// </summary>
        All = NativeMethods.SC_IDLESTYLING_ALL
    }
}

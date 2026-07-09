/*
 * PopupMode.cs --
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
    /// Behavior of the standard edit control context menu.
    /// </summary>
    /// <seealso cref="Scintilla.UsePopup(PopupMode)" />
    [ObjectId("b2de2173-d1b4-46af-bccd-d9c22d0a3f66")]
    public enum PopupMode
    {
        /// <summary>
        /// Never show the default editing menu.
        /// </summary>
        Never = NativeMethods.SC_POPUP_NEVER,

        /// <summary>
        /// Show default editing menu if clicking on the control.
        /// </summary>
        All = NativeMethods.SC_POPUP_ALL,

        /// <summary>
        /// Show default editing menu only if clicking on text area.
        /// </summary>
        /// <remarks>To receive the <see cref="Scintilla.MarginRightClick" /> event, this value must be used.</remarks>
        /// <seealso cref="Scintilla.MarginRightClick" />
        Text = NativeMethods.SC_POPUP_TEXT
    }
}

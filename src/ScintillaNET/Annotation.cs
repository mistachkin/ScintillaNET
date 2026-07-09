/*
 * Annotation.cs --
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
    /// Visibility and location of annotations in a <see cref="Scintilla" /> control
    /// </summary>
    [ObjectId("3d671aae-38ee-4f76-97ac-05bf674ad623")]
    public enum Annotation
    {
        /// <summary>
        /// Annotations are not displayed. This is the default.
        /// </summary>
        Hidden = NativeMethods.ANNOTATION_HIDDEN,

        /// <summary>
        /// Annotations are drawn left justified with no adornment.
        /// </summary>
        Standard = NativeMethods.ANNOTATION_STANDARD,

        /// <summary>
        /// Annotations are indented to match the text and are surrounded by a box.
        /// </summary>
        Boxed = NativeMethods.ANNOTATION_BOXED,

        /// <summary>
        /// Annotations are indented to match the text.
        /// </summary>
        Indented = NativeMethods.ANNOTATION_INDENTED
    }
}

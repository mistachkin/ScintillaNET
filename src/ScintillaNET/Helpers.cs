/*
 * Helpers.cs --
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

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace ScintillaNET
{
    /// <summary>
    /// Provides internal static helper routines used throughout the
    /// ScintillaNET control, including clipboard copy and export (HTML and
    /// RTF), marshaling between managed strings and native encoded text
    /// buffers, per-byte and per-character style conversion, and simple
    /// value clamping utilities.
    /// </summary>
    [ObjectId("9abc7259-a5e9-4f43-b560-c5579f336cb0")]
    internal static class Helpers
    {
        #region Fields
        /// <summary>
        /// Indicates whether the custom clipboard formats used for styled
        /// copy operations have already been registered with the system.
        /// </summary>
        private static bool registeredFormats;

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// The registered clipboard format identifier for HTML content.
        /// </summary>
        private static uint CF_HTML;

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// The registered clipboard format identifier for Rich Text
        /// Format (RTF) content.
        /// </summary>
        private static uint CF_RTF;

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// The registered clipboard format identifier that marks a
        /// whole-line selection (the "MSDEVLineSelect" format).
        /// </summary>
        private static uint CF_LINESELECT;

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// The registered clipboard format identifier used by Visual
        /// Studio to tag a line cut or copy operation.
        /// </summary>
        private static uint CF_VSLINETAG;
        #endregion Fields

        ///////////////////////////////////////////////////////////////////////

        #region Methods
        /// <summary>
        /// Copies all remaining bytes from one stream to another using a
        /// fixed-size intermediate buffer.
        /// </summary>
        /// <param name="source">
        /// The stream to read from.
        /// </param>
        /// <param name="destination">
        /// The stream to write the copied bytes to.
        /// </param>
        /// <returns>
        /// The total number of bytes copied.
        /// </returns>
        public static long CopyTo(Stream source, Stream destination)
        {
          byte[] buffer = new byte[2048];
          int bytesRead;
          long totalBytes = 0;
          while ((bytesRead = source.Read(buffer, 0, buffer.Length)) > 0)
          {
            destination.Write(buffer, 0, bytesRead);
            totalBytes += bytesRead;
          }
          return totalBytes;
        }

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Computes the total number of bytes contained in a list of array
        /// segments.
        /// </summary>
        /// <param name="segments">
        /// The list of array segments to total.
        /// </param>
        /// <returns>
        /// The combined number of bytes across every segment.
        /// </returns>
        private static int TotalCount(List<ArraySegment<byte>> segments)
        {
            int total = 0;
            foreach (ArraySegment<byte> segment in segments)
                total += segment.Count;
            return total;
        }

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Converts a bitmap image into a flat byte array containing the
        /// red, green, blue, and alpha components of every pixel, in that
        /// order, four bytes per pixel.
        /// </summary>
        /// <param name="image">
        /// The bitmap to convert.
        /// </param>
        /// <returns>
        /// A newly allocated byte array holding the RGBA components of each
        /// pixel, row by row.
        /// </returns>
        public static unsafe byte[] BitmapToArgb(Bitmap image)
        {
            // This code originally used Image.LockBits and some fast byte
            // copying, however, the endianness of the image formats was making
            // my brain hurt. For now I'm going to use the slow but simple
            // GetPixel approach.

            byte[] bytes = new byte[4 * image.Width * image.Height];

            int i = 0;
            for (int y = 0; y < image.Height; y++)
            {
                for (int x = 0; x < image.Width; x++)
                {
                    Color color = image.GetPixel(x, y);
                    bytes[i++] = color.R;
                    bytes[i++] = color.G;
                    bytes[i++] = color.B;
                    bytes[i++] = color.A;
                }
            }

            return bytes;
        }

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Converts an array of per-byte style values into an array of
        /// per-character (UTF-16 code unit) style values for the given
        /// encoded text. Each source byte is decoded once using a single
        /// stateful decoder so the character count matches the whole-buffer
        /// count; every code unit a byte produces inherits that byte's
        /// style, and a trailing incomplete sequence is styled from the
        /// final byte.
        /// </summary>
        /// <param name="styles">
        /// A pointer to the native array of per-byte style values, one
        /// entry for each byte of <paramref name="text" />.
        /// </param>
        /// <param name="text">
        /// A pointer to the native encoded text buffer being restyled.
        /// </param>
        /// <param name="length">
        /// The number of bytes in <paramref name="text" /> and in
        /// <paramref name="styles" />.
        /// </param>
        /// <param name="encoding">
        /// The encoding used to decode <paramref name="text" /> into
        /// characters.
        /// </param>
        /// <returns>
        /// A byte array of per-character style values, one entry for each
        /// decoded UTF-16 code unit.
        /// </returns>
        public static unsafe byte[] ByteToCharStyles(
            byte* styles,     /* in */
            byte* text,       /* in */
            int length,       /* in */
            Encoding encoding /* in */
            )
        {
            // This is used by annotations and margins to get all the styles in
            // one call. It converts an array of styles where each element
            // corresponds to a BYTE to an array of styles where each element
            // corresponds to a CHARACTER (UTF-16 unit).
            //
            // Walk the text with a single STATEFUL decoder (Decoder.Convert),
            // which is consistent with the whole-buffer GetCharCount used to
            // size "result" and is cross-runtime safe -- unlike per-byte
            // Decoder.GetCharCount, which does not persist multi-byte state on
            // .NET Core / Mono and can over-/under-count (an
            // IndexOutOfRangeException / style desync reachable from
            // AnnotationStyles / MarginStyles on untrusted or multibyte text).
            // Each source byte is fed once; the units it completes (0 for a
            // continuation byte, 1 for a BMP char, 2 for a surrogate pair) all
            // take that byte's style. Decoder.Convert has no pointer overload
            // on the older target frameworks, hence the copy to a managed
            // byte[].

            byte[] result = new byte[encoding.GetCharCount(text, length)];
            if (result.Length == 0)
                return result;

            byte[] bytes = new byte[length];
            Marshal.Copy((IntPtr)text, bytes, 0, length);

            Decoder decoder = encoding.GetDecoder();
            char[] scratch = new char[2];
            int charPos = 0;
            int bytesUsed, units;
            bool completed;

            for (int bytePos = 0;
                bytePos < length && charPos < result.Length; bytePos++)
            {
                decoder.Convert(
                    bytes, bytePos, 1, scratch, 0, 2, false, out bytesUsed,
                    out units, out completed);
                byte style = *(styles + bytePos);
                for (int i = 0; i < units && charPos < result.Length; i++)
                    result[charPos++] = style;
            }

            // Flush a trailing incomplete sequence (it decodes to a
            // replacement char that the whole-buffer GetCharCount already
            // counted); style it from the last byte.
            if (charPos < result.Length)
            {
                decoder.Convert(
                    bytes, length, 0, scratch, 0, 2, true, out bytesUsed,
                    out units, out completed);
                byte style = length > 0 ? *(styles + (length - 1)) : (byte)0;
                for (int i = 0; i < units && charPos < result.Length; i++)
                    result[charPos++] = style;
            }

            return result;
        }

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Converts an array of per-character (UTF-16 code unit) style
        /// values into an array of per-byte style values for the given
        /// encoded text. Uses the same stateful decoder as ByteToCharStyles,
        /// for the same consistency and cross-runtime reasons; every byte of
        /// a character inherits the style of that character's first code
        /// unit.
        /// </summary>
        /// <param name="styles">
        /// The array of per-character style values to distribute across the
        /// bytes.
        /// </param>
        /// <param name="text">
        /// A pointer to the native encoded text buffer being restyled.
        /// </param>
        /// <param name="length">
        /// The number of bytes in <paramref name="text" />.
        /// </param>
        /// <param name="encoding">
        /// The encoding used to decode <paramref name="text" /> into
        /// characters.
        /// </param>
        /// <returns>
        /// A byte array of per-byte style values, one entry for each byte of
        /// <paramref name="text" />.
        /// </returns>
        public static unsafe byte[] CharToByteStyles(
            byte[] styles,    /* in */
            byte* text,       /* in */
            int length,       /* in */
            Encoding encoding /* in */
            )
        {
            // This is used by annotations and margins to style all the text in
            // one call. It converts an array of styles where each element
            // corresponds to a CHARACTER (UTF-16 unit) to an array of styles
            // where each element corresponds to a BYTE. Uses the same stateful
            // Decoder.Convert as ByteToCharStyles, for the same consistency /
            // cross-runtime reasons; every byte of a character takes that
            // character's style (the style at its first UTF-16 unit).

            byte[] result = new byte[length];
            if (length == 0 || styles.Length == 0)
                return result;

            byte[] bytes = new byte[length];
            Marshal.Copy((IntPtr)text, bytes, 0, length);

            Decoder decoder = encoding.GetDecoder();
            char[] scratch = new char[2];
            int charPos = 0;
            int bytesUsed, units;
            bool completed;

            for (int bytePos = 0;
                bytePos < length && charPos < styles.Length; bytePos++)
            {
                result[bytePos] = styles[charPos];
                decoder.Convert(
                    bytes, bytePos, 1, scratch, 0, 2, false, out bytesUsed,
                    out units, out completed);
                charPos += units;
            }

            return result;
        }

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Copies a block of native memory into a newly allocated movable
        /// (GMEM_MOVEABLE) global memory handle suitable for the clipboard,
        /// using a direct native copy so no managed buffer is involved.
        /// </summary>
        /// <param name="source">
        /// A pointer to the source native memory to copy.
        /// </param>
        /// <param name="length">
        /// The number of bytes to copy.
        /// </param>
        /// <returns>
        /// A movable global memory handle owning a copy of the data, or
        /// <see cref="IntPtr.Zero" /> if allocation or locking fails.
        /// </returns>
        // The clipboard requires a movable (GMEM_MOVEABLE) global memory
        // handle, but NativeMemoryStream uses fixed (AllocHGlobal) memory;
        // copy the payload into a movable handle to give to SetClipboardData.
        // On success the clipboard owns the returned handle; the caller frees
        // it only if SetClipboardData fails.
        private static IntPtr CopyToMovableHGlobal(IntPtr source, int length)
        {
            IntPtr hGlobal = NativeMethods.GlobalAlloc(
                NativeMethods.GMEM_MOVEABLE, new UIntPtr((uint)length));
            if (hGlobal == IntPtr.Zero)
                return IntPtr.Zero;

            IntPtr dest = NativeMethods.GlobalLock(hGlobal);
            if (dest == IntPtr.Zero)
            {
                NativeMethods.GlobalFree(hGlobal);
                return IntPtr.Zero;
            }

            try
            {
                // Direct native copy (no intermediate managed buffer), so
                // nothing between GlobalLock and the return can throw and leak
                // the handle.
                NativeMethods.MoveMemory(
                    dest, source, new UIntPtr((uint)length));
            }
            finally
            {
                NativeMethods.GlobalUnlock(hGlobal);
            }

            return hGlobal;
        }

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Constrains a 32-bit integer value to the inclusive range defined
        /// by a minimum and a maximum.
        /// </summary>
        /// <param name="value">
        /// The value to constrain.
        /// </param>
        /// <param name="min">
        /// The inclusive lower bound.
        /// </param>
        /// <param name="max">
        /// The inclusive upper bound.
        /// </param>
        /// <returns>
        /// <paramref name="min" /> when <paramref name="value" /> is below
        /// it, <paramref name="max" /> when it is above it; otherwise
        /// <paramref name="value" /> unchanged.
        /// </returns>
        public static int Clamp(int value, int min, int max)
        {
            if (value < min)
                return min;

            if (value > max)
                return max;

            return value;
        }

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Constrains a 64-bit integer value to the inclusive range defined
        /// by a minimum and a maximum.
        /// </summary>
        /// <param name="value">
        /// The value to constrain.
        /// </param>
        /// <param name="min">
        /// The inclusive lower bound.
        /// </param>
        /// <param name="max">
        /// The inclusive upper bound.
        /// </param>
        /// <returns>
        /// <paramref name="min" /> when <paramref name="value" /> is below
        /// it, <paramref name="max" /> when it is above it; otherwise
        /// <paramref name="value" /> unchanged.
        /// </returns>
        public static long Clamp(long value, long min, long max)
        {
            if (value < min)
                return min;

            if (value > max)
                return max;

            return value;
        }

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Constrains a 32-bit integer value so that it is no less than the
        /// specified minimum.
        /// </summary>
        /// <param name="value">
        /// The value to constrain.
        /// </param>
        /// <param name="min">
        /// The inclusive lower bound.
        /// </param>
        /// <returns>
        /// <paramref name="min" /> when <paramref name="value" /> is below
        /// it; otherwise <paramref name="value" /> unchanged.
        /// </returns>
        public static int ClampMin(int value, int min)
        {
            if (value < min)
                return min;

            return value;
        }

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Constrains a 64-bit integer value so that it is no less than the
        /// specified minimum.
        /// </summary>
        /// <param name="value">
        /// The value to constrain.
        /// </param>
        /// <param name="min">
        /// The inclusive lower bound.
        /// </param>
        /// <returns>
        /// <paramref name="min" /> when <paramref name="value" /> is below
        /// it; otherwise <paramref name="value" /> unchanged.
        /// </returns>
        public static long ClampMin(long value, long min)
        {
            if (value < min)
                return min;

            return value;
        }

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Copies text from a <see cref="Scintilla" /> control to the
        /// clipboard in the requested formats (plain text, and/or styled RTF
        /// and HTML), optionally using the current selection or the current
        /// line.
        /// </summary>
        /// <param name="scintilla">
        /// The control whose content is being copied.
        /// </param>
        /// <param name="format">
        /// The clipboard formats to produce.
        /// </param>
        /// <param name="useSelection">
        /// true to copy the current selection; false to copy the byte range
        /// given by <paramref name="startBytePos" /> and
        /// <paramref name="endBytePos" />.
        /// </param>
        /// <param name="allowLine">
        /// true to copy the current line when the selection is empty.
        /// </param>
        /// <param name="startBytePos">
        /// The starting byte position of the range to copy when not using
        /// the selection.
        /// </param>
        /// <param name="endBytePos">
        /// The ending byte position of the range to copy when not using the
        /// selection.
        /// </param>
        public static void Copy(
            Scintilla scintilla, /* in */
            CopyFormat format,   /* in */
            bool useSelection,   /* in */
            bool allowLine,      /* in */
            long startBytePos,   /* in */
            long endBytePos      /* in */
            )
        {
            // Plain text
            if ((format & CopyFormat.Text) > 0)
            {
                if (useSelection)
                {
                    if (allowLine)
                        scintilla.DirectMessage(
                            NativeMethods.SCI_COPYALLOWLINE);
                    else
                        scintilla.DirectMessage(NativeMethods.SCI_COPY);
                }
                else
                {
                    scintilla.DirectMessage(NativeMethods.SCI_COPYRANGE,
                        new IntPtr(startBytePos), new IntPtr(endBytePos));
                }
            }

            // RTF and/or HTML
            if ((format & (CopyFormat.Rtf | CopyFormat.Html)) > 0)
            {
                // If we ever allow more than UTF-8, this will have to be
                // revisited
                Debug.Assert(scintilla.DirectMessage(
                    NativeMethods.SCI_GETCODEPAGE).ToInt32() ==
                    NativeMethods.SC_CP_UTF8);

                if (!registeredFormats)
                {
                    // Register non-standard clipboard formats.
                    // Scintilla -> ScintillaWin.cxx
                    // NppExport -> HTMLExporter.h
                    // NppExport -> RTFExporter.h

                    CF_LINESELECT = NativeMethods.RegisterClipboardFormat(
                        "MSDEVLineSelect");
                    CF_VSLINETAG = NativeMethods.RegisterClipboardFormat(
                        "VisualStudioEditorOperationsLineCutCopyClipboardTag");
                    CF_HTML = NativeMethods.RegisterClipboardFormat(
                        "HTML Format");
                    CF_RTF = NativeMethods.RegisterClipboardFormat(
                        "Rich Text Format");
                    registeredFormats = true;
                }

                bool lineCopy = false;
                StyleData[] styles = null;
                List<ArraySegment<byte>> styledSegments = null;

                if (useSelection)
                {
                    bool selIsEmpty = scintilla.DirectMessage(
                        NativeMethods.SCI_GETSELECTIONEMPTY) != IntPtr.Zero;
                    if (selIsEmpty)
                    {
                        if (allowLine)
                        {
                            // Get the current line
                            styledSegments = GetStyledSegments(
                                scintilla, false, true, 0, 0, out styles);
                            lineCopy = true;
                        }
                    }
                    else
                    {
                        // Get every selection
                        styledSegments = GetStyledSegments(
                            scintilla, true, false, 0, 0, out styles);
                    }
                }
                else if (startBytePos != endBytePos)
                {
                    // User-specified range
                    styledSegments = GetStyledSegments(
                        scintilla, false, false, startBytePos, endBytePos,
                        out styles);
                }

                // If we have segments and can open the clipboard
                if (styledSegments != null && styledSegments.Count > 0 &&
                    NativeMethods.OpenClipboard(scintilla.Handle))
                {
                    if ((format & CopyFormat.Text) == 0)
                    {
                        // Do the things default (plain text) processing would
                        // normally give us
                        NativeMethods.EmptyClipboard();

                        if (lineCopy)
                        {
                            // Clipboard tags
                            NativeMethods.SetClipboardData(
                                CF_LINESELECT, IntPtr.Zero);
                            NativeMethods.SetClipboardData(
                                CF_VSLINETAG, IntPtr.Zero);
                        }
                    }

                    // RTF
                    if ((format & CopyFormat.Rtf) > 0)
                        CopyRtf(scintilla, styles, styledSegments);

                    // HTML
                    if ((format & CopyFormat.Html) > 0)
                        CopyHtml(scintilla, styles, styledSegments);

                    NativeMethods.CloseClipboard();
                }
            }
        }

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Builds an HTML clipboard payload (CF_HTML) from the given styled
        /// text segments and places it on the clipboard. Styling is emitted
        /// as CSS spans; documents whose serialized size would overflow the
        /// CF_HTML eight-digit offset header are skipped.
        /// </summary>
        /// <param name="scintilla">
        /// The control providing style, tab, and line-ending information.
        /// </param>
        /// <param name="styles">
        /// The style table describing every style used by the segments.
        /// </param>
        /// <param name="styledSegments">
        /// The interleaved character and style byte segments to serialize.
        /// </param>
        private static unsafe void CopyHtml(
            Scintilla scintilla,                    /* in */
            StyleData[] styles,                     /* in */
            List<ArraySegment<byte>> styledSegments /* in */
            )
        {
            // NppExport -> NppExport.cpp
            // NppExport -> HTMLExporter.cpp
            // http://blogs.msdn.com/b/jmstall/archive/2007/01/21/html-clipboard.aspx
            // http://blogs.msdn.com/b/jmstall/archive/2007/01/21/sample-code-html-clipboard.aspx
            // https://msdn.microsoft.com/en-us/library/windows/desktop/ms649015.aspx

            try
            {
                long pos = 0;
                byte[] bytes;

                // Write HTML
                using (NativeMemoryStream ms = new NativeMemoryStream(
                    TotalCount(styledSegments)))
                using (StreamWriter tw = new StreamWriter(
                    ms, new UTF8Encoding(false)))
                {
                    const int INDEX_START_HTML = 23;
                    const int INDEX_START_FRAGMENT = 65;
                    const int INDEX_END_FRAGMENT = 87;
                    const int INDEX_END_HTML = 41;

                    tw.WriteLine("Version:0.9");
                    tw.WriteLine("StartHTML:00000000");
                    tw.WriteLine("EndHTML:00000000");
                    tw.WriteLine("StartFragment:00000000");
                    tw.WriteLine("EndFragment:00000000");
                    tw.Flush();

                    // Patch header
                    pos = ms.Position;
                    ms.Seek(INDEX_START_HTML, SeekOrigin.Begin);
                    ms.Write((bytes = Encoding.ASCII.GetBytes(
                        ms.Length.ToString("D8"))), 0, bytes.Length);
                    ms.Seek(pos, SeekOrigin.Begin);

                    tw.WriteLine("<html>");
                    tw.WriteLine("<head>");
                    tw.WriteLine(@"<meta charset=""utf-8"" />");
                    tw.WriteLine(@"<title>ScintillaNET v{0}</title>",
                        scintilla.GetType().Assembly.GetName().Version
                            .ToString(3));
                    tw.WriteLine("</head>");
                    tw.WriteLine("<body>");
                    tw.Flush();

                    // Patch header
                    pos = ms.Position;
                    ms.Seek(INDEX_START_FRAGMENT, SeekOrigin.Begin);
                    ms.Write((bytes = Encoding.ASCII.GetBytes(
                        ms.Length.ToString("D8"))), 0, bytes.Length);
                    ms.Seek(pos, SeekOrigin.Begin);
                    tw.WriteLine("<!--StartFragment -->");

                    // Write the styles.
                    // We're doing the style tag in the body to include it in
                    // the "fragment".
                    tw.WriteLine(@"<style type=""text/css"" scoped="""">");
                    tw.Write("div#segments {");
                    tw.Write(" float: left;");
                    tw.Write(" white-space: pre;");
                    tw.Write(" line-height: {0}px;",
                        scintilla.DirectMessage(NativeMethods.SCI_TEXTHEIGHT,
                            new IntPtr(0)).ToInt32());
                    tw.Write(" background-color: #{0:X2}{1:X2}{2:X2};",
                        (styles[Style.Default].BackColor >> 0) & 0xFF,
                        (styles[Style.Default].BackColor >> 8) & 0xFF,
                        (styles[Style.Default].BackColor >> 16) & 0xFF);
                    tw.WriteLine(" }");

                    for (int i = 0; i < styles.Length; i++)
                    {
                        if (!styles[i].Used)
                            continue;

                        tw.Write("span.s{0} {{", i);
                        tw.Write(@" font-family: ""{0}"";",
                            styles[i].FontName);
                        tw.Write(" font-size: {0}pt;", styles[i].SizeF);
                        tw.Write(" font-weight: {0};", styles[i].Weight);
                        if (styles[i].Italic != 0)
                            tw.Write(" font-style: italic;");
                        if (styles[i].Underline != 0)
                            tw.Write(" text-decoration: underline;");
                        tw.Write(" background-color: #{0:X2}{1:X2}{2:X2};",
                            (styles[i].BackColor >> 0) & 0xFF,
                            (styles[i].BackColor >> 8) & 0xFF,
                            (styles[i].BackColor >> 16) & 0xFF);
                        tw.Write(" color: #{0:X2}{1:X2}{2:X2};",
                            (styles[i].ForeColor >> 0) & 0xFF,
                            (styles[i].ForeColor >> 8) & 0xFF,
                            (styles[i].ForeColor >> 16) & 0xFF);
                        switch ((StyleCase)styles[i].Case)
                        {
                            case StyleCase.Upper:
                                tw.Write(" text-transform: uppercase;");
                                break;
                            case StyleCase.Lower:
                                tw.Write(" text-transform: lowercase;");
                                break;
                        }

                        if (styles[i].Visible == 0)
                            tw.Write(" visibility: hidden;");
                        tw.WriteLine(" }");
                    }

                    tw.WriteLine("</style>");
                    tw.Write(
                        @"<div id=""segments""><span class=""s{0}"">",
                        Style.Default);
                    tw.Flush();

                    int tabSize = scintilla.DirectMessage(
                        NativeMethods.SCI_GETTABWIDTH).ToInt32();
                    string tab = new string(' ', tabSize);

                    tw.AutoFlush = true;
                    int lastStyle = Style.Default;
                    bool unicodeLineEndings = ((scintilla.DirectMessage(
                        NativeMethods.SCI_GETLINEENDTYPESACTIVE).ToInt32() &
                        NativeMethods.SC_LINE_END_TYPE_UNICODE) > 0);
                    foreach (ArraySegment<byte> seg in styledSegments)
                    {
                        int endOffset = seg.Offset + seg.Count;
                        for (int i = seg.Offset; i < endOffset; i += 2)
                        {
                            byte ch = seg.Array[i];
                            byte style = seg.Array[i + 1];

                            if (lastStyle != style)
                            {
                                tw.Write(
                                    @"</span><span class=""s{0}"">", style);
                                lastStyle = style;
                            }

                            switch (ch)
                            {
                                case (byte)'<':
                                    tw.Write("&lt;");
                                    break;

                                case (byte)'>':
                                    tw.Write("&gt;");
                                    break;

                                case (byte)'&':
                                    tw.Write("&amp;");
                                    break;

                                case (byte)'\t':
                                    tw.Write(tab);
                                    break;

                                case (byte)'\r':
                                    if (i + 2 < endOffset)
                                    {
                                        if (seg.Array[i + 2] == (byte)'\n')
                                            i += 2;
                                    }

                                    // Either way, this is a line break
                                    goto case (byte)'\n';

                                case 0xC2:
                                    if (unicodeLineEndings &&
                                        i + 2 < endOffset)
                                    {
                                        // NEL \u0085
                                        if (seg.Array[i + 2] == 0x85)
                                        {
                                            i += 2;
                                            goto case (byte)'\n';
                                        }
                                    }

                                    // Not a Unicode line break
                                    goto default;

                                case 0xE2:
                                    if (unicodeLineEndings &&
                                        i + 4 < endOffset)
                                    {
                                        if (seg.Array[i + 2] == 0x80 &&
                                            // LS \u2028
                                            seg.Array[i + 4] == 0xA8)
                                        {
                                            i += 4;
                                            goto case (byte)'\n';
                                        }
                                        else if (seg.Array[i + 2] == 0x80 &&
                                            // PS \u2029
                                            seg.Array[i + 4] == 0xA9)
                                        {
                                            i += 4;
                                            goto case (byte)'\n';
                                        }
                                    }

                                    // Not a Unicode line break
                                    goto default;

                                case (byte)'\n':
                                    // All your line breaks are belong to us
                                    tw.Write("\r\n");
                                    break;

                                default:

                                    if (ch == 0)
                                    {
                                        // Scintilla behavior is to allow
                                        // control characters except for NULL
                                        // which will cause the Clipboard to
                                        // truncate the string.
                                        tw.Write(" "); // Replace with space
                                        break;
                                    }

                                    ms.WriteByte(ch);
                                    break;
                            }
                        }
                    }

                    tw.AutoFlush = false;
                    tw.WriteLine("</span></div>");
                    tw.Flush();

                    // Patch header
                    pos = ms.Position;
                    ms.Seek(INDEX_END_FRAGMENT, SeekOrigin.Begin);
                    ms.Write((bytes = Encoding.ASCII.GetBytes(
                        ms.Length.ToString("D8"))), 0, bytes.Length);
                    ms.Seek(pos, SeekOrigin.Begin);
                    tw.WriteLine("<!--EndFragment-->");

                    tw.WriteLine("</body>");
                    tw.WriteLine("</html>");
                    tw.Flush();

                    // Patch header
                    pos = ms.Position;
                    ms.Seek(INDEX_END_HTML, SeekOrigin.Begin);
                    ms.Write((bytes = Encoding.ASCII.GetBytes(
                        ms.Length.ToString("D8"))), 0, bytes.Length);
                    ms.Seek(pos, SeekOrigin.Begin);

                    // Terminator
                    ms.WriteByte(0);

                    // The CF_HTML header reserves 8-digit offset fields; a
                    // serialized payload of 100,000,000+ bytes overflows them
                    // ("D8" is a minimum, not a maximum, width) and corrupts
                    // the header. For such pathologically large documents,
                    // skip the HTML clipboard format rather than publishing a
                    // malformed payload.
                    if (ms.Length <= 99999999)
                    {
                        // Hand the clipboard a movable copy; ms keeps and
                        // frees its own buffer.
                        IntPtr hGlobal = CopyToMovableHGlobal(
                            ms.Pointer, (int)ms.Length);
                        if (hGlobal != IntPtr.Zero &&
                            NativeMethods.SetClipboardData(
                                CF_HTML, hGlobal) == IntPtr.Zero)
                            // clipboard rejected it; release the copy
                            NativeMethods.GlobalFree(hGlobal);
                    }
                }
            }
            catch (Exception ex)
            {
                // Yes, we swallow any exceptions. That may seem like code
                // smell but this matches the behavior of the Clipboard class,
                // Windows Forms controls, and native Scintilla.
                Debug.Fail(ex.Message, ex.ToString());
            }
        }

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Builds a Rich Text Format (RTF) clipboard payload (CF_RTF) from
        /// the given styled text segments and places it on the clipboard,
        /// emitting font and color tables and encoding non-ASCII text as
        /// Unicode escapes.
        /// </summary>
        /// <param name="scintilla">
        /// The control providing style, tab, and line-ending information.
        /// </param>
        /// <param name="styles">
        /// The style table describing every style used by the segments.
        /// </param>
        /// <param name="styledSegments">
        /// The interleaved character and style byte segments to serialize.
        /// </param>
        private static unsafe void CopyRtf(
            Scintilla scintilla,                    /* in */
            StyleData[] styles,                     /* in */
            List<ArraySegment<byte>> styledSegments /* in */
            )
        {
            // NppExport -> NppExport.cpp
            // NppExport -> RTFExporter.cpp
            // http://en.wikipedia.org/wiki/Rich_Text_Format
            // https://msdn.microsoft.com/en-us/library/windows/desktop/ms649013.aspx
            // http://forums.codeguru.com/showthread.php?242982-Converting-pixels-to-twips
            // http://en.wikipedia.org/wiki/UTF-8

            try
            {
                // Calculate twips per space
                int twips;
                FontStyle fontStyle = FontStyle.Regular;
                if (styles[Style.Default].Weight >= 700)
                    fontStyle |= FontStyle.Bold;
                if (styles[Style.Default].Italic != 0)
                    fontStyle |= FontStyle.Italic;
                if (styles[Style.Default].Underline != 0)
                    fontStyle |= FontStyle.Underline;

                using (Graphics graphics = scintilla.CreateGraphics())
                using (Font font = new Font(styles[Style.Default].FontName,
                    styles[Style.Default].SizeF, fontStyle))
                using (StringFormat format = new StringFormat(
                    StringFormat.GenericTypographic))
                {
                    // Graphics.MeasureString ignores trailing whitespace
                    // unless MeasureTrailingSpaces is set, so measuring a lone
                    // " " returned almost nothing -- which made the tab width
                    // far too small. Measure the real advance width of a
                    // space, then convert pixels -> twips (1 inch = 1440
                    // twips).
                    format.FormatFlags |=
                        StringFormatFlags.MeasureTrailingSpaces;
                    float width = graphics.MeasureString(
                        " ", font, PointF.Empty, format).Width;
                    twips = (int)Math.Round((width / graphics.DpiX) * 1440);
                }

                // Write RTF
                using (NativeMemoryStream ms = new NativeMemoryStream(
                    TotalCount(styledSegments)))
                using (StreamWriter tw = new StreamWriter(ms, Encoding.ASCII))
                {
                    int tabWidth = scintilla.DirectMessage(
                        NativeMethods.SCI_GETTABWIDTH).ToInt32();
                    int deftab = tabWidth * twips;

                    tw.WriteLine(@"{{\rtf1\ansi\deff0\deftab{0}", deftab);
                    tw.Flush();

                    // Build the font table
                    tw.Write(@"{\fonttbl");
                    tw.Write(@"{{\f0 {0};}}", styles[Style.Default].FontName);
                    int fontIndex = 1;
                    for (int i = 0; i < styles.Length; i++)
                    {
                        if (!styles[i].Used)
                            continue;

                        if (i == Style.Default)
                            continue;

                        // Not a completely unique list, but close enough
                        if (styles[i].FontName !=
                            styles[Style.Default].FontName)
                        {
                            styles[i].FontIndex = fontIndex++;
                            tw.Write(@"{{\f{0} {1};}}",
                                styles[i].FontIndex, styles[i].FontName);
                        }
                    }
                    tw.WriteLine("}"); // fonttbl
                    tw.Flush();

                    // Build the color table
                    tw.Write(@"{\colortbl");
                    tw.Write(@"\red{0}\green{1}\blue{2};",
                        (styles[Style.Default].ForeColor >> 0) & 0xFF,
                        (styles[Style.Default].ForeColor >> 8) & 0xFF,
                        (styles[Style.Default].ForeColor >> 16) & 0xFF);
                    tw.Write(@"\red{0}\green{1}\blue{2};",
                        (styles[Style.Default].BackColor >> 0) & 0xFF,
                        (styles[Style.Default].BackColor >> 8) & 0xFF,
                        (styles[Style.Default].BackColor >> 16) & 0xFF);
                    styles[Style.Default].ForeColorIndex = 0;
                    styles[Style.Default].BackColorIndex = 1;
                    int colorIndex = 2;
                    for (int i = 0; i < styles.Length; i++)
                    {
                        if (!styles[i].Used)
                            continue;

                        if (i == Style.Default)
                            continue;

                        // Not a completely unique list, but close enough
                        if (styles[i].ForeColor !=
                            styles[Style.Default].ForeColor)
                        {
                            styles[i].ForeColorIndex = colorIndex++;
                            tw.Write(@"\red{0}\green{1}\blue{2};",
                                (styles[i].ForeColor >> 0) & 0xFF,
                                (styles[i].ForeColor >> 8) & 0xFF,
                                (styles[i].ForeColor >> 16) & 0xFF);
                        }
                        else
                        {
                            styles[i].ForeColorIndex =
                                styles[Style.Default].ForeColorIndex;
                        }

                        if (styles[i].BackColor !=
                            styles[Style.Default].BackColor)
                        {
                            styles[i].BackColorIndex = colorIndex++;
                            tw.Write(@"\red{0}\green{1}\blue{2};",
                                (styles[i].BackColor >> 0) & 0xFF,
                                (styles[i].BackColor >> 8) & 0xFF,
                                (styles[i].BackColor >> 16) & 0xFF);
                        }
                        else
                        {
                            styles[i].BackColorIndex =
                                styles[Style.Default].BackColorIndex;
                        }
                    }
                    tw.WriteLine("}"); // colortbl
                    tw.Flush();

                    // Start with the default style
                    tw.Write(@"\f{0}\fs{1}\cf{2}\chshdng0\chcbpat{3}\cb{3} ",
                        styles[Style.Default].FontIndex,
                        (int)(styles[Style.Default].SizeF * 2),
                        styles[Style.Default].ForeColorIndex,
                        styles[Style.Default].BackColorIndex);
                    if (styles[Style.Default].Italic != 0)
                        tw.Write(@"\i");
                    if (styles[Style.Default].Underline != 0)
                        tw.Write(@"\ul");
                    if (styles[Style.Default].Weight >= 700)
                        tw.Write(@"\b");

                    tw.AutoFlush = true;
                    int lastStyle = Style.Default;
                    bool unicodeLineEndings = ((scintilla.DirectMessage(
                        NativeMethods.SCI_GETLINEENDTYPESACTIVE).ToInt32() &
                        NativeMethods.SC_LINE_END_TYPE_UNICODE) > 0);
                    foreach (ArraySegment<byte> seg in styledSegments)
                    {
                        int endOffset = seg.Offset + seg.Count;
                        for (int i = seg.Offset; i < endOffset; i += 2)
                        {
                            byte ch = seg.Array[i];
                            byte style = seg.Array[i + 1];

                            if (lastStyle != style)
                            {
                                // Change the style
                                if (styles[lastStyle].FontIndex !=
                                    styles[style].FontIndex)
                                    tw.Write(
                                        @"\f{0}", styles[style].FontIndex);
                                if (styles[lastStyle].SizeF !=
                                    styles[style].SizeF)
                                    tw.Write(@"\fs{0}",
                                        (int)(styles[style].SizeF * 2));
                                if (styles[lastStyle].ForeColorIndex !=
                                    styles[style].ForeColorIndex)
                                    tw.Write(@"\cf{0}",
                                        styles[style].ForeColorIndex);
                                if (styles[lastStyle].BackColorIndex !=
                                    styles[style].BackColorIndex)
                                    tw.Write(@"\chshdng0\chcbpat{0}\cb{0}",
                                        styles[style].BackColorIndex);
                                if (styles[lastStyle].Italic !=
                                    styles[style].Italic)
                                    tw.Write(@"\i{0}",
                                        styles[style].Italic != 0 ? "" : "0");
                                if (styles[lastStyle].Underline !=
                                    styles[style].Underline)
                                    tw.Write(@"\ul{0}",
                                        styles[style].Underline != 0
                                            ? "" : "0");
                                if (styles[lastStyle].Weight !=
                                    styles[style].Weight)
                                {
                                    if (styles[style].Weight >= 700 &&
                                        styles[lastStyle].Weight < 700)
                                        tw.Write(@"\b");
                                    else if (styles[style].Weight < 700 &&
                                        styles[lastStyle].Weight >= 700)
                                        tw.Write(@"\b0");
                                }

                                // NOTE: We don't support StyleData.Visible and
                                // StyleData.Case in RTF

                                lastStyle = style;
                                tw.Write("\n"); // Delimiter
                            }

                            switch (ch)
                            {
                                case (byte)'{':
                                    tw.Write(@"\{");
                                    break;

                                case (byte)'}':
                                    tw.Write(@"\}");
                                    break;

                                case (byte)'\\':
                                    tw.Write(@"\\");
                                    break;

                                case (byte)'\t':
                                    tw.Write(@"\tab ");
                                    break;

                                case (byte)'\r':
                                    if (i + 2 < endOffset)
                                    {
                                        if (seg.Array[i + 2] == (byte)'\n')
                                            i += 2;
                                    }

                                    // Either way, this is a line break
                                    goto case (byte)'\n';

                                case 0xC2:
                                    if (unicodeLineEndings &&
                                        i + 2 < endOffset)
                                    {
                                        // NEL \u0085
                                        if (seg.Array[i + 2] == 0x85)
                                        {
                                            i += 2;
                                            goto case (byte)'\n';
                                        }
                                    }

                                    // Not a Unicode line break
                                    goto default;

                                case 0xE2:
                                    if (unicodeLineEndings &&
                                        i + 4 < endOffset)
                                    {
                                        if (seg.Array[i + 2] == 0x80 &&
                                            // LS \u2028
                                            seg.Array[i + 4] == 0xA8)
                                        {
                                            i += 4;
                                            goto case (byte)'\n';
                                        }
                                        else if (seg.Array[i + 2] == 0x80 &&
                                            // PS \u2029
                                            seg.Array[i + 4] == 0xA9)
                                        {
                                            i += 4;
                                            goto case (byte)'\n';
                                        }
                                    }

                                    // Not a Unicode line break
                                    goto default;

                                case (byte)'\n':
                                    // All your line breaks are belong to us
                                    tw.WriteLine(@"\par");
                                    break;

                                default:

                                    if (ch == 0)
                                    {
                                        // Scintilla behavior is to allow
                                        // control characters except for NULL
                                        // which will cause the Clipboard to
                                        // truncate the string.
                                        tw.Write(" "); // Replace with space
                                        break;
                                    }

                                    if (ch > 0x7F)
                                    {
                                        // Treat as UTF-8 code point
                                        int unicode = 0;
                                        if (ch < 0xE0 && i + 2 < endOffset)
                                        {
                                            unicode |= ((0x1F & ch) << 6);
                                            unicode |=
                                                (0x3F & seg.Array[i + 2]);
                                            tw.Write(@"\u{0}?", unicode);
                                            i += 2;
                                            break;
                                        }
                                        else if (ch < 0xF0 &&
                                            i + 4 < endOffset)
                                        {
                                            unicode |= ((0xF & ch) << 12);
                                            unicode |= ((0x3F &
                                                seg.Array[i + 2]) << 6);
                                            unicode |=
                                                (0x3F & seg.Array[i + 4]);
                                            tw.Write(@"\u{0}?", unicode);
                                            i += 4;
                                            break;
                                        }
                                        else if (ch < 0xF8 &&
                                            i + 6 < endOffset)
                                        {
                                            unicode |= ((0x7 & ch) << 18);
                                            unicode |= ((0x3F &
                                                seg.Array[i + 2]) << 12);
                                            unicode |= ((0x3F &
                                                seg.Array[i + 4]) << 6);
                                            unicode |=
                                                (0x3F & seg.Array[i + 6]);
                                            tw.Write(@"\u{0}?", unicode);
                                            i += 6;
                                            break;
                                        }
                                    }

                                    // Regular ANSI char
                                    ms.WriteByte(ch);
                                    break;
                            }
                        }
                    }

                    tw.AutoFlush = false;
                    tw.WriteLine("}"); // rtf1
                    tw.Flush();

                    // Terminator
                    ms.WriteByte(0);

                    // Hand the clipboard a movable copy; ms keeps and frees
                    // its own buffer.
                    IntPtr hGlobal = CopyToMovableHGlobal(
                        ms.Pointer, (int)ms.Length);
                    if (hGlobal != IntPtr.Zero &&
                        NativeMethods.SetClipboardData(
                            CF_RTF, hGlobal) == IntPtr.Zero)
                        // clipboard rejected it; release the copy
                        NativeMethods.GlobalFree(hGlobal);
                }
            }
            catch (Exception ex)
            {
                // Yes, we swallow any exceptions. That may seem like code
                // smell but this matches the behavior of the Clipboard class,
                // Windows Forms controls, and native Scintilla.
                Debug.Fail(ex.Message, ex.ToString());
            }
        }

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Encodes a string into a newly allocated byte array using the
        /// specified encoding, optionally appending a terminating zero byte.
        /// </summary>
        /// <param name="text">
        /// The string to encode.
        /// </param>
        /// <param name="encoding">
        /// The encoding used to produce the bytes.
        /// </param>
        /// <param name="zeroTerminated">
        /// true to append a terminating zero byte to the result.
        /// </param>
        /// <returns>
        /// A byte array containing the encoded text, plus a trailing zero
        /// byte when <paramref name="zeroTerminated" /> is true.
        /// </returns>
        public static unsafe byte[] GetBytes(
            string text,        /* in */
            Encoding encoding,  /* in */
            bool zeroTerminated /* in */
            )
        {
            if (string.IsNullOrEmpty(text))
                return (zeroTerminated ? new byte[] { 0 } : new byte[0]);

            int count = encoding.GetByteCount(text);
            byte[] buffer = new byte[count + (zeroTerminated ? 1 : 0)];

            fixed (byte* bp = buffer)
            fixed (char* ch = text)
            {
                encoding.GetBytes(ch, text.Length, bp, count);
            }

            if (zeroTerminated)
                buffer[buffer.Length - 1] = 0;

            return buffer;
        }

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Encodes the specified number of characters from a character array
        /// into a newly allocated byte array using the given encoding,
        /// optionally appending a terminating zero byte.
        /// </summary>
        /// <param name="text">
        /// The character array to encode.
        /// </param>
        /// <param name="length">
        /// The number of characters from <paramref name="text" /> to encode.
        /// </param>
        /// <param name="encoding">
        /// The encoding used to produce the bytes.
        /// </param>
        /// <param name="zeroTerminated">
        /// true to append a terminating zero byte to the result.
        /// </param>
        /// <returns>
        /// A byte array containing the encoded characters, plus a trailing
        /// zero byte when <paramref name="zeroTerminated" /> is true.
        /// </returns>
        public static unsafe byte[] GetBytes(
            char[] text,        /* in */
            int length,         /* in */
            Encoding encoding,  /* in */
            bool zeroTerminated /* in */
            )
        {
            fixed (char* cp = text)
            {
                int count = encoding.GetByteCount(cp, length);
                byte[] buffer = new byte[count + (zeroTerminated ? 1 : 0)];
                fixed (byte* bp = buffer)
                    encoding.GetBytes(cp, length, bp, buffer.Length);

                if (zeroTerminated)
                    buffer[buffer.Length - 1] = 0;

                return buffer;
            }
        }

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Produces an HTML fragment (CSS styles plus styled markup)
        /// representing the given byte range of a
        /// <see cref="Scintilla" /> control.
        /// </summary>
        /// <param name="scintilla">
        /// The control whose styled text is being exported.
        /// </param>
        /// <param name="startBytePos">
        /// The starting byte position of the range.
        /// </param>
        /// <param name="endBytePos">
        /// The ending byte position of the range.
        /// </param>
        /// <returns>
        /// An HTML string describing the styled text, or an empty string
        /// when the range is empty.
        /// </returns>
        public static string GetHtml(
            Scintilla scintilla, /* in */
            long startBytePos,   /* in */
            long endBytePos      /* in */
            )
        {
            // If we ever allow more than UTF-8, this will have to be revisited
            Debug.Assert(scintilla.DirectMessage(
                NativeMethods.SCI_GETCODEPAGE).ToInt32() ==
                NativeMethods.SC_CP_UTF8);

            if (startBytePos == endBytePos)
                return string.Empty;

            StyleData[] styles = null;
            List<ArraySegment<byte>> styledSegments = GetStyledSegments(
                scintilla, false, false, startBytePos, endBytePos,
                out styles);

            using (NativeMemoryStream ms = new NativeMemoryStream(
                TotalCount(styledSegments))) // Hint
            using (StreamWriter sw = new StreamWriter(
                ms, new UTF8Encoding(false)))
            {
                // Write the styles
                sw.WriteLine(@"<style type=""text/css"" scoped="""">");
                sw.Write("div#segments {");
                sw.Write(" float: left;");
                sw.Write(" white-space: pre;");
                sw.Write(" line-height: {0}px;",
                    scintilla.DirectMessage(NativeMethods.SCI_TEXTHEIGHT,
                        new IntPtr(0)).ToInt32());
                sw.Write(" background-color: #{0:X2}{1:X2}{2:X2};",
                    (styles[Style.Default].BackColor >> 0) & 0xFF,
                    (styles[Style.Default].BackColor >> 8) & 0xFF,
                    (styles[Style.Default].BackColor >> 16) & 0xFF);
                sw.WriteLine(" }");

                for (int i = 0; i < styles.Length; i++)
                {
                    if (!styles[i].Used)
                        continue;

                    sw.Write("span.s{0} {{", i);
                    sw.Write(@" font-family: ""{0}"";", styles[i].FontName);
                    sw.Write(" font-size: {0}pt;", styles[i].SizeF);
                    sw.Write(" font-weight: {0};", styles[i].Weight);
                    if (styles[i].Italic != 0)
                        sw.Write(" font-style: italic;");
                    if (styles[i].Underline != 0)
                        sw.Write(" text-decoration: underline;");
                    sw.Write(" background-color: #{0:X2}{1:X2}{2:X2};",
                        (styles[i].BackColor >> 0) & 0xFF,
                        (styles[i].BackColor >> 8) & 0xFF,
                        (styles[i].BackColor >> 16) & 0xFF);
                    sw.Write(" color: #{0:X2}{1:X2}{2:X2};",
                        (styles[i].ForeColor >> 0) & 0xFF,
                        (styles[i].ForeColor >> 8) & 0xFF,
                        (styles[i].ForeColor >> 16) & 0xFF);
                    switch ((StyleCase)styles[i].Case)
                    {
                        case StyleCase.Upper:
                            sw.Write(" text-transform: uppercase;");
                            break;
                        case StyleCase.Lower:
                            sw.Write(" text-transform: lowercase;");
                            break;
                    }

                    if (styles[i].Visible == 0)
                        sw.Write(" visibility: hidden;");

                    sw.WriteLine(" }");
                }

                sw.WriteLine("</style>");

                bool unicodeLineEndings = ((scintilla.DirectMessage(
                    NativeMethods.SCI_GETLINEENDTYPESACTIVE).ToInt32() &
                    NativeMethods.SC_LINE_END_TYPE_UNICODE) > 0);
                int tabSize = scintilla.DirectMessage(
                    NativeMethods.SCI_GETTABWIDTH).ToInt32();
                string tab = new string(' ', tabSize);
                int lastStyle = Style.Default;

                // Write the styled text
                sw.Write(
                    @"<div id=""segments""><span class=""s{0}"">",
                    Style.Default);
                sw.Flush();
                sw.AutoFlush = true;

                foreach (ArraySegment<byte> seg in styledSegments)
                {
                    int endOffset = seg.Offset + seg.Count;
                    for (int i = seg.Offset; i < endOffset; i += 2)
                    {
                        byte ch = seg.Array[i];
                        byte style = seg.Array[i + 1];

                        if (lastStyle != style)
                        {
                            sw.Write(@"</span><span class=""s{0}"">", style);
                            lastStyle = style;
                        }

                        switch (ch)
                        {
                            case (byte)'<':
                                sw.Write("&lt;");
                                break;

                            case (byte)'>':
                                sw.Write("&gt;");
                                break;

                            case (byte)'&':
                                sw.Write("&amp;");
                                break;

                            case (byte)'\t':
                                sw.Write(tab);
                                break;

                            case (byte)'\r':
                                if (i + 2 < endOffset)
                                {
                                    if (seg.Array[i + 2] == (byte)'\n')
                                        i += 2;
                                }

                                // Either way, this is a line break
                                goto case (byte)'\n';

                            case 0xC2:
                                if (unicodeLineEndings && i + 2 < endOffset)
                                {
                                    if (seg.Array[i + 2] == 0x85) // NEL \u0085
                                    {
                                        i += 2;
                                        goto case (byte)'\n';
                                    }
                                }

                                // Not a Unicode line break
                                goto default;

                            case 0xE2:
                                if (unicodeLineEndings && i + 4 < endOffset)
                                {
                                    if (seg.Array[i + 2] == 0x80 &&
                                        seg.Array[i + 4] == 0xA8) // LS \u2028
                                    {
                                        i += 4;
                                        goto case (byte)'\n';
                                    }
                                    else if (seg.Array[i + 2] == 0x80 &&
                                        seg.Array[i + 4] == 0xA9) // PS \u2029
                                    {
                                        i += 4;
                                        goto case (byte)'\n';
                                    }
                                }

                                // Not a Unicode line break
                                goto default;

                            case (byte)'\n':
                                // All your line breaks are belong to us
                                sw.Write("\r\n");
                                break;

                            default:

                                if (ch == 0)
                                {
                                    // Replace NUL with space
                                    sw.Write(" ");
                                    break;
                                }

                                ms.WriteByte(ch);
                                break;
                        }
                    }
                }

                sw.AutoFlush = false;
                sw.WriteLine("</span></div>");
                sw.Flush();

                return GetString(ms.Pointer, (int)ms.Length, Encoding.UTF8);
            }
        }

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Constructs a managed string from a native byte buffer using the
        /// specified encoding.
        /// </summary>
        /// <param name="bytes">
        /// A pointer to the native byte buffer to decode.
        /// </param>
        /// <param name="length">
        /// The number of bytes to decode from <paramref name="bytes" />.
        /// </param>
        /// <param name="encoding">
        /// The encoding used to decode the bytes.
        /// </param>
        /// <returns>
        /// The decoded string.
        /// </returns>
        public static unsafe string GetString(
            IntPtr bytes,     /* in */
            int length,       /* in */
            Encoding encoding /* in */
            )
        {
            sbyte* ptr = (sbyte*)bytes;
            string str = new string(ptr, 0, length, encoding);

            return str;
        }

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Retrieves the interleaved character and style byte segments for
        /// the requested content -- every selection, the current line, or an
        /// explicit byte range -- and builds the table of styles those
        /// segments use.
        /// </summary>
        /// <param name="scintilla">
        /// The control to read styled text from.
        /// </param>
        /// <param name="currentSelection">
        /// true to gather one segment per selection.
        /// </param>
        /// <param name="currentLine">
        /// true to gather the single current line, when not gathering
        /// selections.
        /// </param>
        /// <param name="startBytePos">
        /// The starting byte position when gathering an explicit range.
        /// </param>
        /// <param name="endBytePos">
        /// The ending byte position when gathering an explicit range.
        /// </param>
        /// <param name="styles">
        /// On return, the table of <see cref="StyleData" /> entries
        /// describing every style referenced by the returned segments.
        /// </param>
        /// <returns>
        /// A list of array segments, each holding interleaved character and
        /// style bytes.
        /// </returns>
        private static unsafe List<ArraySegment<byte>> GetStyledSegments(
            Scintilla scintilla,   /* in */
            bool currentSelection, /* in */
            bool currentLine,      /* in */
            long startBytePos,     /* in */
            long endBytePos,       /* in */
            out StyleData[] styles /* out */
            )
        {
            List<ArraySegment<byte>> segments = new List<ArraySegment<byte>>();
            if (currentSelection)
            {
                // Get each selection as a segment.
                // Rectangular selections are ordered top to bottom and have
                // line breaks appended.
                List<Tuple<long, long>> ranges =
                    new List<Tuple<long, long>>();
                int selCount = scintilla.DirectMessage(
                    NativeMethods.SCI_GETSELECTIONS).ToInt32();
                for (int i = 0; i < selCount; i++)
                {
                    long selStartBytePos = scintilla.DirectMessage(
                        NativeMethods.SCI_GETSELECTIONNSTART,
                        new IntPtr(i)).ToInt64();
                    long selEndBytePos = scintilla.DirectMessage(
                        NativeMethods.SCI_GETSELECTIONNEND,
                        new IntPtr(i)).ToInt64();

                    ranges.Add(Tuple.Create(selStartBytePos, selEndBytePos));
                }

                bool selIsRect = scintilla.DirectMessage(
                    NativeMethods.SCI_SELECTIONISRECTANGLE) != IntPtr.Zero;
                if (selIsRect)
                {
                    // NOTE: the original discarded OrderBy's result (a no-op);
                    // this actually sorts the ranges top to bottom, as the
                    // comment intended.
                    ranges.Sort(delegate(
                        Tuple<long, long> x, Tuple<long, long> y)
                    {
                        return x.Item1.CompareTo(y.Item1);
                    });
                }

                foreach (Tuple<long, long> range in ranges)
                {
                    ArraySegment<byte> styledText = GetStyledText(
                        scintilla, range.Item1, range.Item2, selIsRect);
                    segments.Add(styledText);
                }
            }
            else if (currentLine)
            {
                // Get the current line
                int mainSelection = scintilla.DirectMessage(
                    NativeMethods.SCI_GETMAINSELECTION).ToInt32();
                long mainCaretPos = scintilla.DirectMessage(
                    NativeMethods.SCI_GETSELECTIONNCARET,
                    new IntPtr(mainSelection)).ToInt64();
                long lineIndex = scintilla.DirectMessage(
                    NativeMethods.SCI_LINEFROMPOSITION,
                    new IntPtr(mainCaretPos)).ToInt64();
                long lineStartBytePos = scintilla.DirectMessage(
                    NativeMethods.SCI_POSITIONFROMLINE,
                    new IntPtr(lineIndex)).ToInt64();
                long lineLength = scintilla.DirectMessage(
                    NativeMethods.SCI_LINELENGTH,
                    new IntPtr(lineIndex)).ToInt64();

                ArraySegment<byte> styledText = GetStyledText(
                    scintilla, lineStartBytePos,
                    (lineStartBytePos + lineLength), false);
                segments.Add(styledText);
            }
            else // User-specified range
            {
                Debug.Assert(startBytePos != endBytePos);
                ArraySegment<byte> styledText = GetStyledText(
                    scintilla, startBytePos, endBytePos, false);
                segments.Add(styledText);
            }

            // Build a list of (used) styles
            styles = new StyleData[NativeMethods.STYLE_MAX + 1];

            styles[Style.Default].Used = true;
            styles[Style.Default].FontName =
                scintilla.Styles[Style.Default].Font;
            styles[Style.Default].SizeF =
                scintilla.Styles[Style.Default].SizeF;
            styles[Style.Default].Weight = scintilla.DirectMessage(
                NativeMethods.SCI_STYLEGETWEIGHT, new IntPtr(Style.Default),
                IntPtr.Zero).ToInt32();
            styles[Style.Default].Italic = scintilla.DirectMessage(
                NativeMethods.SCI_STYLEGETITALIC, new IntPtr(Style.Default),
                IntPtr.Zero).ToInt32();
            styles[Style.Default].Underline = scintilla.DirectMessage(
                NativeMethods.SCI_STYLEGETUNDERLINE, new IntPtr(Style.Default),
                IntPtr.Zero).ToInt32();
            styles[Style.Default].BackColor = scintilla.DirectMessage(
                NativeMethods.SCI_STYLEGETBACK, new IntPtr(Style.Default),
                IntPtr.Zero).ToInt32();
            styles[Style.Default].ForeColor = scintilla.DirectMessage(
                NativeMethods.SCI_STYLEGETFORE, new IntPtr(Style.Default),
                IntPtr.Zero).ToInt32();
            styles[Style.Default].Case = scintilla.DirectMessage(
                NativeMethods.SCI_STYLEGETCASE, new IntPtr(Style.Default),
                IntPtr.Zero).ToInt32();
            styles[Style.Default].Visible = scintilla.DirectMessage(
                NativeMethods.SCI_STYLEGETVISIBLE, new IntPtr(Style.Default),
                IntPtr.Zero).ToInt32();

            foreach (ArraySegment<byte> seg in segments)
            {
                for (int i = 0; i < seg.Count; i += 2)
                {
                    byte style = seg.Array[i + 1];
                    if (!styles[style].Used)
                    {
                        styles[style].Used = true;
                        styles[style].FontName =
                            scintilla.Styles[style].Font;
                        styles[style].SizeF = scintilla.Styles[style].SizeF;
                        styles[style].Weight = scintilla.DirectMessage(
                            NativeMethods.SCI_STYLEGETWEIGHT,
                            new IntPtr(style), IntPtr.Zero).ToInt32();
                        styles[style].Italic = scintilla.DirectMessage(
                            NativeMethods.SCI_STYLEGETITALIC,
                            new IntPtr(style), IntPtr.Zero).ToInt32();
                        styles[style].Underline = scintilla.DirectMessage(
                            NativeMethods.SCI_STYLEGETUNDERLINE,
                            new IntPtr(style), IntPtr.Zero).ToInt32();
                        styles[style].BackColor = scintilla.DirectMessage(
                            NativeMethods.SCI_STYLEGETBACK,
                            new IntPtr(style), IntPtr.Zero).ToInt32();
                        styles[style].ForeColor = scintilla.DirectMessage(
                            NativeMethods.SCI_STYLEGETFORE,
                            new IntPtr(style), IntPtr.Zero).ToInt32();
                        styles[style].Case = scintilla.DirectMessage(
                            NativeMethods.SCI_STYLEGETCASE,
                            new IntPtr(style), IntPtr.Zero).ToInt32();
                        styles[style].Visible = scintilla.DirectMessage(
                            NativeMethods.SCI_STYLEGETVISIBLE,
                            new IntPtr(style), IntPtr.Zero).ToInt32();
                    }
                }
            }

            return segments;
        }

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Retrieves a single range of text together with its per-byte
        /// styles as an interleaved character and style byte array,
        /// optionally appending a styled line break for rectangular-selection
        /// rows.
        /// </summary>
        /// <param name="scintilla">
        /// The control to read styled text from.
        /// </param>
        /// <param name="startBytePos">
        /// The starting byte position of the range.
        /// </param>
        /// <param name="endBytePos">
        /// The ending byte position of the range.
        /// </param>
        /// <param name="addLineBreak">
        /// true to append a carriage-return and line-feed pair, carrying the
        /// trailing style, to the segment.
        /// </param>
        /// <returns>
        /// An array segment containing the interleaved character and style
        /// bytes for the range.
        /// </returns>
        private static unsafe ArraySegment<byte> GetStyledText(
            Scintilla scintilla, /* in */
            long startBytePos,   /* in */
            long endBytePos,     /* in */
            bool addLineBreak    /* in */
            )
        {
            Debug.Assert(endBytePos > startBytePos);

            // Make sure the range is styled
            scintilla.DirectMessage(NativeMethods.SCI_COLOURISE,
                new IntPtr(startBytePos), new IntPtr(endBytePos));

            long rangeLength = (endBytePos - startBytePos);
            // 2 bytes per source byte (interleaved char + style) plus the
            // optional line break (4) and NUL terminator (2). Compute in
            // 64-bit and reject a range too large for an int-sized buffer
            // rather than overflowing to a negative length.
            long bufferLength = (rangeLength * 2) + (addLineBreak ? 4 : 0) + 2;
            if (bufferLength > int.MaxValue)
                throw new ArgumentException(
                    "The styled range is too large to serialize.");
            int byteLength = (int)rangeLength;
            byte[] buffer = new byte[(int)bufferLength];
            fixed (byte* bp = buffer)
            {
                NativeMethods.Sci_TextRangeFull* tr =
                    stackalloc NativeMethods.Sci_TextRangeFull[1];
                tr->chrg.cpMin = new IntPtr(startBytePos);
                tr->chrg.cpMax = new IntPtr(endBytePos);
                tr->lpstrText = new IntPtr(bp);

                scintilla.DirectMessage(
                    NativeMethods.SCI_GETSTYLEDTEXTFULL, IntPtr.Zero,
                    new IntPtr(tr));
                byteLength *= 2;
            }

            // Add a line break?
            // We do this when this range is part of a rectangular selection.
            if (addLineBreak)
            {
                // An empty rectangular-selection row has no preceding style
                // cell (Release strips the assert above), so fall back to
                // style 0 rather than indexing buffer[-1].
                byte style = byteLength > 0 ? buffer[byteLength - 1] : (byte)0;

                buffer[byteLength++] = (byte)'\r';
                buffer[byteLength++] = style;
                buffer[byteLength++] = (byte)'\n';
                buffer[byteLength++] = style;

                // Fix-up the NULL terminator just in case
                buffer[byteLength] = 0;
                buffer[byteLength + 1] = 0;
            }

            return new ArraySegment<byte>(buffer, 0, byteLength);
        }

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Translates a Windows Forms <see cref="Keys" /> value into the
        /// corresponding Scintilla key definition, remapping the keys whose
        /// Scintilla codes differ and preserving the modifier bits.
        /// </summary>
        /// <param name="keys">
        /// The key, including any modifier bits, to translate.
        /// </param>
        /// <returns>
        /// The Scintilla key definition combining the translated key code
        /// and the original modifiers.
        /// </returns>
        public static int TranslateKeys(Keys keys)
        {
            int keyCode;

            // For some reason Scintilla uses different values for these
            // keys...
            switch (keys & Keys.KeyCode)
            {
                case Keys.Down:
                    keyCode = NativeMethods.SCK_DOWN;
                    break;
                case Keys.Up:
                    keyCode = NativeMethods.SCK_UP;
                    break;
                case Keys.Left:
                    keyCode = NativeMethods.SCK_LEFT;
                    break;
                case Keys.Right:
                    keyCode = NativeMethods.SCK_RIGHT;
                    break;
                case Keys.Home:
                    keyCode = NativeMethods.SCK_HOME;
                    break;
                case Keys.End:
                    keyCode = NativeMethods.SCK_END;
                    break;
                case Keys.Prior:
                    keyCode = NativeMethods.SCK_PRIOR;
                    break;
                case Keys.Next:
                    keyCode = NativeMethods.SCK_NEXT;
                    break;
                case Keys.Delete:
                    keyCode = NativeMethods.SCK_DELETE;
                    break;
                case Keys.Insert:
                    keyCode = NativeMethods.SCK_INSERT;
                    break;
                case Keys.Escape:
                    keyCode = NativeMethods.SCK_ESCAPE;
                    break;
                case Keys.Back:
                    keyCode = NativeMethods.SCK_BACK;
                    break;
                case Keys.Tab:
                    keyCode = NativeMethods.SCK_TAB;
                    break;
                case Keys.Return:
                    keyCode = NativeMethods.SCK_RETURN;
                    break;
                case Keys.Add:
                    keyCode = NativeMethods.SCK_ADD;
                    break;
                case Keys.Subtract:
                    keyCode = NativeMethods.SCK_SUBTRACT;
                    break;
                case Keys.Divide:
                    keyCode = NativeMethods.SCK_DIVIDE;
                    break;
                case Keys.LWin:
                    keyCode = NativeMethods.SCK_WIN;
                    break;
                case Keys.RWin:
                    keyCode = NativeMethods.SCK_RWIN;
                    break;
                case Keys.Apps:
                    keyCode = NativeMethods.SCK_MENU;
                    break;
                case Keys.Oem2:
                    keyCode = (byte)'/';
                    break;
                case Keys.Oem3:
                    keyCode = (byte)'`';
                    break;
                case Keys.Oem4:
                    keyCode = '[';
                    break;
                case Keys.Oem5:
                    keyCode = '\\';
                    break;
                case Keys.Oem6:
                    keyCode = ']';
                    break;
                default:
                    keyCode = (int)(keys & Keys.KeyCode);
                    break;
            }

            // No translation necessary for the modifiers. Just add them back
            // in.
            int keyDefinition = keyCode | (int)(keys & Keys.Modifiers);
            return keyDefinition;
        }
        #endregion Methods

        ///////////////////////////////////////////////////////////////////////

        #region Types
        /// <summary>
        /// Captures the resolved visual attributes of a single Scintilla
        /// style, used when serializing styled text to HTML or RTF.
        /// </summary>
        [ObjectId("f1c65f45-a89f-4f83-94fc-b76b87a404ae")]
        private struct StyleData
        {
            /// <summary>
            /// Indicates whether this style is actually referenced by the
            /// text being serialized.
            /// </summary>
            public bool Used;

            ///////////////////////////////////////////////////////////////////

            /// <summary>
            /// The name of the font associated with this style.
            /// </summary>
            public string FontName;

            ///////////////////////////////////////////////////////////////////

            /// <summary>
            /// The font table index assigned to this style (RTF only).
            /// </summary>
            public int FontIndex; // RTF Only

            ///////////////////////////////////////////////////////////////////

            /// <summary>
            /// The font size, in points, of this style.
            /// </summary>
            public float SizeF;

            ///////////////////////////////////////////////////////////////////

            /// <summary>
            /// The font weight of this style, where a value of 700 or
            /// greater is treated as bold.
            /// </summary>
            public int Weight;

            ///////////////////////////////////////////////////////////////////

            /// <summary>
            /// A nonzero value when this style is italic.
            /// </summary>
            public int Italic;

            ///////////////////////////////////////////////////////////////////

            /// <summary>
            /// A nonzero value when this style is underlined.
            /// </summary>
            public int Underline;

            ///////////////////////////////////////////////////////////////////

            /// <summary>
            /// The background color of this style, packed as 0x00BBGGRR.
            /// </summary>
            public int BackColor;

            ///////////////////////////////////////////////////////////////////

            /// <summary>
            /// The color table index assigned to this style's background
            /// color (RTF only).
            /// </summary>
            public int BackColorIndex; // RTF Only

            ///////////////////////////////////////////////////////////////////

            /// <summary>
            /// The foreground color of this style, packed as 0x00BBGGRR.
            /// </summary>
            public int ForeColor;

            ///////////////////////////////////////////////////////////////////

            /// <summary>
            /// The color table index assigned to this style's foreground
            /// color (RTF only).
            /// </summary>
            public int ForeColorIndex; // RTF Only

            ///////////////////////////////////////////////////////////////////

            /// <summary>
            /// The letter-case transformation applied by this style (HTML
            /// only).
            /// </summary>
            public int Case; // HTML only

            ///////////////////////////////////////////////////////////////////

            /// <summary>
            /// A zero value when text in this style is hidden (HTML only).
            /// </summary>
            public int Visible; // HTML only
        }
        #endregion Types
    }
}

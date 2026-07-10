/*
 * NativeMethods.cs --
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
using System.Runtime.InteropServices;
using System.Text;

namespace ScintillaNET
{
    /// <summary>
    /// This class provides the native interop surface for the Scintilla and
    /// Lexilla libraries: the message, notification, and style codes; the
    /// structures marshaled to and from the control; the direct-function and
    /// loader delegates; and the P/Invoke entry points.
    /// </summary>
    [ObjectId("bc514982-ce14-4ab0-995a-5a43eccd175f")]
    internal static class NativeMethods
    {
        #region Constants

        // Portability note (Windows HOST layer). user32 (window class,
        // WndProc, clipboard, the mouse-wheel IMessageFilter) and ole32
        // (RevokeDragDrop) back the WinForms Control host and are irreducibly
        // Win32: a Linux/macOS port would need a ported WinForms plus the
        // platform Scintilla (GTK/Cocoa) and would replace this whole layer.
        // kernel32's LoadLibraryEx/GetProcAddress are used ONLY on the .NET
        // Framework targets; modern targets load the native modules through
        // the cross-platform NativeLibrary seam in Scintilla.cs
        // (LoadNativeModule / GetNativeExport). The Scintilla and Lexilla
        // calls themselves are portable -- Winapi calling convention, IntPtr /
        // long widths, UTF-8 strings -- so only this host layer is
        // Windows-bound.
        /// <summary>
        /// The DLL_NAME_KERNEL32 constant.
        /// </summary>
        private const string DLL_NAME_KERNEL32 = "kernel32.dll";
        /// <summary>
        /// The DLL_NAME_OLE32 constant.
        /// </summary>
        private const string DLL_NAME_OLE32 = "ole32.dll";
        /// <summary>
        /// The DLL_NAME_USER32 constant.
        /// </summary>
        private const string DLL_NAME_USER32 = "user32.dll";

        /// <summary>
        /// The INVALID_POSITION constant.
        /// </summary>
        public const int INVALID_POSITION = -1;

        // Autocompletions
        /// <summary>
        /// The SC_AC_FILLUP constant.
        /// </summary>
        public const int SC_AC_FILLUP = 1;
        /// <summary>
        /// The SC_AC_DOUBLECLICK constant.
        /// </summary>
        public const int SC_AC_DOUBLECLICK = 2;
        /// <summary>
        /// The SC_AC_TAB constant.
        /// </summary>
        public const int SC_AC_TAB = 3;
        /// <summary>
        /// The SC_AC_NEWLINE constant.
        /// </summary>
        public const int SC_AC_NEWLINE = 4;
        /// <summary>
        /// The SC_AC_COMMAND constant.
        /// </summary>
        public const int SC_AC_COMMAND = 5;

        // Annotations
        /// <summary>
        /// The ANNOTATION_HIDDEN constant.
        /// </summary>
        public const int ANNOTATION_HIDDEN = 0;
        /// <summary>
        /// The ANNOTATION_STANDARD constant.
        /// </summary>
        public const int ANNOTATION_STANDARD = 1;
        /// <summary>
        /// The ANNOTATION_BOXED constant.
        /// </summary>
        public const int ANNOTATION_BOXED = 2;
        /// <summary>
        /// The ANNOTATION_INDENTED constant.
        /// </summary>
        public const int ANNOTATION_INDENTED = 3;

        // Clipboard formats
        /// <summary>
        /// The CF_HTML constant.
        /// </summary>
        public const string CF_HTML = "HTML Format";

        // Idle styling
        /// <summary>
        /// The SC_IDLESTYLING_NONE constant.
        /// </summary>
        public const int SC_IDLESTYLING_NONE = 0;
        /// <summary>
        /// The SC_IDLESTYLING_TOVISIBLE constant.
        /// </summary>
        public const int SC_IDLESTYLING_TOVISIBLE = 1;
        /// <summary>
        /// The SC_IDLESTYLING_AFTERVISIBLE constant.
        /// </summary>
        public const int SC_IDLESTYLING_AFTERVISIBLE = 2;
        /// <summary>
        /// The SC_IDLESTYLING_ALL constant.
        /// </summary>
        public const int SC_IDLESTYLING_ALL = 3;

        // Indentation
        /// <summary>
        /// The SC_IV_NONE constant.
        /// </summary>
        public const int SC_IV_NONE = 0;
        /// <summary>
        /// The SC_IV_REAL constant.
        /// </summary>
        public const int SC_IV_REAL = 1;
        /// <summary>
        /// The SC_IV_LOOKFORWARD constant.
        /// </summary>
        public const int SC_IV_LOOKFORWARD = 2;
        /// <summary>
        /// The SC_IV_LOOKBOTH constant.
        /// </summary>
        public const int SC_IV_LOOKBOTH = 3;

        // Keys
        /// <summary>
        /// The SCMOD_NORM constant.
        /// </summary>
        public const int SCMOD_NORM = 0;
        /// <summary>
        /// The SCMOD_SHIFT constant.
        /// </summary>
        public const int SCMOD_SHIFT = 1;
        /// <summary>
        /// The SCMOD_CTRL constant.
        /// </summary>
        public const int SCMOD_CTRL = 2;
        /// <summary>
        /// The SCMOD_ALT constant.
        /// </summary>
        public const int SCMOD_ALT = 4;
        /// <summary>
        /// The SCMOD_SUPER constant.
        /// </summary>
        public const int SCMOD_SUPER = 8;
        /// <summary>
        /// The SCMOD_META constant.
        /// </summary>
        public const int SCMOD_META = 16;

        /// <summary>
        /// The SCI_NORM constant.
        /// </summary>
        public const int SCI_NORM = 0;
        /// <summary>
        /// The SCI_SHIFT constant.
        /// </summary>
        public const int SCI_SHIFT = SCMOD_SHIFT;
        /// <summary>
        /// The SCI_CTRL constant.
        /// </summary>
        public const int SCI_CTRL = SCMOD_CTRL;
        /// <summary>
        /// The SCI_ALT constant.
        /// </summary>
        public const int SCI_ALT = SCMOD_ALT;
        /// <summary>
        /// The SCI_META constant.
        /// </summary>
        public const int SCI_META = SCMOD_META;
        /// <summary>
        /// The SCI_CSHIFT constant.
        /// </summary>
        public const int SCI_CSHIFT = (SCI_CTRL | SCI_SHIFT);
        /// <summary>
        /// The SCI_ASHIFT constant.
        /// </summary>
        public const int SCI_ASHIFT = (SCI_ALT | SCI_SHIFT);

        // Caret styles
        /// <summary>
        /// The CARETSTYLE_INVISIBLE constant.
        /// </summary>
        public const int CARETSTYLE_INVISIBLE = 0;
        /// <summary>
        /// The CARETSTYLE_LINE constant.
        /// </summary>
        public const int CARETSTYLE_LINE = 1;
        /// <summary>
        /// The CARETSTYLE_BLOCK constant.
        /// </summary>
        public const int CARETSTYLE_BLOCK = 2;

        // Line edges
        /// <summary>
        /// The EDGE_NONE constant.
        /// </summary>
        public const int EDGE_NONE = 0;
        /// <summary>
        /// The EDGE_LINE constant.
        /// </summary>
        public const int EDGE_LINE = 1;
        /// <summary>
        /// The EDGE_BACKGROUND constant.
        /// </summary>
        public const int EDGE_BACKGROUND = 2;
        /// <summary>
        /// The EDGE_MULTILINE constant.
        /// </summary>
        public const int EDGE_MULTILINE = 3;

        // Message-only window
        /// <summary>
        /// The HWND_MESSAGE constant.
        /// </summary>
        public const int HWND_MESSAGE = (-3);

        // Indicators
        /// <summary>
        /// Indicator style enumeration and some constants.
        /// </summary>
        public const int INDIC_PLAIN = 0;
        /// <summary>
        /// Indicator style enumeration and some constants.
        /// </summary>
        public const int INDIC_SQUIGGLE = 1;
        /// <summary>
        /// Indicator style enumeration and some constants.
        /// </summary>
        public const int INDIC_TT = 2;
        /// <summary>
        /// Indicator style enumeration and some constants.
        /// </summary>
        public const int INDIC_DIAGONAL = 3;
        /// <summary>
        /// Indicator style enumeration and some constants.
        /// </summary>
        public const int INDIC_STRIKE = 4;
        /// <summary>
        /// Indicator style enumeration and some constants.
        /// </summary>
        public const int INDIC_HIDDEN = 5;
        /// <summary>
        /// Indicator style enumeration and some constants.
        /// </summary>
        public const int INDIC_BOX = 6;
        /// <summary>
        /// Indicator style enumeration and some constants.
        /// </summary>
        public const int INDIC_ROUNDBOX = 7;
        /// <summary>
        /// Indicator style enumeration and some constants.
        /// </summary>
        public const int INDIC_STRAIGHTBOX = 8;
        /// <summary>
        /// Indicator style enumeration and some constants.
        /// </summary>
        public const int INDIC_DASH = 9;
        /// <summary>
        /// Indicator style enumeration and some constants.
        /// </summary>
        public const int INDIC_DOTS = 10;
        /// <summary>
        /// Indicator style enumeration and some constants.
        /// </summary>
        public const int INDIC_SQUIGGLELOW = 11;
        /// <summary>
        /// Indicator style enumeration and some constants.
        /// </summary>
        public const int INDIC_DOTBOX = 12;
        /// <summary>
        /// Indicator style enumeration and some constants.
        /// </summary>
        public const int INDIC_SQUIGGLEPIXMAP = 13;
        /// <summary>
        /// Indicator style enumeration and some constants.
        /// </summary>
        public const int INDIC_COMPOSITIONTHICK = 14;
        /// <summary>
        /// Indicator style enumeration and some constants.
        /// </summary>
        public const int INDIC_COMPOSITIONTHIN = 15;
        /// <summary>
        /// Indicator style enumeration and some constants.
        /// </summary>
        public const int INDIC_FULLBOX = 16;
        /// <summary>
        /// Indicator style enumeration and some constants.
        /// </summary>
        public const int INDIC_TEXTFORE = 17;
        /// <summary>
        /// Indicator style enumeration and some constants.
        /// </summary>
        public const int INDIC_POINT = 18;
        /// <summary>
        /// Indicator style enumeration and some constants.
        /// </summary>
        public const int INDIC_POINTCHARACTER = 19;
        /// <summary>
        /// Indicator style enumeration and some constants.
        /// </summary>
        /// <remarks>
        /// Scintilla 3.7.2: indicators 0-35 (32-35 reserved for IME).
        /// </remarks>
        public const int INDIC_MAX = 35;
        /// <summary>
        /// INDIC_CONTAINER, INDIC_IME, INDIC_IME_MAX, and INDIC_MAX are
        /// indicator numbers, not IndicatorStyles so should not really be in
        /// the INDIC_ enumeration. They are redeclared in IndicatorNumbers
        /// INDICATOR_.
        /// </summary>
        public const int INDIC_CONTAINER = 8;

        // Phases
        /// <summary>
        /// The SC_PHASES_ONE constant.
        /// </summary>
        public const int SC_PHASES_ONE = 0;
        /// <summary>
        /// The SC_PHASES_TWO constant.
        /// </summary>
        public const int SC_PHASES_TWO = 1;
        /// <summary>
        /// The SC_PHASES_MULTIPLE constant.
        /// </summary>
        public const int SC_PHASES_MULTIPLE = 2;

        // Indicator flags
        /// <summary>
        /// The SC_INDICFLAG_VALUEFORE constant.
        /// </summary>
        public const int SC_INDICFLAG_VALUEFORE = 1;
        /// <summary>
        /// The SC_INDICVALUEBIT constant.
        /// </summary>
        public const int SC_INDICVALUEBIT = 0x1000000;
        /// <summary>
        /// The SC_INDICVALUEMASK constant.
        /// </summary>
        public const int SC_INDICVALUEMASK = 0xFFFFFF;

        // public const int INDIC0_MASK = 0x20;
        // public const int INDIC1_MASK = 0x40;
        // public const int INDIC2_MASK = 0x80;
        // public const int INDICS_MASK = 0xE0;

        /// <summary>
        /// Maximum value of keywordSet parameter of SetKeyWords.
        /// </summary>
        public const int KEYWORDSET_MAX = 8;

        // Alpha ranges
        /// <summary>
        /// The SC_ALPHA_TRANSPARENT constant.
        /// </summary>
        public const int SC_ALPHA_TRANSPARENT = 0;
        /// <summary>
        /// The SC_ALPHA_OPAQUE constant.
        /// </summary>
        public const int SC_ALPHA_OPAQUE = 255;
        /// <summary>
        /// The SC_ALPHA_NOALPHA constant.
        /// </summary>
        public const int SC_ALPHA_NOALPHA = 256;

        // Automatic folding
        /// <summary>
        /// The SC_AUTOMATICFOLD_SHOW constant.
        /// </summary>
        public const int SC_AUTOMATICFOLD_SHOW = 0x0001;
        /// <summary>
        /// The SC_AUTOMATICFOLD_CLICK constant.
        /// </summary>
        public const int SC_AUTOMATICFOLD_CLICK = 0x0002;
        /// <summary>
        /// The SC_AUTOMATICFOLD_CHANGE constant.
        /// </summary>
        public const int SC_AUTOMATICFOLD_CHANGE = 0x0004;

        // Caret sticky behavior
        /// <summary>
        /// The SC_CARETSTICKY_OFF constant.
        /// </summary>
        public const int SC_CARETSTICKY_OFF = 0;
        /// <summary>
        /// The SC_CARETSTICKY_ON constant.
        /// </summary>
        public const int SC_CARETSTICKY_ON = 1;
        /// <summary>
        /// The SC_CARETSTICKY_WHITESPACE constant.
        /// </summary>
        public const int SC_CARETSTICKY_WHITESPACE = 2;

        // Encodings
        /// <summary>
        /// The SC_CP_UTF8 value can be used to enter Unicode mode. This is the
        /// same value as CP_UTF8 in Windows.
        /// </summary>
        public const int SC_CP_UTF8 = 65001;

        // Cursors
        /// <summary>
        /// The SC_CURSORNORMAL constant.
        /// </summary>
        public const int SC_CURSORNORMAL = -1;
        /// <summary>
        /// The SC_CURSORARROW constant.
        /// </summary>
        public const int SC_CURSORARROW = 2;
        /// <summary>
        /// The SC_CURSORWAIT constant.
        /// </summary>
        public const int SC_CURSORWAIT = 4;
        /// <summary>
        /// The SC_CURSORREVERSEARROW constant.
        /// </summary>
        public const int SC_CURSORREVERSEARROW = 7;

        // Font quality
        /// <summary>
        /// The SC_EFF_QUALITY_DEFAULT constant.
        /// </summary>
        public const int SC_EFF_QUALITY_DEFAULT = 0;
        /// <summary>
        /// The SC_EFF_QUALITY_NON_ANTIALIASED constant.
        /// </summary>
        public const int SC_EFF_QUALITY_NON_ANTIALIASED = 1;
        /// <summary>
        /// The SC_EFF_QUALITY_ANTIALIASED constant.
        /// </summary>
        public const int SC_EFF_QUALITY_ANTIALIASED = 2;
        /// <summary>
        /// The SC_EFF_QUALITY_LCD_OPTIMIZED constant.
        /// </summary>
        public const int SC_EFF_QUALITY_LCD_OPTIMIZED = 3;

        // End-of-line
        /// <summary>
        /// The SC_EOL_CRLF constant.
        /// </summary>
        public const int SC_EOL_CRLF = 0;
        /// <summary>
        /// The SC_EOL_CR constant.
        /// </summary>
        public const int SC_EOL_CR = 1;
        /// <summary>
        /// The SC_EOL_LF constant.
        /// </summary>
        public const int SC_EOL_LF = 2;

        // Fold action
        /// <summary>
        /// The SC_FOLDACTION_CONTRACT constant.
        /// </summary>
        public const int SC_FOLDACTION_CONTRACT = 0;
        /// <summary>
        /// The SC_FOLDACTION_EXPAND constant.
        /// </summary>
        public const int SC_FOLDACTION_EXPAND = 1;
        /// <summary>
        /// The SC_FOLDACTION_TOGGLE constant.
        /// </summary>
        public const int SC_FOLDACTION_TOGGLE = 2;

        // Fold level
        /// <summary>
        /// The SC_FOLDLEVELBASE constant.
        /// </summary>
        public const int SC_FOLDLEVELBASE = 0x400;
        /// <summary>
        /// The SC_FOLDLEVELWHITEFLAG constant.
        /// </summary>
        public const int SC_FOLDLEVELWHITEFLAG = 0x1000;
        /// <summary>
        /// The SC_FOLDLEVELHEADERFLAG constant.
        /// </summary>
        public const int SC_FOLDLEVELHEADERFLAG = 0x2000;
        /// <summary>
        /// The SC_FOLDLEVELNUMBERMASK constant.
        /// </summary>
        public const int SC_FOLDLEVELNUMBERMASK = 0x0FFF;

        // Fold flags
        /// <summary>
        /// The SC_FOLDFLAG_LINEBEFORE_EXPANDED constant.
        /// </summary>
        public const int SC_FOLDFLAG_LINEBEFORE_EXPANDED = 0x0002;
        /// <summary>
        /// The SC_FOLDFLAG_LINEBEFORE_CONTRACTED constant.
        /// </summary>
        public const int SC_FOLDFLAG_LINEBEFORE_CONTRACTED = 0x0004;
        /// <summary>
        /// The SC_FOLDFLAG_LINEAFTER_EXPANDED constant.
        /// </summary>
        public const int SC_FOLDFLAG_LINEAFTER_EXPANDED = 0x0008;
        /// <summary>
        /// The SC_FOLDFLAG_LINEAFTER_CONTRACTED constant.
        /// </summary>
        public const int SC_FOLDFLAG_LINEAFTER_CONTRACTED = 0x0010;
        /// <summary>
        /// The SC_FOLDFLAG_LEVELNUMBERS constant.
        /// </summary>
        public const int SC_FOLDFLAG_LEVELNUMBERS = 0x0040;
        /// <summary>
        /// The SC_FOLDFLAG_LINESTATE constant.
        /// </summary>
        public const int SC_FOLDFLAG_LINESTATE = 0x0080;

        // Fold display text
        /// <summary>
        /// The SC_FOLDDISPLAYTEXT_HIDDEN constant.
        /// </summary>
        public const int SC_FOLDDISPLAYTEXT_HIDDEN = 0;
        /// <summary>
        /// The SC_FOLDDISPLAYTEXT_STANDARD constant.
        /// </summary>
        public const int SC_FOLDDISPLAYTEXT_STANDARD = 1;
        /// <summary>
        /// The SC_FOLDDISPLAYTEXT_BOXED constant.
        /// </summary>
        public const int SC_FOLDDISPLAYTEXT_BOXED = 2;

        // Line end type
        /// <summary>
        /// Line end types which may be used in addition to LF, CR, and CRLF
        /// SC_LINE_END_TYPE_UNICODE includes U+2028 Line Separator, U+2029
        /// Paragraph Separator, and U+0085 Next Line.
        /// </summary>
        public const int SC_LINE_END_TYPE_DEFAULT = 0;
        /// <summary>
        /// Line end types which may be used in addition to LF, CR, and CRLF
        /// SC_LINE_END_TYPE_UNICODE includes U+2028 Line Separator, U+2029
        /// Paragraph Separator, and U+0085 Next Line.
        /// </summary>
        public const int SC_LINE_END_TYPE_UNICODE = 1;

        // Margins
        /// <summary>
        /// The SC_MAX_MARGIN constant.
        /// </summary>
        public const int SC_MAX_MARGIN = 4;

        /// <summary>
        /// The SC_MARGIN_SYMBOL constant.
        /// </summary>
        public const int SC_MARGIN_SYMBOL = 0;
        /// <summary>
        /// The SC_MARGIN_NUMBER constant.
        /// </summary>
        public const int SC_MARGIN_NUMBER = 1;
        /// <summary>
        /// The SC_MARGIN_BACK constant.
        /// </summary>
        public const int SC_MARGIN_BACK = 2;
        /// <summary>
        /// The SC_MARGIN_FORE constant.
        /// </summary>
        public const int SC_MARGIN_FORE = 3;
        /// <summary>
        /// The SC_MARGIN_TEXT constant.
        /// </summary>
        public const int SC_MARGIN_TEXT = 4;
        /// <summary>
        /// The SC_MARGIN_RTEXT constant.
        /// </summary>
        public const int SC_MARGIN_RTEXT = 5;
        /// <summary>
        /// The SC_MARGIN_COLOUR constant.
        /// </summary>
        public const int SC_MARGIN_COLOUR = 6;

        /// <summary>
        /// The SC_MARGINOPTION_NONE constant.
        /// </summary>
        public const int SC_MARGINOPTION_NONE = 0;
        /// <summary>
        /// The SC_MARGINOPTION_SUBLINESELECT constant.
        /// </summary>
        public const int SC_MARGINOPTION_SUBLINESELECT = 1;

        // Markers
        /// <summary>
        /// The MARKER_MAX constant.
        /// </summary>
        public const int MARKER_MAX = 31;
        /// <summary>
        /// The SC_MARK_CIRCLE constant.
        /// </summary>
        public const int SC_MARK_CIRCLE = 0;
        /// <summary>
        /// The SC_MARK_ROUNDRECT constant.
        /// </summary>
        public const int SC_MARK_ROUNDRECT = 1;
        /// <summary>
        /// The SC_MARK_ARROW constant.
        /// </summary>
        public const int SC_MARK_ARROW = 2;
        /// <summary>
        /// The SC_MARK_SMALLRECT constant.
        /// </summary>
        public const int SC_MARK_SMALLRECT = 3;
        /// <summary>
        /// The SC_MARK_SHORTARROW constant.
        /// </summary>
        public const int SC_MARK_SHORTARROW = 4;
        /// <summary>
        /// The SC_MARK_EMPTY constant.
        /// </summary>
        public const int SC_MARK_EMPTY = 5;
        /// <summary>
        /// The SC_MARK_ARROWDOWN constant.
        /// </summary>
        public const int SC_MARK_ARROWDOWN = 6;
        /// <summary>
        /// The SC_MARK_MINUS constant.
        /// </summary>
        public const int SC_MARK_MINUS = 7;
        /// <summary>
        /// The SC_MARK_PLUS constant.
        /// </summary>
        public const int SC_MARK_PLUS = 8;
        /// <summary>
        /// Shapes used for outlining column.
        /// </summary>
        public const int SC_MARK_VLINE = 9;
        /// <summary>
        /// The SC_MARK_LCORNER constant.
        /// </summary>
        public const int SC_MARK_LCORNER = 10;
        /// <summary>
        /// The SC_MARK_TCORNER constant.
        /// </summary>
        public const int SC_MARK_TCORNER = 11;
        /// <summary>
        /// The SC_MARK_BOXPLUS constant.
        /// </summary>
        public const int SC_MARK_BOXPLUS = 12;
        /// <summary>
        /// The SC_MARK_BOXPLUSCONNECTED constant.
        /// </summary>
        public const int SC_MARK_BOXPLUSCONNECTED = 13;
        /// <summary>
        /// The SC_MARK_BOXMINUS constant.
        /// </summary>
        public const int SC_MARK_BOXMINUS = 14;
        /// <summary>
        /// The SC_MARK_BOXMINUSCONNECTED constant.
        /// </summary>
        public const int SC_MARK_BOXMINUSCONNECTED = 15;
        /// <summary>
        /// The SC_MARK_LCORNERCURVE constant.
        /// </summary>
        public const int SC_MARK_LCORNERCURVE = 16;
        /// <summary>
        /// The SC_MARK_TCORNERCURVE constant.
        /// </summary>
        public const int SC_MARK_TCORNERCURVE = 17;
        /// <summary>
        /// The SC_MARK_CIRCLEPLUS constant.
        /// </summary>
        public const int SC_MARK_CIRCLEPLUS = 18;
        /// <summary>
        /// The SC_MARK_CIRCLEPLUSCONNECTED constant.
        /// </summary>
        public const int SC_MARK_CIRCLEPLUSCONNECTED = 19;
        /// <summary>
        /// The SC_MARK_CIRCLEMINUS constant.
        /// </summary>
        public const int SC_MARK_CIRCLEMINUS = 20;
        /// <summary>
        /// The SC_MARK_CIRCLEMINUSCONNECTED constant.
        /// </summary>
        public const int SC_MARK_CIRCLEMINUSCONNECTED = 21;
        /// <summary>
        /// Invisible mark that only sets the line background colour.
        /// </summary>
        public const int SC_MARK_BACKGROUND = 22;
        /// <summary>
        /// The SC_MARK_DOTDOTDOT constant.
        /// </summary>
        public const int SC_MARK_DOTDOTDOT = 23;
        /// <summary>
        /// The SC_MARK_ARROWS constant.
        /// </summary>
        public const int SC_MARK_ARROWS = 24;
        /// <summary>
        /// The SC_MARK_PIXMAP constant.
        /// </summary>
        public const int SC_MARK_PIXMAP = 25;
        /// <summary>
        /// The SC_MARK_FULLRECT constant.
        /// </summary>
        public const int SC_MARK_FULLRECT = 26;
        /// <summary>
        /// The SC_MARK_LEFTRECT constant.
        /// </summary>
        public const int SC_MARK_LEFTRECT = 27;
        /// <summary>
        /// The SC_MARK_AVAILABLE constant.
        /// </summary>
        public const int SC_MARK_AVAILABLE = 28;
        /// <summary>
        /// The SC_MARK_UNDERLINE constant.
        /// </summary>
        public const int SC_MARK_UNDERLINE = 29;
        /// <summary>
        /// The SC_MARK_RGBAIMAGE constant.
        /// </summary>
        public const int SC_MARK_RGBAIMAGE = 30;
        /// <summary>
        /// The SC_MARK_BOOKMARK constant.
        /// </summary>
        public const int SC_MARK_BOOKMARK = 31;
        /// <summary>
        /// The SC_MARK_CHARACTER constant.
        /// </summary>
        public const int SC_MARK_CHARACTER = 10000;
        /// <summary>
        /// The SC_MARKNUM_FOLDEREND constant.
        /// </summary>
        public const int SC_MARKNUM_FOLDEREND = 25;
        /// <summary>
        /// The SC_MARKNUM_FOLDEROPENMID constant.
        /// </summary>
        public const int SC_MARKNUM_FOLDEROPENMID = 26;
        /// <summary>
        /// The SC_MARKNUM_FOLDERMIDTAIL constant.
        /// </summary>
        public const int SC_MARKNUM_FOLDERMIDTAIL = 27;
        /// <summary>
        /// The SC_MARKNUM_FOLDERTAIL constant.
        /// </summary>
        public const int SC_MARKNUM_FOLDERTAIL = 28;
        /// <summary>
        /// The SC_MARKNUM_FOLDERSUB constant.
        /// </summary>
        public const int SC_MARKNUM_FOLDERSUB = 29;
        /// <summary>
        /// The SC_MARKNUM_FOLDER constant.
        /// </summary>
        public const int SC_MARKNUM_FOLDER = 30;
        /// <summary>
        /// The SC_MARKNUM_FOLDEROPEN constant.
        /// </summary>
        public const int SC_MARKNUM_FOLDEROPEN = 31;
        /// <summary>
        /// SC_MASK_FOLDERS doesn't go in an enumeration as larger than max
        /// 32-bit positive integer.
        /// </summary>
        public const uint SC_MASK_FOLDERS = 0xFE000000;

        /// <summary>
        /// The SC_MULTIPASTE_ONCE constant.
        /// </summary>
        public const int SC_MULTIPASTE_ONCE = 0;
        /// <summary>
        /// The SC_MULTIPASTE_EACH constant.
        /// </summary>
        public const int SC_MULTIPASTE_EACH = 1;

        /// <summary>
        /// The SC_ORDER_PRESORTED constant.
        /// </summary>
        public const int SC_ORDER_PRESORTED = 0;
        /// <summary>
        /// The SC_ORDER_PERFORMSORT constant.
        /// </summary>
        public const int SC_ORDER_PERFORMSORT = 1;
        /// <summary>
        /// The SC_ORDER_CUSTOM constant.
        /// </summary>
        public const int SC_ORDER_CUSTOM = 2;

        // Update notification reasons
        /// <summary>
        /// The SC_UPDATE_CONTENT constant.
        /// </summary>
        public const int SC_UPDATE_CONTENT = 0x01;
        /// <summary>
        /// The SC_UPDATE_SELECTION constant.
        /// </summary>
        public const int SC_UPDATE_SELECTION = 0x02;
        /// <summary>
        /// The SC_UPDATE_V_SCROLL constant.
        /// </summary>
        public const int SC_UPDATE_V_SCROLL = 0x04;
        /// <summary>
        /// The SC_UPDATE_H_SCROLL constant.
        /// </summary>
        public const int SC_UPDATE_H_SCROLL = 0x08;

        // Modified notification types
        /// <summary>
        /// Notifications Type of modification and the action which caused the
        /// modification. These are defined as a bit mask to make it easy to
        /// specify which notifications are wanted. One bit is set from each of
        /// SC_MOD_* and SC_PERFORMED_*.
        /// </summary>
        public const int SC_MOD_INSERTTEXT = 0x1;
        /// <summary>
        /// Notifications Type of modification and the action which caused the
        /// modification. These are defined as a bit mask to make it easy to
        /// specify which notifications are wanted. One bit is set from each of
        /// SC_MOD_* and SC_PERFORMED_*.
        /// </summary>
        public const int SC_MOD_DELETETEXT = 0x2;
        /// <summary>
        /// Notifications Type of modification and the action which caused the
        /// modification. These are defined as a bit mask to make it easy to
        /// specify which notifications are wanted. One bit is set from each of
        /// SC_MOD_* and SC_PERFORMED_*.
        /// </summary>
        public const int SC_MOD_BEFOREINSERT = 0x400;
        /// <summary>
        /// Notifications Type of modification and the action which caused the
        /// modification. These are defined as a bit mask to make it easy to
        /// specify which notifications are wanted. One bit is set from each of
        /// SC_MOD_* and SC_PERFORMED_*.
        /// </summary>
        public const int SC_MOD_BEFOREDELETE = 0x800;
        /// <summary>
        /// Notifications Type of modification and the action which caused the
        /// modification. These are defined as a bit mask to make it easy to
        /// specify which notifications are wanted. One bit is set from each of
        /// SC_MOD_* and SC_PERFORMED_*.
        /// </summary>
        public const int SC_MOD_CHANGEANNOTATION = 0x20000;
        /// <summary>
        /// Notifications Type of modification and the action which caused the
        /// modification. These are defined as a bit mask to make it easy to
        /// specify which notifications are wanted. One bit is set from each of
        /// SC_MOD_* and SC_PERFORMED_*.
        /// </summary>
        public const int SC_MOD_INSERTCHECK = 0x100000;

        // Modified flags
        /// <summary>
        /// Notifications Type of modification and the action which caused the
        /// modification. These are defined as a bit mask to make it easy to
        /// specify which notifications are wanted. One bit is set from each of
        /// SC_MOD_* and SC_PERFORMED_*.
        /// </summary>
        public const int SC_PERFORMED_USER = 0x10;
        /// <summary>
        /// Notifications Type of modification and the action which caused the
        /// modification. These are defined as a bit mask to make it easy to
        /// specify which notifications are wanted. One bit is set from each of
        /// SC_MOD_* and SC_PERFORMED_*.
        /// </summary>
        public const int SC_PERFORMED_UNDO = 0x20;
        /// <summary>
        /// Notifications Type of modification and the action which caused the
        /// modification. These are defined as a bit mask to make it easy to
        /// specify which notifications are wanted. One bit is set from each of
        /// SC_MOD_* and SC_PERFORMED_*.
        /// </summary>
        public const int SC_PERFORMED_REDO = 0x40;

        // Status codes
        /// <summary>
        /// The SC_STATUS_OK constant.
        /// </summary>
        public const int SC_STATUS_OK = 0;
        /// <summary>
        /// The SC_STATUS_FAILURE constant.
        /// </summary>
        public const int SC_STATUS_FAILURE = 1;
        /// <summary>
        /// The SC_STATUS_BADALLOC constant.
        /// </summary>
        public const int SC_STATUS_BADALLOC = 2;
        /// <summary>
        /// The SC_STATUS_WARN_START constant.
        /// </summary>
        public const int SC_STATUS_WARN_START = 1000;
        /// <summary>
        /// The SC_STATUS_WARN_REGEX constant.
        /// </summary>
        public const int SC_STATUS_WARN_REGEX = 1001;

        // Dwell
        /// <summary>
        /// The SC_TIME_FOREVER constant.
        /// </summary>
        public const int SC_TIME_FOREVER = 10000000;

        // Property types
        /// <summary>
        /// The SC_TYPE_BOOLEAN constant.
        /// </summary>
        public const int SC_TYPE_BOOLEAN = 0;
        /// <summary>
        /// The SC_TYPE_INTEGER constant.
        /// </summary>
        public const int SC_TYPE_INTEGER = 1;
        /// <summary>
        /// The SC_TYPE_STRING constant.
        /// </summary>
        public const int SC_TYPE_STRING = 2;

        // Search flags
        /// <summary>
        /// The SCFIND_WHOLEWORD constant.
        /// </summary>
        public const int SCFIND_WHOLEWORD = 0x2;
        /// <summary>
        /// The SCFIND_MATCHCASE constant.
        /// </summary>
        public const int SCFIND_MATCHCASE = 0x4;
        /// <summary>
        /// The SCFIND_WORDSTART constant.
        /// </summary>
        public const int SCFIND_WORDSTART = 0x00100000;
        /// <summary>
        /// The SCFIND_REGEXP constant.
        /// </summary>
        public const int SCFIND_REGEXP = 0x00200000;
        /// <summary>
        /// The SCFIND_POSIX constant.
        /// </summary>
        public const int SCFIND_POSIX = 0x00400000;
        /// <summary>
        /// The SCFIND_CXX11REGEX constant.
        /// </summary>
        public const int SCFIND_CXX11REGEX = 0x00800000;

        // Functions
        /// <summary>
        /// Define start of Scintilla messages to be greater than all Windows
        /// edit (EM_*) messages as many EM_ messages can be used although that
        /// use is deprecated.
        /// </summary>
        public const int SCI_START = 2000;
        /// <summary>
        /// The SCI_OPTIONAL_START constant.
        /// </summary>
        public const int SCI_OPTIONAL_START = 3000;
        /// <summary>
        /// The SCI_LEXER_START constant.
        /// </summary>
        public const int SCI_LEXER_START = 4000;
        /// <summary>
        /// Add text to the document at current position.
        /// </summary>
        public const int SCI_ADDTEXT = 2001;
        /// <summary>
        /// Add array of cells to document.
        /// </summary>
        public const int SCI_ADDSTYLEDTEXT = 2002;
        /// <summary>
        /// Insert string at a position.
        /// </summary>
        public const int SCI_INSERTTEXT = 2003;
        /// <summary>
        /// Change the text that is being inserted in response to
        /// SC_MOD_INSERTCHECK.
        /// </summary>
        public const int SCI_CHANGEINSERTION = 2672;
        /// <summary>
        /// Delete all text in the document.
        /// </summary>
        public const int SCI_CLEARALL = 2004;
        /// <summary>
        /// Delete a range of text in the document.
        /// </summary>
        public const int SCI_DELETERANGE = 2645;
        /// <summary>
        /// Set all style bytes to 0, remove all folding information.
        /// </summary>
        public const int SCI_CLEARDOCUMENTSTYLE = 2005;
        /// <summary>
        /// Returns the number of bytes in the document.
        /// </summary>
        public const int SCI_GETLENGTH = 2006;
        /// <summary>
        /// Returns the character byte at the position.
        /// </summary>
        public const int SCI_GETCHARAT = 2007;
        /// <summary>
        /// Returns the position of the caret.
        /// </summary>
        public const int SCI_GETCURRENTPOS = 2008;
        /// <summary>
        /// Returns the position of the opposite end of the selection to the
        /// caret.
        /// </summary>
        public const int SCI_GETANCHOR = 2009;
        /// <summary>
        /// Returns the style byte at the position.
        /// </summary>
        public const int SCI_GETSTYLEAT = 2010;
        /// <summary>
        /// Redoes the next action on the undo history.
        /// </summary>
        public const int SCI_REDO = 2011;
        /// <summary>
        /// Choose between collecting actions into the undo history and
        /// discarding them.
        /// </summary>
        public const int SCI_SETUNDOCOLLECTION = 2012;
        /// <summary>
        /// Select all the text in the document.
        /// </summary>
        public const int SCI_SELECTALL = 2013;
        /// <summary>
        /// Remember the current position in the undo history as the position
        /// at which the document was saved.
        /// </summary>
        public const int SCI_SETSAVEPOINT = 2014;
        /// <summary>
        /// Retrieve a buffer of cells. Returns the number of bytes in the
        /// buffer not including terminating NULs.
        /// </summary>
        public const int SCI_GETSTYLEDTEXT = 2015;
        /// <summary>
        /// Retrieve a buffer of cells that can be past 2GB. Returns the number
        /// of bytes in the buffer not including terminating NULs.
        /// </summary>
        public const int SCI_GETSTYLEDTEXTFULL = 2778;
        /// <summary>
        /// Are there any redoable actions in the undo history?.
        /// </summary>
        public const int SCI_CANREDO = 2016;
        /// <summary>
        /// Retrieve the line number at which a particular marker is located.
        /// </summary>
        public const int SCI_MARKERLINEFROMHANDLE = 2017;
        /// <summary>
        /// Delete a marker.
        /// </summary>
        public const int SCI_MARKERDELETEHANDLE = 2018;
        /// <summary>
        /// Is undo history being collected?.
        /// </summary>
        public const int SCI_GETUNDOCOLLECTION = 2019;
        /// <summary>
        /// Are white space characters currently visible? Returns one of SCWS_*
        /// constants.
        /// </summary>
        public const int SCI_GETVIEWWS = 2020;
        /// <summary>
        /// Make white space characters invisible, always visible or visible
        /// outside indentation.
        /// </summary>
        public const int SCI_SETVIEWWS = 2021;
        /// <summary>
        /// Find the position from a point within the window.
        /// </summary>
        public const int SCI_POSITIONFROMPOINT = 2022;
        /// <summary>
        /// Find the position from a point within the window but return
        /// INVALID_POSITION if not close to text.
        /// </summary>
        public const int SCI_POSITIONFROMPOINTCLOSE = 2023;
        /// <summary>
        /// Set caret to start of a line and ensure it is visible.
        /// </summary>
        public const int SCI_GOTOLINE = 2024;
        /// <summary>
        /// Set caret to a position and ensure it is visible.
        /// </summary>
        public const int SCI_GOTOPOS = 2025;
        /// <summary>
        /// Set the selection anchor to a position. The anchor is the opposite
        /// end of the selection from the caret.
        /// </summary>
        public const int SCI_SETANCHOR = 2026;
        /// <summary>
        /// Retrieve the text of the line containing the caret. Returns the
        /// index of the caret on the line. Result is NUL-terminated.
        /// </summary>
        public const int SCI_GETCURLINE = 2027;
        /// <summary>
        /// Retrieve the position of the last correctly styled character.
        /// </summary>
        public const int SCI_GETENDSTYLED = 2028;
        /// <summary>
        /// Convert all line endings in the document to one mode.
        /// </summary>
        public const int SCI_CONVERTEOLS = 2029;
        /// <summary>
        /// Retrieve the current end of line mode - one of CRLF, CR, or LF.
        /// </summary>
        public const int SCI_GETEOLMODE = 2030;
        /// <summary>
        /// Set the current end of line mode.
        /// </summary>
        public const int SCI_SETEOLMODE = 2031;
        /// <summary>
        /// Set the current styling position to start. The unused parameter is
        /// no longer used and should be set to 0.
        /// </summary>
        public const int SCI_STARTSTYLING = 2032;
        /// <summary>
        /// Change style from current styling position for length characters to
        /// a style and move the current styling position to after this newly
        /// styled segment.
        /// </summary>
        public const int SCI_SETSTYLING = 2033;
        /// <summary>
        /// Is drawing done first into a buffer or direct to the screen?.
        /// </summary>
        public const int SCI_GETBUFFEREDDRAW = 2034;
        /// <summary>
        /// If drawing is buffered then each line of text is drawn into a
        /// bitmap buffer before drawing it to the screen to avoid flicker.
        /// </summary>
        public const int SCI_SETBUFFEREDDRAW = 2035;
        /// <summary>
        /// Change the visible size of a tab to be a multiple of the width of a
        /// space character.
        /// </summary>
        public const int SCI_SETTABWIDTH = 2036;
        /// <summary>
        /// Retrieve the visible size of a tab.
        /// </summary>
        public const int SCI_GETTABWIDTH = 2121;
        /// <summary>
        /// Clear explicit tabstops on a line.
        /// </summary>
        public const int SCI_CLEARTABSTOPS = 2675;
        /// <summary>
        /// Add an explicit tab stop for a line.
        /// </summary>
        public const int SCI_ADDTABSTOP = 2676;
        /// <summary>
        /// Find the next explicit tab stop position on a line after a
        /// position.
        /// </summary>
        public const int SCI_GETNEXTTABSTOP = 2677;
        /// <summary>
        /// Set the code page used to interpret the bytes of the document as
        /// characters. The SC_CP_UTF8 value can be used to enter Unicode mode.
        /// </summary>
        public const int SCI_SETCODEPAGE = 2037;
        /// <summary>
        /// Set the symbol used for a particular marker number.
        /// </summary>
        public const int SCI_MARKERDEFINE = 2040;
        /// <summary>
        /// Set the foreground colour used for a particular marker number.
        /// </summary>
        public const int SCI_MARKERSETFORE = 2041;
        /// <summary>
        /// Set the background colour used for a particular marker number.
        /// </summary>
        public const int SCI_MARKERSETBACK = 2042;
        /// <summary>
        /// Set the background colour used for a particular marker number when
        /// its folding block is selected.
        /// </summary>
        public const int SCI_MARKERSETBACKSELECTED = 2292;
        /// <summary>
        /// Enable/disable highlight for current folding block (smallest one
        /// that contains the caret).
        /// </summary>
        public const int SCI_MARKERENABLEHIGHLIGHT = 2293;
        /// <summary>
        /// Add a marker to a line, returning an ID which can be used to find
        /// or delete the marker.
        /// </summary>
        public const int SCI_MARKERADD = 2043;
        /// <summary>
        /// Delete a marker from a line.
        /// </summary>
        public const int SCI_MARKERDELETE = 2044;
        /// <summary>
        /// Delete all markers with a particular number from all lines.
        /// </summary>
        public const int SCI_MARKERDELETEALL = 2045;
        /// <summary>
        /// Get a bit mask of all the markers set on a line.
        /// </summary>
        public const int SCI_MARKERGET = 2046;
        /// <summary>
        /// Find the next line at or after lineStart that includes a marker in
        /// mask. Return -1 when no more lines.
        /// </summary>
        public const int SCI_MARKERNEXT = 2047;
        /// <summary>
        /// Find the previous line before lineStart that includes a marker in
        /// mask.
        /// </summary>
        public const int SCI_MARKERPREVIOUS = 2048;
        /// <summary>
        /// Define a marker from a pixmap.
        /// </summary>
        public const int SCI_MARKERDEFINEPIXMAP = 2049;
        /// <summary>
        /// Add a set of markers to a line.
        /// </summary>
        public const int SCI_MARKERADDSET = 2466;
        /// <summary>
        /// Set the alpha used for a marker that is drawn in the text area, not
        /// the margin.
        /// </summary>
        public const int SCI_MARKERSETALPHA = 2476;
        /// <summary>
        /// Set a margin to be either numeric or symbolic.
        /// </summary>
        public const int SCI_SETMARGINTYPEN = 2240;
        /// <summary>
        /// Retrieve the type of a margin.
        /// </summary>
        public const int SCI_GETMARGINTYPEN = 2241;
        /// <summary>
        /// Set the width of a margin to a width expressed in pixels.
        /// </summary>
        public const int SCI_SETMARGINWIDTHN = 2242;
        /// <summary>
        /// Retrieve the width of a margin in pixels.
        /// </summary>
        public const int SCI_GETMARGINWIDTHN = 2243;
        /// <summary>
        /// Set a mask that determines which markers are displayed in a margin.
        /// </summary>
        public const int SCI_SETMARGINMASKN = 2244;
        /// <summary>
        /// Retrieve the marker mask of a margin.
        /// </summary>
        public const int SCI_GETMARGINMASKN = 2245;
        /// <summary>
        /// Make a margin sensitive or insensitive to mouse clicks.
        /// </summary>
        public const int SCI_SETMARGINSENSITIVEN = 2246;
        /// <summary>
        /// Retrieve the mouse click sensitivity of a margin.
        /// </summary>
        public const int SCI_GETMARGINSENSITIVEN = 2247;
        /// <summary>
        /// Set the cursor shown when the mouse is inside a margin.
        /// </summary>
        public const int SCI_SETMARGINCURSORN = 2248;
        /// <summary>
        /// Retrieve the cursor shown in a margin.
        /// </summary>
        public const int SCI_GETMARGINCURSORN = 2249;
        /// <summary>
        /// Set the background colour of a margin. Only visible for
        /// SC_MARGIN_COLOUR.
        /// </summary>
        public const int SCI_SETMARGINBACKN = 2250;
        /// <summary>
        /// Retrieve the background colour of a margin.
        /// </summary>
        public const int SCI_GETMARGINBACKN = 2251;
        /// <summary>
        /// Allocate a non-standard number of margins.
        /// </summary>
        public const int SCI_SETMARGINS = 2252;
        /// <summary>
        /// How many margins are there?.
        /// </summary>
        public const int SCI_GETMARGINS = 2253;
        /// <summary>
        /// Clear all the styles and make equivalent to the global default
        /// style.
        /// </summary>
        public const int SCI_STYLECLEARALL = 2050;
        /// <summary>
        /// Set the foreground colour of a style.
        /// </summary>
        public const int SCI_STYLESETFORE = 2051;
        /// <summary>
        /// Set the background colour of a style.
        /// </summary>
        public const int SCI_STYLESETBACK = 2052;
        /// <summary>
        /// Set a style to be bold or not.
        /// </summary>
        public const int SCI_STYLESETBOLD = 2053;
        /// <summary>
        /// Set a style to be italic or not.
        /// </summary>
        public const int SCI_STYLESETITALIC = 2054;
        /// <summary>
        /// Set the size of characters of a style.
        /// </summary>
        public const int SCI_STYLESETSIZE = 2055;
        /// <summary>
        /// Set the font of a style.
        /// </summary>
        public const int SCI_STYLESETFONT = 2056;
        /// <summary>
        /// Set a style to have its end of line filled or not.
        /// </summary>
        public const int SCI_STYLESETEOLFILLED = 2057;
        /// <summary>
        /// Reset the default style to its state at startup.
        /// </summary>
        public const int SCI_STYLERESETDEFAULT = 2058;
        /// <summary>
        /// Set a style to be underlined or not.
        /// </summary>
        public const int SCI_STYLESETUNDERLINE = 2059;
        /// <summary>
        /// Get the foreground colour of a style.
        /// </summary>
        public const int SCI_STYLEGETFORE = 2481;
        /// <summary>
        /// Get the background colour of a style.
        /// </summary>
        public const int SCI_STYLEGETBACK = 2482;
        /// <summary>
        /// Get is a style bold or not.
        /// </summary>
        public const int SCI_STYLEGETBOLD = 2483;
        /// <summary>
        /// Get is a style italic or not.
        /// </summary>
        public const int SCI_STYLEGETITALIC = 2484;
        /// <summary>
        /// Get the size of characters of a style.
        /// </summary>
        public const int SCI_STYLEGETSIZE = 2485;
        /// <summary>
        /// Get the font of a style. Returns the length of the fontName Result
        /// is NUL-terminated.
        /// </summary>
        public const int SCI_STYLEGETFONT = 2486;
        /// <summary>
        /// Get is a style to have its end of line filled or not.
        /// </summary>
        public const int SCI_STYLEGETEOLFILLED = 2487;
        /// <summary>
        /// Get is a style underlined or not.
        /// </summary>
        public const int SCI_STYLEGETUNDERLINE = 2488;
        /// <summary>
        /// Get is a style mixed case, or to force upper or lower case.
        /// </summary>
        public const int SCI_STYLEGETCASE = 2489;
        /// <summary>
        /// Get the character get of the font in a style.
        /// </summary>
        public const int SCI_STYLEGETCHARACTERSET = 2490;
        /// <summary>
        /// Get is a style visible or not.
        /// </summary>
        public const int SCI_STYLEGETVISIBLE = 2491;
        /// <summary>
        /// Get is a style changeable or not (read only). Experimental feature,
        /// currently buggy.
        /// </summary>
        public const int SCI_STYLEGETCHANGEABLE = 2492;
        /// <summary>
        /// Get is a style a hotspot or not.
        /// </summary>
        public const int SCI_STYLEGETHOTSPOT = 2493;
        /// <summary>
        /// Set a style to be mixed case, or to force upper or lower case.
        /// </summary>
        public const int SCI_STYLESETCASE = 2060;
        /// <summary>
        /// Set the size of characters of a style. Size is in points multiplied
        /// by 100.
        /// </summary>
        public const int SCI_STYLESETSIZEFRACTIONAL = 2061;
        /// <summary>
        /// Get the size of characters of a style in points multiplied by 100.
        /// </summary>
        public const int SCI_STYLEGETSIZEFRACTIONAL = 2062;
        /// <summary>
        /// Set the weight of characters of a style.
        /// </summary>
        public const int SCI_STYLESETWEIGHT = 2063;
        /// <summary>
        /// Get the weight of characters of a style.
        /// </summary>
        public const int SCI_STYLEGETWEIGHT = 2064;
        /// <summary>
        /// Set the character set of the font in a style.
        /// </summary>
        public const int SCI_STYLESETCHARACTERSET = 2066;
        /// <summary>
        /// Set a style to be a hotspot or not.
        /// </summary>
        public const int SCI_STYLESETHOTSPOT = 2409;
        /// <summary>
        /// Set the foreground colour of the main and additional selections and
        /// whether to use this setting.
        /// </summary>
        public const int SCI_SETSELFORE = 2067;
        /// <summary>
        /// Set the background colour of the main and additional selections and
        /// whether to use this setting.
        /// </summary>
        public const int SCI_SETSELBACK = 2068;
        /// <summary>
        /// Get the alpha of the selection.
        /// </summary>
        public const int SCI_GETSELALPHA = 2477;
        /// <summary>
        /// Set the alpha of the selection.
        /// </summary>
        public const int SCI_SETSELALPHA = 2478;
        /// <summary>
        /// Is the selection end of line filled?.
        /// </summary>
        public const int SCI_GETSELEOLFILLED = 2479;
        /// <summary>
        /// Set the selection to have its end of line filled or not.
        /// </summary>
        public const int SCI_SETSELEOLFILLED = 2480;
        /// <summary>
        /// Set the foreground colour of the caret.
        /// </summary>
        public const int SCI_SETCARETFORE = 2069;
        /// <summary>
        /// When key+modifier combination keyDefinition is pressed perform
        /// sciCommand.
        /// </summary>
        public const int SCI_ASSIGNCMDKEY = 2070;
        /// <summary>
        /// When key+modifier combination keyDefinition is pressed do nothing.
        /// </summary>
        public const int SCI_CLEARCMDKEY = 2071;
        /// <summary>
        /// Drop all key mappings.
        /// </summary>
        public const int SCI_CLEARALLCMDKEYS = 2072;
        /// <summary>
        /// Set the styles for a segment of the document.
        /// </summary>
        public const int SCI_SETSTYLINGEX = 2073;
        /// <summary>
        /// Set a style to be visible or not.
        /// </summary>
        public const int SCI_STYLESETVISIBLE = 2074;
        /// <summary>
        /// Get the time in milliseconds that the caret is on and off.
        /// </summary>
        public const int SCI_GETCARETPERIOD = 2075;
        /// <summary>
        /// Get the time in milliseconds that the caret is on and off. 0 =
        /// steady on.
        /// </summary>
        public const int SCI_SETCARETPERIOD = 2076;
        /// <summary>
        /// Set the set of characters making up words for when moving or
        /// selecting by word. First sets defaults like SetCharsDefault.
        /// </summary>
        public const int SCI_SETWORDCHARS = 2077;
        /// <summary>
        /// Get the set of characters making up words for when moving or
        /// selecting by word. Returns the number of characters.
        /// </summary>
        public const int SCI_GETWORDCHARS = 2646;
        /// <summary>
        /// Start a sequence of actions that is undone and redone as a unit.
        /// May be nested.
        /// </summary>
        public const int SCI_BEGINUNDOACTION = 2078;
        /// <summary>
        /// End a sequence of actions that is undone and redone as a unit.
        /// </summary>
        public const int SCI_ENDUNDOACTION = 2079;
        /// <summary>
        /// Set an indicator to plain, squiggle or TT.
        /// </summary>
        public const int SCI_INDICSETSTYLE = 2080;
        /// <summary>
        /// Retrieve the style of an indicator.
        /// </summary>
        public const int SCI_INDICGETSTYLE = 2081;
        /// <summary>
        /// Set the foreground colour of an indicator.
        /// </summary>
        public const int SCI_INDICSETFORE = 2082;
        /// <summary>
        /// Retrieve the foreground colour of an indicator.
        /// </summary>
        public const int SCI_INDICGETFORE = 2083;
        /// <summary>
        /// Set an indicator to draw under text or over(default).
        /// </summary>
        public const int SCI_INDICSETUNDER = 2510;
        /// <summary>
        /// Retrieve whether indicator drawn under or over text.
        /// </summary>
        public const int SCI_INDICGETUNDER = 2511;
        /// <summary>
        /// Set a hover indicator to plain, squiggle or TT.
        /// </summary>
        public const int SCI_INDICSETHOVERSTYLE = 2680;
        /// <summary>
        /// Retrieve the hover style of an indicator.
        /// </summary>
        public const int SCI_INDICGETHOVERSTYLE = 2681;
        /// <summary>
        /// Set the foreground hover colour of an indicator.
        /// </summary>
        public const int SCI_INDICSETHOVERFORE = 2682;
        /// <summary>
        /// Retrieve the foreground hover colour of an indicator.
        /// </summary>
        public const int SCI_INDICGETHOVERFORE = 2683;
        /// <summary>
        /// Set the attributes of an indicator.
        /// </summary>
        public const int SCI_INDICSETFLAGS = 2684;
        /// <summary>
        /// Retrieve the attributes of an indicator.
        /// </summary>
        public const int SCI_INDICGETFLAGS = 2685;
        /// <summary>
        /// Set the foreground colour of all whitespace and whether to use this
        /// setting.
        /// </summary>
        public const int SCI_SETWHITESPACEFORE = 2084;
        /// <summary>
        /// Set the background colour of all whitespace and whether to use this
        /// setting.
        /// </summary>
        public const int SCI_SETWHITESPACEBACK = 2085;
        /// <summary>
        /// Set the size of the dots used to mark space characters.
        /// </summary>
        public const int SCI_SETWHITESPACESIZE = 2086;
        /// <summary>
        /// Get the size of the dots used to mark space characters.
        /// </summary>
        public const int SCI_GETWHITESPACESIZE = 2087;
        // public const int SCI_SETSTYLEBITS = 2090;
        // public const int SCI_GETSTYLEBITS = 2091;
        /// <summary>
        /// Used to hold extra styling information for each line.
        /// </summary>
        public const int SCI_SETLINESTATE = 2092;
        /// <summary>
        /// Retrieve the extra styling information for a line.
        /// </summary>
        public const int SCI_GETLINESTATE = 2093;
        /// <summary>
        /// Retrieve the last line number that has line state.
        /// </summary>
        public const int SCI_GETMAXLINESTATE = 2094;
        /// <summary>
        /// Is the background of the line containing the caret in a different
        /// colour?.
        /// </summary>
        public const int SCI_GETCARETLINEVISIBLE = 2095;
        /// <summary>
        /// Display the background of the line containing the caret in a
        /// different colour.
        /// </summary>
        public const int SCI_SETCARETLINEVISIBLE = 2096;
        /// <summary>
        /// Get the colour of the background of the line containing the caret.
        /// </summary>
        public const int SCI_GETCARETLINEBACK = 2097;
        /// <summary>
        /// Set the colour of the background of the line containing the caret.
        /// </summary>
        public const int SCI_SETCARETLINEBACK = 2098;
        /// <summary>
        /// Set a style to be changeable or not (read only). Experimental
        /// feature, currently buggy.
        /// </summary>
        public const int SCI_STYLESETCHANGEABLE = 2099;
        /// <summary>
        /// Display a auto-completion list. The lengthEntered parameter
        /// indicates how many characters before the caret should be used to
        /// provide context.
        /// </summary>
        public const int SCI_AUTOCSHOW = 2100;
        /// <summary>
        /// Remove the auto-completion list from the screen.
        /// </summary>
        public const int SCI_AUTOCCANCEL = 2101;
        /// <summary>
        /// Is there an auto-completion list visible?.
        /// </summary>
        public const int SCI_AUTOCACTIVE = 2102;
        /// <summary>
        /// Retrieve the position of the caret when the auto-completion list
        /// was displayed.
        /// </summary>
        public const int SCI_AUTOCPOSSTART = 2103;
        /// <summary>
        /// User has selected an item so remove the list and insert the
        /// selection.
        /// </summary>
        public const int SCI_AUTOCCOMPLETE = 2104;
        /// <summary>
        /// Define a set of character that when typed cancel the
        /// auto-completion list.
        /// </summary>
        public const int SCI_AUTOCSTOPS = 2105;
        /// <summary>
        /// Change the separator character in the string setting up an
        /// auto-completion list. Default is space but can be changed if items
        /// contain space.
        /// </summary>
        public const int SCI_AUTOCSETSEPARATOR = 2106;
        /// <summary>
        /// Retrieve the auto-completion list separator character.
        /// </summary>
        public const int SCI_AUTOCGETSEPARATOR = 2107;
        /// <summary>
        /// Select the item in the auto-completion list that starts with a
        /// string.
        /// </summary>
        public const int SCI_AUTOCSELECT = 2108;
        /// <summary>
        /// Should the auto-completion list be cancelled if the user backspaces
        /// to a position before where the box was created.
        /// </summary>
        public const int SCI_AUTOCSETCANCELATSTART = 2110;
        /// <summary>
        /// Retrieve whether auto-completion cancelled by backspacing before
        /// start.
        /// </summary>
        public const int SCI_AUTOCGETCANCELATSTART = 2111;
        /// <summary>
        /// Define a set of characters that when typed will cause the
        /// autocompletion to choose the selected item.
        /// </summary>
        public const int SCI_AUTOCSETFILLUPS = 2112;
        /// <summary>
        /// Should a single item auto-completion list automatically choose the
        /// item.
        /// </summary>
        public const int SCI_AUTOCSETCHOOSESINGLE = 2113;
        /// <summary>
        /// Retrieve whether a single item auto-completion list automatically
        /// choose the item.
        /// </summary>
        public const int SCI_AUTOCGETCHOOSESINGLE = 2114;
        /// <summary>
        /// Set whether case is significant when performing auto-completion
        /// searches.
        /// </summary>
        public const int SCI_AUTOCSETIGNORECASE = 2115;
        /// <summary>
        /// Retrieve state of ignore case flag.
        /// </summary>
        public const int SCI_AUTOCGETIGNORECASE = 2116;
        /// <summary>
        /// Display a list of strings and send notification when user chooses
        /// one.
        /// </summary>
        public const int SCI_USERLISTSHOW = 2117;
        /// <summary>
        /// Set whether or not autocompletion is hidden automatically when
        /// nothing matches.
        /// </summary>
        public const int SCI_AUTOCSETAUTOHIDE = 2118;
        /// <summary>
        /// Retrieve whether or not autocompletion is hidden automatically when
        /// nothing matches.
        /// </summary>
        public const int SCI_AUTOCGETAUTOHIDE = 2119;
        /// <summary>
        /// Set whether or not autocompletion deletes any word characters after
        /// the inserted text upon completion.
        /// </summary>
        public const int SCI_AUTOCSETDROPRESTOFWORD = 2270;
        /// <summary>
        /// Retrieve whether or not autocompletion deletes any word characters
        /// after the inserted text upon completion.
        /// </summary>
        public const int SCI_AUTOCGETDROPRESTOFWORD = 2271;
        /// <summary>
        /// Register an XPM image for use in autocompletion lists.
        /// </summary>
        public const int SCI_REGISTERIMAGE = 2405;
        /// <summary>
        /// Clear all the registered XPM images.
        /// </summary>
        public const int SCI_CLEARREGISTEREDIMAGES = 2408;
        /// <summary>
        /// Retrieve the auto-completion list type-separator character.
        /// </summary>
        public const int SCI_AUTOCGETTYPESEPARATOR = 2285;
        /// <summary>
        /// Change the type-separator character in the string setting up an
        /// auto-completion list. Default is '?' but can be changed if items
        /// contain '?'.
        /// </summary>
        public const int SCI_AUTOCSETTYPESEPARATOR = 2286;
        /// <summary>
        /// Set the maximum width, in characters, of auto-completion and user
        /// lists. Set to 0 to autosize to fit longest item, which is the
        /// default.
        /// </summary>
        public const int SCI_AUTOCSETMAXWIDTH = 2208;
        /// <summary>
        /// Get the maximum width, in characters, of auto-completion and user
        /// lists.
        /// </summary>
        public const int SCI_AUTOCGETMAXWIDTH = 2209;
        /// <summary>
        /// Set the maximum height, in rows, of auto-completion and user lists.
        /// The default is 5 rows.
        /// </summary>
        public const int SCI_AUTOCSETMAXHEIGHT = 2210;
        /// <summary>
        /// Set the maximum height, in rows, of auto-completion and user lists.
        /// </summary>
        public const int SCI_AUTOCGETMAXHEIGHT = 2211;
        /// <summary>
        /// Set the number of spaces used for one level of indentation.
        /// </summary>
        public const int SCI_SETINDENT = 2122;
        /// <summary>
        /// Retrieve indentation size.
        /// </summary>
        public const int SCI_GETINDENT = 2123;
        /// <summary>
        /// Indentation will only use space characters if useTabs is false,
        /// otherwise it will use a combination of tabs and spaces.
        /// </summary>
        public const int SCI_SETUSETABS = 2124;
        /// <summary>
        /// Retrieve whether tabs will be used in indentation.
        /// </summary>
        public const int SCI_GETUSETABS = 2125;
        /// <summary>
        /// Change the indentation of a line to a number of columns.
        /// </summary>
        public const int SCI_SETLINEINDENTATION = 2126;
        /// <summary>
        /// Retrieve the number of columns that a line is indented.
        /// </summary>
        public const int SCI_GETLINEINDENTATION = 2127;
        /// <summary>
        /// Retrieve the position before the first non indentation character on
        /// a line.
        /// </summary>
        public const int SCI_GETLINEINDENTPOSITION = 2128;
        /// <summary>
        /// Retrieve the column number of a position, taking tab width into
        /// account.
        /// </summary>
        public const int SCI_GETCOLUMN = 2129;
        /// <summary>
        /// Count characters between two positions.
        /// </summary>
        public const int SCI_COUNTCHARACTERS = 2633;
        /// <summary>
        /// Show or hide the horizontal scroll bar.
        /// </summary>
        public const int SCI_SETHSCROLLBAR = 2130;
        /// <summary>
        /// Is the horizontal scroll bar visible?.
        /// </summary>
        public const int SCI_GETHSCROLLBAR = 2131;
        /// <summary>
        /// Show or hide indentation guides.
        /// </summary>
        public const int SCI_SETINDENTATIONGUIDES = 2132;
        /// <summary>
        /// Are the indentation guides visible?.
        /// </summary>
        public const int SCI_GETINDENTATIONGUIDES = 2133;
        /// <summary>
        /// Set the highlighted indentation guide column. 0 = no highlighted
        /// guide.
        /// </summary>
        public const int SCI_SETHIGHLIGHTGUIDE = 2134;
        /// <summary>
        /// Get the highlighted indentation guide column.
        /// </summary>
        public const int SCI_GETHIGHLIGHTGUIDE = 2135;
        /// <summary>
        /// Get the position after the last visible characters on a line.
        /// </summary>
        public const int SCI_GETLINEENDPOSITION = 2136;
        /// <summary>
        /// Get the code page used to interpret the bytes of the document as
        /// characters.
        /// </summary>
        public const int SCI_GETCODEPAGE = 2137;
        /// <summary>
        /// Get the foreground colour of the caret.
        /// </summary>
        public const int SCI_GETCARETFORE = 2138;
        /// <summary>
        /// In read-only mode?.
        /// </summary>
        public const int SCI_GETREADONLY = 2140;
        /// <summary>
        /// Sets the position of the caret.
        /// </summary>
        public const int SCI_SETCURRENTPOS = 2141;
        /// <summary>
        /// Sets the position that starts the selection - this becomes the
        /// anchor.
        /// </summary>
        public const int SCI_SETSELECTIONSTART = 2142;
        /// <summary>
        /// Returns the position at the start of the selection.
        /// </summary>
        public const int SCI_GETSELECTIONSTART = 2143;
        /// <summary>
        /// Sets the position that ends the selection - this becomes the caret.
        /// </summary>
        public const int SCI_SETSELECTIONEND = 2144;
        /// <summary>
        /// Returns the position at the end of the selection.
        /// </summary>
        public const int SCI_GETSELECTIONEND = 2145;
        /// <summary>
        /// Set caret to a position, while removing any existing selection.
        /// </summary>
        public const int SCI_SETEMPTYSELECTION = 2556;
        /// <summary>
        /// Sets the print magnification added to the point size of each style
        /// for printing.
        /// </summary>
        public const int SCI_SETPRINTMAGNIFICATION = 2146;
        /// <summary>
        /// Returns the print magnification.
        /// </summary>
        public const int SCI_GETPRINTMAGNIFICATION = 2147;
        /// <summary>
        /// Modify colours when printing for clearer printed text.
        /// </summary>
        public const int SCI_SETPRINTCOLOURMODE = 2148;
        /// <summary>
        /// Returns the print colour mode.
        /// </summary>
        public const int SCI_GETPRINTCOLOURMODE = 2149;
        /// <summary>
        /// Find some text in the document.
        /// </summary>
        public const int SCI_FINDTEXT = 2150;
        /// <summary>
        /// Draw the document into a display context such as a printer.
        /// </summary>
        public const int SCI_FORMATRANGE = 2151;
        /// <summary>
        /// Retrieve the display line at the top of the display.
        /// </summary>
        public const int SCI_GETFIRSTVISIBLELINE = 2152;
        /// <summary>
        /// Retrieve the contents of a line. Returns the length of the line.
        /// </summary>
        public const int SCI_GETLINE = 2153;
        /// <summary>
        /// Returns the number of lines in the document. There is always at
        /// least one.
        /// </summary>
        public const int SCI_GETLINECOUNT = 2154;
        /// <summary>
        /// Sets the size in pixels of the left margin.
        /// </summary>
        public const int SCI_SETMARGINLEFT = 2155;
        /// <summary>
        /// Returns the size in pixels of the left margin.
        /// </summary>
        public const int SCI_GETMARGINLEFT = 2156;
        /// <summary>
        /// Sets the size in pixels of the right margin.
        /// </summary>
        public const int SCI_SETMARGINRIGHT = 2157;
        /// <summary>
        /// Returns the size in pixels of the right margin.
        /// </summary>
        public const int SCI_GETMARGINRIGHT = 2158;
        /// <summary>
        /// Is the document different from when it was last saved?.
        /// </summary>
        public const int SCI_GETMODIFY = 2159;
        /// <summary>
        /// Select a range of text.
        /// </summary>
        public const int SCI_SETSEL = 2160;
        /// <summary>
        /// Retrieve the selected text. Return the length of the text. Result
        /// is NUL-terminated.
        /// </summary>
        public const int SCI_GETSELTEXT = 2161;
        /// <summary>
        /// Retrieve a range of text. Return the length of the text.
        /// </summary>
        public const int SCI_GETTEXTRANGE = 2162;
        /// <summary>
        /// Retrieve a range of text that can be past 2GB. Return the length of
        /// the text.
        /// </summary>
        public const int SCI_GETTEXTRANGEFULL = 2039;
        /// <summary>
        /// Draw the selection either highlighted or in normal
        /// (non-highlighted) style.
        /// </summary>
        public const int SCI_HIDESELECTION = 2163;
        /// <summary>
        /// Retrieve the x value of the point in the window where a position is
        /// displayed.
        /// </summary>
        public const int SCI_POINTXFROMPOSITION = 2164;
        /// <summary>
        /// Retrieve the y value of the point in the window where a position is
        /// displayed.
        /// </summary>
        public const int SCI_POINTYFROMPOSITION = 2165;
        /// <summary>
        /// Retrieve the line containing a position.
        /// </summary>
        public const int SCI_LINEFROMPOSITION = 2166;
        /// <summary>
        /// Retrieve the position at the start of a line.
        /// </summary>
        public const int SCI_POSITIONFROMLINE = 2167;
        /// <summary>
        /// Scroll horizontally and vertically.
        /// </summary>
        public const int SCI_LINESCROLL = 2168;
        /// <summary>
        /// Ensure the caret is visible.
        /// </summary>
        public const int SCI_SCROLLCARET = 2169;
        /// <summary>
        /// Scroll the argument positions and the range between them into view
        /// giving priority to the primary position then the secondary
        /// position. This may be used to make a search match visible.
        /// </summary>
        public const int SCI_SCROLLRANGE = 2569;
        /// <summary>
        /// Replace the selected text with the argument text.
        /// </summary>
        public const int SCI_REPLACESEL = 2170;
        /// <summary>
        /// Set to read only or read write.
        /// </summary>
        public const int SCI_SETREADONLY = 2171;
        /// <summary>
        /// Null operation.
        /// </summary>
        public const int SCI_NULL = 2172;
        /// <summary>
        /// Will a paste succeed?.
        /// </summary>
        public const int SCI_CANPASTE = 2173;
        /// <summary>
        /// Are there any undoable actions in the undo history?.
        /// </summary>
        public const int SCI_CANUNDO = 2174;
        /// <summary>
        /// Delete the undo history.
        /// </summary>
        public const int SCI_EMPTYUNDOBUFFER = 2175;
        /// <summary>
        /// Undo one action in the undo history.
        /// </summary>
        public const int SCI_UNDO = 2176;
        /// <summary>
        /// Cut the selection to the clipboard.
        /// </summary>
        public const int SCI_CUT = 2177;
        /// <summary>
        /// Copy the selection to the clipboard.
        /// </summary>
        public const int SCI_COPY = 2178;
        /// <summary>
        /// Paste the contents of the clipboard into the document replacing the
        /// selection.
        /// </summary>
        public const int SCI_PASTE = 2179;
        /// <summary>
        /// Clear the selection.
        /// </summary>
        public const int SCI_CLEAR = 2180;
        /// <summary>
        /// Replace the contents of the document with the argument text.
        /// </summary>
        public const int SCI_SETTEXT = 2181;
        /// <summary>
        /// Retrieve all the text in the document. Returns number of characters
        /// retrieved. Result is NUL-terminated.
        /// </summary>
        public const int SCI_GETTEXT = 2182;
        /// <summary>
        /// Retrieve the number of characters in the document.
        /// </summary>
        public const int SCI_GETTEXTLENGTH = 2183;
        /// <summary>
        /// Retrieve a pointer to a function that processes messages for this
        /// Scintilla.
        /// </summary>
        public const int SCI_GETDIRECTFUNCTION = 2184;
        /// <summary>
        /// Retrieve a pointer value to use as the first argument when calling
        /// the function returned by GetDirectFunction.
        /// </summary>
        public const int SCI_GETDIRECTPOINTER = 2185;
        /// <summary>
        /// Set to overtype (true) or insert mode.
        /// </summary>
        public const int SCI_SETOVERTYPE = 2186;
        /// <summary>
        /// Returns true if overtype mode is active otherwise false is
        /// returned.
        /// </summary>
        public const int SCI_GETOVERTYPE = 2187;
        /// <summary>
        /// Set the width of the insert mode caret.
        /// </summary>
        public const int SCI_SETCARETWIDTH = 2188;
        /// <summary>
        /// Returns the width of the insert mode caret.
        /// </summary>
        public const int SCI_GETCARETWIDTH = 2189;
        /// <summary>
        /// Sets the position that starts the target which is used for updating
        /// the document without affecting the scroll position.
        /// </summary>
        public const int SCI_SETTARGETSTART = 2190;
        /// <summary>
        /// Get the position that starts the target.
        /// </summary>
        public const int SCI_GETTARGETSTART = 2191;
        /// <summary>
        /// Sets the position that ends the target which is used for updating
        /// the document without affecting the scroll position.
        /// </summary>
        public const int SCI_SETTARGETEND = 2192;
        /// <summary>
        /// Get the position that ends the target.
        /// </summary>
        public const int SCI_GETTARGETEND = 2193;
        /// <summary>
        /// Replace the target text with the argument text. Text is counted so
        /// it can contain NULs. Returns the length of the replacement text.
        /// </summary>
        public const int SCI_REPLACETARGET = 2194;
        /// <summary>
        /// Replace the target text with the argument text after \d processing.
        /// Text is counted so it can contain NULs. Looks for \d where d is
        /// between 1 and 9 and replaces these with the strings matched in the
        /// last search operation which were surrounded by \( and \). Returns
        /// the length of the replacement text including any change caused by
        /// processing the \d patterns.
        /// </summary>
        public const int SCI_REPLACETARGETRE = 2195;
        /// <summary>
        /// Search for a counted string in the target and set the target to the
        /// found range. Text is counted so it can contain NULs. Returns start
        /// of found range or -1 for failure in which case target is not moved.
        /// </summary>
        public const int SCI_SEARCHINTARGET = 2197;
        /// <summary>
        /// Set the search flags used by SearchInTarget.
        /// </summary>
        public const int SCI_SETSEARCHFLAGS = 2198;
        /// <summary>
        /// Get the search flags used by SearchInTarget.
        /// </summary>
        public const int SCI_GETSEARCHFLAGS = 2199;
        /// <summary>
        /// Show a call tip containing a definition near position pos.
        /// </summary>
        public const int SCI_CALLTIPSHOW = 2200;
        /// <summary>
        /// Remove the call tip from the screen.
        /// </summary>
        public const int SCI_CALLTIPCANCEL = 2201;
        /// <summary>
        /// Is there an active call tip?.
        /// </summary>
        public const int SCI_CALLTIPACTIVE = 2202;
        /// <summary>
        /// Retrieve the position where the caret was before displaying the
        /// call tip.
        /// </summary>
        public const int SCI_CALLTIPPOSSTART = 2203;
        /// <summary>
        /// Set the start position in order to change when backspacing removes
        /// the calltip.
        /// </summary>
        public const int SCI_CALLTIPSETPOSSTART = 2214;
        /// <summary>
        /// Highlight a segment of the definition.
        /// </summary>
        public const int SCI_CALLTIPSETHLT = 2204;
        /// <summary>
        /// Set the background colour for the call tip.
        /// </summary>
        public const int SCI_CALLTIPSETBACK = 2205;
        /// <summary>
        /// Set the foreground colour for the call tip.
        /// </summary>
        public const int SCI_CALLTIPSETFORE = 2206;
        /// <summary>
        /// Set the foreground colour for the highlighted part of the call tip.
        /// </summary>
        public const int SCI_CALLTIPSETFOREHLT = 2207;
        /// <summary>
        /// Enable use of STYLE_CALLTIP and set call tip tab size in pixels.
        /// </summary>
        public const int SCI_CALLTIPUSESTYLE = 2212;
        /// <summary>
        /// Set position of calltip, above or below text.
        /// </summary>
        public const int SCI_CALLTIPSETPOSITION = 2213;
        /// <summary>
        /// Find the display line of a document line taking hidden lines into
        /// account.
        /// </summary>
        public const int SCI_VISIBLEFROMDOCLINE = 2220;
        /// <summary>
        /// Find the document line of a display line taking hidden lines into
        /// account.
        /// </summary>
        public const int SCI_DOCLINEFROMVISIBLE = 2221;
        /// <summary>
        /// The number of display lines needed to wrap a document line.
        /// </summary>
        public const int SCI_WRAPCOUNT = 2235;
        /// <summary>
        /// Set the fold level of a line. This encodes an integer level along
        /// with flags indicating whether the line is a header and whether it
        /// is effectively white space.
        /// </summary>
        public const int SCI_SETFOLDLEVEL = 2222;
        /// <summary>
        /// Retrieve the fold level of a line.
        /// </summary>
        public const int SCI_GETFOLDLEVEL = 2223;
        /// <summary>
        /// Find the last child line of a header line.
        /// </summary>
        public const int SCI_GETLASTCHILD = 2224;
        /// <summary>
        /// Find the parent line of a child line.
        /// </summary>
        public const int SCI_GETFOLDPARENT = 2225;
        /// <summary>
        /// Make a range of lines visible.
        /// </summary>
        public const int SCI_SHOWLINES = 2226;
        /// <summary>
        /// Make a range of lines invisible.
        /// </summary>
        public const int SCI_HIDELINES = 2227;
        /// <summary>
        /// Is a line visible?.
        /// </summary>
        public const int SCI_GETLINEVISIBLE = 2228;
        /// <summary>
        /// Are all lines visible?.
        /// </summary>
        public const int SCI_GETALLLINESVISIBLE = 2236;
        /// <summary>
        /// Show the children of a header line.
        /// </summary>
        public const int SCI_SETFOLDEXPANDED = 2229;
        /// <summary>
        /// Is a header line expanded?.
        /// </summary>
        public const int SCI_GETFOLDEXPANDED = 2230;
        /// <summary>
        /// Switch a header line between expanded and contracted.
        /// </summary>
        public const int SCI_TOGGLEFOLD = 2231;
        /// <summary>
        /// Expand or contract a fold header.
        /// </summary>
        public const int SCI_FOLDLINE = 2237;
        /// <summary>
        /// Expand or contract a fold header and its children.
        /// </summary>
        public const int SCI_FOLDCHILDREN = 2238;
        /// <summary>
        /// Expand a fold header and all children. Use the level argument
        /// instead of the line's current level.
        /// </summary>
        public const int SCI_EXPANDCHILDREN = 2239;
        /// <summary>
        /// Expand or contract all fold headers.
        /// </summary>
        public const int SCI_FOLDALL = 2662;
        /// <summary>
        /// Ensure a particular line is visible by expanding any header line
        /// hiding it.
        /// </summary>
        public const int SCI_ENSUREVISIBLE = 2232;
        /// <summary>
        /// Set automatic folding behaviours.
        /// </summary>
        public const int SCI_SETAUTOMATICFOLD = 2663;
        /// <summary>
        /// Get automatic folding behaviours.
        /// </summary>
        public const int SCI_GETAUTOMATICFOLD = 2664;
        /// <summary>
        /// Set some style options for folding.
        /// </summary>
        public const int SCI_SETFOLDFLAGS = 2233;
        /// <summary>
        /// Ensure a particular line is visible by expanding any header line
        /// hiding it. Use the currently set visibility policy to determine
        /// which range to display.
        /// </summary>
        public const int SCI_ENSUREVISIBLEENFORCEPOLICY = 2234;
        /// <summary>
        /// Sets whether a tab pressed when caret is within indentation
        /// indents.
        /// </summary>
        public const int SCI_SETTABINDENTS = 2260;
        /// <summary>
        /// Does a tab pressed when caret is within indentation indent?.
        /// </summary>
        public const int SCI_GETTABINDENTS = 2261;
        /// <summary>
        /// Sets whether a backspace pressed when caret is within indentation
        /// unindents.
        /// </summary>
        public const int SCI_SETBACKSPACEUNINDENTS = 2262;
        /// <summary>
        /// Does a backspace pressed when caret is within indentation
        /// unindent?.
        /// </summary>
        public const int SCI_GETBACKSPACEUNINDENTS = 2263;
        /// <summary>
        /// Sets the time the mouse must sit still to generate a mouse dwell
        /// event.
        /// </summary>
        public const int SCI_SETMOUSEDWELLTIME = 2264;
        /// <summary>
        /// Retrieve the time the mouse must sit still to generate a mouse
        /// dwell event.
        /// </summary>
        public const int SCI_GETMOUSEDWELLTIME = 2265;
        /// <summary>
        /// Get position of start of word.
        /// </summary>
        public const int SCI_WORDSTARTPOSITION = 2266;
        /// <summary>
        /// Get position of end of word.
        /// </summary>
        public const int SCI_WORDENDPOSITION = 2267;
        /// <summary>
        /// Is the range start..end considered a word?.
        /// </summary>
        public const int SCI_ISRANGEWORD = 2691;
        /// <summary>
        /// Sets whether text is word wrapped.
        /// </summary>
        public const int SCI_SETWRAPMODE = 2268;
        /// <summary>
        /// Retrieve whether text is word wrapped.
        /// </summary>
        public const int SCI_GETWRAPMODE = 2269;
        /// <summary>
        /// Set the display mode of visual flags for wrapped lines.
        /// </summary>
        public const int SCI_SETWRAPVISUALFLAGS = 2460;
        /// <summary>
        /// Retrive the display mode of visual flags for wrapped lines.
        /// </summary>
        public const int SCI_GETWRAPVISUALFLAGS = 2461;
        /// <summary>
        /// Set the location of visual flags for wrapped lines.
        /// </summary>
        public const int SCI_SETWRAPVISUALFLAGSLOCATION = 2462;
        /// <summary>
        /// Retrive the location of visual flags for wrapped lines.
        /// </summary>
        public const int SCI_GETWRAPVISUALFLAGSLOCATION = 2463;
        /// <summary>
        /// Set the start indent for wrapped lines.
        /// </summary>
        public const int SCI_SETWRAPSTARTINDENT = 2464;
        /// <summary>
        /// Retrive the start indent for wrapped lines.
        /// </summary>
        public const int SCI_GETWRAPSTARTINDENT = 2465;
        /// <summary>
        /// Sets how wrapped sublines are placed. Default is fixed.
        /// </summary>
        public const int SCI_SETWRAPINDENTMODE = 2472;
        /// <summary>
        /// Retrieve how wrapped sublines are placed. Default is fixed.
        /// </summary>
        public const int SCI_GETWRAPINDENTMODE = 2473;
        /// <summary>
        /// Sets the degree of caching of layout information.
        /// </summary>
        public const int SCI_SETLAYOUTCACHE = 2272;
        /// <summary>
        /// Retrieve the degree of caching of layout information.
        /// </summary>
        public const int SCI_GETLAYOUTCACHE = 2273;
        /// <summary>
        /// Sets the document width assumed for scrolling.
        /// </summary>
        public const int SCI_SETSCROLLWIDTH = 2274;
        /// <summary>
        /// Retrieve the document width assumed for scrolling.
        /// </summary>
        public const int SCI_GETSCROLLWIDTH = 2275;
        /// <summary>
        /// Sets whether the maximum width line displayed is used to set scroll
        /// width.
        /// </summary>
        public const int SCI_SETSCROLLWIDTHTRACKING = 2516;
        /// <summary>
        /// Retrieve whether the scroll width tracks wide lines.
        /// </summary>
        public const int SCI_GETSCROLLWIDTHTRACKING = 2517;
        /// <summary>
        /// Measure the pixel width of some text in a particular style. NUL
        /// terminated text argument. Does not handle tab or control
        /// characters.
        /// </summary>
        public const int SCI_TEXTWIDTH = 2276;
        /// <summary>
        /// Sets the scroll range so that maximum scroll position has the last
        /// line at the bottom of the view (default). Setting this to false
        /// allows scrolling one page below the last line.
        /// </summary>
        public const int SCI_SETENDATLASTLINE = 2277;
        /// <summary>
        /// Retrieve whether the maximum scroll position has the last line at
        /// the bottom of the view.
        /// </summary>
        public const int SCI_GETENDATLASTLINE = 2278;
        /// <summary>
        /// Retrieve the height of a particular line of text in pixels.
        /// </summary>
        public const int SCI_TEXTHEIGHT = 2279;
        /// <summary>
        /// Show or hide the vertical scroll bar.
        /// </summary>
        public const int SCI_SETVSCROLLBAR = 2280;
        /// <summary>
        /// Is the vertical scroll bar visible?.
        /// </summary>
        public const int SCI_GETVSCROLLBAR = 2281;
        /// <summary>
        /// Append a string to the end of the document without changing the
        /// selection.
        /// </summary>
        public const int SCI_APPENDTEXT = 2282;
        /// <summary>
        /// Is drawing done in two phases with backgrounds drawn before
        /// foregrounds?.
        /// </summary>
        public const int SCI_GETTWOPHASEDRAW = 2283;
        /// <summary>
        /// In twoPhaseDraw mode, drawing is performed in two phases, first the
        /// background and then the foreground. This avoids chopping off
        /// characters that overlap the next run.
        /// </summary>
        public const int SCI_SETTWOPHASEDRAW = 2284;
        /// <summary>
        /// How many phases is drawing done in?.
        /// </summary>
        public const int SCI_GETPHASESDRAW = 2673;
        /// <summary>
        /// In one phase draw, text is drawn in a series of rectangular blocks
        /// with no overlap. In two phase draw, text is drawn in a series of
        /// lines allowing runs to overlap horizontally. In multiple phase
        /// draw, each element is drawn over the whole drawing area, allowing
        /// text to overlap from one line to the next.
        /// </summary>
        public const int SCI_SETPHASESDRAW = 2674;
        /// <summary>
        /// Choose the quality level for text from the FontQuality enumeration.
        /// </summary>
        public const int SCI_SETFONTQUALITY = 2611;
        /// <summary>
        /// Retrieve the quality level for text.
        /// </summary>
        public const int SCI_GETFONTQUALITY = 2612;
        /// <summary>
        /// Scroll so that a display line is at the top of the display.
        /// </summary>
        public const int SCI_SETFIRSTVISIBLELINE = 2613;
        /// <summary>
        /// Change the effect of pasting when there are multiple selections.
        /// </summary>
        public const int SCI_SETMULTIPASTE = 2614;
        /// <summary>
        /// Retrieve the effect of pasting when there are multiple selections.
        /// </summary>
        public const int SCI_GETMULTIPASTE = 2615;
        /// <summary>
        /// Retrieve the value of a tag from a regular expression search.
        /// Result is NUL-terminated.
        /// </summary>
        public const int SCI_GETTAG = 2616;
        /// <summary>
        /// Make the target range start and end be the same as the selection
        /// range start and end.
        /// </summary>
        public const int SCI_TARGETFROMSELECTION = 2287;
        /// <summary>
        /// Sets the target to the whole document.
        /// </summary>
        public const int SCI_TARGETWHOLEDOCUMENT = 2690;
        /// <summary>
        /// Join the lines in the target.
        /// </summary>
        public const int SCI_LINESJOIN = 2288;
        /// <summary>
        /// Split the lines in the target into lines that are less wide than
        /// pixelWidth where possible.
        /// </summary>
        public const int SCI_LINESSPLIT = 2289;
        /// <summary>
        /// Set one of the colours used as a chequerboard pattern in the fold
        /// margin.
        /// </summary>
        public const int SCI_SETFOLDMARGINCOLOUR = 2290;
        /// <summary>
        /// Set the other colour used as a chequerboard pattern in the fold
        /// margin.
        /// </summary>
        public const int SCI_SETFOLDMARGINHICOLOUR = 2291;
        /// <summary>
        /// Move caret down one line.
        /// </summary>
        public const int SCI_LINEDOWN = 2300;
        /// <summary>
        /// Move caret down one line extending selection to new caret position.
        /// </summary>
        public const int SCI_LINEDOWNEXTEND = 2301;
        /// <summary>
        /// Move caret up one line.
        /// </summary>
        public const int SCI_LINEUP = 2302;
        /// <summary>
        /// Move caret up one line extending selection to new caret position.
        /// </summary>
        public const int SCI_LINEUPEXTEND = 2303;
        /// <summary>
        /// Move caret left one character.
        /// </summary>
        public const int SCI_CHARLEFT = 2304;
        /// <summary>
        /// Move caret left one character extending selection to new caret
        /// position.
        /// </summary>
        public const int SCI_CHARLEFTEXTEND = 2305;
        /// <summary>
        /// Move caret right one character.
        /// </summary>
        public const int SCI_CHARRIGHT = 2306;
        /// <summary>
        /// Move caret right one character extending selection to new caret
        /// position.
        /// </summary>
        public const int SCI_CHARRIGHTEXTEND = 2307;
        /// <summary>
        /// Move caret left one word.
        /// </summary>
        public const int SCI_WORDLEFT = 2308;
        /// <summary>
        /// Move caret left one word extending selection to new caret position.
        /// </summary>
        public const int SCI_WORDLEFTEXTEND = 2309;
        /// <summary>
        /// Move caret right one word.
        /// </summary>
        public const int SCI_WORDRIGHT = 2310;
        /// <summary>
        /// Move caret right one word extending selection to new caret
        /// position.
        /// </summary>
        public const int SCI_WORDRIGHTEXTEND = 2311;
        /// <summary>
        /// Move caret to first position on line.
        /// </summary>
        public const int SCI_HOME = 2312;
        /// <summary>
        /// Move caret to first position on line extending selection to new
        /// caret position.
        /// </summary>
        public const int SCI_HOMEEXTEND = 2313;
        /// <summary>
        /// Move caret to last position on line.
        /// </summary>
        public const int SCI_LINEEND = 2314;
        /// <summary>
        /// Move caret to last position on line extending selection to new
        /// caret position.
        /// </summary>
        public const int SCI_LINEENDEXTEND = 2315;
        /// <summary>
        /// Move caret to first position in document.
        /// </summary>
        public const int SCI_DOCUMENTSTART = 2316;
        /// <summary>
        /// Move caret to first position in document extending selection to new
        /// caret position.
        /// </summary>
        public const int SCI_DOCUMENTSTARTEXTEND = 2317;
        /// <summary>
        /// Move caret to last position in document.
        /// </summary>
        public const int SCI_DOCUMENTEND = 2318;
        /// <summary>
        /// Move caret to last position in document extending selection to new
        /// caret position.
        /// </summary>
        public const int SCI_DOCUMENTENDEXTEND = 2319;
        /// <summary>
        /// Move caret one page up.
        /// </summary>
        public const int SCI_PAGEUP = 2320;
        /// <summary>
        /// Move caret one page up extending selection to new caret position.
        /// </summary>
        public const int SCI_PAGEUPEXTEND = 2321;
        /// <summary>
        /// Move caret one page down.
        /// </summary>
        public const int SCI_PAGEDOWN = 2322;
        /// <summary>
        /// Move caret one page down extending selection to new caret position.
        /// </summary>
        public const int SCI_PAGEDOWNEXTEND = 2323;
        /// <summary>
        /// Switch from insert to overtype mode or the reverse.
        /// </summary>
        public const int SCI_EDITTOGGLEOVERTYPE = 2324;
        /// <summary>
        /// Cancel any modes such as call tip or auto-completion list display.
        /// </summary>
        public const int SCI_CANCEL = 2325;
        /// <summary>
        /// Delete the selection or if no selection, the character before the
        /// caret.
        /// </summary>
        public const int SCI_DELETEBACK = 2326;
        /// <summary>
        /// If selection is empty or all on one line replace the selection with
        /// a tab character. If more than one line selected, indent the lines.
        /// </summary>
        public const int SCI_TAB = 2327;
        /// <summary>
        /// If selection is empty or all on one line dedent the line if caret
        /// is at start, else move caret. If more than one line selected,
        /// dedent the lines.
        /// </summary>
        public const int SCI_BACKTAB = 2328;
        /// <summary>
        /// Insert a new line, may use a CRLF, CR or LF depending on EOL mode.
        /// </summary>
        public const int SCI_NEWLINE = 2329;
        /// <summary>
        /// Insert a Form Feed character.
        /// </summary>
        public const int SCI_FORMFEED = 2330;
        /// <summary>
        /// Move caret to before first visible character on line. If already
        /// there move to first character on line.
        /// </summary>
        public const int SCI_VCHOME = 2331;
        /// <summary>
        /// Like VCHome but extending selection to new caret position.
        /// </summary>
        public const int SCI_VCHOMEEXTEND = 2332;
        /// <summary>
        /// Magnify the displayed text by increasing the sizes by 1 point.
        /// </summary>
        public const int SCI_ZOOMIN = 2333;
        /// <summary>
        /// Make the displayed text smaller by decreasing the sizes by 1 point.
        /// </summary>
        public const int SCI_ZOOMOUT = 2334;
        /// <summary>
        /// Delete the word to the left of the caret.
        /// </summary>
        public const int SCI_DELWORDLEFT = 2335;
        /// <summary>
        /// Delete the word to the right of the caret.
        /// </summary>
        public const int SCI_DELWORDRIGHT = 2336;
        /// <summary>
        /// Delete the word to the right of the caret, but not the trailing
        /// non-word characters.
        /// </summary>
        public const int SCI_DELWORDRIGHTEND = 2518;
        /// <summary>
        /// Cut the line containing the caret.
        /// </summary>
        public const int SCI_LINECUT = 2337;
        /// <summary>
        /// Delete the line containing the caret.
        /// </summary>
        public const int SCI_LINEDELETE = 2338;
        /// <summary>
        /// Switch the current line with the previous.
        /// </summary>
        public const int SCI_LINETRANSPOSE = 2339;
        /// <summary>
        /// Duplicate the current line.
        /// </summary>
        public const int SCI_LINEDUPLICATE = 2404;
        /// <summary>
        /// Transform the selection to lower case.
        /// </summary>
        public const int SCI_LOWERCASE = 2340;
        /// <summary>
        /// Transform the selection to upper case.
        /// </summary>
        public const int SCI_UPPERCASE = 2341;
        /// <summary>
        /// Scroll the document down, keeping the caret visible.
        /// </summary>
        public const int SCI_LINESCROLLDOWN = 2342;
        /// <summary>
        /// Scroll the document up, keeping the caret visible.
        /// </summary>
        public const int SCI_LINESCROLLUP = 2343;
        /// <summary>
        /// Delete the selection or if no selection, the character before the
        /// caret. Will not delete the character before at the start of a line.
        /// </summary>
        public const int SCI_DELETEBACKNOTLINE = 2344;
        /// <summary>
        /// Move caret to first position on display line.
        /// </summary>
        public const int SCI_HOMEDISPLAY = 2345;
        /// <summary>
        /// Move caret to first position on display line extending selection to
        /// new caret position.
        /// </summary>
        public const int SCI_HOMEDISPLAYEXTEND = 2346;
        /// <summary>
        /// Move caret to last position on display line.
        /// </summary>
        public const int SCI_LINEENDDISPLAY = 2347;
        /// <summary>
        /// Move caret to last position on display line extending selection to
        /// new caret position.
        /// </summary>
        public const int SCI_LINEENDDISPLAYEXTEND = 2348;
        /// <summary>
        /// Like Home but when word-wrap is enabled goes first to start of
        /// display line HomeDisplay, then to start of document line Home.
        /// </summary>
        public const int SCI_HOMEWRAP = 2349;
        /// <summary>
        /// Reverse order of selected lines.
        /// </summary>
        public const int SCI_LINEREVERSE = 2354;
        /// <summary>
        /// Like HomeExtend but when word-wrap is enabled extends first to
        /// start of display line HomeDisplayExtend, then to start of document
        /// line HomeExtend.
        /// </summary>
        public const int SCI_HOMEWRAPEXTEND = 2450;
        /// <summary>
        /// Like LineEnd but when word-wrap is enabled goes first to end of
        /// display line LineEndDisplay, then to start of document line
        /// LineEnd.
        /// </summary>
        public const int SCI_LINEENDWRAP = 2451;
        /// <summary>
        /// Like LineEndExtend but when word-wrap is enabled extends first to
        /// end of display line LineEndDisplayExtend, then to start of document
        /// line LineEndExtend.
        /// </summary>
        public const int SCI_LINEENDWRAPEXTEND = 2452;
        /// <summary>
        /// Like VCHome but when word-wrap is enabled goes first to start of
        /// display line VCHomeDisplay, then behaves like VCHome.
        /// </summary>
        public const int SCI_VCHOMEWRAP = 2453;
        /// <summary>
        /// Like VCHomeExtend but when word-wrap is enabled extends first to
        /// start of display line VCHomeDisplayExtend, then behaves like
        /// VCHomeExtend.
        /// </summary>
        public const int SCI_VCHOMEWRAPEXTEND = 2454;
        /// <summary>
        /// Copy the line containing the caret.
        /// </summary>
        public const int SCI_LINECOPY = 2455;
        /// <summary>
        /// Move the caret inside current view if it's not there already.
        /// </summary>
        public const int SCI_MOVECARETINSIDEVIEW = 2401;
        /// <summary>
        /// How many characters are on a line, including end of line
        /// characters?.
        /// </summary>
        public const int SCI_LINELENGTH = 2350;
        /// <summary>
        /// Highlight the characters at two positions.
        /// </summary>
        public const int SCI_BRACEHIGHLIGHT = 2351;
        /// <summary>
        /// Use specified indicator to highlight matching braces instead of
        /// changing their style.
        /// </summary>
        public const int SCI_BRACEHIGHLIGHTINDICATOR = 2498;
        /// <summary>
        /// Highlight the character at a position indicating there is no
        /// matching brace.
        /// </summary>
        public const int SCI_BRACEBADLIGHT = 2352;
        /// <summary>
        /// Use specified indicator to highlight non matching brace instead of
        /// changing its style.
        /// </summary>
        public const int SCI_BRACEBADLIGHTINDICATOR = 2499;
        /// <summary>
        /// Find the position of a matching brace or INVALID_POSITION if no
        /// match. The maxReStyle must be 0 for now. It may be defined in a
        /// future release.
        /// </summary>
        public const int SCI_BRACEMATCH = 2353;
        /// <summary>
        /// Are the end of line characters visible?.
        /// </summary>
        public const int SCI_GETVIEWEOL = 2355;
        /// <summary>
        /// Make the end of line characters visible or invisible.
        /// </summary>
        public const int SCI_SETVIEWEOL = 2356;
        /// <summary>
        /// Retrieve a pointer to the document object.
        /// </summary>
        public const int SCI_GETDOCPOINTER = 2357;
        /// <summary>
        /// Change the document object used.
        /// </summary>
        public const int SCI_SETDOCPOINTER = 2358;
        /// <summary>
        /// Set which document modification events are sent to the container.
        /// </summary>
        public const int SCI_SETMODEVENTMASK = 2359;
        /// <summary>
        /// Retrieve the column number which text should be kept within.
        /// </summary>
        public const int SCI_GETEDGECOLUMN = 2360;
        /// <summary>
        /// Set the column number of the edge. If text goes past the edge then
        /// it is highlighted.
        /// </summary>
        public const int SCI_SETEDGECOLUMN = 2361;
        /// <summary>
        /// Retrieve the edge highlight mode.
        /// </summary>
        public const int SCI_GETEDGEMODE = 2362;
        /// <summary>
        /// The edge may be displayed by a line (EDGE_LINE/EDGE_MULTILINE) or
        /// by highlighting text that goes beyond it (EDGE_BACKGROUND) or not
        /// displayed at all (EDGE_NONE).
        /// </summary>
        public const int SCI_SETEDGEMODE = 2363;
        /// <summary>
        /// Retrieve the colour used in edge indication.
        /// </summary>
        public const int SCI_GETEDGECOLOUR = 2364;
        /// <summary>
        /// Change the colour used in edge indication.
        /// </summary>
        public const int SCI_SETEDGECOLOUR = 2365;
        /// <summary>
        /// Sets the current caret position to be the search anchor.
        /// </summary>
        public const int SCI_SEARCHANCHOR = 2366;
        /// <summary>
        /// Find some text starting at the search anchor. Does not ensure the
        /// selection is visible.
        /// </summary>
        public const int SCI_SEARCHNEXT = 2367;
        /// <summary>
        /// Find some text starting at the search anchor and moving backwards.
        /// Does not ensure the selection is visible.
        /// </summary>
        public const int SCI_SEARCHPREV = 2368;
        /// <summary>
        /// Retrieves the number of lines completely visible.
        /// </summary>
        public const int SCI_LINESONSCREEN = 2370;
        /// <summary>
        /// Set whether a pop up menu is displayed automatically when the user
        /// presses the wrong mouse button on certain areas.
        /// </summary>
        public const int SCI_USEPOPUP = 2371;
        /// <summary>
        /// Is the selection rectangular? The alternative is the more common
        /// stream selection.
        /// </summary>
        public const int SCI_SELECTIONISRECTANGLE = 2372;
        /// <summary>
        /// Set the zoom level. This number of points is added to the size of
        /// all fonts. It may be positive to magnify or negative to reduce.
        /// </summary>
        public const int SCI_SETZOOM = 2373;
        /// <summary>
        /// Retrieve the zoom level.
        /// </summary>
        public const int SCI_GETZOOM = 2374;
        /// <summary>
        /// Create a new document object. Starts with reference count of 1 and
        /// not selected into editor.
        /// </summary>
        public const int SCI_CREATEDOCUMENT = 2375;
        /// <summary>
        /// Extend life of document.
        /// </summary>
        public const int SCI_ADDREFDOCUMENT = 2376;
        /// <summary>
        /// Release a reference to the document, deleting document if it fades
        /// to black.
        /// </summary>
        public const int SCI_RELEASEDOCUMENT = 2377;
        /// <summary>
        /// Get which document modification events are sent to the container.
        /// </summary>
        public const int SCI_GETMODEVENTMASK = 2378;
        /// <summary>
        /// Change internal focus flag.
        /// </summary>
        public const int SCI_SETFOCUS = 2380;
        /// <summary>
        /// Get internal focus flag.
        /// </summary>
        public const int SCI_GETFOCUS = 2381;
        /// <summary>
        /// Change error status - 0 = OK.
        /// </summary>
        public const int SCI_SETSTATUS = 2382;
        /// <summary>
        /// Get error status.
        /// </summary>
        public const int SCI_GETSTATUS = 2383;
        /// <summary>
        /// Set whether the mouse is captured when its button is pressed.
        /// </summary>
        public const int SCI_SETMOUSEDOWNCAPTURES = 2384;
        /// <summary>
        /// Get whether mouse gets captured.
        /// </summary>
        public const int SCI_GETMOUSEDOWNCAPTURES = 2385;
        /// <summary>
        /// Sets the cursor to one of the SC_CURSOR* values.
        /// </summary>
        public const int SCI_SETCURSOR = 2386;
        /// <summary>
        /// Get cursor type.
        /// </summary>
        public const int SCI_GETCURSOR = 2387;
        /// <summary>
        /// Change the way control characters are displayed: If symbol is
        /// &lt; 32, keep the drawn way, else, use the given character.
        /// </summary>
        public const int SCI_SETCONTROLCHARSYMBOL = 2388;
        /// <summary>
        /// Get the way control characters are displayed.
        /// </summary>
        public const int SCI_GETCONTROLCHARSYMBOL = 2389;
        /// <summary>
        /// Move to the previous change in capitalisation.
        /// </summary>
        public const int SCI_WORDPARTLEFT = 2390;
        /// <summary>
        /// Move to the previous change in capitalisation extending selection
        /// to new caret position.
        /// </summary>
        public const int SCI_WORDPARTLEFTEXTEND = 2391;
        /// <summary>
        /// Move to the change next in capitalisation.
        /// </summary>
        public const int SCI_WORDPARTRIGHT = 2392;
        /// <summary>
        /// Move to the next change in capitalisation extending selection to
        /// new caret position.
        /// </summary>
        public const int SCI_WORDPARTRIGHTEXTEND = 2393;
        /// <summary>
        /// Set the way the display area is determined when a particular line
        /// is to be moved to by Find, FindNext, GotoLine, etc.
        /// </summary>
        public const int SCI_SETVISIBLEPOLICY = 2394;
        /// <summary>
        /// Delete back from the current position to the start of the line.
        /// </summary>
        public const int SCI_DELLINELEFT = 2395;
        /// <summary>
        /// Delete forwards from the current position to the end of the line.
        /// </summary>
        public const int SCI_DELLINERIGHT = 2396;
        /// <summary>
        /// Set the xOffset (ie, horizontal scroll position).
        /// </summary>
        public const int SCI_SETXOFFSET = 2397;
        /// <summary>
        /// Get the xOffset (ie, horizontal scroll position).
        /// </summary>
        public const int SCI_GETXOFFSET = 2398;
        /// <summary>
        /// Set the last x chosen value to be the caret x position.
        /// </summary>
        public const int SCI_CHOOSECARETX = 2399;
        /// <summary>
        /// Set the focus to this Scintilla widget.
        /// </summary>
        public const int SCI_GRABFOCUS = 2400;
        /// <summary>
        /// Set the way the caret is kept visible when going sideways. The
        /// exclusion zone is given in pixels.
        /// </summary>
        public const int SCI_SETXCARETPOLICY = 2402;
        /// <summary>
        /// Set the way the line the caret is on is kept visible. The exclusion
        /// zone is given in lines.
        /// </summary>
        public const int SCI_SETYCARETPOLICY = 2403;
        /// <summary>
        /// Set printing to line wrapped (SC_WRAP_WORD) or not line wrapped
        /// (SC_WRAP_NONE).
        /// </summary>
        public const int SCI_SETPRINTWRAPMODE = 2406;
        /// <summary>
        /// Is printing line wrapped?.
        /// </summary>
        public const int SCI_GETPRINTWRAPMODE = 2407;
        /// <summary>
        /// Set a fore colour for active hotspots.
        /// </summary>
        public const int SCI_SETHOTSPOTACTIVEFORE = 2410;
        /// <summary>
        /// Get the fore colour for active hotspots.
        /// </summary>
        public const int SCI_GETHOTSPOTACTIVEFORE = 2494;
        /// <summary>
        /// Set a back colour for active hotspots.
        /// </summary>
        public const int SCI_SETHOTSPOTACTIVEBACK = 2411;
        /// <summary>
        /// Get the back colour for active hotspots.
        /// </summary>
        public const int SCI_GETHOTSPOTACTIVEBACK = 2495;
        /// <summary>
        /// Enable / Disable underlining active hotspots.
        /// </summary>
        public const int SCI_SETHOTSPOTACTIVEUNDERLINE = 2412;
        /// <summary>
        /// Get whether underlining for active hotspots.
        /// </summary>
        public const int SCI_GETHOTSPOTACTIVEUNDERLINE = 2496;
        /// <summary>
        /// Limit hotspots to single line so hotspots on two lines don't merge.
        /// </summary>
        public const int SCI_SETHOTSPOTSINGLELINE = 2421;
        /// <summary>
        /// Get the HotspotSingleLine property.
        /// </summary>
        public const int SCI_GETHOTSPOTSINGLELINE = 2497;
        /// <summary>
        /// Move caret down one paragraph (delimited by empty lines).
        /// </summary>
        public const int SCI_PARADOWN = 2413;
        /// <summary>
        /// Extend selection down one paragraph (delimited by empty lines).
        /// </summary>
        public const int SCI_PARADOWNEXTEND = 2414;
        /// <summary>
        /// Move caret up one paragraph (delimited by empty lines).
        /// </summary>
        public const int SCI_PARAUP = 2415;
        /// <summary>
        /// Extend selection up one paragraph (delimited by empty lines).
        /// </summary>
        public const int SCI_PARAUPEXTEND = 2416;
        /// <summary>
        /// Given a valid document position, return a position that differs in
        /// a number of characters. Returned value is always between 0 and last
        /// position in document.
        /// </summary>
        public const int SCI_POSITIONRELATIVE = 2670;
        /// <summary>
        /// Copy a range of text to the clipboard. Positions are clipped into
        /// the document.
        /// </summary>
        public const int SCI_COPYRANGE = 2419;
        /// <summary>
        /// Copy argument text to the clipboard.
        /// </summary>
        public const int SCI_COPYTEXT = 2420;
        /// <summary>
        /// Set the selection mode to stream (SC_SEL_STREAM) or rectangular
        /// (SC_SEL_RECTANGLE/SC_SEL_THIN) or by lines (SC_SEL_LINES).
        /// </summary>
        public const int SCI_SETSELECTIONMODE = 2422;
        /// <summary>
        /// Get the mode of the current selection.
        /// </summary>
        public const int SCI_GETSELECTIONMODE = 2423;
        /// <summary>
        /// Retrieve the position of the start of the selection at the given
        /// line (INVALID_POSITION if no selection on this line).
        /// </summary>
        public const int SCI_GETLINESELSTARTPOSITION = 2424;
        /// <summary>
        /// Retrieve the position of the end of the selection at the given line
        /// (INVALID_POSITION if no selection on this line).
        /// </summary>
        public const int SCI_GETLINESELENDPOSITION = 2425;
        /// <summary>
        /// Move caret down one line, extending rectangular selection to new
        /// caret position.
        /// </summary>
        public const int SCI_LINEDOWNRECTEXTEND = 2426;
        /// <summary>
        /// Move caret up one line, extending rectangular selection to new
        /// caret position.
        /// </summary>
        public const int SCI_LINEUPRECTEXTEND = 2427;
        /// <summary>
        /// Move caret left one character, extending rectangular selection to
        /// new caret position.
        /// </summary>
        public const int SCI_CHARLEFTRECTEXTEND = 2428;
        /// <summary>
        /// Move caret right one character, extending rectangular selection to
        /// new caret position.
        /// </summary>
        public const int SCI_CHARRIGHTRECTEXTEND = 2429;
        /// <summary>
        /// Move caret to first position on line, extending rectangular
        /// selection to new caret position.
        /// </summary>
        public const int SCI_HOMERECTEXTEND = 2430;
        /// <summary>
        /// Move caret to before first visible character on line. If already
        /// there move to first character on line. In either case, extend
        /// rectangular selection to new caret position.
        /// </summary>
        public const int SCI_VCHOMERECTEXTEND = 2431;
        /// <summary>
        /// Move caret to last position on line, extending rectangular
        /// selection to new caret position.
        /// </summary>
        public const int SCI_LINEENDRECTEXTEND = 2432;
        /// <summary>
        /// Move caret one page up, extending rectangular selection to new
        /// caret position.
        /// </summary>
        public const int SCI_PAGEUPRECTEXTEND = 2433;
        /// <summary>
        /// Move caret one page down, extending rectangular selection to new
        /// caret position.
        /// </summary>
        public const int SCI_PAGEDOWNRECTEXTEND = 2434;
        /// <summary>
        /// Move caret to top of page, or one page up if already at top of
        /// page.
        /// </summary>
        public const int SCI_STUTTEREDPAGEUP = 2435;
        /// <summary>
        /// Move caret to top of page, or one page up if already at top of
        /// page, extending selection to new caret position.
        /// </summary>
        public const int SCI_STUTTEREDPAGEUPEXTEND = 2436;
        /// <summary>
        /// Move caret to bottom of page, or one page down if already at bottom
        /// of page.
        /// </summary>
        public const int SCI_STUTTEREDPAGEDOWN = 2437;
        /// <summary>
        /// Move caret to bottom of page, or one page down if already at bottom
        /// of page, extending selection to new caret position.
        /// </summary>
        public const int SCI_STUTTEREDPAGEDOWNEXTEND = 2438;
        /// <summary>
        /// Move caret left one word, position cursor at end of word.
        /// </summary>
        public const int SCI_WORDLEFTEND = 2439;
        /// <summary>
        /// Move caret left one word, position cursor at end of word, extending
        /// selection to new caret position.
        /// </summary>
        public const int SCI_WORDLEFTENDEXTEND = 2440;
        /// <summary>
        /// Move caret right one word, position cursor at end of word.
        /// </summary>
        public const int SCI_WORDRIGHTEND = 2441;
        /// <summary>
        /// Move caret right one word, position cursor at end of word,
        /// extending selection to new caret position.
        /// </summary>
        public const int SCI_WORDRIGHTENDEXTEND = 2442;
        /// <summary>
        /// Set the set of characters making up whitespace for when moving or
        /// selecting by word. Should be called after SetWordChars.
        /// </summary>
        public const int SCI_SETWHITESPACECHARS = 2443;
        /// <summary>
        /// Get the set of characters making up whitespace for when moving or
        /// selecting by word.
        /// </summary>
        public const int SCI_GETWHITESPACECHARS = 2647;
        /// <summary>
        /// Set the set of characters making up punctuation characters Should
        /// be called after SetWordChars.
        /// </summary>
        public const int SCI_SETPUNCTUATIONCHARS = 2648;
        /// <summary>
        /// Get the set of characters making up punctuation characters.
        /// </summary>
        public const int SCI_GETPUNCTUATIONCHARS = 2649;
        /// <summary>
        /// Reset the set of characters for whitespace and word characters to
        /// the defaults.
        /// </summary>
        public const int SCI_SETCHARSDEFAULT = 2444;
        /// <summary>
        /// Get currently selected item position in the auto-completion list.
        /// </summary>
        public const int SCI_AUTOCGETCURRENT = 2445;
        /// <summary>
        /// Get currently selected item text in the auto-completion list
        /// Returns the length of the item text Result is NUL-terminated.
        /// </summary>
        public const int SCI_AUTOCGETCURRENTTEXT = 2610;
        /// <summary>
        /// Set auto-completion case insensitive behaviour to either prefer
        /// case-sensitive matches or have no preference.
        /// </summary>
        public const int SCI_AUTOCSETCASEINSENSITIVEBEHAVIOUR = 2634;
        /// <summary>
        /// Get auto-completion case insensitive behaviour.
        /// </summary>
        public const int SCI_AUTOCGETCASEINSENSITIVEBEHAVIOUR = 2635;
        /// <summary>
        /// Change the effect of autocompleting when there are multiple
        /// selections.
        /// </summary>
        public const int SCI_AUTOCSETMULTI = 2636;
        /// <summary>
        /// Retrieve the effect of autocompleting when there are multiple
        /// selections.
        /// </summary>
        public const int SCI_AUTOCGETMULTI = 2637;
        /// <summary>
        /// Set the way autocompletion lists are ordered.
        /// </summary>
        public const int SCI_AUTOCSETORDER = 2660;
        /// <summary>
        /// Get the way autocompletion lists are ordered.
        /// </summary>
        public const int SCI_AUTOCGETORDER = 2661;
        /// <summary>
        /// Enlarge the document to a particular size of text bytes.
        /// </summary>
        public const int SCI_ALLOCATE = 2446;
        /// <summary>
        /// Returns the target converted to UTF8. Return the length in bytes.
        /// </summary>
        public const int SCI_TARGETASUTF8 = 2447;
        /// <summary>
        /// Set the length of the utf8 argument for calling EncodedFromUTF8.
        /// Set to -1 and the string will be measured to the first nul.
        /// </summary>
        public const int SCI_SETLENGTHFORENCODE = 2448;
        /// <summary>
        /// Translates a UTF8 string into the document encoding. Return the
        /// length of the result in bytes. On error return 0.
        /// </summary>
        public const int SCI_ENCODEDFROMUTF8 = 2449;
        /// <summary>
        /// Find the position of a column on a line taking into account tabs
        /// and multi-byte characters. If beyond end of line, return line end
        /// position.
        /// </summary>
        public const int SCI_FINDCOLUMN = 2456;
        /// <summary>
        /// Can the caret preferred x position only be changed by explicit
        /// movement commands?.
        /// </summary>
        public const int SCI_GETCARETSTICKY = 2457;
        /// <summary>
        /// Stop the caret preferred x position changing when the user types.
        /// </summary>
        public const int SCI_SETCARETSTICKY = 2458;
        /// <summary>
        /// Switch between sticky and non-sticky: meant to be bound to a key.
        /// </summary>
        public const int SCI_TOGGLECARETSTICKY = 2459;
        /// <summary>
        /// Enable/Disable convert-on-paste for line endings.
        /// </summary>
        public const int SCI_SETPASTECONVERTENDINGS = 2467;
        /// <summary>
        /// Get convert-on-paste setting.
        /// </summary>
        public const int SCI_GETPASTECONVERTENDINGS = 2468;
        /// <summary>
        /// Duplicate the selection. If selection empty duplicate the line
        /// containing the caret.
        /// </summary>
        public const int SCI_SELECTIONDUPLICATE = 2469;
        /// <summary>
        /// Set background alpha of the caret line.
        /// </summary>
        public const int SCI_SETCARETLINEBACKALPHA = 2470;
        /// <summary>
        /// Get the background alpha of the caret line.
        /// </summary>
        public const int SCI_GETCARETLINEBACKALPHA = 2471;
        /// <summary>
        /// Set the style of the caret to be drawn.
        /// </summary>
        public const int SCI_SETCARETSTYLE = 2512;
        /// <summary>
        /// Returns the current style of the caret.
        /// </summary>
        public const int SCI_GETCARETSTYLE = 2513;
        /// <summary>
        /// Set the indicator used for IndicatorFillRange and
        /// IndicatorClearRange.
        /// </summary>
        public const int SCI_SETINDICATORCURRENT = 2500;
        /// <summary>
        /// Get the current indicator.
        /// </summary>
        public const int SCI_GETINDICATORCURRENT = 2501;
        /// <summary>
        /// Set the value used for IndicatorFillRange.
        /// </summary>
        public const int SCI_SETINDICATORVALUE = 2502;
        /// <summary>
        /// Get the current indicator value.
        /// </summary>
        public const int SCI_GETINDICATORVALUE = 2503;
        /// <summary>
        /// Turn a indicator on over a range.
        /// </summary>
        public const int SCI_INDICATORFILLRANGE = 2504;
        /// <summary>
        /// Turn a indicator off over a range.
        /// </summary>
        public const int SCI_INDICATORCLEARRANGE = 2505;
        /// <summary>
        /// Are any indicators present at pos?.
        /// </summary>
        public const int SCI_INDICATORALLONFOR = 2506;
        /// <summary>
        /// What value does a particular indicator have at a position?.
        /// </summary>
        public const int SCI_INDICATORVALUEAT = 2507;
        /// <summary>
        /// Where does a particular indicator start?.
        /// </summary>
        public const int SCI_INDICATORSTART = 2508;
        /// <summary>
        /// Where does a particular indicator end?.
        /// </summary>
        public const int SCI_INDICATOREND = 2509;
        /// <summary>
        /// Set number of entries in position cache.
        /// </summary>
        public const int SCI_SETPOSITIONCACHE = 2514;
        /// <summary>
        /// How many entries are allocated to the position cache?.
        /// </summary>
        public const int SCI_GETPOSITIONCACHE = 2515;
        /// <summary>
        /// Copy the selection, if selection empty copy the line with the
        /// caret.
        /// </summary>
        public const int SCI_COPYALLOWLINE = 2519;
        /// <summary>
        /// Compact the document buffer and return a read-only pointer to the
        /// characters in the document.
        /// </summary>
        public const int SCI_GETCHARACTERPOINTER = 2520;
        /// <summary>
        /// Return a read-only pointer to a range of characters in the
        /// document. May move the gap so that the range is contiguous, but
        /// will only move up to lengthRange bytes.
        /// </summary>
        public const int SCI_GETRANGEPOINTER = 2643;
        /// <summary>
        /// Return a position which, to avoid performance costs, should not be
        /// within the range of a call to GetRangePointer.
        /// </summary>
        public const int SCI_GETGAPPOSITION = 2644;
        /// <summary>
        /// Set the alpha fill colour of the given indicator.
        /// </summary>
        public const int SCI_INDICSETALPHA = 2523;
        /// <summary>
        /// Get the alpha fill colour of the given indicator.
        /// </summary>
        public const int SCI_INDICGETALPHA = 2524;
        /// <summary>
        /// Set the alpha outline colour of the given indicator.
        /// </summary>
        public const int SCI_INDICSETOUTLINEALPHA = 2558;
        /// <summary>
        /// Get the alpha outline colour of the given indicator.
        /// </summary>
        public const int SCI_INDICGETOUTLINEALPHA = 2559;
        /// <summary>
        /// Set extra ascent for each line.
        /// </summary>
        public const int SCI_SETEXTRAASCENT = 2525;
        /// <summary>
        /// Get extra ascent for each line.
        /// </summary>
        public const int SCI_GETEXTRAASCENT = 2526;
        /// <summary>
        /// Set extra descent for each line.
        /// </summary>
        public const int SCI_SETEXTRADESCENT = 2527;
        /// <summary>
        /// Get extra descent for each line.
        /// </summary>
        public const int SCI_GETEXTRADESCENT = 2528;
        /// <summary>
        /// Which symbol was defined for markerNumber with MarkerDefine.
        /// </summary>
        public const int SCI_MARKERSYMBOLDEFINED = 2529;
        /// <summary>
        /// Set the text in the text margin for a line.
        /// </summary>
        public const int SCI_MARGINSETTEXT = 2530;
        /// <summary>
        /// Get the text in the text margin for a line.
        /// </summary>
        public const int SCI_MARGINGETTEXT = 2531;
        /// <summary>
        /// Set the style number for the text margin for a line.
        /// </summary>
        public const int SCI_MARGINSETSTYLE = 2532;
        /// <summary>
        /// Get the style number for the text margin for a line.
        /// </summary>
        public const int SCI_MARGINGETSTYLE = 2533;
        /// <summary>
        /// Set the style in the text margin for a line.
        /// </summary>
        public const int SCI_MARGINSETSTYLES = 2534;
        /// <summary>
        /// Get the styles in the text margin for a line.
        /// </summary>
        public const int SCI_MARGINGETSTYLES = 2535;
        /// <summary>
        /// Clear the margin text on all lines.
        /// </summary>
        public const int SCI_MARGINTEXTCLEARALL = 2536;
        /// <summary>
        /// Get the start of the range of style numbers used for margin text.
        /// </summary>
        public const int SCI_MARGINSETSTYLEOFFSET = 2537;
        /// <summary>
        /// Get the start of the range of style numbers used for margin text.
        /// </summary>
        public const int SCI_MARGINGETSTYLEOFFSET = 2538;
        /// <summary>
        /// Set the margin options.
        /// </summary>
        public const int SCI_SETMARGINOPTIONS = 2539;
        /// <summary>
        /// Get the margin options.
        /// </summary>
        public const int SCI_GETMARGINOPTIONS = 2557;
        /// <summary>
        /// Set the annotation text for a line.
        /// </summary>
        public const int SCI_ANNOTATIONSETTEXT = 2540;
        /// <summary>
        /// Get the annotation text for a line.
        /// </summary>
        public const int SCI_ANNOTATIONGETTEXT = 2541;
        /// <summary>
        /// Set the style number for the annotations for a line.
        /// </summary>
        public const int SCI_ANNOTATIONSETSTYLE = 2542;
        /// <summary>
        /// Get the style number for the annotations for a line.
        /// </summary>
        public const int SCI_ANNOTATIONGETSTYLE = 2543;
        /// <summary>
        /// Set the annotation styles for a line.
        /// </summary>
        public const int SCI_ANNOTATIONSETSTYLES = 2544;
        /// <summary>
        /// Get the annotation styles for a line.
        /// </summary>
        public const int SCI_ANNOTATIONGETSTYLES = 2545;
        /// <summary>
        /// Get the number of annotation lines for a line.
        /// </summary>
        public const int SCI_ANNOTATIONGETLINES = 2546;
        /// <summary>
        /// Clear the annotations from all lines.
        /// </summary>
        public const int SCI_ANNOTATIONCLEARALL = 2547;
        /// <summary>
        /// Set the visibility for the annotations for a view.
        /// </summary>
        public const int SCI_ANNOTATIONSETVISIBLE = 2548;
        /// <summary>
        /// Get the visibility for the annotations for a view.
        /// </summary>
        public const int SCI_ANNOTATIONGETVISIBLE = 2549;
        /// <summary>
        /// Get the start of the range of style numbers used for annotations.
        /// </summary>
        public const int SCI_ANNOTATIONSETSTYLEOFFSET = 2550;
        /// <summary>
        /// Get the start of the range of style numbers used for annotations.
        /// </summary>
        public const int SCI_ANNOTATIONGETSTYLEOFFSET = 2551;
        /// <summary>
        /// Release all extended (&gt;255) style numbers.
        /// </summary>
        public const int SCI_RELEASEALLEXTENDEDSTYLES = 2552;
        /// <summary>
        /// Allocate some extended (&gt;255) style numbers and return the
        /// start of the range.
        /// </summary>
        public const int SCI_ALLOCATEEXTENDEDSTYLES = 2553;
        /// <summary>
        /// Add a container action to the undo stack.
        /// </summary>
        public const int SCI_ADDUNDOACTION = 2560;
        /// <summary>
        /// Find the position of a character from a point within the window.
        /// </summary>
        public const int SCI_CHARPOSITIONFROMPOINT = 2561;
        /// <summary>
        /// Find the position of a character from a point within the window.
        /// Return INVALID_POSITION if not close to text.
        /// </summary>
        public const int SCI_CHARPOSITIONFROMPOINTCLOSE = 2562;
        /// <summary>
        /// Set whether switching to rectangular mode while selecting with the
        /// mouse is allowed.
        /// </summary>
        public const int SCI_SETMOUSESELECTIONRECTANGULARSWITCH = 2668;
        /// <summary>
        /// Whether switching to rectangular mode while selecting with the
        /// mouse is allowed.
        /// </summary>
        public const int SCI_GETMOUSESELECTIONRECTANGULARSWITCH = 2669;
        /// <summary>
        /// Set whether multiple selections can be made.
        /// </summary>
        public const int SCI_SETMULTIPLESELECTION = 2563;
        /// <summary>
        /// Whether multiple selections can be made.
        /// </summary>
        public const int SCI_GETMULTIPLESELECTION = 2564;
        /// <summary>
        /// Set whether typing can be performed into multiple selections.
        /// </summary>
        public const int SCI_SETADDITIONALSELECTIONTYPING = 2565;
        /// <summary>
        /// Whether typing can be performed into multiple selections.
        /// </summary>
        public const int SCI_GETADDITIONALSELECTIONTYPING = 2566;
        /// <summary>
        /// Set whether additional carets will blink.
        /// </summary>
        public const int SCI_SETADDITIONALCARETSBLINK = 2567;
        /// <summary>
        /// Whether additional carets will blink.
        /// </summary>
        public const int SCI_GETADDITIONALCARETSBLINK = 2568;
        /// <summary>
        /// Set whether additional carets are visible.
        /// </summary>
        public const int SCI_SETADDITIONALCARETSVISIBLE = 2608;
        /// <summary>
        /// Whether additional carets are visible.
        /// </summary>
        public const int SCI_GETADDITIONALCARETSVISIBLE = 2609;
        /// <summary>
        /// Retrieve the current tab draw mode. Returns one of SCTD_*
        /// constants.
        /// </summary>
        public const int SCI_GETTABDRAWMODE = 2698;
        /// <summary>
        /// Set how tabs are drawn when visible.
        /// </summary>
        public const int SCI_SETTABDRAWMODE = 2699;
        /// <summary>
        /// How many selections are there?.
        /// </summary>
        public const int SCI_GETSELECTIONS = 2570;
        /// <summary>
        /// Is every selected range empty?.
        /// </summary>
        public const int SCI_GETSELECTIONEMPTY = 2650;
        /// <summary>
        /// Clear selections to a single empty stream selection.
        /// </summary>
        public const int SCI_CLEARSELECTIONS = 2571;
        /// <summary>
        /// Set a simple selection.
        /// </summary>
        public const int SCI_SETSELECTION = 2572;
        /// <summary>
        /// Add a selection.
        /// </summary>
        public const int SCI_ADDSELECTION = 2573;
        /// <summary>
        /// Drop one selection.
        /// </summary>
        public const int SCI_DROPSELECTIONN = 2671;
        /// <summary>
        /// Set the main selection.
        /// </summary>
        public const int SCI_SETMAINSELECTION = 2574;
        /// <summary>
        /// Which selection is the main selection.
        /// </summary>
        public const int SCI_GETMAINSELECTION = 2575;
        /// <summary>
        /// Set the caret position of the nth selection.
        /// </summary>
        public const int SCI_SETSELECTIONNCARET = 2576;
        /// <summary>
        /// Return the caret position of the nth selection.
        /// </summary>
        public const int SCI_GETSELECTIONNCARET = 2577;
        /// <summary>
        /// Set the anchor position of the nth selection.
        /// </summary>
        public const int SCI_SETSELECTIONNANCHOR = 2578;
        /// <summary>
        /// Return the anchor position of the nth selection.
        /// </summary>
        public const int SCI_GETSELECTIONNANCHOR = 2579;
        /// <summary>
        /// Set the virtual space of the caret of the nth selection.
        /// </summary>
        public const int SCI_SETSELECTIONNCARETVIRTUALSPACE = 2580;
        /// <summary>
        /// Return the virtual space of the caret of the nth selection.
        /// </summary>
        public const int SCI_GETSELECTIONNCARETVIRTUALSPACE = 2581;
        /// <summary>
        /// Set the virtual space of the anchor of the nth selection.
        /// </summary>
        public const int SCI_SETSELECTIONNANCHORVIRTUALSPACE = 2582;
        /// <summary>
        /// Return the virtual space of the anchor of the nth selection.
        /// </summary>
        public const int SCI_GETSELECTIONNANCHORVIRTUALSPACE = 2583;
        /// <summary>
        /// Sets the position that starts the selection - this becomes the
        /// anchor.
        /// </summary>
        public const int SCI_SETSELECTIONNSTART = 2584;
        /// <summary>
        /// Returns the position at the start of the selection.
        /// </summary>
        public const int SCI_GETSELECTIONNSTART = 2585;
        /// <summary>
        /// Sets the position that ends the selection - this becomes the
        /// currentPosition.
        /// </summary>
        public const int SCI_SETSELECTIONNEND = 2586;
        /// <summary>
        /// Returns the position at the end of the selection.
        /// </summary>
        public const int SCI_GETSELECTIONNEND = 2587;
        /// <summary>
        /// Set the caret position of the rectangular selection.
        /// </summary>
        public const int SCI_SETRECTANGULARSELECTIONCARET = 2588;
        /// <summary>
        /// Return the caret position of the rectangular selection.
        /// </summary>
        public const int SCI_GETRECTANGULARSELECTIONCARET = 2589;
        /// <summary>
        /// Set the anchor position of the rectangular selection.
        /// </summary>
        public const int SCI_SETRECTANGULARSELECTIONANCHOR = 2590;
        /// <summary>
        /// Return the anchor position of the rectangular selection.
        /// </summary>
        public const int SCI_GETRECTANGULARSELECTIONANCHOR = 2591;
        /// <summary>
        /// Set the virtual space of the caret of the rectangular selection.
        /// </summary>
        public const int SCI_SETRECTANGULARSELECTIONCARETVIRTUALSPACE = 2592;
        /// <summary>
        /// Return the virtual space of the caret of the rectangular selection.
        /// </summary>
        public const int SCI_GETRECTANGULARSELECTIONCARETVIRTUALSPACE = 2593;
        /// <summary>
        /// Set the virtual space of the anchor of the rectangular selection.
        /// </summary>
        public const int SCI_SETRECTANGULARSELECTIONANCHORVIRTUALSPACE = 2594;
        /// <summary>
        /// Return the virtual space of the anchor of the rectangular
        /// selection.
        /// </summary>
        public const int SCI_GETRECTANGULARSELECTIONANCHORVIRTUALSPACE = 2595;
        /// <summary>
        /// Set options for virtual space behaviour.
        /// </summary>
        public const int SCI_SETVIRTUALSPACEOPTIONS = 2596;
        /// <summary>
        /// Return options for virtual space behaviour.
        /// </summary>
        public const int SCI_GETVIRTUALSPACEOPTIONS = 2597;
        /// <summary>
        /// The SCI_SETRECTANGULARSELECTIONMODIFIER constant.
        /// </summary>
        public const int SCI_SETRECTANGULARSELECTIONMODIFIER = 2598;
        /// <summary>
        /// Get the modifier key used for rectangular selection.
        /// </summary>
        public const int SCI_GETRECTANGULARSELECTIONMODIFIER = 2599;
        /// <summary>
        /// Set the foreground colour of additional selections. Must have
        /// previously called SetSelFore with non-zero first argument for this
        /// to have an effect.
        /// </summary>
        public const int SCI_SETADDITIONALSELFORE = 2600;
        /// <summary>
        /// Set the background colour of additional selections. Must have
        /// previously called SetSelBack with non-zero first argument for this
        /// to have an effect.
        /// </summary>
        public const int SCI_SETADDITIONALSELBACK = 2601;
        /// <summary>
        /// Set the alpha of the selection.
        /// </summary>
        public const int SCI_SETADDITIONALSELALPHA = 2602;
        /// <summary>
        /// Get the alpha of the selection.
        /// </summary>
        public const int SCI_GETADDITIONALSELALPHA = 2603;
        /// <summary>
        /// Set the foreground colour of additional carets.
        /// </summary>
        public const int SCI_SETADDITIONALCARETFORE = 2604;
        /// <summary>
        /// Get the foreground colour of additional carets.
        /// </summary>
        public const int SCI_GETADDITIONALCARETFORE = 2605;
        /// <summary>
        /// Set the main selection to the next selection.
        /// </summary>
        public const int SCI_ROTATESELECTION = 2606;
        /// <summary>
        /// Swap that caret and anchor of the main selection.
        /// </summary>
        public const int SCI_SWAPMAINANCHORCARET = 2607;
        /// <summary>
        /// Add the next occurrence of the main selection to the set of
        /// selections as main. If the current selection is empty then select
        /// word around caret.
        /// </summary>
        public const int SCI_MULTIPLESELECTADDNEXT = 2688;
        /// <summary>
        /// Add each occurrence of the main selection in the target to the set
        /// of selections. If the current selection is empty then select word
        /// around caret.
        /// </summary>
        public const int SCI_MULTIPLESELECTADDEACH = 2689;
        /// <summary>
        /// Indicate that the internal state of a lexer has changed over a
        /// range and therefore there may be a need to redraw.
        /// </summary>
        public const int SCI_CHANGELEXERSTATE = 2617;
        /// <summary>
        /// Find the next line at or after lineStart that is a contracted fold
        /// header line. Return -1 when no more lines.
        /// </summary>
        public const int SCI_CONTRACTEDFOLDNEXT = 2618;
        /// <summary>
        /// Centre current line in window.
        /// </summary>
        public const int SCI_VERTICALCENTRECARET = 2619;
        /// <summary>
        /// Move the selected lines up one line, shifting the line above after
        /// the selection.
        /// </summary>
        public const int SCI_MOVESELECTEDLINESUP = 2620;
        /// <summary>
        /// Move the selected lines down one line, shifting the line below
        /// before the selection.
        /// </summary>
        public const int SCI_MOVESELECTEDLINESDOWN = 2621;
        /// <summary>
        /// Set the identifier reported as idFrom in notification messages.
        /// </summary>
        public const int SCI_SETIDENTIFIER = 2622;
        /// <summary>
        /// Get the identifier.
        /// </summary>
        public const int SCI_GETIDENTIFIER = 2623;
        /// <summary>
        /// Set the width for future RGBA image data.
        /// </summary>
        public const int SCI_RGBAIMAGESETWIDTH = 2624;
        /// <summary>
        /// Set the height for future RGBA image data.
        /// </summary>
        public const int SCI_RGBAIMAGESETHEIGHT = 2625;
        /// <summary>
        /// Set the scale factor in percent for future RGBA image data.
        /// </summary>
        public const int SCI_RGBAIMAGESETSCALE = 2651;
        /// <summary>
        /// Define a marker from RGBA data. It has the width and height from
        /// RGBAImageSetWidth/Height.
        /// </summary>
        public const int SCI_MARKERDEFINERGBAIMAGE = 2626;
        /// <summary>
        /// Register an RGBA image for use in autocompletion lists. It has the
        /// width and height from RGBAImageSetWidth/Height.
        /// </summary>
        public const int SCI_REGISTERRGBAIMAGE = 2627;
        /// <summary>
        /// Scroll to start of document.
        /// </summary>
        public const int SCI_SCROLLTOSTART = 2628;
        /// <summary>
        /// Scroll to end of document.
        /// </summary>
        public const int SCI_SCROLLTOEND = 2629;
        /// <summary>
        /// Set the technology used.
        /// </summary>
        public const int SCI_SETTECHNOLOGY = 2630;
        /// <summary>
        /// Get the tech.
        /// </summary>
        public const int SCI_GETTECHNOLOGY = 2631;
        /// <summary>
        /// Create an ILoader*.
        /// </summary>
        public const int SCI_CREATELOADER = 2632;
        /// <summary>
        /// On macOS, show a find indicator.
        /// </summary>
        public const int SCI_FINDINDICATORSHOW = 2640;
        /// <summary>
        /// On macOS, flash a find indicator, then fade out.
        /// </summary>
        public const int SCI_FINDINDICATORFLASH = 2641;
        /// <summary>
        /// On macOS, hide the find indicator.
        /// </summary>
        public const int SCI_FINDINDICATORHIDE = 2642;
        /// <summary>
        /// Move caret to before first visible character on display line. If
        /// already there move to first character on display line.
        /// </summary>
        public const int SCI_VCHOMEDISPLAY = 2652;
        /// <summary>
        /// Like VCHomeDisplay but extending selection to new caret position.
        /// </summary>
        public const int SCI_VCHOMEDISPLAYEXTEND = 2653;
        /// <summary>
        /// Is the caret line always visible?.
        /// </summary>
        public const int SCI_GETCARETLINEVISIBLEALWAYS = 2654;
        /// <summary>
        /// Sets the caret line to always visible.
        /// </summary>
        public const int SCI_SETCARETLINEVISIBLEALWAYS = 2655;
        /// <summary>
        /// Set the line end types that the application wants to use. May not
        /// be used if incompatible with lexer or encoding.
        /// </summary>
        public const int SCI_SETLINEENDTYPESALLOWED = 2656;
        /// <summary>
        /// Get the line end types currently allowed.
        /// </summary>
        public const int SCI_GETLINEENDTYPESALLOWED = 2657;
        /// <summary>
        /// Get the line end types currently recognised. May be a subset of the
        /// allowed types due to lexer limitation.
        /// </summary>
        public const int SCI_GETLINEENDTYPESACTIVE = 2658;
        /// <summary>
        /// Set the way a character is drawn.
        /// </summary>
        public const int SCI_SETREPRESENTATION = 2665;
        /// <summary>
        /// Get the way a character is drawn. Result is NUL-terminated.
        /// </summary>
        public const int SCI_GETREPRESENTATION = 2666;
        /// <summary>
        /// Remove a character representation.
        /// </summary>
        public const int SCI_CLEARREPRESENTATION = 2667;
        /// <summary>
        /// Sets both the start and end of the target in one call.
        /// </summary>
        public const int SCI_SETTARGETRANGE = 2686;
        /// <summary>
        /// Retrieve the text in the target.
        /// </summary>
        public const int SCI_GETTARGETTEXT = 2687;
        /// <summary>
        /// Sets limits to idle styling.
        /// </summary>
        public const int SCI_SETIDLESTYLING = 2692;
        /// <summary>
        /// Retrieve the limits to idle styling.
        /// </summary>
        public const int SCI_GETIDLESTYLING = 2693;
        /// <summary>
        /// Add a new vertical edge to the view.
        /// </summary>
        public const int SCI_MULTIEDGEADDLINE = 2694;
        /// <summary>
        /// Clear all vertical edges.
        /// </summary>
        public const int SCI_MULTIEDGECLEARALL = 2695;
        /// <summary>
        /// Set whether the mouse wheel can be active outside the window.
        /// </summary>
        public const int SCI_SETMOUSEWHEELCAPTURES = 2696;
        /// <summary>
        /// Get whether mouse wheel can be active outside the window.
        /// </summary>
        public const int SCI_GETMOUSEWHEELCAPTURES = 2697;
        /// <summary>
        /// Switch a header line between expanded and contracted and show some
        /// text after the line.
        /// </summary>
        public const int SCI_TOGGLEFOLDSHOWTEXT = 2700;
        /// <summary>
        /// Set the style of fold display text.
        /// </summary>
        public const int SCI_FOLDDISPLAYTEXTSETSTYLE = 2701;
        /// <summary>
        /// Start notifying the container of all key presses and commands.
        /// </summary>
        public const int SCI_STARTRECORD = 3001;
        /// <summary>
        /// Stop notifying the container of all key presses and commands.
        /// </summary>
        public const int SCI_STOPRECORD = 3002;
        /// <summary>
        /// The SCI_SETLEXER constant.
        /// </summary>
        public const int SCI_SETLEXER = 4001;
        /// <summary>
        /// Set the lexer from an ILexer*.
        /// </summary>
        public const int SCI_SETILEXER = 4033;
        /// <summary>
        /// Retrieve the lexing language of the document.
        /// </summary>
        public const int SCI_GETLEXER = 4002;
        /// <summary>
        /// Colourise a segment of the document using the current lexing
        /// language.
        /// </summary>
        public const int SCI_COLOURISE = 4003;
        /// <summary>
        /// Set up a value that may be used by a lexer for some optional
        /// feature.
        /// </summary>
        public const int SCI_SETPROPERTY = 4004;
        /// <summary>
        /// Set up the key words used by the lexer.
        /// </summary>
        public const int SCI_SETKEYWORDS = 4005;
        /// <summary>
        /// The SCI_SETLEXERLANGUAGE constant.
        /// </summary>
        public const int SCI_SETLEXERLANGUAGE = 4006;
        /// <summary>
        /// The SCI_LOADLEXERLIBRARY constant.
        /// </summary>
        public const int SCI_LOADLEXERLIBRARY = 4007;
        /// <summary>
        /// Retrieve a "property" value previously set with SetProperty. Result
        /// is NUL-terminated.
        /// </summary>
        public const int SCI_GETPROPERTY = 4008;
        /// <summary>
        /// Retrieve a "property" value previously set with SetProperty, with
        /// "$()" variable replacement on returned buffer. Result is
        /// NUL-terminated.
        /// </summary>
        public const int SCI_GETPROPERTYEXPANDED = 4009;
        /// <summary>
        /// Retrieve a "property" value previously set with SetProperty,
        /// interpreted as an int AFTER any "$()" variable replacement.
        /// </summary>
        public const int SCI_GETPROPERTYINT = 4010;
        // public const int SCI_GETSTYLEBITSNEEDED = 4011;
        /// <summary>
        /// Retrieve the name of the lexer. Return the length of the text.
        /// Result is NUL-terminated.
        /// </summary>
        public const int SCI_GETLEXERLANGUAGE = 4012;
        /// <summary>
        /// For private communication between an application and a known lexer.
        /// </summary>
        public const int SCI_PRIVATELEXERCALL = 4013;
        /// <summary>
        /// Retrieve a '\n' separated list of properties understood by the
        /// current lexer. Result is NUL-terminated.
        /// </summary>
        public const int SCI_PROPERTYNAMES = 4014;
        /// <summary>
        /// Retrieve the type of a property.
        /// </summary>
        public const int SCI_PROPERTYTYPE = 4015;
        /// <summary>
        /// Describe a property. Result is NUL-terminated.
        /// </summary>
        public const int SCI_DESCRIBEPROPERTY = 4016;
        /// <summary>
        /// Retrieve a '\n' separated list of descriptions of the keyword sets
        /// understood by the current lexer. Result is NUL-terminated.
        /// </summary>
        public const int SCI_DESCRIBEKEYWORDSETS = 4017;
        /// <summary>
        /// Bit set of LineEndType enumertion for which line ends beyond the
        /// standard LF, CR, and CRLF are supported by the lexer.
        /// </summary>
        public const int SCI_GETLINEENDTYPESSUPPORTED = 4018;
        /// <summary>
        /// Allocate a set of sub styles for a particular base style, returning
        /// start of range.
        /// </summary>
        public const int SCI_ALLOCATESUBSTYLES = 4020;
        /// <summary>
        /// The starting style number for the sub styles associated with a base
        /// style.
        /// </summary>
        public const int SCI_GETSUBSTYLESSTART = 4021;
        /// <summary>
        /// The number of sub styles associated with a base style.
        /// </summary>
        public const int SCI_GETSUBSTYLESLENGTH = 4022;
        /// <summary>
        /// For a sub style, return the base style, else return the argument.
        /// </summary>
        public const int SCI_GETSTYLEFROMSUBSTYLE = 4027;
        /// <summary>
        /// For a secondary style, return the primary style, else return the
        /// argument.
        /// </summary>
        public const int SCI_GETPRIMARYSTYLEFROMSTYLE = 4028;
        /// <summary>
        /// Free allocated sub styles.
        /// </summary>
        public const int SCI_FREESUBSTYLES = 4023;
        /// <summary>
        /// Set the identifiers that are shown in a particular style.
        /// </summary>
        public const int SCI_SETIDENTIFIERS = 4024;
        /// <summary>
        /// Where styles are duplicated by a feature such as active/inactive
        /// code return the distance between the two types.
        /// </summary>
        public const int SCI_DISTANCETOSECONDARYSTYLES = 4025;
        /// <summary>
        /// Get the set of base styles that can be extended with sub styles
        /// Result is NUL-terminated.
        /// </summary>
        public const int SCI_GETSUBSTYLEBASES = 4026;
        // public const int SCI_SETUSEPALETTE = 2039;
        // public const int SCI_GETUSEPALETTE = 2139;

        // Keys
        /// <summary>
        /// The SCK_DOWN constant.
        /// </summary>
        public const int SCK_DOWN = 300;
        /// <summary>
        /// The SCK_UP constant.
        /// </summary>
        public const int SCK_UP = 301;
        /// <summary>
        /// The SCK_LEFT constant.
        /// </summary>
        public const int SCK_LEFT = 302;
        /// <summary>
        /// The SCK_RIGHT constant.
        /// </summary>
        public const int SCK_RIGHT = 303;
        /// <summary>
        /// The SCK_HOME constant.
        /// </summary>
        public const int SCK_HOME = 304;
        /// <summary>
        /// The SCK_END constant.
        /// </summary>
        public const int SCK_END = 305;
        /// <summary>
        /// The SCK_PRIOR constant.
        /// </summary>
        public const int SCK_PRIOR = 306;
        /// <summary>
        /// The SCK_NEXT constant.
        /// </summary>
        public const int SCK_NEXT = 307;
        /// <summary>
        /// The SCK_DELETE constant.
        /// </summary>
        public const int SCK_DELETE = 308;
        /// <summary>
        /// The SCK_INSERT constant.
        /// </summary>
        public const int SCK_INSERT = 309;
        /// <summary>
        /// The SCK_ESCAPE constant.
        /// </summary>
        public const int SCK_ESCAPE = 7;
        /// <summary>
        /// The SCK_BACK constant.
        /// </summary>
        public const int SCK_BACK = 8;
        /// <summary>
        /// The SCK_TAB constant.
        /// </summary>
        public const int SCK_TAB = 9;
        /// <summary>
        /// The SCK_RETURN constant.
        /// </summary>
        public const int SCK_RETURN = 13;
        /// <summary>
        /// The SCK_ADD constant.
        /// </summary>
        public const int SCK_ADD = 310;
        /// <summary>
        /// The SCK_SUBTRACT constant.
        /// </summary>
        public const int SCK_SUBTRACT = 311;
        /// <summary>
        /// The SCK_DIVIDE constant.
        /// </summary>
        public const int SCK_DIVIDE = 312;
        /// <summary>
        /// The SCK_WIN constant.
        /// </summary>
        public const int SCK_WIN = 313;
        /// <summary>
        /// The SCK_RWIN constant.
        /// </summary>
        public const int SCK_RWIN = 314;
        /// <summary>
        /// The SCK_MENU constant.
        /// </summary>
        public const int SCK_MENU = 315;

        // Notifications
        /// <summary>
        /// The SCN_STYLENEEDED constant.
        /// </summary>
        public const int SCN_STYLENEEDED = 2000;
        /// <summary>
        /// The SCN_CHARADDED constant.
        /// </summary>
        public const int SCN_CHARADDED = 2001;
        /// <summary>
        /// The SCN_SAVEPOINTREACHED constant.
        /// </summary>
        public const int SCN_SAVEPOINTREACHED = 2002;
        /// <summary>
        /// The SCN_SAVEPOINTLEFT constant.
        /// </summary>
        public const int SCN_SAVEPOINTLEFT = 2003;
        /// <summary>
        /// The SCN_MODIFYATTEMPTRO constant.
        /// </summary>
        public const int SCN_MODIFYATTEMPTRO = 2004;
        /// <summary>
        /// GTK Specific to work around focus and accelerator problems:.
        /// </summary>
        public const int SCN_KEY = 2005;
        /// <summary>
        /// The SCN_DOUBLECLICK constant.
        /// </summary>
        public const int SCN_DOUBLECLICK = 2006;
        /// <summary>
        /// The SCN_UPDATEUI constant.
        /// </summary>
        public const int SCN_UPDATEUI = 2007;
        /// <summary>
        /// The SCN_MODIFIED constant.
        /// </summary>
        public const int SCN_MODIFIED = 2008;
        /// <summary>
        /// The SCN_MACRORECORD constant.
        /// </summary>
        public const int SCN_MACRORECORD = 2009;
        /// <summary>
        /// The SCN_MARGINCLICK constant.
        /// </summary>
        public const int SCN_MARGINCLICK = 2010;
        /// <summary>
        /// The SCN_NEEDSHOWN constant.
        /// </summary>
        public const int SCN_NEEDSHOWN = 2011;
        /// <summary>
        /// The SCN_PAINTED constant.
        /// </summary>
        public const int SCN_PAINTED = 2013;
        /// <summary>
        /// The SCN_USERLISTSELECTION constant.
        /// </summary>
        public const int SCN_USERLISTSELECTION = 2014;
        /// <summary>
        /// The SCN_URIDROPPED constant.
        /// </summary>
        public const int SCN_URIDROPPED = 2015;
        /// <summary>
        /// The SCN_DWELLSTART constant.
        /// </summary>
        public const int SCN_DWELLSTART = 2016;
        /// <summary>
        /// The SCN_DWELLEND constant.
        /// </summary>
        public const int SCN_DWELLEND = 2017;
        /// <summary>
        /// The SCN_ZOOM constant.
        /// </summary>
        public const int SCN_ZOOM = 2018;
        /// <summary>
        /// The SCN_HOTSPOTCLICK constant.
        /// </summary>
        public const int SCN_HOTSPOTCLICK = 2019;
        /// <summary>
        /// The SCN_HOTSPOTDOUBLECLICK constant.
        /// </summary>
        public const int SCN_HOTSPOTDOUBLECLICK = 2020;
        /// <summary>
        /// The SCN_CALLTIPCLICK constant.
        /// </summary>
        public const int SCN_CALLTIPCLICK = 2021;
        /// <summary>
        /// The SCN_AUTOCSELECTION constant.
        /// </summary>
        public const int SCN_AUTOCSELECTION = 2022;
        /// <summary>
        /// The SCN_INDICATORCLICK constant.
        /// </summary>
        public const int SCN_INDICATORCLICK = 2023;
        /// <summary>
        /// The SCN_INDICATORRELEASE constant.
        /// </summary>
        public const int SCN_INDICATORRELEASE = 2024;
        /// <summary>
        /// The SCN_AUTOCCANCELLED constant.
        /// </summary>
        public const int SCN_AUTOCCANCELLED = 2025;
        /// <summary>
        /// The SCN_AUTOCCHARDELETED constant.
        /// </summary>
        public const int SCN_AUTOCCHARDELETED = 2026;
        /// <summary>
        /// The SCN_HOTSPOTRELEASECLICK constant.
        /// </summary>
        public const int SCN_HOTSPOTRELEASECLICK = 2027;
        /// <summary>
        /// The SCN_FOCUSIN constant.
        /// </summary>
        public const int SCN_FOCUSIN = 2028;
        /// <summary>
        /// The SCN_FOCUSOUT constant.
        /// </summary>
        public const int SCN_FOCUSOUT = 2029;
        /// <summary>
        /// The SCN_AUTOCCOMPLETED constant.
        /// </summary>
        public const int SCN_AUTOCCOMPLETED = 2030;
        /// <summary>
        /// The SCN_MARGINRIGHTCLICK constant.
        /// </summary>
        public const int SCN_MARGINRIGHTCLICK = 2031;

        // Popup
        /// <summary>
        /// The SC_POPUP_NEVER constant.
        /// </summary>
        public const int SC_POPUP_NEVER = 0;
        /// <summary>
        /// The SC_POPUP_ALL constant.
        /// </summary>
        public const int SC_POPUP_ALL = 1;
        /// <summary>
        /// The SC_POPUP_TEXT constant.
        /// </summary>
        public const int SC_POPUP_TEXT = 2;

        // Line wrapping
        /// <summary>
        /// The SC_WRAP_NONE constant.
        /// </summary>
        public const int SC_WRAP_NONE = 0;
        /// <summary>
        /// The SC_WRAP_WORD constant.
        /// </summary>
        public const int SC_WRAP_WORD = 1;
        /// <summary>
        /// The SC_WRAP_CHAR constant.
        /// </summary>
        public const int SC_WRAP_CHAR = 2;
        /// <summary>
        /// The SC_WRAP_WHITESPACE constant.
        /// </summary>
        public const int SC_WRAP_WHITESPACE = 3;

        /// <summary>
        /// The SC_WRAPVISUALFLAG_NONE constant.
        /// </summary>
        public const int SC_WRAPVISUALFLAG_NONE = 0x0000;
        /// <summary>
        /// The SC_WRAPVISUALFLAG_END constant.
        /// </summary>
        public const int SC_WRAPVISUALFLAG_END = 0x0001;
        /// <summary>
        /// The SC_WRAPVISUALFLAG_START constant.
        /// </summary>
        public const int SC_WRAPVISUALFLAG_START = 0x0002;
        /// <summary>
        /// The SC_WRAPVISUALFLAG_MARGIN constant.
        /// </summary>
        public const int SC_WRAPVISUALFLAG_MARGIN = 0x0004;

        /// <summary>
        /// The SC_WRAPVISUALFLAGLOC_DEFAULT constant.
        /// </summary>
        public const int SC_WRAPVISUALFLAGLOC_DEFAULT = 0x0000;
        /// <summary>
        /// The SC_WRAPVISUALFLAGLOC_END_BY_TEXT constant.
        /// </summary>
        public const int SC_WRAPVISUALFLAGLOC_END_BY_TEXT = 0x0001;
        /// <summary>
        /// The SC_WRAPVISUALFLAGLOC_START_BY_TEXT constant.
        /// </summary>
        public const int SC_WRAPVISUALFLAGLOC_START_BY_TEXT = 0x0002;

        /// <summary>
        /// The SC_WRAPINDENT_FIXED constant.
        /// </summary>
        public const int SC_WRAPINDENT_FIXED = 0;
        /// <summary>
        /// The SC_WRAPINDENT_SAME constant.
        /// </summary>
        public const int SC_WRAPINDENT_SAME = 1;
        /// <summary>
        /// The SC_WRAPINDENT_INDENT constant.
        /// </summary>
        public const int SC_WRAPINDENT_INDENT = 2;

        // Virtual space
        /// <summary>
        /// The SCVS_NONE constant.
        /// </summary>
        public const int SCVS_NONE = 0;
        /// <summary>
        /// The SCVS_RECTANGULARSELECTION constant.
        /// </summary>
        public const int SCVS_RECTANGULARSELECTION = 1;
        /// <summary>
        /// The SCVS_USERACCESSIBLE constant.
        /// </summary>
        public const int SCVS_USERACCESSIBLE = 2;
        /// <summary>
        /// The SCVS_NOWRAPLINESTART constant.
        /// </summary>
        public const int SCVS_NOWRAPLINESTART = 4;

        // Styles constants
        /// <summary>
        /// Styles in range 32..39 are predefined for parts of the UI and are
        /// not used as normal styles.
        /// </summary>
        public const int STYLE_DEFAULT = 32;
        /// <summary>
        /// Styles in range 32..39 are predefined for parts of the UI and are
        /// not used as normal styles.
        /// </summary>
        public const int STYLE_LINENUMBER = 33;
        /// <summary>
        /// Styles in range 32..39 are predefined for parts of the UI and are
        /// not used as normal styles.
        /// </summary>
        public const int STYLE_BRACELIGHT = 34;
        /// <summary>
        /// Styles in range 32..39 are predefined for parts of the UI and are
        /// not used as normal styles.
        /// </summary>
        public const int STYLE_BRACEBAD = 35;
        /// <summary>
        /// Styles in range 32..39 are predefined for parts of the UI and are
        /// not used as normal styles.
        /// </summary>
        public const int STYLE_CONTROLCHAR = 36;
        /// <summary>
        /// Styles in range 32..39 are predefined for parts of the UI and are
        /// not used as normal styles.
        /// </summary>
        public const int STYLE_INDENTGUIDE = 37;
        /// <summary>
        /// Styles in range 32..39 are predefined for parts of the UI and are
        /// not used as normal styles.
        /// </summary>
        public const int STYLE_CALLTIP = 38;
        /// <summary>
        /// Styles in range 32..39 are predefined for parts of the UI and are
        /// not used as normal styles.
        /// </summary>
        public const int STYLE_FOLDDISPLAYTEXT = 39;
        /// <summary>
        /// Styles in range 32..39 are predefined for parts of the UI and are
        /// not used as normal styles.
        /// </summary>
        public const int STYLE_LASTPREDEFINED = 39;
        /// <summary>
        /// Styles in range 32..39 are predefined for parts of the UI and are
        /// not used as normal styles.
        /// </summary>
        public const int STYLE_MAX = 255;

        /// <summary>
        /// The SC_FONT_SIZE_MULTIPLIER constant.
        /// </summary>
        public const int SC_FONT_SIZE_MULTIPLIER = 100;
        /// <summary>
        /// The SC_CASE_MIXED constant.
        /// </summary>
        public const int SC_CASE_MIXED = 0;
        /// <summary>
        /// The SC_CASE_UPPER constant.
        /// </summary>
        public const int SC_CASE_UPPER = 1;
        /// <summary>
        /// The SC_CASE_LOWER constant.
        /// </summary>
        public const int SC_CASE_LOWER = 2;
        /// <summary>
        /// The SC_CASE_CAMEL constant.
        /// </summary>
        public const int SC_CASE_CAMEL = 3;

        // Technology
        /// <summary>
        /// The SC_TECHNOLOGY_DEFAULT constant.
        /// </summary>
        public const int SC_TECHNOLOGY_DEFAULT = 0;
        /// <summary>
        /// The SC_TECHNOLOGY_DIRECTWRITE constant.
        /// </summary>
        public const int SC_TECHNOLOGY_DIRECTWRITE = 1;
        /// <summary>
        /// The SC_TECHNOLOGY_DIRECTWRITERETAIN constant.
        /// </summary>
        public const int SC_TECHNOLOGY_DIRECTWRITERETAIN = 2;
        /// <summary>
        /// The SC_TECHNOLOGY_DIRECTWRITEDC constant.
        /// </summary>
        public const int SC_TECHNOLOGY_DIRECTWRITEDC = 3;

        // Tab draw
        /// <summary>
        /// The SCTD_LONGARROW constant.
        /// </summary>
        public const int SCTD_LONGARROW = 0;
        /// <summary>
        /// The SCTD_STRIKEOUT constant.
        /// </summary>
        public const int SCTD_STRIKEOUT = 1;

        // Undo
        /// <summary>
        /// The UNDO_MAY_COALESCE constant.
        /// </summary>
        public const int UNDO_MAY_COALESCE = 1;

        // Whitespace
        /// <summary>
        /// The SCWS_INVISIBLE constant.
        /// </summary>
        public const int SCWS_INVISIBLE = 0;
        /// <summary>
        /// The SCWS_VISIBLEALWAYS constant.
        /// </summary>
        public const int SCWS_VISIBLEALWAYS = 1;
        /// <summary>
        /// The SCWS_VISIBLEAFTERINDENT constant.
        /// </summary>
        public const int SCWS_VISIBLEAFTERINDENT = 2;
        /// <summary>
        /// The SCWS_VISIBLEONLYININDENT constant.
        /// </summary>
        public const int SCWS_VISIBLEONLYININDENT = 3;

        // Window messages
        /// <summary>
        /// The WM_CREATE constant.
        /// </summary>
        public const int WM_CREATE = 0x0001;
        /// <summary>
        /// The WM_DESTROY constant.
        /// </summary>
        public const int WM_DESTROY = 0x0002;
        /// <summary>
        /// The WM_SETCURSOR constant.
        /// </summary>
        public const int WM_SETCURSOR = 0x0020;
        /// <summary>
        /// The WM_NOTIFY constant.
        /// </summary>
        public const int WM_NOTIFY = 0x004E;
        /// <summary>
        /// The WM_LBUTTONDBLCLK constant.
        /// </summary>
        public const int WM_LBUTTONDBLCLK = 0x0203;
        /// <summary>
        /// The WM_RBUTTONDBLCLK constant.
        /// </summary>
        public const int WM_RBUTTONDBLCLK = 0x0206;
        /// <summary>
        /// The WM_MBUTTONDBLCLK constant.
        /// </summary>
        public const int WM_MBUTTONDBLCLK = 0x0209;
        /// <summary>
        /// The WM_XBUTTONDBLCLK constant.
        /// </summary>
        public const int WM_XBUTTONDBLCLK = 0x020D;
        /// <summary>
        /// The WM_MOUSEWHEEL constant.
        /// </summary>
        public const int WM_MOUSEWHEEL = 0x020A;
        /// <summary>
        /// The WM_USER constant.
        /// </summary>
        public const int WM_USER = 0x0400;
        /// <summary>
        /// The WM_REFLECT constant.
        /// </summary>
        public const int WM_REFLECT = WM_USER + 0x1C00;

        // Window styles
        /// <summary>
        /// The WS_BORDER constant.
        /// </summary>
        public const int WS_BORDER = 0x00800000;
        /// <summary>
        /// The WS_EX_CLIENTEDGE constant.
        /// </summary>
        public const int WS_EX_CLIENTEDGE = 0x00000200;

        #endregion Constants

        #region Lexer Constants

        // Lexers
        /// <summary>
        /// For SciLexer.h.
        /// </summary>
        public const int SCLEX_CONTAINER = 0;
        /// <summary>
        /// For SciLexer.h.
        /// </summary>
        public const int SCLEX_NULL = 1;
        /// <summary>
        /// For SciLexer.h.
        /// </summary>
        public const int SCLEX_PYTHON = 2;
        /// <summary>
        /// For SciLexer.h.
        /// </summary>
        public const int SCLEX_CPP = 3;
        /// <summary>
        /// For SciLexer.h.
        /// </summary>
        public const int SCLEX_HTML = 4;
        /// <summary>
        /// For SciLexer.h.
        /// </summary>
        public const int SCLEX_XML = 5;
        /// <summary>
        /// For SciLexer.h.
        /// </summary>
        public const int SCLEX_PERL = 6;
        /// <summary>
        /// For SciLexer.h.
        /// </summary>
        public const int SCLEX_SQL = 7;
        /// <summary>
        /// For SciLexer.h.
        /// </summary>
        public const int SCLEX_VB = 8;
        /// <summary>
        /// For SciLexer.h.
        /// </summary>
        public const int SCLEX_PROPERTIES = 9;
        /// <summary>
        /// For SciLexer.h.
        /// </summary>
        public const int SCLEX_ERRORLIST = 10;
        /// <summary>
        /// For SciLexer.h.
        /// </summary>
        public const int SCLEX_MAKEFILE = 11;
        /// <summary>
        /// For SciLexer.h.
        /// </summary>
        public const int SCLEX_BATCH = 12;
        /// <summary>
        /// For SciLexer.h.
        /// </summary>
        public const int SCLEX_XCODE = 13;
        /// <summary>
        /// For SciLexer.h.
        /// </summary>
        public const int SCLEX_LATEX = 14;
        /// <summary>
        /// For SciLexer.h.
        /// </summary>
        public const int SCLEX_LUA = 15;
        /// <summary>
        /// For SciLexer.h.
        /// </summary>
        public const int SCLEX_DIFF = 16;
        /// <summary>
        /// For SciLexer.h.
        /// </summary>
        public const int SCLEX_CONF = 17;
        /// <summary>
        /// For SciLexer.h.
        /// </summary>
        public const int SCLEX_PASCAL = 18;
        /// <summary>
        /// For SciLexer.h.
        /// </summary>
        public const int SCLEX_AVE = 19;
        /// <summary>
        /// For SciLexer.h.
        /// </summary>
        public const int SCLEX_ADA = 20;
        /// <summary>
        /// For SciLexer.h.
        /// </summary>
        public const int SCLEX_LISP = 21;
        /// <summary>
        /// For SciLexer.h.
        /// </summary>
        public const int SCLEX_RUBY = 22;
        /// <summary>
        /// For SciLexer.h.
        /// </summary>
        public const int SCLEX_EIFFEL = 23;
        /// <summary>
        /// For SciLexer.h.
        /// </summary>
        public const int SCLEX_EIFFELKW = 24;
        /// <summary>
        /// For SciLexer.h.
        /// </summary>
        public const int SCLEX_TCL = 25;
        /// <summary>
        /// For SciLexer.h.
        /// </summary>
        public const int SCLEX_NNCRONTAB = 26;
        /// <summary>
        /// For SciLexer.h.
        /// </summary>
        public const int SCLEX_BULLANT = 27;
        /// <summary>
        /// For SciLexer.h.
        /// </summary>
        public const int SCLEX_VBSCRIPT = 28;
        /// <summary>
        /// For SciLexer.h.
        /// </summary>
        public const int SCLEX_BAAN = 31;
        /// <summary>
        /// For SciLexer.h.
        /// </summary>
        public const int SCLEX_MATLAB = 32;
        /// <summary>
        /// For SciLexer.h.
        /// </summary>
        public const int SCLEX_SCRIPTOL = 33;
        /// <summary>
        /// For SciLexer.h.
        /// </summary>
        public const int SCLEX_ASM = 34;
        /// <summary>
        /// For SciLexer.h.
        /// </summary>
        public const int SCLEX_CPPNOCASE = 35;
        /// <summary>
        /// For SciLexer.h.
        /// </summary>
        public const int SCLEX_FORTRAN = 36;
        /// <summary>
        /// For SciLexer.h.
        /// </summary>
        public const int SCLEX_F77 = 37;
        /// <summary>
        /// For SciLexer.h.
        /// </summary>
        public const int SCLEX_CSS = 38;
        /// <summary>
        /// For SciLexer.h.
        /// </summary>
        public const int SCLEX_POV = 39;
        /// <summary>
        /// For SciLexer.h.
        /// </summary>
        public const int SCLEX_LOUT = 40;
        /// <summary>
        /// For SciLexer.h.
        /// </summary>
        public const int SCLEX_ESCRIPT = 41;
        /// <summary>
        /// For SciLexer.h.
        /// </summary>
        public const int SCLEX_PS = 42;
        /// <summary>
        /// For SciLexer.h.
        /// </summary>
        public const int SCLEX_NSIS = 43;
        /// <summary>
        /// For SciLexer.h.
        /// </summary>
        public const int SCLEX_MMIXAL = 44;
        /// <summary>
        /// For SciLexer.h.
        /// </summary>
        public const int SCLEX_CLW = 45;
        /// <summary>
        /// For SciLexer.h.
        /// </summary>
        public const int SCLEX_CLWNOCASE = 46;
        /// <summary>
        /// For SciLexer.h.
        /// </summary>
        public const int SCLEX_LOT = 47;
        /// <summary>
        /// For SciLexer.h.
        /// </summary>
        public const int SCLEX_YAML = 48;
        /// <summary>
        /// For SciLexer.h.
        /// </summary>
        public const int SCLEX_TEX = 49;
        /// <summary>
        /// For SciLexer.h.
        /// </summary>
        public const int SCLEX_METAPOST = 50;
        /// <summary>
        /// For SciLexer.h.
        /// </summary>
        public const int SCLEX_POWERBASIC = 51;
        /// <summary>
        /// For SciLexer.h.
        /// </summary>
        public const int SCLEX_FORTH = 52;
        /// <summary>
        /// For SciLexer.h.
        /// </summary>
        public const int SCLEX_ERLANG = 53;
        /// <summary>
        /// For SciLexer.h.
        /// </summary>
        public const int SCLEX_OCTAVE = 54;
        /// <summary>
        /// For SciLexer.h.
        /// </summary>
        public const int SCLEX_MSSQL = 55;
        /// <summary>
        /// For SciLexer.h.
        /// </summary>
        public const int SCLEX_VERILOG = 56;
        /// <summary>
        /// For SciLexer.h.
        /// </summary>
        public const int SCLEX_KIX = 57;
        /// <summary>
        /// For SciLexer.h.
        /// </summary>
        public const int SCLEX_GUI4CLI = 58;
        /// <summary>
        /// For SciLexer.h.
        /// </summary>
        public const int SCLEX_SPECMAN = 59;
        /// <summary>
        /// For SciLexer.h.
        /// </summary>
        public const int SCLEX_AU3 = 60;
        /// <summary>
        /// For SciLexer.h.
        /// </summary>
        public const int SCLEX_APDL = 61;
        /// <summary>
        /// For SciLexer.h.
        /// </summary>
        public const int SCLEX_BASH = 62;
        /// <summary>
        /// For SciLexer.h.
        /// </summary>
        public const int SCLEX_ASN1 = 63;
        /// <summary>
        /// For SciLexer.h.
        /// </summary>
        public const int SCLEX_VHDL = 64;
        /// <summary>
        /// For SciLexer.h.
        /// </summary>
        public const int SCLEX_CAML = 65;
        /// <summary>
        /// For SciLexer.h.
        /// </summary>
        public const int SCLEX_BLITZBASIC = 66;
        /// <summary>
        /// For SciLexer.h.
        /// </summary>
        public const int SCLEX_PUREBASIC = 67;
        /// <summary>
        /// For SciLexer.h.
        /// </summary>
        public const int SCLEX_HASKELL = 68;
        /// <summary>
        /// For SciLexer.h.
        /// </summary>
        public const int SCLEX_PHPSCRIPT = 69;
        /// <summary>
        /// For SciLexer.h.
        /// </summary>
        public const int SCLEX_TADS3 = 70;
        /// <summary>
        /// For SciLexer.h.
        /// </summary>
        public const int SCLEX_REBOL = 71;
        /// <summary>
        /// For SciLexer.h.
        /// </summary>
        public const int SCLEX_SMALLTALK = 72;
        /// <summary>
        /// For SciLexer.h.
        /// </summary>
        public const int SCLEX_FLAGSHIP = 73;
        /// <summary>
        /// For SciLexer.h.
        /// </summary>
        public const int SCLEX_CSOUND = 74;
        /// <summary>
        /// For SciLexer.h.
        /// </summary>
        public const int SCLEX_FREEBASIC = 75;
        /// <summary>
        /// For SciLexer.h.
        /// </summary>
        public const int SCLEX_INNOSETUP = 76;
        /// <summary>
        /// For SciLexer.h.
        /// </summary>
        public const int SCLEX_OPAL = 77;
        /// <summary>
        /// For SciLexer.h.
        /// </summary>
        public const int SCLEX_SPICE = 78;
        /// <summary>
        /// For SciLexer.h.
        /// </summary>
        public const int SCLEX_D = 79;
        /// <summary>
        /// For SciLexer.h.
        /// </summary>
        public const int SCLEX_CMAKE = 80;
        /// <summary>
        /// For SciLexer.h.
        /// </summary>
        public const int SCLEX_GAP = 81;
        /// <summary>
        /// For SciLexer.h.
        /// </summary>
        public const int SCLEX_PLM = 82;
        /// <summary>
        /// For SciLexer.h.
        /// </summary>
        public const int SCLEX_PROGRESS = 83;
        /// <summary>
        /// For SciLexer.h.
        /// </summary>
        public const int SCLEX_ABAQUS = 84;
        /// <summary>
        /// For SciLexer.h.
        /// </summary>
        public const int SCLEX_ASYMPTOTE = 85;
        /// <summary>
        /// For SciLexer.h.
        /// </summary>
        public const int SCLEX_R = 86;
        /// <summary>
        /// For SciLexer.h.
        /// </summary>
        public const int SCLEX_MAGIK = 87;
        /// <summary>
        /// For SciLexer.h.
        /// </summary>
        public const int SCLEX_POWERSHELL = 88;
        /// <summary>
        /// For SciLexer.h.
        /// </summary>
        public const int SCLEX_MYSQL = 89;
        /// <summary>
        /// For SciLexer.h.
        /// </summary>
        public const int SCLEX_PO = 90;
        /// <summary>
        /// For SciLexer.h.
        /// </summary>
        public const int SCLEX_TAL = 91;
        /// <summary>
        /// For SciLexer.h.
        /// </summary>
        public const int SCLEX_COBOL = 92;
        /// <summary>
        /// For SciLexer.h.
        /// </summary>
        public const int SCLEX_TACL = 93;
        /// <summary>
        /// For SciLexer.h.
        /// </summary>
        public const int SCLEX_SORCUS = 94;
        /// <summary>
        /// For SciLexer.h.
        /// </summary>
        public const int SCLEX_POWERPRO = 95;
        /// <summary>
        /// For SciLexer.h.
        /// </summary>
        public const int SCLEX_NIMROD = 96;
        /// <summary>
        /// For SciLexer.h.
        /// </summary>
        public const int SCLEX_SML = 97;
        /// <summary>
        /// For SciLexer.h.
        /// </summary>
        public const int SCLEX_MARKDOWN = 98;
        /// <summary>
        /// For SciLexer.h.
        /// </summary>
        public const int SCLEX_TXT2TAGS = 99;
        /// <summary>
        /// For SciLexer.h.
        /// </summary>
        public const int SCLEX_A68K = 100;
        /// <summary>
        /// For SciLexer.h.
        /// </summary>
        public const int SCLEX_MODULA = 101;
        /// <summary>
        /// For SciLexer.h.
        /// </summary>
        public const int SCLEX_COFFEESCRIPT = 102;
        /// <summary>
        /// For SciLexer.h.
        /// </summary>
        public const int SCLEX_TCMD = 103;
        /// <summary>
        /// For SciLexer.h.
        /// </summary>
        public const int SCLEX_AVS = 104;
        /// <summary>
        /// For SciLexer.h.
        /// </summary>
        public const int SCLEX_ECL = 105;
        /// <summary>
        /// For SciLexer.h.
        /// </summary>
        public const int SCLEX_OSCRIPT = 106;
        /// <summary>
        /// For SciLexer.h.
        /// </summary>
        public const int SCLEX_VISUALPROLOG = 107;
        /// <summary>
        /// For SciLexer.h.
        /// </summary>
        public const int SCLEX_LITERATEHASKELL = 108;
        /// <summary>
        /// For SciLexer.h.
        /// </summary>
        public const int SCLEX_STTXT = 109;
        /// <summary>
        /// For SciLexer.h.
        /// </summary>
        public const int SCLEX_KVIRC = 110;
        /// <summary>
        /// For SciLexer.h.
        /// </summary>
        public const int SCLEX_RUST = 111;
        /// <summary>
        /// For SciLexer.h.
        /// </summary>
        public const int SCLEX_DMAP = 112;
        /// <summary>
        /// For SciLexer.h.
        /// </summary>
        public const int SCLEX_AS = 113;
        /// <summary>
        /// For SciLexer.h.
        /// </summary>
        public const int SCLEX_DMIS = 114;
        /// <summary>
        /// For SciLexer.h.
        /// </summary>
        public const int SCLEX_REGISTRY = 115;
        /// <summary>
        /// For SciLexer.h.
        /// </summary>
        public const int SCLEX_BIBTEX = 116;
        /// <summary>
        /// For SciLexer.h.
        /// </summary>
        public const int SCLEX_SREC = 117;
        /// <summary>
        /// For SciLexer.h.
        /// </summary>
        public const int SCLEX_IHEX = 118;
        /// <summary>
        /// For SciLexer.h.
        /// </summary>
        public const int SCLEX_TEHEX = 119;
        /// <summary>
        /// For SciLexer.h.
        /// </summary>
        public const int SCLEX_JSON = 120;
        /// <summary>
        /// When a lexer specifies its language as SCLEX_AUTOMATIC it receives
        /// a value assigned in sequence from SCLEX_AUTOMATIC+1.
        /// </summary>
        public const int SCLEX_AUTOMATIC = 1000;

        // Ada
        /// <summary>
        /// Lexical states for SCLEX_ADA.
        /// </summary>
        public const int SCE_ADA_DEFAULT = 0;
        /// <summary>
        /// Lexical states for SCLEX_ADA.
        /// </summary>
        public const int SCE_ADA_WORD = 1;
        /// <summary>
        /// Lexical states for SCLEX_ADA.
        /// </summary>
        public const int SCE_ADA_IDENTIFIER = 2;
        /// <summary>
        /// Lexical states for SCLEX_ADA.
        /// </summary>
        public const int SCE_ADA_NUMBER = 3;
        /// <summary>
        /// Lexical states for SCLEX_ADA.
        /// </summary>
        public const int SCE_ADA_DELIMITER = 4;
        /// <summary>
        /// Lexical states for SCLEX_ADA.
        /// </summary>
        public const int SCE_ADA_CHARACTER = 5;
        /// <summary>
        /// Lexical states for SCLEX_ADA.
        /// </summary>
        public const int SCE_ADA_CHARACTEREOL = 6;
        /// <summary>
        /// Lexical states for SCLEX_ADA.
        /// </summary>
        public const int SCE_ADA_STRING = 7;
        /// <summary>
        /// Lexical states for SCLEX_ADA.
        /// </summary>
        public const int SCE_ADA_STRINGEOL = 8;
        /// <summary>
        /// Lexical states for SCLEX_ADA.
        /// </summary>
        public const int SCE_ADA_LABEL = 9;
        /// <summary>
        /// Lexical states for SCLEX_ADA.
        /// </summary>
        public const int SCE_ADA_COMMENTLINE = 10;
        /// <summary>
        /// Lexical states for SCLEX_ADA.
        /// </summary>
        public const int SCE_ADA_ILLEGAL = 11;

        // ASM
        /// <summary>
        /// Lexical states for SCLEX_ASM, SCLEX_AS.
        /// </summary>
        public const int SCE_ASM_DEFAULT = 0;
        /// <summary>
        /// Lexical states for SCLEX_ASM, SCLEX_AS.
        /// </summary>
        public const int SCE_ASM_COMMENT = 1;
        /// <summary>
        /// Lexical states for SCLEX_ASM, SCLEX_AS.
        /// </summary>
        public const int SCE_ASM_NUMBER = 2;
        /// <summary>
        /// Lexical states for SCLEX_ASM, SCLEX_AS.
        /// </summary>
        public const int SCE_ASM_STRING = 3;
        /// <summary>
        /// Lexical states for SCLEX_ASM, SCLEX_AS.
        /// </summary>
        public const int SCE_ASM_OPERATOR = 4;
        /// <summary>
        /// Lexical states for SCLEX_ASM, SCLEX_AS.
        /// </summary>
        public const int SCE_ASM_IDENTIFIER = 5;
        /// <summary>
        /// Lexical states for SCLEX_ASM, SCLEX_AS.
        /// </summary>
        public const int SCE_ASM_CPUINSTRUCTION = 6;
        /// <summary>
        /// Lexical states for SCLEX_ASM, SCLEX_AS.
        /// </summary>
        public const int SCE_ASM_MATHINSTRUCTION = 7;
        /// <summary>
        /// Lexical states for SCLEX_ASM, SCLEX_AS.
        /// </summary>
        public const int SCE_ASM_REGISTER = 8;
        /// <summary>
        /// Lexical states for SCLEX_ASM, SCLEX_AS.
        /// </summary>
        public const int SCE_ASM_DIRECTIVE = 9;
        /// <summary>
        /// Lexical states for SCLEX_ASM, SCLEX_AS.
        /// </summary>
        public const int SCE_ASM_DIRECTIVEOPERAND = 10;
        /// <summary>
        /// Lexical states for SCLEX_ASM, SCLEX_AS.
        /// </summary>
        public const int SCE_ASM_COMMENTBLOCK = 11;
        /// <summary>
        /// Lexical states for SCLEX_ASM, SCLEX_AS.
        /// </summary>
        public const int SCE_ASM_CHARACTER = 12;
        /// <summary>
        /// Lexical states for SCLEX_ASM, SCLEX_AS.
        /// </summary>
        public const int SCE_ASM_STRINGEOL = 13;
        /// <summary>
        /// Lexical states for SCLEX_ASM, SCLEX_AS.
        /// </summary>
        public const int SCE_ASM_EXTINSTRUCTION = 14;
        /// <summary>
        /// Lexical states for SCLEX_ASM, SCLEX_AS.
        /// </summary>
        public const int SCE_ASM_COMMENTDIRECTIVE = 15;

        // Batch
        /// <summary>
        /// Lexical states for SCLEX_BATCH.
        /// </summary>
        public const int SCE_BAT_DEFAULT = 0;
        /// <summary>
        /// Lexical states for SCLEX_BATCH.
        /// </summary>
        public const int SCE_BAT_COMMENT = 1;
        /// <summary>
        /// Lexical states for SCLEX_BATCH.
        /// </summary>
        public const int SCE_BAT_WORD = 2;
        /// <summary>
        /// Lexical states for SCLEX_BATCH.
        /// </summary>
        public const int SCE_BAT_LABEL = 3;
        /// <summary>
        /// Lexical states for SCLEX_BATCH.
        /// </summary>
        public const int SCE_BAT_HIDE = 4;
        /// <summary>
        /// Lexical states for SCLEX_BATCH.
        /// </summary>
        public const int SCE_BAT_COMMAND = 5;
        /// <summary>
        /// Lexical states for SCLEX_BATCH.
        /// </summary>
        public const int SCE_BAT_IDENTIFIER = 6;
        /// <summary>
        /// Lexical states for SCLEX_BATCH.
        /// </summary>
        public const int SCE_BAT_OPERATOR =  7;

        // CPP
        /// <summary>
        /// Lexical states for SCLEX_CPP Lexical states for SCLEX_BULLANT
        /// Lexical states for SCLEX_TACL Lexical states for SCLEX_TAL.
        /// </summary>
        public const int SCE_C_DEFAULT = 0;
        /// <summary>
        /// Lexical states for SCLEX_CPP Lexical states for SCLEX_BULLANT
        /// Lexical states for SCLEX_TACL Lexical states for SCLEX_TAL.
        /// </summary>
        public const int SCE_C_COMMENT = 1;
        /// <summary>
        /// Lexical states for SCLEX_CPP Lexical states for SCLEX_BULLANT
        /// Lexical states for SCLEX_TACL Lexical states for SCLEX_TAL.
        /// </summary>
        public const int SCE_C_COMMENTLINE = 2;
        /// <summary>
        /// Lexical states for SCLEX_CPP Lexical states for SCLEX_BULLANT
        /// Lexical states for SCLEX_TACL Lexical states for SCLEX_TAL.
        /// </summary>
        public const int SCE_C_COMMENTDOC = 3;
        /// <summary>
        /// Lexical states for SCLEX_CPP Lexical states for SCLEX_BULLANT
        /// Lexical states for SCLEX_TACL Lexical states for SCLEX_TAL.
        /// </summary>
        public const int SCE_C_NUMBER = 4;
        /// <summary>
        /// Lexical states for SCLEX_CPP Lexical states for SCLEX_BULLANT
        /// Lexical states for SCLEX_TACL Lexical states for SCLEX_TAL.
        /// </summary>
        public const int SCE_C_WORD = 5;
        /// <summary>
        /// Lexical states for SCLEX_CPP Lexical states for SCLEX_BULLANT
        /// Lexical states for SCLEX_TACL Lexical states for SCLEX_TAL.
        /// </summary>
        public const int SCE_C_STRING = 6;
        /// <summary>
        /// Lexical states for SCLEX_CPP Lexical states for SCLEX_BULLANT
        /// Lexical states for SCLEX_TACL Lexical states for SCLEX_TAL.
        /// </summary>
        public const int SCE_C_CHARACTER = 7;
        /// <summary>
        /// Lexical states for SCLEX_CPP Lexical states for SCLEX_BULLANT
        /// Lexical states for SCLEX_TACL Lexical states for SCLEX_TAL.
        /// </summary>
        public const int SCE_C_UUID = 8;
        /// <summary>
        /// Lexical states for SCLEX_CPP Lexical states for SCLEX_BULLANT
        /// Lexical states for SCLEX_TACL Lexical states for SCLEX_TAL.
        /// </summary>
        public const int SCE_C_PREPROCESSOR = 9;
        /// <summary>
        /// Lexical states for SCLEX_CPP Lexical states for SCLEX_BULLANT
        /// Lexical states for SCLEX_TACL Lexical states for SCLEX_TAL.
        /// </summary>
        public const int SCE_C_OPERATOR = 10;
        /// <summary>
        /// Lexical states for SCLEX_CPP Lexical states for SCLEX_BULLANT
        /// Lexical states for SCLEX_TACL Lexical states for SCLEX_TAL.
        /// </summary>
        public const int SCE_C_IDENTIFIER = 11;
        /// <summary>
        /// Lexical states for SCLEX_CPP Lexical states for SCLEX_BULLANT
        /// Lexical states for SCLEX_TACL Lexical states for SCLEX_TAL.
        /// </summary>
        public const int SCE_C_STRINGEOL = 12;
        /// <summary>
        /// Lexical states for SCLEX_CPP Lexical states for SCLEX_BULLANT
        /// Lexical states for SCLEX_TACL Lexical states for SCLEX_TAL.
        /// </summary>
        public const int SCE_C_VERBATIM = 13;
        /// <summary>
        /// Lexical states for SCLEX_CPP Lexical states for SCLEX_BULLANT
        /// Lexical states for SCLEX_TACL Lexical states for SCLEX_TAL.
        /// </summary>
        public const int SCE_C_REGEX = 14;
        /// <summary>
        /// Lexical states for SCLEX_CPP Lexical states for SCLEX_BULLANT
        /// Lexical states for SCLEX_TACL Lexical states for SCLEX_TAL.
        /// </summary>
        public const int SCE_C_COMMENTLINEDOC = 15;
        /// <summary>
        /// Lexical states for SCLEX_CPP Lexical states for SCLEX_BULLANT
        /// Lexical states for SCLEX_TACL Lexical states for SCLEX_TAL.
        /// </summary>
        public const int SCE_C_WORD2 = 16;
        /// <summary>
        /// Lexical states for SCLEX_CPP Lexical states for SCLEX_BULLANT
        /// Lexical states for SCLEX_TACL Lexical states for SCLEX_TAL.
        /// </summary>
        public const int SCE_C_COMMENTDOCKEYWORD = 17;
        /// <summary>
        /// Lexical states for SCLEX_CPP Lexical states for SCLEX_BULLANT
        /// Lexical states for SCLEX_TACL Lexical states for SCLEX_TAL.
        /// </summary>
        public const int SCE_C_COMMENTDOCKEYWORDERROR = 18;
        /// <summary>
        /// Lexical states for SCLEX_CPP Lexical states for SCLEX_BULLANT
        /// Lexical states for SCLEX_TACL Lexical states for SCLEX_TAL.
        /// </summary>
        public const int SCE_C_GLOBALCLASS = 19;
        /// <summary>
        /// Lexical states for SCLEX_CPP Lexical states for SCLEX_BULLANT
        /// Lexical states for SCLEX_TACL Lexical states for SCLEX_TAL.
        /// </summary>
        public const int SCE_C_STRINGRAW = 20;
        /// <summary>
        /// Lexical states for SCLEX_CPP Lexical states for SCLEX_BULLANT
        /// Lexical states for SCLEX_TACL Lexical states for SCLEX_TAL.
        /// </summary>
        public const int SCE_C_TRIPLEVERBATIM = 21;
        /// <summary>
        /// Lexical states for SCLEX_CPP Lexical states for SCLEX_BULLANT
        /// Lexical states for SCLEX_TACL Lexical states for SCLEX_TAL.
        /// </summary>
        public const int SCE_C_HASHQUOTEDSTRING = 22;
        /// <summary>
        /// Lexical states for SCLEX_CPP Lexical states for SCLEX_BULLANT
        /// Lexical states for SCLEX_TACL Lexical states for SCLEX_TAL.
        /// </summary>
        public const int SCE_C_PREPROCESSORCOMMENT = 23;
        /// <summary>
        /// Lexical states for SCLEX_CPP Lexical states for SCLEX_BULLANT
        /// Lexical states for SCLEX_TACL Lexical states for SCLEX_TAL.
        /// </summary>
        public const int SCE_C_PREPROCESSORCOMMENTDOC = 24;
        /// <summary>
        /// Lexical states for SCLEX_CPP Lexical states for SCLEX_BULLANT
        /// Lexical states for SCLEX_TACL Lexical states for SCLEX_TAL.
        /// </summary>
        public const int SCE_C_USERLITERAL = 25;
        /// <summary>
        /// Lexical states for SCLEX_CPP Lexical states for SCLEX_BULLANT
        /// Lexical states for SCLEX_TACL Lexical states for SCLEX_TAL.
        /// </summary>
        public const int SCE_C_TASKMARKER = 26;
        /// <summary>
        /// Lexical states for SCLEX_CPP Lexical states for SCLEX_BULLANT
        /// Lexical states for SCLEX_TACL Lexical states for SCLEX_TAL.
        /// </summary>
        public const int SCE_C_ESCAPESEQUENCE = 27;


        //CLW
        /// <summary>
        /// Lexical states for SCLEX_CLW.
        /// </summary>
        public const int SCE_CLW_DEFAULT = 0;
        /// <summary>
        /// Lexical states for SCLEX_CLW.
        /// </summary>
        public const int SCE_CLW_LABEL = 1;
        /// <summary>
        /// Lexical states for SCLEX_CLW.
        /// </summary>
        public const int SCE_CLW_COMMENT = 2;
        /// <summary>
        /// Lexical states for SCLEX_CLW.
        /// </summary>
        public const int SCE_CLW_STRING = 3;
        /// <summary>
        /// Lexical states for SCLEX_CLW.
        /// </summary>
        public const int SCE_CLW_USER_IDENTIFIER = 4;
        /// <summary>
        /// Lexical states for SCLEX_CLW.
        /// </summary>
        public const int SCE_CLW_INTEGER_CONSTANT = 5;
        /// <summary>
        /// Lexical states for SCLEX_CLW.
        /// </summary>
        public const int SCE_CLW_REAL_CONSTANT = 6;
        /// <summary>
        /// Lexical states for SCLEX_CLW.
        /// </summary>
        public const int SCE_CLW_PICTURE_STRING = 7;
        /// <summary>
        /// Lexical states for SCLEX_CLW.
        /// </summary>
        public const int SCE_CLW_KEYWORD = 8;
        /// <summary>
        /// Lexical states for SCLEX_CLW.
        /// </summary>
        public const int SCE_CLW_COMPILER_DIRECTIVE = 9;
        /// <summary>
        /// Lexical states for SCLEX_CLW.
        /// </summary>
        public const int SCE_CLW_RUNTIME_EXPRESSIONS = 10;
        /// <summary>
        /// Lexical states for SCLEX_CLW.
        /// </summary>
        public const int SCE_CLW_BUILTIN_PROCEDURES_FUNCTION = 11;
        /// <summary>
        /// Lexical states for SCLEX_CLW.
        /// </summary>
        public const int SCE_CLW_STRUCTURE_DATA_TYPE = 12;
        /// <summary>
        /// Lexical states for SCLEX_CLW.
        /// </summary>
        public const int SCE_CLW_ATTRIBUTE = 13;
        /// <summary>
        /// Lexical states for SCLEX_CLW.
        /// </summary>
        public const int SCE_CLW_STANDARD_EQUATE = 14;
        /// <summary>
        /// Lexical states for SCLEX_CLW.
        /// </summary>
        public const int SCE_CLW_ERROR = 15;
        /// <summary>
        /// Lexical states for SCLEX_CLW.
        /// </summary>
        public const int SCE_CLW_DEPRECATED = 16;

        // CSS
        /// <summary>
        /// Lexical states for SCLEX_CSS.
        /// </summary>
        public const int SCE_CSS_DEFAULT = 0;
        /// <summary>
        /// Lexical states for SCLEX_CSS.
        /// </summary>
        public const int SCE_CSS_TAG = 1;
        /// <summary>
        /// Lexical states for SCLEX_CSS.
        /// </summary>
        public const int SCE_CSS_CLASS = 2;
        /// <summary>
        /// Lexical states for SCLEX_CSS.
        /// </summary>
        public const int SCE_CSS_PSEUDOCLASS = 3;
        /// <summary>
        /// Lexical states for SCLEX_CSS.
        /// </summary>
        public const int SCE_CSS_UNKNOWN_PSEUDOCLASS = 4;
        /// <summary>
        /// Lexical states for SCLEX_CSS.
        /// </summary>
        public const int SCE_CSS_OPERATOR = 5;
        /// <summary>
        /// Lexical states for SCLEX_CSS.
        /// </summary>
        public const int SCE_CSS_IDENTIFIER = 6;
        /// <summary>
        /// Lexical states for SCLEX_CSS.
        /// </summary>
        public const int SCE_CSS_UNKNOWN_IDENTIFIER = 7;
        /// <summary>
        /// Lexical states for SCLEX_CSS.
        /// </summary>
        public const int SCE_CSS_VALUE = 8;
        /// <summary>
        /// Lexical states for SCLEX_CSS.
        /// </summary>
        public const int SCE_CSS_COMMENT = 9;
        /// <summary>
        /// Lexical states for SCLEX_CSS.
        /// </summary>
        public const int SCE_CSS_ID = 10;
        /// <summary>
        /// Lexical states for SCLEX_CSS.
        /// </summary>
        public const int SCE_CSS_IMPORTANT = 11;
        /// <summary>
        /// Lexical states for SCLEX_CSS.
        /// </summary>
        public const int SCE_CSS_DIRECTIVE = 12;
        /// <summary>
        /// Lexical states for SCLEX_CSS.
        /// </summary>
        public const int SCE_CSS_DOUBLESTRING = 13;
        /// <summary>
        /// Lexical states for SCLEX_CSS.
        /// </summary>
        public const int SCE_CSS_SINGLESTRING = 14;
        /// <summary>
        /// Lexical states for SCLEX_CSS.
        /// </summary>
        public const int SCE_CSS_IDENTIFIER2 = 15;
        /// <summary>
        /// Lexical states for SCLEX_CSS.
        /// </summary>
        public const int SCE_CSS_ATTRIBUTE = 16;
        /// <summary>
        /// Lexical states for SCLEX_CSS.
        /// </summary>
        public const int SCE_CSS_IDENTIFIER3 = 17;
        /// <summary>
        /// Lexical states for SCLEX_CSS.
        /// </summary>
        public const int SCE_CSS_PSEUDOELEMENT = 18;
        /// <summary>
        /// Lexical states for SCLEX_CSS.
        /// </summary>
        public const int SCE_CSS_EXTENDED_IDENTIFIER = 19;
        /// <summary>
        /// Lexical states for SCLEX_CSS.
        /// </summary>
        public const int SCE_CSS_EXTENDED_PSEUDOCLASS = 20;
        /// <summary>
        /// Lexical states for SCLEX_CSS.
        /// </summary>
        public const int SCE_CSS_EXTENDED_PSEUDOELEMENT = 21;
        /// <summary>
        /// Lexical states for SCLEX_CSS.
        /// </summary>
        public const int SCE_CSS_MEDIA = 22;
        /// <summary>
        /// Lexical states for SCLEX_CSS.
        /// </summary>
        public const int SCE_CSS_VARIABLE = 23;

        // Fortran
        /// <summary>
        /// Lexical states for SCLEX_FORTRAN.
        /// </summary>
        public const int SCE_F_DEFAULT = 0;
        /// <summary>
        /// Lexical states for SCLEX_FORTRAN.
        /// </summary>
        public const int SCE_F_COMMENT = 1;
        /// <summary>
        /// Lexical states for SCLEX_FORTRAN.
        /// </summary>
        public const int SCE_F_NUMBER = 2;
        /// <summary>
        /// Lexical states for SCLEX_FORTRAN.
        /// </summary>
        public const int SCE_F_STRING1 = 3;
        /// <summary>
        /// Lexical states for SCLEX_FORTRAN.
        /// </summary>
        public const int SCE_F_STRING2 = 4;
        /// <summary>
        /// Lexical states for SCLEX_FORTRAN.
        /// </summary>
        public const int SCE_F_STRINGEOL = 5;
        /// <summary>
        /// Lexical states for SCLEX_FORTRAN.
        /// </summary>
        public const int SCE_F_OPERATOR = 6;
        /// <summary>
        /// Lexical states for SCLEX_FORTRAN.
        /// </summary>
        public const int SCE_F_IDENTIFIER = 7;
        /// <summary>
        /// Lexical states for SCLEX_FORTRAN.
        /// </summary>
        public const int SCE_F_WORD = 8;
        /// <summary>
        /// Lexical states for SCLEX_FORTRAN.
        /// </summary>
        public const int SCE_F_WORD2 = 9;
        /// <summary>
        /// Lexical states for SCLEX_FORTRAN.
        /// </summary>
        public const int SCE_F_WORD3 = 10;
        /// <summary>
        /// Lexical states for SCLEX_FORTRAN.
        /// </summary>
        public const int SCE_F_PREPROCESSOR = 11;
        /// <summary>
        /// Lexical states for SCLEX_FORTRAN.
        /// </summary>
        public const int SCE_F_OPERATOR2 = 12;
        /// <summary>
        /// Lexical states for SCLEX_FORTRAN.
        /// </summary>
        public const int SCE_F_LABEL = 13;
        /// <summary>
        /// Lexical states for SCLEX_FORTRAN.
        /// </summary>
        public const int SCE_F_CONTINUATION = 14;

        // HTML
        /// <summary>
        /// Lexical states for SCLEX_HTML, SCLEX_XML.
        /// </summary>
        public const int SCE_H_DEFAULT = 0;
        /// <summary>
        /// Lexical states for SCLEX_HTML, SCLEX_XML.
        /// </summary>
        public const int SCE_H_TAG = 1;
        /// <summary>
        /// Lexical states for SCLEX_HTML, SCLEX_XML.
        /// </summary>
        public const int SCE_H_TAGUNKNOWN = 2;
        /// <summary>
        /// Lexical states for SCLEX_HTML, SCLEX_XML.
        /// </summary>
        public const int SCE_H_ATTRIBUTE = 3;
        /// <summary>
        /// Lexical states for SCLEX_HTML, SCLEX_XML.
        /// </summary>
        public const int SCE_H_ATTRIBUTEUNKNOWN = 4;
        /// <summary>
        /// Lexical states for SCLEX_HTML, SCLEX_XML.
        /// </summary>
        public const int SCE_H_NUMBER = 5;
        /// <summary>
        /// Lexical states for SCLEX_HTML, SCLEX_XML.
        /// </summary>
        public const int SCE_H_DOUBLESTRING = 6;
        /// <summary>
        /// Lexical states for SCLEX_HTML, SCLEX_XML.
        /// </summary>
        public const int SCE_H_SINGLESTRING = 7;
        /// <summary>
        /// Lexical states for SCLEX_HTML, SCLEX_XML.
        /// </summary>
        public const int SCE_H_OTHER = 8;
        /// <summary>
        /// Lexical states for SCLEX_HTML, SCLEX_XML.
        /// </summary>
        public const int SCE_H_COMMENT = 9;
        /// <summary>
        /// Lexical states for SCLEX_HTML, SCLEX_XML.
        /// </summary>
        public const int SCE_H_ENTITY = 10;
        /// <summary>
        /// XML and ASP.
        /// </summary>
        public const int SCE_H_TAGEND = 11;
        /// <summary>
        /// Lexical states for SCLEX_HTML, SCLEX_XML.
        /// </summary>
        public const int SCE_H_XMLSTART = 12;
        /// <summary>
        /// Lexical states for SCLEX_HTML, SCLEX_XML.
        /// </summary>
        public const int SCE_H_XMLEND = 13;
        /// <summary>
        /// Lexical states for SCLEX_HTML, SCLEX_XML.
        /// </summary>
        public const int SCE_H_SCRIPT = 14;
        /// <summary>
        /// Lexical states for SCLEX_HTML, SCLEX_XML.
        /// </summary>
        public const int SCE_H_ASP = 15;
        /// <summary>
        /// Lexical states for SCLEX_HTML, SCLEX_XML.
        /// </summary>
        public const int SCE_H_ASPAT = 16;
        /// <summary>
        /// Lexical states for SCLEX_HTML, SCLEX_XML.
        /// </summary>
        public const int SCE_H_CDATA = 17;
        /// <summary>
        /// Lexical states for SCLEX_HTML, SCLEX_XML.
        /// </summary>
        public const int SCE_H_QUESTION = 18;
        /// <summary>
        /// More HTML.
        /// </summary>
        public const int SCE_H_VALUE = 19;
        /// <summary>
        /// X-Code, ASP.NET, JSP.
        /// </summary>
        public const int SCE_H_XCCOMMENT = 20;

        // JSON
        /// <summary>
        /// Lexical states for SCLEX_JSON.
        /// </summary>
        public const int SCE_JSON_DEFAULT = 0;
        /// <summary>
        /// Lexical states for SCLEX_JSON.
        /// </summary>
        public const int SCE_JSON_NUMBER = 1;
        /// <summary>
        /// Lexical states for SCLEX_JSON.
        /// </summary>
        public const int SCE_JSON_STRING = 2;
        /// <summary>
        /// Lexical states for SCLEX_JSON.
        /// </summary>
        public const int SCE_JSON_STRINGEOL = 3;
        /// <summary>
        /// Lexical states for SCLEX_JSON.
        /// </summary>
        public const int SCE_JSON_PROPERTYNAME = 4;
        /// <summary>
        /// Lexical states for SCLEX_JSON.
        /// </summary>
        public const int SCE_JSON_ESCAPESEQUENCE = 5;
        /// <summary>
        /// Lexical states for SCLEX_JSON.
        /// </summary>
        public const int SCE_JSON_LINECOMMENT = 6;
        /// <summary>
        /// Lexical states for SCLEX_JSON.
        /// </summary>
        public const int SCE_JSON_BLOCKCOMMENT = 7;
        /// <summary>
        /// Lexical states for SCLEX_JSON.
        /// </summary>
        public const int SCE_JSON_OPERATOR = 8;
        /// <summary>
        /// Lexical states for SCLEX_JSON.
        /// </summary>
        public const int SCE_JSON_URI = 9;
        /// <summary>
        /// Lexical states for SCLEX_JSON.
        /// </summary>
        public const int SCE_JSON_COMPACTIRI = 10;
        /// <summary>
        /// Lexical states for SCLEX_JSON.
        /// </summary>
        public const int SCE_JSON_KEYWORD = 11;
        /// <summary>
        /// Lexical states for SCLEX_JSON.
        /// </summary>
        public const int SCE_JSON_LDKEYWORD = 12;
        /// <summary>
        /// Lexical states for SCLEX_JSON.
        /// </summary>
        public const int SCE_JSON_ERROR = 13;

        // Lisp
        /// <summary>
        /// Lexical states for SCLEX_LISP.
        /// </summary>
        public const int SCE_LISP_DEFAULT = 0;
        /// <summary>
        /// Lexical states for SCLEX_LISP.
        /// </summary>
        public const int SCE_LISP_COMMENT = 1;
        /// <summary>
        /// Lexical states for SCLEX_LISP.
        /// </summary>
        public const int SCE_LISP_NUMBER = 2;
        /// <summary>
        /// Lexical states for SCLEX_LISP.
        /// </summary>
        public const int SCE_LISP_KEYWORD = 3;
        /// <summary>
        /// Lexical states for SCLEX_LISP.
        /// </summary>
        public const int SCE_LISP_KEYWORD_KW = 4;
        /// <summary>
        /// Lexical states for SCLEX_LISP.
        /// </summary>
        public const int SCE_LISP_SYMBOL = 5;
        /// <summary>
        /// Lexical states for SCLEX_LISP.
        /// </summary>
        public const int SCE_LISP_STRING = 6;
        /// <summary>
        /// Lexical states for SCLEX_LISP.
        /// </summary>
        public const int SCE_LISP_STRINGEOL = 8;
        /// <summary>
        /// Lexical states for SCLEX_LISP.
        /// </summary>
        public const int SCE_LISP_IDENTIFIER = 9;
        /// <summary>
        /// Lexical states for SCLEX_LISP.
        /// </summary>
        public const int SCE_LISP_OPERATOR = 10;
        /// <summary>
        /// Lexical states for SCLEX_LISP.
        /// </summary>
        public const int SCE_LISP_SPECIAL = 11;
        /// <summary>
        /// Lexical states for SCLEX_LISP.
        /// </summary>
        public const int SCE_LISP_MULTI_COMMENT = 12;

        // Lua
        /// <summary>
        /// Lexical states for SCLEX_LUA.
        /// </summary>
        public const int SCE_LUA_DEFAULT = 0;
        /// <summary>
        /// Lexical states for SCLEX_LUA.
        /// </summary>
        public const int SCE_LUA_COMMENT = 1;
        /// <summary>
        /// Lexical states for SCLEX_LUA.
        /// </summary>
        public const int SCE_LUA_COMMENTLINE = 2;
        /// <summary>
        /// Lexical states for SCLEX_LUA.
        /// </summary>
        public const int SCE_LUA_COMMENTDOC = 3;
        /// <summary>
        /// Lexical states for SCLEX_LUA.
        /// </summary>
        public const int SCE_LUA_NUMBER = 4;
        /// <summary>
        /// Lexical states for SCLEX_LUA.
        /// </summary>
        public const int SCE_LUA_WORD = 5;
        /// <summary>
        /// Lexical states for SCLEX_LUA.
        /// </summary>
        public const int SCE_LUA_STRING = 6;
        /// <summary>
        /// Lexical states for SCLEX_LUA.
        /// </summary>
        public const int SCE_LUA_CHARACTER = 7;
        /// <summary>
        /// Lexical states for SCLEX_LUA.
        /// </summary>
        public const int SCE_LUA_LITERALSTRING = 8;
        /// <summary>
        /// Lexical states for SCLEX_LUA.
        /// </summary>
        public const int SCE_LUA_PREPROCESSOR = 9;
        /// <summary>
        /// Lexical states for SCLEX_LUA.
        /// </summary>
        public const int SCE_LUA_OPERATOR = 10;
        /// <summary>
        /// Lexical states for SCLEX_LUA.
        /// </summary>
        public const int SCE_LUA_IDENTIFIER = 11;
        /// <summary>
        /// Lexical states for SCLEX_LUA.
        /// </summary>
        public const int SCE_LUA_STRINGEOL = 12;
        /// <summary>
        /// Lexical states for SCLEX_LUA.
        /// </summary>
        public const int SCE_LUA_WORD2 = 13;
        /// <summary>
        /// Lexical states for SCLEX_LUA.
        /// </summary>
        public const int SCE_LUA_WORD3 = 14;
        /// <summary>
        /// Lexical states for SCLEX_LUA.
        /// </summary>
        public const int SCE_LUA_WORD4 = 15;
        /// <summary>
        /// Lexical states for SCLEX_LUA.
        /// </summary>
        public const int SCE_LUA_WORD5 = 16;
        /// <summary>
        /// Lexical states for SCLEX_LUA.
        /// </summary>
        public const int SCE_LUA_WORD6 = 17;
        /// <summary>
        /// Lexical states for SCLEX_LUA.
        /// </summary>
        public const int SCE_LUA_WORD7 = 18;
        /// <summary>
        /// Lexical states for SCLEX_LUA.
        /// </summary>
        public const int SCE_LUA_WORD8 = 19;
        /// <summary>
        /// Lexical states for SCLEX_LUA.
        /// </summary>
        public const int SCE_LUA_LABEL = 20;

        /// <summary>
        /// Lexical states for SCLEX_PASCAL.
        /// </summary>
        public const int SCE_PAS_DEFAULT = 0;
        /// <summary>
        /// Lexical states for SCLEX_PASCAL.
        /// </summary>
        public const int SCE_PAS_IDENTIFIER = 1;
        /// <summary>
        /// Lexical states for SCLEX_PASCAL.
        /// </summary>
        public const int SCE_PAS_COMMENT = 2;
        /// <summary>
        /// Lexical states for SCLEX_PASCAL.
        /// </summary>
        public const int SCE_PAS_COMMENT2 = 3;
        /// <summary>
        /// Lexical states for SCLEX_PASCAL.
        /// </summary>
        public const int SCE_PAS_COMMENTLINE = 4;
        /// <summary>
        /// Lexical states for SCLEX_PASCAL.
        /// </summary>
        public const int SCE_PAS_PREPROCESSOR = 5;
        /// <summary>
        /// Lexical states for SCLEX_PASCAL.
        /// </summary>
        public const int SCE_PAS_PREPROCESSOR2 = 6;
        /// <summary>
        /// Lexical states for SCLEX_PASCAL.
        /// </summary>
        public const int SCE_PAS_NUMBER = 7;
        /// <summary>
        /// Lexical states for SCLEX_PASCAL.
        /// </summary>
        public const int SCE_PAS_HEXNUMBER = 8;
        /// <summary>
        /// Lexical states for SCLEX_PASCAL.
        /// </summary>
        public const int SCE_PAS_WORD = 9;
        /// <summary>
        /// Lexical states for SCLEX_PASCAL.
        /// </summary>
        public const int SCE_PAS_STRING = 10;
        /// <summary>
        /// Lexical states for SCLEX_PASCAL.
        /// </summary>
        public const int SCE_PAS_STRINGEOL = 11;
        /// <summary>
        /// Lexical states for SCLEX_PASCAL.
        /// </summary>
        public const int SCE_PAS_CHARACTER = 12;
        /// <summary>
        /// Lexical states for SCLEX_PASCAL.
        /// </summary>
        public const int SCE_PAS_OPERATOR = 13;
        /// <summary>
        /// Lexical states for SCLEX_PASCAL.
        /// </summary>
        public const int SCE_PAS_ASM = 14;

        // Perl
        /// <summary>
        /// Lexical states for SCLEX_PERL.
        /// </summary>
        public const int SCE_PL_DEFAULT = 0;
        /// <summary>
        /// Lexical states for SCLEX_PERL.
        /// </summary>
        public const int SCE_PL_ERROR = 1;
        /// <summary>
        /// Lexical states for SCLEX_PERL.
        /// </summary>
        public const int SCE_PL_COMMENTLINE = 2;
        /// <summary>
        /// Lexical states for SCLEX_PERL.
        /// </summary>
        public const int SCE_PL_POD = 3;
        /// <summary>
        /// Lexical states for SCLEX_PERL.
        /// </summary>
        public const int SCE_PL_NUMBER = 4;
        /// <summary>
        /// Lexical states for SCLEX_PERL.
        /// </summary>
        public const int SCE_PL_WORD = 5;
        /// <summary>
        /// Lexical states for SCLEX_PERL.
        /// </summary>
        public const int SCE_PL_STRING = 6;
        /// <summary>
        /// Lexical states for SCLEX_PERL.
        /// </summary>
        public const int SCE_PL_CHARACTER = 7;
        /// <summary>
        /// Lexical states for SCLEX_PERL.
        /// </summary>
        public const int SCE_PL_PUNCTUATION = 8;
        /// <summary>
        /// Lexical states for SCLEX_PERL.
        /// </summary>
        public const int SCE_PL_PREPROCESSOR = 9;
        /// <summary>
        /// Lexical states for SCLEX_PERL.
        /// </summary>
        public const int SCE_PL_OPERATOR = 10;
        /// <summary>
        /// Lexical states for SCLEX_PERL.
        /// </summary>
        public const int SCE_PL_IDENTIFIER = 11;
        /// <summary>
        /// Lexical states for SCLEX_PERL.
        /// </summary>
        public const int SCE_PL_SCALAR = 12;
        /// <summary>
        /// Lexical states for SCLEX_PERL.
        /// </summary>
        public const int SCE_PL_ARRAY = 13;
        /// <summary>
        /// Lexical states for SCLEX_PERL.
        /// </summary>
        public const int SCE_PL_HASH = 14;
        /// <summary>
        /// Lexical states for SCLEX_PERL.
        /// </summary>
        public const int SCE_PL_SYMBOLTABLE = 15;
        /// <summary>
        /// Lexical states for SCLEX_PERL.
        /// </summary>
        public const int SCE_PL_VARIABLE_INDEXER = 16;
        /// <summary>
        /// Lexical states for SCLEX_PERL.
        /// </summary>
        public const int SCE_PL_REGEX = 17;
        /// <summary>
        /// Lexical states for SCLEX_PERL.
        /// </summary>
        public const int SCE_PL_REGSUBST = 18;
        /// <summary>
        /// Lexical states for SCLEX_PERL.
        /// </summary>
        public const int SCE_PL_LONGQUOTE = 19;
        /// <summary>
        /// Lexical states for SCLEX_PERL.
        /// </summary>
        public const int SCE_PL_BACKTICKS = 20;
        /// <summary>
        /// Lexical states for SCLEX_PERL.
        /// </summary>
        public const int SCE_PL_DATASECTION = 21;
        /// <summary>
        /// Lexical states for SCLEX_PERL.
        /// </summary>
        public const int SCE_PL_HERE_DELIM = 22;
        /// <summary>
        /// Lexical states for SCLEX_PERL.
        /// </summary>
        public const int SCE_PL_HERE_Q = 23;
        /// <summary>
        /// Lexical states for SCLEX_PERL.
        /// </summary>
        public const int SCE_PL_HERE_QQ = 24;
        /// <summary>
        /// Lexical states for SCLEX_PERL.
        /// </summary>
        public const int SCE_PL_HERE_QX = 25;
        /// <summary>
        /// Lexical states for SCLEX_PERL.
        /// </summary>
        public const int SCE_PL_STRING_Q = 26;
        /// <summary>
        /// Lexical states for SCLEX_PERL.
        /// </summary>
        public const int SCE_PL_STRING_QQ = 27;
        /// <summary>
        /// Lexical states for SCLEX_PERL.
        /// </summary>
        public const int SCE_PL_STRING_QX = 28;
        /// <summary>
        /// Lexical states for SCLEX_PERL.
        /// </summary>
        public const int SCE_PL_STRING_QR = 29;
        /// <summary>
        /// Lexical states for SCLEX_PERL.
        /// </summary>
        public const int SCE_PL_STRING_QW = 30;
        /// <summary>
        /// Lexical states for SCLEX_PERL.
        /// </summary>
        public const int SCE_PL_POD_VERB = 31;
        /// <summary>
        /// Lexical states for SCLEX_PERL.
        /// </summary>
        public const int SCE_PL_SUB_PROTOTYPE = 40;
        /// <summary>
        /// Lexical states for SCLEX_PERL.
        /// </summary>
        public const int SCE_PL_FORMAT_IDENT = 41;
        /// <summary>
        /// Lexical states for SCLEX_PERL.
        /// </summary>
        public const int SCE_PL_FORMAT = 42;
        /// <summary>
        /// Lexical states for SCLEX_PERL.
        /// </summary>
        public const int SCE_PL_STRING_VAR = 43;
        /// <summary>
        /// Lexical states for SCLEX_PERL.
        /// </summary>
        public const int SCE_PL_XLAT = 44;
        /// <summary>
        /// Lexical states for SCLEX_PERL.
        /// </summary>
        public const int SCE_PL_REGEX_VAR = 54;
        /// <summary>
        /// Lexical states for SCLEX_PERL.
        /// </summary>
        public const int SCE_PL_REGSUBST_VAR = 55;
        /// <summary>
        /// Lexical states for SCLEX_PERL.
        /// </summary>
        public const int SCE_PL_BACKTICKS_VAR = 57;
        /// <summary>
        /// Lexical states for SCLEX_PERL.
        /// </summary>
        public const int SCE_PL_HERE_QQ_VAR = 61;
        /// <summary>
        /// Lexical states for SCLEX_PERL.
        /// </summary>
        public const int SCE_PL_HERE_QX_VAR = 62;
        /// <summary>
        /// Lexical states for SCLEX_PERL.
        /// </summary>
        public const int SCE_PL_STRING_QQ_VAR = 64;
        /// <summary>
        /// Lexical states for SCLEX_PERL.
        /// </summary>
        public const int SCE_PL_STRING_QX_VAR = 65;
        /// <summary>
        /// Lexical states for SCLEX_PERL.
        /// </summary>
        public const int SCE_PL_STRING_QR_VAR = 66;

        // PowerShell
        /// <summary>
        /// Lexical state for SCLEX_POWERSHELL.
        /// </summary>
        public const int SCE_POWERSHELL_DEFAULT = 0;
        /// <summary>
        /// Lexical state for SCLEX_POWERSHELL.
        /// </summary>
        public const int SCE_POWERSHELL_COMMENT = 1;
        /// <summary>
        /// Lexical state for SCLEX_POWERSHELL.
        /// </summary>
        public const int SCE_POWERSHELL_STRING = 2;
        /// <summary>
        /// Lexical state for SCLEX_POWERSHELL.
        /// </summary>
        public const int SCE_POWERSHELL_CHARACTER = 3;
        /// <summary>
        /// Lexical state for SCLEX_POWERSHELL.
        /// </summary>
        public const int SCE_POWERSHELL_NUMBER = 4;
        /// <summary>
        /// Lexical state for SCLEX_POWERSHELL.
        /// </summary>
        public const int SCE_POWERSHELL_VARIABLE = 5;
        /// <summary>
        /// Lexical state for SCLEX_POWERSHELL.
        /// </summary>
        public const int SCE_POWERSHELL_OPERATOR = 6;
        /// <summary>
        /// Lexical state for SCLEX_POWERSHELL.
        /// </summary>
        public const int SCE_POWERSHELL_IDENTIFIER = 7;
        /// <summary>
        /// Lexical state for SCLEX_POWERSHELL.
        /// </summary>
        public const int SCE_POWERSHELL_KEYWORD = 8;
        /// <summary>
        /// Lexical state for SCLEX_POWERSHELL.
        /// </summary>
        public const int SCE_POWERSHELL_CMDLET = 9;
        /// <summary>
        /// Lexical state for SCLEX_POWERSHELL.
        /// </summary>
        public const int SCE_POWERSHELL_ALIAS = 10;
        /// <summary>
        /// Lexical state for SCLEX_POWERSHELL.
        /// </summary>
        public const int SCE_POWERSHELL_FUNCTION = 11;
        /// <summary>
        /// Lexical state for SCLEX_POWERSHELL.
        /// </summary>
        public const int SCE_POWERSHELL_USER1 = 12;
        /// <summary>
        /// Lexical state for SCLEX_POWERSHELL.
        /// </summary>
        public const int SCE_POWERSHELL_COMMENTSTREAM = 13;
        /// <summary>
        /// Lexical state for SCLEX_POWERSHELL.
        /// </summary>
        public const int SCE_POWERSHELL_HERE_STRING = 14;
        /// <summary>
        /// Lexical state for SCLEX_POWERSHELL.
        /// </summary>
        public const int SCE_POWERSHELL_HERE_CHARACTER = 15;
        /// <summary>
        /// Lexical state for SCLEX_POWERSHELL.
        /// </summary>
        public const int SCE_POWERSHELL_COMMENTDOCKEYWORD = 16;

        // Properties
        /// <summary>
        /// Lexical states for SCLEX_PROPERTIES.
        /// </summary>
        public const int SCE_PROPS_DEFAULT = 0;
        /// <summary>
        /// Lexical states for SCLEX_PROPERTIES.
        /// </summary>
        public const int SCE_PROPS_COMMENT = 1;
        /// <summary>
        /// Lexical states for SCLEX_PROPERTIES.
        /// </summary>
        public const int SCE_PROPS_SECTION = 2;
        /// <summary>
        /// Lexical states for SCLEX_PROPERTIES.
        /// </summary>
        public const int SCE_PROPS_ASSIGNMENT = 3;
        /// <summary>
        /// Lexical states for SCLEX_PROPERTIES.
        /// </summary>
        public const int SCE_PROPS_DEFVAL = 4;
        /// <summary>
        /// Lexical states for SCLEX_PROPERTIES.
        /// </summary>
        public const int SCE_PROPS_KEY = 5;

        // PHP script
        /// <summary>
        /// PHP.
        /// </summary>
        public const int SCE_HPHP_COMPLEX_VARIABLE = 104;
        /// <summary>
        /// PHP.
        /// </summary>
        public const int SCE_HPHP_DEFAULT = 118;
        /// <summary>
        /// The SCE_HPHP_HSTRING constant.
        /// </summary>
        public const int SCE_HPHP_HSTRING = 119;
        /// <summary>
        /// The SCE_HPHP_SIMPLESTRING constant.
        /// </summary>
        public const int SCE_HPHP_SIMPLESTRING = 120;
        /// <summary>
        /// The SCE_HPHP_WORD constant.
        /// </summary>
        public const int SCE_HPHP_WORD = 121;
        /// <summary>
        /// The SCE_HPHP_NUMBER constant.
        /// </summary>
        public const int SCE_HPHP_NUMBER = 122;
        /// <summary>
        /// The SCE_HPHP_VARIABLE constant.
        /// </summary>
        public const int SCE_HPHP_VARIABLE = 123;
        /// <summary>
        /// The SCE_HPHP_COMMENT constant.
        /// </summary>
        public const int SCE_HPHP_COMMENT = 124;
        /// <summary>
        /// The SCE_HPHP_COMMENTLINE constant.
        /// </summary>
        public const int SCE_HPHP_COMMENTLINE = 125;
        /// <summary>
        /// The SCE_HPHP_HSTRING_VARIABLE constant.
        /// </summary>
        public const int SCE_HPHP_HSTRING_VARIABLE = 126;
        /// <summary>
        /// The SCE_HPHP_OPERATOR constant.
        /// </summary>
        public const int SCE_HPHP_OPERATOR = 127;

        // SQL
        /// <summary>
        /// Lexical states for SCLEX_SQL.
        /// </summary>
        public const int SCE_SQL_DEFAULT = 0;
        /// <summary>
        /// Lexical states for SCLEX_SQL.
        /// </summary>
        public const int SCE_SQL_COMMENT = 1;
        /// <summary>
        /// Lexical states for SCLEX_SQL.
        /// </summary>
        public const int SCE_SQL_COMMENTLINE = 2;
        /// <summary>
        /// Lexical states for SCLEX_SQL.
        /// </summary>
        public const int SCE_SQL_COMMENTDOC = 3;
        /// <summary>
        /// Lexical states for SCLEX_SQL.
        /// </summary>
        public const int SCE_SQL_NUMBER = 4;
        /// <summary>
        /// Lexical states for SCLEX_SQL.
        /// </summary>
        public const int SCE_SQL_WORD = 5;
        /// <summary>
        /// Lexical states for SCLEX_SQL.
        /// </summary>
        public const int SCE_SQL_STRING = 6;
        /// <summary>
        /// Lexical states for SCLEX_SQL.
        /// </summary>
        public const int SCE_SQL_CHARACTER = 7;
        /// <summary>
        /// Lexical states for SCLEX_SQL.
        /// </summary>
        public const int SCE_SQL_SQLPLUS = 8;
        /// <summary>
        /// Lexical states for SCLEX_SQL.
        /// </summary>
        public const int SCE_SQL_SQLPLUS_PROMPT = 9;
        /// <summary>
        /// Lexical states for SCLEX_SQL.
        /// </summary>
        public const int SCE_SQL_OPERATOR = 10;
        /// <summary>
        /// Lexical states for SCLEX_SQL.
        /// </summary>
        public const int SCE_SQL_IDENTIFIER = 11;
        /// <summary>
        /// Lexical states for SCLEX_SQL.
        /// </summary>
        public const int SCE_SQL_SQLPLUS_COMMENT = 13;
        /// <summary>
        /// Lexical states for SCLEX_SQL.
        /// </summary>
        public const int SCE_SQL_COMMENTLINEDOC = 15;
        /// <summary>
        /// Lexical states for SCLEX_SQL.
        /// </summary>
        public const int SCE_SQL_WORD2 = 16;
        /// <summary>
        /// Lexical states for SCLEX_SQL.
        /// </summary>
        public const int SCE_SQL_COMMENTDOCKEYWORD = 17;
        /// <summary>
        /// Lexical states for SCLEX_SQL.
        /// </summary>
        public const int SCE_SQL_COMMENTDOCKEYWORDERROR = 18;
        /// <summary>
        /// Lexical states for SCLEX_SQL.
        /// </summary>
        public const int SCE_SQL_USER1 = 19;
        /// <summary>
        /// Lexical states for SCLEX_SQL.
        /// </summary>
        public const int SCE_SQL_USER2 = 20;
        /// <summary>
        /// Lexical states for SCLEX_SQL.
        /// </summary>
        public const int SCE_SQL_USER3 = 21;
        /// <summary>
        /// Lexical states for SCLEX_SQL.
        /// </summary>
        public const int SCE_SQL_USER4 = 22;
        /// <summary>
        /// Lexical states for SCLEX_SQL.
        /// </summary>
        public const int SCE_SQL_QUOTEDIDENTIFIER = 23;
        /// <summary>
        /// Lexical states for SCLEX_SQL.
        /// </summary>
        public const int SCE_SQL_QOPERATOR = 24;

        // Python
        /// <summary>
        /// Lexical states for SCLEX_PYTHON.
        /// </summary>
        public const int SCE_P_DEFAULT = 0;
        /// <summary>
        /// Lexical states for SCLEX_PYTHON.
        /// </summary>
        public const int SCE_P_COMMENTLINE = 1;
        /// <summary>
        /// Lexical states for SCLEX_PYTHON.
        /// </summary>
        public const int SCE_P_NUMBER = 2;
        /// <summary>
        /// Lexical states for SCLEX_PYTHON.
        /// </summary>
        public const int SCE_P_STRING = 3;
        /// <summary>
        /// Lexical states for SCLEX_PYTHON.
        /// </summary>
        public const int SCE_P_CHARACTER = 4;
        /// <summary>
        /// Lexical states for SCLEX_PYTHON.
        /// </summary>
        public const int SCE_P_WORD = 5;
        /// <summary>
        /// Lexical states for SCLEX_PYTHON.
        /// </summary>
        public const int SCE_P_TRIPLE = 6;
        /// <summary>
        /// Lexical states for SCLEX_PYTHON.
        /// </summary>
        public const int SCE_P_TRIPLEDOUBLE = 7;
        /// <summary>
        /// Lexical states for SCLEX_PYTHON.
        /// </summary>
        public const int SCE_P_CLASSNAME = 8;
        /// <summary>
        /// Lexical states for SCLEX_PYTHON.
        /// </summary>
        public const int SCE_P_DEFNAME = 9;
        /// <summary>
        /// Lexical states for SCLEX_PYTHON.
        /// </summary>
        public const int SCE_P_OPERATOR = 10;
        /// <summary>
        /// Lexical states for SCLEX_PYTHON.
        /// </summary>
        public const int SCE_P_IDENTIFIER = 11;
        /// <summary>
        /// Lexical states for SCLEX_PYTHON.
        /// </summary>
        public const int SCE_P_COMMENTBLOCK = 12;
        /// <summary>
        /// Lexical states for SCLEX_PYTHON.
        /// </summary>
        public const int SCE_P_STRINGEOL = 13;
        /// <summary>
        /// Lexical states for SCLEX_PYTHON.
        /// </summary>
        public const int SCE_P_WORD2 = 14;
        /// <summary>
        /// Lexical states for SCLEX_PYTHON.
        /// </summary>
        public const int SCE_P_DECORATOR = 15;

        // Ruby
        /// <summary>
        /// Lexical states for SCLEX_RUBY.
        /// </summary>
        public const int SCE_RB_DEFAULT = 0;
        /// <summary>
        /// Lexical states for SCLEX_RUBY.
        /// </summary>
        public const int SCE_RB_ERROR = 1;
        /// <summary>
        /// Lexical states for SCLEX_RUBY.
        /// </summary>
        public const int SCE_RB_COMMENTLINE = 2;
        /// <summary>
        /// Lexical states for SCLEX_RUBY.
        /// </summary>
        public const int SCE_RB_POD = 3;
        /// <summary>
        /// Lexical states for SCLEX_RUBY.
        /// </summary>
        public const int SCE_RB_NUMBER = 4;
        /// <summary>
        /// Lexical states for SCLEX_RUBY.
        /// </summary>
        public const int SCE_RB_WORD = 5;
        /// <summary>
        /// Lexical states for SCLEX_RUBY.
        /// </summary>
        public const int SCE_RB_STRING = 6;
        /// <summary>
        /// Lexical states for SCLEX_RUBY.
        /// </summary>
        public const int SCE_RB_CHARACTER = 7;
        /// <summary>
        /// Lexical states for SCLEX_RUBY.
        /// </summary>
        public const int SCE_RB_CLASSNAME = 8;
        /// <summary>
        /// Lexical states for SCLEX_RUBY.
        /// </summary>
        public const int SCE_RB_DEFNAME = 9;
        /// <summary>
        /// Lexical states for SCLEX_RUBY.
        /// </summary>
        public const int SCE_RB_OPERATOR = 10;
        /// <summary>
        /// Lexical states for SCLEX_RUBY.
        /// </summary>
        public const int SCE_RB_IDENTIFIER = 11;
        /// <summary>
        /// Lexical states for SCLEX_RUBY.
        /// </summary>
        public const int SCE_RB_REGEX = 12;
        /// <summary>
        /// Lexical states for SCLEX_RUBY.
        /// </summary>
        public const int SCE_RB_GLOBAL = 13;
        /// <summary>
        /// Lexical states for SCLEX_RUBY.
        /// </summary>
        public const int SCE_RB_SYMBOL = 14;
        /// <summary>
        /// Lexical states for SCLEX_RUBY.
        /// </summary>
        public const int SCE_RB_MODULE_NAME = 15;
        /// <summary>
        /// Lexical states for SCLEX_RUBY.
        /// </summary>
        public const int SCE_RB_INSTANCE_VAR = 16;
        /// <summary>
        /// Lexical states for SCLEX_RUBY.
        /// </summary>
        public const int SCE_RB_CLASS_VAR = 17;
        /// <summary>
        /// Lexical states for SCLEX_RUBY.
        /// </summary>
        public const int SCE_RB_BACKTICKS = 18;
        /// <summary>
        /// Lexical states for SCLEX_RUBY.
        /// </summary>
        public const int SCE_RB_DATASECTION = 19;
        /// <summary>
        /// Lexical states for SCLEX_RUBY.
        /// </summary>
        public const int SCE_RB_HERE_DELIM = 20;
        /// <summary>
        /// Lexical states for SCLEX_RUBY.
        /// </summary>
        public const int SCE_RB_HERE_Q = 21;
        /// <summary>
        /// Lexical states for SCLEX_RUBY.
        /// </summary>
        public const int SCE_RB_HERE_QQ = 22;
        /// <summary>
        /// Lexical states for SCLEX_RUBY.
        /// </summary>
        public const int SCE_RB_HERE_QX = 23;
        /// <summary>
        /// Lexical states for SCLEX_RUBY.
        /// </summary>
        public const int SCE_RB_STRING_Q = 24;
        /// <summary>
        /// Lexical states for SCLEX_RUBY.
        /// </summary>
        public const int SCE_RB_STRING_QQ = 25;
        /// <summary>
        /// Lexical states for SCLEX_RUBY.
        /// </summary>
        public const int SCE_RB_STRING_QX = 26;
        /// <summary>
        /// Lexical states for SCLEX_RUBY.
        /// </summary>
        public const int SCE_RB_STRING_QR = 27;
        /// <summary>
        /// Lexical states for SCLEX_RUBY.
        /// </summary>
        public const int SCE_RB_STRING_QW = 28;
        /// <summary>
        /// Lexical states for SCLEX_RUBY.
        /// </summary>
        public const int SCE_RB_WORD_DEMOTED = 29;
        /// <summary>
        /// Lexical states for SCLEX_RUBY.
        /// </summary>
        public const int SCE_RB_STDIN = 30;
        /// <summary>
        /// Lexical states for SCLEX_RUBY.
        /// </summary>
        public const int SCE_RB_STDOUT = 31;
        /// <summary>
        /// Lexical states for SCLEX_RUBY.
        /// </summary>
        public const int SCE_RB_STDERR = 40;
        /// <summary>
        /// Lexical states for SCLEX_RUBY.
        /// </summary>
        public const int SCE_RB_UPPER_BOUND = 41;

        // Smalltalk
        /// <summary>
        /// Lexical states for SCLEX_SMALLTALK.
        /// </summary>
        public const int SCE_ST_DEFAULT = 0;
        /// <summary>
        /// Lexical states for SCLEX_SMALLTALK.
        /// </summary>
        public const int SCE_ST_STRING = 1;
        /// <summary>
        /// Lexical states for SCLEX_SMALLTALK.
        /// </summary>
        public const int SCE_ST_NUMBER = 2;
        /// <summary>
        /// Lexical states for SCLEX_SMALLTALK.
        /// </summary>
        public const int SCE_ST_COMMENT = 3;
        /// <summary>
        /// Lexical states for SCLEX_SMALLTALK.
        /// </summary>
        public const int SCE_ST_SYMBOL = 4;
        /// <summary>
        /// Lexical states for SCLEX_SMALLTALK.
        /// </summary>
        public const int SCE_ST_BINARY = 5;
        /// <summary>
        /// Lexical states for SCLEX_SMALLTALK.
        /// </summary>
        public const int SCE_ST_BOOL = 6;
        /// <summary>
        /// Lexical states for SCLEX_SMALLTALK.
        /// </summary>
        public const int SCE_ST_SELF = 7;
        /// <summary>
        /// Lexical states for SCLEX_SMALLTALK.
        /// </summary>
        public const int SCE_ST_SUPER = 8;
        /// <summary>
        /// Lexical states for SCLEX_SMALLTALK.
        /// </summary>
        public const int SCE_ST_NIL = 9;
        /// <summary>
        /// Lexical states for SCLEX_SMALLTALK.
        /// </summary>
        public const int SCE_ST_GLOBAL = 10;
        /// <summary>
        /// Lexical states for SCLEX_SMALLTALK.
        /// </summary>
        public const int SCE_ST_RETURN = 11;
        /// <summary>
        /// Lexical states for SCLEX_SMALLTALK.
        /// </summary>
        public const int SCE_ST_SPECIAL = 12;
        /// <summary>
        /// Lexical states for SCLEX_SMALLTALK.
        /// </summary>
        public const int SCE_ST_KWSEND = 13;
        /// <summary>
        /// Lexical states for SCLEX_SMALLTALK.
        /// </summary>
        public const int SCE_ST_ASSIGN = 14;
        /// <summary>
        /// Lexical states for SCLEX_SMALLTALK.
        /// </summary>
        public const int SCE_ST_CHARACTER = 15;
        /// <summary>
        /// Lexical states for SCLEX_SMALLTALK.
        /// </summary>
        public const int SCE_ST_SPEC_SEL = 16;

        // Basic / VB
        /// <summary>
        /// Lexical states for SCLEX_VB, SCLEX_VBSCRIPT, SCLEX_POWERBASIC,
        /// SCLEX_BLITZBASIC, SCLEX_PUREBASIC, SCLEX_FREEBASIC.
        /// </summary>
        public const int SCE_B_DEFAULT = 0;
        /// <summary>
        /// Lexical states for SCLEX_VB, SCLEX_VBSCRIPT, SCLEX_POWERBASIC,
        /// SCLEX_BLITZBASIC, SCLEX_PUREBASIC, SCLEX_FREEBASIC.
        /// </summary>
        public const int SCE_B_COMMENT = 1;
        /// <summary>
        /// Lexical states for SCLEX_VB, SCLEX_VBSCRIPT, SCLEX_POWERBASIC,
        /// SCLEX_BLITZBASIC, SCLEX_PUREBASIC, SCLEX_FREEBASIC.
        /// </summary>
        public const int SCE_B_NUMBER = 2;
        /// <summary>
        /// Lexical states for SCLEX_VB, SCLEX_VBSCRIPT, SCLEX_POWERBASIC,
        /// SCLEX_BLITZBASIC, SCLEX_PUREBASIC, SCLEX_FREEBASIC.
        /// </summary>
        public const int SCE_B_KEYWORD = 3;
        /// <summary>
        /// Lexical states for SCLEX_VB, SCLEX_VBSCRIPT, SCLEX_POWERBASIC,
        /// SCLEX_BLITZBASIC, SCLEX_PUREBASIC, SCLEX_FREEBASIC.
        /// </summary>
        public const int SCE_B_STRING = 4;
        /// <summary>
        /// Lexical states for SCLEX_VB, SCLEX_VBSCRIPT, SCLEX_POWERBASIC,
        /// SCLEX_BLITZBASIC, SCLEX_PUREBASIC, SCLEX_FREEBASIC.
        /// </summary>
        public const int SCE_B_PREPROCESSOR = 5;
        /// <summary>
        /// Lexical states for SCLEX_VB, SCLEX_VBSCRIPT, SCLEX_POWERBASIC,
        /// SCLEX_BLITZBASIC, SCLEX_PUREBASIC, SCLEX_FREEBASIC.
        /// </summary>
        public const int SCE_B_OPERATOR = 6;
        /// <summary>
        /// Lexical states for SCLEX_VB, SCLEX_VBSCRIPT, SCLEX_POWERBASIC,
        /// SCLEX_BLITZBASIC, SCLEX_PUREBASIC, SCLEX_FREEBASIC.
        /// </summary>
        public const int SCE_B_IDENTIFIER = 7;
        /// <summary>
        /// Lexical states for SCLEX_VB, SCLEX_VBSCRIPT, SCLEX_POWERBASIC,
        /// SCLEX_BLITZBASIC, SCLEX_PUREBASIC, SCLEX_FREEBASIC.
        /// </summary>
        public const int SCE_B_DATE = 8;
        /// <summary>
        /// Lexical states for SCLEX_VB, SCLEX_VBSCRIPT, SCLEX_POWERBASIC,
        /// SCLEX_BLITZBASIC, SCLEX_PUREBASIC, SCLEX_FREEBASIC.
        /// </summary>
        public const int SCE_B_STRINGEOL = 9;
        /// <summary>
        /// Lexical states for SCLEX_VB, SCLEX_VBSCRIPT, SCLEX_POWERBASIC,
        /// SCLEX_BLITZBASIC, SCLEX_PUREBASIC, SCLEX_FREEBASIC.
        /// </summary>
        public const int SCE_B_KEYWORD2 = 10;
        /// <summary>
        /// Lexical states for SCLEX_VB, SCLEX_VBSCRIPT, SCLEX_POWERBASIC,
        /// SCLEX_BLITZBASIC, SCLEX_PUREBASIC, SCLEX_FREEBASIC.
        /// </summary>
        public const int SCE_B_KEYWORD3 = 11;
        /// <summary>
        /// Lexical states for SCLEX_VB, SCLEX_VBSCRIPT, SCLEX_POWERBASIC,
        /// SCLEX_BLITZBASIC, SCLEX_PUREBASIC, SCLEX_FREEBASIC.
        /// </summary>
        public const int SCE_B_KEYWORD4 = 12;
        /// <summary>
        /// Lexical states for SCLEX_VB, SCLEX_VBSCRIPT, SCLEX_POWERBASIC,
        /// SCLEX_BLITZBASIC, SCLEX_PUREBASIC, SCLEX_FREEBASIC.
        /// </summary>
        public const int SCE_B_CONSTANT = 13;
        /// <summary>
        /// Lexical states for SCLEX_VB, SCLEX_VBSCRIPT, SCLEX_POWERBASIC,
        /// SCLEX_BLITZBASIC, SCLEX_PUREBASIC, SCLEX_FREEBASIC.
        /// </summary>
        public const int SCE_B_ASM = 14;
        /// <summary>
        /// Lexical states for SCLEX_VB, SCLEX_VBSCRIPT, SCLEX_POWERBASIC,
        /// SCLEX_BLITZBASIC, SCLEX_PUREBASIC, SCLEX_FREEBASIC.
        /// </summary>
        public const int SCE_B_LABEL = 15;
        /// <summary>
        /// Lexical states for SCLEX_VB, SCLEX_VBSCRIPT, SCLEX_POWERBASIC,
        /// SCLEX_BLITZBASIC, SCLEX_PUREBASIC, SCLEX_FREEBASIC.
        /// </summary>
        public const int SCE_B_ERROR = 16;
        /// <summary>
        /// Lexical states for SCLEX_VB, SCLEX_VBSCRIPT, SCLEX_POWERBASIC,
        /// SCLEX_BLITZBASIC, SCLEX_PUREBASIC, SCLEX_FREEBASIC.
        /// </summary>
        public const int SCE_B_HEXNUMBER = 17;
        /// <summary>
        /// Lexical states for SCLEX_VB, SCLEX_VBSCRIPT, SCLEX_POWERBASIC,
        /// SCLEX_BLITZBASIC, SCLEX_PUREBASIC, SCLEX_FREEBASIC.
        /// </summary>
        public const int SCE_B_BINNUMBER = 18;
        /// <summary>
        /// Lexical states for SCLEX_VB, SCLEX_VBSCRIPT, SCLEX_POWERBASIC,
        /// SCLEX_BLITZBASIC, SCLEX_PUREBASIC, SCLEX_FREEBASIC.
        /// </summary>
        public const int SCE_B_COMMENTBLOCK = 19;
        /// <summary>
        /// Lexical states for SCLEX_VB, SCLEX_VBSCRIPT, SCLEX_POWERBASIC,
        /// SCLEX_BLITZBASIC, SCLEX_PUREBASIC, SCLEX_FREEBASIC.
        /// </summary>
        public const int SCE_B_DOCLINE = 20;
        /// <summary>
        /// Lexical states for SCLEX_VB, SCLEX_VBSCRIPT, SCLEX_POWERBASIC,
        /// SCLEX_BLITZBASIC, SCLEX_PUREBASIC, SCLEX_FREEBASIC.
        /// </summary>
        public const int SCE_B_DOCBLOCK = 21;
        /// <summary>
        /// Lexical states for SCLEX_VB, SCLEX_VBSCRIPT, SCLEX_POWERBASIC,
        /// SCLEX_BLITZBASIC, SCLEX_PUREBASIC, SCLEX_FREEBASIC.
        /// </summary>
        public const int SCE_B_DOCKEYWORD = 22;

        // Markdown
        /// <summary>
        /// Lexical state for SCLEX_MARKDOWN.
        /// </summary>
        public const int SCE_MARKDOWN_DEFAULT = 0;
        /// <summary>
        /// Lexical state for SCLEX_MARKDOWN.
        /// </summary>
        public const int SCE_MARKDOWN_LINE_BEGIN = 1;
        /// <summary>
        /// Lexical state for SCLEX_MARKDOWN.
        /// </summary>
        public const int SCE_MARKDOWN_STRONG1 = 2;
        /// <summary>
        /// Lexical state for SCLEX_MARKDOWN.
        /// </summary>
        public const int SCE_MARKDOWN_STRONG2 = 3;
        /// <summary>
        /// Lexical state for SCLEX_MARKDOWN.
        /// </summary>
        public const int SCE_MARKDOWN_EM1 = 4;
        /// <summary>
        /// Lexical state for SCLEX_MARKDOWN.
        /// </summary>
        public const int SCE_MARKDOWN_EM2 = 5;
        /// <summary>
        /// Lexical state for SCLEX_MARKDOWN.
        /// </summary>
        public const int SCE_MARKDOWN_HEADER1 = 6;
        /// <summary>
        /// Lexical state for SCLEX_MARKDOWN.
        /// </summary>
        public const int SCE_MARKDOWN_HEADER2 = 7;
        /// <summary>
        /// Lexical state for SCLEX_MARKDOWN.
        /// </summary>
        public const int SCE_MARKDOWN_HEADER3 = 8;
        /// <summary>
        /// Lexical state for SCLEX_MARKDOWN.
        /// </summary>
        public const int SCE_MARKDOWN_HEADER4 = 9;
        /// <summary>
        /// Lexical state for SCLEX_MARKDOWN.
        /// </summary>
        public const int SCE_MARKDOWN_HEADER5 = 10;
        /// <summary>
        /// Lexical state for SCLEX_MARKDOWN.
        /// </summary>
        public const int SCE_MARKDOWN_HEADER6 = 11;
        /// <summary>
        /// Lexical state for SCLEX_MARKDOWN.
        /// </summary>
        public const int SCE_MARKDOWN_PRECHAR = 12;
        /// <summary>
        /// Lexical state for SCLEX_MARKDOWN.
        /// </summary>
        public const int SCE_MARKDOWN_ULIST_ITEM = 13;
        /// <summary>
        /// Lexical state for SCLEX_MARKDOWN.
        /// </summary>
        public const int SCE_MARKDOWN_OLIST_ITEM = 14;
        /// <summary>
        /// Lexical state for SCLEX_MARKDOWN.
        /// </summary>
        public const int SCE_MARKDOWN_BLOCKQUOTE = 15;
        /// <summary>
        /// Lexical state for SCLEX_MARKDOWN.
        /// </summary>
        public const int SCE_MARKDOWN_STRIKEOUT = 16;
        /// <summary>
        /// Lexical state for SCLEX_MARKDOWN.
        /// </summary>
        public const int SCE_MARKDOWN_HRULE = 17;
        /// <summary>
        /// Lexical state for SCLEX_MARKDOWN.
        /// </summary>
        public const int SCE_MARKDOWN_LINK = 18;
        /// <summary>
        /// Lexical state for SCLEX_MARKDOWN.
        /// </summary>
        public const int SCE_MARKDOWN_CODE = 19;
        /// <summary>
        /// Lexical state for SCLEX_MARKDOWN.
        /// </summary>
        public const int SCE_MARKDOWN_CODE2 = 20;
        /// <summary>
        /// Lexical state for SCLEX_MARKDOWN.
        /// </summary>
        public const int SCE_MARKDOWN_CODEBK = 21;

        // R
        /// <summary>
        /// Lexical states for SCLEX_R.
        /// </summary>
        public const int SCE_R_DEFAULT = 0;
        /// <summary>
        /// Lexical states for SCLEX_R.
        /// </summary>
        public const int SCE_R_COMMENT = 1;
        /// <summary>
        /// Lexical states for SCLEX_R.
        /// </summary>
        public const int SCE_R_KWORD = 2;
        /// <summary>
        /// Lexical states for SCLEX_R.
        /// </summary>
        public const int SCE_R_BASEKWORD = 3;
        /// <summary>
        /// Lexical states for SCLEX_R.
        /// </summary>
        public const int SCE_R_OTHERKWORD = 4;
        /// <summary>
        /// Lexical states for SCLEX_R.
        /// </summary>
        public const int SCE_R_NUMBER = 5;
        /// <summary>
        /// Lexical states for SCLEX_R.
        /// </summary>
        public const int SCE_R_STRING = 6;
        /// <summary>
        /// Lexical states for SCLEX_R.
        /// </summary>
        public const int SCE_R_STRING2 = 7;
        /// <summary>
        /// Lexical states for SCLEX_R.
        /// </summary>
        public const int SCE_R_OPERATOR = 8;
        /// <summary>
        /// Lexical states for SCLEX_R.
        /// </summary>
        public const int SCE_R_IDENTIFIER = 9;
        /// <summary>
        /// Lexical states for SCLEX_R.
        /// </summary>
        public const int SCE_R_INFIX = 10;
        /// <summary>
        /// Lexical states for SCLEX_R.
        /// </summary>
        public const int SCE_R_INFIXEOL = 11;

        // Tcl
        /// <summary>
        /// Lexical states for SCLEX_TCL.
        /// </summary>
        public const int SCE_TCL_DEFAULT = 0;
        /// <summary>
        /// Lexical states for SCLEX_TCL.
        /// </summary>
        public const int SCE_TCL_COMMENT = 1;
        /// <summary>
        /// Lexical states for SCLEX_TCL.
        /// </summary>
        public const int SCE_TCL_COMMENTLINE = 2;
        /// <summary>
        /// Lexical states for SCLEX_TCL.
        /// </summary>
        public const int SCE_TCL_NUMBER = 3;
        /// <summary>
        /// Lexical states for SCLEX_TCL.
        /// </summary>
        public const int SCE_TCL_WORD_IN_QUOTE = 4;
        /// <summary>
        /// Lexical states for SCLEX_TCL.
        /// </summary>
        public const int SCE_TCL_IN_QUOTE = 5;
        /// <summary>
        /// Lexical states for SCLEX_TCL.
        /// </summary>
        public const int SCE_TCL_OPERATOR = 6;
        /// <summary>
        /// Lexical states for SCLEX_TCL.
        /// </summary>
        public const int SCE_TCL_IDENTIFIER = 7;
        /// <summary>
        /// Lexical states for SCLEX_TCL.
        /// </summary>
        public const int SCE_TCL_SUBSTITUTION = 8;
        /// <summary>
        /// Lexical states for SCLEX_TCL.
        /// </summary>
        public const int SCE_TCL_SUB_BRACE = 9;
        /// <summary>
        /// Lexical states for SCLEX_TCL.
        /// </summary>
        public const int SCE_TCL_MODIFIER = 10;
        /// <summary>
        /// Lexical states for SCLEX_TCL.
        /// </summary>
        public const int SCE_TCL_EXPAND = 11;
        /// <summary>
        /// Lexical states for SCLEX_TCL.
        /// </summary>
        public const int SCE_TCL_WORD = 12;
        /// <summary>
        /// Lexical states for SCLEX_TCL.
        /// </summary>
        public const int SCE_TCL_WORD2 = 13;
        /// <summary>
        /// Lexical states for SCLEX_TCL.
        /// </summary>
        public const int SCE_TCL_WORD3 = 14;
        /// <summary>
        /// Lexical states for SCLEX_TCL.
        /// </summary>
        public const int SCE_TCL_WORD4 = 15;
        /// <summary>
        /// Lexical states for SCLEX_TCL.
        /// </summary>
        public const int SCE_TCL_WORD5 = 16;
        /// <summary>
        /// Lexical states for SCLEX_TCL.
        /// </summary>
        public const int SCE_TCL_WORD6 = 17;
        /// <summary>
        /// Lexical states for SCLEX_TCL.
        /// </summary>
        public const int SCE_TCL_WORD7 = 18;
        /// <summary>
        /// Lexical states for SCLEX_TCL.
        /// </summary>
        public const int SCE_TCL_WORD8 = 19;
        /// <summary>
        /// Lexical states for SCLEX_TCL.
        /// </summary>
        public const int SCE_TCL_COMMENT_BOX = 20;
        /// <summary>
        /// Lexical states for SCLEX_TCL.
        /// </summary>
        public const int SCE_TCL_BLOCK_COMMENT = 21;

        // Verilog
        /// <summary>
        /// Lexical states for SCLEX_VERILOG.
        /// </summary>
        public const int SCE_V_DEFAULT = 0;
        /// <summary>
        /// Lexical states for SCLEX_VERILOG.
        /// </summary>
        public const int SCE_V_COMMENT = 1;
        /// <summary>
        /// Lexical states for SCLEX_VERILOG.
        /// </summary>
        public const int SCE_V_COMMENTLINE = 2;
        /// <summary>
        /// Lexical states for SCLEX_VERILOG.
        /// </summary>
        public const int SCE_V_COMMENTLINEBANG = 3;
        /// <summary>
        /// Lexical states for SCLEX_VERILOG.
        /// </summary>
        public const int SCE_V_NUMBER = 4;
        /// <summary>
        /// Lexical states for SCLEX_VERILOG.
        /// </summary>
        public const int SCE_V_WORD = 5;
        /// <summary>
        /// Lexical states for SCLEX_VERILOG.
        /// </summary>
        public const int SCE_V_STRING = 6;
        /// <summary>
        /// Lexical states for SCLEX_VERILOG.
        /// </summary>
        public const int SCE_V_WORD2 = 7;
        /// <summary>
        /// Lexical states for SCLEX_VERILOG.
        /// </summary>
        public const int SCE_V_WORD3 = 8;
        /// <summary>
        /// Lexical states for SCLEX_VERILOG.
        /// </summary>
        public const int SCE_V_PREPROCESSOR = 9;
        /// <summary>
        /// Lexical states for SCLEX_VERILOG.
        /// </summary>
        public const int SCE_V_OPERATOR = 10;
        /// <summary>
        /// Lexical states for SCLEX_VERILOG.
        /// </summary>
        public const int SCE_V_IDENTIFIER = 11;
        /// <summary>
        /// Lexical states for SCLEX_VERILOG.
        /// </summary>
        public const int SCE_V_STRINGEOL = 12;
        /// <summary>
        /// Lexical states for SCLEX_VERILOG.
        /// </summary>
        public const int SCE_V_USER = 19;
        /// <summary>
        /// Lexical states for SCLEX_VERILOG.
        /// </summary>
        public const int SCE_V_COMMENT_WORD = 20;
        /// <summary>
        /// Lexical states for SCLEX_VERILOG.
        /// </summary>
        public const int SCE_V_INPUT = 21;
        /// <summary>
        /// Lexical states for SCLEX_VERILOG.
        /// </summary>
        public const int SCE_V_OUTPUT = 22;
        /// <summary>
        /// Lexical states for SCLEX_VERILOG.
        /// </summary>
        public const int SCE_V_INOUT = 23;
        /// <summary>
        /// Lexical states for SCLEX_VERILOG.
        /// </summary>
        public const int SCE_V_PORT_CONNECT = 24;

        #endregion Lexer Constants

        #region Callbacks

        /// <summary>
        /// Represents the Scintilla direct-function entry point obtained via
        /// SCI_GETDIRECTFUNCTION. Invoking it dispatches a message straight to
        /// the control, bypassing the window message queue.
        /// </summary>
        /// <param name="ptr">
        /// The opaque direct pointer returned by SCI_GETDIRECTPOINTER that
        /// identifies the target Scintilla document and view.
        /// </param>
        /// <param name="iMessage">
        /// The Scintilla message code (one of the SCI_* values) to dispatch.
        /// </param>
        /// <param name="wParam">
        /// The first message-specific parameter.
        /// </param>
        /// <param name="lParam">
        /// The second message-specific parameter.
        /// </param>
        /// <returns>
        /// The message-specific result returned by the Scintilla control.
        /// </returns>
        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        [ObjectId("60d73d32-6699-4c39-8db9-6335af25aa58")]
        public delegate IntPtr Scintilla_DirectFunction(
            IntPtr ptr, int iMessage, IntPtr wParam, IntPtr lParam);

        ///////////////////////////////////////////////////////////////////////

        // Lexilla's CreateLexer(const char *name) -> ILexer5*. The name is
        // passed as a pointer to a NUL-terminated UTF-8 buffer (portable; no
        // ANSI code-page assumption). Winapi = __stdcall on Windows x86
        // (matching LEXILLA_CALL), cdecl on x64/ARM/ARM64 and non-Windows.
        /// <summary>
        /// Represents Lexilla's exported CreateLexer function, which creates a
        /// lexer instance (ILexer5*) for the named language.
        /// </summary>
        /// <param name="name">
        /// A pointer to a NUL-terminated UTF-8 buffer holding the lexer name.
        /// </param>
        /// <returns>
        /// A pointer to the newly created lexer, or a null pointer when no
        /// lexer matches the supplied name.
        /// </returns>
        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        [ObjectId("e2b1e1e1-5484-43ae-9b0b-d4cb56ccf6fe")]
        public delegate IntPtr Lexilla_CreateLexer(IntPtr name);

        #endregion Callbacks

        #region Functions

        /// <summary>
        /// Closes the clipboard, releasing the lock taken by a prior call to
        /// OpenClipboard so that other windows can access it.
        /// </summary>
        /// <returns>
        /// true if the clipboard was closed successfully; otherwise, false.
        /// </returns>
        [DllImport(DLL_NAME_USER32, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool CloseClipboard();

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Retrieves the address of an exported function or variable from the
        /// specified dynamic-link library (DLL) module.
        /// </summary>
        /// <param name="hModule">
        /// A handle to the DLL module that contains the function or variable.
        /// </param>
        /// <param name="lpProcName">
        /// The name of the exported function or variable to locate.
        /// </param>
        /// <returns>
        /// The address of the exported function or variable, or
        /// <see cref="IntPtr.Zero" /> if the export cannot be found.
        /// </returns>
        [DllImport(DLL_NAME_KERNEL32, CharSet = CharSet.Ansi,
            ExactSpelling = true, SetLastError = true)]
        public static extern IntPtr GetProcAddress(
            HandleRef hModule, string lpProcName);

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Empties the clipboard and frees the handles to the data it holds,
        /// then assigns ownership to the window that has it open.
        /// </summary>
        /// <returns>
        /// true if the clipboard was emptied successfully; otherwise, false.
        /// </returns>
        [DllImport(DLL_NAME_USER32, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool EmptyClipboard();

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Loads the specified module into the address space of the calling
        /// process.
        /// </summary>
        /// <param name="lpFileName">
        /// The name of the module (library) to load.
        /// </param>
        /// <returns>
        /// A handle to the loaded module, or <see cref="IntPtr.Zero" /> if the
        /// module could not be loaded.
        /// </returns>
        [DllImport(DLL_NAME_KERNEL32, EntryPoint = "LoadLibraryW",
            CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern IntPtr LoadLibrary(string lpFileName);

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// The LOAD_WITH_ALTERED_SEARCH_PATH constant.
        /// </summary>
        public const uint LOAD_WITH_ALTERED_SEARCH_PATH = 0x00000008;

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Loads the specified module into the address space of the calling
        /// process, using the supplied flags to control how the search for the
        /// module is performed.
        /// </summary>
        /// <param name="lpFileName">
        /// The name of the module (library) to load.
        /// </param>
        /// <param name="hFile">
        /// Reserved; must be <see cref="IntPtr.Zero" />.
        /// </param>
        /// <param name="dwFlags">
        /// Flags that control how the module is loaded, such as
        /// <see cref="LOAD_WITH_ALTERED_SEARCH_PATH" />.
        /// </param>
        /// <returns>
        /// A handle to the loaded module, or <see cref="IntPtr.Zero" /> if the
        /// module could not be loaded.
        /// </returns>
        [DllImport(DLL_NAME_KERNEL32, EntryPoint = "LoadLibraryExW",
            CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern IntPtr LoadLibraryEx(
            string lpFileName, IntPtr hFile, uint dwFlags);

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// The GMEM_MOVEABLE constant.
        /// </summary>
        public const uint GMEM_MOVEABLE = 0x0002;

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Allocates the specified number of bytes from the heap.
        /// </summary>
        /// <param name="uFlags">
        /// The memory-allocation attributes, such as
        /// <see cref="GMEM_MOVEABLE" />.
        /// </param>
        /// <param name="dwBytes">
        /// The number of bytes to allocate.
        /// </param>
        /// <returns>
        /// A handle to the newly allocated memory object, or
        /// <see cref="IntPtr.Zero" /> if the allocation fails.
        /// </returns>
        [DllImport(DLL_NAME_KERNEL32, SetLastError = true)]
        public static extern IntPtr GlobalAlloc(uint uFlags, UIntPtr dwBytes);

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Locks a global memory object and returns a pointer to the first
        /// byte of the object's memory block.
        /// </summary>
        /// <param name="hMem">
        /// A handle to the global memory object to lock.
        /// </param>
        /// <returns>
        /// A pointer to the first byte of the memory block, or
        /// <see cref="IntPtr.Zero" /> if the object could not be locked.
        /// </returns>
        [DllImport(DLL_NAME_KERNEL32, SetLastError = true)]
        public static extern IntPtr GlobalLock(IntPtr hMem);

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Decrements the lock count of a global memory object that was
        /// previously locked by <see cref="GlobalLock" />.
        /// </summary>
        /// <param name="hMem">
        /// A handle to the global memory object to unlock.
        /// </param>
        /// <returns>
        /// true if the object remains locked after decrementing the lock
        /// count; otherwise, false.
        /// </returns>
        [DllImport(DLL_NAME_KERNEL32, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GlobalUnlock(IntPtr hMem);

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Frees the specified global memory object and invalidates its
        /// handle.
        /// </summary>
        /// <param name="hMem">
        /// A handle to the global memory object to free.
        /// </param>
        /// <returns>
        /// <see cref="IntPtr.Zero" /> if the object is freed successfully;
        /// otherwise, a handle to the memory object.
        /// </returns>
        [DllImport(DLL_NAME_KERNEL32, SetLastError = true)]
        public static extern IntPtr GlobalFree(IntPtr hMem);

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Copies a block of memory from one location to another, correctly
        /// handling overlapping source and destination regions.
        /// </summary>
        /// <param name="destination">
        /// A pointer to the starting address of the destination block.
        /// </param>
        /// <param name="source">
        /// A pointer to the starting address of the source block.
        /// </param>
        /// <param name="length">
        /// The number of bytes to copy.
        /// </param>
        [DllImport(DLL_NAME_KERNEL32, EntryPoint = "RtlMoveMemory")]
        public static extern void MoveMemory(
            IntPtr destination, IntPtr source, UIntPtr length);

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Opens the clipboard for examination and prevents other windows from
        /// modifying its contents.
        /// </summary>
        /// <param name="hWndNewOwner">
        /// A handle to the window to associate with the open clipboard, or
        /// <see cref="IntPtr.Zero" /> to associate it with the current task.
        /// </param>
        /// <returns>
        /// true if the clipboard was opened successfully; otherwise, false.
        /// </returns>
        [DllImport(DLL_NAME_USER32, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool OpenClipboard(IntPtr hWndNewOwner);

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Registers a new clipboard format with the given name, returning a
        /// value that identifies the format.
        /// </summary>
        /// <param name="lpszFormat">
        /// The name of the clipboard format to register.
        /// </param>
        /// <returns>
        /// The identifier of the registered clipboard format, or zero if the
        /// format could not be registered.
        /// </returns>
        [DllImport(DLL_NAME_USER32, SetLastError = true)]
        public static extern uint RegisterClipboardFormat(string lpszFormat);

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Revokes the registration of the specified window as a target for
        /// OLE drag-and-drop operations.
        /// </summary>
        /// <param name="hwnd">
        /// A handle to the window whose drag-and-drop registration is revoked.
        /// </param>
        /// <returns>
        /// An HRESULT status code; S_OK (zero) indicates success.
        /// </returns>
        [DllImport(DLL_NAME_OLE32, ExactSpelling = true)]
        public static extern int RevokeDragDrop(IntPtr hwnd);

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Sends the specified message to a window, invoking its window
        /// procedure and waiting until the message has been processed.
        /// </summary>
        /// <param name="hWnd">
        /// A handle to the window whose window procedure receives the message.
        /// </param>
        /// <param name="msg">
        /// The message to send.
        /// </param>
        /// <param name="wParam">
        /// The first message-specific parameter.
        /// </param>
        /// <param name="lParam">
        /// The second message-specific parameter.
        /// </param>
        /// <returns>
        /// The result of the message processing, which depends on the message
        /// that was sent.
        /// </returns>
        [DllImport(DLL_NAME_USER32, EntryPoint = "SendMessageW",
            CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern IntPtr SendMessage(
            HandleRef hWnd, int msg, IntPtr wParam, IntPtr lParam);

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Determines whether one window is a child or descendant of another
        /// specified parent window.
        /// </summary>
        /// <param name="hWndParent">
        /// A handle to the candidate parent window.
        /// </param>
        /// <param name="hWndChild">
        /// A handle to the window to test.
        /// </param>
        /// <returns>
        /// true if the window is a child or descendant of the parent window;
        /// otherwise, false.
        /// </returns>
        [DllImport(DLL_NAME_USER32)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool IsChild(
            HandleRef hWndParent, IntPtr hWndChild);

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// The GW_OWNER constant.
        /// </summary>
        public const uint GW_OWNER = 4;

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Retrieves a handle to a window that has the specified relationship
        /// to the given window.
        /// </summary>
        /// <param name="hWnd">
        /// A handle to the window whose related window is retrieved.
        /// </param>
        /// <param name="uCmd">
        /// The relationship between the given window and the one to retrieve,
        /// such as <see cref="GW_OWNER" />.
        /// </param>
        /// <returns>
        /// A handle to the related window, or <see cref="IntPtr.Zero" /> if no
        /// such window exists.
        /// </returns>
        [DllImport(DLL_NAME_USER32)]
        public static extern IntPtr GetWindow(IntPtr hWnd, uint uCmd);

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Places data of the specified clipboard format on the clipboard,
        /// which must already be open.
        /// </summary>
        /// <param name="uFormat">
        /// The clipboard format of the data being supplied.
        /// </param>
        /// <param name="hMem">
        /// A handle to the data in the given format.
        /// </param>
        /// <returns>
        /// A handle to the data if it was placed on the clipboard; otherwise,
        /// <see cref="IntPtr.Zero" />.
        /// </returns>
        [DllImport(DLL_NAME_USER32, SetLastError = true)]
        public static extern IntPtr SetClipboardData(
            uint uFormat, IntPtr hMem);

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Changes the parent window of the specified child window.
        /// </summary>
        /// <param name="hWndChild">
        /// A handle to the child window whose parent is being changed.
        /// </param>
        /// <param name="hWndNewParent">
        /// A handle to the new parent window, or <see cref="IntPtr.Zero" /> to
        /// make the desktop window the new parent.
        /// </param>
        /// <returns>
        /// A handle to the previous parent window, or
        /// <see cref="IntPtr.Zero" /> if the function fails.
        /// </returns>
        [DllImport(DLL_NAME_USER32, SetLastError = true)]
        public static extern IntPtr SetParent(
            IntPtr hWndChild, IntPtr hWndNewParent);

        #endregion Functions

        #region Structures

        // http://www.openrce.org/articles/full_view/23
        // It's worth noting that this structure (and the 64-bit version below)
        // represents the ILoader class virtual function table (vtable), NOT
        // the ILoader interface defined in ILexer.h. In this case they are
        // identical because the ILoader class contains only functions.
        /// <summary>
        /// Mirrors the native Scintilla 32-bit <c>ILoader</c> virtual function
        /// table (vtable), whose slots use the __stdcall calling convention.
        /// It streams document data into a native loader on 32-bit Windows.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        [ObjectId("551c3781-b02c-43dd-bb29-ff6aab06d72d")]
        public unsafe struct ILoaderVTable32
        {
            /// <summary>
            /// The slot holding the loader's Release function pointer.
            /// </summary>
            public ReleaseDelegate Release;

            ///////////////////////////////////////////////////////////////////

            /// <summary>
            /// The slot holding the loader's AddData function pointer.
            /// </summary>
            public AddDataDelegate AddData;

            ///////////////////////////////////////////////////////////////////

            /// <summary>
            /// The slot holding the loader's ConvertToDocument function
            /// pointer.
            /// </summary>
            public ConvertToDocumentDelegate ConvertToDocument;

            ///////////////////////////////////////////////////////////////////

            /// <summary>
            /// Represents the loader's Release method, which decrements the
            /// loader's reference count and frees it when the count reaches
            /// zero.
            /// </summary>
            /// <param name="self">
            /// A pointer to the loader instance (the implicit this pointer).
            /// </param>
            /// <returns>
            /// The reference count remaining after the call.
            /// </returns>
            [UnmanagedFunctionPointer(CallingConvention.StdCall)]
            [ObjectId("1960346f-52f5-4c6f-b62f-cafded39ca89")]
            public delegate int ReleaseDelegate(IntPtr self);

            ///////////////////////////////////////////////////////////////////

            /// <summary>
            /// Represents the loader's AddData method, which appends a block
            /// of bytes to the document being loaded.
            /// </summary>
            /// <param name="self">
            /// A pointer to the loader instance (the implicit this pointer).
            /// </param>
            /// <param name="data">
            /// A pointer to the block of bytes to append to the document.
            /// </param>
            /// <param name="length">
            /// The number of bytes to append.
            /// </param>
            /// <returns>
            /// A status code; zero indicates success.
            /// </returns>
            [UnmanagedFunctionPointer(CallingConvention.StdCall)]
            [ObjectId("360ad629-a852-4d26-b7ae-9023138e28a3")]
            public delegate int AddDataDelegate(
                IntPtr self, byte* data, IntPtr length);

            ///////////////////////////////////////////////////////////////////

            /// <summary>
            /// Represents the loader's ConvertToDocument method, which
            /// finalizes loading and returns the completed document.
            /// </summary>
            /// <param name="self">
            /// A pointer to the loader instance (the implicit this pointer).
            /// </param>
            /// <returns>
            /// A pointer to the completed Scintilla document.
            /// </returns>
            [UnmanagedFunctionPointer(CallingConvention.StdCall)]
            [ObjectId("d44b9656-437a-4c36-91f0-ac736ccc0f7a")]
            public delegate IntPtr ConvertToDocumentDelegate(IntPtr self);
        }

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Mirrors the native Scintilla 64-bit <c>ILoader</c> virtual function
        /// table (vtable), whose slots use the cdecl calling convention. It
        /// streams document data into a native loader on 64-bit and
        /// non-Windows targets.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        [ObjectId("a9ce0ee8-375b-42a7-92cd-ac16a7e45508")]
        public unsafe struct ILoaderVTable64
        {
            /// <summary>
            /// The slot holding the loader's Release function pointer.
            /// </summary>
            public ReleaseDelegate Release;

            ///////////////////////////////////////////////////////////////////

            /// <summary>
            /// The slot holding the loader's AddData function pointer.
            /// </summary>
            public AddDataDelegate AddData;

            ///////////////////////////////////////////////////////////////////

            /// <summary>
            /// The slot holding the loader's ConvertToDocument function
            /// pointer.
            /// </summary>
            public ConvertToDocumentDelegate ConvertToDocument;

            ///////////////////////////////////////////////////////////////////

            /// <summary>
            /// Represents the loader's Release method, which decrements the
            /// loader's reference count and frees it when the count reaches
            /// zero.
            /// </summary>
            /// <param name="self">
            /// A pointer to the loader instance (the implicit this pointer).
            /// </param>
            /// <returns>
            /// The reference count remaining after the call.
            /// </returns>
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            [ObjectId("0d68a9e0-d5c0-4e12-8506-110235e7a834")]
            public delegate int ReleaseDelegate(IntPtr self);

            ///////////////////////////////////////////////////////////////////

            /// <summary>
            /// Represents the loader's AddData method, which appends a block
            /// of bytes to the document being loaded.
            /// </summary>
            /// <param name="self">
            /// A pointer to the loader instance (the implicit this pointer).
            /// </param>
            /// <param name="data">
            /// A pointer to the block of bytes to append to the document.
            /// </param>
            /// <param name="length">
            /// The number of bytes to append.
            /// </param>
            /// <returns>
            /// A status code; zero indicates success.
            /// </returns>
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            [ObjectId("2d7d4908-6a50-4b6c-8f56-9f29d4d8f111")]
            public delegate int AddDataDelegate(
                IntPtr self, byte* data, IntPtr length);

            ///////////////////////////////////////////////////////////////////

            /// <summary>
            /// Represents the loader's ConvertToDocument method, which
            /// finalizes loading and returns the completed document.
            /// </summary>
            /// <param name="self">
            /// A pointer to the loader instance (the implicit this pointer).
            /// </param>
            /// <returns>
            /// A pointer to the completed Scintilla document.
            /// </returns>
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            [ObjectId("d48b88fc-af1e-44ec-b396-ff99027fc6c0")]
            public delegate IntPtr ConvertToDocumentDelegate(IntPtr self);
        }

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Mirrors the native Scintilla <c>Sci_CharacterRange</c> structure,
        /// which describes a range of text using 32-bit character positions.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        [ObjectId("64957b32-2c76-4690-b3ee-5630c65a8671")]
        public struct Sci_CharacterRange
        {
            /// <summary>
            /// The position of the first character in the range.
            /// </summary>
            public int cpMin;

            ///////////////////////////////////////////////////////////////////

            /// <summary>
            /// The position one past the last character in the range.
            /// </summary>
            public int cpMax;
        }

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Mirrors the native Scintilla <c>Sci_NotifyHeader</c> structure (the
        /// Win32 NMHDR), the common header of every notification.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        [ObjectId("3606b289-6329-48d6-a5b2-525fd2e41f59")]
        public struct Sci_NotifyHeader
        {
            /// <summary>
            /// A handle to the control that sent the notification.
            /// </summary>
            public IntPtr hwndFrom;

            ///////////////////////////////////////////////////////////////////

            /// <summary>
            /// The identifier of the control that sent the notification.
            /// </summary>
            public IntPtr idFrom;

            ///////////////////////////////////////////////////////////////////

            /// <summary>
            /// The notification code (one of the SCN_* values).
            /// </summary>
            public int code;
        }

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Mirrors the native Scintilla <c>Sci_TextRange</c> structure, which
        /// pairs a character range with a buffer that receives its text.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        [ObjectId("936b1024-e51c-4689-af79-fd5a95057daa")]
        public struct Sci_TextRange
        {
            /// <summary>
            /// The range of characters whose text is retrieved.
            /// </summary>
            public Sci_CharacterRange chrg;

            ///////////////////////////////////////////////////////////////////

            /// <summary>
            /// A pointer to the buffer that receives the retrieved text.
            /// </summary>
            public IntPtr lpstrText;
        }

        ///////////////////////////////////////////////////////////////////////

        // 64-bit range structs (Scintilla 5.x SCI_*FULL messages). cpMin/cpMax
        // are Sci_Position (ptrdiff_t / pointer-sized), so IntPtr here --
        // correct on every architecture AND OS, unlike Sci_CharacterRange
        // whose Sci_PositionCR (C long) is 32-bit on Windows but 64-bit on
        // Linux/macOS. Prefer these over the legacy CHARRANGE-compatible
        // structs.
        /// <summary>
        /// Mirrors the native Scintilla <c>Sci_CharacterRangeFull</c>
        /// structure, which describes a range of text using pointer-sized
        /// character positions.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        [ObjectId("8d59e7e1-4975-410d-be74-8728ca8ed4e1")]
        public struct Sci_CharacterRangeFull
        {
            /// <summary>
            /// The position of the first character in the range.
            /// </summary>
            public IntPtr cpMin;

            ///////////////////////////////////////////////////////////////////

            /// <summary>
            /// The position one past the last character in the range.
            /// </summary>
            public IntPtr cpMax;
        }

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Mirrors the native Scintilla <c>Sci_TextRangeFull</c> structure,
        /// which pairs a pointer-sized character range with a buffer that
        /// receives its text.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        [ObjectId("a6f86fd0-b5c6-430a-ae05-816681d383d5")]
        public struct Sci_TextRangeFull
        {
            /// <summary>
            /// The range of characters whose text is retrieved.
            /// </summary>
            public Sci_CharacterRangeFull chrg;

            ///////////////////////////////////////////////////////////////////

            /// <summary>
            /// A pointer to the buffer that receives the retrieved text.
            /// </summary>
            public IntPtr lpstrText;
        }

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Mirrors the native Scintilla <c>SCNotification</c> structure, which
        /// carries the details of a notification raised by the control.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        [ObjectId("535f0e46-59d9-4a7d-8502-0f0a5ac8de53")]
        public struct SCNotification
        {
            /// <summary>
            /// The common notification header carried by every Scintilla
            /// notification.
            /// </summary>
            public Sci_NotifyHeader nmhdr;

            ///////////////////////////////////////////////////////////////////

            // NOTE: position/length/linesAdded/line/annotationLinesAdded are
            //       the Scintilla "Sci_Position" fields. They are
            //       pointer-sized (ptrdiff_t) in Scintilla 3.8.0+ -- which
            //       includes the deployed native library (4.x) -- so they MUST
            //       be IntPtr here. As 32-bit "int" they would misalign every
            //       field after the first one on x64 and corrupt SCN_MODIFIED
            //       (garbage length/linesAdded/line). Consumers read them via
            //       IntPtr.ToInt32() (document positions < 2 GB). If the
            //       native library is ever downgraded below 3.8.0 (int
            //       Sci_Position) these five would need to revert to int.
            //       text, wParam and lParam are genuine pointers
            //       (pointer-sized in every version) and always stay IntPtr.
            /// <summary>
            /// The document position associated with the notification.
            /// </summary>
            public IntPtr position;

            ///////////////////////////////////////////////////////////////////

            /// <summary>
            /// The character or key code associated with the notification.
            /// </summary>
            public int ch;

            ///////////////////////////////////////////////////////////////////

            /// <summary>
            /// The key-modifier flags (SCMOD_* values) that were in effect.
            /// </summary>
            public int modifiers;

            ///////////////////////////////////////////////////////////////////

            /// <summary>
            /// The bit flags describing the kind of change, for SCN_MODIFIED
            /// notifications.
            /// </summary>
            public int modificationType;

            ///////////////////////////////////////////////////////////////////

            /// <summary>
            /// A pointer to text associated with the notification, when one is
            /// supplied.
            /// </summary>
            public IntPtr text;

            ///////////////////////////////////////////////////////////////////

            /// <summary>
            /// The length, in bytes, of the associated text or affected range.
            /// </summary>
            public IntPtr length;

            ///////////////////////////////////////////////////////////////////

            /// <summary>
            /// The number of lines added, or removed when negative, by a
            /// modification.
            /// </summary>
            public IntPtr linesAdded;

            ///////////////////////////////////////////////////////////////////

            /// <summary>
            /// The message code recorded for an SCN_MACRORECORD notification.
            /// </summary>
            public int message;

            ///////////////////////////////////////////////////////////////////

            /// <summary>
            /// The wParam value of the recorded message or reported
            /// notification.
            /// </summary>
            public IntPtr wParam;

            ///////////////////////////////////////////////////////////////////

            /// <summary>
            /// The lParam value of the recorded message or reported
            /// notification.
            /// </summary>
            public IntPtr lParam;

            ///////////////////////////////////////////////////////////////////

            /// <summary>
            /// The line number associated with the notification.
            /// </summary>
            public IntPtr line;

            ///////////////////////////////////////////////////////////////////

            /// <summary>
            /// The new fold level of the affected line, for fold changes.
            /// </summary>
            public int foldLevelNow;

            ///////////////////////////////////////////////////////////////////

            /// <summary>
            /// The previous fold level of the affected line, for fold changes.
            /// </summary>
            public int foldLevelPrev;

            ///////////////////////////////////////////////////////////////////

            /// <summary>
            /// The margin that was clicked, for SCN_MARGINCLICK notifications.
            /// </summary>
            public int margin;

            ///////////////////////////////////////////////////////////////////

            /// <summary>
            /// The identifier of the user list, for SCN_USERLISTSELECTION
            /// notifications.
            /// </summary>
            public int listType;

            ///////////////////////////////////////////////////////////////////

            /// <summary>
            /// The x-coordinate, in pixels, associated with the notification.
            /// </summary>
            public int x;

            ///////////////////////////////////////////////////////////////////

            /// <summary>
            /// The y-coordinate, in pixels, associated with the notification.
            /// </summary>
            public int y;

            ///////////////////////////////////////////////////////////////////

            /// <summary>
            /// The container-supplied token, for container-based
            /// modifications.
            /// </summary>
            public int token;

            ///////////////////////////////////////////////////////////////////

            /// <summary>
            /// The number of annotation lines added, or removed when negative.
            /// </summary>
            public IntPtr annotationLinesAdded;

            ///////////////////////////////////////////////////////////////////

            /// <summary>
            /// The SC_UPDATE_* flags describing what changed, for
            /// SCN_UPDATEUI.
            /// </summary>
            public int updated;

            ///////////////////////////////////////////////////////////////////

            /// <summary>
            /// The method by which an autocompletion or user list was
            /// completed.
            /// </summary>
            public int listCompletionMethod;

            ///////////////////////////////////////////////////////////////////

            /// <summary>
            /// The source of an added character, for SCN_CHARADDED
            /// notifications.
            /// </summary>
            public int characterSource;
        }

        #endregion Structures
    }
}
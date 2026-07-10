/*
 * LineCollection.cs --
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
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace ScintillaNET
{
    /// <summary>
    /// An immutable collection of lines of text in a
    /// <see cref="Scintilla" /> control.
    /// </summary>
    [ObjectId("f4e77c9f-b667-464f-a5b5-dd715d9100c1")]
    public class LineCollection : IEnumerable<Line>
    {
        #region Private Data
        /// <summary>
        /// The <see cref="Scintilla" /> control that owns this collection.
        /// </summary>
        private readonly Scintilla scintilla;

        /// <summary>
        /// The per-line data, one entry per line plus a terminal entry,
        /// tracking the starting CHARACTER position of each line.
        /// </summary>
        private GapBuffer<PerLine> perLineData;

        /// <summary>
        /// The line index at which the "step" break occurs.  The step is a
        /// break in the continuity of our line starts; it allows us to delay
        /// the updating of every line start when text is inserted or deleted.
        /// </summary>
        private long stepLine;

        /// <summary>
        /// The pending CHARACTER-length adjustment that applies to every line
        /// after <see cref="stepLine" />.
        /// </summary>
        private long stepLength;
        #endregion

        ///////////////////////////////////////////////////////////////////////

        #region Public Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="LineCollection" />
        /// class.
        /// </summary>
        /// <param name="scintilla">
        /// The <see cref="Scintilla" /> control that created this collection.
        /// </param>
        public LineCollection(
            Scintilla scintilla /* in */
            )
        {
            this.scintilla = scintilla;
            this.scintilla.SCNotification += scintilla_SCNotification;

            this.perLineData = new GapBuffer<PerLine>();
            this.perLineData.Add(new PerLine(0));
            this.perLineData.Add(new PerLine(0)); // Terminal
        }
        #endregion

        ///////////////////////////////////////////////////////////////////////

        #region Public Properties
        /// <summary>
        /// Gets a value indicating whether all the document lines are visible
        /// (not hidden).
        /// </summary>
        public bool AllLinesVisible
        {
            get
            {
                return (scintilla.DirectMessage(
                    NativeMethods.SCI_GETALLLINESVISIBLE) != IntPtr.Zero);
            }
        }

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Gets the number of lines in the <see cref="LineCollection" />.
        /// </summary>
        public long Count
        {
            get
            {
                //
                // Subtract the terminal line.
                //
                return (perLineData.Count - 1);
            }
        }

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Gets the <see cref="Line" /> at the specified zero-based index.
        /// </summary>
        /// <param name="index">
        /// The zero-based index of the <see cref="Line" /> to get.
        /// </param>
        public Line this[long index]
        {
            get
            {
                index = Helpers.Clamp(index, 0, Count - 1);

                return new Line(scintilla, index);
            }
        }
        #endregion

        ///////////////////////////////////////////////////////////////////////

        #region Internal Properties
        /// <summary>
        /// Gets the number of CHARACTERS in the document.
        /// </summary>
        internal long TextLength
        {
            get
            {
                //
                // Where the terminal line begins.
                //
                return CharPositionFromLine(perLineData.Count - 1);
            }
        }
        #endregion

        ///////////////////////////////////////////////////////////////////////

        #region Public Methods
#if DEBUG
        /// <summary>
        /// Dumps the line buffer to a string.
        /// </summary>
        /// <returns>
        /// A string representing the line buffer.
        /// </returns>
        public string Dump()
        {
            using (StringWriter writer = new StringWriter())
            {
                scintilla.Lines.Dump(writer);

                return writer.ToString();
            }
        }

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Dumps the line buffer to the specified <see cref="TextWriter" />.
        /// </summary>
        /// <param name="writer">
        /// The writer to use for dumping the line buffer.
        /// </param>
        public unsafe void Dump(
            TextWriter writer /* in */
            )
        {
            int totalChars = 0;

            for (int i = 0; i < perLineData.Count; i++)
            {
                string error = totalChars == CharPositionFromLine(i) ?
                    null : "*";

                if (i == perLineData.Count - 1)
                {
                    writer.WriteLine(
                        "{0}[{1}] {2} (terminal)", error, i,
                        CharPositionFromLine(i));
                }
                else
                {
                    int len = scintilla.DirectMessage(
                        NativeMethods.SCI_GETLINE, new IntPtr(i)).ToInt32();

                    byte[] bytes = new byte[len];

                    fixed (byte* ptr = bytes)
                        scintilla.DirectMessage(
                            NativeMethods.SCI_GETLINE, new IntPtr(i),
                            new IntPtr(ptr));

                    string str = scintilla.Encoding.GetString(bytes);
                    string containsMultibyte = "U";

                    if (perLineData[i].ContainsMultibyte ==
                            ContainsMultibyte.Yes)
                        containsMultibyte = "Y";
                    else if (perLineData[i].ContainsMultibyte ==
                            ContainsMultibyte.No)
                        containsMultibyte = "N";

                    writer.WriteLine(
                        "{0}[{1}] {2}:{3}:{4} {5}", error, i,
                        CharPositionFromLine(i), str.Length, containsMultibyte,
                        str.Replace("\r", "\\r").Replace("\n", "\\n"));

                    totalChars += str.Length;
                }
            }
        }
#endif
        #endregion

        ///////////////////////////////////////////////////////////////////////

        #region Internal Methods
        /// <summary>
        /// Converts a BYTE offset to a CHARACTER offset.
        /// </summary>
        /// <param name="pos">
        /// The zero-based BYTE offset to convert.
        /// </param>
        /// <returns>
        /// The zero-based CHARACTER offset corresponding to
        /// <paramref name="pos" />.
        /// </returns>
        internal long ByteToCharPosition(
            long pos /* in */
            )
        {
            Debug.Assert(pos >= 0);
            Debug.Assert(pos <= scintilla.DirectMessage(
                NativeMethods.SCI_GETLENGTH).ToInt64());

            long line = scintilla.DirectMessage(
                NativeMethods.SCI_LINEFROMPOSITION,
                new IntPtr(pos)).ToInt64();

            long byteStart = scintilla.DirectMessage(
                NativeMethods.SCI_POSITIONFROMLINE,
                new IntPtr(line)).ToInt64();

            long count = CharPositionFromLine(line) +
                GetCharCount(byteStart, pos - byteStart);

            return count;
        }

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Returns the number of CHARACTERS in a line.
        /// </summary>
        /// <param name="index">
        /// The zero-based index of the line.
        /// </param>
        /// <returns>
        /// The number of CHARACTERS in the line at
        /// <paramref name="index" />.
        /// </returns>
        internal long CharLineLength(
            long index /* in */
            )
        {
            Debug.Assert(index >= 0);
            Debug.Assert(index < Count);

            //
            // A line's length is calculated by subtracting its start offset
            // from the start of the line following.  We keep a terminal
            // (faux) line at the end of the list so we can calculate the
            // length of the last line.
            //
            if (index + 1 <= stepLine)
                return perLineData[(int)(index + 1)].Start -
                    perLineData[(int)index].Start;
            else if (index <= stepLine)
                return (perLineData[(int)(index + 1)].Start + stepLength) -
                    perLineData[(int)index].Start;
            else
                return (perLineData[(int)(index + 1)].Start + stepLength) -
                    (perLineData[(int)index].Start + stepLength);
        }

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Returns the CHARACTER offset where the line begins.
        /// </summary>
        /// <param name="index">
        /// The zero-based index of the line.
        /// </param>
        /// <returns>
        /// The CHARACTER offset where the line at
        /// <paramref name="index" /> begins.
        /// </returns>
        internal long CharPositionFromLine(
            long index /* in */
            )
        {
            Debug.Assert(index >= 0);
            // Allow query of terminal line start
            Debug.Assert(index < perLineData.Count);

            long start = perLineData[(int)index].Start;

            if (index > stepLine)
                start += stepLength;

            return start;
        }

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Converts a CHARACTER offset to a BYTE offset.
        /// </summary>
        /// <param name="pos">
        /// The zero-based CHARACTER offset to convert.
        /// </param>
        /// <returns>
        /// The zero-based BYTE offset corresponding to
        /// <paramref name="pos" />.
        /// </returns>
        internal long CharToBytePosition(
            long pos /* in */
            )
        {
            Debug.Assert(pos >= 0);
            Debug.Assert(pos <= TextLength);

            //
            // Adjust to the nearest line start.
            //
            long line = LineFromCharPosition(pos);

            long lineByteStart = scintilla.DirectMessage(
                NativeMethods.SCI_POSITIONFROMLINE,
                new IntPtr(line)).ToInt64();

            pos -= CharPositionFromLine(line);

            if (pos <= 0)
                return lineByteStart;

            //
            // Optimization when the line contains NO multibyte characters.
            //
            if (!LineContainsMultibyteChar(line))
                return (lineByteStart + pos);

            //
            // Find the byte offset within the line whose CHARACTER count
            // equals "pos", using the same whole-buffer decoder that
            // ByteToCharPosition / GetCharCount use.  This makes
            // CharToBytePosition the exact inverse of ByteToCharPosition,
            // so the map stays self-consistent even for malformed UTF-8 --
            // where native SCI_POSITIONRELATIVE classifies invalid bytes
            // differently than the .NET decoder that produces the char
            // counts (and the "Text" a caller sees).
            //
            long lineByteLength = scintilla.DirectMessage(
                NativeMethods.SCI_LINELENGTH, new IntPtr(line)).ToInt64();

            IntPtr ptr = scintilla.DirectMessage(
                NativeMethods.SCI_GETRANGEPOINTER, new IntPtr(lineByteStart),
                new IntPtr(lineByteLength));

            int lo = 0;
            int hi = (int)lineByteLength;

            while (lo < hi)
            {
                int mid = lo + ((hi - lo) / 2);

                if (GetCharCount(ptr, mid, scintilla.Encoding) < pos)
                    lo = mid + 1;
                else
                    hi = mid;
            }

            return lineByteStart + lo;
        }

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Returns the line index containing the CHARACTER position.
        /// </summary>
        /// <param name="pos">
        /// The zero-based CHARACTER position to locate.
        /// </param>
        /// <returns>
        /// The zero-based index of the line containing
        /// <paramref name="pos" />.
        /// </returns>
        internal long LineFromCharPosition(
            long pos /* in */
            )
        {
            Debug.Assert(pos >= 0);

            //
            // Iterative binary search.
            //
            // http://en.wikipedia.org/wiki/Binary_search_algorithm
            // System.Collections.Generic.ArraySortHelper.InternalBinarySearch
            //
            long low = 0L;
            long high = Count - 1;

            while (low <= high)
            {
                long mid = low + ((high - low) / 2);
                long start = CharPositionFromLine(mid);

                if (pos == start)
                    return mid;
                else if (start < pos)
                    low = mid + 1;
                else
                    high = mid - 1;
            }

            //
            // After the while exits, 'low' will point to the index where
            // 'pos' should be inserted (if we were creating a new line
            // start).  The line containing 'pos' then would be 'low - 1'.
            //
            return low - 1;
        }

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Rebuilds the entire line data mirror from the native
        /// <see cref="Scintilla" /> document.
        /// </summary>
        internal void RebuildLineData()
        {
            stepLine = 0;
            stepLength = 0;

            perLineData = new GapBuffer<PerLine>();
            perLineData.Add(new PerLine(0));
            perLineData.Add(new PerLine(0)); // Terminal

            //
            // Fake an insert notification.
            //
            NativeMethods.SCNotification scn =
                new NativeMethods.SCNotification();

            long adjustedLines = scintilla.DirectMessage(
                NativeMethods.SCI_GETLINECOUNT).ToInt64() - 1;

            scn.linesAdded = new IntPtr(adjustedLines);
            scn.position = IntPtr.Zero;
            scn.length = scintilla.DirectMessage(NativeMethods.SCI_GETLENGTH);

            scn.text = scintilla.DirectMessage(
                NativeMethods.SCI_GETRANGEPOINTER, scn.position, scn.length);

            TrackInsertText(scn);
        }
        #endregion

        ///////////////////////////////////////////////////////////////////////

        #region Private Methods
        /// <summary>
        /// Adjusts the number of CHARACTERS in a line.
        /// </summary>
        /// <param name="index">
        /// The zero-based index of the line to adjust.
        /// </param>
        /// <param name="delta">
        /// The signed change in the number of CHARACTERS in the line.
        /// </param>
        private void AdjustLineLength(
            long index, /* in */
            long delta  /* in */
            )
        {
            MoveStep(index);
            stepLength += delta;

            //
            // Invalidate the multibyte flag.
            //
            PerLine perLine = perLineData[(int)index];

            perLine.ContainsMultibyte = ContainsMultibyte.Unkown;
            perLineData[(int)index] = perLine;
        }

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Stops tracking the line at the specified index.
        /// </summary>
        /// <param name="index">
        /// The zero-based index of the line to remove.  Must be at least one
        /// and no greater than the last non-terminal line.
        /// </param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when <paramref name="index" /> is out of range.
        /// </exception>
        private void DeletePerLine(
            long index /* in */
            )
        {
            Debug.Assert(index != 0);

            //
            // Bounds guard (Debug.Assert is stripped in Release): refuse an
            // index that would walk off perLineData -- CharLineLength(index)
            // reads perLineData[index + 1].
            //
            if (index < 1 || index > perLineData.Count - 2)
                throw new ArgumentOutOfRangeException("index");

            MoveStep(index);

            //
            // Subtract the line length.
            //
            stepLength -= CharLineLength(index);

            //
            // Remove the line.
            //
            perLineData.RemoveAt((int)index);

            //
            // Move the step to the line before the one removed.
            //
            stepLine--;
        }

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Gets the number of CHARACTERS in a BYTE range.
        /// </summary>
        /// <param name="pos">
        /// The starting BYTE offset of the range.
        /// </param>
        /// <param name="length">
        /// The length of the range, in BYTES.
        /// </param>
        /// <returns>
        /// The number of CHARACTERS in the specified BYTE range.
        /// </returns>
        private int GetCharCount(
            long pos,   /* in */
            long length /* in */
            )
        {
            IntPtr ptr = scintilla.DirectMessage(
                NativeMethods.SCI_GETRANGEPOINTER, new IntPtr(pos),
                new IntPtr(length));

            return GetCharCount(ptr, (int)length, scintilla.Encoding);
        }

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Gets the number of CHARACTERS in a BYTE range.
        /// </summary>
        /// <param name="text">
        /// A pointer to the start of the BYTE range.
        /// </param>
        /// <param name="length">
        /// The length of the range, in BYTES.
        /// </param>
        /// <param name="encoding">
        /// The <see cref="Encoding" /> used to decode the BYTES.
        /// </param>
        /// <returns>
        /// The number of CHARACTERS in the specified BYTE range.
        /// </returns>
        private static unsafe int GetCharCount(
            IntPtr text,      /* in */
            int length,       /* in */
            Encoding encoding /* in */
            )
        {
            if (text == IntPtr.Zero || length == 0)
                return 0;

            //
            // NOTE: Never use SCI_COUNTCHARACTERS.  It counts CRLF as 1 char!
            //
            int count = encoding.GetCharCount((byte*)text, length);

            return count;
        }

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Determines whether a line contains any multibyte (Unicode)
        /// characters, caching the result on the line.
        /// </summary>
        /// <param name="index">
        /// The zero-based index of the line to test.
        /// </param>
        /// <returns>
        /// Non-zero (true) if the line at <paramref name="index" /> contains
        /// at least one multibyte character; otherwise, false.
        /// </returns>
        private bool LineContainsMultibyteChar(
            long index /* in */
            )
        {
            PerLine perLine = perLineData[(int)index];

            if (perLine.ContainsMultibyte == ContainsMultibyte.Unkown)
            {
                perLine.ContainsMultibyte =
                    (scintilla.DirectMessage(NativeMethods.SCI_LINELENGTH,
                        new IntPtr(index)).ToInt64() == CharLineLength(index))
                        ? ContainsMultibyte.No
                        : ContainsMultibyte.Yes;

                perLineData[(int)index] = perLine;
            }

            return (perLine.ContainsMultibyte == ContainsMultibyte.Yes);
        }

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Tracks a new line with the given CHARACTER length.
        /// </summary>
        /// <param name="index">
        /// The zero-based index at which to insert the new line.
        /// </param>
        /// <param name="length">
        /// The length of the new line, in CHARACTERS.
        /// </param>
        private void InsertPerLine(
            long index, /* in */
            long length /* in */
            )
        {
            MoveStep(index);

            PerLine data;
            long lineStart = 0;

            //
            // Add the new line length to the existing line start.
            //
            data = perLineData[(int)index];
            lineStart = data.Start;
            data.Start += length;
            perLineData[(int)index] = data;

            //
            // Insert the new line.
            //
            data = new PerLine(lineStart);
            perLineData.Insert((int)index, data);

            //
            // Move the step.
            //
            stepLength += length;
            stepLine++;
        }

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Moves the step boundary to the specified line, applying the
        /// pending length adjustment to the lines it passes over.
        /// </summary>
        /// <param name="line">
        /// The zero-based line index to which the step should be moved.
        /// </param>
        private void MoveStep(
            long line /* in */
            )
        {
            if (stepLength == 0)
            {
                stepLine = line;
            }
            else if (stepLine < line)
            {
                PerLine data;

                while (stepLine < line)
                {
                    stepLine++;
                    data = perLineData[(int)stepLine];
                    data.Start += stepLength;
                    perLineData[(int)stepLine] = data;
                }
            }
            else if (stepLine > line)
            {
                PerLine data;

                while (stepLine > line)
                {
                    data = perLineData[(int)stepLine];
                    data.Start -= stepLength;
                    perLineData[(int)stepLine] = data;
                    stepLine--;
                }
            }
        }

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Handles the <see cref="Scintilla.SCNotification" /> event,
        /// updating the line data mirror in response to document
        /// modifications.
        /// </summary>
        /// <param name="sender">
        /// The source of the event.
        /// </param>
        /// <param name="e">
        /// A <see cref="SCNotificationEventArgs" /> that contains the event
        /// data.
        /// </param>
        private void scintilla_SCNotification(
            object sender,            /* in */
            SCNotificationEventArgs e /* in */
            )
        {
            NativeMethods.SCNotification scn = e.SCNotification;

            switch (scn.nmhdr.code)
            {
                case NativeMethods.SCN_MODIFIED:
                    ScnModified(scn);
                    break;
            }
        }

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Applies a document modification notification to the line data
        /// mirror, rebuilding the entire mirror if tracking faults.
        /// </summary>
        /// <param name="scn">
        /// The <see cref="NativeMethods.SCNotification" /> describing the
        /// modification.
        /// </param>
        private void ScnModified(
            NativeMethods.SCNotification scn /* in */
            )
        {
            try
            {
                if ((scn.modificationType &
                        NativeMethods.SC_MOD_DELETETEXT) > 0)
                {
                    TrackDeleteText(scn);
                }

                if ((scn.modificationType &
                        NativeMethods.SC_MOD_INSERTTEXT) > 0)
                {
                    TrackInsertText(scn);
                }
            }
            catch (Exception)
            {
                //
                // Never let a tracking fault leave the managed line mirror
                // permanently out of sync with native -- that would make
                // every subsequent edit re-fault (a persistent DoS on
                // malformed or pathological content).  Resync the entire
                // mirror from native instead.
                //
                RebuildLineData();
            }
        }

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Updates the line data mirror to reflect deleted text.
        /// </summary>
        /// <param name="scn">
        /// The <see cref="NativeMethods.SCNotification" /> describing the
        /// deletion.
        /// </param>
        private void TrackDeleteText(
            NativeMethods.SCNotification scn /* in */
            )
        {
            long startLine = scintilla.DirectMessage(
                NativeMethods.SCI_LINEFROMPOSITION, scn.position).ToInt64();

            if (scn.linesAdded == IntPtr.Zero)
            {
                //
                // That was easy.
                //
                int delta = GetCharCount(
                    scn.text, scn.length.ToInt32(), scintilla.Encoding);

                AdjustLineLength(startLine, delta * -1);
            }
            else
            {
                //
                // Adjust the existing line.
                //
                long lineByteStart = scintilla.DirectMessage(
                    NativeMethods.SCI_POSITIONFROMLINE,
                    new IntPtr(startLine)).ToInt64();

                long lineByteLength = scintilla.DirectMessage(
                    NativeMethods.SCI_LINELENGTH,
                    new IntPtr(startLine)).ToInt64();

                AdjustLineLength(startLine, GetCharCount(
                    lineByteStart, lineByteLength) -
                    CharLineLength(startLine));

                //
                // Bound the removal to the lines the mirror actually has
                // after startLine, so a line delta larger than the tracked
                // count cannot walk DeletePerLine off the end of
                // perLineData.
                //
                long linesRemoved = Math.Min(
                    scn.linesAdded.ToInt64() * -1, (Count - 1) - startLine);

                for (long i = 0; i < linesRemoved; i++)
                {
                    //
                    // Deleted line.
                    //
                    DeletePerLine(startLine + 1);
                }
            }
        }

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Updates the line data mirror to reflect inserted text.
        /// </summary>
        /// <param name="scn">
        /// The <see cref="NativeMethods.SCNotification" /> describing the
        /// insertion.
        /// </param>
        private void TrackInsertText(
            NativeMethods.SCNotification scn /* in */
            )
        {
            long startLine = scintilla.DirectMessage(
                NativeMethods.SCI_LINEFROMPOSITION, scn.position).ToInt64();

            if (scn.linesAdded == IntPtr.Zero)
            {
                //
                // That was easy.
                //
                int delta = GetCharCount(
                    scn.position.ToInt64(), scn.length.ToInt64());

                AdjustLineLength(startLine, delta);
            }
            else
            {
                long lineByteStart = 0;
                long lineByteLength = 0;

                //
                // Adjust existing line.
                //
                lineByteStart = scintilla.DirectMessage(
                    NativeMethods.SCI_POSITIONFROMLINE,
                    new IntPtr(startLine)).ToInt64();

                lineByteLength = scintilla.DirectMessage(
                    NativeMethods.SCI_LINELENGTH,
                    new IntPtr(startLine)).ToInt64();

                AdjustLineLength(startLine, GetCharCount(
                    lineByteStart, lineByteLength) -
                    CharLineLength(startLine));

                long linesAdded = scn.linesAdded.ToInt64();

                for (long i = 1; i <= linesAdded; i++)
                {
                    long line = startLine + i;

                    //
                    // Insert new line.
                    //
                    lineByteStart += lineByteLength;

                    lineByteLength = scintilla.DirectMessage(
                        NativeMethods.SCI_LINELENGTH,
                        new IntPtr(line)).ToInt64();

                    InsertPerLine(
                        line, GetCharCount(lineByteStart, lineByteLength));
                }
            }
        }
        #endregion

        ///////////////////////////////////////////////////////////////////////

        #region IEnumerable<Line> Members
        /// <summary>
        /// Provides an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// An <see cref="IEnumerator{T}" /> that iterates through all the
        /// <see cref="Line" /> objects within the
        /// <see cref="LineCollection" />.
        /// </returns>
        public IEnumerator<Line> GetEnumerator()
        {
            long count = Count;

            for (long i = 0; i < count; i++)
                yield return this[i];

            yield break;
        }

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Provides an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// An <see cref="IEnumerator" /> that iterates through all the
        /// <see cref="Line" /> objects within the
        /// <see cref="LineCollection" />.
        /// </returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
        #endregion

        ///////////////////////////////////////////////////////////////////////

        #region Nested Types
        /// <summary>
        /// The per-line data tracked for each line.
        /// </summary>
        [ObjectId("737303ae-657d-469f-95c6-0b9b1e9e4e0c")]
        private struct PerLine
        {
            #region Public Data
            /// <summary>
            /// The CHARACTER position where the line begins.
            /// </summary>
            public long Start;

            /// <summary>
            /// Indicates whether the line contains multibyte (Unicode)
            /// characters: <see cref="ContainsMultibyte.Yes" /> if it does,
            /// <see cref="ContainsMultibyte.No" /> if it does not, or
            /// <see cref="ContainsMultibyte.Unkown" /> if not yet determined.
            /// </summary>
            /// <remarks>
            /// Using an enumeration instead of <see cref="Nullable{T}" />
            /// because it uses less memory per line.
            /// </remarks>
            public ContainsMultibyte ContainsMultibyte;
            #endregion

            ///////////////////////////////////////////////////////////////////

            #region Public Constructors
            /// <summary>
            /// Initializes a new instance of the <see cref="PerLine" />
            /// structure.
            /// </summary>
            /// <param name="start">
            /// The CHARACTER position where the line begins.
            /// </param>
            public PerLine(
                long start /* in */
                )
            {
                this.Start = start;
                this.ContainsMultibyte = ContainsMultibyte.Unkown;
            }
            #endregion
        }

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Indicates whether a line contains multibyte (Unicode) characters.
        /// </summary>
        [ObjectId("118ec65d-1427-4fd9-b74d-6232a2f8860d")]
        private enum ContainsMultibyte
        {
            /// <summary>
            /// The line contains no multibyte characters.
            /// </summary>
            No = -1,

            /// <summary>
            /// It is not yet known whether the line contains multibyte
            /// characters.
            /// </summary>
            Unkown,

            /// <summary>
            /// The line contains at least one multibyte character.
            /// </summary>
            Yes
        }
        #endregion
    }
}

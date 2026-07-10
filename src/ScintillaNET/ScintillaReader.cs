/*
 * ScintillaReader.cs --
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
using System.IO;

namespace ScintillaNET
{
    /// <summary>
    /// Implements a <see cref="System.IO.TextReader" /> that reads its text
    /// from a <see cref="Scintilla" /> control.
    /// </summary>
    [ObjectId("b1ef9720-c85b-4209-b0c9-d68649e8567c")]
    public class ScintillaReader : TextReader
    {
        #region Private Constants
        /// <summary>
        /// The arbitrarily chosen default number of characters to buffer at a
        /// time.
        /// </summary>
        private const int DefaultBufferSize = 256;
        #endregion

        ///////////////////////////////////////////////////////////////////////

        #region Private Data
        /// <summary>
        /// The Scintilla control this reader reads its text from.
        /// </summary>
        private Scintilla _scintilla;

        /// <summary>
        /// The number of characters to buffer at a time.
        /// </summary>
        private int _bufferSize;

        /// <summary>
        /// The most recently buffered block of text, or null once the end of
        /// the input has been reached.
        /// </summary>
        private string _data;

        /// <summary>
        /// The index of the next character to be read from the current
        /// buffer.
        /// </summary>
        private int _dataIndex;

        /// <summary>
        /// The index, within the Scintilla control, of the next character to
        /// buffer.
        /// </summary>
        private long _nextData;

        /// <summary>
        /// The index, within the Scintilla control, just past the last
        /// character to be read.
        /// </summary>
        private long _lastData;
        #endregion

        ///////////////////////////////////////////////////////////////////////

        #region Private Properties
        /// <summary>
        /// Gets the number of buffered characters left to be read.
        /// </summary>
        private int BufferRemaining
        {
            get { return _data != null ? _data.Length - _dataIndex : 0; }
        }

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Gets the number of unbuffered characters left to be read.
        /// </summary>
        private long UnbufferedRemaining
        {
            get { return _lastData - _nextData; }
        }

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Gets the total number of characters left to be read.
        /// </summary>
        private long TotalRemaining
        {
            get { return BufferRemaining + UnbufferedRemaining; }
        }
        #endregion

        ///////////////////////////////////////////////////////////////////////

        #region Public Constructors
        /// <summary>
        /// Constructs an instance of this class that reads all text from the
        /// specified Scintilla control.
        /// </summary>
        /// <param name="scintilla">
        /// The Scintilla control from which to read.
        /// </param>
        public ScintillaReader(
            Scintilla scintilla /* in */
            )
            : this(scintilla, 0, scintilla.TextLength)
        {
            // do nothing.
        }

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Constructs an instance of this class that reads all text from the
        /// specified Scintilla control.
        /// </summary>
        /// <param name="scintilla">
        /// The Scintilla control from which to read.
        /// </param>
        /// <param name="bufferSize">
        /// The number of characters to buffer at a time.
        /// </param>
        public ScintillaReader(
            Scintilla scintilla, /* in */
            int bufferSize       /* in */
            )
            : this(scintilla, 0, scintilla.TextLength, bufferSize)
        {
            // do nothing.
        }

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Constructs an instance of this class that reads a subsection from
        /// the specified Scintilla control.
        /// </summary>
        /// <param name="scintilla">
        /// The Scintilla control from which to read.
        /// </param>
        /// <param name="start">
        /// The index of the first character to read.
        /// </param>
        /// <param name="end">
        /// The index just past the last character to read.
        /// </param>
        public ScintillaReader(
            Scintilla scintilla, /* in */
            long start,          /* in */
            long end             /* in */
            )
            : this(scintilla, start, end, DefaultBufferSize)
        {
            // do nothing.
        }

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Constructs an instance of this class that reads a subsection from
        /// the specified Scintilla control.
        /// </summary>
        /// <param name="scintilla">
        /// The Scintilla control from which to read.
        /// </param>
        /// <param name="start">
        /// The index of the first character to read.
        /// </param>
        /// <param name="end">
        /// The index just past the last character to read.
        /// </param>
        /// <param name="bufferSize">
        /// The number of characters to buffer at a time.
        /// </param>
        public ScintillaReader(
            Scintilla scintilla, /* in */
            long start,          /* in */
            long end,            /* in */
            int bufferSize       /* in */
            )
        {
            _scintilla = scintilla;
            _bufferSize = bufferSize > 0 ? bufferSize : DefaultBufferSize;
            _nextData = start;
            _lastData = end;

            // ensure start state is valid
            BufferNextRegion();
        }
        #endregion

        ///////////////////////////////////////////////////////////////////////

        #region Public Methods
        /// <summary>
        /// Returns the next character to be read from the reader without
        /// actually removing it from the stream.  Returns -1 if no characters
        /// are available.
        /// </summary>
        /// <returns>
        /// The next character from the input stream, or -1 if no more
        /// characters are available.
        /// </returns>
        public override int Peek()
        {
            // _data is set to null upon EOF
            return _data != null ? _data[_dataIndex] : -1;
        }

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Removes a character from the stream and returns it.  Returns -1 if
        /// no characters are available.
        /// </summary>
        /// <returns>
        /// The next character from the input stream, or -1 if no more
        /// characters are available.
        /// </returns>
        public override int Read()
        {
            if (_data != null)
            {
                // EOF not reached
                char n = _data[_dataIndex++];
                if (_dataIndex >= _data.Length)
                {
                    // end of buffer reached; load next section
                    BufferNextRegion();
                }
                return n;
            }
            else
            {
                return -1;
            }
        }

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Reads a maximum of count characters from the current stream and
        /// writes the data to buffer, beginning at index.
        /// </summary>
        /// <param name="buffer">
        /// The buffer to receive the characters.
        /// </param>
        /// <param name="index">
        /// The position in buffer at which to begin writing.
        /// </param>
        /// <param name="count">
        /// The maximum number of characters to read.
        /// </param>
        /// <returns>
        /// The actual number of characters that have been read.  The number
        /// will be less than or equal to count.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">
        /// buffer is null.
        /// </exception>
        /// <exception cref="System.ArgumentException">
        /// The buffer length minus index is less than count.
        /// </exception>
        /// <exception cref="System.ArgumentException">
        /// index or count is negative.
        /// </exception>
        public override int Read(
            char[] buffer, /* in */
            int index,     /* in */
            int count      /* in */
            )
        {
            return ReadBlock(buffer, index, count);
        }

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Reads a maximum of count characters from the current stream and
        /// writes the data to buffer, beginning at index.
        /// </summary>
        /// <param name="buffer">
        /// The buffer to receive the characters.
        /// </param>
        /// <param name="index">
        /// The position in buffer at which to begin writing.
        /// </param>
        /// <param name="count">
        /// The maximum number of characters to read.
        /// </param>
        /// <returns>
        /// The actual number of characters that have been read.  The number
        /// will be less than or equal to count.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">
        /// buffer is null.
        /// </exception>
        /// <exception cref="System.ArgumentOutOfRangeException">
        /// The buffer length minus index is less than count.
        /// </exception>
        /// <exception cref="System.ArgumentOutOfRangeException">
        /// index or count is negative.
        /// </exception>
        public override int ReadBlock(
            char[] buffer, /* in */
            int index,     /* in */
            int count      /* in */
            )
        {
            if (_data != null)
            {
                int bufferRemaining = BufferRemaining;
                if (count < bufferRemaining)
                {
                    // buffer larger than read size
                    _data.CopyTo(_dataIndex, buffer, index, count);
                    _dataIndex += count;
                    return count;
                }
                else
                {
                    // buffer smaller or equal to read size
                    _data.CopyTo(_dataIndex, buffer, index, bufferRemaining);
                    if (count > bufferRemaining)
                    {
                        // buffer is smaller; read rest
                        int toRead = (int)Math.Min(
                            count - bufferRemaining, UnbufferedRemaining);
                        string rest = _scintilla.GetTextRange(
                            _nextData, toRead);
                        // GetTextRange rounds a boundary that bisects a
                        // surrogate pair up to the whole code point, so it
                        // can return one extra unit.  Never copy or report
                        // more than requested (which would overrun the
                        // caller's buffer) or split a surrogate: drop a
                        // trailing partial astral char here and re-read it
                        // whole next time.
                        int restLen = rest.Length > toRead
                            ? toRead - 1 : rest.Length;
                        rest.CopyTo(
                            0, buffer, index + bufferRemaining, restLen);
                        count = bufferRemaining + restLen;
                        _nextData += restLen;
                    }
                    // read at least up to buffer's end; refill buffer
                    BufferNextRegion();
                    return count;
                }
            }
            else
            {
                return 0;
            }
        }
        #endregion

        ///////////////////////////////////////////////////////////////////////

        #region Private Methods
        /// <summary>
        /// Fills the buffer with the next section of text.
        /// </summary>
        private void BufferNextRegion()
        {
            if (_nextData < _lastData)
            {
                int size = (int)Math.Min(_lastData - _nextData, _bufferSize);
                _data = _scintilla.GetTextRange(_nextData, size);
                _nextData += _data.Length;
                _dataIndex = 0;
            }
            else
            {
                _data = null;
            }
        }
        #endregion
    }
}

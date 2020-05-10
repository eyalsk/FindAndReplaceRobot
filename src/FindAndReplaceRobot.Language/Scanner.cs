namespace FindAndReplaceRobot.Language
{
    using System;
    using System.Diagnostics;

    using static InvisibleCharacters;

    public sealed class Scanner
    {
        private readonly ReadOnlyMemory<char> _text;
        private int _offset;
        private int _prevIndex = -1;

        public Scanner(ReadOnlyMemory<char> text)
        {
            _text = text;

            TextLength = text.Length;

            Position = new Position(1, 0);
        }

        [Flags]
        public enum TextEndingFlags
        {
            None,
            CR = 1 << 0,
            LF = 1 << 1,
            CRLF = CR | LF
        }

        private enum ReadMode
        {
            Normal,
            Lookahead,
            Peeking
        }

        public Scanner(string text) : this(text.AsMemory()) { }

        public int TextLength { get; }

        public int CurrentIndex { get; private set; }

        public int AbsoluteIndex => CurrentIndex + _offset;

        public Position Position { get; private set; }

        public ReadOnlyMemory<char> GetSlice(Range range) => _text[range];

        public TextEndingFlags GetSliceEnding() => GetSliceEnding(CurrentIndex..AbsoluteIndex);

        public TextEndingFlags GetSliceEnding(Range range)
        {
            var span = _text[range].Span;

            return span[^1] switch
            {
                '\n' => span.Length >= 2 && span[^2] == '\r' ? TextEndingFlags.CRLF : TextEndingFlags.LF,
                '\r' => TextEndingFlags.CR,
                _ => TextEndingFlags.None,
            };
        }

        public void MoveAhead()
        {
            CurrentIndex = AbsoluteIndex < TextLength ? AbsoluteIndex : TextLength;

            _offset = 0;
        }

        public bool MoveNext() => CurrentIndex < TextLength && ++CurrentIndex < TextLength;

        public char ReadAhead()
        {
            if (_offset >= 0) _offset++;

            return GetChar(AbsoluteIndex, ref _offset, ReadMode.Lookahead);
        }

        public char PeekAhead(ref int offset)
        {
            if (offset <= 0) throw new ArgumentOutOfRangeException(nameof(offset));

            return GetChar(CurrentIndex + offset, ref offset, ReadMode.Peeking);
        }

        public void StepTo(int offset)
        {
            if (offset < 0) throw new ArgumentOutOfRangeException(nameof(offset));

            _offset = offset;

            MoveAhead();
        }

        public char ReadChar()
        {
            var offset = 0;

            return GetChar(CurrentIndex, ref offset, ReadMode.Normal);
        }

        private char GetChar(int index, ref int offset, ReadMode mode)
        {
            var ch = index < TextLength ? _text.Span[index] : EndOfFile;

            if (TrySkipCarriageReturn(index, ref ch))
            {
                index++;

                if (mode == ReadMode.Normal)
                {
                    CurrentIndex++;
                }
                else if (mode == ReadMode.Lookahead || mode == ReadMode.Peeking)
                {
                    offset++;
                }
            }

            if (index > _prevIndex)
            {
                Position = ch == NewLine ? Position.NextLine() : Position.NextColumn();

                _prevIndex = index;
            }

            Debug.WriteLine($"{ch.ToReadableString()}\t\t[{index}+{offset}]\t\t[{Position.LineNumber}:{Position.ColumnNumber}]\t\t{mode}");

            return ch;

            bool TrySkipCarriageReturn(int index, ref char ch)
            {
                if (ch == Return)
                {
                    var nextChar = ++index < TextLength ? _text.Span[index] : ch;

                    if (nextChar == NewLine)
                    {
                        ch = nextChar;

                        return true;
                    }
                }

                return false;
            }
        }
    }
}
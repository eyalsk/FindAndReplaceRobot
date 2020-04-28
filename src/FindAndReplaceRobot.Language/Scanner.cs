namespace FindAndReplaceRobot.Language
{
    using System;

    using static InvisibleCharacters;

    public sealed class Scanner
    {
        private readonly ReadOnlyMemory<char> _text;
        private int _offset;
        private int _prevIndex = -1;

        private enum ReadMode
        {
            Normal,
            Lookahead,
            Peeking
        }

        public Scanner(ReadOnlyMemory<char> text)
        {
            _text = text;

            TextLength = text.Length;

            Position = new Position(0);
        }

        public Scanner(string text) : this(text.AsMemory()) { }

        public int TextLength { get; }

        public int CurrentIndex { get; private set; }

        public int AbsoluteIndex => CurrentIndex + _offset;

        public Position Position { get; private set; }

        public ReadOnlyMemory<char> GetSlice(Range range) => _text[range];

        public void MoveAhead()
        {
            CurrentIndex = AbsoluteIndex < TextLength ? AbsoluteIndex : TextLength;

            _offset = 0;
        }

        public bool MoveNext() => CurrentIndex < TextLength && ++CurrentIndex < TextLength;

        public char ReadAhead(int offset = 1)
        {
            if (offset < 0) throw new ArgumentOutOfRangeException(nameof(offset));

            _offset = offset;

            return GetChar(AbsoluteIndex, ReadMode.Lookahead);
        }

        public char PeekAhead(int offset = 1)
        {
            if (offset < 0) throw new ArgumentOutOfRangeException(nameof(offset));

            return GetChar(CurrentIndex + offset, ReadMode.Peeking);
        }

        public char ReadChar() => GetChar(CurrentIndex, ReadMode.Normal);

        private char GetChar(int index, ReadMode mode)
        {
            var ch = index < TextLength ? _text.Span[index] : EndOfFile;

            if (TrySkipCarriageReturn(index, ref ch))
            {
                index++;

                if (mode == ReadMode.Normal)
                {
                    CurrentIndex++;
                }
                else if (mode == ReadMode.Lookahead)
                {
                    _offset++;
                }
            }

            if (index > _prevIndex)
            {
                Position = ch == NewLine ? Position.NextLine(index) : Position.NextColumn(index);

                _prevIndex = index;
            }

            return ch;
        }

        private bool TrySkipCarriageReturn(int index, ref char ch)
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
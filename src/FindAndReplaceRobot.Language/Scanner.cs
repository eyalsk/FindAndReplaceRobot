namespace FindAndReplaceRobot.Language
{
    using System;

    using static InvisibleCharacters;

    public sealed class Scanner
    {
        private ReadOnlyMemory<char> _text;
        private readonly int _length;
        private int _offset;

        public Scanner(ReadOnlyMemory<char> text)
        {
            _text = text;
            _length = text.Length;
        }

        public Scanner(string text) : this(text.AsMemory()) { }

        public int CurrentPosition { get; private set; }

        public int AbsolutePosition => CurrentPosition + _offset;

        public ReadOnlyMemory<char> GetSlice(Range range) => _text[range];

        public void MoveAhead()
        {
            CurrentPosition += _offset;
            _offset = 0;
        }

        public bool MoveNext() => CurrentPosition < _length && ++CurrentPosition < _length;

        public char ReadAhead(int offset = 1)
        {
            if (offset < 0) throw new ArgumentOutOfRangeException(nameof(offset));

            _offset = offset;

            return AbsolutePosition < _length ? _text.Span[AbsolutePosition] : EndOfFile;
        }

        public char ReadChar() => CurrentPosition < _length ? _text.Span[CurrentPosition] : EndOfFile;
    }
}
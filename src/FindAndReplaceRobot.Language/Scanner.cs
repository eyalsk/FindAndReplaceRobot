namespace FindAndReplaceRobot.Language
{
    using System;

    using static InvisibleCharacters;

    public sealed class Scanner
    {
        private ReadOnlyMemory<char> _text;
        private readonly int _length;
        private int _baseIndex;
        private int _offset = 1;

        public Scanner(ReadOnlyMemory<char> text)
        {
            _text = text;
            _length = text.Length;
        }

        public Scanner(string text) : this(text.AsMemory())
        {
        }

        public bool Next() => _baseIndex < _length && ++_baseIndex < _length;

        public void MoveAhead()
        {
            _baseIndex += _offset;
            _offset = 1;
        }

        public char ReadChar() => _baseIndex < _length ? _text.Span[_baseIndex] : EndOfFile;

        public char ReadAhead(int offset = 1)
        {
            if (offset < 0) throw new ArgumentOutOfRangeException(nameof(offset));

            _offset = offset;

            var position = _baseIndex + offset;

            return position < _length ? _text.Span[position] : EndOfFile;
        }
    }
}
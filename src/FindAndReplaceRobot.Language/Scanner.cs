namespace FindAndReplaceRobot.Language
{
    using System;

    using static InvisibleCharacters;

    public sealed class Scanner
    {
        private ReadOnlyMemory<char> _text;
        private readonly int _length;
        private int _baseIndex = -1;

        public Scanner(ReadOnlyMemory<char> text)
        {
            _text = text;

            _length = text.Length;
        }

        public Scanner(string text) : this(text.AsMemory())
        {
        }

        public bool Next() => ++_baseIndex < _length;

        public char ReadChar() => _baseIndex < _length ? _text.Span[_baseIndex] : EndOfFile;
    }
}
namespace FindAndReplaceRobot.Language
{
    using System;

    using static InvisibleCharacters;

    public sealed class Scanner
    {
        private readonly ReadOnlyMemory<char> _text;

        public Scanner(ReadOnlyMemory<char> text)
        {
            _text = text;

            TextLength = text.Length;
        }

        public Scanner(string text) : this(text.AsMemory()) { }

        public int TextLength { get; }

        public int CurrentIndex { get; private set; } = -1;

        public ReadOnlyMemory<char> GetSlice(Range range) => _text[range];

        public char Peek(int offset = 0)
        {
            if (offset < 0) throw new ArgumentOutOfRangeException(nameof(offset));

            var index = (CurrentIndex < 0 ? 0 : CurrentIndex) + offset;

            return index < TextLength ? _text.Span[index] : EndOfFile;
        }

        public char Read()
        {
            if (CurrentIndex < TextLength) CurrentIndex++;

            return CurrentIndex < TextLength ? _text.Span[CurrentIndex] : EndOfFile;
        }
    }
}
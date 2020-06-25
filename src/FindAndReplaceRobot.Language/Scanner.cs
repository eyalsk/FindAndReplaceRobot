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

        public int CurrentIndex { get; private set; }

        public ReadOnlyMemory<char> GetSlice(Range range) => _text[range];

        public char Peek()
        {
            return CurrentIndex < TextLength ? _text.Span[CurrentIndex] : EndOfFile;
        }

        public void Consume()
        {
            if (CurrentIndex < TextLength) CurrentIndex++;
        }
    }
}
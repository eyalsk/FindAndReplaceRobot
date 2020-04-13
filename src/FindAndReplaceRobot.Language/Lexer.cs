namespace FindAndReplaceRobot.Language
{
    using System;
    using FindAndReplaceRobot.Language.Tokens;

    using static InvisibleCharacters;

    public sealed class Lexer
    {
        private readonly Scanner _scanner;

        public Lexer(Scanner scanner) => _scanner = scanner ?? throw new ArgumentNullException(nameof(scanner));

        public Token? ReadToken()
        {
            while (true)
            {
                switch (_scanner.ReadChar())
                {
                    case '@':
                        return LexAnnotation();
                    case '[':
                        return LexSection();
                    case EndOfFile:
                        return null;
                }

                _scanner.MoveNext();
            }
        }

        private Token LexAnnotation()
        {
            for (int index = 1; ; index++)
            {
                var ch = _scanner.ReadAhead(index);

                if (ch != '@' && !char.IsLetterOrDigit(ch))
                {
                    var token = new Token(_scanner.CurrentPosition, TokenKind.Annotation, ReadSlice());

                    _scanner.MoveAhead();

                    return token;
                }
            }
        }

        private Token LexSection()
        {
            for (int index = 1; ; index++)
            {
                var ch = _scanner.ReadAhead(index);

                if (ch == ']')
                {
                    var token = new Token(_scanner.CurrentPosition, TokenKind.Section, ReadSlice());

                    _scanner.MoveAhead();

                    return token;
                }
            }
        }

        private ReadOnlyMemory<char> ReadSlice() => _scanner.ReadSlice(_scanner.CurrentPosition + 1, _scanner.AbsolutePosition);
    }
}

namespace FindAndReplaceRobot.Language
{
    using System;
    using FindAndReplaceRobot.Language.Tokens;

    using static InvisibleCharacters;

    public sealed class Lexer
    {
        private readonly Scanner _scanner;
        private bool _canAnnotate = true;

        public Lexer(Scanner scanner) => _scanner = scanner ?? throw new ArgumentNullException(nameof(scanner));

        public Token? ReadToken()
        {
            while (true)
            {
                switch (_scanner.ReadChar())
                {
                    case '@' when _canAnnotate:
                        return LexAnnotation();
                    case '[':
                        _canAnnotate = false;
                        return LexSection();
                    case NewLine when _scanner.ReadAhead() == NewLine:
                        _canAnnotate = true;
                        break;
                    case EndOfFile:
                        return null;
                }

                _scanner.MoveNext();
            }
        }

        private Token LexAnnotation()
        {
            TokenBuilder builder = new TokenBuilder();
            builder.SetPosition(_scanner.CurrentPosition);

            for (int index = 1; ; index++)
            {
                var ch = _scanner.ReadAhead(index);

                if (ch == '(' || ch == NewLine || ch == EndOfFile)
                {
                    builder
                        .SetKind(TokenKind.Annotation)
                        .SetValue(GetSlice());

                    _scanner.MoveAhead();

                    if (ch == '(')
                    {
                        //builder.AddToken(LexAnnotationArgumentList());
                    }

                    break;
                }
                else if (!char.IsLetter(ch))
                {
                    // todo: Error

                    _scanner.MoveAhead();

                    break;
                }
            }

            return builder.Build();
        }

        private Token LexSection()
        {
            for (int index = 1; ; index++)
            {
                var ch = _scanner.ReadAhead(index);

                if (ch == ']')
                {
                    var token = new Token(_scanner.CurrentPosition, TokenKind.Section, GetSlice());

                    _scanner.MoveAhead();

                    return token;
                }
            }
        }

        private ReadOnlyMemory<char> GetSlice() => _scanner.GetSlice((_scanner.CurrentPosition + 1).._scanner.AbsolutePosition);
    }
}

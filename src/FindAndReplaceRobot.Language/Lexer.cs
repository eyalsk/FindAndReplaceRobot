namespace FindAndReplaceRobot.Language
{
    using FindAndReplaceRobot.Language.Tokens;

    using static InvisibleCharacters;

    public sealed class Lexer
    {
        private readonly Scanner _scanner;
        private readonly TokenBuilder _builder;
        
        public Lexer(Scanner scanner)
        {
            _scanner = scanner;
            _builder = new TokenBuilder();
        }

        public Token? ReadToken()
        {
            while(true)
            {
                switch (_scanner.ReadChar())
                {
                    case '@':
                        LexAnnotation();
                        break;
                    case '[':
                        LexSection();
                        break;
                    case EndOfFile:
                        return null;
                }

                if (_builder.HasValidToken)
                {
                    return _builder.Build();
                }

                _scanner.Next();
            }
        }

        private void LexAnnotation()
        {
            _builder.StartAt(_scanner.Position);

            for (int index = 1; ; index++)
            {
                var ch = _scanner.ReadAhead(index);

                if (ch != '@' && !char.IsLetterOrDigit(ch))
                {
                    _builder
                        .EndAt(_scanner.Position)
                        .SetKind(TokenKind.Annotation);
                    break;
                }

                _builder.AppendChar(ch);
            }

            _scanner.MoveAhead();
        }

        private void LexSection()
        {
            _builder.StartAt(_scanner.Position);

            for (int index = 1; ; index++)
            {
                var ch = _scanner.ReadAhead(index);

                if (ch == ']')
                {
                    _builder
                        .EndAt(_scanner.Position)
                        .SetKind(TokenKind.Section);
                    break;
                }

                _builder.AppendChar(ch);
            }

            _scanner.MoveAhead();
        }
    }
}

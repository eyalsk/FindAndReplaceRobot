namespace FindAndReplaceRobot.Language
{
    using System;

    using static InvisibleCharacters;

    public sealed class Lexer
    {
        private readonly Scanner _scanner;
        private Token _prevToken = new Token(0..0, TokenKind.None, ReadOnlyMemory<char>.Empty);

        public Lexer(Scanner scanner)
        {
            _scanner = scanner ?? throw new ArgumentNullException(nameof(scanner));
        }

        private static bool IsCharNewLineOrEOF(char ch) =>
            ch == NewLine || ch == EndOfFile;

        private static bool IsCharIdentifier(char ch) => char.IsLetterOrDigit(ch);

        private static bool IsCharLabel(char ch) => ch == Space || IsCharIdentifier(ch);

        public Token ReadToken()
        {
            Token? token = null;

            while (true)
            {
                switch (_scanner.ReadChar())
                {
                    case '@':
                        token = CreateToken(TokenKind.AtSign);
                        break;
                    case '(':
                        token = CreateToken(TokenKind.OpenParens);
                        break;
                    case ')':
                        token = CreateToken(TokenKind.CloseParens);
                        break;
                    case ',':
                        token = CreateToken(TokenKind.Comma);
                        break;
                    case '[':
                    case '"':
                    case '/':
                        return LexQuotedLiteral();
                    case Tab:
                    case Space:
                        // NYI
                        break;
                    case NewLine:
                        // NYI
                        break;
                    case EndOfFile:
                        return new Token(
                            _scanner.CurrentIndex.._scanner.CurrentIndex,
                            TokenKind.EndOfFile,
                            ReadOnlyMemory<char>.Empty);
                    default:
                        if (_prevToken.Kind == TokenKind.AtSign)
                        {
                            token = LexIdentifier();
                        }
                        else
                        {
                            // NYI: LexLiteral();
                        }
                        break;
                }

                if (token is object)
                {
                    _prevToken = token;

                    return token;
                }

                _scanner.MoveNext();
            }

            Token CreateToken(TokenKind kind)
            {
                var range = _scanner.CurrentIndex..(_scanner.CurrentIndex + 1);

                _scanner.MoveNext();

                return new Token(
                        range,
                        kind,
                        _scanner.GetSlice(range));
            }
        }

        private Token LexIdentifier()
        {
            var start = _scanner.CurrentIndex;

            while (true)
            {
                var ch = _scanner.ReadChar();

                if (IsCharNewLineOrEOF(ch) || !IsCharIdentifier(ch))
                {
                    var kind = IsCharNewLineOrEOF(ch) ? TokenKind.Identifier : TokenKind.Error;

                    var end = _scanner.GetSliceEnding(start.._scanner.CurrentIndex) == TextEndingFlags.CR
                                ? _scanner.CurrentIndex - 1
                                : _scanner.CurrentIndex;

                    return new Token(
                        start..end,
                        kind,
                        _scanner.GetSlice(start..end));
                }

                _scanner.MoveNext();
            }
        }

        private Token LexQuotedLiteral()
        {
            var start = _scanner.CurrentIndex;
            var kind = TokenKind.None;

            while (true)
            {
                var ch = _scanner.ReadChar();

                if (ch == '[')
                {
                    // note: Do nothing, character is legal.
                }
                else if (ch == ']')
                {
                    kind = TokenKind.Label;
                }
                else if (!IsCharLabel(ch))
                {
                    kind = TokenKind.Error;
                }

                if (kind != TokenKind.None)
                {
                    var end = _scanner.CurrentIndex;
                    var slice = (start: start + 1, end);

                    if (kind == TokenKind.Error)
                    {
                        end = _scanner.GetSliceEnding(start..end) == TextEndingFlags.CR ? --end : end;

                        slice.end = end;
                    }
                    else
                    {
                        end++;
                    }

                    _scanner.MoveNext();

                    return new Token(
                        start..end,
                        kind,
                        _scanner.GetSlice(slice.start..slice.end));
                }

                _scanner.MoveNext();
            }
        }
    }
}

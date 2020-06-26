namespace FindAndReplaceRobot.Language
{
    using System;
    using System.Text;
    using static InvisibleCharacters;

    public sealed class Lexer
    {
        private readonly Scanner _scanner;
        private readonly StringBuilder _currentTokenValue;
        private TokenKind _prevTokenKind;

        public Lexer(Scanner scanner)
        {
            _scanner = scanner ?? throw new ArgumentNullException(nameof(scanner));
            _currentTokenValue = new StringBuilder();
        }

        private static bool IsCharNewLineOrEOF(char ch) =>
            ch == NewLine || ch == EndOfFile;

        private static bool IsCharIdentifier(char ch) =>
            char.IsLetterOrDigit(ch);

        private static bool IsCharLabel(char ch) =>
            ch == Space || IsCharIdentifier(ch);

        public Token ReadToken()
        {
            while (true)
            {
                switch (_scanner.Peek())
                {
                    case '@' when _prevTokenKind != TokenKind.AtSign:
                        return CreateToken(TokenKind.AtSign);
                    case '(' when _prevTokenKind != TokenKind.OpenParens:
                        return CreateToken(TokenKind.OpenParens);
                    case ')' when _prevTokenKind != TokenKind.CloseParens:
                        return CreateToken(TokenKind.CloseParens);
                    case ',' when _prevTokenKind != TokenKind.Comma:
                        return CreateToken(TokenKind.Comma);
                    case '[':
                        return LexQuotedLiteral(TokenKind.Label);
                    case '"':
                        return LexQuotedLiteral(TokenKind.String);
                    case '/':
                        return LexQuotedLiteral(TokenKind.Regex);
                    case Tab:
                    case Space:
                        _scanner.Consume();
                        // NYI
                        break;
                    case NewLine:
                        _scanner.Consume();
                        // NYI
                        break;
                    case EndOfFile:
                        return new Token(
                            _scanner.CurrentIndex.._scanner.CurrentIndex,
                            TokenKind.EndOfFile,
                            ReadOnlyMemory<char>.Empty);
                    default:
                        if (_prevTokenKind == TokenKind.AtSign)
                        {
                            return LexIdentifier();
                        }
                        else
                        {
                            _scanner.Consume();
                            // NYI: return LexLiteral();
                        }
                        break;
                }
            }

            Token CreateToken(TokenKind kind)
            {
                var range = _scanner.CurrentIndex..(_scanner.CurrentIndex + 1);
                _scanner.Consume();

                _prevTokenKind = kind;

                return new Token(
                    range,
                    kind,
                    _scanner.GetSlice(range));
            }
        }

        private Token LexIdentifier()
        {
            var tokenStart = _scanner.CurrentIndex;
            bool isError;

            while (true)
            {
                var ch = _scanner.Peek();

                if (IsCharNewLineOrEOF(ch))
                {
                    isError = false;
                    break;
                }

                _scanner.Consume();

                if (!IsCharIdentifier(ch))
                {
                    isError = true;
                    break;
                }
            }

            var tokenEnd = _scanner.CurrentIndex;

            return new Token(
                tokenStart..tokenEnd,
                isError ? TokenKind.Error : TokenKind.Identifier,
                _scanner.GetSlice(tokenStart..tokenEnd));
        }

        private Token LexQuotedLiteral(TokenKind kind)
        {
            var closingChar = kind switch
            {
                TokenKind.Label => ']',
                TokenKind.String => '"',
                TokenKind.Regex => '/',
                _ => throw new ArgumentException("Invalid token kind for a quoted literal.", nameof(kind))
            };

            _currentTokenValue.Clear();

            var tokenStart = _scanner.CurrentIndex;

            _scanner.Consume();

            var tokenValueStart = _scanner.CurrentIndex;
            var isError = false;

            while (true)
            {
                var ch = _scanner.Peek();
                _scanner.Consume();

                if (ch == closingChar)
                {
                    var isEscaped = _scanner.Peek() == closingChar;
                    if (!isEscaped) break;

                    _currentTokenValue.Append(_scanner.GetSlice(tokenValueStart.._scanner.CurrentIndex));
                    _scanner.Consume();

                    tokenValueStart = _scanner.CurrentIndex;
                }
                else if (ch == EndOfFile)
                {
                    isError = true;
                }
                else if (kind == TokenKind.Label && !IsCharLabel(ch)) // For the sake of the resulting squiggle span, consider checking this after the loop.
                {
                    isError = true;
                }

                if (isError) break;
            }

            var tokenEnd = _scanner.CurrentIndex;

            if (isError)
            {
                return new Token(
                    tokenStart..tokenEnd,
                    TokenKind.Error,
                    _scanner.GetSlice(tokenStart..tokenEnd));
            }

            var tokenValueEnd = tokenEnd - 1;
            var lastTokenValueSlice = _scanner.GetSlice(tokenValueStart..tokenValueEnd);
            var tokenValue = _currentTokenValue.Append(lastTokenValueSlice).ToString().AsMemory();

            return new Token(tokenStart..tokenEnd, kind, tokenValue);
        }
    }
}
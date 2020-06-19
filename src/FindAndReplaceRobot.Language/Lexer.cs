namespace FindAndReplaceRobot.Language
{
    using System;

    using static InvisibleCharacters;

    public sealed class Lexer
    {
        private readonly Scanner _scanner;
        private TokenKind _prevKind;

        public Lexer(Scanner scanner)
        {
            _scanner = scanner ?? throw new ArgumentNullException(nameof(scanner));
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
                switch (_scanner.Read())
                {
                    case '@' when _prevKind != TokenKind.AtSign:
                        return CreateToken(TokenKind.AtSign);
                    case '(' when _prevKind != TokenKind.OpenParens:
                        return CreateToken(TokenKind.OpenParens);
                    case ')' when _prevKind != TokenKind.CloseParens:
                        return CreateToken(TokenKind.CloseParens);
                    case ',' when _prevKind != TokenKind.Comma:
                        return CreateToken(TokenKind.Comma);
                    case '[':
                        return LexQuotedLiteral(TokenKind.Label);
                    case '"':
                        return LexQuotedLiteral(TokenKind.String);
                    case '/':
                        return LexQuotedLiteral(TokenKind.Regex);
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
                        if (_prevKind == TokenKind.AtSign)
                        {
                            return LexIdentifier();
                        }
                        else
                        {
                            // NYI: return LexLiteral();
                        }
                        break;
                }
            }

            Token CreateToken(TokenKind kind)
            {
                var range = _scanner.CurrentIndex..(_scanner.CurrentIndex + 1);

                _prevKind = kind;

                return new Token(
                    range,
                    kind,
                    _scanner.GetSlice(range));
            }
        }

        private Token LexIdentifier()
        {
            var ch = _scanner.Peek();
            var start = _scanner.CurrentIndex;
            bool isError;

            while (true)
            {
                var isNewLineOrEOF = IsCharNewLineOrEOF(ch);

                if (isNewLineOrEOF || !IsCharIdentifier(ch))
                {
                    isError = !isNewLineOrEOF;

                    break;
                }

                ch = _scanner.Read();
            }

            var end = _scanner.CurrentIndex;

            if (isError) end++;

            return new Token(
                start..end,
                isError ? TokenKind.Error : TokenKind.Identifier,
                _scanner.GetSlice(start..end));
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

            var start = _scanner.CurrentIndex;
            var isError = false;

            while (true)
            {
                var ch = _scanner.Read();

                if (ch == closingChar)
                {
                    var isEscaped = _scanner.Peek(1) == closingChar;

                    if (!isEscaped) break;

                    // TODO: Begin building a token value that has the unescaped text.
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

            var end = _scanner.CurrentIndex;
            var slice = (start, end);
            
            if (end < _scanner.TextLength) end++;

            if (isError)
            {
                slice.end = end;
            }
            else
            {
                slice.start++;
            }

            return new Token(
                start..end,
                isError ? TokenKind.Error : kind,
                _scanner.GetSlice(slice.start..slice.end));
        }
    }
}
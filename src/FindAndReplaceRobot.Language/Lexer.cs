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
                switch (_scanner.ReadChar())
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
                        _scanner.MoveNext();
                        return LexQuotedLiteral(TokenKind.Label);
                    case '"':
                        _scanner.MoveNext();
                        return LexQuotedLiteral(TokenKind.String);
                    case '/':
                        _scanner.MoveNext();
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

                _scanner.MoveNext();
            }

            Token CreateToken(TokenKind kind)
            {
                var range = _scanner.CurrentIndex..(_scanner.CurrentIndex + 1);

                _prevKind = kind;

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
            bool isError;

            while (true)
            {
                var ch = _scanner.ReadChar();
                var isNewLineOrEOF = IsCharNewLineOrEOF(ch);

                if (isNewLineOrEOF || !IsCharIdentifier(ch))
                {
                    isError = !isNewLineOrEOF;

                    break;
                }

                _scanner.MoveNext();
            }

            var end = _scanner.CurrentIndex;

            if (isError)
            {
                end++;
            }
            else
            {
                end = _scanner.GetSliceEnding(start..end) == TextEndingFlags.CR
                        ? end - 1
                        : end;
            }

            _scanner.MoveNext();

            return new Token(
                        start..end,
                        isError ? TokenKind.Error : TokenKind.Identifier,
                        _scanner.GetSlice(start..end));
        }

        private Token LexQuotedLiteral(TokenKind kind)
        {
            var start = _scanner.CurrentIndex - 1;
            var isError = false;
            var closingChar = kind switch
            {
                TokenKind.Label => ']',
                TokenKind.String => '"',
                TokenKind.Regex => '/',
            };

            while (true)
            {
                var ch = _scanner.ReadChar();

                if (ch == closingChar)
                {
                    var offset = 1;
                    var isEscaped = _scanner.PeekAhead(ref offset) == closingChar;
                    _scanner.StepTo(offset: 1);

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

                _scanner.MoveNext();

                if (isError) break;
            }

            var end = _scanner.CurrentIndex - 1;
            var slice = (start, end);
            var isEmpty = start == end;

            if (end < _scanner.TextLength) end++;

            if (isError)
            {
                slice.end = end;
            }
            else
            {
                slice.start = isEmpty ? slice.end : slice.start + 1;
            }

            return new Token(
                start..end,
                isError ? TokenKind.Error : kind,
                _scanner.GetSlice(slice.start..slice.end));
        }
    }
}
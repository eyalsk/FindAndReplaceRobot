namespace FindAndReplaceRobot.Language.Tests.Extensions
{
    using System.Collections.Generic;

    using FindAndReplaceRobot.Language.Tokens;

    internal static class LexerExtension
    {
        public static Token ReadTokenByKind(this Lexer lexer, TokenKind kind)
        {
            while (true)
            {
                var token = lexer.ReadToken();

                if (token.Kind == kind)
                {
                    return token;
                }
                else if (token.Kind == TokenKind.EndOfLine)
                {
                    return new Token(token.Start, token.End, TokenKind.Error, token.Value);
                }
            }
        }

        public static IEnumerable<Token> ReadTokensByKind(this Lexer lexer, TokenKind kind)
        {
            var tokens = new List<Token>();

            while (true)
            {
                var token = lexer.ReadToken();

                if (token.Kind == kind)
                {
                    tokens.Add(token);
                }
                else if (token.Kind == TokenKind.EndOfLine)
                {
                    break;
                }
            }

            return tokens;
        }
    }
}

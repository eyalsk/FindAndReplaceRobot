namespace FindAndReplaceRobot.Language.Tests.Extensions
{
    using System.Collections.Generic;
    using FindAndReplaceRobot.Language.Tokens;

    internal static class LexerExtension
    {
        public static Token? ReadTokenByKind(this Lexer lexer, TokenKind kind)
        {
            while (true)
            {
                var token = lexer.ReadToken();

                if (token == null || token.Kind == kind) return token;
            }
        }

        public static IReadOnlyList<Token> ReadTokensByKind(this Lexer lexer, TokenKind kind)
        {
            var tokens = new List<Token>();

            while (true)
            {
                var token = lexer.ReadToken();

                if (token?.Kind == kind)
                {
                    tokens.Add(token);
                }
                else if (token == null)
                {
                    break;
                }
            }

            return tokens;
        }
    }
}

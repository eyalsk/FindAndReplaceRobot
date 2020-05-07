namespace FindAndReplaceRobot.Language.Tests.Extensions
{
    using System.Collections.Generic;
    using FindAndReplaceRobot.Language;

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
                else if (token.Kind == TokenKind.EndOfFile)
                {
                    return new Token(token.Range, token.Depth, TokenKind.Error, TokenKind.None, token.Value);
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
                else if (token.Kind == TokenKind.EndOfFile)
                {
                    break;
                }
            }

            return tokens;
        }
    }
}

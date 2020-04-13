namespace FindAndReplaceRobot.Language.Tests.Extensions
{
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
    }
}

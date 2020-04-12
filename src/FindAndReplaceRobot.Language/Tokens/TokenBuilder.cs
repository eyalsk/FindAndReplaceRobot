namespace FindAndReplaceRobot.Language.Tokens
{
    using System.Text;

    public sealed class TokenBuilder
    {
        private int _startIndex = -1;
        private int _endIndex = -1;
        private TokenKind _kind;
        private StringBuilder _builder = new StringBuilder();

        public bool HasValidToken =>
            _startIndex > -1 &&
            _endIndex > -1 &&
            _kind != TokenKind.None &&
            _builder.Length > 0;

        public TokenBuilder StartAt(int index)
        {
            _startIndex = index;

            return this;
        }

        public TokenBuilder EndAt(int index)
        {
            _endIndex = index;

            return this;
        }

        public TokenBuilder SetKind(TokenKind kind)
        {
            _kind = kind;

            return this;
        }

        public TokenBuilder AppendChar(char value)
        {
            _builder.Append(value);

            return this;
        }

        public Token Build()
        {
            var chunks = _builder.GetChunks();
            chunks.MoveNext();

            var token = new Token(_startIndex, _endIndex, _kind, chunks.Current);

            _startIndex = -1;
            _endIndex = -1;
            _kind = TokenKind.None;

            _builder.Clear();

            return token;
        }
    }
}

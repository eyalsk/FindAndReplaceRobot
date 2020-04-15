namespace FindAndReplaceRobot.Language.Tokens
{
    using System;
    using System.Collections.Immutable;

    public sealed class TokenBuilder
    {
        private int _start;
        private TokenKind _kind;
        private ReadOnlyMemory<char> _value;
        private ImmutableArray<Token>.Builder? _builder;
        
        public TokenBuilder AddToken(Token token)
        {
            _builder ??= ImmutableArray.CreateBuilder<Token>();
            _builder.Add(token);

            return this;
        }

        public TokenBuilder SetPosition(int start)
        {
            _start = start;

            return this;
        }

        public TokenBuilder SetKind(TokenKind kind)
        {
            _kind = kind;

            return this;
        }

        public TokenBuilder SetValue(ReadOnlyMemory<char> value)
        {
            _value = value;

            return this;
        }

        public Token Build()
        {
            var token = new Token(_start, _kind, _value, _builder is object ? _builder.ToImmutable() : default);

            _start = 0;
            _kind = TokenKind.None;
            _value = ReadOnlyMemory<char>.Empty;
            _builder?.Clear();

            return token;
        }
    }
}

namespace FindAndReplaceRobot.Language
{
    using System;

    public sealed class Token
    {
        public Token(Range range, TokenKind kind, ReadOnlyMemory<char> value)
        {
            if (range.Start.Value < 0) throw new ArgumentOutOfRangeException(nameof(range));
            if (range.End.Value < 0 || range.End.Value < range.Start.Value) throw new ArgumentOutOfRangeException(nameof(range));
            if (!Enum.IsDefined(typeof(TokenKind), kind)) throw new ArgumentException(nameof(kind));

            Range = range;
            Kind = kind;
            Value = value;
        }

        public Range Range { get; }

        public TokenKind Kind { get; }

        public ReadOnlyMemory<char> Value { get; }

        public override string ToString() =>
            Kind == TokenKind.Space ||
            Kind == TokenKind.Tab ||
            Kind == TokenKind.NewLine ||
            Kind == TokenKind.EndOfFile ? string.Empty : Value.ToString();
    }
}
namespace FindAndReplaceRobot.Language.Tests
{
    using System;

    using FindAndReplaceRobot.Language;

    using Shouldly;

    using Xunit;

    public sealed class TokenTests
    {
        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        public void Should_not_throw_when_range_start_above_or_equal_to_zero(int start)
        {
            Should.NotThrow(() =>
                new Token(range: start..(start + 1), kind: TokenKind.None, value: ReadOnlyMemory<char>.Empty));
        }

        [Fact]
        public void Should_throw_when_range_start_below_zero()
        {
            Should.Throw<ArgumentOutOfRangeException>(() =>
                new Token(range: -1..0, kind: TokenKind.None, value: ReadOnlyMemory<char>.Empty));
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        public void Should_not_throw_when_range_end_above_or_equal_to_zero(int end)
        {
            Should.NotThrow(() =>
                new Token(range: 0..end, kind: TokenKind.None, value: ReadOnlyMemory<char>.Empty));
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(0)]
        public void Should_throw_when_range_end_below_zero_or_below_start(int end)
        {
            Should.Throw<ArgumentOutOfRangeException>(() =>
                new Token(range: 1..end, kind: TokenKind.None, value: ReadOnlyMemory<char>.Empty));
        }

        [Fact]
        public void Should_not_throw_when_kind_is_defined()
        {
            Should.NotThrow(() =>
                new Token(range: .., kind: TokenKind.None, value: ReadOnlyMemory<char>.Empty));
        }

        [Fact]
        public void Should_throw_when_kind_is_undefined()
        {
            Should.Throw<ArgumentException>(() =>
                new Token(range: .., kind: (TokenKind)int.MinValue, value: ReadOnlyMemory<char>.Empty));
        }
    }
}
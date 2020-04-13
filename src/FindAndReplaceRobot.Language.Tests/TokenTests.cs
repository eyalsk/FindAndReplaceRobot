namespace FindAndReplaceRobot.Language.Tests
{
    using System;
    using FindAndReplaceRobot.Language.Tokens;
    using NUnit.Framework;
    using Shouldly;

    internal class TokenTests
    {
        [TestCase(0)]
        [TestCase(1)]
        public void Should_not_throw_when_start_above_or_equal_to_zero(int start)
        {
            Should.NotThrow(() => new Token(start, kind: TokenKind.None, value: ReadOnlyMemory<char>.Empty));
        }

        [Test]
        public void Should_throw_when_start_below_zero()
        {
            Should.Throw<ArgumentOutOfRangeException>(() => new Token(start: -1, kind: TokenKind.None, value: ReadOnlyMemory<char>.Empty));
        }

        [Test]
        public void Should_not_throw_when_kind_is_defined()
        {
            Should.NotThrow(() => new Token(start: 0, kind: TokenKind.None, value: ReadOnlyMemory<char>.Empty));
        }

        [Test]
        public void Should_throw_when_kind_is_undefined()
        {
            Should.Throw<ArgumentException>(() => new Token(start: 0, kind: (TokenKind)int.MinValue, value: ReadOnlyMemory<char>.Empty));
        }
    }
}
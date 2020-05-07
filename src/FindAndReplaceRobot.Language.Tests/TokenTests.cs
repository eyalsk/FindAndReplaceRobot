namespace FindAndReplaceRobot.Language.Tests
{
    using System;
    using FindAndReplaceRobot.Language;

    using NUnit.Framework;

    using Shouldly;

    internal class TokenTests
    {
        [TestCase(0)]
        [TestCase(1)]
        public void Should_not_throw_when_range_start_above_or_equal_to_zero(int start)
        {
            Should.NotThrow(() =>
                new Token(range: start..(start + 1), depth: 1, kind: TokenKind.None, TokenKind.None, value: ReadOnlyMemory<char>.Empty));
        }

        [Test]
        public void Should_throw_when_range_start_below_zero()
        {
            Should.Throw<ArgumentOutOfRangeException>(() =>
                new Token(range: -1..0, depth: 1, kind: TokenKind.None, TokenKind.None, value: ReadOnlyMemory<char>.Empty));
        }

        [TestCase(0)]
        [TestCase(1)]
        public void Should_not_throw_when_range_end_above_or_equal_to_zero(int end)
        {
            Should.NotThrow(() =>
                new Token(range: 0..end, depth: 1, kind: TokenKind.None, TokenKind.None, value: ReadOnlyMemory<char>.Empty));
        }

        [TestCase(-1)]
        [TestCase(0)]
        public void Should_throw_when_end_below_zero_or_below_start(int end)
        {
            Should.Throw<ArgumentOutOfRangeException>(() =>
                new Token(range: 1..end, depth: 1, kind: TokenKind.None, TokenKind.None, value: ReadOnlyMemory<char>.Empty));
        }

        [Test]
        public void Should_not_throw_when_kind_is_defined()
        {
            Should.NotThrow(() =>
                new Token(range: .., depth: 1, kind: TokenKind.None, TokenKind.None, value: ReadOnlyMemory<char>.Empty));
        }

        [Test]
        public void Should_throw_when_kind_is_undefined()
        {
            Should.Throw<ArgumentException>(() =>
                new Token(range: .., depth: 1, kind: (TokenKind)int.MinValue, TokenKind.None, value: ReadOnlyMemory<char>.Empty));
        }

        [Test]
        public void Should_not_throw_when_context_is_defined()
        {
            Should.NotThrow(() =>
                new Token(range: .., depth: 1, kind: TokenKind.None, TokenKind.None, value: ReadOnlyMemory<char>.Empty));
        }

        [Test]
        public void Should_throw_when_context_is_undefined()
        {
            Should.Throw<ArgumentException>(() =>
                new Token(range: .., depth: 1,kind: TokenKind.None, (TokenKind)int.MinValue, value: ReadOnlyMemory<char>.Empty));
        }

        [Test]
        public void Should_not_throw_when_depth_above_zero()
        {
            Should.NotThrow(() =>
                new Token(range: .., depth: 1, kind: TokenKind.None, TokenKind.None, value: ReadOnlyMemory<char>.Empty));
        }

        [TestCase(-1)]
        [TestCase(0)]
        public void Should_throw_when_depth_below_or_equal_zero(int depth)
        {
            Should.Throw<ArgumentOutOfRangeException>(() =>
                new Token(range: .., depth, kind: TokenKind.None, TokenKind.None, value: ReadOnlyMemory<char>.Empty));
        }
    }
}
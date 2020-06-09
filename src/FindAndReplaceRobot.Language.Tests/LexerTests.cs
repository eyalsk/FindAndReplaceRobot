namespace FindAndReplaceRobot.Language.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using FindAndReplaceRobot.Language;
    using FindAndReplaceRobot.Language.Tests.Extensions;

    using Shouldly;

    using Xunit;

    public sealed class LexerTests
    {
        [Fact]
        public void Should_not_throw_when_scanner_is_created()
        {
            Should.NotThrow(() => new Lexer(scanner: new Scanner("")));
        }

        [Fact]
        public void Should_throw_when_scanner_is_null()
        {
            Should.Throw<ArgumentNullException>(() => new Lexer(scanner: null!));
        }

        public sealed class Identifiers
        {
            public static IEnumerable<object[]> IdentifierCases => new[]
            {
                new object[] { "@A", (1..2, "A") },
                new object[] { "@A1\n", (1..3, "A1") },
                new object[] { "@A\r\n", (1..2, "A") },
                new object[] { "@A\r\n@B2", (1..2, "A"), (5..7, "B2") },
                new object[] { "@A\n@B", (1..2, "A"), (4..5, "B") },
            };

            public static IEnumerable<object[]> MalformedIdentifierCases => new[]
            {
                new object[] { "@@", 1..2, "@" },
                new object[] { "@A B", 1..3, "A " },
                // todo: new object[] { "@A\nB", 1..3, "A\n" }
            };

            [Theory]
            [MemberData(nameof(IdentifierCases))]
            public void Should_lex_identifiers(string text, params (Range, string)[] expectedTokensInfo)
            {
                var scanner = new Scanner(text);
                var lexer = new Lexer(scanner);

                lexer.ReadTokensByKind(TokenKind.Identifier)
                     .Select(t => (t.Range, t.Value.ToString()))
                     .ShouldBe(expectedTokensInfo);
            }

            [Theory]
            [MemberData(nameof(MalformedIdentifierCases))]
            public void Should_not_lex_malformed_identifiers(string text, Range expectedRange, string expectedValue)
            {
                var scanner = new Scanner(text);
                var lexer = new Lexer(scanner);

                lexer.ReadTokensByKind(TokenKind.Error)
                     .Select(t => (t.Range, t.Value.ToString()))
                     .FirstOrDefault()
                     .ShouldBe((expectedRange, expectedValue));
            }
        }

        public sealed class Labels
        {
            public static IEnumerable<object[]> LabelCases => new[]
            {
                new object[] { "[]", (0..2, "") },
                new object[] { "[A]", (0..3, "A") },
                new object[] { "[A1B]\n", (0..5, "A1B") },
                new object[] { "[A B]\r\n", (0..5, "A B") },
                new object[] { "[A]\r\n[B]", (0..3, "A"), (5..8, "B") },
                new object[] { "[A]\n[2]", (0..3, "A"), (4..7, "2") }
            };

            public static IEnumerable<object[]> MalformedLabelCases => new[]
            {
                new object[] { "[", 0..1, "[" },
                new object[] { "[[", 0..2, "[[" },
                new object[] { "[[]", 0..2, "[[" },
                new object[] { "[]]", 0..3, "[]]" },
                new object[] { "[]A", 0..3, "[]A" },
                new object[] { "[A", 0..2, "[A" },
                new object[] { "[A\r\nB]", 0..4, "[A\r\n" },
                new object[] { "[A\rB]", 0..3, "[A\r" },
                new object[] { "[A\n2]", 0..3, "[A\n" },
                new object[] { "[A&2]", 0..3, "[A&" }
            };

            [Theory]
            [MemberData(nameof(LabelCases))]
            public void Should_lex_labels(string text, params (Range, string)[] expectedTokensInfo)
            {
                var scanner = new Scanner(text);
                var lexer = new Lexer(scanner);

                lexer.ReadTokensByKind(TokenKind.Label)
                     .Select(t => (t.Range, t.Value.ToString()))
                     .ShouldBe(expectedTokensInfo);
            }

            [Theory]
            [MemberData(nameof(MalformedLabelCases))]
            public void Should_not_lex_malformed_labels(string text, Range expectedRange, string expectedValue)
            {
                var scanner = new Scanner(text);
                var lexer = new Lexer(scanner);
                var token = lexer.ReadToken();

                token.ShouldSatisfyAllConditions(
                    () => token.Range.ShouldBe(expectedRange),
                    () => token.Kind.ShouldBe(TokenKind.Error),
                    () => token.Value.ToString().ShouldBe(expectedValue));
            }
        }

        public sealed class Symbols
        {
            public static IEnumerable<object[]> SymbolCases => new[]
            {
                new object[] { '@', TokenKind.AtSign },
                new object[] { '(', TokenKind.OpenParens },
                new object[] { ')', TokenKind.CloseParens },
                new object[] { ',', TokenKind.Comma },
                new object[] { '\0', TokenKind.EndOfFile }
            };

            [Theory]
            [MemberData(nameof(SymbolCases))]
            public void Should_lex_symbols(char symbols, TokenKind results)
            {
                var scanner = new Scanner(symbols.ToString());
                var lexer = new Lexer(scanner);
                var token = lexer.ReadToken();

                token.Kind.ShouldBe(results);
            }
        }
    }
}
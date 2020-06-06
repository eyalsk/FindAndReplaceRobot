namespace FindAndReplaceRobot.Language.Tests
{
    using System;
    using System.Linq;

    using FindAndReplaceRobot.Language;
    using FindAndReplaceRobot.Language.Tests.Extensions;

    using NUnit.Framework;

    using Shouldly;

    internal class LexerTests
    {
        [Test]
        public void Should_not_throw_when_scanner_is_created()
        {
            Should.NotThrow(() => new Lexer(scanner: new Scanner("")));
        }

        [Test]
        public void Should_throw_when_scanner_is_null()
        {
            Should.Throw<ArgumentNullException>(() => new Lexer(scanner: null!));
        }

        [TestCase('@', TokenKind.AtSign)]
        [TestCase('(', TokenKind.OpenParens)]
        [TestCase(')', TokenKind.CloseParens)]
        [TestCase(',', TokenKind.Comma)]
        [TestCase('\0', TokenKind.EndOfFile)]
        public void Should_lex_symbols(char symbols, TokenKind results)
        {
            var scanner = new Scanner(symbols.ToString());
            var lexer = new Lexer(scanner);
            var token = lexer.ReadToken();

            token.Kind.ShouldBe(results);
        }

        [TestCase("@A", "Range:1..2; Value:A")]
        [TestCase("@A1\n", "Range:1..3; Value:A1")]
        [TestCase("@A\r\n", "Range:1..2; Value:A")]
        [TestCase("@A\r\n@B2", "Range:1..2; Value:A", "Range:5..7; Value:B2")]
        [TestCase("@A\n@B", "Range:1..2; Value:A", "Range:4..5; Value:B")]
        public void Should_lex_identifiers(string text, params string[] identifiers)
        {
            var scanner = new Scanner(text);
            var lexer = new Lexer(scanner);

            lexer.ReadTokensByKind(TokenKind.Identifier)
                 .Select(t => t.GetRangeAndValue())
                 .ShouldBe(identifiers);
        }

        [TestCase("[]", "Range:0..2")]
        [TestCase("[A]", "Range:0..3; Value:A")]
        [TestCase("[A1B]\n", "Range:0..5; Value:A1B")]
        [TestCase("[A B]\r\n", "Range:0..5; Value:A B")]
        [TestCase("[A]\r\n[B]", "Range:0..3; Value:A", "Range:5..8; Value:B")]
        [TestCase("[A]\n[2]", "Range:0..3; Value:A", "Range:4..7; Value:2")]
        public void Should_lex_labels(string text, params string[] tokenInfo)
        {
            var scanner = new Scanner(text);
            var lexer = new Lexer(scanner);

            lexer.ReadTokensByKind(TokenKind.Label)
                 .Select(t => t.GetRangeAndValue())
                 .ShouldBe(tokenInfo);
        }

        [TestCase("[", "Range:0..1; Value:[")]
        [TestCase("[[", "Range:0..2; Value:[[")]
        [TestCase("[[]", "Range:0..2; Value:[[")]
        [TestCase("[]]", "Range:0..3; Value:[]]")]
        [TestCase("[]A", "Range:0..3; Value:[]A")]
        [TestCase("[A", "Range:0..2; Value:[A")]
        [TestCase("[A\r\nB]", "Range:0..4; Value:[A\r\n")]
        [TestCase("[A\rB]", "Range:0..3; Value:[A\r")]
        [TestCase("[A\n2]", "Range:0..3; Value:[A\n")]
        [TestCase("[A&2]", "Range:0..3; Value:[A&")]
        public void Should_not_lex_malformed_labels(string text, string tokenInfo)
        {
            var scanner = new Scanner(text);
            var lexer = new Lexer(scanner);
            var token = lexer.ReadToken();

            token.ShouldSatisfyAllConditions(
                () => token.GetRangeAndValue().ShouldBe(tokenInfo),
                () => token.Kind.ShouldBe(TokenKind.Error));
        }
    }
}
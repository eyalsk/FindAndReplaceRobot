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

        [TestCase("@A", "A")]
        [TestCase("@A1\n", "A1")]
        [TestCase("@A\r\n", "A")]
        [TestCase("@A\r\n@B2", "A", "B2")]
        [TestCase("@A\n@B", "A", "B")]
        public void Should_lex_identifiers(string text, params string[] identifiers)
        {
            var scanner = new Scanner(text);
            var lexer = new Lexer(scanner);

            lexer.ReadTokensByKind(TokenKind.Identifier)
                 .Select(t => t.Value.ToString())
                 .ShouldBe(identifiers);
        }

        [TestCase("[A]", "A")]
        [TestCase("[A1B]\n", "A1B")]
        [TestCase("[A B]\r\n", "A B")]
        [TestCase("[A]\r\n[B]", "A", "B")]
        [TestCase("[A]\n[2]", "A", "2")]
        public void Should_lex_labels(string text, params string[] labels)
        {
            var scanner = new Scanner(text);
            var lexer = new Lexer(scanner);

            lexer.ReadTokensByKind(TokenKind.Label)
                 .Select(t => t.Value.ToString())
                 .ShouldBe(labels);
        }
    }
}
namespace FindAndReplaceRobot.Language.Tests
{
    using System;
    using FindAndReplaceRobot.Language.Tests.Extensions;
    using FindAndReplaceRobot.Language.Tokens;
    using NUnit.Framework;
    using Shouldly;

    internal class LexerTests
    {
        [Test]
        public void Should_not_throw_when_scanner_is_object()
        {
            Should.NotThrow(() => new Lexer(new Scanner("")));
        }

        [Test]
        public void Should_throw_when_scanner_is_null()
        {
            Should.Throw<ArgumentNullException>(() => new Lexer(null!));
        }

        [TestCase("@MyCustomAnnotation")]
        [TestCase("@MyCustomAnnotation\r\n")]
        [TestCase("@MyCustomAnnotation\n")]
        [TestCase("@MyCustomAnnotation()")]
        [TestCase("@MyCustomAnnotation\r\n@MyCustomAnnotation")]
        [TestCase("@MyCustomAnnotation\n@MyCustomAnnotation()")]
        public void Should_succeed_lexing_annotations(string text)
        {
            var scanner = new Scanner(text);
            var lexer = new Lexer(scanner);

            var token = lexer.ReadTokenByKind(TokenKind.Annotation);

            lexer.ShouldSatisfyAllConditions(
                () => token.ShouldNotBeNull(),
                () => token!.Kind.ShouldBe(TokenKind.Annotation));
        }

        [TestCase("MyCustomAnnotation")]
        [TestCase("[MyCustomAnnotation]")]
        public void Should_fail_lexing_annotations(string text)
        {
            var scanner = new Scanner(text);
            var lexer = new Lexer(scanner);

            var token = lexer.ReadTokenByKind(TokenKind.Annotation);

            token.ShouldBeNull();
        }

        [Test]
        public void Should_succeed_lexing_sections()
        {
            var scanner = new Scanner("[MyCustomSection]");
            var lexer = new Lexer(scanner);

            var token = lexer.ReadTokenByKind(TokenKind.Section);

            lexer.ShouldSatisfyAllConditions(
                () => token.ShouldNotBeNull(),
                () => token!.Kind.ShouldBe(TokenKind.Section));
        }

        [TestCase("MyCustomAnnotation")]
        [TestCase("@MyCustomAnnotation()")]
        public void Should_fail_lexing_sections(string text)
        {
            var scanner = new Scanner(text);
            var lexer = new Lexer(scanner);

            var token = lexer.ReadTokenByKind(TokenKind.Section);

            token.ShouldBeNull();
        }
    }
}
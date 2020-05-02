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
        [TestCase("@MyCustomAnnotation( )")]
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

            token.Kind.ShouldBe(TokenKind.Error);
        }

        [TestCase("@MyCustomAnnotation([MySection])", "[MySection]")]
        [TestCase("@MyCustomAnnotation( [MySection])", "[MySection]")]
        [TestCase("@MyCustomAnnotation([MySection] )", "[MySection]")]
        [TestCase("@MyCustomAnnotation( [MySection] )", "[MySection]")]
        [TestCase("@MyCustomAnnotation([MySe ction])", "[MySe ction]")]
        [TestCase("@MyCustomAnnotation(\"MySection\")", "\"MySection\"")]
        [TestCase("@MyCustomAnnotation(\"MySection\", [MySection])", "\"MySection\"", "[MySection]")]
        public void Should_succeed_lexing_annotations_with_arguments(string text, params string[] args)
        {
            var scanner = new Scanner(text);
            var lexer = new Lexer(scanner);

            var tokens = lexer.ReadTokensByKind(TokenKind.AnnotationArgument);

            tokens.ShouldAllBe(t => args.Contains(t.Value.ToString()));
        }

        [TestCase("@MyCustomAnnotation([MySection]")]
        public void Should_fail_lexing_annotations_with_arguments(string text)
        {
            var scanner = new Scanner(text);
            var lexer = new Lexer(scanner);

            var token = lexer.ReadTokenByKind(TokenKind.AnnotationArgument);

            token.Kind.ShouldBe(TokenKind.Error);
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

            token.Kind.ShouldBe(TokenKind.Error);
        }
    }
}
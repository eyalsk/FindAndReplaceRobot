namespace FindAndReplaceRobot.Language.Tests
{
    using System;
    using System.Collections.Generic;
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

        [Test]
        public void Should_succeed_lexing_annotations_with_arguments()
        {
            var cases = new List<string>
            {
                "@MyCustomAnnotation([MySection])",
                "@MyCustomAnnotation( [MySection])",
                "@MyCustomAnnotation([MySection] )",
                "@MyCustomAnnotation( [MySection] )",
                "@MyCustomAnnotation([MySe ction])",
                "@MyCustomAnnotation(\"MySection\")",
                "@MyCustomAnnotation([MySection], \"MySection\")"
            };

            var results = new List<(string, TokenKind)>();

            foreach (var text in cases)
            {
                var scanner = new Scanner(text);
                var lexer = new Lexer(scanner);

                results.AddRange(lexer.ReadTokensByKind(TokenKind.AnnotationArgument).Select(t => (t.Value.ToString(), t.Context)));
            }

            results.ShouldBe(new[] {
                ("MySection", TokenKind.Section),
                ("MySection", TokenKind.Section),
                ("MySection", TokenKind.Section),
                ("MySection", TokenKind.Section),
                ("MySe ction", TokenKind.Section),
                ("MySection", TokenKind.String),
                ("MySection", TokenKind.Section),
                ("MySection", TokenKind.String),
            });
        }

        [TestCase("@MyCustomAnnotation([MySection]")]
        [TestCase("@MyCustomAnnotation[MySection]")]
        [TestCase("@MyCustomAnnotation(\"MySection])")]
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

        [Test]
        public void Should_succeed_lexing_newlines()
        {
            var scanner = new Scanner("@Annotation\r\n@Annotation\n[Section]");
            var lexer = new Lexer(scanner);

            var results = new List<Token>();

            while (true)
            {
                var token = lexer.ReadToken();

                if (token.Kind == TokenKind.EndOfFile) break;

                results.Add(token);
            }

            results.Count.ShouldBe(3);
        }

        [Test]
        public void Should_succeed_lexing_indents()
        {
            var scanner = new Scanner("[Section]\nItem1\n  @Annotation\n@Annotation\n\tItem2");
            var lexer = new Lexer(scanner);

            var tokens = lexer.ReadTokensByKind(TokenKind.Indent).ToList();

            tokens.Count.ShouldBe(2);
        }
    }
}
namespace FindAndReplaceRobot.Language.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using FindAndReplaceRobot.Language;
    using FindAndReplaceRobot.Language.Tests.Extensions;
    using FindAndReplaceRobot.Language.Tests.Utils;

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

        [TestCase("@🎲")]
        [TestCase("@🎲\r\n")]
        [TestCase("@🎲\n")]
        [TestCase("@🎲()")]
        [TestCase("@🎲( )")]
        [TestCase("@🎲\r\n@🎲")]
        [TestCase("@🎲\n@🎲()")]
        public void Should_lex_well_formed_annotations(string pattern)
        {
            var text = Randomizer.GenerateOnlyLettersString(pattern);
            var scanner = new Scanner(text);
            var lexer = new Lexer(scanner);
            var token = lexer.ReadTokenByKind(TokenKind.Annotation);

            token.Kind.ShouldBe(TokenKind.Annotation);
        }

        [TestCase("🎲")]
        [TestCase("[🎲]")]
        public void Should_not_lex_malformed_annotations(string pattern)
        {
            var text = Randomizer.GenerateOnlyLettersString(pattern);
            var scanner = new Scanner(text);
            var lexer = new Lexer(scanner);
            var token = lexer.ReadTokenByKind(TokenKind.Annotation);

            token.Kind.ShouldBe(TokenKind.Error);
        }

        [Test]
        public void Should_lex_well_formed_annotations_with_arguments()
        {
            var patterns = new List<string>
            {
                "@🎲([🎲])",
                "@🎲( [🎲])",
                "@🎲([🎲] )",
                "@🎲( [🎲] )",
                "@🎲([🎲 🎲])",
                "@🎲(\"🎲\")",
                "@🎲([🎲], \"🎲\")"
            };

            var results = new List<TokenKind>();

            foreach (var pattern in patterns)
            {
                var text = Randomizer.GenerateOnlyLettersString(pattern);
                var scanner = new Scanner(text);
                var lexer = new Lexer(scanner);

                results.AddRange(lexer.ReadTokensByKind(TokenKind.AnnotationArgument).Select(t => (t.Context)));
            }

            results.ShouldBe(new[] {
                TokenKind.Section,
                TokenKind.Section,
                TokenKind.Section,
                TokenKind.Section,
                TokenKind.Section,
                TokenKind.String,
                TokenKind.Section,
                TokenKind.String
            });
        }

        [TestCase("@🎲([🎲]")]
        [TestCase("@🎲[🎲]")]
        [TestCase("@🎲(\"🎲])")]
        public void Should_not_lex_annotations_with_malformed_arguments(string pattern)
        {
            var text = Randomizer.GenerateOnlyLettersString(pattern);
            var scanner = new Scanner(text);
            var lexer = new Lexer(scanner);
            var token = lexer.ReadTokenByKind(TokenKind.AnnotationArgument);

            token.Kind.ShouldBe(TokenKind.Error);
        }

        [Test]
        public void Should_lex_well_formed_sections()
        {
            var text = Randomizer.GenerateString("[🎲]");
            var scanner = new Scanner(text);
            var lexer = new Lexer(scanner);
            var token = lexer.ReadTokenByKind(TokenKind.Section);

            token.Kind.ShouldBe(TokenKind.Section);
        }

        [TestCase("🎲")]
        [TestCase("@🎲()")]
        public void Should_not_lex_malformed_sections(string pattern)
        {
            var text = Randomizer.GenerateString(pattern);
            var scanner = new Scanner(text);
            var lexer = new Lexer(scanner);
            var token = lexer.ReadTokenByKind(TokenKind.Section);

            token.Kind.ShouldBe(TokenKind.Error);
        }

        [TestCase("🎲")]
        [TestCase("🎲 -> 🎲")]
        public void Should_lex_items(string pattern)
        {
            var text = Randomizer.GenerateString(pattern);
            var scanner = new Scanner(text);
            var lexer = new Lexer(scanner);
            var token = lexer.ReadTokenByKind(TokenKind.Item);

            token.Kind.ShouldBe(TokenKind.Item);
        }

        [Test]
        public void Should_lex_text_with_mixed_newlines()
        {
            var scanner = new Scanner("@A\r\n@Bc\n[S]");
            var lexer = new Lexer(scanner);
            var results = new List<(string, TokenKind)>();

            while (true)
            {
                var token = lexer.ReadToken();

                if (token.Kind == TokenKind.EndOfFile) break;

                results.Add((token.Value.ToString(), token.Kind));
            }

            results.ShouldBe(new[]
            {
                ("A", TokenKind.Annotation),
                ("Bc", TokenKind.Annotation),
                ("S", TokenKind.Section)
            });
        }

        [TestCase("I1\n I2\n  I3")]
        [TestCase("I1\r\n I2\r\n  I3")]
        public void Should_lex_text_with_nested_constructs(string text)
        {
            var scanner = new Scanner(text);
            var lexer = new Lexer(scanner);
            var results = new List<(string, int)>();

            while (true)
            {
                var token = lexer.ReadToken();

                if (token.Kind == TokenKind.EndOfFile) break;

                results.Add((token.Value.ToString(), token.Depth));
            }

            results.ShouldBe(new[]
            {
                ("I1", 1),
                ("I2", 2),
                ("I3", 3)
            });
        }

        [TestCase("I1\n I2\n\n I3")]
        [TestCase("I1\r\n I2\r\n\r\n I3")]
        public void Should_lex_text_with_blank_lines(string text)
        {
            var scanner = new Scanner(text);
            var lexer = new Lexer(scanner);
            var results = new List<(string, int)>();

            while (true)
            {
                var token = lexer.ReadToken();

                if (token.Kind == TokenKind.EndOfFile) break;

                results.Add((token.Value.ToString(), token.Depth));
            }

            results.ShouldBe(new[]
            {
                ("I1", 1),
                ("I2", 2),
                ("I3", 2),
            });
        }

        [TestCase("@A    \n@B")]
        [TestCase("@A    \r\n@B")]
        [TestCase("@A()    \n@B")]
        [TestCase("@A()    \r\n@B")]
        [TestCase("[A]    \n[B]")]
        [TestCase("[A]    \r\n[B]")]
        [TestCase("A    \nB")]
        [TestCase("A    \r\nB")]
        public void Should_lex_text_with_leading_spaces(string text)
        {
            var scanner = new Scanner(text);
            var lexer = new Lexer(scanner);
            var results = new List<string>();

            while (true)
            {
                var token = lexer.ReadToken();

                if (token.Kind == TokenKind.EndOfFile) break;

                results.Add(token.Value.ToString());
            }

            results.ShouldBe(new[] { "A", "B" });
        }
    }
}
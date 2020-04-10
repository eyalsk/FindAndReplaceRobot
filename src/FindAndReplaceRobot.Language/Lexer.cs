namespace FindAndReplaceRobot.Language
{
    using System;

    public sealed class Lexer
    {
        private readonly Scanner _scanner;

        public Lexer(Scanner scanner)
        {
            _scanner = scanner;
        }

        public void ReadToken()
        {
            while (true)
            {
                switch (_scanner.ReadChar())
                {
                    case '@':
                        LexAnnotations();
                        Console.WriteLine();
                        break;
                    case '[':
                        LexSections();
                        Console.WriteLine();
                        break;
                }

                if (!_scanner.Next()) break;
            }
        }

        private void LexAnnotations()
        {
            for (int index = 0; ; index++)
            {
                var ch = _scanner.ReadAhead(index);

                if (ch != '@' && !char.IsLetterOrDigit(ch))
                {
                    break;
                }

                Console.Write(ch);
            }

            _scanner.MoveAhead();
        }

        private void LexSections()
        {
            for (int index = 0; ; index++)
            {
                var ch = _scanner.ReadAhead(index);

                if (ch == ']')
                {
                    Console.Write(ch);
                    break;
                }

                Console.Write(ch);
            }

            _scanner.MoveAhead();
        }
    }
}

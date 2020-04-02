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

        public Token ReadToken()
        {
            while (_scanner.Next())
            {
                var ch = _scanner.GetCurrentValue();

                switch (ch.Value)
                {
                    case '@' when ch.Position.ColumnNumber == 1:
                    {
                        break;
                    }
                };
            }

            throw new NotImplementedException();
        }
    }
}

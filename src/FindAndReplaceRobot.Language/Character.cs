namespace FindAndReplaceRobot.Language
{
    using static InvisibleCharacters;

    public struct Character
    {
        public Character(char value, Position position)
        {
            Value = value;

            Position = position;
        }

        public Position Position { get; }

        public char Value { get; }

        public override string ToString() => 
            Value switch {
                Space       => "Space",
                Tab         => "Tab",
                Return      => "Return",
                NewLine     => "NewLine",
                EndOfFile   => "EOF",
                _           => Value.ToString()
            };
    }
}
namespace FindAndReplaceRobot.Language
{
    using static InvisibleCharacters;

    public static class CharExtension
    {
        public static string ToReadableString(this char value) =>
            value switch
            {
                Space => "Space",
                Tab => "Tab",
                Return => "Return",
                NewLine => "NewLine",
                EndOfFile => "EOF",
                _ => value.ToString()
            };
    }
}
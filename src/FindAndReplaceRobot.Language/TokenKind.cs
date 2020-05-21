namespace FindAndReplaceRobot.Language
{
    public enum TokenKind
    {
        None,

        AtSign,
        Comma,

        OpenParens,
        CloseParens,

        OpenBracket,
        CloseBracket,

        Value,

        QuotedLiteral,
        Label,
        String,
        Regex,

        Literal,
        Identifier,
        Integer,

        EndOfFile,
        NewLine,
        Tab,
        Space,

        Error
    }
}

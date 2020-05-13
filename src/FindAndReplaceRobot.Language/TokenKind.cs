namespace FindAndReplaceRobot.Language
{
    public enum TokenKind
    {
        None = 0,

        Section = 100,
        Subsection = 101,
        Item = 102,

        Annotation = 200,
        AnnotationArgumentList = 201,
        AnnotationArgument = 202,

        Map = 300,
        Value = 301,
        Label = 302,
        Identifier = 303,

        Operator = 400,
        LHS = 401,
        RHS = 402,

        ValueLiteral = 500,
        String = 501,
        Integer = 502,
        Regex = 503,

        Whitespace = 600,
        EndOfFile = 601,
        NewLine = 602,
        Indent = 603,
        Tab = 604,
        Space = 605,

        Error = 700,

        Keyword = 800
    }
}

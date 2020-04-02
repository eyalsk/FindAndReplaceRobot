namespace FindAndReplaceRobot.Language
{
    public enum TokenType
    {
        None = 0,

        Section = 100,
        Subsection = 101,
        Set = 102,

        Annotation = 200,
        AnnotationArgumentList = 201,
        AnnotationArgument = 202,

        Map = 300,
        Value = 301,
        Label = 302,
        Identifier = 303,

        ValueLiteral = 400,
        String = 401,
        Integer = 402,
        Regex = 403,

        Whitespace = 500,
        Indent = 501,
        Newline = 502,

        Keyword = 600
    }
}

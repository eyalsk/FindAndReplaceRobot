namespace FindAndReplaceRobot.Language
{
    using System;

    [Flags]
    public enum TextEndingFlags
    {
        None,
        CR = 1 << 0,
        LF = 1 << 1,
        CRLF = CR | LF,
        EOF = 1 << 2
    }
}
﻿namespace FindAndReplaceRobot.Language.Driver
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using FindAndReplaceRobot.Language.Tokens;

    internal static class Program
    {
        private static void Main()
        {
            var basePath = AppContext.BaseDirectory;
            var projectPath = basePath.Substring(0, basePath.IndexOf("bin"));
            var filePath = Path.Combine(projectPath, "test.farr");

            var text = File.ReadAllText(filePath);
            var scanner = new Scanner(text);
            var lexer = new Lexer(scanner);

            Console.WriteLine($"Text\t\t\tStart Position\tLength\tToken Type");

            while (true)
            {
                var token = lexer.ReadToken();

                if (token is null) break;

                Console.WriteLine($"{token.Value}\n\t\t\t{token.Start}\t\t{token.Length}\t{token.Kind}");
            }
        }
    }
}

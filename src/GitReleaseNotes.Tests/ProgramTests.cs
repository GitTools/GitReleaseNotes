#region License

// --------------------------------------------------
// Copyright © OKB. All Rights Reserved.
// 
// This software is proprietary information of OKB.
// USE IS SUBJECT TO LICENSE TERMS.
// --------------------------------------------------

#endregion

using System;
using System.IO;

using Args;
using Args.Help;
using Args.Help.Formatters;

using Shouldly;

using Xunit;

namespace GitReleaseNotes.Tests
{
    public class ProgramTests
    {
        [Fact]
        public void NoArgumentsShouldOutputHelp()
        {
            using (var programWriter = new StringWriter())
            {
                var originalOut = Console.Out;
                try
                {
                    Console.SetOut(programWriter);
                    Program.Main(new string[0]);

                    var modelBindingDefinition = Configuration.Configure<GitReleaseNotesArguments>();
                    var help = new HelpProvider().GenerateModelHelp(modelBindingDefinition);

                    var bufferWidth = Console.IsOutputRedirected ? 80 : Console.BufferWidth;
                    var f = new ConsoleHelpFormatter(bufferWidth, 1, 5);

                    using (var helpWriter = new StringWriter())
                    {
                        f.WriteHelp(help, helpWriter);

                        programWriter.ToString().ShouldContain(helpWriter.ToString());
                    }
                }
                finally
                {
                    Console.SetOut(originalOut);
                }
            }
        }
    }
}
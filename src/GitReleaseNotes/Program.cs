using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Args;
using Args.Help;
using Args.Help.Formatters;
using GitReleaseNotes.FileSystem;

namespace GitReleaseNotes
{
    public static class Program
    {
        // TODO Fix logging.. Just choose serilog or something which liblog picks up
        private static readonly ILog Log = GitReleaseNotesEnvironment.Log;

        static int Main(string[] args)
        {
            GitReleaseNotesEnvironment.Log = new ConsoleLog();

            var modelBindingDefinition = Configuration.Configure<GitReleaseNotesArguments>();

            if (args.Any(a => a == "/?" || a == "?" || a.Equals("/help", StringComparison.InvariantCultureIgnoreCase)))
            {
                ShowHelp(modelBindingDefinition);

                return 0;
            }

            var exitCode = 0;

            var arguments = modelBindingDefinition.CreateAndBind(args);
            if (string.IsNullOrEmpty(arguments.OutputFile))
            {
                ShowHelp(modelBindingDefinition);
                return 1;
            }
             
            var parameters = arguments.ToParameters();
            //if (!context.Validate())
            //{
            //    return -1;
            //}

            // In case the user puts in a relative path as current directory, first get the full path
            parameters.WorkingDirectory = Path.GetFullPath(parameters.WorkingDirectory);

            try
            {
                var fileSystem = new FileSystem.FileSystem();
                var releaseFileWriter = new ReleaseFileWriter(fileSystem);
                string outputFile = null;
                var previousReleaseNotes = new SemanticReleaseNotes();

                var outputPath = parameters.WorkingDirectory;
                var outputDirectory = new DirectoryInfo(outputPath);
                if (outputDirectory.Name == ".git")
                {
                    outputPath = outputDirectory.Parent.FullName;
                }

                if (!string.IsNullOrEmpty(arguments.OutputFile))
                {
                    outputFile = Path.IsPathRooted(arguments.OutputFile)
                        ? arguments.OutputFile
                        : Path.Combine(outputPath, arguments.OutputFile);
                    previousReleaseNotes = new ReleaseNotesFileReader(fileSystem, outputPath).ReadPreviousReleaseNotes(outputFile);
                }

                var releaseNotesGenerator = new ReleaseNotesGenerator(parameters);
                var releaseNotes = releaseNotesGenerator.GenerateReleaseNotesAsync(previousReleaseNotes).Result;

                var releaseNotesOutput = releaseNotes.ToString();
                releaseFileWriter.OutputReleaseNotesFile(releaseNotesOutput, outputFile);

                Log.WriteLine("Done");
            }
            catch (GitReleaseNotesException ex)
            {
                exitCode = -1;
                Log.WriteLine("An expected error occurred: {0}", ex.Message);
            }
            catch (Exception ex)
            {
                exitCode = -2;
                Log.WriteLine("An unexpected error occurred: {0}", ex.Message);
            }

            if (Debugger.IsAttached)
            {
                Console.WriteLine("Press any key to continue");
                Console.ReadKey();
            }

            return exitCode;
        }

        private static void ShowHelp(IModelBindingDefinition<GitReleaseNotesArguments> modelBindingDefinition, string reason = null)
        {
            var help = new HelpProvider().GenerateModelHelp(modelBindingDefinition);
            var f = new ConsoleHelpFormatter();
            f.WriteHelp(help, Console.Out);

            if (reason != null)
            {
                Console.WriteLine();
                Console.WriteLine(reason);
            }
        }
    }
}

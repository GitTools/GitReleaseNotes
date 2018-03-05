using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
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

        public static int Main(string[] args)
        {
            // Add the TLS 1.2 protocol to the Service Point manager to fix `The request was aborted: Could not create SSL/TLS secure channel.`
            ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12;

            GitReleaseNotesEnvironment.Log = new ConsoleLog();

            var modelBindingDefinition = Configuration.Configure<GitReleaseNotesArguments>();

            if (!args.Any() || args.Any(a => a == "/?" || a == "?" || a.Equals("/help", StringComparison.InvariantCultureIgnoreCase)))
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
            var context = arguments.ToContext();
            //if (!context.Validate())
            //{
            //    return -1;
            //}

            try
            {
                var fileSystem = new FileSystem.FileSystem();
                var releaseFileWriter = new ReleaseFileWriter(fileSystem);
                string outputFile = null;
                var previousReleaseNotes = new SemanticReleaseNotes();

                var outputPath = context.WorkingDirectory;
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

                var releaseNotesGenerator = new ReleaseNotesGenerator(context);
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

            var bufferWidth = Console.IsOutputRedirected ? 80 : Console.BufferWidth;
            var f = new ConsoleHelpFormatter(bufferWidth, 1, 5);
            f.WriteHelp(help, Console.Out);

            if (reason != null)
            {
                Console.WriteLine();
                Console.WriteLine(reason);
            }
        }
    }
}

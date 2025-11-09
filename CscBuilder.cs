using System;
using System.IO;

namespace CscBuilder
{
    class Program
    {
        static int Main(string[] args)
        {
            try
            {
                Console.WriteLine("CscBuilder - Simple C# Build Tool");
                Console.WriteLine("==================================");
                Console.WriteLine();

                // Determine config file path
                string configPath = DetermineConfigPath(args);

                if (configPath == null)
                {
                    PrintUsage();
                    return 1;
                }

                Console.WriteLine("Using configuration: " + configPath);
                Console.WriteLine();

                // Load configuration
                BuildConfig config = BuildConfig.Load(configPath);

                // Validate configuration
                if (config.SourceFiles.Count == 0)
                {
                    Console.Error.WriteLine("Error: No source files found to compile.");
                    Console.Error.WriteLine("Check your <Compile Include=\"...\"/> entries in the configuration file.");
                    return 1;
                }

                // Build command line
                CscCommandBuilder commandBuilder = new CscCommandBuilder(config);
                commandBuilder.PrintBuildInfo();
                commandBuilder.EnsureOutputDirectory();

                string commandLine = commandBuilder.BuildCommandLine();

                // Execute build
                BuildExecutor executor = new BuildExecutor(config.FrameworkVersion);
                bool success = executor.Execute(commandLine);

                return success ? 0 : 1;
            }
            catch (FileNotFoundException ex)
            {
                Console.Error.WriteLine("Error: " + ex.Message);
                return 1;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("Unexpected error: " + ex.Message);
                Console.Error.WriteLine(ex.StackTrace);
                return 1;
            }
        }

        private static string DetermineConfigPath(string[] args)
        {
            // If argument provided, use it
            if (args.Length > 0)
            {
                string path = args[0];
                if (File.Exists(path))
                {
                    return Path.GetFullPath(path);
                }
                else
                {
                    Console.Error.WriteLine("Error: Configuration file not found: " + path);
                    return null;
                }
            }

            // Otherwise, search for build.xml in current directory
            string defaultConfig = Path.Combine(Environment.CurrentDirectory, "build.xml");
            if (File.Exists(defaultConfig))
            {
                return defaultConfig;
            }

            Console.Error.WriteLine("Error: No build.xml found in current directory.");
            Console.Error.WriteLine("Please provide a configuration file path as an argument.");
            return null;
        }

        private static void PrintUsage()
        {
            Console.WriteLine("Usage:");
            Console.WriteLine("  CscBuilder.exe [config-file]");
            Console.WriteLine();
            Console.WriteLine("Arguments:");
            Console.WriteLine("  config-file    Path to XML configuration file (optional)");
            Console.WriteLine("                 If not specified, looks for 'build.xml' in current directory");
            Console.WriteLine();
            Console.WriteLine("Example:");
            Console.WriteLine("  CscBuilder.exe");
            Console.WriteLine("  CscBuilder.exe mybuild.xml");
        }
    }
}

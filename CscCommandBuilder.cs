using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace CscBuilder
{
    public class CscCommandBuilder
    {
        private BuildConfig config;

        public CscCommandBuilder(BuildConfig config)
        {
            this.config = config;
        }

        public string BuildCommandLine()
        {
            var args = new List<string>();

            // Output file
            string outputPath = config.GetFullOutputPath();
            args.Add("/out:" + QuoteIfNeeded(outputPath));

            // Target type (exe or library)
            string target = config.OutputType == "dll" ? "library" : "exe";
            args.Add("/target:" + target);

            // Platform
            args.Add("/platform:" + config.Platform);

            // Configuration-specific options
            if (config.Configuration.Equals("Release", StringComparison.OrdinalIgnoreCase))
            {
                args.Add("/optimize+");
            }
            else if (config.Configuration.Equals("Debug", StringComparison.OrdinalIgnoreCase))
            {
                args.Add("/optimize-");
                args.Add("/debug+");
                args.Add("/debug:full");
            }

            // References
            foreach (string reference in config.References)
            {
                args.Add("/reference:" + QuoteIfNeeded(reference));
            }

            // Source files
            foreach (string sourceFile in config.SourceFiles)
            {
                args.Add(QuoteIfNeeded(sourceFile));
            }

            return string.Join(" ", args);
        }

        private string QuoteIfNeeded(string path)
        {
            // Quote the path if it contains spaces
            if (path.Contains(" "))
            {
                return "\"" + path + "\"";
            }
            return path;
        }

        public void EnsureOutputDirectory()
        {
            string outputPath = config.GetFullOutputPath();
            string outputDir = Path.GetDirectoryName(outputPath);

            if (!string.IsNullOrEmpty(outputDir) && !Directory.Exists(outputDir))
            {
                Directory.CreateDirectory(outputDir);
                Console.WriteLine("Created output directory: " + outputDir);
            }
        }

        public void PrintBuildInfo()
        {
            Console.WriteLine("=== CscBuilder - Build Configuration ===");
            Console.WriteLine("Framework:     " + config.FrameworkVersion);
            Console.WriteLine("Configuration: " + config.Configuration);
            Console.WriteLine("Platform:      " + config.Platform);
            Console.WriteLine("Output Type:   " + config.OutputType);
            Console.WriteLine("Output:        " + config.GetFullOutputPath());
            Console.WriteLine("Source Files:  " + config.SourceFiles.Count + " file(s)");
            Console.WriteLine("References:    " + config.References.Count + " reference(s)");
            Console.WriteLine();
        }
    }
}

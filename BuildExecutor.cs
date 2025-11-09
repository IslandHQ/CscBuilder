using System;
using System.Diagnostics;
using System.IO;
using Microsoft.Win32;

namespace CscBuilder
{
    public class BuildExecutor
    {
        private string cscPath;

        public BuildExecutor(string frameworkVersion)
        {
            cscPath = FindCscPath(frameworkVersion);
        }

        private string FindCscPath(string frameworkVersion)
        {
            string systemRoot = Environment.GetEnvironmentVariable("SystemRoot");

            // First, try to find the specified framework version
            string[] specifiedPaths = new string[]
            {
                Path.Combine(systemRoot, @"Microsoft.NET\Framework64\" + frameworkVersion + @"\csc.exe"),
                Path.Combine(systemRoot, @"Microsoft.NET\Framework\" + frameworkVersion + @"\csc.exe"),
            };

            foreach (string path in specifiedPaths)
            {
                if (File.Exists(path))
                {
                    return path;
                }
            }

            // Fallback: Try to find csc.exe in common locations (64-bit versions prioritized)
            string[] fallbackPaths = new string[]
            {
                // .NET Framework 4.0 (64-bit first)
                Path.Combine(systemRoot, @"Microsoft.NET\Framework64\v4.0.30319\csc.exe"),
                Path.Combine(systemRoot, @"Microsoft.NET\Framework\v4.0.30319\csc.exe"),

                // .NET Framework 3.5 (64-bit first)
                Path.Combine(systemRoot, @"Microsoft.NET\Framework64\v3.5\csc.exe"),
                Path.Combine(systemRoot, @"Microsoft.NET\Framework\v3.5\csc.exe"),

                // .NET Framework 2.0 (64-bit first)
                Path.Combine(systemRoot, @"Microsoft.NET\Framework64\v2.0.50727\csc.exe"),
                Path.Combine(systemRoot, @"Microsoft.NET\Framework\v2.0.50727\csc.exe"),
            };

            foreach (string path in fallbackPaths)
            {
                if (File.Exists(path))
                {
                    return path;
                }
            }

            // If not found, assume csc.exe is in PATH
            return "csc.exe";
        }

        public bool Execute(string commandLine)
        {
            if (cscPath != "csc.exe")
            {
                Console.WriteLine("Using C# compiler: " + cscPath);
            }
            else
            {
                Console.WriteLine("Using C# compiler from PATH");
            }

            Console.WriteLine();
            Console.WriteLine("=== Building... ===");

            ProcessStartInfo psi = new ProcessStartInfo
            {
                FileName = cscPath,
                Arguments = commandLine,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            try
            {
                using (Process process = Process.Start(psi))
                {
                    string output = process.StandardOutput.ReadToEnd();
                    string error = process.StandardError.ReadToEnd();

                    process.WaitForExit();

                    // Display output
                    if (!string.IsNullOrWhiteSpace(output))
                    {
                        Console.WriteLine(output);
                    }

                    if (!string.IsNullOrWhiteSpace(error))
                    {
                        Console.Error.WriteLine(error);
                    }

                    if (process.ExitCode == 0)
                    {
                        Console.WriteLine();
                        Console.WriteLine("=== Build succeeded ===");
                        return true;
                    }
                    else
                    {
                        Console.WriteLine();
                        Console.WriteLine("=== Build failed with exit code: " + process.ExitCode + " ===");
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("Error executing csc.exe: " + ex.Message);
                return false;
            }
        }
    }
}

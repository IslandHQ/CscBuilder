using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace CscBuilder
{
    public class BuildConfig
    {
        // PropertyGroup settings
        public string OutputType { get; set; }       // exe or dll
        public string OutputPath { get; set; }
        public string AssemblyName { get; set; }
        public string Platform { get; set; }         // x86, x64, anycpu
        public string Configuration { get; set; }    // Debug, Release
        public string FrameworkVersion { get; set; } // v4.0.30319, v3.5, v2.0.50727

        // ItemGroup settings
        public List<string> SourceFiles { get; set; }
        public List<string> References { get; set; }

        // Base directory for relative paths
        public string BaseDirectory { get; set; }

        public BuildConfig()
        {
            // Default values
            OutputType = "exe";
            OutputPath = "bin";
            AssemblyName = "output";
            Platform = "anycpu";
            Configuration = "Release";
            FrameworkVersion = "v4.0.30319";
            SourceFiles = new List<string>();
            References = new List<string>();
            BaseDirectory = Environment.CurrentDirectory;
        }

        public static BuildConfig Load(string configPath)
        {
            if (!File.Exists(configPath))
            {
                throw new FileNotFoundException("Configuration file not found: " + configPath);
            }

            var config = new BuildConfig();
            config.BaseDirectory = Path.GetDirectoryName(Path.GetFullPath(configPath));

            XDocument doc = XDocument.Load(configPath);
            XElement root = doc.Root;

            if (root == null || root.Name.LocalName != "Project")
            {
                throw new InvalidOperationException("Invalid XML format. Root element must be <Project>");
            }

            // Parse PropertyGroup
            XElement propGroup = root.Element("PropertyGroup");
            if (propGroup != null)
            {
                config.ParsePropertyGroup(propGroup);
            }

            // Parse ItemGroup
            foreach (XElement itemGroup in root.Elements("ItemGroup"))
            {
                config.ParseItemGroup(itemGroup);
            }

            return config;
        }

        private void ParsePropertyGroup(XElement propGroup)
        {
            if (propGroup.Element("OutputType") != null)
                OutputType = propGroup.Element("OutputType").Value.Trim().ToLower();

            if (propGroup.Element("OutputPath") != null)
                OutputPath = propGroup.Element("OutputPath").Value.Trim();

            if (propGroup.Element("AssemblyName") != null)
                AssemblyName = propGroup.Element("AssemblyName").Value.Trim();

            if (propGroup.Element("Platform") != null)
                Platform = propGroup.Element("Platform").Value.Trim().ToLower();

            if (propGroup.Element("Configuration") != null)
                Configuration = propGroup.Element("Configuration").Value.Trim();

            if (propGroup.Element("FrameworkVersion") != null)
                FrameworkVersion = propGroup.Element("FrameworkVersion").Value.Trim();
        }

        private void ParseItemGroup(XElement itemGroup)
        {
            // Parse Compile elements
            foreach (XElement compile in itemGroup.Elements("Compile"))
            {
                XAttribute attr = compile.Attribute("Include");
                string include = attr != null ? attr.Value : null;
                if (!string.IsNullOrWhiteSpace(include))
                {
                    ExpandAndAddFiles(include, SourceFiles);
                }
            }

            // Parse Reference elements
            foreach (XElement reference in itemGroup.Elements("Reference"))
            {
                XAttribute attr = reference.Attribute("Include");
                string include = attr != null ? attr.Value : null;
                if (!string.IsNullOrWhiteSpace(include))
                {
                    References.Add(include.Trim());
                }
            }
        }

        private void ExpandAndAddFiles(string pattern, List<string> targetList)
        {
            // Handle wildcard patterns like **/*.cs or *.cs
            if (pattern.Contains("*"))
            {
                string searchPath = BaseDirectory;
                SearchOption searchOption = SearchOption.TopDirectoryOnly;
                string searchPattern = pattern;

                // Handle **/*.cs pattern (recursive search)
                if (pattern.StartsWith("**/"))
                {
                    searchOption = SearchOption.AllDirectories;
                    searchPattern = pattern.Substring(3); // Remove **/
                }
                else if (pattern.Contains("/") || pattern.Contains("\\"))
                {
                    // Handle patterns like src/*.cs
                    int lastSep = Math.Max(pattern.LastIndexOf('/'), pattern.LastIndexOf('\\'));
                    searchPath = Path.Combine(BaseDirectory, pattern.Substring(0, lastSep));
                    searchPattern = pattern.Substring(lastSep + 1);
                }

                if (Directory.Exists(searchPath))
                {
                    string[] files = Directory.GetFiles(searchPath, searchPattern, searchOption);
                    targetList.AddRange(files);
                }
            }
            else
            {
                // Direct file path
                string fullPath = Path.Combine(BaseDirectory, pattern);
                if (File.Exists(fullPath))
                {
                    targetList.Add(fullPath);
                }
            }
        }

        public string GetOutputFileName()
        {
            string extension = OutputType == "dll" ? ".dll" : ".exe";
            return AssemblyName + extension;
        }

        public string GetFullOutputPath()
        {
            string outputDir = Path.IsPathRooted(OutputPath)
                ? OutputPath
                : Path.Combine(BaseDirectory, OutputPath);

            return Path.Combine(outputDir, GetOutputFileName());
        }
    }
}

using NDesk.Options;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GitExporter
{
    class Program
    {
        // Args.
        private static string oldcommit;
        private static string youngcommit;
        private static bool verbose;
        private static string directory;

        /// <summary>
        /// Quick and dirty export tool for changed files in GIT between two 
        /// commits.
        /// </summary>
        /// <param name="args">3 args - Hashes (or IDs) of the two commits, then output directory.</param>
        static void Main(string[] args)
        {
            var optionset = new OptionSet()
            {
                {"oc=", "[required] Old Commit", x => oldcommit = x},
                {"yc=", "[required] Young Commit", x => youngcommit = x},
                {"v", "Verbose", x => verbose = true},
                {"d=", "[required] Directory to Export to", x => directory = x},
            };

            optionset.Parse(args);

            if (string.IsNullOrEmpty(oldcommit) 
                || string.IsNullOrEmpty(youngcommit)
                || string.IsNullOrEmpty(directory))
            {
                optionset.WriteOptionDescriptions(Console.Out);
                return;
            }

            var info = new ProcessStartInfo()
            {
                FileName = "git",
                Arguments = string.Format("diff-tree -r --no-commit-id --name-only --diff-filter=ACMT {0} {1}", oldcommit, youngcommit),
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = true
            };
            var proc = Process.Start(info);
            var outputsb = new StringBuilder();
            while (!proc.StandardOutput.EndOfStream)
                outputsb.AppendLine(proc.StandardOutput.ReadLine());

            // each file is seperated by newline.
            var files = outputsb.ToString().Split(new string[]{Environment.NewLine}, StringSplitOptions.RemoveEmptyEntries);
            foreach(var f in files)
            {
                var fn = Environment.CurrentDirectory + "\\" + f;
                var newname = directory + "\\" + f;
                var dir = Path.GetDirectoryName(newname);
                if (!Directory.Exists(dir))
                    Directory.CreateDirectory(dir);
                if (File.Exists(fn))
                {
                    File.Copy(fn, newname, true);
                    Verbose("Copied: " + newname);
                }
                else
                {
                    Verbose("Could not Find File: " + fn);
                }
            }
        }

        public static void Verbose(string line)
        {
            if (verbose)
            {
                Console.WriteLine(line);
            }
        }
    }
}

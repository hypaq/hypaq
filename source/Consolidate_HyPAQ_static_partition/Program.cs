using System;
using System.Collections.Generic;
using System.IO;

namespace LogFileConsolidator
{
    class Program
    {
        /// <summary>
        /// Main method that processes the log files and consolidates the results.
        /// </summary>
        /// <param name="args">Command-line arguments. Expects one argument: the filename containing the list of source folders.</param>
        static void Main(string[] args)
        {
            if (args.Length != 1)
            {
                Console.WriteLine("Usage: LogFileConsolidator.exe <list_of_source_folders.txt>");
                return;
            }

            string sourceFoldersFile = args[0];

            if (!File.Exists(sourceFoldersFile))
            {
                Console.WriteLine($"Error: The file '{sourceFoldersFile}' does not exist.");
                return;
            }

            List<string> sourceFolders = new List<string>();

            // Read the list of source folders from the file
            try
            {
                using (StreamReader reader = new StreamReader(sourceFoldersFile))
                {
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        if (!string.IsNullOrWhiteSpace(line))
                        {
                            sourceFolders.Add(line.Trim());
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading the source folders file: {ex.Message}");
                return;
            }

            DateTime endTime = DateTime.Now;
            // Prepare the output file
            string outputFile = Path.Combine(Directory.GetCurrentDirectory(), "finalresults_hypaq_static_partition - " + endTime.ToString("yyyy-MM-dd-HHmmss") + ".txt");

            // Write the header to the output file
            try
            {
                using (StreamWriter writer = new StreamWriter(outputFile))
                {
                    writer.WriteLine("Processing file;Number of vertices;Number of hyperedges;Number of min.cuts;Total Time Spent");

                    // Process each source folder
                    foreach (string sourceFolder in sourceFolders)
                    {
                        if (Directory.Exists(sourceFolder))
                        {
                            // Get all .log files in the source folder
                            string[] logFiles = Directory.GetFiles(sourceFolder, "*.log", SearchOption.AllDirectories);

                            foreach (string logFile in logFiles)
                            {
                                try
                                {
                                    // Extract the required information from the log file
                                    Dictionary<string, string> logInfo = ExtractLogInfo(logFile);

                                    // Write the information to the output file
                                    if (logInfo != null)
                                    {
                                        writer.WriteLine($"{logInfo["Processing file"]};{logInfo["Number of vertices"]};{logInfo["Number of hyperedges"]};{logInfo["Number of min.cuts"]};{logInfo["Total Time Spent"]}");
                                    }
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine($"Error processing log file '{logFile}': {ex.Message}");
                                }
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Warning: The directory '{sourceFolder}' does not exist.");
                        }
                    }
                }

                Console.WriteLine($"Consolidation complete. Results saved to '{outputFile}'.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error writing to the output file: {ex.Message}");
                return;
            }
        }

        /// <summary>
        /// Extracts the required information from a log file.
        /// </summary>
        /// <param name="logFile">The path to the log file.</param>
        /// <returns>A dictionary containing the extracted information.</returns>
        static Dictionary<string, string> ExtractLogInfo(string logFile)
        {
            Dictionary<string, string> logInfo = new Dictionary<string, string>
            {
                { "Processing file", "" },
                { "Number of vertices", "" },
                { "Number of hyperedges", "" },
                { "Number of min.cuts", "" },
                { "Total Time Spent", "" }
            };

            using (StreamReader reader = new StreamReader(logFile))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    // Check for the required information
                    if (line.StartsWith("Processing file:"))
                    {
                        logInfo["Processing file"] = line.Substring("Processing file:".Length).Trim();
                    }
                    else if (line.StartsWith("Number of vertices:"))
                    {
                        logInfo["Number of vertices"] = line.Substring("Number of vertices:".Length).Trim();
                    }
                    else if (line.StartsWith("Number of hyperedges:"))
                    {
                        logInfo["Number of hyperedges"] = line.Substring("Number of hyperedges:".Length).Trim();
                    }
                    else if (line.StartsWith("Number of min.cuts:"))
                    {
                        logInfo["Number of min.cuts"] = line.Substring("Number of min.cuts:".Length).Trim();
                    }
                    else if (line.StartsWith("Total Time Spent:"))
                    {
                        logInfo["Total Time Spent"] = line.Substring("Total Time Spent:".Length).Trim();
                    }
                }
            }

            // Check if all required information was found
            foreach (var key in logInfo.Keys)
            {
                if (string.IsNullOrEmpty(logInfo[key]))
                {
                    Console.WriteLine($"Warning: Missing '{key}' in log file '{logFile}'.");
                    return null; // Skip this log file
                }
            }

            return logInfo;
        }
    }
}

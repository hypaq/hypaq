using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Collections.Generic;

namespace QASMToQRMConverter
{
    class Program
    {
        // Define a dictionary to hold gate times in nanoseconds
        static Dictionary<string, double> gateTimes = new Dictionary<string, double>()
        {
            // Single-Qubit Gates
            { "I", 0 },
            { "X", 35 },
            { "Y", 35 },
            { "Z", 0 },
            { "H", 35 },
            { "S", 0 },
            { "T", 0 },
            { "RX", 35 },
            { "RY", 35 },
            { "RZ", 0 },
            // Two-Qubit Gates
            { "CX", 350 }, // Average of 200 ns - 500 ns
            { "CZ", 350 },
            { "ISWAP", 350 },
            // Multi-Qubit Gates
            { "CCX", 700 }, // Assuming double the time of CX
            // Measurement
            { "MEASURE", 5500 } // Average of 4 µs - 7 µs (converted to ns)
        };

        static void Main(string[] args)
        {
            if (args.Length < 1)
            {
                Console.WriteLine("Please provide the path to the folder containing QASM files.");
                return;
            }

            string folderPath = args[0];

            if (!Directory.Exists(folderPath))
            {
                Console.WriteLine("The provided folder path does not exist.");
                return;
            }

            string[] qasmFiles = Directory.GetFiles(folderPath, "*.qasm");

            foreach (string filePath in qasmFiles)
            {
                ProcessQASMFile(filePath);
            }

            Console.WriteLine("Processing complete. Check the folder for generated .qrm files.");
            GenerateConsolidatedFile(folderPath);
        }

        static void ProcessQASMFile(string filePath)
        {
            string[] lines = File.ReadAllLines(filePath);
            string outputFilePath = Path.ChangeExtension(filePath, ".qrm");

            using (StreamWriter writer = new StreamWriter(outputFilePath))
            {
                bool isQPUActive = false;
                int totalQubits = CountTotalQubits(lines);
                int qubitBoundary = totalQubits / 2; // Balance qubits equally between QPU1 and QPU2

                for (int i = 0; i < lines.Length; i++)
                {
                    string line = lines[i].Trim();

                    // Skip empty lines
                    if (string.IsNullOrWhiteSpace(line))
                    {
                        writer.WriteLine(line);
                        continue;
                    }

                    // Determine QPU based on the qubit index in the line
                    int qubitIndex = GetQubitIndex(line);
                    bool shouldSwitchQPUForLine = qubitIndex >= qubitBoundary;

                    // Start a new QPU block if necessary
                    if (qubitIndex != -1 && !isQPUActive)
                    {
                        writer.WriteLine(shouldSwitchQPUForLine ? "#pragma qpu_begin QPU2" : "#pragma qpu_begin QPU1");
                        isQPUActive = true;
                    }

                    // Write the line
                    writer.WriteLine(line);

                    // Close QPU block if next line should belong to another QPU
                    if (i + 1 < lines.Length)
                    {
                        int nextQubitIndex = GetQubitIndex(lines[i + 1].Trim());
                        if ((nextQubitIndex >= qubitBoundary && qubitIndex < qubitBoundary) || (nextQubitIndex < qubitBoundary && qubitIndex >= qubitBoundary))
                        {
                            if (isQPUActive)
                            {
                                writer.WriteLine("#pragma qpu_end");
                                isQPUActive = false;
                            }
                        }
                    }
                }

                // Close any remaining open QPU block
                if (isQPUActive)
                {
                    writer.WriteLine("#pragma qpu_end");
                }
            }
        }

        static int GetQubitIndex(string line)
        {
            Match match = Regex.Match(line, @"q\[(\d+)\]");
            if (match.Success)
            {
                return int.Parse(match.Groups[1].Value);
            }
            return -1;
        }

        static int CountTotalQubits(string[] lines)
        {
            int maxIndex = -1;
            foreach (string line in lines)
            {
                int qubitIndex = GetQubitIndex(line);
                if (qubitIndex > maxIndex)
                {
                    maxIndex = qubitIndex;
                }
            }
            return maxIndex + 1; // Since qubit indices start from 0
        }

        static void GenerateConsolidatedFile(string folderPath)
        {
            string[] qrmFiles = Directory.GetFiles(folderPath, "*.qrm");
            string outputFilePath = Path.Combine(folderPath, "pragma_results.csv");

            using (StreamWriter writer = new StreamWriter(outputFilePath))
            {
                // Write CSV header
                writer.WriteLine("File;Number of Threads;Number of #pragma qpu_begin;Number of #pragma qpu_end;Number of Binary Operations;Number of Cross-QPU Operations;Number of Qubits;Total Exec Time Estimated (ns)");

                foreach (string filePath in qrmFiles)
                {
                    string[] lines = File.ReadAllLines(filePath);
                    int numThreads = Regex.Matches(string.Join("\n", lines), "#pragma qpu_begin").Count;
                    int numQPUBegin = Regex.Matches(string.Join("\n", lines), "#pragma qpu_begin").Count;
                    int numQPUEnd = Regex.Matches(string.Join("\n", lines), "#pragma qpu_end").Count;
                    int numBinaryOps = CountBinaryOperations(lines);
                    int numCrossQPUOps = CountCrossQPUOperations(lines);
                    int totalQubits = CountTotalQubits(lines);
                    double totalExecTime = EstimateTotalExecutionTime(lines);

                    string fileName = Path.GetFileName(filePath);
                    writer.WriteLine($"{fileName};{numThreads};{numQPUBegin};{numQPUEnd};{numBinaryOps};{numCrossQPUOps};{totalQubits};{totalExecTime}");
                }
            }

            Console.WriteLine("Consolidated summary file created: " + outputFilePath);
        }

        static int CountBinaryOperations(string[] lines)
        {
            int count = 0;
            string binaryOpPattern = @"\b(cx|cz|cswap|iswap)\b";

            foreach (string line in lines)
            {
                if (Regex.IsMatch(line, binaryOpPattern))
                {
                    count++;
                }
            }

            return count;
        }

        static int CountCrossQPUOperations(string[] lines)
        {
            int count = 0;
            int totalQubits = CountTotalQubits(lines);
            int qubitBoundary = totalQubits / 2;

            foreach (string line in lines)
            {
                if (line.StartsWith("cx") || line.StartsWith("cz") || line.StartsWith("cswap") || line.StartsWith("iswap"))
                {
                    MatchCollection matches = Regex.Matches(line, @"q\[(\d+)\]");
                    if (matches.Count >= 2)
                    {
                        int qubit1 = int.Parse(matches[0].Groups[1].Value);
                        int qubit2 = int.Parse(matches[1].Groups[1].Value);

                        // Check if qubits belong to different QPUs
                        if ((qubit1 < qubitBoundary && qubit2 >= qubitBoundary) || (qubit1 >= qubitBoundary && qubit2 < qubitBoundary))
                        {
                            count++;
                        }
                    }
                }
            }

            return count;
        }

        static double EstimateTotalExecutionTime(string[] lines)
        {
            double totalTime = 0;

            foreach (string line in lines)
            {
                string trimmedLine = line.Trim();

                // Skip comments, pragmas, and empty lines
                if (string.IsNullOrWhiteSpace(trimmedLine) || trimmedLine.StartsWith("//") || trimmedLine.StartsWith("#"))
                {
                    continue;
                }

                // Extract the instruction
                string instruction = GetInstruction(trimmedLine);

                if (instruction != null)
                {
                    double gateTime = GetGateTime(instruction);
                    totalTime += gateTime;
                }
            }

            return totalTime;
        }

        static string GetInstruction(string line)
        {
            // Remove any parameters (e.g., angles in rotations)
            string lineWithoutParams = Regex.Replace(line, @"\s*\(.*\)", "");

            // Match instruction at the beginning of the line
            Match match = Regex.Match(lineWithoutParams, @"^(\w+)");
            if (match.Success)
            {
                return match.Groups[1].Value.ToUpper();
            }
            return null;
        }

        static double GetGateTime(string instruction)
        {
            // Handle rotation gates (e.g., RX, RY, RZ)
            if (instruction.StartsWith("RX") || instruction.StartsWith("RY") || instruction.StartsWith("RZ"))
            {
                instruction = instruction.Substring(0, 2).ToUpper();
            }

            if (gateTimes.ContainsKey(instruction))
            {
                return gateTimes[instruction];
            }

            // For measurement instructions
            if (instruction.StartsWith("MEASURE"))
            {
                return gateTimes["MEASURE"];
            }

            // Default to 0 if the instruction is not recognized
            return 0;
        }
    }
}

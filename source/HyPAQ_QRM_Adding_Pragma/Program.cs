using System;
using System.IO;
using System.Text.RegularExpressions;

namespace QASMToQRMConverter
{
    class Program
    {
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

                    // Handle function definitions and segments
                    if (line.StartsWith("def ") || line.StartsWith("gate "))
                    {
                        writer.WriteLine(line);
                        int functionEndIndex = FindFunctionEndIndex(lines, i);
                        for (int j = i + 1; j <= functionEndIndex; j++)
                        {
                            string functionLine = lines[j].Trim();
                            HandleLineWithQPUPragmas(writer, functionLine, ref isQPUActive, qubitBoundary);
                        }
                        i = functionEndIndex;
                        continue;
                    }

                    // Handle control structures (if, else, while, for)
                    if (line.StartsWith("if") || line.StartsWith("else") || line.StartsWith("while") || line.StartsWith("for"))
                    {
                        if (isQPUActive)
                        {
                            writer.WriteLine("#pragma qpu_end");
                            isQPUActive = false;
                        }

                        writer.WriteLine(line);

                        // Special handling for 'for' loops to add QPU primitives
                        if (line.StartsWith("for"))
                        {
                            writer.WriteLine("{");

                            // Determine which QPU should handle the loop based on qubit usage
                            int loopEndIndex = FindLoopEndIndex(lines, i);
                            if (loopEndIndex != -1)
                            {
                                // Split loop into two parts if it crosses QPU boundaries
                                writer.WriteLine($"// Split loop between QPU1 and QPU2 based on qubit boundary");
                                HandleLoopPragmas(writer, lines, i + 1, loopEndIndex, qubitBoundary);

                                i = loopEndIndex;
                                writer.WriteLine("}");
                                continue;
                            }
                        }

                        continue;
                    }

                    // Handle regular lines
                    HandleLineWithQPUPragmas(writer, line, ref isQPUActive, qubitBoundary);
                }

                // Close any remaining open QPU block
                if (isQPUActive)
                {
                    writer.WriteLine("#pragma qpu_end");
                }
            }
        }

        static void HandleLineWithQPUPragmas(StreamWriter writer, string line, ref bool isQPUActive, int qubitBoundary)
        {
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

            // Close QPU block if line ends a function or loop
            if (line.StartsWith("measure") && isQPUActive)
            {
                writer.WriteLine("#pragma qpu_end");
                isQPUActive = false;
            }
        }

        static void HandleLoopPragmas(StreamWriter writer, string[] lines, int startIndex, int loopEndIndex, int qubitBoundary)
        {
            for (int i = startIndex; i <= loopEndIndex; i++)
            {
                string loopLine = lines[i].Trim();
                int qubitIndex = GetQubitIndex(loopLine);
                bool shouldSwitchQPUForLine = qubitIndex >= qubitBoundary;

                if (qubitIndex != -1)
                {
                    writer.WriteLine(shouldSwitchQPUForLine ? "#pragma qpu_begin QPU2" : "#pragma qpu_begin QPU1");
                }

                writer.WriteLine(loopLine);

                if (qubitIndex != -1)
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
            return maxIndex + 1;
        }

        static int FindFunctionEndIndex(string[] lines, int startIndex)
        {
            int braceCount = 0;
            for (int i = startIndex; i < lines.Length; i++)
            {
                string line = lines[i].Trim();
                if (line.Contains("{")) braceCount++;
                if (line.Contains("}")) braceCount--;
                if (braceCount == 0 && i != startIndex) return i;
            }
            return -1;
        }

        static int FindLoopEndIndex(string[] lines, int startIndex)
        {
            int braceCount = 0;
            for (int i = startIndex; i < lines.Length; i++)
            {
                string line = lines[i].Trim();
                if (line.Contains("{")) braceCount++;
                if (line.Contains("}")) braceCount--;
                if (braceCount == 0 && i != startIndex) return i;
            }
            return -1;
        }
    }
}

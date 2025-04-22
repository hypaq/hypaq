using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace QasmToHypergraph
{
    class Program
    {
        /// <summary>
        /// Main method that processes the .qasm files and generates .ph and .dh files.
        /// </summary>
        /// <param name="args">Command-line arguments. Expects one argument: the filename containing the list of source folders.</param>
        static void Main(string[] args)
        {
            if (args.Length != 1)
            {
                Console.WriteLine("Usage: QasmToHypergraph.exe <list_of_source_folders.txt>");
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

            // Process each source folder
            foreach (string sourceFolder in sourceFolders)
            {
                if (Directory.Exists(sourceFolder))
                {
                    // Get all .qasm files in the source folder and subfolders
                    string[] qasmFiles = Directory.GetFiles(sourceFolder, "*.qasm", SearchOption.AllDirectories);

                    foreach (string qasmFile in qasmFiles)
                    {
                        try
                        {
                            Console.WriteLine($"Processing file: {qasmFile}");
                            ProcessQasmFile(qasmFile);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error processing .qasm file '{qasmFile}': {ex.Message}");
                        }
                    }
                }
                else
                {
                    Console.WriteLine($"Warning: The directory '{sourceFolder}' does not exist.");
                }
            }

            Console.WriteLine("Processing complete.");
        }

        /// <summary>
        /// Processes a single .qasm file, generating the .ph and .dh files.
        /// </summary>
        /// <param name="qasmFile">The path to the .qasm file.</param>
        static void ProcessQasmFile(string qasmFile)
        {
            // Read the .qasm file and parse it to extract gates and qubits
            List<Gate> gates = ParseQasmFile(qasmFile);

            if (gates == null || gates.Count == 0)
            {
                Console.WriteLine($"No gates found in file '{qasmFile}'.");
                return;
            }

            // Get the list of qubits used in the circuit
            HashSet<string> qubits = new HashSet<string>();

            foreach (var gate in gates)
            {
                qubits.UnionWith(gate.Qubits);
            }

            List<string> qubitList = qubits.OrderBy(q => q).ToList();

            // Create a mapping from qubit name to index
            Dictionary<string, int> qubitIndices = new Dictionary<string, int>();
            for (int i = 0; i < qubitList.Count; i++)
            {
                qubitIndices[qubitList[i]] = i;
            }

            // Build the incidence matrix for the primal hypergraph
            int numGates = gates.Count;
            int numQubits = qubitList.Count;

            int[,] incidenceMatrix = new int[numGates, numQubits];

            for (int i = 0; i < numGates; i++)
            {
                foreach (string qubit in gates[i].Qubits)
                {
                    int qubitIndex = qubitIndices[qubit];
                    incidenceMatrix[i, qubitIndex] = 1;
                }
            }

            // Write the incidence matrix to the .ph file
            string phFile = Path.ChangeExtension(qasmFile, ".ph");
            WriteIncidenceMatrix(phFile, incidenceMatrix);

            // Build the incidence matrix for the dual hypergraph (transpose of the primal incidence matrix)
            int[,] dualIncidenceMatrix = TransposeMatrix(incidenceMatrix);

            // Write the dual incidence matrix to the .dh file
            string dhFile = Path.ChangeExtension(qasmFile, ".dh");
            WriteIncidenceMatrix(dhFile, dualIncidenceMatrix);
        }

        /// <summary>
        /// Parses a .qasm file to extract the gates and the qubits they act upon.
        /// </summary>
        /// <param name="qasmFile">The path to the .qasm file.</param>
        /// <returns>A list of gates with their associated qubits.</returns>
        static List<Gate> ParseQasmFile(string qasmFile)
        {
            List<Gate> gates = new List<Gate>();

            // Regular expression to match gate operations
            // This regex matches lines like: gate_name q[0]; or gate_name q[0],q[1];
            Regex gateRegex = new Regex(@"^\s*(\w+)\s+(.+);", RegexOptions.Compiled);

            // Regular expression to match qubit identifiers
            // This regex matches q[0], q[1], etc.
            Regex qubitRegex = new Regex(@"q\[\d+\]", RegexOptions.Compiled);

            // Read the .qasm file line by line
            string[] lines = File.ReadAllLines(qasmFile);

            foreach (string line in lines)
            {
                // Ignore comments and empty lines
                string trimmedLine = line.Split(new[] { "//" }, StringSplitOptions.None)[0].Trim();
                if (string.IsNullOrEmpty(trimmedLine))
                {
                    continue;
                }

                // Skip lines that start with declarations or non-gate instructions
                if (trimmedLine.StartsWith("OPENQASM") || trimmedLine.StartsWith("include") || trimmedLine.StartsWith("qreg") ||
                    trimmedLine.StartsWith("creg") || trimmedLine.StartsWith("measure") || trimmedLine.StartsWith("barrier"))
                {
                    continue;
                }

                Match gateMatch = gateRegex.Match(trimmedLine);
                if (gateMatch.Success)
                {
                    string gateName = gateMatch.Groups[1].Value;
                    string qubitsStr = gateMatch.Groups[2].Value;

                    // Extract qubits
                    List<string> qubitList = new List<string>();

                    // Remove any parentheses (for parameterized gates)
                    qubitsStr = Regex.Replace(qubitsStr, @"\([^\)]*\)", "");

                    // Split qubits by comma
                    string[] qubits = qubitsStr.Split(',');

                    foreach (string qubit in qubits)
                    {
                        string q = qubit.Trim();
                        Match qubitMatch = qubitRegex.Match(q);
                        if (qubitMatch.Success)
                        {
                            qubitList.Add(qubitMatch.Value);
                        }
                        else
                        {
                            // Handle cases where qubit identifiers are not in the expected format
                            Console.WriteLine($"Warning: Unrecognized qubit identifier '{q}' in file '{qasmFile}'.");
                        }
                    }

                    if (qubitList.Count > 0)
                    {
                        gates.Add(new Gate
                        {
                            Name = gateName,
                            Qubits = qubitList
                        });
                    }
                }
            }

            return gates;
        }

        /// <summary>
        /// Writes the incidence matrix to a file, including a timestamp as the first line.
        /// </summary>
        /// <param name="filename">The output filename.</param>
        /// <param name="matrix">The incidence matrix to write.</param>
        static void WriteIncidenceMatrix(string filename, int[,] matrix)
        {
            using (StreamWriter writer = new StreamWriter(filename))
            {
                // Write the timestamp
                writer.WriteLine(DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss"));

                int rows = matrix.GetLength(0);
                int cols = matrix.GetLength(1);

                for (int i = 0; i < rows; i++)
                {
                    string line = "";
                    for (int j = 0; j < cols; j++)
                    {
                        line += matrix[i, j] + "\t";
                    }
                    writer.WriteLine(line.TrimEnd());
                }
            }
        }

        /// <summary>
        /// Transposes a 2D integer matrix.
        /// </summary>
        /// <param name="matrix">The matrix to transpose.</param>
        /// <returns>The transposed matrix.</returns>
        static int[,] TransposeMatrix(int[,] matrix)
        {
            int rows = matrix.GetLength(0);
            int cols = matrix.GetLength(1);

            int[,] transposed = new int[cols, rows];

            for (int i = 0; i < cols; i++)
            {
                for (int j = 0; j < rows; j++)
                {
                    transposed[i, j] = matrix[j, i];
                }
            }

            return transposed;
        }

        /// <summary>
        /// Represents a quantum gate and the qubits it acts upon.
        /// </summary>
        class Gate
        {
            public string Name { get; set; }
            public List<string> Qubits { get; set; }
        }
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace AdaptiveQuantumCircuitGenerator
{
    class Program
    {
        // Define the qubit counts for which to generate circuits
        static readonly int[] QubitCounts = { 4, 16, 24, 32, 40, 48 };

        static void Main(string[] args)
        {
            // Validate input arguments
            if (args.Length != 1)
            {
                Console.WriteLine("Usage: AdaptiveQuantumCircuitGenerator.exe <output_folder_path>");
                return;
            }

            string outputFolderPath = args[0];

            // Ensure the output directory exists; if not, create it
            if (!Directory.Exists(outputFolderPath))
            {
                try
                {
                    Directory.CreateDirectory(outputFolderPath);
                    Console.WriteLine($"Created directory: {outputFolderPath}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error creating directory '{outputFolderPath}': {ex.Message}");
                    return;
                }
            }

            // Iterate through each qubit count and generate corresponding QASM files
            foreach (var qubitCount in QubitCounts)
            {
                string fileName = $"adaptive_random_{qubitCount}.qasm";
                string filePath = Path.Combine(outputFolderPath, fileName);

                Console.WriteLine($"\nGenerating {fileName}...");

                try
                {
                    string qasmContent = GenerateQasmCode(qubitCount);
                    File.WriteAllText(filePath, qasmContent, Encoding.UTF8);
                    Console.WriteLine($"Successfully created {fileName}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error generating {fileName}: {ex.Message}");
                }
            }

            Console.WriteLine("\nAll files generated successfully.");
        }

        /// <summary>
        /// Generates QASM 3.0 code for an adaptive quantum circuit with the specified number of qubits.
        /// </summary>
        /// <param name="qubitCount">Number of qubits in the circuit.</param>
        /// <returns>String containing the QASM code.</returns>
        static string GenerateQasmCode(int qubitCount)
        {
            // Calculate the number of classical bits (for measurements and controls)
            // For simplicity, let's define classical bits as half the number of qubits, rounded up
            int classicalBitCount = (int)Math.Ceiling(qubitCount / 2.0);

            // Initialize a StringBuilder to construct the QASM code
            StringBuilder qasm = new StringBuilder();

            // QASM Headers
            qasm.AppendLine("OPENQASM 3.0;");
            qasm.AppendLine("include \"qelib1.inc\";");
            qasm.AppendLine();

            // Define qubits and classical bits
            qasm.AppendLine($"qreg q[{qubitCount}];");
            qasm.AppendLine($"creg c[{classicalBitCount}];");
            qasm.AppendLine();

            // Apply Hadamard gates to all qubits
            for (int i = 0; i < qubitCount; i++)
            {
                qasm.AppendLine($"h q[{i}];");
            }
            qasm.AppendLine();

            // Apply X gates to a subset of qubits
            // For diversity, let's apply X gates to qubits at even indices
            for (int i = 0; i < qubitCount; i += 2)
            {
                qasm.AppendLine($"x q[{i}];");
            }
            qasm.AppendLine();

            // Apply CX (CNOT) gates between qubits
            // For diversity, connect qubits in pairs: q[0]-q[1], q[2]-q[3], etc.
            for (int i = 0; i < qubitCount - 1; i += 2)
            {
                qasm.AppendLine($"cx q[{i}], q[{i + 1}];");
            }
            qasm.AppendLine();

            // Insert an IF statement
            // Select a classical bit to control the IF
            int ifClassicalBit = classicalBitCount > 0 ? 0 : 0; // Use c[0] if exists
            qasm.AppendLine($"if (c[{ifClassicalBit}]) {{");
            qasm.AppendLine($"    // Operations within IF");
            qasm.AppendLine($"    cx q[0], q[1];");
            qasm.AppendLine($"    while (c[{ifClassicalBit}] < 2) {{");
            qasm.AppendLine($"        x q[2];");
            qasm.AppendLine($"    }}");
            qasm.AppendLine($"}} else {{");
            qasm.AppendLine($"    // Operations within ELSE");
            qasm.AppendLine($"    z q[1];");
            qasm.AppendLine($"}}");
            qasm.AppendLine();

            // Insert a FOR loop
            qasm.AppendLine($"for (int i = 0; i < {qubitCount}; i++) {{");
            qasm.AppendLine($"    h q[i];");
            qasm.AppendLine($"}}");
            qasm.AppendLine();

            // Insert a nested control structure (e.g., IF within FOR)
            qasm.AppendLine($"for (int i = 0; i < {qubitCount}; i++) {{");
            qasm.AppendLine($"    if (c[{ifClassicalBit}] == i % {classicalBitCount}) {{");
            qasm.AppendLine($"        x q[i];");
            qasm.AppendLine($"    }}");
            qasm.AppendLine($"}}");
            qasm.AppendLine();

            // Measurements
            for (int i = 0; i < qubitCount; i++)
            {
                // Assign measurements to classical bits in a round-robin fashion
                int classicalBit = i % classicalBitCount;
                qasm.AppendLine($"measure q[{i}] -> c[{classicalBit}];");
            }
            qasm.AppendLine();

            // Conditional operations based on measurements
            qasm.AppendLine($"if (c[{ifClassicalBit}] == 1) {{");
            qasm.AppendLine($"    y q[0];");
            qasm.AppendLine($"}}");
            qasm.AppendLine();

            return qasm.ToString();
        }
    }
}

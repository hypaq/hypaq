using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using QasmToHypergraph;
using QasmToHypergraph.Models;

namespace QasmToHypergraph
{
    class Program
    {
        static void Main(string[] args)
        {
            // Validate input arguments
            if (args.Length != 1)
            {
                Console.WriteLine("Usage: QasmToHypergraph.exe <path_to_qasm_file>");
                return;
            }

            string qasmFilePath = args[0];

            // Check if file exists
            if (!File.Exists(qasmFilePath))
            {
                Console.WriteLine($"Error: File '{qasmFilePath}' does not exist.");
                return;
            }

            // Initialize hypergraph components
            Hypergraph hypergraph = new Hypergraph();
            Stack<HypergraphGroup> controlGroupStack = new Stack<HypergraphGroup>();
            int gateCounter = 0;
            int controlCounter = 0;
            int timestamp = 0;
            Hyperedge lastProcessedEdge = null;

            // Define regular expressions for parsing
            Regex qregRegex = new Regex(@"qreg\s+(\w+)\s*\[\s*(\d+)\s*\]\s*;");
            Regex cregRegex = new Regex(@"creg\s+(\w+)\s*\[\s*(\d+)\s*\]\s*;");
            Regex gateRegex = new Regex(@"(\w+)\s*(?:\([^)]*\))?\s+((?:q\[\d+\]\s*,\s*)*q\[\d+\])\s*;");
            Regex ifRegex = new Regex(@"if\s*\(");
            Regex elseRegex = new Regex(@"else\b");
            Regex whileRegex = new Regex(@"while\s*\(");
            Regex forRegex = new Regex(@"for\s*\(");
            Regex switchRegex = new Regex(@"switch\s*\(");
            Regex closingBraceRegex = new Regex(@"}");
            Regex measureRegex = new Regex(@"measure\s+(\w+\[\d+\])\s*->\s*(\w+\[\d+\]);");

            try
            {
                // Read all lines from the QASM file
                string[] lines = File.ReadAllLines(qasmFilePath);
                int lineNumber = 0;

                foreach (string originalLine in lines)
                {
                    lineNumber++;
                    string line = originalLine.Trim();

                    // Skip empty lines and comments
                    if (string.IsNullOrEmpty(line) || line.StartsWith("//") || line.StartsWith("#"))
                        continue;

                    // Handle qreg declarations
                    Match qregMatch = qregRegex.Match(line);
                    if (qregMatch.Success)
                    {
                        string registerName = qregMatch.Groups[1].Value;
                        int size = int.Parse(qregMatch.Groups[2].Value);

                        for (int i = 0; i < size; i++)
                        {
                            string qubitId = $"{registerName}[{i}]";
                            hypergraph.AddVertex(qubitId);
                        }
                        continue;
                    }

                    // Handle creg declarations
                    Match cregMatch = cregRegex.Match(line);
                    if (cregMatch.Success)
                    {
                        string registerName = cregMatch.Groups[1].Value;
                        int size = int.Parse(cregMatch.Groups[2].Value);

                        for (int i = 0; i < size; i++)
                        {
                            string cregId = $"{registerName}[{i}]";
                            hypergraph.AddVertex(cregId);
                        }
                        continue;
                    }

                    // Handle measure operations
                    Match measureMatch = measureRegex.Match(line);
                    if (measureMatch.Success)
                    {
                        string qubit = measureMatch.Groups[1].Value;
                        string creg = measureMatch.Groups[2].Value;

                        // Ensure qubit and creg are added as vertices
                        hypergraph.AddVertex(qubit);
                        hypergraph.AddVertex(creg);

                        // Create a hyperedge for measurement
                        string hyperedgeId = $"measure_{gateCounter++}";
                        List<string> connectedVertices = new List<string> { qubit, creg };
                        Hyperedge measureEdge = new Hyperedge(hyperedgeId, connectedVertices, "measure", timestamp++);
                        hypergraph.AddHyperedge(measureEdge);

                        // Handle temporal dependency
                        if (lastProcessedEdge != null)
                        {
                            hypergraph.Dependencies.Add(new Dependency(lastProcessedEdge.Id, measureEdge.Id));
                        }
                        lastProcessedEdge = measureEdge;

                        continue;
                    }

                    // Handle gate operations
                    Match gateMatch = gateRegex.Match(line);
                    if (gateMatch.Success)
                    {
                        string gateType = gateMatch.Groups[1].Value;
                        string qubitsList = gateMatch.Groups[2].Value;

                        // Split qubits by comma and trim spaces
                        List<string> qubits = new List<string>();
                        foreach (string qb in qubitsList.Split(','))
                        {
                            string qubit = qb.Trim();
                            qubits.Add(qubit);
                            hypergraph.AddVertex(qubit); // Ensure qubit is added
                        }

                        if (qubits.Count > 0)
                        {
                            // Create a unique hyperedge ID
                            string hyperedgeId = $"{gateType}_{gateCounter++}";

                            Hyperedge edge = new Hyperedge(hyperedgeId, qubits, "gate", timestamp++);

                            if (controlGroupStack.Count > 0)
                            {
                                HypergraphGroup currentGroup = controlGroupStack.Peek();
                                currentGroup.Hyperedges.Add(edge);
                            }
                            else
                            {
                                hypergraph.AddHyperedge(edge);
                            }

                            // Handle temporal dependency
                            if (lastProcessedEdge != null)
                            {
                                hypergraph.Dependencies.Add(new Dependency(lastProcessedEdge.Id, edge.Id));
                            }
                            lastProcessedEdge = edge;
                        }
                        continue;
                    }

                    // Handle IF constructs
                    if (ifRegex.IsMatch(line))
                    {
                        // Create a control node for IF condition
                        string controlId = $"c_IF_{controlCounter}";
                        hypergraph.AddVertex(controlId);

                        // Create a hyperedge for IF control
                        string hyperedgeId = $"control_IF_{controlCounter}";
                        // Connect to all cregs (c[0], c[1], etc.)
                        List<string> connectedVertices = new List<string>();
                        foreach (var vertex in hypergraph.Vertices)
                        {
                            if (vertex.Id.StartsWith("c["))
                            {
                                connectedVertices.Add(vertex.Id);
                            }
                        }
                        connectedVertices.Add(controlId);

                        Hyperedge controlEdge = new Hyperedge(hyperedgeId, connectedVertices, "control_IF", timestamp++);
                        hypergraph.AddHyperedge(controlEdge);

                        // Create groups for IF_TRUE and IF_FALSE
                        HypergraphGroup G_IF_TRUE = new HypergraphGroup($"IF_TRUE_{controlCounter}");
                        HypergraphGroup G_IF_FALSE = new HypergraphGroup($"IF_FALSE_{controlCounter}");
                        hypergraph.AddGroup(G_IF_TRUE);
                        hypergraph.AddGroup(G_IF_FALSE);

                        // Push IF_TRUE group onto the stack
                        controlGroupStack.Push(G_IF_TRUE);

                        // Handle temporal dependency
                        if (lastProcessedEdge != null)
                        {
                            hypergraph.Dependencies.Add(new Dependency(lastProcessedEdge.Id, controlEdge.Id));
                        }
                        lastProcessedEdge = controlEdge;

                        controlCounter++;
                        continue;
                    }

                    // Handle ELSE constructs
                    if (elseRegex.IsMatch(line))
                    {
                        if (controlGroupStack.Count > 0)
                        {
                            // Pop the IF_TRUE group
                            HypergraphGroup G_IF_TRUE = controlGroupStack.Pop();

                            // Find the corresponding IF_FALSE group
                            string ifFalseId = G_IF_TRUE.Id.Replace("IF_TRUE", "IF_FALSE");
                            HypergraphGroup G_IF_FALSE = hypergraph.Groups.Find(g => g.Id == ifFalseId);

                            if (G_IF_FALSE != null)
                            {
                                // Create a hyperedge for ELSE control
                                string hyperedgeId = $"control_ELSE_{G_IF_FALSE.Id.Split('_')[2]}";
                                List<string> connectedVertices = new List<string>();
                                foreach (var vertex in hypergraph.Vertices)
                                {
                                    if (vertex.Id.StartsWith("c["))
                                    {
                                        connectedVertices.Add(vertex.Id);
                                    }
                                }
                                // Assuming the ELSE condition connects to the same control node as IF
                                string correspondingControlId = $"c_IF_{G_IF_FALSE.Id.Split('_')[2]}";
                                connectedVertices.Add(correspondingControlId);

                                Hyperedge elseControlEdge = new Hyperedge(hyperedgeId, connectedVertices, "control_ELSE", timestamp++);
                                hypergraph.AddHyperedge(elseControlEdge);

                                // Handle temporal dependency
                                if (lastProcessedEdge != null)
                                {
                                    hypergraph.Dependencies.Add(new Dependency(lastProcessedEdge.Id, elseControlEdge.Id));
                                }
                                lastProcessedEdge = elseControlEdge;

                                // Push IF_FALSE group onto the stack
                                controlGroupStack.Push(G_IF_FALSE);
                            }
                            else
                            {
                                Console.WriteLine($"Warning: ELSE without matching IF_FALSE group at line {lineNumber}.");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Warning: ELSE without matching IF_TRUE group at line {lineNumber}.");
                        }
                        continue;
                    }

                    // Handle WHILE loops
                    if (whileRegex.IsMatch(line))
                    {
                        // Create a control node for WHILE condition
                        string controlId = $"c_WHILE_{controlCounter}";
                        hypergraph.AddVertex(controlId);

                        // Create a hyperedge for WHILE control
                        string hyperedgeId = $"control_WHILE_{controlCounter}";
                        // Connect to all cregs (c[0], c[1], etc.)
                        List<string> connectedVertices = new List<string>();
                        foreach (var vertex in hypergraph.Vertices)
                        {
                            if (vertex.Id.StartsWith("c["))
                            {
                                connectedVertices.Add(vertex.Id);
                            }
                        }
                        connectedVertices.Add(controlId);

                        Hyperedge controlEdge = new Hyperedge(hyperedgeId, connectedVertices, "control_WHILE", timestamp++);
                        hypergraph.AddHyperedge(controlEdge);

                        // Create group for WHILE_LOOP
                        HypergraphGroup G_WHILE_LOOP = new HypergraphGroup($"WHILE_LOOP_{controlCounter}");
                        hypergraph.AddGroup(G_WHILE_LOOP);

                        // Push WHILE_LOOP group onto the stack
                        controlGroupStack.Push(G_WHILE_LOOP);

                        // Handle temporal dependency
                        if (lastProcessedEdge != null)
                        {
                            hypergraph.Dependencies.Add(new Dependency(lastProcessedEdge.Id, controlEdge.Id));
                        }
                        lastProcessedEdge = controlEdge;

                        controlCounter++;
                        continue;
                    }

                    // Handle FOR loops
                    if (forRegex.IsMatch(line))
                    {
                        // Create a control node for FOR loop
                        string controlId = $"c_FOR_{controlCounter}";
                        hypergraph.AddVertex(controlId);

                        // Create a hyperedge for FOR control
                        string hyperedgeId = $"control_FOR_{controlCounter}";
                        // Connect to all cregs (c[0], c[1], etc.)
                        List<string> connectedVertices = new List<string>();
                        foreach (var vertex in hypergraph.Vertices)
                        {
                            if (vertex.Id.StartsWith("c["))
                            {
                                connectedVertices.Add(vertex.Id);
                            }
                        }
                        connectedVertices.Add(controlId);

                        Hyperedge controlEdge = new Hyperedge(hyperedgeId, connectedVertices, "control_FOR", timestamp++);
                        hypergraph.AddHyperedge(controlEdge);

                        // Create group for FOR_LOOP
                        HypergraphGroup G_FOR_LOOP = new HypergraphGroup($"FOR_LOOP_{controlCounter}");
                        hypergraph.AddGroup(G_FOR_LOOP);

                        // Push FOR_LOOP group onto the stack
                        controlGroupStack.Push(G_FOR_LOOP);

                        // Handle temporal dependency
                        if (lastProcessedEdge != null)
                        {
                            hypergraph.Dependencies.Add(new Dependency(lastProcessedEdge.Id, controlEdge.Id));
                        }
                        lastProcessedEdge = controlEdge;

                        controlCounter++;
                        continue;
                    }

                    // Handle SWITCH statements
                    if (switchRegex.IsMatch(line))
                    {
                        // Create a control node for SWITCH condition
                        string controlId = $"c_SWITCH_{controlCounter}";
                        hypergraph.AddVertex(controlId);

                        // Create a hyperedge for SWITCH control
                        string hyperedgeId = $"control_SWITCH_{controlCounter}";
                        // Connect to all cregs (c[0], c[1], etc.)
                        List<string> connectedVertices = new List<string>();
                        foreach (var vertex in hypergraph.Vertices)
                        {
                            if (vertex.Id.StartsWith("c["))
                            {
                                connectedVertices.Add(vertex.Id);
                            }
                        }
                        connectedVertices.Add(controlId);

                        Hyperedge controlEdge = new Hyperedge(hyperedgeId, connectedVertices, "control_SWITCH", timestamp++);
                        hypergraph.AddHyperedge(controlEdge);

                        // Create group for SWITCH_CASES
                        HypergraphGroup G_SWITCH_CASES = new HypergraphGroup($"SWITCH_CASES_{controlCounter}");
                        hypergraph.AddGroup(G_SWITCH_CASES);

                        // Push SWITCH_CASES group onto the stack
                        controlGroupStack.Push(G_SWITCH_CASES);

                        // Handle temporal dependency
                        if (lastProcessedEdge != null)
                        {
                            hypergraph.Dependencies.Add(new Dependency(lastProcessedEdge.Id, controlEdge.Id));
                        }
                        lastProcessedEdge = controlEdge;

                        controlCounter++;
                        continue;
                    }

                    // Handle closing braces
                    if (closingBraceRegex.IsMatch(line))
                    {
                        if (controlGroupStack.Count > 0)
                        {
                            controlGroupStack.Pop();
                        }
                        else
                        {
                            Console.WriteLine($"Warning: Unmatched closing brace '}}' at line {lineNumber}.");
                        }
                        continue;
                    }

                    // Additional parsing for other instructions can be added here

                    // If line doesn't match any pattern, continue
                }

                // Prepare the output .hype file path
                string hypeFilePath = Path.ChangeExtension(qasmFilePath, ".hype");

                // Write the hypergraph to the .hype file
                using (StreamWriter writer = new StreamWriter(hypeFilePath))
                {
                    // Write Vertices
                    writer.WriteLine("# Vertices");
                    foreach (var vertex in hypergraph.Vertices)
                    {
                        writer.WriteLine(vertex.Id);
                    }
                    writer.WriteLine();

                    // Write Hyperedges
                    writer.WriteLine("# Hyperedges");
                    foreach (var edge in hypergraph.Hyperedges)
                    {
                        writer.WriteLine($"{edge.Id} " + string.Join(" ", edge.ConnectedVertices));
                    }
                    writer.WriteLine();

                    // Write Groups
                    writer.WriteLine("# Groups");
                    foreach (var group in hypergraph.Groups)
                    {
                        WriteGroup(writer, group, 0);
                    }
                    writer.WriteLine();

                    // Write Dependencies
                    writer.WriteLine("# Dependencies");
                    foreach (var dep in hypergraph.Dependencies)
                    {
                        writer.WriteLine($"{dep.From} -> {dep.To}");
                    }
                }

                Console.WriteLine($"Hypergraph has been successfully written to '{hypeFilePath}'.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }

        // Recursively writes groups and their subgroups with indentation
        static void WriteGroup(StreamWriter writer, HypergraphGroup group, int indentLevel)
        {
            string indent = new string(' ', indentLevel * 4);
            writer.WriteLine($"{indent}Group: {group.Id}");

            // Write Hyperedges in the group
            if (group.Hyperedges.Count > 0)
            {
                writer.WriteLine($"{indent}    Hyperedges:");
                foreach (var edge in group.Hyperedges)
                {
                    writer.WriteLine($"{indent}        {edge.Id} " + string.Join(" ", edge.ConnectedVertices));
                }
            }

            // Recursively write subgroups
            if (group.Subgroups.Count > 0)
            {
                writer.WriteLine($"{indent}    Subgroups:");
                foreach (var subgroup in group.Subgroups)
                {
                    WriteGroup(writer, subgroup, indentLevel + 2);
                }
            }
        }
    }
}

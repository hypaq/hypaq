using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using HypeToIncidenceMatrix.Models;

namespace HypeToIncidenceMatrix
{
    class Program
    {
        static void Main(string[] args)
        {
            // Validate input arguments
            if (args.Length != 1)
            {
                Console.WriteLine("Usage: HypeToIncidenceMatrix.exe <path_to_hype_file>");
                return;
            }

            string hypeFilePath = args[0];

            // Check if file exists
            if (!File.Exists(hypeFilePath))
            {
                Console.WriteLine($"Error: File '{hypeFilePath}' does not exist.");
                return;
            }

            // Initialize hypergraph components
            Hypergraph hypergraph = new Hypergraph();

            // Define regular expressions for parsing
            // Updated regex to allow vertices with or without indices
            Regex hyperedgePattern = new Regex(@"^(?<id>\w+)\s+(?<vertices>(?:\w+(?:\[\d+\])?\s*)+)$");
            Regex groupStartPattern = new Regex(@"^Group:\s*(?<groupName>\w+)");
            Regex groupHyperedgePattern = new Regex(@"^\s*(?<id>\w+)\s+(?<vertices>(?:\w+(?:\[\d+\])?\s*)+)$");

            try
            {
                // Read all lines from the .hype file
                string[] lines = File.ReadAllLines(hypeFilePath);
                string currentSection = "";
                string currentGroup = "";
                bool isInGroupHyperedges = false;
                int lineNumber = 0;

                foreach (string originalLine in lines)
                {
                    lineNumber++;
                    string line = originalLine.Trim();

                    // Skip empty lines and comments
                    if (string.IsNullOrEmpty(line) || line.StartsWith("//"))
                        continue;

                    // Detect section headers
                    if (line.StartsWith("#"))
                    {
                        if (line.Contains("Vertices"))
                            currentSection = "Vertices";
                        else if (line.Contains("Hyperedges"))
                            currentSection = "Hyperedges";
                        else if (line.Contains("Groups"))
                            currentSection = "Groups";
                        else
                            currentSection = "";
                        continue;
                    }

                    // Process based on current section
                    switch (currentSection)
                    {
                        case "Vertices":
                            {
                                hypergraph.AddVertex(line);
                                break;
                            }
                        case "Hyperedges":
                            {
                                Match match = hyperedgePattern.Match(line);
                                if (match.Success)
                                {
                                    string hyperedgeId = match.Groups["id"].Value;
                                    string verticesStr = match.Groups["vertices"].Value;
                                    List<string> connectedVertices = new List<string>();

                                    foreach (string vertex in verticesStr.Split(' ', StringSplitOptions.RemoveEmptyEntries))
                                    {
                                        string trimmedVertex = vertex.Trim();
                                        connectedVertices.Add(trimmedVertex);
                                        // Optionally, ensure vertex exists in hypergraph
                                        if (!hypergraph.Vertices.Contains(new Vertex(trimmedVertex)))
                                        {
                                            Console.WriteLine($"Warning: Vertex '{trimmedVertex}' referenced in hyperedge '{hyperedgeId}' not found in Vertices section (line {lineNumber}).");
                                        }
                                    }

                                    Hyperedge hyperedge = new Hyperedge(hyperedgeId, connectedVertices, "hyperedge", lineNumber);
                                    hypergraph.AddHyperedge(hyperedge);
                                    Console.WriteLine($"Parsed Hyperedge: {hyperedgeId} connected to [{string.Join(", ", connectedVertices)}]");
                                }
                                else
                                {
                                    Console.WriteLine($"Warning: Unable to parse hyperedge at line {lineNumber}: '{line}'");
                                }
                                break;
                            }
                        case "Groups":
                            {
                                // Detect group start
                                Match groupMatch = groupStartPattern.Match(line);
                                if (groupMatch.Success)
                                {
                                    currentGroup = groupMatch.Groups["groupName"].Value;
                                    isInGroupHyperedges = false; // Reset flag for new group
                                    Console.WriteLine($"Detected Group: {currentGroup}");
                                    continue;
                                }

                                // Detect 'Hyperedges:' line within a group
                                if (line.StartsWith("Hyperedges:"))
                                {
                                    isInGroupHyperedges = true;
                                    Console.WriteLine($"Entering Hyperedges of Group: {currentGroup}");
                                    continue;
                                }

                                // If currently parsing group hyperedges
                                if (isInGroupHyperedges && !line.StartsWith("Group:"))
                                {
                                    Match groupHyperedgeMatch = groupHyperedgePattern.Match(originalLine);
                                    if (groupHyperedgeMatch.Success)
                                    {
                                        string hyperedgeId = groupHyperedgeMatch.Groups["id"].Value;
                                        string verticesStr = groupHyperedgeMatch.Groups["vertices"].Value;
                                        List<string> connectedVertices = new List<string>();

                                        foreach (string vertex in verticesStr.Split(' ', StringSplitOptions.RemoveEmptyEntries))
                                        {
                                            string trimmedVertex = vertex.Trim();
                                            connectedVertices.Add(trimmedVertex);
                                            // Optionally, ensure vertex exists in hypergraph
                                            if (!hypergraph.Vertices.Contains(new Vertex(trimmedVertex)))
                                            {
                                                Console.WriteLine($"Warning: Vertex '{trimmedVertex}' referenced in group '{currentGroup}' hyperedge '{hyperedgeId}' not found in Vertices section (line {lineNumber}).");
                                            }
                                        }

                                        Hyperedge hyperedge = new Hyperedge(hyperedgeId, connectedVertices, $"group_{currentGroup}", lineNumber);
                                        hypergraph.AddHyperedge(hyperedge);
                                        Console.WriteLine($"Parsed Group Hyperedge: {hyperedgeId} connected to [{string.Join(", ", connectedVertices)}]");
                                    }
                                    else
                                    {
                                        Console.WriteLine($"Warning: Unable to parse group hyperedge at line {lineNumber}: '{originalLine}'");
                                    }
                                }

                                break;
                            }
                        default:
                            {
                                // Ignore other sections like Dependencies
                                break;
                            }
                    }
                }

                // Assign indices to Vertices and Hyperedges
                List<Vertex> vertexList = new List<Vertex>(hypergraph.Vertices);
                vertexList.Sort((v1, v2) => v1.Id.CompareTo(v2.Id)); // Optional: sort for consistency

                List<Hyperedge> hyperedgeList = new List<Hyperedge>(hypergraph.Hyperedges);
                hyperedgeList.Sort((h1, h2) => h1.Timestamp.CompareTo(h2.Timestamp)); // Sort by appearance

                // Create mappings
                Dictionary<string, int> vertexIndices = new Dictionary<string, int>();
                for (int i = 0; i < vertexList.Count; i++)
                {
                    vertexIndices[vertexList[i].Id] = i;
                }

                Dictionary<string, int> hyperedgeIndices = new Dictionary<string, int>();
                for (int j = 0; j < hyperedgeList.Count; j++)
                {
                    hyperedgeIndices[hyperedgeList[j].Id] = j;
                }

                // Initialize incidence matrix
                int[,] incidenceMatrix = new int[vertexList.Count, hyperedgeList.Count];

                // Populate incidence matrix
                foreach (var hyperedge in hyperedgeList)
                {
                    if (hyperedgeIndices.TryGetValue(hyperedge.Id, out int col))
                    {
                        foreach (var vertexId in hyperedge.ConnectedVertices)
                        {
                            if (vertexIndices.TryGetValue(vertexId, out int row))
                            {
                                incidenceMatrix[row, col] = 1;
                            }
                            else
                            {
                                Console.WriteLine($"Warning: Vertex '{vertexId}' in hyperedge '{hyperedge.Id}' not found in Vertices list.");
                            }
                        }
                    }
                }

                // Prepare the output .h file path
                string hFilePath = Path.ChangeExtension(hypeFilePath, ".h");

                // Write the incidence matrix to the .h file
                using (StreamWriter writer = new StreamWriter(hFilePath))
                {
                    // Write header row with hyperedge IDs
                    writer.Write("Vertex\t");
                    for (int j = 0; j < hyperedgeList.Count; j++)
                    {
                        writer.Write($"{hyperedgeList[j].Id}\t");
                    }
                    writer.WriteLine();

                    // Write each vertex row
                    for (int i = 0; i < vertexList.Count; i++)
                    {
                        writer.Write($"{vertexList[i].Id}\t");
                        for (int j = 0; j < hyperedgeList.Count; j++)
                        {
                            writer.Write($"{incidenceMatrix[i, j]}\t");
                        }
                        writer.WriteLine();
                    }
                }

                // Debugging Information
                Console.WriteLine($"\nParsing completed successfully.");
                Console.WriteLine($"Total Vertices: {vertexList.Count}");
                Console.WriteLine($"Total Hyperedges: {hyperedgeList.Count}");
                Console.WriteLine($"Incidence matrix has been successfully written to '{hFilePath}'.\n");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred during transformation: {ex.Message}");
            }
        }
    }
}

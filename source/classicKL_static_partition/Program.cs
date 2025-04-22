using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace KLGraphPartitioning
{
    /// <summary>
    /// Program implementing the Kernighan-Lin graph partitioning algorithm.
    /// Processes .h files from directories listed in an input file.
    /// </summary>
    class Program
    {
        /// <summary>
        /// Class to represent a node in the graph.
        /// </summary>
        class Node
        {
            public int nodeid;           // Identifier of the node
            public int partition;        // Partition of the node (0 or 1)
            public int gain;             // Gain value of the node
            public bool locked;          // Lock status of the node
        }

        // Global variables
        static int[,] adjacencyMatrix;          // Graph adjacency matrix
        static int vertexCount;                 // Number of vertices
        static int hyperedgeCount;              // Number of hyperedges (calculated from incidence matrix)
        static List<Node> nodes;                // List of nodes
        static int[] partitionSize = new int[2]; // Size of each partition
        static int totalCost;                   // Total cost (cut size)
        static List<Tuple<int, int>> swaps;     // List of swaps performed
        static List<int> cumulativeGains;       // Cumulative gains after each swap

        /// <summary>
        /// Main function implementing the Kernighan-Lin algorithm.
        /// Processes .h files from directories listed in an input file.
        /// </summary>
        /// <param name="args">Command-line arguments.</param>
        static void Main(string[] args)
        {
            if (args.Length >= 1)
            {
                string listFilename = args[0];

                // Read the list of directory paths
                List<string> directories = new List<string>();
                try
                {
                    using (StreamReader sr = new StreamReader(listFilename))
                    {
                        string line;
                        while ((line = sr.ReadLine()) != null)
                        {
                            if (!string.IsNullOrWhiteSpace(line))
                            {
                                directories.Add(line.Trim());
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("Error: Unable to open the list file.");
                    Console.WriteLine(e.Message);
                    Environment.Exit(1);
                }

                // Process each directory
                foreach (string directory in directories)
                {
                    if (Directory.Exists(directory))
                    {
                        // Get all .h files in the directory
                        string[] hFiles = Directory.GetFiles(directory, "*.h");

                        foreach (string hFile in hFiles)
                        {
                            ProcessGraph(hFile);
                        }
                    }
                    else
                    {
                        Console.WriteLine($"Error: Directory '{directory}' does not exist.");
                    }
                }
            }
            else
            {
                Console.WriteLine("Usage: Program.exe <list_of_directories>");
                Environment.Exit(1);
            }
        }

        /// <summary>
        /// Processes a single graph file using the Kernighan-Lin algorithm.
        /// Creates a log file with the same name but with a .log extension in the same directory.
        /// </summary>
        /// <param name="graphFile">The graph file to process.</param>
        static void ProcessGraph(string graphFile)
        {
            // Create the log file name in the same directory
            string logFile = Path.ChangeExtension(graphFile, ".log");

            // Open the log file for writing
            try
            {
                using (StreamWriter logWriter = new StreamWriter(logFile))
                {
                    // Redirect Console output to the log file
                    TextWriter originalConsoleOut = Console.Out;
                    Console.SetOut(logWriter);

                    // Start timing
                    DateTime startTime = DateTime.Now;
                    Console.WriteLine("Start Time: " + startTime.ToString("yyyy-MM-dd HH:mm:ss.fff"));

                    // Initialize data structures
                    Init();

                    // Read the graph from the file
                    if (!InputFromFile(graphFile))
                    {
                        // If reading failed, restore Console output and continue to next file
                        Console.SetOut(originalConsoleOut);
                        return;
                    }

                    // Proceed with the Kernighan-Lin algorithm
                    InitialPartition();
                    totalCost = CalculateCutSize();
                    Console.WriteLine("Initial Partition:");
                    PrintPartition();
                    Console.WriteLine("Initial Cut Size: " + totalCost);
                    KernighanLinAlgorithm();

                    // Finish timing
                    DateTime endTime = DateTime.Now;
                    Console.WriteLine("Finish Time: " + endTime.ToString("yyyy-MM-dd HH:mm:ss.fff"));

                    // Calculate total time spent
                    TimeSpan totalTime = endTime - startTime;
                    Console.WriteLine("Total Time Spent: " + totalTime.TotalMilliseconds + " ms");

                    // Restore Console output
                    Console.SetOut(originalConsoleOut);

                    // Optionally, print a message to the console indicating completion
                    Console.WriteLine($"Processed graph file '{graphFile}', log saved to '{logFile}'.");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error: Unable to create or write to log file '{logFile}'.");
                Console.WriteLine(e.Message);
                // No need to restore Console output here as exception handling takes care of that
            }
        }

        /// <summary>
        /// Initializes all data structures and variables.
        /// </summary>
        static void Init()
        {
            adjacencyMatrix = null;
            vertexCount = 0;
            hyperedgeCount = 0;
            nodes = new List<Node>();
            partitionSize[0] = 0;
            partitionSize[1] = 0;
            totalCost = 0;
            swaps = new List<Tuple<int, int>>();
            cumulativeGains = new List<int>();
        }

        /// <summary>
        /// Reads the graph from an incidence matrix in the input file, ignoring the first line (timestamp),
        /// constructs the adjacency matrix, and calculates the number of vertices and hyperedges.
        /// </summary>
        /// <param name="filename">Name of the input file.</param>
        /// <returns>True if reading was successful; otherwise, false.</returns>
        static bool InputFromFile(string filename)
        {
            try
            {
                using (StreamReader sr = new StreamReader(filename))
                {
                    // Ignore the first line (timestamp)
                    var line = sr.ReadLine();

                    List<List<int>> incidenceMatrix = new List<List<int>>();

                    // Read the incidence matrix
                    while ((line = sr.ReadLine()) != null)
                    {
                        line = line.Trim();
                        if (string.IsNullOrEmpty(line))
                        {
                            continue; // Skip empty lines
                        }

                        var tokens = line.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);

                        List<int> row = new List<int>();
                        foreach (var token in tokens)
                        {
                            int value = int.Parse(token);
                            if (value != 0 && value != 1)
                            {
                                Console.WriteLine("Error: Incidence matrix must contain only 0 or 1.");
                                return false;
                            }
                            row.Add(value);
                        }
                        incidenceMatrix.Add(row);
                    }

                    // Determine the number of vertices
                    if (incidenceMatrix.Count == 0)
                    {
                        Console.WriteLine("Error: Incidence matrix is empty.");
                        return false;
                    }

                    int vertexCountFromIncidence = incidenceMatrix[0].Count;

                    // Verify that all rows have the same number of columns
                    foreach (var row in incidenceMatrix)
                    {
                        if (row.Count != vertexCountFromIncidence)
                        {
                            Console.WriteLine("Error: Inconsistent number of vertices in incidence matrix rows.");
                            return false;
                        }
                    }

                    vertexCount = vertexCountFromIncidence;
                    hyperedgeCount = incidenceMatrix.Count; // Number of hyperedges

                    // Initialize the adjacency matrix
                    adjacencyMatrix = new int[vertexCount, vertexCount];

                    // Construct the adjacency matrix from the incidence matrix
                    foreach (var edgeRow in incidenceMatrix)
                    {
                        List<int> verticesInEdge = new List<int>();
                        for (int i = 0; i < edgeRow.Count; i++)
                        {
                            if (edgeRow[i] == 1)
                            {
                                verticesInEdge.Add(i);
                            }
                        }

                        // For all pairs of vertices in this edge, set adjacency
                        for (int i = 0; i < verticesInEdge.Count; i++)
                        {
                            for (int j = i + 1; j < verticesInEdge.Count; j++)
                            {
                                int u = verticesInEdge[i];
                                int v = verticesInEdge[j];
                                adjacencyMatrix[u, v] = 1;
                                adjacencyMatrix[v, u] = 1; // Since the graph is undirected
                            }
                        }
                    }

                    // Initialize nodes
                    for (int i = 0; i < vertexCount; i++)
                    {
                        nodes.Add(new Node
                        {
                            nodeid = i,
                            partition = 0, // Will be set during initial partitioning
                            gain = 0,
                            locked = false
                        });
                    }

                    // Optionally, print the graph information
                    Console.WriteLine("Processing file: " + filename);
                    Console.WriteLine("Number of vertices: " + vertexCount);
                    Console.WriteLine("Number of hyperedges: " + hyperedgeCount); // Added line
                }
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine("Error: Unable to read graph file '" + filename + "'.");
                Console.WriteLine(e.Message);
                return false;
            }
        }

        /// <summary>
        /// Performs an initial partitioning of the nodes.
        /// </summary>
        static void InitialPartition()
        {
            partitionSize[0] = 0;
            partitionSize[1] = 0;

            for (int i = 0; i < vertexCount; i++)
            {
                nodes[i].partition = i % 2; // Simple initial partition
                partitionSize[nodes[i].partition]++;
            }
        }

        /// <summary>
        /// Calculates the cost (cut size) of the current partition.
        /// </summary>
        /// <returns>The total cut size.</returns>
        static int CalculateCutSize()
        {
            int cutSize = 0;
            for (int i = 0; i < vertexCount; i++)
            {
                for (int j = i + 1; j < vertexCount; j++)
                {
                    if (adjacencyMatrix[i, j] == 1 &&
                        nodes[i].partition != nodes[j].partition)
                    {
                        cutSize++;
                    }
                }
            }
            return cutSize;
        }

        /// <summary>
        /// Prints the current partitioning of nodes.
        /// </summary>
        static void PrintPartition()
        {
            Console.Write("Partition A: {");
            foreach (var node in nodes)
            {
                if (node.partition == 0)
                {
                    Console.Write((char)('a' + node.nodeid) + " ");
                }
            }
            Console.WriteLine("}");
            Console.Write("Partition B: {");
            foreach (var node in nodes)
            {
                if (node.partition == 1)
                {
                    Console.Write((char)('a' + node.nodeid) + " ");
                }
            }
            Console.WriteLine("}\n");
        }

        /// <summary>
        /// Kernighan-Lin algorithm implementation.
        /// </summary>
        static void KernighanLinAlgorithm()
        {
            bool improvement = true;

            while (improvement)
            {
                // Step 1: Initialize gains and unlock all nodes
                foreach (var node in nodes)
                {
                    node.gain = CalculateNodeGain(node.nodeid);
                    node.locked = false;
                }

                swaps.Clear();
                cumulativeGains.Clear();

                // Step 2: Find pairs of nodes to swap
                for (int i = 0; i < vertexCount / 2; i++)
                {
                    var candidatePairs = new List<Tuple<int, int, int>>();

                    foreach (var a in nodes.Where(n => n.partition == 0 && !n.locked))
                    {
                        foreach (var b in nodes.Where(n => n.partition == 1 && !n.locked))
                        {
                            int gain = a.gain + b.gain - 2 * adjacencyMatrix[a.nodeid, b.nodeid];
                            candidatePairs.Add(new Tuple<int, int, int>(a.nodeid, b.nodeid, gain));
                        }
                    }

                    if (candidatePairs.Count == 0)
                    {
                        break;
                    }

                    // Select the pair with maximum gain
                    var maxGainPair = candidatePairs.OrderByDescending(p => p.Item3).First();

                    // Lock the nodes
                    nodes[maxGainPair.Item1].locked = true;
                    nodes[maxGainPair.Item2].locked = true;

                    // Record the swap and cumulative gain
                    swaps.Add(new Tuple<int, int>(maxGainPair.Item1, maxGainPair.Item2));
                    if (cumulativeGains.Count == 0)
                    {
                        cumulativeGains.Add(maxGainPair.Item3);
                    }
                    else
                    {
                        cumulativeGains.Add(cumulativeGains.Last() + maxGainPair.Item3);
                    }

                    // Update gains
                    UpdateGains(maxGainPair.Item1, maxGainPair.Item2);
                }

                // Step 3: Find k that maximizes the cumulative gain
                int maxGain = int.MinValue;
                int k = -1;
                for (int i = 0; i < cumulativeGains.Count; i++)
                {
                    if (cumulativeGains[i] > maxGain)
                    {
                        maxGain = cumulativeGains[i];
                        k = i;
                    }
                }

                if (maxGain > 0)
                {
                    // Perform swaps up to k
                    for (int i = 0; i <= k; i++)
                    {
                        SwapNodes(swaps[i].Item1, swaps[i].Item2);
                    }

                    totalCost -= maxGain;
                    Console.WriteLine("Improved Cut Size: " + totalCost);
                    PrintPartition();
                }
                else
                {
                    improvement = false;
                }
            }

            Console.WriteLine("Final Partition:");
            PrintPartition();
            // Changed line label from "Final Cut Size" to "Number of min.cuts"
            Console.WriteLine("Number of min.cuts: " + totalCost);
        }

        /// <summary>
        /// Calculates the gain of a node.
        /// </summary>
        /// <param name="nodeId">Identifier of the node.</param>
        /// <returns>The gain value.</returns>
        static int CalculateNodeGain(int nodeId)
        {
            int internalCost = 0;
            int externalCost = 0;
            int partition = nodes[nodeId].partition;

            for (int i = 0; i < vertexCount; i++)
            {
                if (adjacencyMatrix[nodeId, i] == 1)
                {
                    if (nodes[i].partition == partition)
                    {
                        internalCost += 1;
                    }
                    else
                    {
                        externalCost += 1;
                    }
                }
            }

            return externalCost - internalCost;
        }

        /// <summary>
        /// Updates the gains of all unlocked nodes after swapping nodes a and b.
        /// </summary>
        /// <param name="aId">Identifier of node a.</param>
        /// <param name="bId">Identifier of node b.</param>
        static void UpdateGains(int aId, int bId)
        {
            foreach (var node in nodes.Where(n => !n.locked))
            {
                if (node.nodeid != aId && node.nodeid != bId)
                {
                    int deltaGain = 0;
                    deltaGain += 2 * adjacencyMatrix[node.nodeid, aId] * (nodes[node.nodeid].partition == nodes[aId].partition ? 1 : -1);
                    deltaGain += 2 * adjacencyMatrix[node.nodeid, bId] * (nodes[node.nodeid].partition == nodes[bId].partition ? 1 : -1);
                    node.gain += deltaGain;
                }
            }
        }

        /// <summary>
        /// Swaps the partitions of nodes a and b.
        /// </summary>
        /// <param name="aId">Identifier of node a.</param>
        /// <param name="bId">Identifier of node b.</param>
        static void SwapNodes(int aId, int bId)
        {
            int tempPartition = nodes[aId].partition;
            nodes[aId].partition = nodes[bId].partition;
            nodes[bId].partition = tempPartition;
        }
    }
}

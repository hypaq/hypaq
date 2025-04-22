using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace FMHypergraphPartitioningOptimized
{
    class Program
    {
        const int LOCKED = 1;
        const int UNLOCKED = 0;

        class Node
        {
            public int gain;
            public int nodeid;
            public int status;
            public int partition;
            public LinkedListNode<Node> bucketNode;
        }

        // Global variables
        static List<int>[] hyperedges;          // Hyperedges represented as lists of node indices
        static List<int>[] adjacencyList;       // Adjacency list of the graph
        static List<int> optimal_partition_set;
        static Node[] nodes;                    // Array of nodes for faster access
        static int[] curr_part = new int[2];
        static int vertex, net, limit_max, limit_min;
        static int optimal_cutsize;
        static int totalGain;
        static int maxPossibleGain;
        static LinkedList<Node>[] gainBuckets;
        static int maxGainIndex;
        static int minGainIndex;
        static int currentCutsize;

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
                            ProcessHypergraph(hFile);
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

        static void ProcessHypergraph(string hypergraphFile)
        {
            // Create the log file name in the same directory
            string logFile = Path.ChangeExtension(hypergraphFile, ".log");

            // Store the original Console output
            TextWriter originalConsoleOut = Console.Out;

            try
            {
                using (StreamWriter logWriter = new StreamWriter(logFile))
                {
                    // Redirect Console output to the log file
                    Console.SetOut(logWriter);

                    // Start timing
                    DateTime startTime = DateTime.Now;
                    Console.WriteLine("Start Time: " + startTime.ToString("yyyy-MM-dd HH:mm:ss.fff"));

                    // Initialize data structures
                    Init();

                    // Read the hypergraph from the file
                    if (!InputFromFile(hypergraphFile))
                    {
                        // If reading failed, return
                        return;
                    }

                    // Proceed with the rest of the processing
                    InitRandomPartition();

                    int initialCutsize = CalculateCutSize();
                    currentCutsize = initialCutsize;
                    Console.WriteLine("Initial Cut Size: " + initialCutsize);

                    // Initialize gain buckets
                    InitializeGainBuckets();

                    // Main loop
                    bool improved = true;
                    while (improved)
                    {
                        improved = PerformPass();
                    }

                    PrintOptimalPartition();

                    // Finish timing
                    DateTime endTime = DateTime.Now;
                    Console.WriteLine("Finish Time: " + endTime.ToString("yyyy-MM-dd HH:mm:ss.fff"));

                    // Calculate total time spent
                    TimeSpan totalTime = endTime - startTime;
                    Console.WriteLine("Total Time Spent: " + totalTime.TotalMilliseconds + " ms");
                }

                // Restore Console output after the StreamWriter is disposed
                Console.SetOut(originalConsoleOut);

                // Optionally, print a message to the console indicating completion
                Console.WriteLine($"Processed hypergraph file '{hypergraphFile}', log saved to '{logFile}'.");
            }
            catch (Exception e)
            {
                // Restore Console output if an exception occurs
                Console.SetOut(originalConsoleOut);
                Console.WriteLine($"Error processing hypergraph file '{hypergraphFile}':");
                Console.WriteLine(e.Message);
            }
        }

        static void Init()
        {
            hyperedges = null;
            optimal_partition_set = null;
            nodes = null;
            curr_part[0] = 0;
            curr_part[1] = 0;
            optimal_cutsize = int.MaxValue;
            totalGain = 0;
        }

        static bool InputFromFile(string filename)
        {
            try
            {
                using (StreamReader sr = new StreamReader(filename))
                {
                    // Ignore the first line (timestamp)
                    var line = sr.ReadLine();

                    List<List<int>> tempHyperedges = new List<List<int>>();

                    // Read the incidence matrix
                    while ((line = sr.ReadLine()) != null)
                    {
                        line = line.Trim();
                        if (string.IsNullOrEmpty(line))
                        {
                            continue; // Skip empty lines
                        }

                        var tokens = line.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);

                        List<int> edge = new List<int>();
                        for (int i = 0; i < tokens.Length; i++)
                        {
                            if (tokens[i] == "1")
                            {
                                edge.Add(i);
                            }
                        }
                        if (edge.Count > 0)
                        {
                            tempHyperedges.Add(edge);
                        }
                    }

                    net = tempHyperedges.Count; // Number of hyperedges
                    if (net == 0)
                    {
                        Console.WriteLine("Error: Incidence matrix is empty.");
                        return false;
                    }

                    // Assuming that the number of vertices is the maximum index in hyperedges
                    vertex = tempHyperedges.Max(e => e.Max()) + 1;

                    hyperedges = tempHyperedges.ToArray();

                    // Build adjacency list
                    adjacencyList = new List<int>[vertex];
                    for (int i = 0; i < vertex; i++)
                    {
                        adjacencyList[i] = new List<int>();
                    }

                    foreach (var edge in hyperedges)
                    {
                        for (int i = 0; i < edge.Count; i++)
                        {
                            int u = edge[i];
                            for (int j = i + 1; j < edge.Count; j++)
                            {
                                int v = edge[j];
                                if (!adjacencyList[u].Contains(v))
                                    adjacencyList[u].Add(v);
                                if (!adjacencyList[v].Contains(u))
                                    adjacencyList[v].Add(u);
                            }
                        }
                    }

                    // Initialize other data structures
                    optimal_partition_set = new List<int>(new int[vertex]);

                    // Calculate area constraints with 10% deviation
                    //limit_min = vertex / 2;
                    //limit_max = vertex / 2;

                    //----------------------------------------------------
                    // Calculate area constraints with 10% deviation
                    //----------------------------------------------------
                    limit_min = (int)((vertex / 2) - 1);
                    limit_max = (int)((vertex / 2) + 1);
                    //----------------------------------------------------

                    // If the number of vertices is odd, adjust the limits
                    if (vertex % 2 != 0)
                    {
                        limit_max += 1;
                    }

                    // Optionally, print the constraints
                    Console.WriteLine("Processing file: " + filename);
                    Console.WriteLine("Number of vertices: " + vertex);
                    Console.WriteLine("Number of hyperedges: " + net);
                    Console.WriteLine("Constraint (min,max): {0}, {1}", limit_min, limit_max);
                }
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine("Error: Unable to read hypergraph file '" + filename + "'.");
                Console.WriteLine(e.Message);
                return false;
            }
        }

        static void InitRandomPartition()
        {
            curr_part[0] = 0;
            curr_part[1] = 0;
            nodes = new Node[vertex];
            Random rand = new Random();

            // Shuffle node indices to randomize initial partitioning
            List<int> nodeIndices = Enumerable.Range(0, vertex).OrderBy(x => rand.Next()).ToList();

            for (int i = 0; i < vertex; ++i)
            {
                int nodeId = nodeIndices[i];
                int part;

                // Alternate partitions while keeping within area constraints
                if (curr_part[0] < curr_part[1])
                {
                    part = 0;
                }
                else
                {
                    part = 1;
                }

                curr_part[part]++;
                nodes[nodeId] = new Node
                {
                    nodeid = nodeId,
                    gain = 0,
                    status = UNLOCKED,
                    partition = part
                };
            }
        }

        static void InitializeGainBuckets()
        {
            // The maximum possible gain is equal to the maximum degree of the graph
            maxPossibleGain = adjacencyList.Max(adj => adj.Count);

            // Initialize gain buckets array
            gainBuckets = new LinkedList<Node>[2 * maxPossibleGain + 1];

            for (int i = 0; i < gainBuckets.Length; i++)
            {
                gainBuckets[i] = new LinkedList<Node>();
            }

            maxGainIndex = -1;
            minGainIndex = gainBuckets.Length;

            // Calculate initial gains and populate gain buckets
            foreach (var node in nodes)
            {
                node.gain = CalculateGain(node.nodeid);
                int gainIndex = node.gain + maxPossibleGain;

                node.bucketNode = gainBuckets[gainIndex].AddLast(node);

                if (gainIndex > maxGainIndex) maxGainIndex = gainIndex;
                if (gainIndex < minGainIndex) minGainIndex = gainIndex;
            }
        }

        static int CalculateGain(int nodeid)
        {
            int fs = 0; // From Set (edges within the same partition)
            int te = 0; // To Set (edges to the other partition)
            int part = nodes[nodeid].partition;

            foreach (int neighbor in adjacencyList[nodeid])
            {
                if (nodes[neighbor].partition == part)
                    fs++;
                else
                    te++;
            }

            return te - fs;
        }

        static bool PerformPass()
        {
            int passGain = 0;
            int[] bestPartition = new int[vertex];
            int bestCutsize = currentCutsize;

            // Reset node statuses
            foreach (var node in nodes)
            {
                node.status = UNLOCKED;
            }

            for (int moveCount = 0; moveCount < vertex; moveCount++)
            {
                Node maxGainNode = SelectNodeWithMaxGain();
                if (maxGainNode == null)
                {
                    break; // No more nodes to move
                }

                if (!ValidSwap(maxGainNode.nodeid))
                {
                    maxGainNode.status = LOCKED;
                    continue;
                }

                // Move the node
                MoveNode(maxGainNode);
                passGain += maxGainNode.gain;

                // Update gains of affected nodes
                UpdateGains(maxGainNode);

                if (currentCutsize < bestCutsize)
                {
                    bestCutsize = currentCutsize;
                    for (int i = 0; i < vertex; i++)
                    {
                        bestPartition[i] = nodes[i].partition;
                    }
                }
            }

            if (bestCutsize < optimal_cutsize)
            {
                optimal_cutsize = bestCutsize;
                optimal_partition_set = bestPartition.ToList();
                return true; // Improvement made
            }

            return false; // No improvement
        }

        static Node SelectNodeWithMaxGain()
        {
            for (int g = maxGainIndex; g >= minGainIndex; g--)
            {
                if (gainBuckets[g].Count > 0)
                {
                    foreach (var node in gainBuckets[g])
                    {
                        if (node.status == UNLOCKED)
                        {
                            return node;
                        }
                    }
                }
            }
            return null;
        }

        static bool ValidSwap(int nodeid)
        {
            int part = nodes[nodeid].partition;
            int fromPartSize = curr_part[part];
            int toPartSize = curr_part[1 - part];

            // Check if moving the node would violate area constraints
            if ((fromPartSize - 1) < limit_min)
            {
                return false;
            }
            if ((toPartSize + 1) > limit_max)
            {
                return false;
            }
            return true;
        }

        static void MoveNode(Node node)
        {
            int oldPart = node.partition;
            int newPart = 1 - oldPart;
            curr_part[oldPart]--;
            curr_part[newPart]++;
            node.partition = newPart;
            node.status = LOCKED;

            // Remove from current gain bucket
            int oldGainIndex = node.gain + maxPossibleGain;
            if (node.bucketNode != null)
            {
                gainBuckets[oldGainIndex].Remove(node.bucketNode);
                node.bucketNode = null;
            }

            // Update current cutsize incrementally
            foreach (int neighbor in adjacencyList[node.nodeid])
            {
                if (nodes[neighbor].partition == oldPart)
                {
                    // Edge was internal, now becomes crossing
                    currentCutsize += 1;
                }
                else
                {
                    // Edge was crossing, now becomes internal
                    currentCutsize -= 1;
                }
            }
        }

        static void UpdateGains(Node tNode)
        {
            int tNodeId = tNode.nodeid;

            foreach (int neighborId in adjacencyList[tNodeId])
            {
                Node node = nodes[neighborId];
                if (node.status == UNLOCKED)
                {
                    int oldGainIndex = node.gain + maxPossibleGain;

                    // Remove node from current gain bucket
                    if (node.bucketNode != null)
                    {
                        gainBuckets[oldGainIndex].Remove(node.bucketNode);
                        node.bucketNode = null;
                    }

                    // Recalculate node's gain
                    node.gain = CalculateGain(node.nodeid);
                    int newGainIndex = node.gain + maxPossibleGain;

                    // Update maxGainIndex and minGainIndex
                    if (newGainIndex > maxGainIndex) maxGainIndex = newGainIndex;
                    if (newGainIndex < minGainIndex) minGainIndex = newGainIndex;

                    // Add node to new gain bucket
                    node.bucketNode = gainBuckets[newGainIndex].AddLast(node);
                }
            }
        }

        static int CalculateCutSize()
        {
            int cuts = 0;
            HashSet<Tuple<int, int>> countedEdges = new HashSet<Tuple<int, int>>();

            for (int i = 0; i < vertex; ++i)
            {
                foreach (int neighbor in adjacencyList[i])
                {
                    if (nodes[i].partition != nodes[neighbor].partition)
                    {
                        int u = Math.Min(i, neighbor);
                        int v = Math.Max(i, neighbor);
                        var edge = Tuple.Create(u, v);

                        if (!countedEdges.Contains(edge))
                        {
                            cuts++;
                            countedEdges.Add(edge);
                        }
                    }
                }
            }
            return cuts;
        }

        static void PrintOptimalPartition()
        {
            Console.WriteLine("Optimal Cut Size = " + optimal_cutsize);
            Console.Write("P1 = {");
            for (int i = 0; i < vertex; ++i)
            {
                if (optimal_partition_set[i] == 0) Console.Write((char)('a' + i) + " ");
            }
            Console.WriteLine("}");
            Console.Write("P2 = {");
            for (int i = 0; i < vertex; ++i)
            {
                if (optimal_partition_set[i] == 1) Console.Write((char)('a' + i) + " ");
            }
            Console.WriteLine("}");
            Console.WriteLine("Number of min.cuts: " + optimal_cutsize);
        }
    }
}

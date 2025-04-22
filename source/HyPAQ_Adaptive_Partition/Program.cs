using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace FMHypergraphPartitioning
{
    class Program
    {
        const int LOCKED = 1;
        const int UNLOCKED = 0;

        class Node
        {
            public int nodeid;
            public int gain;
            public int status;
            public int partition;
            public List<int> hyperedges; // List of hyperedge IDs
            public LinkedListNode<Node> bucketNode;
        }

        class Hyperedge
        {
            public int id;
            public List<int> nodes; // Node IDs
            public int part0; // Number of nodes in partition 0
            public int part1; // Number of nodes in partition 1
        }

        // Global variables
        static Hyperedge[] hyperedges;
        static Node[] nodes;
        static int[] curr_part = new int[2];
        static int vertex, net, limit_max, limit_min;
        static int optimal_cutsize;
        static int currentCutsize;
        static List<int> optimal_partition_set;
        static int maxPossibleGain;
        static LinkedList<Node>[] gainBuckets;
        static int maxGainIndex;
        static int minGainIndex;

        static void Main(string[] args)
        {
            if (args.Length >= 1)
            {
                string listFilename = args[0];

                // Read the list of file paths
                List<string> filePaths = new List<string>();
                try
                {
                    using (StreamReader sr = new StreamReader(listFilename))
                    {
                        string line;
                        while ((line = sr.ReadLine()) != null)
                        {
                            if (!string.IsNullOrWhiteSpace(line))
                            {
                                filePaths.Add(line.Trim());
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

                // Process each file
                foreach (string filePath in filePaths)
                {
                    if (File.Exists(filePath))
                    {
                        ProcessHypergraph(filePath);
                    }
                    else
                    {
                        Console.WriteLine($"Error: File '{filePath}' does not exist.");
                    }
                }
            }
            else
            {
                Console.WriteLine("Usage: Program.exe <list_of_files>");
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
            currentCutsize = 0;
        }

        static bool InputFromFile(string filename)
        {
            try
            {
                using (StreamReader sr = new StreamReader(filename))
                {
                    // Read the first line (legends) and ignore it
                    var line = sr.ReadLine();

                    if (line == null)
                    {
                        Console.WriteLine("Error: Hypergraph file is empty.");
                        return false;
                    }

                    // Parse the header to determine the number of hyperedges
                    var headerTokens = line.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);

                    if (headerTokens.Length < 2)
                    {
                        Console.WriteLine("Error: Header line does not contain hyperedge information.");
                        return false;
                    }

                    int numHyperedges = headerTokens.Length - 1; // Exclude the first column (vertex label)

                    // Initialize hyperedges
                    List<Hyperedge> tempHyperedges = new List<Hyperedge>();
                    for (int heId = 0; heId < numHyperedges; heId++)
                    {
                        tempHyperedges.Add(new Hyperedge
                        {
                            id = heId,
                            nodes = new List<int>(),
                            part0 = 0,
                            part1 = 0
                        });
                    }

                    HashSet<int> nodeIds = new HashSet<int>();
                    int vertexCount = 0;

                    // Read the incidence matrix
                    while ((line = sr.ReadLine()) != null)
                    {
                        line = line.Trim();
                        if (string.IsNullOrEmpty(line))
                        {
                            continue; // Skip empty lines
                        }

                        var tokens = line.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);

                        if (tokens.Length != numHyperedges + 1)
                        {
                            Console.WriteLine("Error: Inconsistent number of columns in hypergraph file.");
                            return false;
                        }

                        // The first token is the vertex label; ignore it
                        for (int heId = 0; heId < numHyperedges; heId++)
                        {
                            if (tokens[heId + 1] == "1")
                            {
                                tempHyperedges[heId].nodes.Add(vertexCount);
                                nodeIds.Add(vertexCount);
                            }
                        }

                        vertexCount++;
                    }

                    net = tempHyperedges.Count; // Number of hyperedges
                    if (net == 0)
                    {
                        Console.WriteLine("Error: No hyperedges found in the incidence matrix.");
                        return false;
                    }

                    vertex = vertexCount;

                    hyperedges = tempHyperedges.ToArray();

                    // Initialize nodes
                    nodes = new Node[vertex];
                    for (int i = 0; i < vertex; i++)
                    {
                        nodes[i] = new Node
                        {
                            nodeid = i,
                            gain = 0,
                            status = UNLOCKED,
                            hyperedges = new List<int>()
                        };
                    }

                    // Build node-hyperedge relationships
                    foreach (var hyperedge in hyperedges)
                    {
                        foreach (int nodeId in hyperedge.nodes)
                        {
                            nodes[nodeId].hyperedges.Add(hyperedge.id);
                        }
                    }

                    // Initialize other data structures
                    optimal_partition_set = new List<int>(new int[vertex]);

                    // Calculate area constraints with allowable deviation
                    limit_min = (int)((vertex / 2.0) - 1);
                    limit_max = (int)((vertex / 2.0) + 1);

                    // If the number of vertices is odd, adjust the limits
                    if (vertex % 2 != 0)
                    {
                        limit_max += 1;
                    }

                    // Print the constraints
                    Console.WriteLine("Processing file: " + filename);
                    Console.WriteLine("Number of vertices: " + vertex);
                    Console.WriteLine("Number of hyperedges: " + net);
                    Console.WriteLine($"Constraint (min,max): {limit_min}, {limit_max}");
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
                nodes[nodeId].partition = part;
            }

            // Initialize hyperedge partition counts
            foreach (var hyperedge in hyperedges)
            {
                hyperedge.part0 = 0;
                hyperedge.part1 = 0;
                foreach (int nodeId in hyperedge.nodes)
                {
                    int part = nodes[nodeId].partition;
                    if (part == 0)
                        hyperedge.part0++;
                    else
                        hyperedge.part1++;
                }
            }
        }

        static void InitializeGainBuckets()
        {
            // The maximum possible gain is equal to the number of hyperedges
            maxPossibleGain = net;

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
            int gain = 0;
            int part = nodes[nodeid].partition;

            foreach (int heId in nodes[nodeid].hyperedges)
            {
                Hyperedge he = hyperedges[heId];
                int myPartCount = (part == 0) ? he.part0 : he.part1;
                int otherPartCount = (part == 0) ? he.part1 : he.part0;

                if (otherPartCount == 0)
                {
                    // The hyperedge is not cut, moving node would make it cut
                    gain -= 1;
                }
                else if (myPartCount == 1)
                {
                    // The hyperedge is currently cut, moving node would make it uncut
                    gain += 1;
                }
            }

            return gain;
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

            // Update current cutsize incrementally and update hyperedge partition counts
            foreach (int heId in node.hyperedges)
            {
                Hyperedge he = hyperedges[heId];
                // Before moving node
                bool wasCut = (he.part0 > 0 && he.part1 > 0);

                // Update partition counts
                if (oldPart == 0)
                {
                    he.part0--;
                    he.part1++;
                }
                else
                {
                    he.part1--;
                    he.part0++;
                }

                // After moving node
                bool isCut = (he.part0 > 0 && he.part1 > 0);

                if (wasCut && !isCut)
                {
                    // Hyperedge became uncut
                    currentCutsize -= 1;
                }
                else if (!wasCut && isCut)
                {
                    // Hyperedge became cut
                    currentCutsize += 1;
                }
            }
        }

        static void UpdateGains(Node movedNode)
        {
            int nodeid = movedNode.nodeid;

            foreach (int heId in movedNode.hyperedges)
            {
                Hyperedge he = hyperedges[heId];

                foreach (int neighborId in he.nodes)
                {
                    if (neighborId == nodeid) continue;

                    Node neighbor = nodes[neighborId];
                    if (neighbor.status == LOCKED) continue;

                    int gainChange = 0;
                    int neighborPart = neighbor.partition;
                    int myPartCount = (neighborPart == 0) ? he.part0 : he.part1;
                    int otherPartCount = (neighborPart == 0) ? he.part1 : he.part0;

                    // Gain adjustment logic
                    if (otherPartCount == 0)
                    {
                        // Hyperedge is uncut, moving neighbor would make it cut
                        gainChange -= 1;
                    }
                    else if (myPartCount == 1)
                    {
                        // Hyperedge is cut, moving neighbor would make it uncut
                        gainChange += 1;
                    }

                    if (gainChange != 0)
                    {
                        // Remove neighbor from current gain bucket
                        int oldGainIndex = neighbor.gain + maxPossibleGain;
                        if (neighbor.bucketNode != null)
                        {
                            gainBuckets[oldGainIndex].Remove(neighbor.bucketNode);
                            neighbor.bucketNode = null;
                        }

                        neighbor.gain += gainChange;
                        int newGainIndex = neighbor.gain + maxPossibleGain;

                        // Update maxGainIndex and minGainIndex
                        if (newGainIndex > maxGainIndex) maxGainIndex = newGainIndex;
                        if (newGainIndex < minGainIndex) minGainIndex = newGainIndex;

                        // Add neighbor to new gain bucket
                        neighbor.bucketNode = gainBuckets[newGainIndex].AddLast(neighbor);
                    }
                }
            }
        }

        static int CalculateCutSize()
        {
            int cuts = 0;
            foreach (var he in hyperedges)
            {
                if (he.part0 > 0 && he.part1 > 0)
                {
                    cuts++;
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

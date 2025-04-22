using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SWGraphPartitioning
{
    /// <summary>
    /// Program implementing the Stoer-Wagner graph partitioning algorithm.
    /// </summary>
    class Program
    {
        const int MAXN = 550;
        const int INF = 1000000000;

        static int[,] edge = new int[MAXN, MAXN];
        static int[] dist = new int[MAXN];
        static bool[] vis = new bool[MAXN];
        static bool[] bin = new bool[MAXN];
        static int n; // Number of nodes

        static void Main(string[] args)
        {
            if (args.Length >= 1)
            {
                string graphFile = args[0];

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

                        // Initialize the adjacency matrix and read the graph
                        Init();
                        if (!InputFromFile(graphFile))
                        {
                            Console.SetOut(originalConsoleOut);
                            return;
                        }

                        // Run the Stoer-Wagner algorithm
                        int minCut = StoerWagner();
                        Console.WriteLine("Minimum Cut Size: " + minCut);

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
                }
            }
            else
            {
                Console.WriteLine("Usage: Program.exe <graph_file>");
            }
        }

        /// <summary>
        /// Initializes all data structures and variables.
        /// </summary>
        static void Init()
        {
            Array.Clear(edge, 0, edge.Length);
            Array.Clear(bin, 0, bin.Length);
        }

        /// <summary>
        /// Reads the graph from an input file, ignoring the first line (timestamp).
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

                    // Read the number of vertices
                    line = sr.ReadLine();
                    if (!int.TryParse(line, out n) || n <= 0 || n >= MAXN)
                    {
                        Console.WriteLine("Error: Invalid number of vertices.");
                        return false;
                    }

                    // Read the adjacency matrix
                    for (int i = 1; i <= n; i++)
                    {
                        line = sr.ReadLine();
                        if (line == null)
                        {
                            Console.WriteLine("Error: Incomplete adjacency matrix.");
                            return false;
                        }

                        var tokens = line.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                        if (tokens.Length != n)
                        {
                            Console.WriteLine("Error: Incorrect number of entries in adjacency matrix row.");
                            return false;
                        }

                        for (int j = 1; j <= n; j++)
                        {
                            if (!int.TryParse(tokens[j - 1], out edge[i, j]))
                            {
                                Console.WriteLine("Error: Invalid value in adjacency matrix.");
                                return false;
                            }
                        }
                    }
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
        /// Contracts the graph to find the minimum cut.
        /// </summary>
        static int Contract(out int s, out int t)
        {
            Array.Clear(dist, 0, dist.Length);
            Array.Clear(vis, 0, vis.Length);
            s = t = -1;
            int minCut = INF, maxc;

            for (int i = 1; i <= n; i++)
            {
                int k = -1;
                maxc = -1;
                for (int j = 1; j <= n; j++)
                {
                    if (!bin[j] && !vis[j] && dist[j] > maxc)
                    {
                        k = j;
                        maxc = dist[j];
                    }
                }

                if (k == -1) return minCut;

                s = t;
                t = k;
                minCut = maxc;
                vis[k] = true;

                for (int j = 1; j <= n; j++)
                {
                    if (!bin[j] && !vis[j])
                    {
                        dist[j] += edge[k, j];
                    }
                }
            }

            return minCut;
        }

        /// <summary>
        /// Stoer-Wagner algorithm implementation.
        /// </summary>
        static int StoerWagner()
        {
            int minCut = INF;

            for (int i = 1; i < n; i++)
            {
                int s, t;
                int ans = Contract(out s, out t);
                bin[t] = true;

                if (minCut > ans)
                {
                    minCut = ans;
                }

                if (minCut == 0)
                {
                    return 0;
                }

                for (int j = 1; j <= n; j++)
                {
                    if (!bin[j])
                    {
                        edge[s, j] = (edge[j, s] += edge[j, t]);
                    }
                }
            }

            return minCut;
        }
    }
}

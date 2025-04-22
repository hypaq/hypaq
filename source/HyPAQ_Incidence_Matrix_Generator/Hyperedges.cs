using System.Collections.Generic;

namespace HypeToIncidenceMatrix.Models
{
    // Represents a hyperedge (gate or control operation) in the hypergraph
    public class Hyperedge
    {
        public string Id { get; set; }
        public List<string> ConnectedVertices { get; set; }
        public string Type { get; set; } // e.g., "gate", "control_IF", etc.
        public int Timestamp { get; set; } // To manage temporal dependencies

        public Hyperedge(string id, List<string> connectedVertices, string type, int timestamp)
        {
            Id = id;
            ConnectedVertices = connectedVertices;
            Type = type;
            Timestamp = timestamp;
        }
    }
}

using System;

namespace QasmToHypergraph.Models
{
    // Represents a vertex (qubit or control node) in the hypergraph
    public class Vertex
    {
        public string Id { get; set; }

        public Vertex(string id)
        {
            Id = id;
        }
    }
}

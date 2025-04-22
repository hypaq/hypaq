using QasmToHypergraph;
using System.Collections.Generic;

namespace QasmToHypergraph.Models
{
    // Represents the entire enhanced hypergraph
    public class Hypergraph
    {
        public HashSet<Vertex> Vertices { get; set; }
        public List<Hyperedge> Hyperedges { get; set; }
        public List<HypergraphGroup> Groups { get; set; }
        public List<Dependency> Dependencies { get; set; }

        public Hypergraph()
        {
            Vertices = new HashSet<Vertex>(new VertexComparer());
            Hyperedges = new List<Hyperedge>();
            Groups = new List<HypergraphGroup>();
            Dependencies = new List<Dependency>();
        }

        // Adds a vertex if it doesn't already exist
        public void AddVertex(string id)
        {
            Vertices.Add(new Vertex(id));
        }

        // Adds a hyperedge
        public void AddHyperedge(Hyperedge edge)
        {
            Hyperedges.Add(edge);
        }

        // Adds a group
        public void AddGroup(HypergraphGroup group)
        {
            Groups.Add(group);
        }
    }

    // Comparer for Vertex to ensure uniqueness in HashSet
    public class VertexComparer : IEqualityComparer<Vertex>
    {
        public bool Equals(Vertex x, Vertex y)
        {
            return x.Id == y.Id;
        }

        public int GetHashCode(Vertex obj)
        {
            return obj.Id.GetHashCode();
        }
    }
}

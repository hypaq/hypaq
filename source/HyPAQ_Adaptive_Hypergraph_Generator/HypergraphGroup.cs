using QasmToHypergraph;
using System.Collections.Generic;

namespace QasmToHypergraph.Models
{
    // Represents a control structure group in the hypergraph
    public class HypergraphGroup
    {
        public string Id { get; set; }
        public List<Hyperedge> Hyperedges { get; set; }
        public List<HypergraphGroup> Subgroups { get; set; }

        public HypergraphGroup(string id)
        {
            Id = id;
            Hyperedges = new List<Hyperedge>();
            Subgroups = new List<HypergraphGroup>();
        }
    }
}

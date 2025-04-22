namespace HypeToIncidenceMatrix.Models
{
    // Represents a temporal dependency between two hyperedges
    public class Dependency
    {
        public string From { get; set; }
        public string To { get; set; }

        public Dependency(string from, string to)
        {
            From = from;
            To = to;
        }
    }
}

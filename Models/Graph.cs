namespace Models
{
    using System;
    using System.Collections.Generic;

    public class Graph
    {
        public Dictionary<int, List<int>> Vertices;

        public List<Community> Communities; 

        public Graph(Graph graph)
        {
            Vertices = graph.Vertices;
            Communities = new List<Community>();
        }

        public Graph()
        {
            Vertices = new Dictionary<int, List<int>>();
            Communities = new List<Community>();
        }
    }
}

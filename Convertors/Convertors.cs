namespace Convertors
{
    using System.IO;
    using System.Linq;

    using Models;

    public static class Convertors
    {
        public static void GraphToCommunityAsListOfNodes(string fileName, Graph graph)
        {
            using (var writer = new StreamWriter(fileName))
            {
                foreach (var graphCommunity in graph.Communities)
                {
                    writer.WriteLine(string.Join(" ", graphCommunity.Nodes));
                }
            }
        }

        public static void GraphToListOfEdges(string fileName, Graph graph)
        {
            using (var writer = new StreamWriter(fileName))
            {
                foreach (var kvp in graph.Vertices)
                {
                    foreach (var edge in kvp.Value)
                    {
                        writer.WriteLine($"{kvp.Key}\t{edge}");
                    }
                }
            }
        }
    }
}

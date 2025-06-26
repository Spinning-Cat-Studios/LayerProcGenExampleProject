using Godot;
using System.Collections.Generic;
using System.Collections.Concurrent;

namespace LayerProcGenExampleProject.Models.Graph
{
    public class GraphNode
    {
        private static int _nextId = 0;
        public int Id { get; }
        public Vector3 Position { get; }

        public GraphNode(Vector3 position)
        {
            Id = System.Threading.Interlocked.Increment(ref _nextId);
            Position = position;
        }
    }

    public class GraphEdge
    {
        public GraphNode StartNode { get; }
        public GraphNode EndNode { get; }
        public float Length { get; }
        public List<Vector3> Waypoints { get; } // The path the edge takes

        public GraphEdge(GraphNode start, GraphNode end, List<Vector3> waypoints)
        {
            StartNode = start;
            EndNode = end;
            Waypoints = waypoints;
            Length = CalculateLength(waypoints);
        }

        private float CalculateLength(List<Vector3> waypoints)
        {
            float length = 0;
            for (int i = 0; i < waypoints.Count - 1; i++)
            {
                length += waypoints[i].DistanceTo(waypoints[i + 1]);
            }
            return length;
        }
    }

    public class RoadGraph
    {
        // Thread-safe collections
        public readonly ConcurrentDictionary<int, GraphNode> Nodes = new();
        public readonly ConcurrentBag<GraphEdge> Edges = new();
        private readonly ConcurrentDictionary<Vector3, GraphNode> _nodePositions = new();

        public GraphNode AddNode(Vector3 position)
        {
            // This pattern is thread-safe with ConcurrentDictionary.
            // It ensures that if multiple threads call this with the same position,
            // only one will successfully create the node, and all will get it.
            return _nodePositions.GetOrAdd(position, (pos) => {
                var newNode = new GraphNode(pos);
                Nodes.TryAdd(newNode.Id, newNode);
                return newNode;
            });
        }

        public void AddEdge(GraphNode start, GraphNode end, List<Vector3> waypoints)
        {
            var newEdge = new GraphEdge(start, end, waypoints);
            // Add is thread-safe for ConcurrentBag
            Edges.Add(newEdge);
        }
    }
}

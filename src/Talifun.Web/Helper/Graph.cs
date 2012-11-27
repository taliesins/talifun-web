using System;
using System.Collections.Generic;
using System.Linq;
#if NET35
using System.Collections.ObjectModel;
using Iesi.Collections.Generic;
#endif

namespace Talifun.Web
{
    public class Graph<T>
    {
        [System.Diagnostics.DebuggerDisplay("{Value}")]
        class Node
        {
            public T Value;
            public bool Visited;
            public int Index;
            public int LowLink;
            public readonly ISet<Node> Incoming = new HashedSet<Node>();
            public readonly ISet<Node> Outgoing = new HashedSet<Node>();
        }

        private readonly Node[] _nodes;

        public Graph(IEnumerable<T> values, Func<T, IEnumerable<T>> getDependencies)
        {
            _nodes = values.Select(value => new Node { Value = value }).ToArray();
            // Create the Incoming edge sets for each node.
            foreach (var fromNode in _nodes)
            {
                foreach (var dependency in getDependencies(fromNode.Value))
                {
                    var toNode = _nodes.First(n => n.Value.Equals(dependency));
                    toNode.Incoming.Add(fromNode);
                    fromNode.Outgoing.Add(toNode);
                }
            }
        }

        public IEnumerable<T> TopologicalSort()
        {
            UnVisitAllNodes();

            var results = new List<T>();
            var initial = _nodes.Where(n => n.Incoming.Count == 0);
            foreach (var node in initial)
            {
                Visit(node, results);
            }
            return results;
        }

        void Visit(Node node, List<T> results)
        {
            if (node.Visited) return;

            node.Visited = true;

            foreach (var next in _nodes.Where(n => n.Incoming.Contains(node)))
            {
                Visit(next, results);
            }

            results.Add(node.Value);
        }

        public IEnumerable<ISet<T>> FindCycles()
        {
            // Tarjan's strongly connected components algorithm
            // http://en.wikipedia.org/wiki/Tarjan%E2%80%99s_strongly_connected_components_algorithm

            var index = 0;
            var stack = new Stack<Node>();
            var cycles = new HashedSet<ISet<T>>();

            foreach (var node in _nodes)
            {
                node.Index = -1;
            }

            foreach (var node in _nodes)
            {
                if (node.Index == -1)
                {
                    StrongConnect(node, stack, ref index, cycles);
                }
            }

            return cycles.Where(c => c.Count > 1); // Singleton sub-graphs are not a cycle for Cassette's purposes.
        }

        void StrongConnect(Node node, Stack<Node> stack, ref int index, ISet<ISet<T>> outputs)
        {
            node.Index = index;
            node.LowLink = index;
            index++;
            stack.Push(node);

            foreach (var next in node.Outgoing)
            {
                if (next.Index == -1)
                {
                    StrongConnect(next, stack, ref index, outputs);
                    node.LowLink = Math.Min(node.LowLink, next.LowLink);
                }
                else if (stack.Contains(next))
                {
                    node.LowLink = Math.Min(node.LowLink, next.Index);
                }
            }

            if (node.LowLink == node.Index)
            {
                var cycle = new HashedSet<T>();
                Node w;
                do
                {
                    w = stack.Pop();
                    cycle.Add(w.Value);
                } while (w != node);
                outputs.Add(cycle);
            }
        }

        void UnVisitAllNodes()
        {
            foreach (var node in _nodes)
            {
                node.Visited = false;
            }
        }
    }

#if NET35
    public static class SetExtensions
    {
        public static bool SetEquals<T>(this Iesi.Collections.Generic.ISet<T> set, IEnumerable<T> otherSet)
        {
            return set.ContainsAll(new Collection<T>(new List<T>(otherSet)));
        }
    }
#endif
}

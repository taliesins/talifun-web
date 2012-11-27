using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace Talifun.Web.Tests.Helper
{
    [TestFixture]
    public class Graph_FindCycles_Tests
    {
        [Test]
        public void NoCycle()
        {
            var edges = new Dictionary<int, int[]>
                {
                    {1, new[] {2}},
                    {2, new[] {3}},
                    {3, new int[] {}}
                };
            var graph = new Graph<int>(new[] {1, 2, 3}, i => edges[i]);

            Assert.IsEmpty(graph.FindCycles());
        }

        [Test]
        public void Disconnected()
        {
            var edges = new Dictionary<int, int[]>
                {
                    {1, new int[] {}},
                    {2, new int[] {}},
                    {3, new int[] {}}
                };
            var graph = new Graph<int>(new[] {1, 2, 3}, i => edges[i]);

            Assert.IsEmpty(graph.FindCycles());
        }

        [Test]
        public void Diamond()
        {
            var edges = new Dictionary<int, int[]>
                {
                    {1, new[] {2, 3}},
                    {2, new[] {4}},
                    {3, new[] {4}},
                    {4, new int[] {}}
                };
            var graph = new Graph<int>(new[] {1, 2, 3, 4}, i => edges[i]);

            Assert.IsEmpty(graph.FindCycles());
        }

        [Test]
        public void TotalCycle()
        {
            var edges = new Dictionary<int, int[]>
                {
                    {1, new[] {2}},
                    {2, new[] {3}},
                    {3, new[] {1}}
                };
            var graph = new Graph<int>(new[] {1, 2, 3}, i => edges[i]);

            Assert.IsTrue(graph.FindCycles().First().SetEquals(new[] {1, 2, 3}));
        }

        [Test]
        public void PartialCycle()
        {
            var edges = new Dictionary<int, int[]>
                {
                    {1, new[] {2}},
                    {2, new[] {3}},
                    {3, new[] {1}},
                    {4, new[] {1}}
                };
            var graph = new Graph<int>(new[] {1, 2, 3, 4}, i => edges[i]);

            Assert.IsTrue(graph.FindCycles().First().SetEquals(new[] {1, 2, 3}));
        }

        [Test]
        public void DisconnectedWithACycle()
        {
            var edges = new Dictionary<int, int[]>
                {
                    {1, new int[] {}},
                    {2, new[] {3}},
                    {3, new[] {2}}
                };
            var graph = new Graph<int>(new[] {1, 2, 3}, i => edges[i]);

            Assert.IsTrue(graph.FindCycles().First().SetEquals(new[] {2, 3}));
        }

        [Test]
        public void DiamondWithCycle()
        {
            var edges = new Dictionary<int, int[]>
                {
                    {1, new[] {2, 3}},
                    {2, new[] {4}},
                    {3, new[] {4}},
                    {4, new[] {1}}
                };
            var graph = new Graph<int>(new[] {1, 2, 3, 4}, i => edges[i]);

            var cycle = graph.FindCycles().First();
            Assert.IsTrue(cycle.SetEquals(new[] {1, 2, 3, 4}));
        }

        [Test]
        public void TwoDisconnectedCycles()
        {
            var edges = new Dictionary<int, int[]>
                {
                    {1, new[] {2}},
                    {2, new[] {1}},
                    {3, new[] {4}},
                    {4, new[] {3}}
                };
            var graph = new Graph<int>(new[] {1, 2, 3, 4}, i => edges[i]);

            var cycles = graph.FindCycles().ToArray();
            Assert.IsTrue(cycles[0].SetEquals(new[] {1, 2}));
            Assert.IsTrue(cycles[1].SetEquals(new[] {3, 4}));
        }

        [Test]
        public void BackReferenceDoesNotCauseCycle()
        {
            var edges = new Dictionary<int, int[]>
                {
                    {1, new[] {2}},
                    {2, new[] {3}},
                    {3, new int[] {}},
                    {4, new[] {1}}
                };
            var graph = new Graph<int>(new[] {1, 2, 3, 4}, i => edges[i]);

            Assert.IsEmpty(graph.FindCycles());
        }

        [Test]
        public void MoreComplexGraphWithNoCycles()
        {
            var edges = new Dictionary<int, int[]>
                {
                    {1, new int[] {}},
                    {2, new int[] {}},
                    {3, new[] {1, 2, 5}},
                    {4, new[] {1, 2, 6}},
                    {5, new[] {2}},
                    {6, new[] {2, 3, 7, 5}},
                    {7, new[] {3}},
                    {8, new[] {3}}
                };
            var graph = new Graph<int>(new[] {1, 2, 3, 4, 5, 6, 7, 8}, i => edges[i]);

            var result = graph.FindCycles().ToArray();
            Assert.IsEmpty(result);
        }

        [Test]
        public void PartialCycle2()
        {
            var edges = new Dictionary<int, int[]>
                {
                    {1, new[] {4}},
                    {2, new[] {4}},
                    {3, new[] {4, 6}},
                    {4, new[] {5}},
                    {5, new int[] {}},
                    {6, new[] {5}}
                };
            var graph = new Graph<int>(new[] {1, 2, 3, 4, 5, 6}, i => edges[i]);

            Assert.IsEmpty(graph.FindCycles());
        }
    }
}

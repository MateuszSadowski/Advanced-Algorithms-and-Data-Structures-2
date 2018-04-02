using ASD.Graphs;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ASD
{
    public class CyclesFinder : MarshalByRefObject
    {
        /// <summary>
        /// Sprawdza czy graf jest drzewem
        /// </summary>
        /// <param name="g">Graf</param>
        /// <returns>true jeśli graf jest drzewem</returns>
        public bool IsTree(Graph g)
        {
            if(g.Directed)
            {
                throw new ArgumentException();
            }
            int cc = 0;
            GeneralSearchGraphExtender.GeneralSearchAll<EdgesStack>(g, null, null, null, out cc);

            return g.EdgesCount == g.VerticesCount - 1 && cc == 1;
        }

        /// <summary>
        /// Wyznacza cykle fundamentalne grafu g względem drzewa t.
        /// Każdy cykl fundamentalny zawiera dokadnie jedną krawędź spoza t.
        /// </summary>
        /// <param name="g">Graf</param>
        /// <param name="t">Drzewo rozpinające grafu g</param>
        /// <returns>Tablica cykli fundamentalnych</returns>
        /// <remarks>W przypadku braku cykli zwracać pustą (0-elementową) tablicę, a nie null</remarks>
        public Edge[][] FindFundamentalCycles(Graph g, Graph t)
        {
            if (g.Directed)
            {
                throw new ArgumentException();
            }

            //TODO: for debugging purpouses, to remove later
            //GraphExport ge = new GraphExport();
            //ge.Export(g);
            //ge.Export(t);

            //check if t is correct spanning tree for g
            if(!IsTree(t) || t.VerticesCount != g.VerticesCount)
            {
                throw new ArgumentException();
            }

            HashSet<Edge> treeEdgeList = new HashSet<Edge>();
            for (int v = 0; v < t.VerticesCount; v++)
            {
                foreach (var e in t.OutEdges(v))
                {
                    treeEdgeList.Add(e);
                }
            }

            Graph h = g.Clone();
            Stack<Edge[]> cycles = new Stack<Edge[]>();
            Edge[] cycle;
            Edge outEdge;
            int outsideEdgesCount = g.EdgesCount - t.EdgesCount;
            Stack<Edge> tmpEdgesDeleted = new Stack<Edge>();

            while (outsideEdgesCount > 0)
            {
                if(!FindFundamentalCycle(h, treeEdgeList, out cycle, out outEdge))
                {   //have not found correct fundemental cycle in this iteration
                    tmpEdgesDeleted.Push(outEdge);  //delete and remember the second outside edge found
                    h.DelEdge(outEdge);
                    continue;   //try again without that edge
                }
                else
                {   //have found correct fundamental cycle
                    cycles.Push(cycle);
                    h.DelEdge(outEdge);
                    outsideEdgesCount--;
                    while(tmpEdgesDeleted.Count > 0)
                    {   //restore previously deleted (unused) edges
                        h.AddEdge(tmpEdgesDeleted.Pop());
                    }
                }
            }

            Edge[][] result = new Edge[cycles.Count][];

            int i = 0;
            while (cycles.Count > 0)
            {
                result[i++] = cycles.Pop();
            }

            return result;
        }

        public bool FindFundamentalCycle(Graph g, HashSet<Edge> treeEdgeList, out Edge[] cycle, out Edge outsideEdge)
        {
            int[] visited = new int[g.VerticesCount]; // 0 - nieodwiedzony, 1 - szary, 2 - czarny
            int[] from = new int[g.VerticesCount];
            EdgesStack edges = new EdgesStack();
            cycle = new Edge[g.VerticesCount];
            bool hasCycle = false;
            int cycleEnd = -1;
            int cc;
            int outOfTreeEdgeCount = 0;
            Edge outEdge = new Edge();
            //Random rand = new Random();
            //int s = rand.Next(0, g.VerticesCount);

            Predicate<int> preVertex = delegate (int v)
            {
                visited[v] = 1; // szary
                return true;
            };

            Predicate<int> postVertex = delegate (int v)
            {
                visited[v] = 2; // czarny

                if (!edges.Empty)
                    edges.Get();

                return true;
            };

            Predicate<Edge> visitEdge = delegate (Edge e)
            {
                if (!g.Directed && from[e.From] == e.To)
                {   //undirected graps are implemented as directed graphs with edges both ways
                    return true;
                }

                if (!treeEdgeList.Contains(e))
                {
                    if(e.From != outEdge.To || e.To != outEdge.From)
                    {
                        outOfTreeEdgeCount++;
                        outEdge = e;
                    }
                }

                if(outOfTreeEdgeCount > 1)
                {   //got 2 edges not contained in the spanning tree
                    return false;
                }

                from[e.To] = e.From;
                edges.Put(e);

                if (visited[e.To] == 1)
                {
                    hasCycle = true;
                    cycleEnd = e.To;
                    return false;
                }

                return true;
            };

            //g.GeneralSearchFrom<EdgesStack>(s, preVertex, postVertex, visitEdge);
            //search for cycle
            g.GeneralSearchAll<EdgesStack>(preVertex, postVertex, visitEdge, out cc);

            //reconstruct cycle
            if (hasCycle)
            {
                int i = 0;
                bool stop = false;

                while (!edges.Empty)
                {
                    Edge e = edges.Get();

                    if (!stop)
                        cycle[i++] = e;

                    if (!edges.Empty && edges.Peek().To == cycleEnd)
                    {
                        stop = true;
                        continue;
                    }
                }

                Array.Resize<Edge>(ref cycle, i);
                Array.Reverse(cycle);

                outsideEdge = outEdge;
                return true;
            }

            //there was no cycle found
            outsideEdge = outEdge;
            cycle = null;
            return false;
        }

        /// <summary>
        /// Dodaje 2 cykle fundamentalne
        /// </summary>
        /// <param name="c1">Pierwszy cykl</param>
        /// <param name="c2">Drugi cykl</param>
        /// <returns>null, jeśli wynikiem nie jest cykl i suma cykli, jeśli wynik jest cyklem</returns>
        public Edge[] AddFundamentalCycles(Edge[] c1, Edge[] c2)
        {

            return null;
        }

    }

}

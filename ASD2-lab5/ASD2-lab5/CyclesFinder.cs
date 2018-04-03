using ASD.Graphs;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ASD
{
    internal class CycleFinderData
    {
        public int[] visited;
        public int[] from;
        public EdgesStack edges;
        public Edge[] cycle;
        public bool hasCycle;
        public int cycleEnd;
        public int outOfTreeEdgeCount;
        public Edge outEdge;

        internal CycleFinderData(Graph g)
        {
            visited = new int[g.VerticesCount];
            from = new int[g.VerticesCount];
            edges = new EdgesStack();
            cycle = new Edge[g.VerticesCount];
            hasCycle = false;
            cycleEnd = -1;
            outOfTreeEdgeCount = 0;
            outEdge = new Edge();
        }
    }

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
            if (!IsTree(t) || t.VerticesCount != g.VerticesCount)
            {
                throw new ArgumentException();
            }
            else
            {
                //check if edges of t are contained in edges of g
                HashSet<Edge> gEdges = new HashSet<Edge>();
                for (int v = 0; v < g.VerticesCount; v++)
                {
                    foreach (var e in g.OutEdges(v))
                    {
                        gEdges.Add(e);
                    }
                }
                for (int v = 0; v < g.VerticesCount; v++)
                {
                    foreach (var e in t.OutEdges(v))
                    {
                        if(!gEdges.Contains(e))
                        {
                            throw new ArgumentException();
                        }
                    }
                }
            }

            //t is correct spanning tree of g
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
            //Edge[] cycle;
            //Edge outEdge;
            int outsideEdgesCount = g.EdgesCount - t.EdgesCount;
            //Stack<Edge> tmpEdgesDeleted = new Stack<Edge>();

            while (outsideEdgesCount > 0)
            {
                //if(!FindFundamentalCycle(h, treeEdgeList, out cycle, out outEdge))
                //{   //have not found correct fundemental cycle in this iteration
                //    tmpEdgesDeleted.Push(outEdge);  //delete and remember the second outside edge found
                //    h.DelEdge(outEdge);
                //    continue;   //try again without that edge
                //}
                //else
                //{   //have found correct fundamental cycle
                //    cycles.Push(cycle);
                //    h.DelEdge(outEdge);
                //    outsideEdgesCount--;
                //    while(tmpEdgesDeleted.Count > 0)
                //    {   //restore previously deleted (unused) edges
                //        h.AddEdge(tmpEdgesDeleted.Pop());
                //    }
                //}
                CycleFinderData data = new CycleFinderData(g);
                if(MyFindFundamentalCycle(0, h, treeEdgeList, data, 0))
                {   //have found correct fundamental cycle
                    cycles.Push(data.cycle);
                    h.DelEdge(data.outEdge);
                    outsideEdgesCount--;
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

        //search for cycle by DFS
        internal bool MyFindFundamentalCycle(int startVertex, Graph g, HashSet<Edge> treeEdgeList, CycleFinderData data, int level)
        {
            bool resultValue;

            //function is valid only for undirected graphs
            if (g.EdgesCount < 3)   //TODO: move outside rec function
            {   //then no cycle can exist
                data.outEdge = new Edge();
                data.cycle = new Edge[0];
                return false;
            }

            //preVertex
            data.visited[startVertex] = 1;

            foreach (var e in g.OutEdges(startVertex))
            {
                //visitEdge
                if (!g.Directed && data.from[e.From] == e.To)
                {   //undirected graps are implemented as directed graphs with edges both ways
                    continue;
                }

                if (!treeEdgeList.Contains(e))
                {
                    if(data.outOfTreeEdgeCount == 0)
                    {
                        data.outOfTreeEdgeCount++;
                        data.outEdge = e;
                    }
                    else
                    {
                        continue;   //choose edge contained in the spanning tree instead
                    }
                }

                data.from[e.To] = e.From;
                data.edges.Put(e);

                if (data.visited[e.To] == 1)
                {   //TODO: found cycle, break recursion
                    data.hasCycle = true;
                    data.cycleEnd = e.To;
                    //outsideEdge = outEdge;
                    return true;
                }

                if(resultValue = MyFindFundamentalCycle(e.To, g, treeEdgeList, data, level + 1))
                {   //found cycle
                    //executed only once
                    if (data.hasCycle && level == 0)
                    {   //reconstruct cycle
                        int i = 0;
                        bool stop = false;

                        while (!data.edges.Empty)
                        {
                            Edge e2 = data.edges.Get();

                            if (!stop)
                                data.cycle[i++] = e2;

                            if (!data.edges.Empty && data.edges.Peek().To == data.cycleEnd)
                            {
                                stop = true;
                                continue;
                            }
                        }

                        Array.Resize<Edge>(ref data.cycle, i);
                        Array.Reverse(data.cycle);
                    }
                    return true;
                }

                //postVertex
                data.visited[e.To] = 2; // czarny TODO: should be e.To ??

                if (!data.edges.Empty)
                    data.edges.Get();
            }

            ////executed only once
            //if (data.hasCycle && level == 0)
            //{   //reconstruct cycle
            //    int i = 0;
            //    bool stop = false;

            //    while (!data.edges.Empty)
            //    {
            //        Edge e2 = data.edges.Get();

            //        if (!stop)
            //            data.cycle[i++] = e2;

            //        if (!data.edges.Empty && data.edges.Peek().To == data.cycleEnd)
            //        {
            //            stop = true;
            //            continue;
            //        }
            //    }

            //    Array.Resize<Edge>(ref data.cycle, i);
            //    Array.Reverse(data.cycle);

            //    return true;
            //}

            return false;
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
            HashSet<Edge> edgesToSkip = new HashSet<Edge>();
            HashSet<Edge> c2EdgeSet = new HashSet<Edge>();
            EdgesQueue edgesQueue = new EdgesQueue();
            //bool[] addEdge = new bool[c2.Length];
            int maxVertices = 0;

            foreach (var e2 in c2)
            {
                c2EdgeSet.Add(e2);
                //remember max vertex number
                if(e2.From > e2.To && maxVertices < e2.From)
                {
                    maxVertices = e2.From;
                }
                else if(maxVertices < e2.To)
                {
                    maxVertices = e2.To;
                }
            }

            foreach (var e1 in c1)
            {
                Edge e1Reversed = new Edge(e1.To, e1.From, e1.Weight);
                if (!c2EdgeSet.Contains(e1) && !c2EdgeSet.Contains(e1Reversed))
                {
                    edgesQueue.Put(e1);
                }
                else
                {
                    edgesToSkip.Add(e1);
                    edgesToSkip.Add(e1Reversed);
                }
                //remember max vertex number
                if (e1.From > e1.To && maxVertices < e1.From)
                {
                    maxVertices = e1.From;
                }
                else if (maxVertices < e1.To)
                {
                    maxVertices = e1.To;
                }
            }

            foreach (var e2 in c2)
            {
                if (!edgesToSkip.Contains(e2))
                {
                    edgesQueue.Put(e2);
                }
            }

            Graph g = new AdjacencyListsGraph<SimpleAdjacencyList>(false, maxVertices + 1);     //undirected

            while(!edgesQueue.Empty)
            {
                Edge e = edgesQueue.Get();
                g.AddEdge(e);
                if(g.OutDegree(e.From) > 2 || g.OutDegree(e.To) > 2)
                {   //result of addition is not a cycle
                    return null;
                }
            }

            int cc;
            HashSet<Edge> visitedEdges = new HashSet<Edge>();

            Edge[] resultCycle = new Edge[g.EdgesCount];
            int i = 0;
            int lastVertex = -1;

            Predicate<Edge> visitEdge = delegate (Edge e)
            {
                if (!visitedEdges.Contains(e))
                {
                    //visitedEdges.Add(e);    //TODO: may be obsolete
                    visitedEdges.Add(new Edge(e.To, e.From, e.Weight));
                    if (lastVertex != -1)
                    {
                        if (e.From != lastVertex)
                        {   //input cycles were seperate and result is also 2 seperate cycles
                            return false;
                        }
                    }
                    resultCycle[i++] = e;
                    lastVertex = e.To;
                }

                return true;
            };

            //Predicate<Edge> visitEdge = delegate(Edge e)
            //{
            //    if(!visitedEdges.Contains(e))
            //    {
            //        //visitedEdges.Add(e);    //TODO: may be obsolete
            //        visitedEdges.Add(new Edge(e.To, e.From, e.Weight));

            //        edgesQueue.Put(e);
            //    }

            //    return true;
            //};

            bool stoped = !GeneralSearchGraphExtender.GeneralSearchAll<EdgesStack>(g, null, null, visitEdge, out cc);

            if(stoped)
            {   //input cycles were seperate and result is also 2 seperate cycles
                return null;
            }

            //Edge[] resultCycle = new Edge[edgesQueue.Count];
            //int i = 0;
            //int lastVertex = -1;
            //while (!edgesQueue.Empty)
            //{
            //    Edge e = edgesQueue.Get();
            //    if(lastVertex != -1)
            //    {
            //        if(e.From != lastVertex)
            //        {   //input cycles were seperate and result is also 2 seperate cycles
            //            return null;
            //        }
            //    }
            //    resultCycle[i++] = e;
            //    lastVertex = e.To;
            //}

            return resultCycle;
            //HashSet<Edge> edgesToSkip = new HashSet<Edge>();
            //HashSet<Edge> c2EdgeSet = new HashSet<Edge>();
            //EdgesQueue edgesQueue = new EdgesQueue();
            ////bool[] addEdge = new bool[c2.Length];

            //foreach (var e2 in c2)
            //{
            //    c2EdgeSet.Add(e2);
            //}

            //foreach (var e1 in c1)
            //{
            //    Edge e1Reversed = new Edge(e1.To, e1.From, e1.Weight);
            //    if(!c2EdgeSet.Contains(e1) && !c2EdgeSet.Contains(e1Reversed))
            //    {
            //        edgesQueue.Put(e1);
            //    }
            //    else
            //    {
            //        edgesToSkip.Add(e1);
            //        edgesToSkip.Add(e1Reversed);
            //    }
            //}

            //foreach (var e2 in c2)
            //{
            //    if(!edgesToSkip.Contains(e2))
            //    {
            //        edgesQueue.Put(e2);
            //    }
            //}

            //Edge[] resultCycle = new Edge[edgesQueue.Count];
            //int i = 0;
            //while(!edgesQueue.Empty)
            //{
            //    resultCycle[i++] = edgesQueue.Get();
            //}

            //return resultCycle;
        }

    }

}

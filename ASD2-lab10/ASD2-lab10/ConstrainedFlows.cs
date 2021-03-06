﻿using ASD.Graphs;
using System.Linq;

namespace ASD
{
    public class ConstrainedFlows : System.MarshalByRefObject
    {
        // testy, dla których ma być generowany obrazek
        // graf w ostatnim teście ma bardzo dużo wierzchołków, więc lepiej go nie wyświetlać
        public static int[] circulationToDisplay = { };
        public static int[] constrainedFlowToDisplay = { };

        /// <summary>
        /// Metoda znajdująca cyrkulację w grafie, z określonymi żądaniami wierzchołków.
        /// Żądania opisane są w tablicy demands. Szukamy funkcji, która dla każdego wierzchołka będzie spełniała warunek:
        /// suma wartości na krawędziach wchodzących - suma wartości na krawędziach wychodzących = demands[v]
        /// </summary>
        /// <param name="G">Graf wejściowy, wagi krawędzi oznaczają przepustowości</param>
        /// <param name="demands">Żądania wierzchołków</param>
        /// <returns>Graf reprezentujący wynikową cyrkulację.
        /// Reprezentacja cyrkulacji jest analogiczna, jak reprezentacja przepływu w innych funkcjach w bibliotece.
        /// Należy zwrócić kopię grafu G, gdzie wagi krawędzi odpowiadają przepływom na tych krawędziach.
        /// Zwróć uwagę na rozróżnienie sytuacji, kiedy mamy zerowy przeływ na krawędzi (czyli istnieje
        /// krawędź z wagą 0) od sytuacji braku krawędzi.
        /// Jeśli żądana cyrkulacja nie istnieje, zwróć null.
        /// </returns>
        /// <remarks>
        /// Nie można modyfikować danych wejściowych!
        /// Złożoność metody powinna być asymptotycznie równa złożoności metody znajdującej największy przeływ (z biblioteki).
        /// </remarks>
        public Graph FindCirculation(Graph G, double[] demands)
        {
            if(!CanBeFeasible(demands))
            {
                return null;
            }

            Graph seekFlowGraph = BuildSeekFlowGraph(G, demands);
            var circulationFlow = SeekCirculation(seekFlowGraph, 0, 1);
            if (circulationFlow.flow == null)
                return null;

            return BuildAnswer(G, circulationFlow);
        }

        internal bool CanBeFeasible(double[] demands)
        {
            return demands.Aggregate((a, b) => a + b) == 0;
        }

        internal (double val, Graph flow) SeekCirculation(Graph seekFlowGraph, int source, int target)
        {
            var circulationFlow = MaxFlowGraphExtender.FordFulkersonDinicMaxFlow(seekFlowGraph, 0, 1, MaxFlowGraphExtender.OriginalDinicBlockingFlow);

            double sumFromSource = 0, sumToTarget = 0;
            foreach (var e in seekFlowGraph.OutEdges(0))
            {
                sumFromSource += e.Weight;
            }
            for (int v = 2; v < seekFlowGraph.VerticesCount; v++)
            {
                foreach (var e in seekFlowGraph.OutEdges(v))
                {
                    if (e.To == 1)
                        sumToTarget += e.Weight;
                }
            }

            if(sumToTarget != circulationFlow.value)
            {
                return (0, null);
            }

            return circulationFlow;
        }

        internal Graph BuildAnswer(Graph G, (double val, Graph flow) circulationFlow)
        {
            Graph c = circulationFlow.flow.IsolatedVerticesGraph(true, G.VerticesCount);

            for (int v = 2; v < G.VerticesCount + 2; v++)
            {
                foreach (var e in circulationFlow.flow.OutEdges(v))
                {
                    if (e.To != 0 && e.To != 1)
                        c.AddEdge(v - 2, e.To - 2, e.Weight);
                }
            }

            return c;  // zmienić
        }

        internal Graph BuildSeekFlowGraph(Graph G, double[] demands)
        {
            int n = G.VerticesCount;
            Graph h = new AdjacencyListsGraph<SimpleAdjacencyList>(true, n + 2);

            //0 -> super-source
            //1 -> super-target
            //2 - n+2 -> vertices
            for (int v = 2; v < n+2; v++)
            {
                if (demands[v - 2] < 0)
                    h.AddEdge(0, v, (-1)*demands[v - 2]);
                else if (demands[v - 2] > 0)
                    h.AddEdge(v, 1, demands[v - 2]);
            }

            for (int v = 0; v < n; v++)
            {
                foreach (var e in G.OutEdges(v))
                {
                    h.AddEdge(e.From + 2, e.To + 2, e.Weight);
                }
            }

            return h;
        }

        /// <summary>
        /// Funkcja zwracająca przepływ z ograniczeniami, czyli przepływ, który dla każdej z krawędzi
        /// ma wartość pomiędzy dolnym ograniczeniem a górnym ograniczeniem.
        /// Zwróć uwagę, że interesuje nas *jakikolwiek* przepływ spełniający te ograniczenia.
        /// </summary>
        /// <param name="source">źródło</param>
        /// <param name="sink">ujście</param>
        /// <param name="G">graf wejściowy, wagi krawędzi oznaczają przepustowości (górne ograniczenia)</param>
        /// <param name="lowerBounds">kopia grafu G, wagi krawędzi oznaczają dolne ograniczenia przepływu</param>
        /// <returns>Graf reprezentujący wynikowy przepływ (analogicznie do poprzedniej funkcji i do reprezentacji
        /// przepływu w funkcjach z biblioteki.
        /// Jeśli żądany przepływ nie istnieje, zwróć null.
        /// </returns>
        /// <remarks>
        /// Nie można modyfikować danych wejściowych!
        /// Złożoność metody powinna być asymptotycznie równa złożoności metody znajdującej największy przeływ (z biblioteki).
        /// </remarks>
        /// <hint>Wykorzystaj poprzednią część zadania.
        /// </hint>
        /// 
        internal double[,] edgeWeightsInLowerBounds;
        internal bool edgeRemoved;
        public Graph FindConstrainedFlow(int source, int sink, Graph G, Graph lowerBounds)
        {
            edgeRemoved = false;
            edgeWeightsInLowerBounds = new double[G.VerticesCount, G.VerticesCount];
            double[] demands = FindDemands(source, sink, lowerBounds);
            if (!CanBeFeasible(demands))
            {
                return null;
            }
            var seekFlowGraph = BuildSeekConstrainedFlowGraph(source, sink, G, demands);
            var circulationFlow = SeekConstrainedCirculation(seekFlowGraph, 0, 1);
            if (circulationFlow.flow == null)
                return null;
            Graph flow = BuildAnswerConstrainedFlow(source, sink, G, circulationFlow);
            return flow;
        }

        internal (double val, Graph flow) SeekConstrainedCirculation(Graph seekFlowGraph, int source, int target)
        {
            var circulationFlow = MaxFlowGraphExtender.FordFulkersonDinicMaxFlow(seekFlowGraph, 0, 1, MaxFlowGraphExtender.OriginalDinicBlockingFlow);

            double sumToTarget = 0;
            for (int v = 2; v < seekFlowGraph.VerticesCount; v++)
            {
                foreach (var e in seekFlowGraph.OutEdges(v))
                {
                    if (e.To == 1)
                        sumToTarget += e.Weight;
                }
            }

            if (sumToTarget != circulationFlow.value)
            {
                return (0, null);
            }

            return circulationFlow;
        }

        internal Graph BuildAnswerConstrainedFlow(int source, int target, Graph G, (double val, Graph flow) circulationFlow)
        {
            Graph c = circulationFlow.flow.IsolatedVerticesGraph(true, G.VerticesCount);

            for (int v = 2; v < G.VerticesCount + 2; v++)
            {
                foreach (var e in circulationFlow.flow.OutEdges(v))
                {
                    if(e.From == target + 2 && e.To == source + 2)
                        continue;

                    if (e.To != 0 && e.To != 1)
                        c.AddEdge(v - 2, e.To - 2, e.Weight + edgeWeightsInLowerBounds[e.From - 2, e.To - 2]);
                }
            }

            if(edgeRemoved)
            {
                c.AddEdge(target, source, edgeWeightsInLowerBounds[target, source]);
            }

            return c;  // zmienić
        }

        internal double[] FindDemands(int source, int sink, Graph lowerBounds)
        {
            double[] demands = new double[lowerBounds.VerticesCount];
            for (int v = 0; v < lowerBounds.VerticesCount; v++)
            {
                foreach (var e in lowerBounds.OutEdges(v))
                {
                    edgeWeightsInLowerBounds[e.From, e.To] = e.Weight;
                    demands[v] += e.Weight;
                    demands[e.To] -= e.Weight;
                }
            }
            return demands;
        }

        internal Graph BuildSeekConstrainedFlowGraph(int source, int sink, Graph G, double[] demands)
        {
            int n = G.VerticesCount;
            Graph h = new AdjacencyListsGraph<SimpleAdjacencyList>(true, n + 2);

            //0 -> super-source
            //1 -> super-target
            //2 - n+2 -> vertices
            for (int v = 2; v < n + 2; v++)
            {
                if (demands[v - 2] < 0)
                    h.AddEdge(0, v, (-1) * demands[v - 2]);
                else if (demands[v - 2] > 0)
                    h.AddEdge(v, 1, demands[v - 2]);
            }

            for (int v = 0; v < n; v++)
            {
                foreach (var e in G.OutEdges(v))
                {
                    h.AddEdge(e.From + 2, e.To + 2, e.Weight - edgeWeightsInLowerBounds[e.From, e.To]);
                }
            }

            if (!h.AddEdge(sink + 2, source + 2, double.MaxValue))
            {   //there is edge [sink, source] in G
                h.DelEdge(sink + 2, source + 2);
                h.AddEdge(sink + 2, source + 2, double.MaxValue);
                edgeRemoved = true;
            }

            return h;
        }

    }
}
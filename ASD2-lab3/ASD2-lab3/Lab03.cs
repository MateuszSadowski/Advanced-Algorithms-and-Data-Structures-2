﻿using ASD.Graphs;
using System;

namespace ASD
{

    // Klasy Lab03Helper NIE WOLNO ZMIENIAĆ !!!
    public class Lab03Helper : System.MarshalByRefObject
    {
        public Graph SquareOfGraph(Graph graph) => graph.SquareOfGraph();
        public Graph LineGraph(Graph graph, out (int x, int y)[] names) => graph.LineGraph(out names);
        public int VertexColoring(Graph graph, out int[] colors) => graph.VertexColoring(out colors);
        public int StrongEdgeColoring(Graph graph, out Graph coloredGraph) => graph.StrongEdgeColoring(out coloredGraph);
    }

    // Uwagi do wszystkich metod
    // 1) Grafy wynikowe powinny być reprezentowane w taki sam sposób jak grafy będące parametrami
    // 2) Grafów będących parametrami nie wolno zmieniać
    static class Lab03
    {
 
        // 0.5 pkt
        // Funkcja zwracajaca kwadrat grafu graph.
        // Kwadratem grafu nazywamy graf o takim samym zbiorze wierzcholkow jak graf bazowy,
        // 2 wierzcholki polaczone sa krawedzia jesli w grafie bazowym byly polaczone krawedzia badz sciezka zlozona z 2 krawedzi
        public static Graph SquareOfGraph(this Graph graph)
        {
            Graph g = graph.IsolatedVerticesGraph();
            int cc;
 
            Predicate<Edge> visitEdge = delegate (Edge e)
            {
                g.AddEdge(e);
                foreach (var e2 in graph.OutEdges(e.To))
                {
                    if (e.From != e2.To)
                        g.AddEdge(e.From, e2.To);
                }
 
                return true;
            };
 
            GeneralSearchGraphExtender.GeneralSearchAll<EdgesQueue>(graph, null, null, visitEdge, out cc);
 
            return g;
        }
 
        // 2 pkt
        // Funkcja zwracająca Graf krawedziowy grafu graph
        // Wierzcholki grafu krawedziwego odpowiadaja krawedziom grafu bazowego,
        // 2 wierzcholki grafu krawedziwego polaczone sa krawedzia
        // jesli w grafie bazowym z krawędzi odpowiadającej pierwszemu z nic można przejść
        // na krawędź odpowiadającą drugiemu z nich przez wspólny wierzchołek.
        //
        // (w grafie skierowanym: 2 wierzcholki grafu krawedziwego polaczone sa krawedzia
        // jesli wierzcholek koncowy krawedzi odpowiadajacej pierwszemu z nich
        // jest wierzcholkiem poczatkowym krawedzi odpowiadajacej drugiemu z nich)
        //
        // do tablicy names nalezy wpisac numery wierzcholkow grafu krawedziowego,
        // np. dla wierzcholka powstalego z krawedzi <0,1> do tabeli zapisujemy krotke (0, 1) - przyda się w dalszych etapach
        //
        // UWAGA: Graf bazowy może być skierowany lub nieskierowany, graf krawędziowy zawsze jest nieskierowany.
        public static Graph LineGraph(this Graph graph, out (int x, int y)[] names)
        {
            // Moze warto stworzyc...
            // graf pomocniczy o takiej samej strukturze krawedzi co pierwotny,
            // waga krawedzi jest numer krawedzi w grafie (taka sztuczka - to beda numery wierzcholkow w grafie krawedzowym)
            Graph g = graph.IsolatedVerticesGraph();
            Graph h;
            (int x, int y)[] tmpNames = new(int x, int y)[graph.EdgesCount];
            int cc = 0, edgeInd = 0;
 
            bool addedEdge = false;
            Predicate<Edge> visitEdgeBuildG = delegate (Edge e)
            {
                //build a copy of graph with edges weight as identifiers for vertices in line grap
                addedEdge = g.AddEdge(e.From, e.To, edgeInd);
                if (addedEdge)
                {
                    tmpNames[edgeInd++] = (e.From, e.To);
                }
 
                return true;
            };
 
            GeneralSearchGraphExtender.GeneralSearchAll<EdgesStack>(graph, null, null, visitEdgeBuildG, out cc);
 
            h = graph.IsolatedVerticesGraph(false, graph.EdgesCount); //UNDIRECTED!!!
 
            Predicate<Edge> visitEdgeBuildH = delegate (Edge e)
            {
                //build line graph from edges with weights
                foreach (var edge in g.OutEdges(e.To))
                {
                    if ((int)edge.Weight != (int)e.Weight)
                    {
                        h.AddEdge((int)e.Weight, (int)edge.Weight);
                    }
                }
 
                return true;
            };
 
            GeneralSearchGraphExtender.GeneralSearchAll<EdgesStack>(g, null, null, visitEdgeBuildH, out cc);
 
            names = tmpNames;
            return h;
        }
 
        // 1 pkt
        // Funkcja znajdujaca poprawne kolorowanie wierzcholkow grafu graph
        // Kolorowanie wierzcholkow jest poprawne, gdy kazde dwa sasiadujace wierzcholki maja rozne kolory
        // Funkcja ma szukać kolorowania wedlug nastepujacego algorytmu zachlannego:
        //
        // Dla wszystkich wierzcholkow (od 0 do n-1)
        //      pokoloruj wierzcholek v na najmniejszy mozliwy kolor (czyli taki, na ktory nie sa pomalowani jego sasiedzi)
        //
        // Nalezy zwrocic liczbe kolorow, a w tablicy colors zapamietac kolory dla poszczegolnych wierzcholkow
        //
        // UWAGA: Dla grafów skierowanych metoda powinna zgłaszać wyjątek ArgumentException
        public static int VertexColoring(this Graph graph, out int[] colors)
        {
            if (graph.Directed)
                throw new ArgumentException();
 
            if (0 == graph.VerticesCount)
            {
                colors = new int[0];
                return 0;
            }
 
            int maxColour = 0;
            int[] vertexColors = new int[graph.VerticesCount];
 
            for (int i = 0; i < vertexColors.Length; i++)
            {
                vertexColors[i] = -1;   //vertex not colored yet
            }
 
            int[] usedColors = new int[graph.VerticesCount];
            int color = 0;
 
            for (int v = 0; v < graph.VerticesCount; v++)
            {
                Array.Clear(usedColors, 0, usedColors.Length);
 
                color = 0;
                foreach (var e in graph.OutEdges(v))
                {
                    if (-1 != vertexColors[e.To])
                    {
                        color = vertexColors[e.To];
                        usedColors[color] = 1;     //colour used
                    }
                }
 
                for (int i = 0; i < usedColors.Length; i++)
                {
                    if (0 == usedColors[i])
                    {
                        vertexColors[v] = i;
 
                        if (i > maxColour)  //for count of all colours
                        {
                            maxColour = i;
                        }
 
                        break;
                    }
                }
            }
 
            colors = vertexColors;
            return maxColour + 1;
        }
 
        // 0.5 pkt
        // Funkcja znajdujaca silne kolorowanie krawedzi grafu graph
        // Silne kolorowanie krawedzi grafu jest poprawne gdy kazde dwie krawedzie, ktore sa ze soba sasiednie
        // albo sa polaczone inna krawedzia, maja rozne kolory.
        //
        // Nalezy zwrocic nowy graf, ktory bedzie kopia zadanego grafu, ale w wagach krawedzi zostana zapisane znalezione kolory
        //
        // Wskazowka - to bardzo proste. Nalezy tu wykorzystac wszystkie poprzednie funkcje.
        // Zastanowic sie co mozemy powiedziec o kolorowaniu wierzcholkow kwadratu grafu krawedziowego - jak sie ma do silnego kolorowania krawedzi grafu bazowego
        public static int StrongEdgeColoring(this Graph graph, out Graph coloredGraph)
        {
            (int x, int y)[] names;
            int[] colors;
            int colorsCount = 0;
 
            if (0 == graph.EdgesCount)
            {
                coloredGraph = graph;
                return 0;
            }
 
            Graph lineGraph = LineGraph(graph, out names);
            Graph lineGraphSquared = SquareOfGraph(lineGraph);
            colorsCount = VertexColoring(lineGraphSquared, out colors);
 
            Graph resultingGraph = graph.IsolatedVerticesGraph(graph.Directed, graph.VerticesCount);
 
            for (int i = 0; i < graph.EdgesCount; i++)
            {
                resultingGraph.AddEdge(names[i].x, names[i].y, colors[i]);
            }
 
            coloredGraph = resultingGraph;
            return colorsCount;
        }
    }
}

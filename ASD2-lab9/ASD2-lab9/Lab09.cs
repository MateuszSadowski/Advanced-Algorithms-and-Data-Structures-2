using ASD.Graphs;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ASD
{

    // DEFINICJA
    // Skojarzeniem indukowanym grafu G nazywamy takie skojarzenie M,
    // ze żadne dwa konce roznych krawedzi z M nie sa polaczone krawedzia w G

    // Uwagi do obu metod
    // 1) Grafow bedacych parametrami nie wolno zmieniac
    // 2) Parametrami są zawsze grafy nieskierowane (nie trzeba tego sprawdzac)

    public class Lab09 : MarshalByRefObject
    {

        /// <summary>
        /// Funkcja znajduje dowolne skojarzenie indukowane o rozmiarze k w grafie graph
        /// </summary>
        /// <param name="graph">Badany graf nieskierowany</param>
        /// <param name="k">Rozmiar szukanego skojarzenia indukowanego</param>
        /// <param name="matching">Znalezione skojarzenie (lub null jeśli nie ma)</param>
        /// <returns>true jeśli znaleziono skojarzenie, false jesli nie znaleziono</returns>
        /// <remarks>
        /// Punktacja:  2 pkt, w tym
        ///     1.5  -  dzialajacy algorytm (testy podstawowe)
        ///     0.5  -  testy wydajnościowe
        /// </remarks>
        /// 

        private int[] verticesStatus;
        private Graph inputGraph;
        private int goalMatchingSize;
        private Edge[] inducedMatching;
        private Edge[] edges;
        private int resultMatchingSize;
        public bool InducedMatching(Graph graph, int k, out Edge[] matching)
        {
            verticesStatus = new int[graph.VerticesCount];
            inputGraph = graph;
            goalMatchingSize = k;
            edges = new Edge[graph.EdgesCount];
            matching = inducedMatching = null;

            int edgesCount = 0;
            for (int v = 0; v < graph.VerticesCount; v++)
            {
                foreach (var e in graph.OutEdges(v))
                {
                    if (e.From < e.To)
                        edges[edgesCount++] = e;
                }
            }

            //GraphExport ge = new GraphExport();
            //ge.Export(graph);

            bool foundMatching = InducedMatchingUtil(new Edge[graph.EdgesCount], 0);

            if(foundMatching)
            {
                matching = new Edge[resultMatchingSize];
                for (int i = 0; i < resultMatchingSize; i++)
                {
                    matching[i] = inducedMatching[i];
                }
            }

            return foundMatching;
        }

        private bool InducedMatchingUtil(Edge[] matching, int matchingSize)
        {
            if(matchingSize == goalMatchingSize)
            {   //is solution
                inducedMatching = matching;
                resultMatchingSize = matchingSize;
                return true;    //finish
            }

            //cut condition
            //if cannot add more edges, return

            //TODO: start from edge from the upper lever
            foreach (var e in edges)
            {
                if (verticesStatus[e.From] > 0 || verticesStatus[e.To] > 0)
                    continue;

                //Add e to matching
                verticesStatus[e.From] = Int32.MaxValue; 
                verticesStatus[e.To] = Int32.MaxValue;

                matching[matchingSize] = e;

                foreach (var e2 in inputGraph.OutEdges(e.From))
                {
                    if (verticesStatus[e2.To] > 0)
                        continue;    

                    verticesStatus[e2.To] = e.From + 1;  
                }

                foreach (var e2 in inputGraph.OutEdges(e.To))
                {
                    if (verticesStatus[e2.To] > 0)
                        continue;

                    verticesStatus[e2.To] = e.To + 1; 
                }

                //state viable condition
                bool foundMatching = InducedMatchingUtil(matching, matchingSize + 1);   //Add another edge
                if (foundMatching)
                    return true;

                //Remove e from matching
                verticesStatus[e.From] = 0; // 0 -> vertex unprocessed
                verticesStatus[e.To] = 0;

                //matching[matchingSize] = null;  //TODO: how to remove edge?

                foreach (var e2 in inputGraph.OutEdges(e.From))
                {
                    if(verticesStatus[e2.To] == e.From + 1)
                        verticesStatus[e2.To] = 0;
                }

                foreach (var e2 in inputGraph.OutEdges(e.To))
                {
                    if (verticesStatus[e2.To] == e.To + 1)
                        verticesStatus[e2.To] = 0;
                }

                //Try next edge
            } 

            return false;
        }

        /// <summary>
        /// Funkcja znajduje skojarzenie indukowane o maksymalnej sumie wag krawedzi w grafie graph
        /// </summary>
        /// <param name="graph">Badany graf nieskierowany</param>
        /// <param name="matching">Znalezione skojarzenie (jeśli puste to tablica 0-elementowa)</param>
        /// <returns>Waga skojarzenia</returns>
        /// <remarks>
        /// Punktacja:  2 pkt, w tym
        ///     1.5  -  dzialajacy algorytm (testy podstawowe)
        ///     0.5  -  testy wydajnościowe
        /// </remarks>
        /// 

        private double maxWeight;
        private double tmpWeight;
        private double sumMaxWeight;
        private double weightInLeftEdges;
        public double MaximalInducedMatching(Graph graph, out Edge[] matching)
        {
            verticesStatus = new int[graph.VerticesCount];
            inputGraph = graph;
            edges = new Edge[graph.EdgesCount];
            matching = inducedMatching = null;

            maxWeight = Int32.MinValue;
            tmpWeight = 0;
            sumMaxWeight = 0;

            int edgesCount = 0;
            for (int v = 0; v < graph.VerticesCount; v++)
            {
                foreach (var e in graph.OutEdges(v))
                {
                    if (e.From < e.To)
                    {
                        edges[edgesCount++] = e;
                        sumMaxWeight += e.Weight;
                    }
                }
            }

            weightInLeftEdges = sumMaxWeight;

            //GraphExport ge = new GraphExport();
            //ge.Export(graph);

            while(InducedMatching)
            MaximalInducedMatchingUtil(new Edge[graph.EdgesCount], 0);

                matching = new Edge[resultMatchingSize];
                for (int i = 0; i < resultMatchingSize; i++)
                {
                    matching[i] = inducedMatching[i];
                }

            return maxWeight;
        }

        private bool MaximalInducedMatchingUtil(Edge[] matching, int matchingSize)
        {
            if (matchingSize == goalMatchingSize)
            {   //is solution
                inducedMatching = matching;
                resultMatchingSize = matchingSize;
                return true;    //finish
            }

            //cut condition
            //if cannot add more edges, return

            //TODO: start from edge from the upper lever
            foreach (var e in edges)
            {
                if (verticesStatus[e.From] > 0 || verticesStatus[e.To] > 0)
                    continue;

                //Add e to matching
                verticesStatus[e.From] = Int32.MaxValue;
                verticesStatus[e.To] = Int32.MaxValue;

                matching[matchingSize] = e;

                foreach (var e2 in inputGraph.OutEdges(e.From))
                {
                    if (verticesStatus[e2.To] > 0)
                        continue;

                    verticesStatus[e2.To] = e.From + 1;
                }

                foreach (var e2 in inputGraph.OutEdges(e.To))
                {
                    if (verticesStatus[e2.To] > 0)
                        continue;

                    verticesStatus[e2.To] = e.To + 1;
                }

                //state viable condition
                bool foundMatching = InducedMatchingUtil(matching, matchingSize + 1);   //Add another edge
                if (foundMatching)
                    return true;

                //Remove e from matching
                verticesStatus[e.From] = 0; // 0 -> vertex unprocessed
                verticesStatus[e.To] = 0;

                //matching[matchingSize] = null;  //TODO: how to remove edge?

                foreach (var e2 in inputGraph.OutEdges(e.From))
                {
                    if (verticesStatus[e2.To] == e.From + 1)
                        verticesStatus[e2.To] = 0;
                }

                foreach (var e2 in inputGraph.OutEdges(e.To))
                {
                    if (verticesStatus[e2.To] == e.To + 1)
                        verticesStatus[e2.To] = 0;
                }

                //Try next edge
            }

            return false;
        }

        //private void MaximalInducedMatchingUtil(Edge[] matching, int matchingSize)
        //{
        //    if (tmpWeight > maxWeight)
        //    {   //is solution
        //        inducedMatching = (Edge[])matching.Clone();
        //        resultMatchingSize = matchingSize;
        //        maxWeight = tmpWeight;
        //    }

        //    //cut condition
        //    //if cannot add more edges, return

        //    //TODO: start from edge from the upper level
        //    for(int i = matchingSize; i < edges.Length; i++)
        //    {
        //        Edge e = edges[i];
        //        if (verticesStatus[e.From] > 0 || verticesStatus[e.To] > 0)
        //            continue;

        //        //Add e to matching
        //        verticesStatus[e.From] = Int32.MaxValue;
        //        verticesStatus[e.To] = Int32.MaxValue;

        //        matching[matchingSize] = e;
        //        tmpWeight += e.Weight;
        //        weightInLeftEdges -= e.Weight;

        //        foreach (var e2 in inputGraph.OutEdges(e.From))
        //        {
        //            if (verticesStatus[e2.To] > 0)
        //                continue;

        //            verticesStatus[e2.To] = e.From + 1;
        //            weightInLeftEdges -= e2.Weight;
        //        }

        //        foreach (var e2 in inputGraph.OutEdges(e.To))
        //        {
        //            if (verticesStatus[e2.To] > 0)
        //                continue;

        //            verticesStatus[e2.To] = e.To + 1;
        //            weightInLeftEdges -= e2.Weight;
        //        }

        //        //state viable condition
        //        if(tmpWeight + weightInLeftEdges > maxWeight)
        //            MaximalInducedMatchingUtil(matching, matchingSize + 1);   //Add another edge

        //        //Remove e from matching
        //        verticesStatus[e.From] = 0; // 0 -> vertex unprocessed
        //        verticesStatus[e.To] = 0;

        //        //matching[matchingSize] = null;  //TODO: how to remove edge?
        //        tmpWeight -= e.Weight;
        //        weightInLeftEdges += e.Weight;

        //        foreach (var e2 in inputGraph.OutEdges(e.From))
        //        {
        //            if (verticesStatus[e2.To] == e.From + 1)
        //            {
        //                verticesStatus[e2.To] = 0;
        //                weightInLeftEdges += e2.Weight;
        //            }
        //        }

        //        foreach (var e2 in inputGraph.OutEdges(e.To))
        //        {
        //            if (verticesStatus[e2.To] == e.To + 1)
        //            {
        //                verticesStatus[e2.To] = 0;
        //                weightInLeftEdges += e2.Weight;
        //            }
        //        }

        //        //Try next edge
        //    }

        //    return;
        //}

        //funkcje pomocnicze

    }
}



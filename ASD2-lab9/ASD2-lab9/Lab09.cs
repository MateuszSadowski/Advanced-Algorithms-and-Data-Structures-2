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

        internal Graph inputGraph;
        internal int targetMatchingSize;

        internal Edge[] resultMatching;

        internal int tmpMatchingSize;
        public bool InducedMatching(Graph graph, int k, out Edge[] matching)
        {
            inputGraph = graph;
            targetMatchingSize = k;

            resultMatching = null;

            tmpMatchingSize = 0;

            bool foundMatching = InducedMatchingUtil(0, new Edge[graph.EdgesCount], new bool[graph.VerticesCount]);

            matching = resultMatching;
            return foundMatching;
        }

        internal bool InducedMatchingUtil(int from, Edge[] matching, bool[] verticesProcessed)
        {
            if(tmpMatchingSize == targetMatchingSize)
            {
                resultMatching = new Edge[tmpMatchingSize];
                Array.Copy(matching, resultMatching, tmpMatchingSize);
                return true;
            }

            for (int v = from; v < inputGraph.VerticesCount; v++)
            {
                if (verticesProcessed[v])
                    continue;

                foreach (var e in inputGraph.OutEdges(v))
                {
                    if (e.From > e.To)  //graph directed, check only edges one direction
                        continue;

                    if (verticesProcessed[e.To]) //vertex not viable
                        continue;

                    //add edge to matching
                    matching[tmpMatchingSize++] = e;
                    bool[] tmpVerticesProcessed = new bool[inputGraph.VerticesCount];
                    Array.Copy(verticesProcessed, tmpVerticesProcessed, verticesProcessed.Length);
                    tmpVerticesProcessed[e.From] = tmpVerticesProcessed[e.To] = true;

                    foreach (var e2 in inputGraph.OutEdges(e.From))
                    {
                        tmpVerticesProcessed[e2.To] = true;
                    }

                    foreach (var e2 in inputGraph.OutEdges(e.To))
                    {
                        tmpVerticesProcessed[e2.To] = true;
                    }

                    bool success = InducedMatchingUtil(v + 1, matching, tmpVerticesProcessed);
                    if (success)
                        return true;

                    //remove edge from matching
                    tmpMatchingSize--;
                }
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
        internal double maxWeight;

        internal double tmpWeight;
        public double MaximalInducedMatching(Graph graph, out Edge[] matching)
        {
            inputGraph = graph;
            maxWeight = Int32.MinValue;
            tmpWeight = 0;

            resultMatching = null;

            tmpMatchingSize = 0;

            MaximalInducedMatchingUtil(0, new Edge[graph.EdgesCount], new bool[graph.VerticesCount]);

            matching = resultMatching;
            return maxWeight;
        }

        //funkcje pomocnicze
        internal void MaximalInducedMatchingUtil(int from, Edge[] matching, bool[] verticesProcessed)
        {
            if (tmpWeight > maxWeight)
            {
                resultMatching = new Edge[tmpMatchingSize]; 
                Array.Copy(matching, resultMatching, tmpMatchingSize);
                maxWeight = tmpWeight;
            }

            for (int v = from; v < inputGraph.VerticesCount; v++)
            {
                if (verticesProcessed[v])
                    continue;

                foreach (var e in inputGraph.OutEdges(v))
                {
                    if (e.From > e.To)  //graph directed, check only edges one direction
                        continue;

                    if (verticesProcessed[e.To]) //vertex not viable
                        continue;

                    //add edge to matching
                    matching[tmpMatchingSize++] = e;
                    tmpWeight += e.Weight;
                    bool[] tmpVerticesProcessed = new bool[inputGraph.VerticesCount];
                    Array.Copy(verticesProcessed, tmpVerticesProcessed, verticesProcessed.Length);
                    tmpVerticesProcessed[e.From] = tmpVerticesProcessed[e.To] = true;

                    foreach (var e2 in inputGraph.OutEdges(e.From))
                    {
                        tmpVerticesProcessed[e2.To] = true;
                    }

                    foreach (var e2 in inputGraph.OutEdges(e.To))
                    {
                        tmpVerticesProcessed[e2.To] = true;
                    }

                    MaximalInducedMatchingUtil(v + 1, matching, tmpVerticesProcessed);

                    //remove edge from matching
                    tmpMatchingSize--;
                    tmpWeight -= e.Weight;
                }
            }
        }
    }
}



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
        internal int resultMatchingSize;

        internal int tmpMatchingSize;
        internal bool[] tmpVertexProcessed;
        public bool InducedMatching(Graph graph, int k, out Edge[] matching)
        {
            inputGraph = graph;
            targetMatchingSize = k;

            resultMatching = null;
            resultMatchingSize = 0;

            tmpMatchingSize = 0;
            tmpVertexProcessed = new bool[graph.VerticesCount];

            bool foundMatching = InducedMatchingUtil(0, new Edge[graph.EdgesCount]);

            matching = resultMatching;
            return foundMatching;
        }

        internal bool InducedMatchingUtil(int nextVertex, Edge[] matching)
        {
            if(tmpMatchingSize == targetMatchingSize)
            {
                resultMatching = resizeMatchingArray(matching);
                return true;
            }

            for (int v = nextVertex; v < inputGraph.VerticesCount; v++)
            {
                if (tmpVertexProcessed[v])  //TODO: might be obsolete
                    continue;

                foreach (var e in inputGraph.OutEdges(v))
                {
                    if (e.From > e.To)  //graph directed, check only edges one direction
                        continue;

                    if (tmpVertexProcessed[e.To]) //vertex not viable
                        continue;

                    //add edge to matching
                    matching[tmpMatchingSize++] = e;
                    tmpVertexProcessed[e.From] = tmpVertexProcessed[e.To] = true;

                    foreach (var e2 in inputGraph.OutEdges(e.From))
                    {
                        tmpVertexProcessed[e2.To] = true;
                    }

                    foreach (var e2 in inputGraph.OutEdges(e.To))
                    {
                        tmpVertexProcessed[e2.To] = true;
                    }

                    bool success = InducedMatchingUtil(v + 1, matching);
                    if (success)
                        return true;

                    //remove edge and try next

                    foreach (var e2 in inputGraph.OutEdges(e.From))
                    {
                        tmpVertexProcessed[e2.To] = false;
                    }

                    foreach (var e2 in inputGraph.OutEdges(e.To))
                    {
                        tmpVertexProcessed[e2.To] = false;
                    }

                    tmpVertexProcessed[e.From] = tmpVertexProcessed[e.To] = false;
                    tmpMatchingSize--;
                }
            }

            return false;
        }

        internal Edge[] resizeMatchingArray(Edge[] matching)
        {
            //if (tmpMatchingSize == 0)
            //    return null;

            resultMatchingSize = tmpMatchingSize;
            Edge[] result = new Edge[resultMatchingSize];
            Array.Copy(matching, result, resultMatchingSize);
            return result;
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
        public double MaximalInducedMatching(Graph graph, out Edge[] matching)
        {
            matching = null;
            return 0;
        }

        //funkcje pomocnicze

    }
}



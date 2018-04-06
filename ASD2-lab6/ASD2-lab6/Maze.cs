using ASD.Graphs;
using System;
using System.Linq;
using System.Text;

namespace ASD
{
    public class Maze : MarshalByRefObject
    {

        /// <summary>
        /// Wersje zadania I oraz II
        /// Zwraca najkrótszy możliwy czas przejścia przez labirynt bez dynamitów lub z dowolną ich liczbą
        /// </summary>
        /// <param name="maze">labirynt</param>
        /// <param name="withDynamite">informacja, czy dostępne są dynamity 
        /// Wersja I zadania -> withDynamites = false, Wersja II zadania -> withDynamites = true</param>
        /// <param name="path">zwracana ścieżka</param>
        /// <param name="t">czas zburzenia ściany (dotyczy tylko wersji II)</param> 
        /// 

        internal char[,] tmpMaze;
        
        public int FindShortestPath(char[,] maze, bool withDynamite, out string path, int t = 0)
        {
            int minDistance = Int32.MaxValue;
            PathsInfo[] pathsInfo = new PathsInfo[1];
            int start = -1;
            int end = -1;
            int height = maze.GetLength(0);
            int width = maze.GetLength(1);
            Graph graph = new AdjacencyListsGraph<SimpleAdjacencyList>(true, maze.GetLength(0) * maze.GetLength(1));
            tmpMaze = maze;

            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    //check for start or end
                    if(start == -1 || end == -1)
                    {
                        if (maze[i, j] == 'S')
                        {
                            start = i * width + j;
                        }
                        else if (maze[i, j] == 'E')
                        {
                            end = i * width + j;
                        }
                    }

                    if(!withDynamite && maze[i, j] == 'X')
                    {
                        continue;
                    }

                    //we have dynamite or we are on 'O'
                        //check to bottom
                        if(withDynamite && i + 1 < height && isX(i, j) && isX(i+1, j))
                        {   //connect X with X
                            int from = i * width + j;
                            int to = (i + 1) * width + j;
                            graph.AddEdge(from, to, t);
                            graph.AddEdge(to, from, t);
                        }
                        else if (withDynamite && i + 1 < height && isX(i, j) && !isX(i + 1, j))
                        {   //connect X with O
                            int from = i * width + j;
                            int to = (i + 1) * width + j;
                            graph.AddEdge(from, to, 1);
                            graph.AddEdge(to, from, t);
                        }
                        else if (i + 1 < height && !isX(i + 1, j))
                        {   //connect O with O
                            int from = i * width + j;
                            int to = (i + 1) * width + j;
                            graph.AddEdge(from, to, 1);
                            graph.AddEdge(to, from, 1);
                        }
                        else if (withDynamite && i + 1 < height && isX(i + 1, j))
                        {   //connect O with X
                            int from = i * width + j;
                            int to = (i + 1) * width + j;
                            graph.AddEdge(from, to, t);
                            graph.AddEdge(to, from, 1);
                        }

                    //check to right
                        if (withDynamite && j + 1 < width && isX(i, j) && isX(i, j + 1))
                        {   //connect X with X
                            int from = i * width + j;
                            int to = i * width + j + 1;
                            graph.AddEdge(from, to, t);
                            graph.AddEdge(to, from, t);
                        }
                        else if (withDynamite && j + 1 < width && isX(i, j) && !isX(i, j + 1))
                        {   //connect X with O
                            int from = i * width + j;
                            int to = i * width + j + 1;
                            graph.AddEdge(from, to, 1);
                            graph.AddEdge(to, from, t);
                        }
                        else if (j + 1 < width && !isX(i, j + 1))
                        {   //connect O with O
                            int from = i * width + j;
                            int to = i * width + j + 1;
                            graph.AddEdge(from, to, 1);
                            graph.AddEdge(to, from, 1);
                        }
                        else if (withDynamite && j + 1 < width && isX(i, j + 1))
                        {   //connect O with X
                            int from = i * width + j;
                            int to = i * width + j + 1;
                            graph.AddEdge(from, to, t);
                            graph.AddEdge(to, from, 1);
                        }
                }
                }

            //search for path
            graph.DijkstraShortestPaths(start, out pathsInfo);

            //GraphExport ge = new GraphExport();
            //ge.Export(graph);

            if (pathsInfo[end].Dist.IsNaN())
            {
                path = "";
                return -1;
            }

            Edge[] pathToEnd = PathsInfo.ConstructPath(start, end, pathsInfo);
            string pathAsChars = "";

            foreach (var edge in pathToEnd)
            {
                char? c = getDirection(edge, width);
                if(c == null)
                {
                    Console.WriteLine("Error getting path.");
                    break;
                }
                pathAsChars += c;
            }

            minDistance = (int)pathsInfo[end].Dist;
            path = pathAsChars; // tej linii na laboratorium nie zmieniamy!
            return minDistance;
        }

        internal bool isX(int height, int width)
        {
            return tmpMaze[height, width] == 'X';
        }

        internal char? getDirection(Edge edge, int width)
        {
            int directonIndicator = edge.To - edge.From;
            if(directonIndicator == 1)
            {
                return 'E';
            }
            else if(directonIndicator == -1)
            {
                return 'W';
            }
            else if (directonIndicator == width)
            {
                return 'S';
            }
            else if (directonIndicator == -width)
            {
                return 'N';
            }

            return null;
        }

        /// <summary>
        /// Wersja III i IV zadania
        /// Zwraca najkrótszy możliwy czas przejścia przez labirynt z użyciem co najwyżej k lasek dynamitu
        /// </summary>
        /// <param name="maze">labirynt</param>
        /// <param name="k">liczba dostępnych lasek dynamitu, dla wersji III k=1</param>
        /// <param name="path">zwracana ścieżka</param>
        /// <param name="t">czas zburzenia ściany</param>
        public int FindShortestPathWithKDynamites(char[,] maze, int k, out string path, int t)
        {
            int minDistance = Int32.MaxValue;
            PathsInfo[] pathsInfo = new PathsInfo[1];
            int start = -1;
            int end = -1;
            int height = maze.GetLength(0);
            int width = maze.GetLength(1);
            //int k = 1;  //number of X's
            Graph graph = new AdjacencyListsGraph<SimpleAdjacencyList>(false, maze.GetLength(0) * maze.GetLength(1));

            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    //check for start or end
                    if (start == -1 || end == -1)
                    {
                        if (maze[i, j] == 'S')
                        {
                            start = i * width + j;
                        }
                        else if (maze[i, j] == 'E')
                        {
                            end = i * width + j;
                        }
                    }

                    //check to bottom
                    if (i + 1 < height && maze[i + 1, j] != 'X')
                    {
                        int from = i * width + j;
                        int to = (i + 1) * width + j;
                        graph.AddEdge(from, to, 1);
                    }
                    else if (i + 1 < height && maze[i + 1, j] == 'X')
                    {
                        int from = i * width + j;
                        int to = (i + 1) * width + j;
                        graph.AddEdge(from, to, t);
                    }

                    //check to right
                    if (j + 1 < width && maze[i, j + 1] != 'X')
                    {
                        int from = i * width + j;
                        int to = i * width + j + 1;
                        graph.AddEdge(from, to, 1);
                    }
                    else if (j + 1 < width && maze[i, j + 1] == 'X')
                    {
                        int from = i * width + j;
                        int to = i * width + j + 1;
                        graph.AddEdge(from, to, t);
                    }
                }
            }

            //search for path
            graph.DijkstraShortestPaths(start, out pathsInfo);

            //GraphExport ge = new GraphExport();
            //ge.Export(graph);

            if (pathsInfo[end].Dist.IsNaN())
            {
                path = null;
                return -1;
            }
            minDistance = (int)pathsInfo[end].Dist;

            path = null; // tej linii na laboratorium nie zmieniamy!
            return 0;
        }
        
    }
}
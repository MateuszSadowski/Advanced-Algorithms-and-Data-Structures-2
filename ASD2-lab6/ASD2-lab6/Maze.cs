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
        internal int mazeHeight;
        internal int mazeWidth;
        internal int cellCount;
        internal int throughWallCost;
        internal Graph graph;

        public int FindShortestPath(char[,] maze, bool withDynamite, out string path, int t = 0)
        {
            int minDistance = Int32.MaxValue;
            PathsInfo[] pathsInfo = new PathsInfo[1];
            int start = -1;
            int end = -1;
            mazeHeight = maze.GetLength(0);
            mazeWidth = maze.GetLength(1);
            Graph graph = new AdjacencyListsGraph<SimpleAdjacencyList>(true, maze.GetLength(0) * maze.GetLength(1));
            tmpMaze = maze;
            throughWallCost = t;

            for (int i = 0; i < mazeHeight; i++)
            {
                for (int j = 0; j < mazeWidth; j++)
                {
                    //check for start or end
                    if(start == -1 || end == -1)
                    {
                        if (maze[i, j] == 'S')
                        {
                            start = i * mazeWidth + j;
                        }
                        else if (maze[i, j] == 'E')
                        {
                            end = i * mazeWidth + j;
                        }
                    }

                    if(!withDynamite && maze[i, j] == 'X')
                    {
                        continue;
                    }

                    //we have dynamite or we are on 'O'
                        //check to bottom
                        if(withDynamite && i + 1 < mazeHeight && isX(i, j) && isX(i+1, j))
                        {   //connect X with X
                            int from = i * mazeWidth + j;
                            int to = (i + 1) * mazeWidth + j;
                            graph.AddEdge(from, to, t);
                            graph.AddEdge(to, from, t);
                        }
                        else if (withDynamite && i + 1 < mazeHeight && isX(i, j) && !isX(i + 1, j))
                        {   //connect X with O
                            int from = i * mazeWidth + j;
                            int to = (i + 1) * mazeWidth + j;
                            graph.AddEdge(from, to, 1);
                            graph.AddEdge(to, from, t);
                        }
                        else if (i + 1 < mazeHeight && !isX(i + 1, j))
                        {   //connect O with O
                            int from = i * mazeWidth + j;
                            int to = (i + 1) * mazeWidth + j;
                            graph.AddEdge(from, to, 1);
                            graph.AddEdge(to, from, 1);
                        }
                        else if (withDynamite && i + 1 < mazeHeight && isX(i + 1, j))
                        {   //connect O with X
                            int from = i * mazeWidth + j;
                            int to = (i + 1) * mazeWidth + j;
                            graph.AddEdge(from, to, t);
                            graph.AddEdge(to, from, 1);
                        }

                    //check to right
                        if (withDynamite && j + 1 < mazeWidth && isX(i, j) && isX(i, j + 1))
                        {   //connect X with X
                            int from = i * mazeWidth + j;
                            int to = i * mazeWidth + j + 1;
                            graph.AddEdge(from, to, t);
                            graph.AddEdge(to, from, t);
                        }
                        else if (withDynamite && j + 1 < mazeWidth && isX(i, j) && !isX(i, j + 1))
                        {   //connect X with O
                            int from = i * mazeWidth + j;
                            int to = i * mazeWidth + j + 1;
                            graph.AddEdge(from, to, 1);
                            graph.AddEdge(to, from, t);
                        }
                        else if (j + 1 < mazeWidth && !isX(i, j + 1))
                        {   //connect O with O
                            int from = i * mazeWidth + j;
                            int to = i * mazeWidth + j + 1;
                            graph.AddEdge(from, to, 1);
                            graph.AddEdge(to, from, 1);
                        }
                        else if (withDynamite && j + 1 < mazeWidth && isX(i, j + 1))
                        {   //connect O with X
                            int from = i * mazeWidth + j;
                            int to = i * mazeWidth + j + 1;
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
                char? c = getDirection(edge, mazeWidth);
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
            if(width != 1 && directonIndicator == 1)
            {
                return 'E';
            }
            else if(width != 1 && directonIndicator == -1)
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
        /// 

        public int FindShortestPathWithKDynamites(char[,] maze, int k, out string path, int t)
        {
            int minDistance = Int32.MaxValue;
            PathsInfo[] pathsInfo = new PathsInfo[1];
            int start = -1;
            int end = -1;
            mazeHeight = maze.GetLength(0);
            mazeWidth = maze.GetLength(1);
            cellCount = mazeHeight * mazeWidth;
            graph = new AdjacencyListsGraph<SimpleAdjacencyList>(true, (k + 1) * cellCount);
            tmpMaze = maze;
            throughWallCost = t;

            for (int layer = 0; layer < k + 1; layer++)
            {
                for (int height = 0; height < mazeHeight; height++)
                {
                    for (int width = 0; width < mazeWidth; width++)
                    {
                        if(start == -1)
                        {
                            if(tmpMaze[height, width] == 'S')
                            {
                                start = getCellNumber(height, width, 0);
                            }
                        }

                        if(end == -1)
                        {
                            if (tmpMaze[height, width] == 'E')
                            {
                                end = getCellNumber(height, width, 0);
                            }
                        }

                        int from = 0, to = 0;

                        //check to right
                        if(isInBoundsOnWidth(width + 1))
                        {
                            from = getCellNumber(height, width, layer);
                            to = getCellNumber(height, width + 1, layer);

                            //First layer, on X
                            if (layer == 0 && isX(height, width) && !isX(height, width + 1))
                            {   
                                //If there is O to right or bottom, add edge from O in current layer to X in next layer
                                if(k != 0)  //check if dynamite is available at all
                                {
                                    connectWithXInNextLayer(to, from);
                                }
                            }

                            //Whichever layer, on O
                            if(!isX(height, width) && !isX(height, width + 1))
                            {
                                connectOwithO(from, to);
                            }

                            //Not last layer, on O
                            if (layer != k && !isX(height, width) && isX(height, width + 1))
                            {
                                connectWithXInNextLayer(from, to);
                            }

                            //Not first layer, on X
                            if(layer != 0 && isX(height, width) && !isX(height, width + 1))
                            {
                                connectXwithO(from, to);
                            }

                            //Not first, not last layer, on X
                            if (layer != 0 && layer != k && isX(height, width) && isX(height, width + 1))
                            {
                                connectWithXInNextLayer(from, to);
                            }
                        }

                        //check to bottom
                        if (isInBoundsOnHeight(height + 1))
                        {
                            from = getCellNumber(height, width, layer);
                            to = getCellNumber(height + 1, width, layer);

                            //First layer, on X
                            if (layer == 0 && isX(height, width) && !isX(height + 1, width))
                            {
                                //If there is O to right or bottom, add edge from O in current layer to X in next layer
                                if (k != 0)  //check if dynamite is available at all
                                {
                                    connectWithXInNextLayer(to, from);
                                }
                            }

                            //Whichever layer, on O
                            if (!isX(height, width) && !isX(height + 1, width))
                            {
                                connectOwithO(from, to);
                            }

                            //Not last layer, on O
                            if (layer != k && !isX(height, width) && isX(height + 1, width))
                            {
                                connectWithXInNextLayer(from, to);
                            }

                            //Not first layer, on X
                            if (layer != 0 && isX(height, width) && !isX(height + 1, width))
                            {
                                connectXwithO(from, to);
                            }

                            //Not first, not last layer, on X
                            if (layer != 0 && layer != k && isX(height, width) && isX(height + 1, width))
                            {
                                connectWithXInNextLayer(from, to);
                            }
                        }
                    }
                }
            }

            for (int layer = 0; layer < k + 1; layer++)
            {
                for (int height = 0; height < mazeHeight; height++)
                {
                    for (int width = 0; width < mazeWidth; width++)
                    {

                        int from = 0, to = 0;

                        //check to left
                        if (width - 1 >= 0)
                        {
                            from = getCellNumber(height, width, layer);
                            to = getCellNumber(height, width - 1, layer);

                            //First layer, on X
                            if (layer == 0 && isX(height, width) && !isX(height, width - 1))
                            {
                                //If there is O to right or bottom, add edge from O in current layer to X in next layer
                                if (k != 0)  //check if dynamite is available at all
                                {
                                    connectWithXInNextLayer(to, from);
                                }
                            }

                            //Whichever layer, on O
                            if (!isX(height, width) && !isX(height, width - 1))
                            {
                                connectOwithO(from, to);
                            }

                            //Not last layer, on O
                            if (layer != k && !isX(height, width) && isX(height, width - 1))
                            {
                                connectWithXInNextLayer(from, to);
                            }

                            //Not first layer, on X
                            if (layer != 0 && isX(height, width) && !isX(height, width - 1))
                            {
                                connectXwithO(from, to);
                            }

                            //Not first, not last layer, on X
                            if (layer != 0 && layer != k && isX(height, width) && isX(height, width - 1))
                            {
                                connectWithXInNextLayer(from, to);
                            }
                        }

                        //check to top
                        if (height - 1 >= 0)
                        {
                            from = getCellNumber(height, width, layer);
                            to = getCellNumber(height - 1, width, layer);

                            //First layer, on X
                            if (layer == 0 && isX(height, width) && !isX(height - 1, width))
                            {
                                //If there is O to right or bottom, add edge from O in current layer to X in next layer
                                if (k != 0)  //check if dynamite is available at all
                                {
                                    connectWithXInNextLayer(to, from);
                                }
                            }

                            //Whichever layer, on O
                            if (!isX(height, width) && !isX(height - 1, width))
                            {
                                connectOwithO(from, to);
                            }

                            //Not last layer, on O
                            if (layer != k && !isX(height, width) && isX(height - 1, width))
                            {
                                connectWithXInNextLayer(from, to);
                            }

                            //Not first layer, on X
                            if (layer != 0 && isX(height, width) && !isX(height - 1, width))
                            {
                                connectXwithO(from, to);
                            }

                            //Not first, not last layer, on X
                            if (layer != 0 && layer != k && isX(height, width) && isX(height - 1, width))
                            {
                                connectWithXInNextLayer(from, to);
                            }
                        }
                    }
                }
            }

            graph.DijkstraShortestPaths(start,out pathsInfo);
            int minEnd = -1;

            for (int dynamiteUsed = 0; dynamiteUsed < k + 1; dynamiteUsed++)
            {
                int endOnCurrentLayer = end + (dynamiteUsed * cellCount);
                if (!pathsInfo[endOnCurrentLayer].Dist.IsNaN())
                {
                    int distance = (int)pathsInfo[endOnCurrentLayer].Dist;
                    if(distance < minDistance)
                    {
                        minDistance = distance;
                        minEnd = endOnCurrentLayer;
                    }
                }
            }

            if(minEnd == -1)
            {
                path = "";
                return -1;
            }

            Edge[] pathToEnd = PathsInfo.ConstructPath(start, minEnd, pathsInfo);
            string pathAsChars = "";

            foreach (var edge in pathToEnd)
            {
                char? c = getDirectionWithDinamites(edge, mazeWidth);
                if (c == null)
                {
                    Console.WriteLine("Error getting path.");
                    break;
                }
                pathAsChars += c;
            }

            path = pathAsChars; // tej linii na laboratorium nie zmieniamy!
            return minDistance;
        }


        internal char? getDirectionWithDinamites(Edge edge, int width)
        {
            int directonIndicator;
            if (edge.Weight == throughWallCost)
            {
                directonIndicator = edge.To - edge.From - cellCount;
            }
            else
            {
                directonIndicator = edge.To - edge.From;
            }
            if (width != 1 && directonIndicator == 1)
            {
                return 'E';
            }
            else if (width != 1 && directonIndicator == -1)
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

        internal bool isInBoundsOnWidth(int width)
        {
            return width < mazeWidth;
        }

        internal bool isInBoundsOnHeight(int height)
        {
            return height < mazeHeight;
        }

        internal int getCellNumber(int i, int j, int layer)
        {
            return i * mazeWidth + j + (layer * cellCount);
        }

        internal void connectXwithX(int from, int to)
        {
            graph.AddEdge(from, to, throughWallCost);
            graph.AddEdge(to, from, throughWallCost);
        }

        internal void connectXwithO(int from, int to)
        {
            graph.AddEdge(from, to, 1);
        }

        internal void connectOwithO(int from, int to)
        {
            graph.AddEdge(from, to, 1);
            graph.AddEdge(to, from, 1);
        }

        internal void connectOwithX(int from, int to)
        {
            graph.AddEdge(from, to, throughWallCost);
            graph.AddEdge(to, from, 1);
        }

        internal void connectWithXInNextLayer(int from, int to)
        {
            to += cellCount;
            graph.AddEdge(from, to, throughWallCost);
        }
    }
}
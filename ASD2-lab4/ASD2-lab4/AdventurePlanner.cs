using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASD
{
    /// <summary>
    /// struktura przechowująca punkt
    /// </summary>
    [Serializable]
    public struct Point
    {
        public int X { get; set; }
        public int Y { get; set; }
        public Point (int x, int y)
        {
            X = x;
            Y = y;
        }
        public override string ToString()
        {
            return $"({X},{Y})";
        }
    }

    public class AdventurePlanner: MarshalByRefObject
    {
        /// <summary>
        /// największy rozmiar tablicy, którą wyświetlamy
        /// ustaw na 0, żeby nic nie wyświetlać
        /// </summary>
        public int MaxToShow = 0;

      
        /// <summary>
        /// Znajduje optymalną pod względem liczby znalezionych skarbów ścieżkę,
        /// zaczynającą się w lewym górnym rogu mapy (0,0), a kończącą się w prawym
        /// dolnym rogu (X-Y-1).
        /// Za każdym razem możemy wykonać albo krok w prawo albo krok w dół.
        /// Pierwszym polem ścieżki powinno być (0,0), a ostatnim polem (X-1,Y-1).        
        /// </summary>
        /// <param name="treasure">liczba znalezionych skarbów</param>
        /// <param name="path">znaleziona ścieżka</param>
        /// <remarks>
        /// Złożoność rozwiązania to O(X * Y).
        /// </remarks>
        /// <returns></returns>
        public int FindPathThere(int[,] treasure, out List<Point> path, out int[,] tmpTreasur)
        {
            int[,] tmpMaxTreasure = new int[treasure.GetLength(0), treasure.GetLength(1)];
            Point[,] tmpPath = new Point[treasure.GetLength(0), treasure.GetLength(1)];

            //might neet to check if board is not 0x0 or 0x1 ...
            tmpMaxTreasure[0, 0] = treasure[0, 0]; //base condition
            tmpPath[0, 0] = new Point(-1, -1); //sentiel
            bool setPath = false;

            for (int i = 0; i < tmpMaxTreasure.GetLength(0); i++)
            {
                for (int j = 0; j < tmpMaxTreasure.GetLength(1); j++)
                {
                    setPath = false;

                    if (0 == i && 0 == j)
                        continue;

                    if(0 == i)  //check only to the left
                    {
                        if (tmpMaxTreasure[i, j] < treasure[i, j] + tmpMaxTreasure[i, j - 1])
                        {
                            tmpMaxTreasure[i, j] = treasure[i, j] + tmpMaxTreasure[i, j - 1];
                            tmpPath[i, j] = new Point(i, j - 1); setPath = true;
                        }

                        if (!setPath)
                            tmpPath[i, j] = new Point(i, j - 1);

                        continue;
                    }

                    if(0 == j)  //check only to the top
                    {
                        if (tmpMaxTreasure[i, j] < treasure[i, j] + tmpMaxTreasure[i - 1, j])
                        {
                            tmpMaxTreasure[i, j] = treasure[i, j] + tmpMaxTreasure[i - 1, j];
                            tmpPath[i, j] = new Point(i - 1, j); setPath = true;
                        }

                        if (!setPath)
                            tmpPath[i, j] = new Point(i - 1, j);

                        continue;
                    }

                    //check both
                    if (tmpMaxTreasure[i, j] < treasure[i, j] + tmpMaxTreasure[i, j - 1])
                    {
                        tmpMaxTreasure[i, j] = treasure[i, j] + tmpMaxTreasure[i, j - 1];
                        tmpPath[i, j] = new Point(i, j - 1); setPath = true;
                    }

                    if (tmpMaxTreasure[i, j] < treasure[i, j] + tmpMaxTreasure[i - 1, j])
                    {
                        tmpMaxTreasure[i, j] = treasure[i, j] + tmpMaxTreasure[i - 1, j];
                        tmpPath[i, j] = new Point(i - 1, j); setPath = true;
                    }

                    if(!setPath)
                        tmpPath[i, j] = new Point(i, j - 1);
                }
            }

            path = new List<Point>();
            Stack<Point> points = new Stack<Point>();

            //reconstruct path
            int x = tmpMaxTreasure.GetLength(0) - 1, y = tmpMaxTreasure.GetLength(1) - 1;
            while (tmpPath[x, y].X != -1 || tmpPath[x, y].Y != -1)
            {
                Point point = tmpPath[x, y];
                points.Push(point);
                x = point.X; y = point.Y;
            }

            //path.Add(new Point(0, 0));
            while(points.Count > 0)
            {
                path.Add(points.Pop());
            }
            path.Add(new Point(tmpMaxTreasure.GetLength(0) - 1, tmpMaxTreasure.GetLength(1) - 1));

            tmpTreasur = tmpMaxTreasure;
            return tmpMaxTreasure[tmpMaxTreasure.GetLength(0) - 1, tmpMaxTreasure.GetLength(1) - 1];
        }

      
        /// <summary>
        /// Znajduje optymalną pod względem liczby znalezionych skarbów ścieżkę,
        /// zaczynającą się w lewym górnym rogu mapy (0,0), dochodzącą do prawego dolnego rogu (X-1,Y-1), a 
        /// następnie wracającą do lewego górnego rogu (0,0).
        /// W pierwszy etapie możemy wykonać albo krok w prawo albo krok w dół. Po osiągnięciu pola (x-1,Y-1)
        /// zacynamy wracać - teraz możemy wykonywać algo krok w prawo albo krok w górę.
        /// Pierwszym i ostatnim polem ścieżki powinno być (0,0).
        /// Możemy założyć, że X,Y >= 2.
        /// </summary>
        /// <param name="treasure">liczba znalezionych skarbów</param>
        /// <param name="path">znaleziona ścieżka</param>
        /// <remarks>
        /// Złożoność rozwiązania to O(X^2 * Y) lub O(X * Y^2).
        /// </remarks>
        /// <returns></returns>
        public int FindPathThereAndBack(int[,] treasure, out List<Point> path)
        {
            List < Point > path1 = new List<Point>();
            int[,] tmpTreasure;
            int value1 = FindPathThere(treasure, out path1, out tmpTreasure);

            int ind = 0;
            while(path1.Count > ind)
            {
                Point point = path1[ind++];
                tmpTreasure[point.X, point.Y] = 0;
            }

            int max = 0;

            if (tmpTreasure.GetLength(0) > 1 && tmpTreasure[tmpTreasure.GetLength(0) - 2, tmpTreasure.GetLength(1) - 1] > max)
                max = tmpTreasure[tmpTreasure.GetLength(0) - 2, tmpTreasure.GetLength(1) - 1];

            if (tmpTreasure.GetLength(1) > 1 && tmpTreasure[tmpTreasure.GetLength(0) - 1, tmpTreasure.GetLength(1) - 2] > max)
                max = tmpTreasure[tmpTreasure.GetLength(0) - 1, tmpTreasure.GetLength(1) - 2];


            path = new List<Point>();
            path.Add(new Point());
            return max + value1;
        }
    }
}

using System;
using System.Collections.Generic;
using ASD.Graphs;

namespace asd2
{
    public class City : MarshalByRefObject
    {
        /// <summary>
        /// Sprawdza przecięcie zadanych ulic-odcinków. Zwraca liczbę punktów wspólnych.
        /// </summary>
        /// <returns>0 - odcinki rozłączne, 
        /// 1 - dokładnie jeden punkt wspólny, 
        /// int.MaxValue - odcinki częściowo pokrywają się (więcej niż 1 punkt wspólny)</returns>
        public int CheckIntersection(Street s1, Street s2)
        {
            double d1 = Point.CrossProduct((s2.p2 - s2.p1),(s1.p1 - s2.p1)); // położenie punktu p1 względem odcinka p3p4
            double d2 = Point.CrossProduct((s2.p2 - s2.p1), (s1.p2 - s2.p1)); // położenie punktu p2 względem odcinka p3p4
            double d3 = Point.CrossProduct((s1.p2 - s1.p1), (s2.p1 - s1.p1)); // położenie punktu p3 względem odcinka p1p2
            double d4 = Point.CrossProduct((s1.p2 - s1.p1), (s2.p2 - s1.p1)); // położenie punktu p4 względem odcinka p1p2

            double d12 = d1 * d2; // „łączne” położenie punktów p1 i p2
                                 // względem odcinka p3p4
            double d34 = d3 * d4; // „łączne” położenie punktów p3 i p4
                                 // względem odcinka p1p2
            // końce jednego z odcinków leżą po tej samej stronie drugiego
            if (d12 > 0 || d34 > 0) return 0;
            // końce żadnego z odcinków nie leżą po tej samej stronie drugiego
            // i końce jednego z odcinków leżą po przeciwnych stronach drugiego
            if (d12 < 0 || d34 < 0) return 1;
            // tu d12== 0 i d34==0
            // czyli wszystkie cztery punkty są współliniowe
            if (((OnSegment(s1.p1, s2.p1, s2.p2) && (s1.p1 != s2.p1 && s1.p1 != s2.p2))
                || (OnSegment(s1.p2, s2.p1, s2.p2)) && (s1.p2 != s2.p1 && s1.p2 != s2.p2))
                || s1.p1 == s2.p1 && s1.p2 == s2.p2 || s1.p1 == s2.p2 && s1.p2 == s2.p1)
                return int.MaxValue;
            // lub odcinki mają wspólny koniec
            if (OnRectangle(s1.p1, s2.p1, s2.p2) || OnRectangle(s1.p2, s2.p1, s2.p2)
                || OnRectangle(s2.p1, s1.p1, s1.p2) || OnRectangle(s2.p2, s1.p1, s1.p2))
                return 1;
            else
                return 0;
        }

        public bool OnRectangle(Point q, Point p1, Point p2)
        {
            // pi=(xi,yi), q=(x,y)
            return Math.Min(p1.x, p2.x) <= q.x && q.x <= Math.Max(p1.x, p2.x) &&
             Math.Min(p1.y, p2.y) <= q.y && q.y <= Math.Max(p1.y, p2.y);
        }

        public bool OnSegment(Point q, Point p1, Point p2)
        {
            return Distance(p1, p2) == Distance(p1, q) + Distance(q, p2);
        }

        public double Distance(Point p1, Point p2)
        {
            return Math.Sqrt((p1.x - p2.x) * (p1.x - p2.x) + (p1.y - p2.y) * (p1.y - p2.y));
        }

        /// <summary>
        /// Sprawdza czy dla podanych par ulic możliwy jest przejazd między nimi (z użyciem być może innych ulic). 
        /// </summary>
        /// <returns>Lista, w której na i-tym miejscu jest informacja czy przejazd między ulicami w i-tej parze z wejścia jest możliwy</returns>
        public bool[] CheckStreetsPairs(Street[] streets, int[] streetsToCheck1, int[] streetsToCheck2)
        {
            if (streetsToCheck1.Length == 0 || streetsToCheck2.Length == 0
                || streetsToCheck1.Length != streetsToCheck2.Length)
                throw new ArgumentException();

            var connectedStreets = new UnionFind(streets.Length);
            var result = new bool[streetsToCheck1.Length];

            for (int i = 0; i < streets.Length; i++)
            {
                for(int j = 0; j < streets.Length; j++)
                {
                    if (connectedStreets.Find(i) != connectedStreets.Find(j))
                    {
                        int streetsIntersect = CheckIntersection(streets[i], streets[j]);
                        if (streetsIntersect == int.MaxValue)
                            throw new ArgumentException();
                        else if (streetsIntersect != 0)
                            connectedStreets.Union(i, j);
                    }
                }
            }

            for (int i = 0; i < streetsToCheck1.Length; i++)
            {
                var street1 = streetsToCheck1[i];
                var street2 = streetsToCheck2[i];
                //already known to be connected
                if (connectedStreets.Find(street1) == connectedStreets.Find(street2))
                    result[i] = true;
            }

            return result;
        }


        /// <summary>
        /// Zwraca punkt przecięcia odcinków s1 i s2.
        /// W przypadku gdy nie ma jednoznacznego takiego punktu rzuć wyjątek ArgumentException
        /// </summary>
        public Point GetIntersectionPoint(Street s1, Street s2)
        {
            //znajdź współczynniki a i b prostych y=ax+b zawierających odcinki s1 i s2
            //uwaga na proste równoległe do osi y
            //uwaga na odcinki równoległe o wspólnych końcu
            //porównaj równania prostych, aby znaleźć ich punkt wspólny
            int segmentsIntersect = CheckIntersection(s1, s2);
            if (segmentsIntersect == 0 || segmentsIntersect == int.MaxValue)
                throw new ArgumentException();

            double a1, b1, a2, b2, x, y;

            if(s1.p1.x == s1.p2.x && s2.p1.x == s2.p2.x)
            {   //both vertical, common end
                if (s1.p1.y == s2.p1.y)
                    return new Point(s1.p1.x, s1.p1.y);
                else if(s1.p1.y == s2.p2.y)
                    return new Point(s1.p1.x, s1.p1.y);
                else if (s1.p2.y == s2.p1.y)
                    return new Point(s1.p1.x, s1.p2.y);
                else if (s1.p2.y == s2.p2.y)
                    return new Point(s1.p1.x, s1.p2.y);
            }

            if(s1.p1.x == s1.p2.x)
            {   //first vertical
                x = s1.p1.x;

                a2 = (s2.p1.y - s2.p2.y) / (s2.p1.x - s2.p2.x);
                b2 = s2.p1.y - a2 * s2.p1.x;
                y = a2 * x + b2;

                return new Point(x, y);
            }

            if(s2.p1.x == s2.p2.x)
            {   //second vertical
                x = s2.p1.x;

                a1 = (s1.p1.y - s1.p2.y) / (s1.p1.x - s1.p2.x);
                b1 = s1.p1.y - a1 * s1.p1.x;
                y = a1 * x + b1;

                return new Point(x, y);
            }

            a1 = (s1.p1.y - s1.p2.y) / (s1.p1.x - s1.p2.x);
            b1 = s1.p1.y - a1 * s1.p1.x;

            a2 = (s2.p1.y - s2.p2.y) / (s2.p1.x - s2.p2.x);
            b2 = s2.p1.y - a2 * s2.p1.x;

            if (a1 == a2)
            {   //parallel, common end
                if (s1.p1.y == s2.p1.y)
                    return new Point(s1.p1.x, s1.p1.y);
                else if (s1.p1.y == s2.p2.y)
                    return new Point(s1.p1.x, s1.p1.y);
                else if (s1.p2.y == s2.p1.y)
                    return new Point(s1.p2.x, s1.p2.y);
                else if (s1.p2.y == s2.p2.y)
                    return new Point(s1.p2.x, s1.p2.y);
            }

            x = (b2 - b1) / (a1 - a2);
            y = a1 * x + b1;

            return new Point(x, y);
        }


        /// <summary>
        /// Sprawdza możliwość przejazdu między dzielnicami-wielokątami district1 i district2,
        /// tzn. istnieją para ulic, pomiędzy którymi jest przejazd 
        /// oraz fragment jednej ulicy należy do obszaru jednej z dzielnic i fragment drugiej należy do obszaru drugiej dzielnicy
        /// </summary>
        /// <returns>Informacja czy istnieje przejazd między dzielnicami</returns>
        public bool CheckDistricts(Street[] streets, Point[] district1, Point[] district2, out List<int> path, out List<Point> intersections)
        {
            var district1Edges = new List<Street>();
            var district2Edges = new List<Street>();

            for (int i = 0; i < district1.Length - 1; i++)
            {
                district1Edges.Add(new Street(district1[i], district1[i + 1]));
            }

            for (int i = 0; i < district2.Length - 1; i++)
            {
                district2Edges.Add(new Street(district2[i], district2[i + 1]));
            }

            var streetsCrossingDistrict1 = new List<int>();
            var streetsCrossingDistrict2 = new List<int>();

            for (int i = 0; i < streets.Length; i++)
            {
                foreach (var edge in district1Edges)
                {
                    if (1 == CheckIntersection(streets[i], edge))
                        streetsCrossingDistrict1.Add(i);
                }

                foreach (var edge in district2Edges)
                {
                    if (1 == CheckIntersection(streets[i], edge))
                        streetsCrossingDistrict2.Add(i);
                }
            }

            if (streetsCrossingDistrict1.Count == 0 || streetsCrossingDistrict2.Count == 0)
            {
                path = new List<int>();
                intersections = new List<Point>();
                return false;
            }

            int[] pairsToCheck1 = new int[streetsCrossingDistrict1.Count * streetsCrossingDistrict2.Count];
            int[] pairsToCheck2 = new int[streetsCrossingDistrict1.Count * streetsCrossingDistrict2.Count];

            int k = 0;
            foreach (var street1 in streetsCrossingDistrict1)
            {
                foreach (var street2 in streetsCrossingDistrict2)
                {
                    pairsToCheck1[k] = street1;
                    pairsToCheck2[k] = street2;
                    k++;
                }
            }

            bool[] edgesConnected = CheckStreetsPairs(streets, pairsToCheck1, pairsToCheck2);

            bool isPossibleToCrossBetweenDistricts = false;
            foreach (var isPossible in edgesConnected)
            {
                if (isPossible)
                    isPossibleToCrossBetweenDistricts = true;
            }
            if (!isPossibleToCrossBetweenDistricts)
            {
                path = new List<int>();
                intersections = new List<Point>();
                return false;
            }

            var graph = new AdjacencyListsGraph<SimpleAdjacencyList>(false, streets.Length);

            for (int i = 0; i < streets.Length; i++)
            {
                for (int j = 0; j < streets.Length; j++)
                {
                    if (1 == CheckIntersection(streets[i], streets[j]))
                        graph.AddEdge(i, j);
                }
            }

            Edge[] shortestPath = null;
            double shortestDist = double.MaxValue;
            PathsInfo[] pathsInfos = null;
            int singleStreetPath = -1;

            for (int i = 0; i < streetsCrossingDistrict1.Count; i++)
            {
                var street1 = streetsCrossingDistrict1[i];
                ShortestPathsGraphExtender.DijkstraShortestPaths(graph, street1, out pathsInfos);
                for (int j = 0; j < streetsCrossingDistrict2.Count; j++)
                {
                    var street2 = streetsCrossingDistrict2[j];
                    if (!pathsInfos[street2].Dist.IsNaN())
                    {
                        if (pathsInfos[street2].Dist == 0)
                            singleStreetPath = street1;

                        if(shortestDist > pathsInfos[street2].Dist)
                        {
                            shortestPath = PathsInfo.ConstructPath(street1, street2, pathsInfos);
                            shortestDist = pathsInfos[street2].Dist;
                        }
                    }
                }
            }

            path = new List<int>();

            if (singleStreetPath != -1)
                path.Add(singleStreetPath);

            if(shortestPath.Length > 0)
            {
                path.Add(shortestPath[0].From);
                for (int i = 0; i < shortestPath.Length; i++)
                {
                    path.Add(shortestPath[i].To);
                }
            }

            intersections = new List<Point>();
            for(int i = 0; i < path.Count - 1; i ++)
            {
                intersections.Add(GetIntersectionPoint(streets[path[i]], streets[path[i + 1]]));
            }
           
            return true;
        }

    }

    [Serializable]
    public struct Point
    {
        public double x;
        public double y;

        public Point(double px, double py) { x = px; y = py; }

        public static Point operator +(Point p1, Point p2) { return new Point(p1.x + p2.x, p1.y + p2.y); }

        public static Point operator -(Point p1, Point p2) { return new Point(p1.x - p2.x, p1.y - p2.y); }

        public static bool operator ==(Point p1, Point p2) { return p1.x == p2.x && p1.y == p2.y; }

        public static bool operator !=(Point p1, Point p2) { return !(p1 == p2); }

        public override bool Equals(object obj) { return base.Equals(obj); }

        public override int GetHashCode() { return base.GetHashCode(); }

        public static double CrossProduct(Point p1, Point p2) { return p1.x * p2.y - p2.x * p1.y; }

        public override string ToString() { return String.Format("({0},{1})", x, y); }
    }

    [Serializable]
    public struct Street
    {
        public Point p1;
        public Point p2;

        public Street(Point p1, Point p2)
        {
            this.p1 = p1;
            this.p2 = p2;
        }
    }
}
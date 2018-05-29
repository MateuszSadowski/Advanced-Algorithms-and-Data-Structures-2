using System;
using System.Collections.Generic;

namespace ASD
{
    public class WaterCalculator : MarshalByRefObject
    {

        /*
         * Metoda sprawdza, czy przechodząc p1->p2->p3 skręcamy w lewo 
         * (jeżeli idziemy prosto, zwracany jest fałsz).
         */
        private bool leftTurn(Point p1, Point p2, Point p3)
        {
            Point w1 = new Point(p2.x - p1.x, p2.y - p1.y);
            Point w2 = new Point(p3.x - p2.x, p3.y - p2.y);
            double vectProduct = w1.x * w2.y - w2.x * w1.y;
            return vectProduct > 0;
        }


        /*
         * Metoda wyznacza punkt na odcinku p1-p2 o zadanej współrzędnej y.
         * Jeżeli taki punkt nie istnieje (bo cały odcinek jest wyżej lub niżej), zgłaszany jest wyjątek ArgumentException.
         */
        private Point getPointAtY(Point p1, Point p2, double y)
        {
            if (p1.y != p2.y)
            {
                double newX = p1.x + (p2.x - p1.x) * (y - p1.y) / (p2.y - p1.y);
                if ((newX - p1.x) * (newX - p2.x) > 0)
                    throw new ArgumentException("Odcinek p1-p2 nie zawiera punktu o zadanej współrzędnej y!");
                return new Point(p1.x + (p2.x - p1.x) * (y - p1.y) / (p2.y - p1.y), y);
            }
            else
            {
                if (p1.y != y)
                    throw new ArgumentException("Odcinek p1-p2 nie zawiera punktu o zadanej współrzędnej y!");
                return new Point((p1.x + p2.x) / 2, y);
            }
        }


        /// <summary>
        /// Funkcja zwraca tablice t taką, że t[i] jest głębokością, na jakiej znajduje się punkt points[i].
        /// 
        /// Przyjmujemy, że pierwszy punkt z tablicy points jest lewym krańcem, a ostatni - prawym krańcem łańcucha górskiego.
        /// </summary>
        public double[] PointDepths(Point[] points)
        {
            var depths = new double[points.Length];
            if (points.Length < 3)
                return depths;

            var maxLeft = new Point[points.Length];
            var maxRight = new Point[points.Length];

            maxLeft[0] = points[0];
            maxRight[points.Length-1] = points[points.Length-1];

            for (int i = 1; i < points.Length; i++)
            {
                if (points[i].y > maxLeft[i - 1].y)
                    maxLeft[i] = points[i];
                else
                    maxLeft[i] = maxLeft[i - 1];
            }

            for (int i = points.Length - 2; i >= 0; i--)
            {
                if (points[i].y > maxRight[i + 1].y)
                    maxRight[i] = points[i];
                else
                    maxRight[i] = maxRight[i + 1];
            }

            for (int i = 0; i < points.Length; i++)
            {
                double depth = Math.Min(maxLeft[i].y, maxRight[i].y) - points[i].y;
                depths[i] = depth < 0 ? 0 : depth;
            }

            return depths;
        }

        /// <summary>
        /// Funkcja zwraca objętość wody, jaka zatrzyma się w górach.
        /// 
        /// Przyjmujemy, że pierwszy punkt z tablicy points jest lewym krańcem, a ostatni - prawym krańcem łańcucha górskiego.
        /// </summary>
        public double WaterVolume(Point[] points)
        {
            double[] depths = PointDepths(points);

            return -1;
        }

        internal double PolygonArea(this Point[] polygon)
        {
            double area = 0;
            for (int i = 1; i < polygon.Length - 1; i++)
            {
                area += CrossProduct(new Point(polygon[i].x - polygon[0].x, polygon[i].y - polygon[0].y),
                    new Point(polygon[i + 1].x - polygon[0].x, polygon[i + 1].y - polygon[0].y));
            }
            area = 0.5 * Math.Abs(area);

            return area;
        }

        internal double CrossProduct(Point p1, Point p2) { return p1.x * p2.y - p2.x * p1.y; }
    }

    [Serializable]
    public struct Point
    {
        public double x, y;
        public Point(double x, double y)
        {
            this.x = x;
            this.y = y;
        }
    }
}

using System;
using System.Collections.Generic;

namespace Lab08
{

    public class Lab08 : MarshalByRefObject
    {

        /// <summary>
        /// funkcja do sprawdzania czy da się ustawić k elementów w odległości co najmniej dist od siebie
        /// </summary>
        /// <param name="a">posortowana tablica elementów</param>
        /// <param name="dist">zadany dystans</param>
        /// <param name="k">liczba elementów do wybrania</param>
        /// <param name="exampleSolution">Wybrane elementy</param>
        /// <returns>true - jeśli zadanie da się zrealizować</returns>
        public bool CanPlaceElementsInDistance(int[] a, int dist, int k, out List<int> exampleSolution)
        {
            int n = a.Length;
            if(n == 0 || k < 1 || k > n || n > 100000)
            {
                throw new ArgumentException();
            }

            int begining = a[0];
            int elementsPlaced = 1;
            List<int> tmpSolution = new List<int>();
            tmpSolution.Add(a[0]);

            for (int i = 1; i < n; i++)
            {
                if(a[i] - begining >= dist)
                {
                    tmpSolution.Add(a[i]);
                    begining = a[i];
                    elementsPlaced++;
                }

                if (elementsPlaced == k)
                {
                    exampleSolution = tmpSolution;
                    return true;
                }
            }

            exampleSolution = null;
            return false;
        }

        /// <summary>
        /// Funkcja wybiera k elementów tablicy a, tak aby minimalny dystans pomiędzy dowolnymi dwiema liczbami (spośród k) był maksymalny
        /// </summary>
        /// <param name="a">posortowana tablica elementów</param>
        /// <param name="k">liczba elementów do wybrania</param>
        /// <param name="exampleSolution">Wybrane elementy</param>
        /// <returns>Maksymalny możliwy dystans między wybranymi elementami</returns>
        public int LargestMinDistance(int[] a, int k, out List<int> exampleSolution)
        {
            int result = -1;
            int n = a.Length;
            if (n <= 1 || k <= 1 || k > n || n > 100000)
            {
                throw new ArgumentException();
            }

            int left = a[0], right = a[n - 1];
            List<int> tmpSolution = new List<int>();
            List<int> bestSolution = new List<int>();

            if (left - right == 0)
            {
                CanPlaceElementsInDistance(a, 0, k, out tmpSolution);

                exampleSolution = tmpSolution;
                return 0;
            }

            while (left < right)
            {
                int mid = (left + right) / 2;

                if(CanPlaceElementsInDistance(a, mid, k, out tmpSolution))
                {
                    if(mid > result)
                    {
                        result = mid;
                        bestSolution = tmpSolution;
                    }
                    left = mid + 1;
                }
                else
                {
                    right = mid;
                }

                if(result == -1 && left == right)
                {
                    if (CanPlaceElementsInDistance(a, 0, k, out tmpSolution))
                    {
                        if (mid > result)
                        {
                            result = 0;
                            bestSolution = tmpSolution;
                        }
                        left = mid + 1;
                    }
                }
            }

            exampleSolution = bestSolution;
            return result;
        }

    }

}

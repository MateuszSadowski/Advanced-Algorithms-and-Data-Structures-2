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

            int lastPositionWherePlaced = a[0];
            int elementsToPlace = k - 1;
            List<int> tmpSolution = new List<int>();
            tmpSolution.Add(a[0]);

            for (int i = 1; i < n; i++)
            {
                int nextPosition = a[i];
                if(nextPosition - lastPositionWherePlaced >= dist)
                {
                    tmpSolution.Add(nextPosition);
                    lastPositionWherePlaced = nextPosition;
                    elementsToPlace -= 1;
                }

                if (elementsToPlace == 0)
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
            int maxMinimalDistance = 0;     //worst case scenario, if impossible to place then throws exception
            int n = a.Length;
            if (n <= 1 || k <= 1 || k > n || n > 100000)
            {
                throw new ArgumentException();
            }

            int smallestDistance = 0, largestDistance = a[n - 1] - a[0];
            List<int> tmpSolution = new List<int>();
            List<int> bestSolution = new List<int>();

            //Perform binary search for best distance
            while (smallestDistance <= largestDistance)
            {
                int mediumDistance;
                try
                {
                    mediumDistance = (smallestDistance + largestDistance) / 2;
                }
                catch(OverflowException)
                {
                    if(smallestDistance % 2 == 1 && largestDistance % 2 == 1)
                    {
                        mediumDistance = smallestDistance / 2 + largestDistance / 2 + 1; 
                    }
                    else
                    {
                        mediumDistance = smallestDistance / 2 + largestDistance / 2;
                    }
                }

                if(CanPlaceElementsInDistance(a, mediumDistance, k, out tmpSolution))
                {
                    maxMinimalDistance = mediumDistance;
                    bestSolution = tmpSolution;

                    smallestDistance = mediumDistance + 1;  //Look for bigger
                }
                else
                {
                    largestDistance = mediumDistance - 1;   //Look for smaller
                }
            }

            exampleSolution = bestSolution;
            return maxMinimalDistance;
        }

    }

}

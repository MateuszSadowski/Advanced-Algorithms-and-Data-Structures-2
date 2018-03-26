
using System;

namespace ASD
{

    public class CarpentersBench : System.MarshalByRefObject
    {

        /// <summary>
        /// Metoda pomocnicza - wymagana przez system
        /// </summary>
        public int Cut(int length, int width, int[,] elements, out Cut cuts)
        {
            (int length, int width, int price)[] _elements = new(int length, int width, int price)[elements.GetLength(0)];
            for (int i = 0; i < _elements.Length; ++i)
            {
                _elements[i].length = elements[i, 0];
                _elements[i].width = elements[i, 1];
                _elements[i].price = elements[i, 2];
            }
            return Cut((length, width), _elements, out cuts);
        }

        /// <summary>
        /// Wyznaczanie optymalnego sposobu pocięcia płyty
        /// </summary>
        /// <param name="sheet">Rozmiary płyty</param>
        /// <param name="elements">Tablica zawierająca informacje o wymiarach i wartości przydatnych elementów</param>
        /// <param name="cuts">Opis cięć prowadzących do uzyskania optymalnego rozwiązania</param>
        /// <returns>Maksymalna sumaryczna wartość wszystkich uzyskanych w wyniku cięcia elementów</returns>
        public int Cut((int length, int width) sheet, (int length, int width, int price)[] elements, out Cut cuts)
        {
            int[,] sheetArr = new int[sheet.length, sheet.width];   //contains best values of current pieces that can be formed
            Cut[,] cutsArr = new Cut[sheet.length, sheet.width];    //contains references for cuts used to form pieces of the best value,

            foreach ((int length, int width, int price) elem in elements)   //loop on elements
            {
                if (elem.length <= sheet.length && elem.width <= sheet.width)    //check if piece of the sheet can be build with one of the given elements
                {
                    if (elem.price > sheetArr[elem.length - 1, elem.width - 1])
                    {
                        sheetArr[elem.length - 1, elem.width - 1] = elem.price;
                        cutsArr[elem.length - 1, elem.width - 1] = new Cut(elem.length, elem.width, elem.price);   //piece is exactly one of the elements (no cut)
                    }
                }
            }

            for (int l = 0; l < sheet.length; l++)   //loop on length
            {
                for (int w = 0; w < sheet.width; w++)    //loop on width
                {
                    cutsArr[l, w] = (cutsArr[l, w] == null) ? (new Cut(l + 1, w + 1, 0)) : cutsArr[l, w];   //no elem of given dimensions (no cut)

                    for (int i = (int)((l + 1) / 2); i > 0; i--)  //go back on length
                    {
                        if (sheetArr[i - 1, w] + sheetArr[l - i, w] > sheetArr[l, w])
                        {
                            sheetArr[l, w] = sheetArr[i - 1, w] + sheetArr[l - i, w];

                            cutsArr[l, w] = new Cut(l + 1, w + 1, sheetArr[l, w], false, i, cutsArr[i - 1, w], cutsArr[l - i, w]);
                        }
                    }

                    for (int i = (int)((w + 1) / 2); i > 0; i--)  //go back on width
                    {
                        if (sheetArr[l, i - 1] + sheetArr[l, w - i] > sheetArr[l, w])
                        {
                            sheetArr[l, w] = sheetArr[l, i - 1] + sheetArr[l, w - i];

                            cutsArr[l, w] = new Cut(l + 1, w + 1, sheetArr[l, w], true, i, cutsArr[l, i - 1], cutsArr[l, w - i]);
                        }
                    }
                }
            }

            cuts = cutsArr[sheet.length - 1, sheet.width - 1];
            return sheetArr[sheet.length - 1, sheet.width - 1];
        }
    }

}
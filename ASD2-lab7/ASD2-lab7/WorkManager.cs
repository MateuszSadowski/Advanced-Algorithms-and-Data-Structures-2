using System;
using System.Linq;

namespace ASD
{
    public class WorkManager : MarshalByRefObject
    {
        /// <summary>
        /// Implementacja wersji 1
        /// W tablicy blocks zapisane s� wagi wszystkich blok�w do przypisania robotnikom.
        /// Ka�dy z nich powinien mie� przypisane bloki sumie wag r�wnej expectedBlockSum.
        /// Metoda zwraca tablic� przypisuj�c� ka�demu z blok�w jedn� z warto�ci:
        /// 1 - je�li blok zosta� przydzielony 1. robotnikowi
        /// 2 - je�li blok zosta� przydzielony 2. robotnikowi
        /// 0 - je�li blok nie zosta� przydzielony do �adnego robotnika
        /// Je�li wymaganego podzia�u nie da si� zrealizowa� metoda zwraca null.
        /// </summary>
        /// 

        public int workerSum;
        public int expectedSum;
        public int[] blockWeights;
        public int blockCount;

        public int[] DivideWorkersWork(int[] blocks, int expectedBlockSum)
        {
            workerSum = 0;
            expectedSum = expectedBlockSum;
            blockWeights = blocks;
            blockCount = blocks.Length;

            int[] tmpBlocks = new int[blocks.Length];

            bool result = DivideWorkersWorkUtil(1, tmpBlocks, 0);
            if(!result)
            {   //if could not be found for 1 worker then fail
                return null;
            }

            workerSum = 0;
            result = DivideWorkersWorkUtil(2, tmpBlocks, 0);
            if (!result)
            {   
                return null;
            }

            return tmpBlocks;
        }

        public bool DivideWorkersWorkUtil(int workerNum, int[] blocks, int nextBlock)
        {
            if(workerSum == expectedSum)
            {
                return true;
            }

            for (int block = nextBlock; block < blockCount; block++)
            {
                if(blocks[block] != 0)
                {   //block already assigned to current worker or another worker
                    continue;
                }

                workerSum += blockWeights[block];
                blocks[block] = workerNum;

                if(workerSum <= expectedSum)
                {
                    bool success = DivideWorkersWorkUtil(workerNum, blocks, block + 1);
                    if(success)
                    {
                        return true;
                    }
                }

                //have not found solution with current block
                workerSum -= blockWeights[block];
                blocks[block] = 0;
                //try next block
            }

            //have not found solution with previous block choice, go back and choose different
            return false;
        }

        /// <summary>
        /// Implementacja wersji 2
        /// Parametry i wynik s� analogiczne do wersji 1.
        /// </summary>
        public int[] DivideWorkWithClosestBlocksCount(int[] blocks, int expectedBlockSum)
        {
            return new int[0];
        }

// Mo�na dopisywa� pola i metody pomocnicze

    }
}


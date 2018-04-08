using System;
using System.Linq;

namespace ASD
{
    public class WorkManager : MarshalByRefObject
    {
        /// <summary>
        /// Implementacja wersji 1
        /// W tablicy blocks zapisane s¹ wagi wszystkich bloków do przypisania robotnikom.
        /// Ka¿dy z nich powinien mieæ przypisane bloki sumie wag równej expectedBlockSum.
        /// Metoda zwraca tablicê przypisuj¹c¹ ka¿demu z bloków jedn¹ z wartoœci:
        /// 1 - jeœli blok zosta³ przydzielony 1. robotnikowi
        /// 2 - jeœli blok zosta³ przydzielony 2. robotnikowi
        /// 0 - jeœli blok nie zosta³ przydzielony do ¿adnego robotnika
        /// Jeœli wymaganego podzia³u nie da siê zrealizowaæ metoda zwraca null.
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
        /// Parametry i wynik s¹ analogiczne do wersji 1.
        /// </summary>
        public int[] DivideWorkWithClosestBlocksCount(int[] blocks, int expectedBlockSum)
        {
            return new int[0];
        }

// Mo¿na dopisywaæ pola i metody pomocnicze

    }
}


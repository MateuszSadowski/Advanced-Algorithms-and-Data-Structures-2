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

        public int[] workerSum;
        public int[] workerBlockCount;
        public int expectedSum;
        public int[] blockWeights;
        public int blockCount;

        public int[] DivideWorkersWork(int[] blocks, int expectedBlockSum)
        {
            expectedSum = expectedBlockSum;
            blockWeights = blocks;
            blockCount = blocks.Length;
            workerSum = new int[3];     //indexing from 1;
            workerSum[0] = -1;      //sentiel
            workerBlockCount = new int[3];     //indexing from 1;
            workerBlockCount[0] = -1;      //sentiel

            int[] tmpBlocks = new int[blockCount];

            workerSum[1] = 0;
            workerBlockCount[1] = 0;
            bool result = DivideWorkersWorkUtil(1, tmpBlocks, 0);
            if(!result)
            {   //if could not be found for 1 worker then fail
                return null;
            }

            workerSum[2] = 0;
            workerBlockCount[2] = 0;
            result = DivideWorkersWorkUtil(2, tmpBlocks, 0);
            if (!result)
            {   
                return null;
            }

            return tmpBlocks;
        }

        public bool DivideWorkersWorkUtil(int workerNum, int[] blocks, int nextBlock)
        {
            if(workerSum[workerNum] == expectedSum)
            {
                return true;
            }

            for (int block = nextBlock; block < blockCount; block++)
            {
                if(blocks[block] != 0 || blockWeights[block] < 0)   //blockWeights[block] < 0 -> block is removed, for second task
                {   //block already assigned to current worker or another worker
                    continue;
                }

                workerSum[workerNum] += blockWeights[block];
                blocks[block] = workerNum;
                workerBlockCount[workerNum] += 1;

                if(workerSum[workerNum] <= expectedSum)
                {
                    bool success = DivideWorkersWorkUtil(workerNum, blocks, block + 1);
                    if(success)
                    {
                        return true;
                    }
                }

                //have not found solution with current block
                workerSum[workerNum] -= blockWeights[block];
                blocks[block] = 0;
                workerBlockCount[workerNum] -= 1;
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
            int minBlockCountDiff = Int32.MaxValue;
            int[] minBlocks = null;
            int[] result = null;

            int[] tmpBlockWeights = (int[]) blocks.Clone();

            do
            {
                int[] removedIndexes = new int[0];
                for (int blocksRemoved = 0; blocksRemoved < blockCount; blocksRemoved++)
                {
                    removedIndexes = new int[blocksRemoved];
                    for (int blockToMove = 0; blockToMove < blocksRemoved; blockToMove++)
                    {
                        for (int blockToRemove = 0; blockToRemove < blockCount; blockToRemove++)
                        {
                            removedIndexes[blockToMove] = blockToRemove; 
                            tmpBlockWeights[blockToRemove] = -1;
                        }
                    }
                }

                result = DivideWorkersWork(tmpBlockWeights, expectedBlockSum);

                if (result != null)
                {
                    if (Math.Abs(workerBlockCount[1] - workerBlockCount[2]) < minBlockCountDiff)    //TODO: Math.Abs may reduce performance
                    {
                        minBlockCountDiff = Math.Abs(workerBlockCount[1] - workerBlockCount[2]);
                        minBlocks = (int[])result.Clone();
                    }
                }

                foreach (var ind in removedIndexes)
                {
                    tmpBlockWeights[ind] = blocks[ind];
                }
            } while (result != null);

            return minBlocks;
        }

// Mo�na dopisywa� pola i metody pomocnicze

    }
}


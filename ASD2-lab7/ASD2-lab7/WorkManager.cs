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
        internal int worker1Sum;
        internal int worker2Sum;
        public int expectedSum;
        public int[] blockWeights;
        public int blockCount;

        public int[] DivideWorkersWork(int[] blocks, int expectedBlockSum)
        {
            //TODO: optimalisation: find smallest block weight and if workerSum > it, then cut branch
            worker1Sum = 0;
            worker2Sum = 0;
            expectedSum = expectedBlockSum;
            blockWeights = blocks;
            blockCount = blocks.Length;

            int[] blocksAssignment = new int[blockCount];

            if(expectedSum == 0)
            {
                return blocksAssignment;
            }

            bool foundSolution = DivideWorkWorker1Util(blocksAssignment, 0);

            if(foundSolution)
            {
                return blocksAssignment;
            }

            return null;
        }

        internal bool DivideWorkWorker1Util(int[] blocks, int nextBlockToTry)
        {
            if (worker1Sum == expectedSum)
            {
                return true;
            }

            for (int blockIndex = nextBlockToTry; blockIndex < blockCount; blockIndex++)
            {
                if (blocks[blockIndex] != 0)   //blockWeights[block] < 0 -> block is removed, for second task
                {   //block already assigned to current worker or another worker
                    continue;
                }

                worker1Sum += blockWeights[blockIndex];
                blocks[blockIndex] = 1;
                if (worker1Sum <= expectedSum)
                {
                    bool success = DivideWorkWorker1Util(blocks, blockIndex + 1);
                    if (success)
                    {   //try to find for worker 2
                        success = DivideWorkWorker2Util(blocks, 0);
                        if(success)
                        {
                            return true;
                        }
                    }
                }

                //have not found solution with current block
                worker1Sum -= blockWeights[blockIndex];
                blocks[blockIndex] = 0;
                //try next block
            }

            return false;
        }

        internal bool DivideWorkWorker2Util(int[] blocks, int nextBlockToTry)
        {
            if (worker2Sum == expectedSum)
            {
                return true;
            }

            for (int blockIndex = nextBlockToTry; blockIndex < blockCount; blockIndex++)
            {
                if (blocks[blockIndex] != 0)   //blockWeights[block] < 0 -> block is removed, for second task
                {   //block already assigned to current worker or another worker
                    continue;
                }

                worker2Sum += blockWeights[blockIndex];
                blocks[blockIndex] = 2;
                if (worker2Sum <= expectedSum)
                {
                    bool success = DivideWorkWorker2Util(blocks, blockIndex + 1);
                    if (success)
                    {   //finish
                        return true;
                    }
                }

                //have not found solution with current block
                worker2Sum -= blockWeights[blockIndex];
                blocks[blockIndex] = 0;
                //try next block
            }

            return false;
        }
        /// <summary>
        /// Implementacja wersji 2
        /// Parametry i wynik s¹ analogiczne do wersji 1.
        /// </summary>
        /// 
        public int[] DivideWorkWithClosestBlocksCount(int[] blocks, int expectedBlockSum)
        {

            return null;
        }


        // Mo¿na dopisywaæ pola i metody pomocnicze

    }
}


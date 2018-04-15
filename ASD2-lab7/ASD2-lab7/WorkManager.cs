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
        internal int expectedSum;
        internal int[] blockWeights;
        internal int blockCount;

        public int[] DivideWorkersWork(int[] blocks, int expectedBlockSum)
        {
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
                if (blocks[blockIndex] != 0)
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
                if (blocks[blockIndex] != 0)
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

        internal int[] bestSolution;
        internal int minBlockCountDiff;
        internal int worker1BlockCount;
        internal int worker2BlockCount;

        internal int minBlockWeight;
        internal int sumBlockWeights;

        public int[] DivideWorkWithClosestBlocksCount(int[] blocks, int expectedBlockSum)
        {
            worker1Sum = 0;
            worker2Sum = 0;
            expectedSum = expectedBlockSum;
            blockWeights = blocks;
            blockCount = blocks.Length;

            int[] blocksAssignment = new int[blockCount];

            //for optimization
            minBlockWeight = Int32.MaxValue;
            sumBlockWeights = 0;
            foreach (var weight in blockWeights)
            {
                if (minBlockWeight > weight)
                {
                    minBlockWeight = weight;
                }

                sumBlockWeights += weight;
            }

            if (expectedSum == 0)
            {
                return blocksAssignment;
            }

            if(expectedSum > sumBlockWeights / 2)
            {
                return null;
            }

            bestSolution = null;
            minBlockCountDiff = Int32.MaxValue;
            worker1BlockCount = 0;
            worker2BlockCount = 0;

            DivideWorkWorker1BestSolutionUtil(blocksAssignment, 0);

            return bestSolution;
        }

        internal bool DivideWorkWorker1BestSolutionUtil(int[] blocks, int nextBlockToTry)
        {
            if (worker1Sum == expectedSum)
            {
                return true;
            }

            for (int blockIndex = nextBlockToTry; blockIndex < blockCount; blockIndex++)
            {
                if (blocks[blockIndex] != 0)
                {   //block already assigned to current worker or another worker
                    continue;
                }

                worker1Sum += blockWeights[blockIndex];
                worker1BlockCount += 1;
                blocks[blockIndex] = 1;
                if (worker1Sum <= expectedSum - minBlockWeight || worker1Sum == expectedSum)
                {
                    bool success = DivideWorkWorker1BestSolutionUtil(blocks, blockIndex + 1);
                    if (success)
                    {   //try to find for worker 2
                        success = DivideWorkWorker2BestSolutionUtil(blocks, nextBlockToTry);
                        if (success)
                        {
                            if(minBlockCountDiff == 0)
                            {   //best possible solution, finish
                                return true;
                            }
                        }
                    }
                }

                //look for different solution
                worker1Sum -= blockWeights[blockIndex];
                worker1BlockCount -= 1;
                blocks[blockIndex] = 0;
                //try next block
            }

            return false;
        }

        internal bool DivideWorkWorker2BestSolutionUtil(int[] blocks, int nextBlockToTry)
        {
            if (worker2Sum == expectedSum)
            {
                int blockCountDiff = Math.Abs(worker1BlockCount - worker2BlockCount);
                if (blockCountDiff < minBlockCountDiff)
                {
                    minBlockCountDiff = blockCountDiff;
                    bestSolution = (int[])blocks.Clone();
                }
                return true;
            }

            for (int blockIndex = nextBlockToTry; blockIndex < blockCount; blockIndex++)
            {
                if (blocks[blockIndex] != 0)
                {   //block already assigned to current worker or another worker
                    continue;
                }

                worker2Sum += blockWeights[blockIndex];
                worker2BlockCount += 1;
                blocks[blockIndex] = 2;
                if (worker2Sum <= expectedSum - minBlockWeight || worker2Sum == expectedSum)
                {
                    bool success = DivideWorkWorker2BestSolutionUtil(blocks, blockIndex + 1);
                    if (success)
                    { 
                        if(minBlockCountDiff == 0)
                        {   //best possible solution
                            return true;
                        }
                    }
                }

                //look for different solution
                worker2Sum -= blockWeights[blockIndex];
                worker2BlockCount -= 1;
                blocks[blockIndex] = 0;
                //try next block
            }

            return false;
        }

        // Mo¿na dopisywaæ pola i metody pomocnicze

    }
}


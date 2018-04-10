using System;
using System.Linq;

namespace ASD
{
    public class WorkManager1 : MarshalByRefObject
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

        public int[] workerSum;
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

            int[] tmpBlocks = new int[blockCount];

            workerSum[1] = 0;
            bool result = DivideWorkersWorkUtil(1, tmpBlocks, 0);
            if(!result)
            {   //if could not be found for 1 worker then fail
                return null;
            }

            workerSum[2] = 0;
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
                if(blocks[block] != 0)   //blockWeights[block] < 0 -> block is removed, for second task
                {   //block already assigned to current worker or another worker
                    continue;
                }

                workerSum[workerNum] += blockWeights[block];
                blocks[block] = workerNum;
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
                //try next block
            }

            //have not found solution with previous block choice, go back and choose different
            return false;
        }

        /// <summary>
        /// Implementacja wersji 2
        /// Parametry i wynik s¹ analogiczne do wersji 1.
        /// </summary>
        /// 

        public int[] bestSolution;
        public int minBlockCountDiff;
        public int workerCount = 2;
        public int[] workerBlockCount;
        public bool firstIteration = true;

        public int[] DivideWorkWithClosestBlocksCount(int[] blocks, int expectedBlockSum)
        {
            expectedSum = expectedBlockSum;
            blockWeights = blocks;
            blockCount = blocks.Length;
            workerSum = new int[3];     //indexing from 1;
            workerSum[0] = -1;      //sentiel
            workerBlockCount = new int[3];     //indexing from 1;
            workerBlockCount[0] = -1;      //sentiel

            bestSolution = null;
            minBlockCountDiff = Int32.MaxValue;

            int[] tmpBlocks = new int[blockCount];

            //workerSum[1] = 0;
            //workerBlockCount[1] = 0;
            DivideWorkersWorkUtilBestSolution(tmpBlocks, 0);

            //workerSum[2] = 0;
            //workerBlockCount[2] = 0;
            //DivideWorkersWorkUtilBestSolution(tmpBlocks, 0);

            return bestSolution;
        }

        public bool DivideWorkersWorkUtilBestSolution(int[] blocks, int nextBlock)
        {
            bool success = false;
            if (workerSum[1] == expectedSum && workerSum[2] == expectedSum)
            {
                int blockCountDiff = Math.Abs(workerBlockCount[1] - workerBlockCount[2]);
                if (blockCountDiff < minBlockCountDiff)
                {
                    minBlockCountDiff = blockCountDiff;
                    bestSolution = (int[])blocks.Clone();
                }
                return true;
            }

            //worker 1
            for (int block = nextBlock; block < blockCount; block++)
            {
                if (blocks[block] != 0)   //blockWeights[block] < 0 -> block is removed, for second task
                {   //block already assigned to current worker or another worker
                    continue;
                }

                workerSum[1] += blockWeights[block];
                blocks[block] = 1;
                workerBlockCount[1] += 1;

                if(firstIteration && workerSum[1] == expectedSum)
                {   //does not have worker 2 set yet
                    firstIteration = false;
                    success = true;
                    break;
                }

                if (workerSum[1] <= expectedSum)
                {
                    success = DivideWorkersWorkUtilBestSolution(blocks, block + 1);
                }

                if(success)
                {   //move to next worker
                    break;
                }

                //if !success, look for different solution
                workerSum[1] -= blockWeights[block];
                blocks[block] = 0;
                workerBlockCount[1] -= 1;
                //try next block
            }

            if(!success) //if loop timed out and !success
            {
                return false;
            }

            //check in furter iterations if after 
            //if (workerSum[1] == expectedSum && workerSum[2] == expectedSum)
            //{
            //    int blockCountDiff = Math.Abs(workerBlockCount[1] - workerBlockCount[2]);
            //    if (blockCountDiff < minBlockCountDiff)
            //    {
            //        minBlockCountDiff = blockCountDiff;
            //        bestSolution = (int[])blocks.Clone();
            //    }
            //    return true;
            //}

            success = false;

            //worker 2
            for (int block = nextBlock; block < blockCount; block++)
            {
                if (blocks[block] != 0)   //blockWeights[block] < 0 -> block is removed, for second task
                {   //block already assigned to current worker or another worker
                    continue;
                }

                workerSum[2] += blockWeights[block];
                blocks[block] = 1;
                workerBlockCount[2] += 1;

                if (workerSum[2] <= expectedSum)
                {
                    success = DivideWorkersWorkUtilBestSolution(blocks, block + 1);
                }

                if (success)
                {   //move to next worker
                    break;
                }

                //regardless of success, look for different solution
                workerSum[2] -= blockWeights[block];
                blocks[block] = 0;
                workerBlockCount[2] -= 1;
                //try next block
            }

            return false;

            //bool success = false;
            //for (int workerNum = 1; workerNum <= workerCount; workerNum++)   //indexing from 1
            //{
            //    if (workerSum[1] == expectedSum && workerSum[2] == expectedSum)
            //    {
            //        int blockCountDiff = Math.Abs(workerBlockCount[1] - workerBlockCount[2]);
            //        if (blockCountDiff < minBlockCountDiff)
            //        {
            //            minBlockCountDiff = blockCountDiff;
            //            bestSolution = (int[])blocks.Clone();
            //        }
            //        return true;
            //    }

            //    for (int block = nextBlock; block < blockCount; block++)
            //    {
            //        if (blocks[block] != 0)   //blockWeights[block] < 0 -> block is removed, for second task
            //        {   //block already assigned to current worker or another worker
            //            continue;
            //        }

            //        workerSum[workerNum] += blockWeights[block];
            //        blocks[block] = workerNum;
            //        workerBlockCount[workerNum] += 1;

            //        if(workerSum[workerNum] == expectedSum)
            //        {
                        
            //        }

            //        if (workerSum[workerNum] <= expectedSum)
            //        {
            //            success = DivideWorkersWorkUtilBestSolution(blocks, block + 1);
            //        }

            //        if(success)
            //        {   //move to next worker
            //            break;
            //        }

            //        //regardless of success, look for different solution
            //        workerSum[workerNum] -= blockWeights[block];
            //        blocks[block] = 0;
            //        workerBlockCount[workerNum] -= 1;
            //        //try next block
            //    }
            //}
        }

// Mo¿na dopisywaæ pola i metody pomocnicze

    }
}


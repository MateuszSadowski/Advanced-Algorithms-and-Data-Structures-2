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

        public int worker1Sum;
        public int worker2Sum;
        public int expectedSum;
        public bool worker1Satisfied;
        public bool worker2Satisfied;
        public int[] blockWeights;
        public int blockCount;
        public int blockCountTotal;
        public int[] DivideWorkersWork(int[] blocks, int expectedBlockSum)
        {
            worker1Sum = 0;
            worker2Sum = 0;
            expectedSum = expectedBlockSum;
            worker1Satisfied = false;
            worker2Satisfied = false;
            blockWeights = blocks;
            blockCount = blocks.Length;
            blockCountTotal = blocks.Length;

            if(expectedBlockSum == 0)
            {
                return new int[blocks.Length];
            }

            int[] tmpBlocks = new int[blocks.Length];
            bool satisfiedAll = DivideWorkersWorkUtil(tmpBlocks, 0);

            if (satisfiedAll)
            {
                return tmpBlocks;
            }

            //DivideWorkersWorkUtil(tmpBlocks, 0);

            //if(worker1Satisfied && worker2Satisfied)
            //{
            //    return tmpBlocks;
            //}

            return null;
        }

        //public bool DivideWorkersWorkUtil(int[] tmpBlocks, int level)   //level - number of blocks assigned in total
        //{
        //    if (worker1Satisfied && worker2Satisfied)
        //    {
        //        return true;
        //    }


        //}

        //public bool DivideWorker1Util(int[] tmpBlocks, int level)
        //{
        //    if(worker1Sum == expectedSum)
        //    {
        //        return true;
        //    }

        //    if (level >= blockCount)
        //    {   //no more block to assign
        //        return false;
        //    }

        //    for (int i = 0; i < blockCountTotal; i++)
        //    {
        //        worker1Sum += blockWeights[i];
        //        tmpBlocks[i] = 1;

        //        DivideWorker1Util()
        //    }
        //}

        //public void DivideWorkersWorkUtil(int[] tmpBlocks, int level)   //level -> block
        //{
        //    if (worker1Satisfied && worker2Satisfied)
        //    {
        //        return;
        //    }
        //    else if (level >= blockCount)
        //    {
        //        return;
        //    }

        //    bool satisfiedAll = false;

        //    //decision - assign block to 1 or 2
        //    if (!worker1Satisfied)
        //    {   //assign to 1
        //        for (int i = level; i < blockCount; i++)
        //        {
        //            worker1Sum += blockWeights[i];
        //            tmpBlocks[i] = 1;

        //            if(worker1Sum == expectedSum)
        //            {
        //                worker1Satisfied = true;
        //                break;
        //            }
        //            else if(worker1Sum > expectedSum)
        //            {
        //                worker1Sum -= blockWeights[i];
        //                tmpBlocks[i] = 0;
        //                continue;
        //            }

        //            DivideWorkersWorkUtil(tmpBlocks, level + 1);

        //            if(worker1Satisfied)
        //            {
        //                break;
        //            }

        //            worker1Sum -= blockWeights[i];
        //            tmpBlocks[i] = 0;
        //        }
        //    }

        //    if (!worker2Satisfied)
        //    {   //assign to 2
        //        for (int i = level; i < blockCount; i++)
        //        {
        //            worker2Sum += blockWeights[i];
        //            tmpBlocks[i] = 1;

        //            if (worker2Sum == expectedSum)
        //            {
        //                worker2Satisfied = true;
        //                break;
        //            }
        //            else if (worker2Sum > expectedSum)
        //            {
        //                worker2Sum -= blockWeights[i];
        //                tmpBlocks[i] = 0;
        //                continue;
        //            }

        //            DivideWorkersWorkUtil(tmpBlocks, level + 1);

        //            if (worker2Satisfied)
        //            {
        //                break;
        //            }

        //            worker2Sum -= blockWeights[i];
        //            tmpBlocks[i] = 0;
        //        }
        //    }

        //    return;
        //}

        //kinda working
        public bool DivideWorkersWorkUtil(int[] tmpBlocks, int level)   //level -> block
        {
            if (worker1Satisfied && worker2Satisfied)
            {
                return true;
            }
            else if (level >= blockCount)
            {
                return false;
            }

            bool satisfiedAll = false;

            //decision - assign block to 1 or 2
            if (!worker1Satisfied)
            {   //assign to 1
                for (int i = level; i < blockCount; i++)
                {
                    worker1Sum += blockWeights[i];
                    tmpBlocks[i] = 1;
                    if (worker1Sum > expectedSum)
                    {
                        worker1Sum -= blockWeights[i];
                        worker1Satisfied = false;
                        tmpBlocks[i] = 0;
                        if (i + 1 == blockCount)
                        {
                            return false;
                        }
                        continue;
                    }
                    else if (worker1Sum == expectedSum)
                    {
                        worker1Satisfied = true;
                        //continue trying for worker 2
                    }

                    satisfiedAll = DivideWorkersWorkUtil(tmpBlocks, level + 1);

                    if (!satisfiedAll)
                    {
                        worker1Sum -= blockWeights[i];
                        worker1Satisfied = false;
                        tmpBlocks[i] = 0;
                    }
                    else
                    {
                        break;
                    }
                }
            }

            if (!worker2Satisfied)
            {   //assign to 2
                for (int i = level; i < blockCount; i++)
                {
                    worker2Sum += blockWeights[i];
                    tmpBlocks[i] = 2;
                    if (worker2Sum > expectedSum)
                    {
                        worker2Sum -= blockWeights[i];
                        worker2Satisfied = false;
                        tmpBlocks[i] = 0;
                        continue;
                    }
                    else if (worker2Sum == expectedSum)
                    {
                        worker2Satisfied = true;
                        //continue trying for worker 1
                    }

                    satisfiedAll = DivideWorkersWorkUtil(tmpBlocks, level + 1);

                    if (!satisfiedAll)
                    {
                        worker2Sum -= blockWeights[i];
                        worker2Satisfied = false;
                        tmpBlocks[i] = 0;
                    }
                    else
                    {
                        break;
                    }
                }

                if (satisfiedAll)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }

            return satisfiedAll;
        }

        //public bool DivideWorkersWorkUtil(int[] tmpBlocks, int level)   //level -> block
        //{
        //    if (worker1Satisfied && worker2Satisfied)
        //    {
        //        return true;
        //    }
        //    else if (level >= blockCount)
        //    {
        //        return false;
        //    }

        //    bool satisfiedAll = false;

        //    //decision - assign block to 1 or 2
        //    if (!worker1Satisfied)
        //    {   //assign to 1
        //        for (int i = level; i < blockCount; i++)
        //        {
        //            worker1Sum += blockWeights[i];
        //            tmpBlocks[i] = 1;
        //            if (worker1Sum > expectedSum)
        //            {
        //                worker1Sum -= blockWeights[i];
        //                worker1Satisfied = false;
        //                tmpBlocks[i] = 0;
        //                return false;
        //            }
        //            else if (worker1Sum == expectedSum)
        //            {
        //                worker1Satisfied = true;
        //                //continue trying for worker 2
        //            }

        //            satisfiedAll = DivideWorkersWorkUtil(tmpBlocks, level + 1);

        //            if (!satisfiedAll)
        //            {
        //                worker1Sum -= blockWeights[i];
        //                worker1Satisfied = false;
        //                tmpBlocks[i] = 0;
        //            }
        //        }
        //    }

        //    if (!worker2Satisfied)
        //    {   //assign to 2
        //        for (int i = level; i < blockCount; i++)
        //        {
        //            worker2Sum += blockWeights[i];
        //            tmpBlocks[i] = 2;
        //            if (worker2Sum > expectedSum)
        //            {
        //                worker2Sum -= blockWeights[i];
        //                worker2Satisfied = false;
        //                tmpBlocks[i] = 0;
        //                continue;
        //            }
        //            else if (worker2Sum == expectedSum)
        //            {
        //                worker2Satisfied = true;
        //                //continue trying for worker 1
        //            }

        //            satisfiedAll = DivideWorkersWorkUtil(tmpBlocks, level + 1);

        //            if (!satisfiedAll)
        //            {
        //                worker2Sum -= blockWeights[i];
        //                worker2Satisfied = false;
        //                tmpBlocks[i] = 0;
        //            }
        //        }

        //        //if (satisfiedAll)
        //        //{
        //        //    return true;
        //        //}
        //        //else
        //        //{
        //        //    return false;
        //        //}
        //    }

        //    return satisfiedAll;
        //}

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


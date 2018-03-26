
using System;
using System.Collections.Generic;

namespace ASD
{
interface IPriorityQueue
    {
    void Put(int p);     // wstawia element do kolejki
    int GetMax();        // pobiera maksymalny element z kolejki (element jest usuwany z kolejki)
    int ShowMax();       // pokazuje maksymalny element kolejki (element pozostaje w kolejce)
    int Count { get; }   // liczba elementów kolejki
    }

class Node
    {
        public int val;
        public Node next;
        public Node prev;

        public Node(int v, Node nodeNext, Node nodePrev)
        {
            val = v;
            next = nodeNext;
            prev = nodePrev;
        }
    }
class LazyPriorityQueue : IPriorityQueue
    {
        public Node head;
        public int count;
    public LazyPriorityQueue()
        {
            head = null; count = 0;
        }

    public void Put(int p)
        {
            if (null == head)
                head = new Node(p, null, null);
            else
            {
                head.prev = new Node(p, head, null);
                head = head.prev;
            }
            count++;
        }

    public int GetMax()
        {
            //Empty queue
            if (null == head)
                throw new InvalidOperationException("Access to empty queue.");

            //Find max value node
            Node it = head;
            Node tmpMaxNode = it;
            int tmpMaxVal = it.val;
            while(null != it.next)
            {
                it = it.next;

                if (it.val > tmpMaxVal)
                {
                    tmpMaxVal = it.val;
                    tmpMaxNode = it;
                }
            }

            count--;
            //Remove max value node
            if (null == tmpMaxNode.prev && null == tmpMaxNode.next)  //Only node
            {
                head = null;
                return tmpMaxVal;
            }
            else if(null == tmpMaxNode.prev)    //First node
            {
                head = head.next;
                head.prev = null;
                return tmpMaxVal;
            }
            else if(null == tmpMaxNode.next)    //Last node
            {
                tmpMaxNode.prev.next = null;
                return tmpMaxVal;
            }
            else
            {
                tmpMaxNode.prev.next = tmpMaxNode.next;
                tmpMaxNode.next.prev = tmpMaxNode.prev;
                return tmpMaxVal;
            }
        }

    public int ShowMax()
        {
            //Empty queue
            if (null == head)
                throw new InvalidOperationException("Access to empty queue.");

            //Find max value node
            Node it = head;
            int tmpMaxVal = it.val;
            while (null != it.next)
            {
                it = it.next;

                if (it.val > tmpMaxVal)
                {
                    tmpMaxVal = it.val;
                }
            }
            return tmpMaxVal;
        }

    public int Count
        {
        get {
                //if (null == head)
                //    return 0;

                //Node it = head;
                //int tmpCount = 1;
                //while (null != it.next)
                //{
                //    it = it.next;
                //    tmpCount++;
                //}
                //return tmpCount;

                return count;
            }
        }

    } // LazyPriorityQueue


class EagerPriorityQueue : IPriorityQueue
    {
        public Node head;
        public int count;
        public EagerPriorityQueue()
        {
            head = null; count = 0;
        }

    public void Put(int p)
        {
            count++;
            //Empty queue
            if (null == head)
            {
                head = new Node(p, null, null);
                return;
            };

            //Find max value node
            Node it = head;
            Node newNode = new Node(p, null, null);

            if(p >= it.val)  //Insert in front
            {
                head.prev = newNode;
                newNode.next = head;
                head = newNode;
                return;
            }

            while (null != it.next)
            {
                it = it.next;

                if (p >= it.val)    //Insert in the middle
                {
                    newNode.next = it;
                    newNode.prev = it.prev;
                    it.prev.next = newNode;
                    it.prev = newNode;
                    return;
                }
            }

            newNode.next = null;    //Insert in the end
            newNode.prev = it;
            it.next = newNode;
            return;
        }

    public int GetMax()
        {
            if (null == head)
                throw new InvalidOperationException("Access to empty queue.");
            else
            {
                count--;
                int tmpMaxVal = head.val;
                head.prev = null;
                head = head.next;
                return tmpMaxVal;
            }
        }

    public int ShowMax()
        {
            if (null == head)
                throw new InvalidOperationException("Access to empty queue.");
            else
            {
                return head.val;
            }
        }

    public int Count
        {
            get
            {
                //if (null == head)
                //    return 0;

                //Node it = head;
                //int tmpCount = 1;
                //while (null != it.next)
                //{
                //    it = it.next;
                //    tmpCount++;
                //}
                //return tmpCount;

                return count;
            }
        }

    } // EagerPriorityQueue


class HeapPriorityQueue : IPriorityQueue
    {
        const int maxCount = 10000;
        int[] arr;
        int height;
    public HeapPriorityQueue()
        {
            arr = new int[maxCount+1];
            arr[0] = Int32.MaxValue;    //Sentiel
            height = 0;
        }

        public void UpHeap(int ind)
        {
            int v = arr[ind];
            while(arr[ind/2] < v)
            {
                arr[ind] = arr[ind / 2];
                ind = ind / 2;
            }
            arr[ind] = v;
        }

        public void DownHeap(int ind, int n)
        {
            int k = ind * 2;
            int v = arr[ind];
            while(k <= n)
            {
                if(k+1 <= n)
                {
                    if (arr[k+1] > arr[k])
                    {
                        k++;
                    }
                }
                if (arr[k] > v)
                {
                    arr[ind] = arr[k];
                    ind = k;
                    k = 2 * ind;
                }
                else
                    break;
            }
            arr[ind] = v;
        }

    public void Put(int p)
        {
            height++;
            if(arr.Length == Count)
                Array.Resize<int>(ref arr, arr.Length*2);
            
            arr[height] = p;
            UpHeap(height);
        }

    public int GetMax()
        {
            if (0 == height)
                throw new InvalidOperationException("Access to empty queue.");

            int tmpMaxVal = arr[1];
            arr[1] = arr[height];
            height--;
            DownHeap(1, height);
        return tmpMaxVal;
        }

    public int ShowMax()
        {
            if (0 == height)
                throw new InvalidOperationException("Access to empty queue.");

            return arr[1];
        }

    public int Count
        {
        get {
            return height;
            }
        }

    } // HeapPriorityQueue

}

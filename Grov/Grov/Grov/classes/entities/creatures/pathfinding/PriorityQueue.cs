using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Grov
{
    class PriorityQueue<T>
    {
        #region fields
        // ************* Fields ************* //

        private List<PQNode<T>> heap;
        #endregion

        #region properties
        // ************* Properties ************* //

        public int Count { get => heap.Count; }
        public bool IsEmpty { get => Count == 0; }
        #endregion

        #region constructor
        // ************* Constructor ************* //

        public PriorityQueue()
        {
            heap = new List<PQNode<T>>();
        }
        #endregion

        #region methods
        // ************* Methods ************* //

        /// <summary>
        /// Enqueues one piece of data
        /// </summary>
        public void Enqueue(int priority, T data)
        {
            int i = heap.Count;
            heap.Add(new PQNode<T>(priority, data));
            Upheap(i);
        }

        /// <summary>
        /// Dequeues the node with the highest priority
        /// </summary>
        /// <returns>The datum with the highest priority</returns>
        public T Dequeue()
        {
            T ret = Peek().Data;
            Swap(0, heap.Count - 1);
            heap.RemoveAt(heap.Count - 1);

            int i = 0;
            Downheap(i);

            return ret;
        }

        /// <summary>
        /// Upheaps from a given index in the heap
        /// </summary>
        /// <param name="i">starting index</param>
        public void Upheap(int i)
        {
            while (heap[i].Priority < heap[ParentOf(i)].Priority)
            {
                Swap(i, ParentOf(i));
                i = ParentOf(i);
            }
        }

        /// <summary>
        /// Downheaps from a given index in the heap
        /// </summary>
        /// <param name="i">starting index</param>
        public void Downheap(int i)
        {
            int j = LeftChildOf(i);
            do
            {
                j = LeftChildOf(i);
                if (j == -1 || (RightChildOf(i) != -1 && heap[RightChildOf(i)].Priority < heap[LeftChildOf(i)].Priority))
                    j = RightChildOf(i);
                if (j != -1 && heap[j].Priority < heap[i].Priority)
                {
                    Swap(i, j);
                    i = j;
                }
                else
                {
                    break;
                }
            }
            while (j != -1);
        }

        /// <summary>
        /// Returns the datum with the highest priority
        /// </summary>
        /// <returns>the datum with the highest priority</returns>
        public PQNode<T> Peek()
        {
            return heap[0];
        }

        /// <summary>
        /// Returns the index of the parent of a given index
        /// </summary>
        /// <param name="index">the index you want to get the parent of</param>
        /// <returns>the index of the parent</returns>
        private int ParentOf(int index)
        {
            if (index <= 0)
                return 0;
            return (index - 1) / 2;
        }

        /// <summary>
        /// Returns the index of the left child of a given index
        /// </summary>
        /// <param name="index">the index you want to get the child of</param>
        /// <returns>the index of the left child OR -1 if not applicable</returns>
        private int LeftChildOf(int index)
        {
            int ret = 2 * index + 1;
            return (ret >= heap.Count ? -1 : ret);
        }

        /// <summary>
        /// Returns the index of the right child of a given index
        /// </summary>
        /// <param name="index">the index you want to get the child of</param>
        /// <returns>the index of the right child OR -1 if not applicable</returns>
        private int RightChildOf(int index)
        {
            int ret = 2 * index + 2;
            return (ret >= heap.Count ? -1 : ret);
        }

        /// <summary>
        /// Swaps 2 nodes at specified indices
        /// </summary>
        private void Swap(int i1, int i2)
        {
            PQNode<T> temp = heap[i1];
            heap[i1] = heap[i2];
            heap[i2] = temp;
        }

        /// <summary>
        /// Prints a representation of the PriorityQueue
        /// </summary>
        public void Print()
        {
            foreach (PQNode<T> n in heap)
                Console.Write("{0}:{1}", n.Priority, n.Data);
            Console.WriteLine();
        }

        /// <summary>
        /// Changes the priority of a given datum
        /// </summary>
        /// <param name="newPriority">the new priority of the datum</param>
        /// <param name="data">the datum you want to update</param>
        public void ModifyPriority(int newPriority, T data)
        {
            int i;
            for (i = 0; i < heap.Count; i++)
            {
                if (heap[i].Data.Equals(data))
                {
                    heap[i].Priority = newPriority;
                    Upheap(i);
                    Downheap(i);
                    return;
                }
            }

        }

        public void Clear()
        {
            heap.Clear();
        }
        #endregion
    }
}

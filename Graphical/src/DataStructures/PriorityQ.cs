using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Graphical.DataStructures
{
    /// <summary>
    /// Binary Priority Queue
    /// </summary>
    /// <typeparam name="TObject">Type of object to store implementing IEquatable interface</typeparam>
    /// <typeparam name="TValue">Type of value associated with TObject implementing IComparable</typeparam>
    public class PriorityQ<TObject, TValue> : BinaryHeap<HeapItem> where TObject : IEquatable<TObject> where TValue : IComparable
    {
        #region Private Properties
        internal Dictionary<TObject, int> heapIndices { get; private set; }
        #endregion

        #region Public Properties
        /// <summary>
        /// HeapIndices dictionary. For testing purposes only.
        /// </summary>
        public Dictionary<TObject, int> HeapIndices { get { return heapIndices; } } 
        #endregion

        #region Constructors

        /// <summary>
        /// PriorityQ default constructor
        /// </summary>
        /// <param name="heapType">MinHeap or MaxHeap</param>
        public PriorityQ(BinaryHeapType heapType) : base(heapType)
        {
            heapIndices = new Dictionary<TObject, int>(this.Capacity);
        }
        /// <summary>
        /// PriorityQ constructor with initial capacity.
        /// </summary>
        /// <param name="heapType">MinHeap or MaxHeap</param>
        /// <param name="capacity">Initial capacity</param>
        public PriorityQ(BinaryHeapType heapType, int capacity) : base(heapType, capacity)
        {
            heapIndices = new Dictionary<TObject, int>(this.Capacity);
        }

        #endregion

        #region Private Methods
        internal override void Swap(int firstIndex, int secondIndex)
        {
            var firstItem = _heapItems[firstIndex];
            var secondItem = _heapItems[secondIndex];
            _heapItems[firstIndex] = secondItem;
            _heapItems[secondIndex] = firstItem;
            heapIndices[(TObject)firstItem.Item] = secondIndex;
            heapIndices[(TObject)secondItem.Item] = firstIndex;
        }
        #endregion

        /// <summary>
        /// Adds a new TObject to the heap with an associated value
        /// </summary>
        /// <param name="item">TObject to add</param>
        /// <param name="value">Associated value</param>
        public void Add(TObject item, TValue value)
        {
            heapIndices[item] = this.Size;
            base.Add(new HeapItem(item, value));
        }

        /// <summary>
        /// Gets the associated value to a given TObject
        /// </summary>
        /// <param name="item">TObject</param>
        /// <returns>Associated value</returns>
        public TValue GetValue(TObject item)
        {
            if (!heapIndices.ContainsKey(item)) { throw new ArgumentException("Element not existing in Priority Queue"); }
            int heapIndex = heapIndices[item];
            return (TValue)_heapItems[heapIndex].Value;
        }

        /// <summary>
        /// Updates the value associate to the given item and restores the Heap property
        /// </summary>
        /// <param name="item">TObject to update value</param>
        /// <param name="newValue">New value</param>
        public void UpdateValue(TObject item, TValue newValue)
        {
            if (!heapIndices.ContainsKey(item)) { throw new ArgumentException("Element not existing in Priority Queue"); }
            int heapIndex = heapIndices[item];
            HeapItem heapItem = _heapItems[heapIndex];
            IComparable currentValue = heapItem.Value;
            heapItem.SetValue(newValue);

            int comparison = newValue.CompareTo(currentValue);

            if ( (HeapType == BinaryHeapType.MinHeap && comparison < 0) ||
                (HeapType == BinaryHeapType.MaxHeap && comparison > 0))
            {
                HeapifyUp(heapIndex);
            }
            else
            {
                HeapifyDown(heapIndex);
            }
        }

        /// <summary>
        /// Returns the value associated to the first item on the Heap
        /// </summary>
        /// <returns>Associated value</returns>
        public TValue PeekValue()
        {
            return (TValue)_heapItems[0].Value;
        }

        /// <summary>
        /// Returns the first item on the Heap
        /// </summary>
        /// <returns></returns>
        public new TObject Peek()
        {
            return (TObject)base.Peek().Item;
        }

        /// <summary>
        /// Returns the first item 
        /// </summary>
        /// <returns></returns>
        public new TObject Take()
        {
            TObject first = (TObject)base.Take().Item;
            heapIndices.Remove(first);
            return first;
        }

        /// <summary>
        /// Removes all items from the queue
        /// </summary>
        public override void Clear()
        {
            heapIndices.Clear();
            base.Clear();
        }
    }

    /// <summary>
    /// MinBinary Priority Queue
    /// </summary>
    /// <typeparam name="TObject">Type of object to store implementing IEquatable interface</typeparam>
    /// <typeparam name="TValue">Type of value associated with TObject implementing IComparable</typeparam>
    public class MinPriorityQ<TObject, TValue> : PriorityQ<TObject, TValue> where TObject : IEquatable<TObject> where TValue : IComparable
    {
        #region Constructor
        /// <summary>
        /// MinPriorityQ default constructor
        /// </summary>
        public MinPriorityQ() : base(BinaryHeapType.MinHeap) { }

        /// <summary>
        /// MinPriorityQ constructor with initial capacity.
        /// </summary>
        /// <param name="capacity">Initial capacity</param>
        public MinPriorityQ(int capacity) : base(BinaryHeapType.MinHeap, capacity) { }

        #endregion
    }

    /// <summary>
    /// MaxBinary Priority Queue
    /// </summary>
    /// <typeparam name="TObject">Type of object to store implementing IEquatable interface</typeparam>
    /// <typeparam name="TValue">Type of value associated with TObject implementing IComparable</typeparam>
    public class MaxPriorityQ<TObject, TValue> : PriorityQ<TObject, TValue> where TObject : IEquatable<TObject> where TValue : IComparable
    {
        #region Constructor
        /// <summary>
        /// MaxPriorityQ default constructor
        /// </summary>
        public MaxPriorityQ() : base(BinaryHeapType.MaxHeap) { }

        /// <summary>
        /// MaxPriorityQ constructor with initial capacity.
        /// </summary>
        /// <param name="capacity">Initial capacity</param>
        public MaxPriorityQ(int capacity) : base(BinaryHeapType.MaxHeap, capacity) { }

        #endregion
    }
}

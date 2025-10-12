/// <summary>
/// Abstract base for items stored in a binary heap.
/// </summary>
public abstract class HeapItem<T> : System.IComparable<T>
{
    /// <summary>
    /// Managed by Heap; used to track the item’s position in the internal array.
    /// </summary>
    public int HeapIndex { get; set; }

    /// <summary>
    /// Compare this item to another for heap ordering.
    /// Return negative if this has higher priority.
    /// </summary>
    public abstract int CompareTo(T other);
}

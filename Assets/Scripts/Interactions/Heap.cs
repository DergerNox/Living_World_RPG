using System;

/// <summary>
/// A simple binary min-heap for fast open-set operations in A*.
/// </summary>
public class Heap<T> where T : HeapItem<T>
{
    private T[] items;
    private int count;

    public Heap(int maxSize)
    {
        items = new T[maxSize];
        count = 0;
    }

    public void Add(T item)
    {
        item.HeapIndex = count;
        items[count] = item;
        SortUp(item);
        count++;
    }

    public T RemoveFirst()
    {
        if (count == 0) throw new InvalidOperationException("Heap is empty");
        T first = items[0];
        count--;
        items[0] = items[count];
        items[0].HeapIndex = 0;
        SortDown(items[0]);
        return first;
    }

    public void UpdateItem(T item)
    {
        SortUp(item);
    }

    public int Count => count;

    public bool Contains(T item)
    {
        if (item.HeapIndex < 0 || item.HeapIndex >= count) return false;
        return items[item.HeapIndex].Equals(item);
    }

    private void SortDown(T item)
    {
        while (true)
        {
            int left = item.HeapIndex * 2 + 1;
            int right = left + 1;
            int smallest = item.HeapIndex;

            if (left < count && items[left].CompareTo(items[smallest]) < 0)
                smallest = left;
            if (right < count && items[right].CompareTo(items[smallest]) < 0)
                smallest = right;

            if (smallest != item.HeapIndex)
                Swap(item, items[smallest]);
            else
                break;
        }
    }

    private void SortUp(T item)
    {
        int parent = (item.HeapIndex - 1) / 2;
        while (item.HeapIndex > 0 && items[parent].CompareTo(item) > 0)
        {
            Swap(items[parent], item);
            parent = (item.HeapIndex - 1) / 2;
        }
    }

    private void Swap(T a, T b)
    {
        items[a.HeapIndex] = b;
        items[b.HeapIndex] = a;
        int ai = a.HeapIndex;
        a.HeapIndex = b.HeapIndex;
        b.HeapIndex = ai;
    }
}

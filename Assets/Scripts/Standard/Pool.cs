using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// A stack which can automatically allocate new items if it runs out
// It is the user's responsibility to return items to the stack once they are done with them
public class Pool<T> where T : new()
{
    int sizeIncrease;
    int maxSize;
    int size;
    public Pool(int size, int maxSize, int sizeIncrease = 10)
    {
        this.size = 0;
        this.sizeIncrease = sizeIncrease;
        this.maxSize = maxSize;
        Add(size);
    }
    Stack<T> stash = new Stack<T>();
    public T Get()
    {
        if (stash.Count > 0) return stash.Pop();
        else if (size < maxSize)
        {
            Debug.Log("Inreasing pool size to " + size + ".");
            size += sizeIncrease;
            Add(sizeIncrease);
            return stash.Pop();
        }
        throw new System.Exception("Pool reached max size");
    }
    public void Return(T item)
    {
        stash.Push(item);
    }

    public void Add(int count)
    {
        for (int i = 0; i < count; i++)
            stash.Push(new T());
        size += count;
    }
    public void Add(T item)
    {
        stash.Push(item);
        size ++;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PreallocatedList<T> where T: class, new()
{
    List<T> stash;
    int next = 0;

    public PreallocatedList(int size)
    {
        stash = new List<T>(size);
        for(int i  = 0; i < size; i++)
        {
            stash.Add(new T());
        }
    }

    public void Reset()
    {
        next = 0;
    }

    public T Get()
    {
        Debug.Assert(next < stash.Count);
        T rv = stash[next];
        next++;
        return rv;
    }
}

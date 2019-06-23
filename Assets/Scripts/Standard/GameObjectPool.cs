using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Pre-allocates GameObjects from a given prefab. Get one using Get(). Please Return() them when you are done.
public class GameObjectPool
{
    // this is badly named as it reports total allocations instead of remaining objects in the pool
    public int size;
    int maxSize;
    GameObject prefab;
    GameObject container;
    Stack<GameObject> pool = new Stack<GameObject>();
    public GameObjectPool(GameObject prefab, int size, GameObject container)
    {
        this.container = container;
        this.size = 0;
        this.prefab = prefab;
        Add(size);
        // add this as a parameter
        maxSize = size * 2;
    }
    GameObject Make()
    {
        GameObject go = GameObject.Instantiate(prefab);
        if (container != null) go.transform.SetParent(container.transform);
        return go;
    }
    void Add(int count)
    {
        for (int i = 0; i < count; i++)
        {
            Return(Make());
        }
        size += count;
    }
    public GameObject Get()
    {
        if (pool.Count == 0 && size < maxSize)
        {
            Debug.Log("Adding 10 more " + prefab.name + "s!");
            Add(10);
        }

        return pool.Pop();
    }

    public void Return(GameObject go)
    {
        pool.Push(go);
        go.SetActive(false);
    }
}

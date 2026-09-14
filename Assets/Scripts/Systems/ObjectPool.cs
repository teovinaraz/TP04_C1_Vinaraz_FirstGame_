using System.Collections.Generic;
using UnityEngine;

public class ObjectPool
{
    private readonly Stack<GameObject> available = new Stack<GameObject>();
    private readonly Transform parent;
    private readonly System.Func<GameObject> factory;

    public ObjectPool(int initialSize, Transform parent, System.Func<GameObject> factory)
    {
        this.parent = parent;
        this.factory = factory;
        for (int i = 0; i < initialSize; i++)
        {
            GameObject item = Create();
            item.SetActive(false);
            available.Push(item);
        }
    }

    private GameObject Create()
    {
        GameObject item = factory();
        item.transform.SetParent(parent);
        return item;
    }

    public GameObject Get()
    {
        GameObject item = available.Count > 0 ? available.Pop() : Create();
        item.SetActive(true);
        return item;
    }

    public void Release(GameObject item)
    {
        if (item == null) return;
        item.SetActive(false);
        available.Push(item);
    }
}

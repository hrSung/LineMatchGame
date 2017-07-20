using System.Collections.Generic;
using UnityEngine;

public class ObjectPool : MonoBehaviour
{
    public PoolItem item_prefab;
    private Stack<PoolItem> item_pools = new Stack<PoolItem>();

    private int pre_pool_count = 100;


    #region MonoBehaviour Function

    void Awake()
    {
        InitPool();
    }

    #endregion

    private void InitPool()
    {
        for (int i = 0; i < pre_pool_count; i++)
        {
            PoolItem item = Instantiate(item_prefab, transform);
            item_pools.Push(item);
        }
    }

    public PoolItem GetItem()
    {
        if(item_pools.Count < 1)
        {
            PoolItem item = Instantiate(item_prefab, transform);
            item.Show();
            return item;
        }
        else
        {
            PoolItem item = item_pools.Pop();
            item.ResetItem();
            item.Show();
            return item;
        }
    }

    public void RealeseItem(PoolItem item)
    {
        item.Hide();
        item_pools.Push(item);
    }
}

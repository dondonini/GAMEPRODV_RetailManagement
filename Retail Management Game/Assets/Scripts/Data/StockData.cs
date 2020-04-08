using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "New Stock")]
public class StockData : ScriptableObject
{
    public StockTypes stockType = StockTypes.None;
    public GameObject stockVisual;
    public GameObject[] prefabs;
    public int stockPrice = 1;
    public Sprite stockThumbnail;

    public StockTypes GetStockType()
    {
        return stockType;
    }

    public int GetStockTypePrice()
    {
        return stockPrice;
    }

    public GameObject GetStockVisual()
    {
        return stockVisual;
    }

    public GameObject[] GetPrefabs()
    {
        return prefabs;
    }

    public Sprite GetThumbnail()
    {
        return stockThumbnail;
    }
}

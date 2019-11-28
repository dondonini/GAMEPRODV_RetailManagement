using UnityEngine;

[System.Serializable]
public class StockToPrefabType
{
    [SerializeField] int stockPrice = 0;
    [SerializeField] StockTypes stockType = StockTypes.None;
    [SerializeField] Material stockThumbnail = null;
    [SerializeField] GameObject[] prefabs = null;

    public StockTypes GetStockType()
    {
        return stockType;
    }

    public int GetStockTypePrice()
    {
        return stockPrice;
    }

    public GameObject[] GetPrefabs()
    {
        return prefabs;
    }

    public Material GetThumbnail()
    {
        return stockThumbnail;
    }
}

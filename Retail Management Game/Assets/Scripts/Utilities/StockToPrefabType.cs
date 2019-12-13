using UnityEngine;

[System.Serializable]
public class StockToPrefabType
{
    [SerializeField] int stockPrice = 0;
    [SerializeField] StockTypes stockType = StockTypes.None;
    [SerializeField] Sprite stockThumbnail = null;
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

    public Sprite GetThumbnail()
    {
        return stockThumbnail;
    }
}

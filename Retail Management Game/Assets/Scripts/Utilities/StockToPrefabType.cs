using UnityEngine;

[System.Serializable]
public class StockToPrefabType
{
    public StockTypes stockType = StockTypes.None;
    public GameObject[] prefabs = null;

    public StockTypes GetStockType()
    {
        return stockType;
    }
}

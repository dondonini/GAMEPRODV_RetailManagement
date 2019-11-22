using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StockCrate : MonoBehaviour
{
    [SerializeField] StockTypes stockType = StockTypes.None;
    [SerializeField] int stockQuantity = 0;
    [SerializeField] int maxQuantity = 10;
    [SerializeField] Image[] imageFaces;

    MapManager mapManager = null;

    StockTypes previousStockType = StockTypes.None;

    private void Start()
    {
        mapManager = MapManager.GetInstance();

        UpdateThumbnail();
    }

    private void Update()
    {
        if (stockType != previousStockType)
        {
            // Stock changed!
            UpdateThumbnail();
        }

        previousStockType = stockType;
    }

    public StockTypes GetStockType()
    {
        return stockType;
    }

    public int GetQuantity()
    {
        return stockQuantity;
    }

    public bool AddQuantity()
    {
        if (stockQuantity >= maxQuantity)
            return false;
        else
        {
            stockQuantity++;
            return true;
        }
    }

    private void UpdateThumbnail()
    {
        Sprite thumbnail = mapManager.GetStockTypeThumbnail(stockType);

        for (int i = 0; i < imageFaces.Length; i++)
        {
            imageFaces[i].sprite = thumbnail;
        }
    }
}

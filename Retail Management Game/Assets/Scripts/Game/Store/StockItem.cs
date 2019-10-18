using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StockItem : MonoBehaviour
{
    [SerializeField] private StockTypes stockType;
    [SerializeField] private int stockAmount = 0;
    [SerializeField] private int price = 10;

    public StockTypes GetStockType()
    {
        return stockType;
    }

    public int GetPrice()
    {
        return price;
    }
}
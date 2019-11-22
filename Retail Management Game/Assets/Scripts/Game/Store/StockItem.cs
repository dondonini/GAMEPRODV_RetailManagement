using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StockItem : MonoBehaviour
{
    [SerializeField] private StockTypes stockType = StockTypes.None;

    public StockTypes GetStockType()
    {
        return stockType;
    }
}
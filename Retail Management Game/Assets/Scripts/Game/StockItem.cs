using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StockItem : MonoBehaviour
{
    [SerializeField] private StockTypes stockType;

    public StockTypes GetStockType()
    {
        return stockType;
    }
}
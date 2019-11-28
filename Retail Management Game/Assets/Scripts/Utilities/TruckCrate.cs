using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class TruckCrate
{
    public StockTypes stockType;
    public int stockAmount;

    public TruckCrate(StockTypes _stockType, int _stockAmount)
    {
        stockType = _stockType;
        stockAmount = _stockAmount;
    }
}

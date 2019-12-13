using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameInfo : MonoBehaviour
{
    static GameInfo instance;

    private void Awake()
    {
        if (instance)
        {
            Debug.Log("GameInfo already exists! I'ma go, bye!");
            Destroy(this);
        }
        else
        {
            instance = this;
            DontDestroyOnLoad(this);
        }
    }

    public static GameInfo GetInstance()
    {
        return instance;
    }

    [SerializeField]List<StockTypes> productsSold = new List<StockTypes>();

    public void ResetGameInfo()
    {
        productsSold.Clear();
    }

    public void AddProductSold(StockTypes soldType)
    {
        productsSold.Add(soldType);
    }

    public StockTypes[] GetProductsSold()
    {
        return productsSold.ToArray();
    }
}

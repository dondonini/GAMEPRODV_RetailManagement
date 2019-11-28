using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StockCrate : MonoBehaviour
{
    [SerializeField] StockTypes stockType = StockTypes.None;
    [SerializeField] int stockQuantity = 0;
    [SerializeField] int maxQuantity = 10;
    [SerializeField] float billboardDistance = 1.0f;
    [SerializeField] Renderer billboardRenderer = null;

    MapManager mapManager = null;

    StockTypes previousStockType = StockTypes.None;

    GameObject firstClaim = null;

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

        if (stockQuantity == 0)
            Destroy(gameObject);

        previousStockType = stockType;
    }

    private void LateUpdate()
    {
        billboardRenderer.transform.position = transform.position + new Vector3(0.25f, billboardDistance, -0.25f);
    }

    public StockTypes GetStockType()
    {
        return stockType;
    }

    public int GetQuantity()
    {
        return stockQuantity;
    }

    public int SetQuantity(int amount)
    {
        int difference = stockQuantity - amount;

        stockQuantity = amount;

        return difference;
    }

    public int AddQuantity(int amount)
    {
        int remainingStock = amount - (maxQuantity - stockQuantity);

        stockQuantity += amount;
        if (stockQuantity > maxQuantity) stockQuantity = maxQuantity;

        return (remainingStock > 0) ? remainingStock : 0;
    }

    private void UpdateThumbnail()
    {
        Material thumbnail = mapManager.GetStockTypeThumbnail(stockType);

        billboardRenderer.material = thumbnail;
    }
    public GameObject IsClaimed()
    {
        return firstClaim;
    }

    public void ClaimItem(GameObject other)
    {
        firstClaim = other;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShelfContainer : MonoBehaviour
{
    [SerializeField] private StockTypes shelfStockType = StockTypes.None;
    [SerializeField] private int stockAmount = 0;
    [SerializeField] private int shelfSize = 10;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void SetShelfStockType(StockTypes stockType)
    {
        shelfStockType = stockType;
    }

    public StockTypes GetShelfStockType()
    {
        return shelfStockType;
    }

    void EmptyShelf()
    {
        if (stockAmount == 0)
        {
            shelfStockType = StockTypes.None;
        }
    }

    public int GetStock(int amount = 1)
    {
        // Check if there's stock to give
        if (stockAmount == 0)
        {
            // Shelf is empty - give nothing
            return 0;
        }
        else if (stockAmount < amount)
        {
            // Shelf is almost empty - give remaining amount
            int givingAmount = stockAmount;
            EmptyShelf();
            return givingAmount;
        }
        else
        {
            // Shelf has enough stock requested - give amount
            return amount;
        }
    }

    public void AddStock(ref int amount, StockTypes stockType)
    {
        // If this ever happens, you're actually dumb
        if (amount <= 0)
        {
            Debug.LogWarning("Uhhhhh... why are you adding nothing to the shelf? Tf?");
        }

        // Check if shelf has stock already
        if (shelfStockType == StockTypes.None)
        {
            // Apply new stock type
            shelfStockType = stockType;
        }

        // Check if existing shelf stock matching what you're adding
        else if (stockType != shelfStockType)
        {
            // Stocks do not match! - do nothing
            return;
        }

        // Check if there's enough space to add amount of stock
        if (amount > shelfSize)
        {
            // Add max of shelf

            // Remove amount
            amount -= shelfSize;

            // Max out shelf
            stockAmount = shelfSize;
        }
        else
        {
            // Add amount
            stockAmount += amount;

            amount = 0;

        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShelfContainer : MonoBehaviour
{
    [SerializeField] private StockTypes shelfStockType = StockTypes.None;
    [SerializeField] private int stockAmount = 0;
    [SerializeField] private int shelfSize = 10;

    [Header("Pickup Faces")]
    [Rename("Front")]
    [SerializeField] private bool allowPickup_F = true;
    [Rename("Back")]
    [SerializeField] private bool allowPickup_B = false;
    [Rename("Left")]
    [SerializeField] private bool allowPickup_L = false;
    [Rename("Right")]
    [SerializeField] private bool allowPickup_R = false;

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0.0f, 1.0f, 0.0f, 0.1f);

        Bounds boundsOfSelf = EssentialFunctions.GetMaxBounds(gameObject);

        if (allowPickup_F)
        {
            Gizmos.DrawCube(boundsOfSelf.center + (transform.forward * 0.5f), new Vector3(boundsOfSelf.size.x, boundsOfSelf.size.y, 0.0f));
        }

        if (allowPickup_B)
        {
            Gizmos.DrawCube(boundsOfSelf.center + (transform.forward * -0.5f), new Vector3(boundsOfSelf.size.x, boundsOfSelf.size.y, 0.0f));
        }

        if (allowPickup_L)
        {
            Gizmos.DrawCube(boundsOfSelf.center + (transform.right * -0.5f), new Vector3(0.0f, boundsOfSelf.size.y, boundsOfSelf.size.z));
        }

        if (allowPickup_R)
        {
            Gizmos.DrawCube(boundsOfSelf.center + (transform.right * 0.5f), new Vector3(0.0f, boundsOfSelf.size.y, boundsOfSelf.size.z));
        }
    }

    private void OnValidate()
    {
        // Always have at least one face available for pickup
        if (!allowPickup_F && !allowPickup_B && !allowPickup_L && !allowPickup_R)
        {
            allowPickup_F = true;
        }
    }

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

    /// <summary>
    /// Returns pickup positions
    /// </summary>
    /// <returns></returns>
    public Vector3[] GetPickupPosition()
    {
        List<Vector3> pickupPositions = new List<Vector3>();

        if (allowPickup_F)
        {
            pickupPositions.Add(transform.position + transform.forward);
        }

        if (allowPickup_B)
        {
            pickupPositions.Add(transform.position + -transform.forward);
        }

        if (allowPickup_L)
        {
            pickupPositions.Add(transform.position + -transform.right);
        }

        if (allowPickup_R)
        {
            pickupPositions.Add(transform.position + transform.right);
        }

        return pickupPositions.ToArray();
    }
}

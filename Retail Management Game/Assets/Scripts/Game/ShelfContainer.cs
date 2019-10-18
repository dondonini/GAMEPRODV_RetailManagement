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

    const float collisionSensitivity = 2.0f;

    Vector3[] pickupPositions = null;

    const float stuckObjectMaxTime = 1.0f;
    Dictionary<Transform, float> stuckObjects = new Dictionary<Transform, float>();

    MapManager mapManager;

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

        UpdatePickupPositionsArray();
    }

    private void OnCollisionEnter(Collision collision)
    {
        ContactPoint contact = collision.GetContact(0);

        GameObject other = contact.otherCollider.transform.root.gameObject;

        Debug.Log(collision.relativeVelocity.magnitude);

        // Add products to the shelf if it hits it hard enough
        if (collision.relativeVelocity.magnitude < collisionSensitivity) return;

        if (other.CompareTag("Product"))
        {
            int result = AddStock(other.GetComponent<StockItem>().GetStockType());
            
            if (result == 0)
                Destroy(other);
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        foreach(ContactPoint contactPoint in collision.contacts)
        {
            Transform other = contactPoint.otherCollider.transform.root;

            if (other == stuckObjects.ContainsKey(other))
            {
                float timer = 0.0f;
                stuckObjects.TryGetValue(other, out timer);

                if (timer <= 0.0f)
                {
                    Debug.Log("LOL! " + other + " is stuck in " + transform + "! XD");
                }
                else
                {
                    //stuckObjects.
                }
            }
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        UpdatePickupPositionsArray();

        // Get MapManager
        mapManager = MapManager.GetInstance();
    }

    public StockTypes ShelfStockType
    {
        get
        {
            return shelfStockType;
        }
        set
        {
            shelfStockType = value;
            if (value == StockTypes.None)
            {
                EmptyShelf();
            }
        }
    }

    public int MaxShelfAmount()
    {
        return shelfSize;
    }

    public int StockAmount()
    {
        return stockAmount;
    }

    void EmptyShelf()
    {
        stockAmount = 0;
        ShelfStockType = StockTypes.None;
    }

    public int GetStock(int amount = 1)
    {
        // Check if there's stock to give
        if (IsEmpty())
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
            stockAmount -= amount;
            // Shelf has enough stock requested - give amount
            return amount;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="amount">The amount of stock to add onto shelf</param>
    /// <param name="stockType">The stock type that is being added to the shelf</param>
    /// <returns>The remainder</returns>
    public int AddStock(int amount, StockTypes stockType)
    {
        int tempAmount = amount;

        // If this ever happens, you're actually dumb
        if (tempAmount <= 0)
        {
            Debug.LogWarning("Uhhhhh... why are you adding nothing to the shelf? Tf?");
        }

        // Check if shelf has stock already
        if (ShelfStockType == StockTypes.None)
        {
            // Apply new stock type
            ShelfStockType = stockType;
        }

        // Check if existing shelf stock doesn't match what you're adding
        else if (stockType != ShelfStockType)
        {
            // Stocks do not match! - do nothing
            return tempAmount;
        }

        // Check if there's enough space to add amount of stock
        if (tempAmount > shelfSize)
        {
            // Add max of shelf

            // Remove amount
            tempAmount -= shelfSize;

            // Max out shelf
            stockAmount = shelfSize;
        }
        else
        {
            // Add amount
            stockAmount += tempAmount;

            tempAmount = 0;
        }

        mapManager.UpdateAvailableStockTypes();

        return tempAmount;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="stockType">The stock type that is being added to the shelf</param>
    /// <returns>The remainder</returns>
    public int AddStock(StockTypes stockType)
    {
        return AddStock(1, stockType);
    }

    /// <summary>
    /// Update pickup positions
    /// </summary>
    void UpdatePickupPositionsArray()
    {
        List<Vector3> foundPositions = new List<Vector3>();

        if (allowPickup_F)
        {
            foundPositions.Add(transform.position + transform.forward);
        }

        if (allowPickup_B)
        {
            foundPositions.Add(transform.position + -transform.forward);
        }

        if (allowPickup_L)
        {
            foundPositions.Add(transform.position + -transform.right);
        }

        if (allowPickup_R)
        {
            foundPositions.Add(transform.position + transform.right);
        }

        pickupPositions = null;
        pickupPositions = foundPositions.ToArray();
    }

    /// <summary>
    /// Returns pickup positions
    /// </summary>
    /// <returns></returns>
    public Vector3[] GetPickupPositions()
    {
        return pickupPositions;
    }

    public bool IsEmpty()
    {
        return stockAmount == 0;
    }
}

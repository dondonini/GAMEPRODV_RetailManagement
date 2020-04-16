using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
using System.Linq;
using UnityEngine.Serialization;

public class ShelfContainer : MonoBehaviour
{
    [Header("Shelf Status")]
    [SerializeField] private StockData stock;
    [SerializeField] private StockTypes shelfStockType = StockTypes.None;
    [SerializeField] private int stockAmount = 0;
    [SerializeField] private int shelfSize = 10;
    [SerializeField] private float playerDetectionDistance = 1.0f;
    [SerializeField] private Bounds boundsOfShelf;

    [FormerlySerializedAs("allowPickup_F")]
    [Header("Pickup Faces")]

    [Rename("Front")]
    [SerializeField] private bool allowPickupF = true;

    [FormerlySerializedAs("allowPickup_B")]
    [Rename("Back")]
    [SerializeField] private bool allowPickupB = false;

    [FormerlySerializedAs("allowPickup_L")]
    [Rename("Left")]
    [SerializeField] private bool allowPickupL = false;

    [FormerlySerializedAs("allowPickup_R")]
    [Rename("Right")]
    [SerializeField] private bool allowPickupR = false;

    private const float COLLISION_SENSITIVITY = 2.0f;

    [Header("References")]
    [SerializeField]
    private TextMeshProUGUI stockNumIndicator = null;
    [SerializeField] private Animator billboardAnimator = null;
    [SerializeField] private ShelfVisual shelfVisual = null;
    [SerializeField] private GameObject stockToaster = null;

    private Vector3[] _pickupPositions = null;
    private int _previousStockAmount = 0;

    private List<ShelfContainer> _adjacentShelves = new List<ShelfContainer>();

    private MapManager _mapManager;

    private Bounds _shelfBounds;
    private static readonly int showNum = Animator.StringToHash("ShowNum");

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0.0f, 1.0f, 0.0f, 0.5f);

        boundsOfShelf = GetShelfBounds();

        if (allowPickupF)
        {
            Gizmos.DrawCube(boundsOfShelf.center + (transform.forward * 0.5f), new Vector3(0.1f, boundsOfShelf.size.y, 0.1f));
        }

        if (allowPickupB)
        {
            Gizmos.DrawCube(boundsOfShelf.center + (transform.forward * -0.5f), new Vector3(0.1f, boundsOfShelf.size.y, 0.1f));
        }

        if (allowPickupL)
        {
            Gizmos.DrawCube(boundsOfShelf.center + (transform.right * -0.5f), new Vector3(0.1f, boundsOfShelf.size.y, 0.1f));
        }

        if (allowPickupR)
        {
            Gizmos.DrawCube(boundsOfShelf.center + (transform.right * 0.5f), new Vector3(0.1f, boundsOfShelf.size.y, 0.1f));
        }

        Gizmos.color = new Color(0.0f, 1.0f, 1.0f, 0.5f);
        Gizmos.DrawWireCube(boundsOfShelf.center, boundsOfShelf.size + new Vector3(playerDetectionDistance, 0.0f, playerDetectionDistance));

        
        foreach(ShelfContainer shelf in GetAllAdjacentShelves())
        {
            if (shelf == this)
                Gizmos.color = new Color(0.0f, 0.0f, 1.0f, 1.0f);
            else
                Gizmos.color = new Color(0.0f, 0.0f, 1.0f, 0.25f);

            Gizmos.DrawCube(shelf.transform.position + new Vector3(0.0f, 2.0f, 0.0f), new Vector3(0.1f, 0.1f, 0.1f));
        }
    }

    private void OnValidate()
    {
        // Always have at least one face available for pickup
        if (!allowPickupF && !allowPickupB && !allowPickupL && !allowPickupR)
        {
            allowPickupF = true;
        }

        stockAmount = Mathf.Clamp(stockAmount, 0, shelfSize);

        UpdatePickupPositionsArray();
        

        _adjacentShelves.Clear();
        _adjacentShelves.AddRange(GetAdjacentShelves());

        // Update Stock
        ShelfVisual[] c = transform.GetComponentsInChildren<ShelfVisual>();

        if (c.Length > 0)
        {
            UnityEditor.EditorApplication.delayCall += () =>
            {
                for (int i = 0; i < c.Length; i++)
                {
                    DestroyImmediate(c[i].gameObject);
                }
            };  
        }

        shelfVisual = null;
        shelfStockType = StockTypes.None;

        if (stock)
        {
            // Setup stats
            shelfStockType = stock.GetStockType();
            GameObject newStockVisual = null;

            // Setup Visuals
            UnityEditor.EditorApplication.delayCall += () =>
            {
                newStockVisual = Instantiate(stock.GetStockVisual(), transform, true);

                shelfVisual = newStockVisual.GetComponent<ShelfVisual>();

                newStockVisual.transform.localPosition = Vector3.zero;
            };
        }

        UpdateVisuals();
    }

    #region Collisions

    private void OnCollisionEnter(Collision collision)
    {
        ContactPoint contact = collision.GetContact(0);

        GameObject other = contact.otherCollider.transform.root.gameObject;

        //Debug.Log(collision.relativeVelocity.magnitude);

        // Add products to the shelf if it hits it hard enough
        if (collision.relativeVelocity.magnitude < COLLISION_SENSITIVITY) return;

        StockItem stockItem = other.GetComponent<StockItem>();
        int result = 0;

        if (other.CompareTag("Product") || other.CompareTag("StockCrate"))
        {
            // Claim the product before any other shelf can >:D
            if (stockItem.IsClaimed()) return;
            else stockItem.ClaimItem(gameObject);

            result = AddStock(stockItem.GetStockType());
        }

        Debug.Log(result);

        if (result == 0)
            Destroy(other);
        else
            stockItem.UnclaimItem(gameObject);
    }

    private void OnCollisionStay(Collision collision)
    {
        foreach (ContactPoint contactPoint in collision.contacts)
        {
            Transform other = contactPoint.otherCollider.transform.root;

            int result = 0;

            StockItem stockItem = other.GetComponent<StockItem>();

            if (other.CompareTag("Product") || other.CompareTag("StockCrate"))
            {
                // Claim the product before any other shelf can >:D
                if (stockItem.IsClaimed()) return;
                else stockItem.ClaimItem(gameObject);

                result = AddStock(stockItem.GetStockType());
            }

            Debug.Log(result);

            if (result == 0)
                Destroy(other.gameObject);
            else
            {
                // Unclaim item
                stockItem.UnclaimItem(gameObject);

                // Get the damn item out of this shelf
                if (allowPickupF)
                {
                    other.transform.position = boundsOfShelf.center + (transform.forward * 0.5f);
                }
                else if (allowPickupB)
                {
                    other.transform.position = boundsOfShelf.center + (transform.forward * -0.5f);
                }

                else if (allowPickupL)
                {
                    other.transform.position = boundsOfShelf.center + (transform.forward * -0.5f);
                }
                else
                {
                    other.transform.position = boundsOfShelf.center + (transform.right * 0.5f);
                }
            }
        }
    }

    #endregion

    // Start is called before the first frame update
    private void Start()
    {
        UpdatePickupPositionsArray();

        // Get MapManager
        _mapManager = MapManager.GetInstance();

        boundsOfShelf = GetShelfBounds();

        _adjacentShelves.Clear();
        _adjacentShelves.AddRange(GetAdjacentShelves());

        _shelfBounds = EssentialFunctions.GetMaxBounds(gameObject);

        _previousStockAmount = stockAmount;

        UpdateStockBillboard();
        UpdateVisuals();

        //Debug.Log(GetAllAdjacentShelves().Length);
    }

    private void Update()
    {
        // Update text when stock changes
        if (stockAmount != _previousStockAmount)
        {
            UpdateStockBillboard();
            UpdateVisuals();
        }

        // Show billboard when player is near by or when low in stock
        if (stockAmount <= 2)
        { 
            billboardAnimator.SetBool(showNum, true);
        }
        else
        {
            billboardAnimator.SetBool(showNum, IsPlayerInDetectionBox());
        }
        
        _previousStockAmount = stockAmount;
    }

    #region Getters and Setters

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
            PushStockToaster(-givingAmount);
            return givingAmount;
        }
        else
        {
            stockAmount -= amount;
            PushStockToaster(-amount);
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

        // If this ever happens, you're actually dumb
        if (amount <= 0)
            Debug.LogWarning("Uhhhhh... why are you adding nothing to the shelf? Tf?");

        // Check if shelf has stock already
        if (ShelfStockType == StockTypes.None)
            // Apply new stock type
            ShelfStockType = stockType;

        // Check if existing shelf stock doesn't match what you're adding
        else if (stockType != ShelfStockType)
        {
            // Stocks do not match! - do nothing

            Debug.Log("StockType doesn't match shelf! " + amount);
            return amount;
        }

        // Fill shelves
        int availableShelfSpace = shelfSize - stockAmount;

        stockAmount += amount;
        if (stockAmount > shelfSize) 
            stockAmount = shelfSize;

        int remainingStock = amount - availableShelfSpace;

        if (remainingStock < 0) 
            remainingStock = 0;

        _mapManager.UpdateAvailableStockTypes();

        // Add to adjacent shelves
        if (remainingStock > 0)
        {
            // No shelves adjacent - just return the remaining stock
            if (_adjacentShelves.Count == 0) 
                return remainingStock;

            // All adjacent shelves are full - return the remaining stock
            if (IsAllAdjacentShelvesFull())
                return remainingStock;

            // Add remaining stock to adjacent shelves
            else
            {
                ShelfContainer[] shelvesWithSpace = GetAdjacentShelvesWithSpace();

                // Add all remaining stock to neighbouring shelf
                if (shelvesWithSpace.Length == 1)
                {
                    remainingStock = shelvesWithSpace[0].AddStock(remainingStock, stockType);

                    Debug.Log("Current Shelf Remaining: " + remainingStock);
                }

                // Evenly divide stock to all adjacent shelves including remaining
                else
                {
                    int newRemainingStock = 0;

                    for (int i = 0; i < remainingStock; i++)
                    {
                        newRemainingStock += shelvesWithSpace[i % shelvesWithSpace.Length].AddStock(stockType);
                    }

                    remainingStock = newRemainingStock;
                    //int shelfQuantity = shelvesWithSpace.Length;
                    //int shelfStockDividedQuantity = Mathf.FloorToInt(remainingStock / shelfQuantity);
                    //int shelfStockRemaining = remainingStock % shelfQuantity;

                    //int newRemaining = 0;

                    //for (int i = 0; i < shelvesWithSpace.Length; i++)
                    //{
                    //    int amountToAdd = shelfStockDividedQuantity;

                    //    // Add remaining stock to shelves
                    //    if (i < shelfStockRemaining)
                    //    {
                    //        amountToAdd++;
                    //    }

                    //    newRemaining += shelvesWithSpace[i].AddStock(amountToAdd, stockType);
                    //}

                    //Debug.Log("Current Shelf Remaining: " + remainingStock + " Total Remaining Stock: " + newRemaining);
                }
            }
        }

        PushStockToaster(amount - remainingStock);
        return remainingStock;
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

    public int AddCrate(StockCrate crate)
    {
        int remainingStock = crate.SetQuantity(AddStock(crate.GetQuantity(), crate.GetStockType()));
        if (remainingStock < 0) remainingStock = 0;

        crate.UnclaimItem(gameObject);

        return remainingStock;
    }

    #endregion

    #region Helpers

    private void EmptyShelf()
    {
        stockAmount = 0;
        ShelfStockType = StockTypes.None;
    }

    /// <summary>
    /// Update pickup positions
    /// </summary>
    private void UpdatePickupPositionsArray()
    {
        List<Vector3> foundPositions = new List<Vector3>();

        if (allowPickupF)
        {
            foundPositions.Add(transform.position + transform.forward);
        }

        if (allowPickupB)
        {
            foundPositions.Add(transform.position + -transform.forward);
        }

        if (allowPickupL)
        {
            foundPositions.Add(transform.position + -transform.right);
        }

        if (allowPickupR)
        {
            foundPositions.Add(transform.position + transform.right);
        }

        _pickupPositions = null;
        _pickupPositions = foundPositions.ToArray();
    }

    /// <summary>
    /// Returns pickup positions
    /// </summary>
    /// <returns></returns>
    public Vector3[] GetPickupPositions()
    {
        return _pickupPositions;
    }

    public bool IsEmpty()
    {
        return stockAmount == 0;
    }

    public bool IsFull()
    {
        return stockAmount >= shelfSize;
    }

    private bool IsAllAdjacentShelvesFull()
    {
        bool isAllFull = true;

        for (int i = 0; i < _adjacentShelves.Count; i++)
        {
            if (!_adjacentShelves[i].IsFull())
                isAllFull = false;
        }

        return isAllFull;
    }

    private ShelfContainer[] GetAdjacentShelvesWithSpace()
    {
        List<ShelfContainer> shelvesWithSpace = new List<ShelfContainer>(GetAllAdjacentShelves());

        for (int i = 0; i < _adjacentShelves.Count; i++)
        {
            if (!_adjacentShelves[i].IsFull())
                shelvesWithSpace.Add(_adjacentShelves[i]);
        }

        return shelvesWithSpace.ToArray();
    }

    private void UpdateStockBillboard()
    {
        if (stockAmount <= 2)
        {
            stockNumIndicator.CrossFadeColor(Color.red, 0.5f, false, false);
        }
        else
        {
            stockNumIndicator.CrossFadeColor(Color.white, 0.5f, false, false);
        }

        stockNumIndicator.SetText(stockAmount.ToString());
    }

    private ShelfContainer[] GetAdjacentShelves()
    {
        //adjacentShelves.Clear();

        Collider[] adjacentObjects = { };
        Physics.OverlapBoxNonAlloc(transform.position +
                                   new Vector3(0.0f,
                                       1.0f,
                                       0.0f),
            new Vector3(1.1f,
                1.0f,
                1.1f),
            adjacentObjects);

        return (from adjacentObject in adjacentObjects
            where adjacentObject.CompareTag("Shelf")
            let shelfContainer = adjacentObject.GetComponent<ShelfContainer>()
            where shelfContainer != this && shelfContainer.shelfStockType == shelfStockType
            select adjacentObject.GetComponent<ShelfContainer>()).ToArray();
    }

    #endregion

    private bool IsPlayerInDetectionBox()
    {
        Collider[] characters = { };
        int charactersLength = Physics.OverlapBoxNonAlloc(_shelfBounds.center,
            _shelfBounds.size +
            new Vector3(playerDetectionDistance,
                0.0f,
                playerDetectionDistance) *
            0.5f,
            characters,
            transform.rotation,
            LayerMask.GetMask("Character"));

        for (int i = 0; i < charactersLength; i++)
        {
            if (characters[i].transform.root.CompareTag("Player"))
            {
                //Debug.Log("Player is near!");
                return true;
            }
        }

        return false;
    }

    private ShelfContainer[] GetAllAdjacentShelves()
    {
        List<ShelfContainer> allAdjShelves = new List<ShelfContainer>();

        CollectShelves(this, ref allAdjShelves);

        void CollectShelves(ShelfContainer shelfContainer, ref List<ShelfContainer> collectedShelves)
        {
            shelfContainer._adjacentShelves.Clear();
            shelfContainer._adjacentShelves.AddRange(shelfContainer.GetAdjacentShelves());

            if (shelfContainer._adjacentShelves.Count <= 0) return;
            
            for (int i = 0; i < shelfContainer._adjacentShelves.Count; i++)
            {
                if (collectedShelves.Contains(shelfContainer._adjacentShelves[i])) continue;

                collectedShelves.Add(shelfContainer._adjacentShelves[i]);

                CollectShelves(shelfContainer._adjacentShelves[i], ref collectedShelves);
            }
        }

        return allAdjShelves.ToArray();
    }

    private void UpdateVisuals()
    {
        if (shelfVisual)
            shelfVisual.StockAmountPercentage = (float)stockAmount / shelfSize;
        else
        {
            ShelfVisual _shelfVisual = GetComponentInChildren<ShelfVisual>();
            
            if (!_shelfVisual) return;
            
            shelfVisual = _shelfVisual;
            
            UpdateVisuals();
        }
    }

    private void PushStockToaster(int amount)
    {
        GameObject newToaster = Instantiate(stockToaster) as GameObject;

        newToaster.transform.position = transform.position;

        Color fontColour = amount > 0 ? Color.green : Color.red;

        UIToaster uiToaster = newToaster.GetComponent<UIToaster>();
        uiToaster.SetupToaster(
            amount.ToString(),
            fontColour,
            1.0f,
            0.8f,
            EasingFunction.Ease.OutExpo,
            4.0f);
    }

    public Bounds GetShelfBounds()
    {
        return EssentialFunctions.GetMaxBounds(gameObject);
    }
}

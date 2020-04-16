using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class StockCrate : StockItem
{
    [SerializeField] private int stockQuantity = 0;
    [SerializeField] private int maxQuantity = 10;
    [SerializeField] private float billboardDistance = 1.0f;

    [SerializeField] private float playerDetectionRadius = 1.0f;
    [SerializeField] private float crossfadeSpeed = 1.0f;

    [Header("Billboard References")]
    [SerializeField] private Transform billboard = null;
    [SerializeField] private TextMeshProUGUI billboardStockNum = null;
    [SerializeField] private Image billboardStockImage = null;
    [SerializeField] private Animator billboardAnimator = null;
    [Range(0.0f, 1.0f)]
    [SerializeField] private float billboardCrossfadePercentage = 0.0f;

    [Header("Stock Toaster")]
    [SerializeField]
    private GameObject stockToaster;

    private MapManager _mapManager = null;

    private StockTypes _previousStockType = StockTypes.None;

    private int _previousStockNum = 0;
    private static readonly int showImage = Animator.StringToHash("ShowImage");

    private void OnValidate()
    {
        UpdateThumbnailNumCrossfade(billboardCrossfadePercentage);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, playerDetectionRadius);
    }

    private void Start()
    {
        _mapManager = MapManager.GetInstance();

        UpdateThumbnail();

        _previousStockNum = stockQuantity;

        UpdateStockNum();
    }

    private void Update()
    {
        if (GetStockType() != _previousStockType)
        {
            // Stock changed!
            UpdateThumbnail();
        }

        if (stockQuantity != _previousStockNum)
        {
            UpdateStockNum();
        }

        billboardAnimator.SetBool(showImage, IsPlayerInDetectionRadius());

        UpdateThumbnailNumCrossfade(billboardCrossfadePercentage);

        if (stockQuantity == 0)
            Destroy(gameObject);

        _previousStockType = GetStockType();
        _previousStockNum = stockQuantity;
    }

    private void LateUpdate()
    {
        billboard.transform.position = transform.position + new Vector3(0.0f, billboardDistance, 0.0f);
    }

    public void SetStockType(StockTypes newStockType)
    {
        stockType = newStockType;
    }

    public int GetQuantity()
    {
        return stockQuantity;
    }

    public int SetQuantity(int amount)
    {
        PushStockToaster(amount - stockQuantity);

        stockQuantity = amount;

        return amount;
    }

    public int AddQuantity(int amount)
    {
        int remainingStock = amount - (maxQuantity - stockQuantity);

        stockQuantity += amount;
        if (stockQuantity > maxQuantity) stockQuantity = maxQuantity;

        return (remainingStock > 0) ? remainingStock : 0;
    }

    #region Visual Updates

    private void UpdateThumbnail()
    {
        Sprite thumbnail = _mapManager.GetStockTypeThumbnail(GetStockType());

        billboardStockImage.sprite = thumbnail;
    }

    private void UpdateStockNum()
    {
        billboardStockNum.text = stockQuantity.ToString();
    }

    private void UpdateThumbnailNumCrossfade(float fadePercentage)
    {
        // Modifying thumbnail
        Color thumbnailColour = billboardStockImage.color;
        thumbnailColour.a = billboardStockImage.sprite ? 1.0f - fadePercentage : 0.0f;
        billboardStockImage.color = thumbnailColour;


        // Modifying number
        Color numColor = billboardStockNum.color;
        numColor.a = fadePercentage;
        billboardStockNum.color = numColor;

    }

    #endregion

    private bool IsPlayerInDetectionRadius()
    {
        Collider[] characters = { };
        int charactersLength = Physics.OverlapSphereNonAlloc(transform.position,
            playerDetectionRadius,
            characters,
            LayerMask.GetMask("Character"));

        for (int i = 0; i < charactersLength; i++)
        {
            if (!characters[i].transform.root.CompareTag("Player")) continue;
            
            Debug.Log("Player is near!");
            return true;
        }

        return false;
    }

    private void PushStockToaster(int amount)
    {
        GameObject newToaster = Instantiate(stockToaster) as GameObject;

        newToaster.transform.position = transform.position;

        Color fontColour = amount > 0 ? Color.green : Color.red;

        UIToaster uiToaster = newToaster.GetComponent<UIToaster>();
        uiToaster.SetupToaster(amount.ToString(),
            fontColour,
            1.0f,
            0.8f,
            EasingFunction.Ease.OutExpo,
            4.0f);
    }

    public override StockItem TakeProduct()
    {
        // Subtract one from crate
        SetQuantity(GetQuantity() - 1);

        GameObject newItem = Instantiate(_mapManager.GetStockTypePrefab(stockType));
        
        return newItem.GetComponent<StockItem>();
    }
}

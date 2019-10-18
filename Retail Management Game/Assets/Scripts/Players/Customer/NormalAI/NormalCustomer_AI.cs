using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class NormalCustomer_AI : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] float movementSpeed = 10.0f;
    [Range(0.0f, 1.0f)]
    public float rotationSpeed = 1.0f;
    [Range(45.0f, 180.0f)]
    [SerializeField] float pickupAngle = 90.0f;
    [SerializeField] float maxPickupDistance = 2.0f;
    [Tooltip("Push multiplyer when the character hits other rigidbodies.")]
    [SerializeField] float pushPower = 2.0f;

    [Header("AI Settings")]
    [SerializeField] float pickupDuration = 2.0f;
    [SerializeField] float purchaseDuration = 2.0f;

    //************************************************************************
    // References

    [SerializeField] Transform equippedPosition;
    [SerializeField] BoxCollider pickupArea;

    //************************************************************************
    // States

    [HideInInspector] public MoveToPositionState moveToPositionState;
    [HideInInspector] public RotateToVector rotateToVectorState;
    [HideInInspector] public DecideProductState decideProductState;
    [HideInInspector] public GrabProductState pickupProductState;

    //************************************************************************
    // Runtime Variables

    [HideInInspector]
    public NavMeshAgent agent = null;
    public NormalCustomer_SM currentState;
    NormalCustomer_SM previousState;

    [HideInInspector] public StockTypes currentWantedProduct = StockTypes.None;
    public Tasks_AI currentTask = Tasks_AI.GetProduct;
    public Vector3 taskDestination;
    public GameObject equippedItem;

    //*************************************************************************
    // Managers

    [HideInInspector] public MapManager mapManager = null;

    private void OnValidate()
    {
        float boxSizeDepth = maxPickupDistance;
        float boxSizeHeight = 2.0f;
        float boxSizeWidth = maxPickupDistance * Mathf.Sin(pickupAngle * Mathf.Deg2Rad);

        pickupArea.size = new Vector3(boxSizeWidth * 2.0f, boxSizeHeight, boxSizeDepth);
        pickupArea.transform.localPosition = new Vector3(0.0f, 0.0f, boxSizeDepth * 0.5f);
    }

    void Awake()
    {
        moveToPositionState = new MoveToPositionState(this);
        rotateToVectorState = new RotateToVector(this);
        decideProductState = new DecideProductState(this);
        pickupProductState = new GrabProductState(this);
    }

    // Start is called before the first frame update
    void Start()
    {
        // Get MapManager
        mapManager = MapManager.GetInstance();

        // Begin with default state
        currentState = decideProductState;

        // Get NavMeshAgent
        agent = GetComponent<NavMeshAgent>();

        // Call initial start method on current state
        currentState.StartState();
    }

    // Update is called once per frame
    void Update()
    {
        // Check if Map Manager is loaded
        if (!mapManager.isDoneLoading) return;

        // Update current state
        currentState.UpdateState();

        // Detect state change and call StartState method in current state
        if (previousState != null && previousState != currentState)
        {
            Debug.Log("State changed! " + previousState + " -> " + currentState);

            // Activate start state
            currentState.StartState();
        }

        // Update previous state
        previousState = currentState;
    }

    private void FixedUpdate()
    {
        // Check if Map Manager is loaded
        if (!mapManager.isDoneLoading) return;

        // Fixed update current state
        currentState.FixedUpdateState();
    }

    //*************************************************************************
    // Interaction methods

    public void Interact()
    {
        // Raycast to see what's infront of AI

        RaycastHit[] hits = Physics.RaycastAll(transform.position, transform.forward, maxPickupDistance, LayerMask.GetMask("Interactive"));

        if (hits.Length > 0)
        {
            Transform rootTransform = null;

            foreach (RaycastHit hit in hits)
            {
                rootTransform = hit.transform.root;
                if (EssentialFunctions.CompareTags(rootTransform, new string[] { "Shelf", "Register" }))
                {
                    break;
                }
            }

            if (rootTransform)
            {
                ShelfContainer shelfContainer = rootTransform.GetComponent<ShelfContainer>();

                if (!shelfContainer)
                {
                    Debug.LogError("Shelf \"" + rootTransform + "\" is tagged as a shelf, but doesn't have a ShelfContainer component.");
                    return;
                }

                if (equippedItem)
                {
                    AddStockToShelf(shelfContainer);
                }
                else
                {
                    GetStockFromShelf(shelfContainer);
                }
            }
        }
    }

    public void EquipItem(Transform item)
    {
        // Get game object
        equippedItem = item.gameObject;

        // Get rigidbody from item and disable physics
        Rigidbody productRB = equippedItem.GetComponent<Rigidbody>();
        productRB.isKinematic = true;

        // Attach item to player hold position
        equippedItem.transform.SetParent(equippedPosition);
        equippedItem.transform.localPosition = Vector3.zero;
    }

    public void UnequipItem()
    {
        if (!equippedItem) return;

        // Get rigidbody on item and enable physics
        Rigidbody productRB = equippedItem.GetComponent<Rigidbody>();
        productRB.isKinematic = false;

        // De-attach item from player and remove item from equip slot
        equippedItem.transform.SetParent(null);
        equippedItem = null;
    }

    public void GetStockFromShelf(ShelfContainer shelf)
    {
        StockTypes stockType = shelf.ShelfStockType;
        int amount = shelf.GetStock();

        if (amount != 0)
        {
            GameObject newItem = Instantiate(mapManager.GetStockTypePrefab(stockType)) as GameObject;
            EquipItem(newItem.transform);
        }
    }

    public void AddStockToShelf(ShelfContainer shelf)
    {
        // Check if there is something equipped
        if (!equippedItem) return;

        // Get stock type that is equipped
        StockTypes equippedType = equippedItem.GetComponent<StockItem>().GetStockType();

        int result = shelf.AddStock(equippedType);

        if (result == 0)
        {
            Destroy(equippedItem);
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrabProductState : NormalCustomer_SM
{
    private readonly NormalCustomer_AI stateMachine;

    float waitTime = 0.0f;

    public GrabProductState(NormalCustomer_AI _SM)
    {
        stateMachine = _SM;
    }

    public void StartState()
    {
        waitTime = stateMachine.pickupDuration;
    }

    #region Transitions

    public void ToDecideProductState()
    {
        
    }

    public void ToDecideRegisterState()
    {
        stateMachine.currentState = stateMachine.decideRegisterState;
    }

    public void ToFacePosition()
    {
        
    }

    public void ToPickupState()
    {
        // Cannot transition to self!
    }

    public void ToPurchaseState()
    {
        
    }

    public void ToWalkToPositionState()
    {
        stateMachine.currentState = stateMachine.moveToPositionState;
    }

    #endregion

    public void UpdateState()
    {
        if (!stateMachine.equippedItem)
        {
            Interact();
        }
        else if (waitTime <= 0.0f)
        {
            stateMachine.currentTask = Tasks_AI.GoToRegister;
            ToDecideRegisterState();
        }
        else
        {
            waitTime -= Time.deltaTime;
        }
    }

    public void FixedUpdateState()
    {
        
    }

    //*************************************************************************
    // Interaction methods

    public void Interact()
    {
        //// Raycast to see what's infront of AI

        //RaycastHit[] hits = Physics.RaycastAll(transform.position, transform.forward, maxPickupDistance, LayerMask.GetMask("Interactive"));

        //if (hits.Length > 0)
        //{
        //    Transform rootTransform = null;

        //    foreach (RaycastHit hit in hits)
        //    {
        //        rootTransform = hit.transform.root;
        //        if (EssentialFunctions.CompareTags(rootTransform, new string[] { "Shelf", "Register" }))
        //        {
        //            break;
        //        }
        //    }

        //    if (rootTransform)
        //    {
        //        ShelfContainer shelfContainer = rootTransform.GetComponent<ShelfContainer>();

        //        if (!shelfContainer)
        //        {
        //            Debug.LogError("Shelf \"" + rootTransform + "\" is tagged as a shelf, but doesn't have a ShelfContainer component.");
        //            return;
        //        }

        //        if (equippedItem)
        //        {
        //            AddStockToShelf(shelfContainer);
        //        }
        //        else
        //        {
        //            GetStockFromShelf(shelfContainer);
        //        }
        //    }
        //}

        ShelfContainer interactShelf = stateMachine.TaskDestinationAsShelf();

        if (interactShelf)
        {
            if (stateMachine.equippedItem)
            {
                AddStockToShelf(interactShelf);
            }
            else
            {
                GetStockFromShelf(interactShelf);
            }
        }
    }

    public void EquipItem(Transform item)
    {
        // Get game object
        stateMachine.equippedItem = item.gameObject;

        // Get rigidbody from item and disable physics
        Rigidbody productRB = stateMachine.equippedItem.GetComponent<Rigidbody>();
        productRB.isKinematic = true;

        // Attach item to player hold position
        stateMachine.equippedItem.transform.SetParent(stateMachine.equippedPosition);
        stateMachine.equippedItem.transform.localPosition = Vector3.zero;
    }

    public void UnequipItem()
    {
        if (!stateMachine.equippedItem) return;

        // Get rigidbody on item and enable physics
        Rigidbody productRB = stateMachine.equippedItem.GetComponent<Rigidbody>();
        productRB.isKinematic = false;

        // De-attach item from player and remove item from equip slot
        stateMachine.equippedItem.transform.SetParent(null);
        stateMachine.equippedItem = null;
    }

    public void GetStockFromShelf(ShelfContainer shelf)
    {
        StockTypes stockType = shelf.ShelfStockType;
        int amount = shelf.GetStock();

        if (amount != 0)
        {
            GameObject newItem = Object.Instantiate(stateMachine.mapManager.GetStockTypePrefab(stockType)) as GameObject;
            EquipItem(newItem.transform);
        }
    }

    public void AddStockToShelf(ShelfContainer shelf)
    {
        // Check if there is something equipped
        if (!stateMachine.equippedItem) return;

        // Get stock type that is equipped
        StockTypes equippedType = stateMachine.equippedItem.GetComponent<StockItem>().GetStockType();

        int result = shelf.AddStock(equippedType);

        if (result == 0)
        {
            Object.Destroy(stateMachine.equippedItem);
        }
    }

    public void ToQueuingState()
    {
        throw new System.NotImplementedException();
    }
}

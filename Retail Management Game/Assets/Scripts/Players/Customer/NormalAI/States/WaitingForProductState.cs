using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaitingForProductState : NormalCustomer_SM
{
    private readonly NormalCustomer_AI stateMachine;

    ShelfContainer shelf;

    const float collisionSensitivity = 2.0f;

    public WaitingForProductState(NormalCustomer_AI _SM)
    {
        stateMachine = _SM;
    }

    public void StartState()
    {
        // Attach collisions
        stateMachine.colliderEvents.OnCollisionEnter_UE.AddListener(OnCollisionEnter_UE);
    }

    #region Transition

    public void ToGetProductState()
    {
        
    }

    public void ToLeaveStoreState()
    {
        
    }

    public void ToPurchaseState()
    {
        stateMachine.currentState = stateMachine.purchaseProductState;
    }

    public void ToWaitForProductState()
    {
        
    }

    public void ToWaitForRegisterState()
    {
        
    }

    #endregion

    public void UpdateState()
    {
        if (stateMachine.equippedItem != null)
        {
            ToPurchaseState();
            return;
        }

        if (!shelf)
        {
            shelf = stateMachine.TaskDestinationAsShelf();
        }
        else
        {
            if (shelf.StockAmount() > 0)
            {
                stateMachine.GetStockFromShelf(shelf);
            }
        }
    }

    public void FixedUpdateState()
    {
        
    }

    void OnCollisionEnter_UE(Collision collision)
    {
        ContactPoint contact = collision.GetContact(0);

        GameObject other = contact.otherCollider.transform.root.gameObject;

        Debug.Log(collision.relativeVelocity.magnitude);

        // Add products to the shelf if it hits it hard enough
        if (collision.relativeVelocity.magnitude < collisionSensitivity) return;

        if (other.CompareTag("Product"))
        {
            StockItem stock = other.GetComponent<StockItem>();

            if (stock)
            {
                if (stock.GetStockType() == stateMachine.currentWantedProduct)
                {
                    stateMachine.EquipItem(other.transform);
                }
            }
        }
    }
}

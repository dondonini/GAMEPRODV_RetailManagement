using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaitingForProductState : NormalCustomer_SM
{
    private readonly NormalCustomer_AI stateMachine;

    ShelfContainer shelf;

    Vector3 rotationDirection = new Vector3(0.0f, 1.0f, 0.5f);
    float rotationSpeed = 1.0f;

    GameObject spinItem = null;

    const float collisionSensitivity = 2.0f;

    public WaitingForProductState(NormalCustomer_AI _SM)
    {
        stateMachine = _SM;
    }

    public void StartState()
    {
        // Attach collisions
        stateMachine.colliderEvents.OnCollisionEnter_UE.AddListener(OnCollisionEnter_UE);

        GameObject model = Object.Instantiate(stateMachine.mapManager.GetStockTypePrefab(stateMachine.currentWantedProduct)) as GameObject;

        // Remove physics
        Rigidbody rb = model.GetComponent<Rigidbody>();
        rb.isKinematic = true;

        // Move model to info halo position
        model.transform.SetParent(stateMachine.infoHalo);
        model.transform.position = stateMachine.infoHalo.position;

        // Disable all scripts on model
        MonoBehaviour[] scripts = model.GetComponents<MonoBehaviour>();
        foreach (MonoBehaviour script in scripts)
        {
            script.enabled = false;
        }

        // Add rotation script
        SimpleRotation rotationScript = model.AddComponent<SimpleRotation>();
        rotationScript.RotationDirection = rotationDirection;
        rotationScript.RotationSpeed = rotationSpeed;

        // Keep reference of model
        spinItem = model;
    }

    public void ExitState()
    {
        Object.Destroy(spinItem);
        spinItem = null;
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

        //Debug.Log(collision.relativeVelocity.magnitude);

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

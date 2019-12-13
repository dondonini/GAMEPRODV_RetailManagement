using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaitingForProductState : NormalCustomer_SM
{
    private readonly NormalCustomer_AI stateMachine;

    ShelfContainer shelf;

    Vector3 rotationDirection = new Vector3(0.0f, 1.0f, 0.5f);
    float rotationSpeed = 50.0f;

    GameObject spinItem = null;

    float patienceTimer = 0.0f;

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

        // Reset patience timer
        patienceTimer = stateMachine.stockPatience;
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
        stateMachine.currentState = stateMachine.leavingState;
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
        if (patienceTimer <= 0.0f)
        {
            stateMachine.gameManager.ForceLoseReasonMessage("RESTOCK THE SHELVES, DINGUS");
            stateMachine.gameManager.LostCustomer();
            ToLeaveStoreState();
        }

        // Go straight to purchasing when the customer throws the product at them
        if (stateMachine.equippedItem != null)
        {
            ToPurchaseState();
            return;
        }

        // Wait until there's stock on the shelf
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


        // Patience Indicator
        stateMachine.patienceBillboard.gameObject.SetActive(true);
        stateMachine.patienceAnimator.SetBool("Show", true);
        stateMachine.patienceText.color = Color.Lerp(Color.white, Color.red, 1.0f - (patienceTimer / stateMachine.stockPatience));

        patienceTimer -= Time.deltaTime;
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
        if (collision.relativeVelocity.magnitude < stateMachine.collisionSensitivity) return;

        switch (other.tag)
        {
            case "Product":
                {
                    // Double check if StockItem exists
                    StockItem stock = other.GetComponent<StockItem>();
                    if (stock)
                    {
                        // Check if stock matches the current wanted product
                        if (stock.GetStockType() == stateMachine.currentWantedProduct)
                        {
                            // Equip cought product
                            stateMachine.EquipItem(other.transform);
                        }
                    }
                    break;
                }
            case "StockCrate":
                {
                    // Double check if StockCrate exists
                    StockCrate crate = other.GetComponent<StockCrate>();
                    if (crate)
                    {
                        /* Check if stock in crate matches the current wanted 
                         * product and if there is enough in the crate to take one
                         */
                        if (crate.GetStockType() == stateMachine.currentWantedProduct && crate.GetQuantity() >= 1)
                        {
                            // Deduct one stock from the crate
                            crate.SetQuantity(crate.GetQuantity() - 1);

                            // Make new stock and add to customer
                            GameObject newStock = Object.Instantiate(stateMachine.mapManager.GetStockTypePrefab(stateMachine.currentWantedProduct)) as GameObject;

                            // Equip the new stock
                            stateMachine.EquipItem(newStock.transform);
                        }
                    }
                    break;
                }
        }
    }
}

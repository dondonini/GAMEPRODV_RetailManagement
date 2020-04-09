using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaitingForProductState : IBase_SM
{
    protected Customer_AI stateMachine = null;

    protected ShelfContainer shelf;

    protected GameObject spinItem = null;

    protected float patienceTimer = 0.0f;

    Vector3 rotationDirection = new Vector3(0.0f, 1.0f, 0.5f);
    readonly float rotationSpeed = 50.0f;

    public virtual void StartState()
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

    public virtual void UpdateActions()
    {
        
    }

    public virtual void UpdateState()
    {

    }

    public virtual void FixedUpdateState()
    {

    }

    public virtual void ExitState()
    {
        Object.Destroy(spinItem);
        spinItem = null;
    }

    public virtual void InterruptState()
    {

    }

    protected virtual void OnCollisionEnter_UE(Collision collision)
    {
        ContactPoint contact = collision.GetContact(0);

        GameObject other = contact.otherCollider.transform.root.gameObject;

        //Debug.Log(collision.relativeVelocity.magnitude);

        // Add products to the shelf if it hits it hard enough
        if (collision.relativeVelocity.magnitude < stateMachine.collisionSensitivity) return;

        StockItem stock = other.GetComponent<StockItem>();

        switch (other.tag)
        {
            case "Product":
                {
                    // Double check if StockItem exists
                    stock = other.GetComponent<StockItem>();
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
                    StockCrate crate = stock as StockCrate;
                    if (crate)
                    {
                        // Claim crate
                        crate.ClaimItem(stateMachine.gameObject);

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

                        // Unclaim crate
                        crate.UnclaimItem(stateMachine.gameObject);
                    }
                    break;
                }
        }
    }
}

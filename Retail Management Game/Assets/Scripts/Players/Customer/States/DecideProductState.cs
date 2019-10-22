using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// In this state, the AI tries to decide on what product it wants to get.
/// </summary>
public class DecideProductState : NormalCustomer_SM
{
    private readonly NormalCustomer_AI stateMachine;

    bool decided = false;

    public DecideProductState(NormalCustomer_AI _SM)
    {
        stateMachine = _SM;
    }

    public void StartState()
    {
        stateMachine.currentTask = Tasks_AI.GetProduct;
        decided = false;
    }

    #region Transition States

    public void ToDecideState()
    {
        // Cannot transition to self!
    }

    public void ToFacePosition()
    {
        
    }

    public void ToPickupState()
    {
        
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
        if (decided)
        {
            // Change state to walk state
            ToWalkToPositionState();
        }

        // Get random shelf wanted
        ShelfContainer selectedShelf = GetShelfDestination();

        stateMachine.taskDestination = selectedShelf.transform;

        Vector3[] pickupPositions = selectedShelf.GetPickupPositions();

        // Set AI destination to selected shelf
        stateMachine.taskDestinationPosition = pickupPositions[Random.Range(0, pickupPositions.Length - 1)];

        decided = true;
    }

    public void FixedUpdateState()
    {

    }

    StockTypes GetRandomStockType(StockTypes exclude = StockTypes.None)
    {
        List<StockTypes> productSelection = stateMachine.mapManager.GetStockTypesAvailable().ToList();
        
        // Remove StockType.None and whatever is exluded
        for (int i = 0; i < productSelection.Count; i++)
        {
            if (productSelection[i] == StockTypes.None || productSelection[i] == exclude)
            {
                productSelection.RemoveAt(i);
            }
        }

        // Return random product in selection
        return productSelection[Random.Range(0, productSelection.Count)];
    }

    ShelfContainer GetShelfDestination()
    {

        // Get a random stock type
        StockTypes selectedType = GetRandomStockType(stateMachine.currentWantedProduct);
        stateMachine.currentWantedProduct = selectedType;

        // Get random shelf with wanted stocktype
        ShelfContainer selectedShelf = stateMachine.mapManager.GetRandomShelvingUnit(stateMachine.currentWantedProduct);

        return selectedShelf;
    }
}

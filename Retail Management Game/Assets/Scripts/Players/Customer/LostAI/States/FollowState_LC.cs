using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowState_LC : FollowState, ILostCustomer_SM
{
    private readonly LostCustomer_AI _stateMachine;
    
    private static readonly int show = Animator.StringToHash("Show");

    public FollowState_LC(LostCustomer_AI _SM)
    {
        stateMachine = _SM;
        _stateMachine = _SM;
    }

    public override void StartState()
    {
        base.StartState();

        if (_stateMachine.playerToFollow == null)
        {
            // Warn me if I fucked up
            Debug.LogWarning("Follow player is not assigned! Did you assign it in WanderState_LC.cs?");
            
            // Disable AI before it starts spamming my console!
            stateMachine.DisableStateMachine();
        }
    }

    #region Transitions

    public void ToWanderState()
    {
        throw new System.NotImplementedException();
    }

    public void ToFollowState()
    {
        throw new System.NotImplementedException();
    }

    public void ToGetProductState()
    {
        _stateMachine.currentState = _stateMachine.getProductState;
    }

    public void ToPurchaseState()
    {
        _stateMachine.currentState = _stateMachine.purchaseState;
    }

    public void ToLeaveStoreState()
    {
        throw new System.NotImplementedException();
    }

    public void ToWaitForProductState()
    {
        throw new System.NotImplementedException();
    }

    public void ToWaitForRegisterState()
    {
        throw new System.NotImplementedException();
    }

    #endregion

    public override void UpdateState()
    {
        base.UpdateState();

        if (CheckForWantedProduct())
            Debug.Log("Product found!");
    }

    public override void ExitState()
    {
        base.ExitState();
        
        // Remove billboard
        _stateMachine.billboardAnimator.SetBool(show, false);
    }

    public override void UpdateActions()
    {
        base.UpdateActions();

        switch (currentAction)
        {
            case FollowActions.None:

                // Setup task destination
                SetupFollow();
                
                // Initial update follow path
                UpdateFollowPath();
                
                break;
            case FollowActions.Following:
                
                // Update follow path frequently
                if (followUpdateTimer >= FOLLOW_UPDATE_RATE)
                {
                    UpdateFollowPath();
                    followUpdateTimer = 0.0f;
                }

                // Update timer
                followUpdateTimer += Time.deltaTime;
                
                break;
            case FollowActions.FoundProduct:
                
                // Get product
                ToPurchaseState();
                break;
            case FollowActions.FoundShelf:
                
                // Get product from shelf
                ToGetProductState();
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private void UpdateFollowPath()
    {
        // Update taskDestinationPosition to follow player
        stateMachine.taskDestinationPosition = stateMachine.taskDestination.position;
        
        // Update NavMeshAgent to go to destination
        stateMachine.navMeshAgent.SetDestination(stateMachine.taskDestinationPosition);
    }

    private bool SetupFollow()
    {
        // Double check if follow player exists
        if (!_stateMachine.playerToFollow) return false;

        // Set taskDestination as the player it's following
        stateMachine.taskDestination = _stateMachine.playerToFollow.transform;

        return true;
    }
    
    private bool CheckForWantedProduct()
    {
        int i;
        
        // Check close by items
        
        GameObject[] allItems = _stateMachine.mapManager.GetAllItemInstances();

        for (i = 0; i < allItems.Length; i++)
        {
            // Check if item is near enough
            float distanceFromAI = Vector3.Distance(_stateMachine.transform.position, allItems[i].transform.position);
            float distanceFromPlayer = Vector3.Distance(_stateMachine.playerToFollow.transform.position,
                allItems[i].transform.position);
            if (distanceFromAI > _stateMachine.checkRadius || distanceFromPlayer > _stateMachine.checkRadius) continue;
            
            Debug.DrawLine(_stateMachine.transform.position, allItems[i].transform.position, Color.white);
            
            // Get and unpack stock item
            StockItem stockItem = allItems[i].transform.GetComponent<StockItem>();
            stockItem = stockItem.TakeProduct();

            stateMachine.EquipItem(stockItem.transform);

            currentAction = FollowActions.FoundProduct;
            
            return true;
        }

        // Check close by shelves
        
        ShelfContainer[] allShelves = _stateMachine.mapManager.GetShelvingUnits();

        for (i = 0; i < allShelves.Length; i++)
        {
            // Check if shelf is near enough
            float distanceFromAI = Vector3.Distance(_stateMachine.transform.position, allShelves[i].transform.position);
            float distanceFromPlayer = Vector3.Distance(_stateMachine.playerToFollow.transform.position,
                allShelves[i].transform.position);
            if (distanceFromAI > _stateMachine.checkRadius || distanceFromPlayer > _stateMachine.checkRadius) continue;
            
            Debug.DrawLine(_stateMachine.transform.position, allShelves[i].transform.position, Color.white);

            // Check if shelf near by has product wanted
            if (allShelves[i].ShelfStockType != stateMachine.currentWantedProduct) continue;
                
            // Setup AI to collect product from shelf
            stateMachine.taskDestination = allShelves[i].transform;

            currentAction = FollowActions.FoundShelf;
                
            return true;
        }

        return false;
    }
}

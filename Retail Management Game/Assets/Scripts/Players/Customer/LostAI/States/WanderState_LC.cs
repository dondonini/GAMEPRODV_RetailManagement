using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WanderState_LC : WanderState, ILostCustomer_SM
{
    private readonly LostCustomer_AI _stateMachine;
    private static readonly int show = Animator.StringToHash("Show");
    private bool _decideOnWantedProduct = false;

    public WanderState_LC(LostCustomer_AI _SM, bool decideOnWantedProduct = false)
    {
        _stateMachine = _SM;
        stateMachine = _SM;

        _decideOnWantedProduct = decideOnWantedProduct;
    }

    public override void StartState()
    {
        base.StartState();
        
        // Hide patience billboard
        _stateMachine.patienceText.gameObject.SetActive(false);
        
        // Show wanted thumbnail
        _stateMachine.wantedThumbnail.gameObject.SetActive(true);
    }

    #region Transitions

    public void ToWanderState()
    {
        throw new NotImplementedException();
    }

    public void ToFollowState()
    {
        _stateMachine.currentState = _stateMachine.followState;
    }

    public void ToGetProductState()
    {
        throw new System.NotImplementedException();
    }

    public void ToPurchaseState()
    {
        throw new System.NotImplementedException();
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

        if (isFirstWaypoint) return;
        
        if (!_stateMachine.playerToFollow)
            _stateMachine.playerToFollow = GetPlayerNearBy();
        else
            currentAction = WanderActions.Helped;
    }

    public override void UpdateActions()
    {
        base.UpdateActions();

        switch (currentAction)
        {
            case WanderActions.None:
                break;
            case WanderActions.DecideOnRandomPosition:
                waitTime = _stateMachine.wanderFrequency;
                break;
            case WanderActions.Moving:
                break;
            case WanderActions.Waiting:
                
                // Setup and show billboard
                SetupThumbnail();
                if (!_stateMachine.billboardAnimator.GetBool(show))
                    _stateMachine.billboardAnimator.SetBool(show, true);
                
                break;
            case WanderActions.Helped:
                ToFollowState();
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private void SetupThumbnail()
    {
        // Get wanted image from map manager
        Sprite wantedImage = _stateMachine.mapManager.GetStockTypeThumbnail(_stateMachine.currentWantedProduct);

        if (!wantedImage)
        {
            Debug.LogWarning("Thumbnail of " + _stateMachine.currentWantedProduct +
                             " is missing! Is it not set up correctly?");
            return;
        }
        
        // Setup image over AI
        _stateMachine.wantedThumbnail.sprite = wantedImage;
    }

    private PlayerController GetPlayerNearBy()
    {
        PlayerController currentClosest = null;
        float distance = float.MaxValue;

        for (int i = 0; i < allPlayers.Length; i++)
        {
            
            // Check if player is close to AI
            Vector3 stateMachinePosition = stateMachine.transform.position;
            float distanceToPlayer = Vector3.Distance(stateMachinePosition, allPlayers[i].position);

            Debug.DrawLine(stateMachinePosition, allPlayers[i].position, Color.red);
            
            if ((distanceToPlayer >= _stateMachine.checkRadius) || (distanceToPlayer >= distance)) continue;
            
            Debug.DrawLine(stateMachine.transform.position, allPlayers[i].position, Color.green);
            
            // Apply that character is closest
            currentClosest = allPlayers[i].GetComponent<PlayerController>();
            distance = distanceToPlayer;
        }

        return currentClosest;
    }
}

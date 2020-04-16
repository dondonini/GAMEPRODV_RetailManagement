using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowState : IBase_SM
{
    protected Customer_AI stateMachine = null;

    protected FollowActions currentAction = FollowActions.None;
    protected const float FOLLOW_UPDATE_RATE = 1.0f;
    protected float followUpdateTimer = 0.0f;
    
    protected enum FollowActions
    {
        None,
        Following,
        FoundShelf,
        FoundProduct,
    }

    public virtual void StartState()
    {
        stateMachine.UpdateInternalDebug("");
    }

    public virtual void ExitState()
    {
        throw new System.NotImplementedException();
    }

    public virtual void UpdateState()
    {
        if (!stateMachine.mapManager.isDoneLoading) return;
        
        UpdateActions();
        
        stateMachine.UpdateInternalDebug("CurrentState = " + currentAction);
    }

    public virtual void FixedUpdateState()
    {
        
    }

    public virtual void InterruptState()
    {
        throw new System.NotImplementedException();
    }

    public virtual void UpdateActions()
    {
    
    }
}

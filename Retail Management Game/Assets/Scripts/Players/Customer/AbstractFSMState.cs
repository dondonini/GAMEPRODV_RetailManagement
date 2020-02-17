using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public enum ExecutionState
{
    NONE,
    ACTIVE,
    COMPLETED,
    TERMINATED,
}

public abstract class AbstractFSMState : ScriptableObject
{
    protected NavMeshAgent _navMeshAgent;

    public ExecutionState ExecutionState { get; protected set; }

    public virtual void OnEnable()
    {
        ExecutionState = ExecutionState.NONE;
    }

    public virtual bool EnterState()
    {
        bool success = true;

        ExecutionState = ExecutionState.ACTIVE;

        // Check if NavMeshAgent is available
        if (!_navMeshAgent) success = false;

        // Success!
        return success;
    }

    public abstract void UpdateState();

    public virtual bool ExitState()
    {
        ExecutionState = ExecutionState.COMPLETED;

        // Success!
        return true;
    }

    public virtual bool SetNavMeshAgent(NavMeshAgent newNavMeshAgent)
    {
        if (!newNavMeshAgent) return false;

        _navMeshAgent = newNavMeshAgent;

        return true;
    }

    public virtual bool SetExecutingNPC(MonoBehaviour newMono)
    {
        return true;
    }
}

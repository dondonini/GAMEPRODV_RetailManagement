using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FiniteStateMachine : MonoBehaviour
{
    [SerializeField]
    AbstractFSMState _startState;
    AbstractFSMState _currentState;
    AbstractFSMState _previousState;

    private void Awake()
    {
        // Clear data
        _currentState = null;
        _previousState = null;
    }

    // Start is called before the first frame update
    void Start()
    {
        if (_startState)
        {
            EnterNextState(_startState);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (_currentState)
        {
            _currentState.UpdateState();
        }
    }

    #region State Management

    private void EnterNextState(AbstractFSMState nextState)
    {
        if (!nextState) { return; }

        _currentState = nextState;
    }

    #endregion
}

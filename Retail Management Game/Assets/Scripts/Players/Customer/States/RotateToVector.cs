using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateToVector : NormalCustomer_SM
{
    private readonly NormalCustomer_AI stateMachine;

    private float marginOfErrorAmount = 0.1f;

    private Quaternion angularVelocity;

    private bool isFinishedRotating = false;

    public RotateToVector(NormalCustomer_AI _SM)
    {
        stateMachine = _SM;
    }

    public void StartState()
    {
        isFinishedRotating = false;

        if (stateMachine.taskDestinationPosition != stateMachine.taskDestination.position)
        {
            stateMachine.taskDestinationPosition = stateMachine.taskDestination.position;
        }
    }

    #region Transitions

    public void ToDecideState()
    {
        
    }

    public void ToFacePosition()
    {
        
    }

    public void ToPickupState()
    {
        stateMachine.currentState = stateMachine.pickupProductState;
    }

    public void ToPurchaseState()
    {
        
    }

    public void ToWalkToPositionState()
    {
        
    }

    #endregion

    public void UpdateState()
    {
        if (isFinishedRotating)
        {
            // Go to a state when done walking
            switch (stateMachine.currentTask)
            {
                // AI should be at the shelf
                case Tasks_AI.GetProduct:
                    {
                        // Pickup product on shelf
                        ToPickupState();

                        break;
                    }

                // AI should be at the register
                case Tasks_AI.PuchaseProduct:
                    {
                        // Purchase items
                        ToPurchaseState();

                        break;
                    }

                // AI should be leaving the store
                case Tasks_AI.LeaveStore:
                    {
                        // TODO: Leave state

                        break;
                    }
            }
        }
    }

    public void FixedUpdateState()
    {
        if (isFinishedRotating) return;

        // Calculate direction to target
        Vector3 targetRot = stateMachine.taskDestinationPosition - stateMachine.transform.position;
        targetRot.y = 0.0f;
        targetRot.Normalize();

        // SmoothDamp towards to target rotation
        stateMachine.transform.rotation = QuaternionUtil.SmoothDamp(stateMachine.transform.rotation, Quaternion.LookRotation(targetRot), ref angularVelocity, stateMachine.rotationSpeed);

        // Calculate the angle between target position and customer forward vector
        float fromToDelta = Vector3.Angle(stateMachine.transform.forward, targetRot);

        Debug.DrawRay(stateMachine.transform.position, targetRot * 5.0f, Color.green);
        Debug.DrawRay(stateMachine.transform.position, stateMachine.transform.forward * 5.0f, Color.red);

        // Stop rotation if angle between target and forward vector is lower than margin of error
        if (fromToDelta <= marginOfErrorAmount)
        {
            isFinishedRotating = true;
        }
    }
}

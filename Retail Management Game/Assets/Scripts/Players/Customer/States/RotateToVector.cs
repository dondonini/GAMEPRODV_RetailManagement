using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateToVector : NormalCustomer_SM
{
    private readonly NormalCustomer_AI stateMachine;

    private float angularVelocity = 0.0f;

    private bool isFinishedRotating = false;

    public RotateToVector(NormalCustomer_AI _SM)
    {
        stateMachine = _SM;
    }

    public void StartState()
    {
        isFinishedRotating = false;
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
        Quaternion target_rot = Quaternion.LookRotation(stateMachine.taskDestination - stateMachine.transform.position);
        float delta = Quaternion.Angle(stateMachine.transform.rotation, target_rot);
        if (delta > 0.0f)
        {
            float t = Mathf.SmoothDampAngle(delta, 0.0f, ref angularVelocity, stateMachine.rotationSpeed);
            t = 1.0f - t / delta;
            stateMachine.transform.rotation = Quaternion.Slerp(stateMachine.transform.rotation, target_rot, t);
        }
        else
        {
            isFinishedRotating = true;
        }
    }
}

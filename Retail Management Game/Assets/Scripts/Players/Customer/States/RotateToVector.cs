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
        Vector3 targetRot = stateMachine.taskDestination - stateMachine.transform.position;

        stateMachine.transform.rotation = QuaternionUtil.SmoothDamp(stateMachine.transform.rotation, Quaternion.LookRotation(targetRot), ref angularVelocity, stateMachine.rotationSpeed);

        //float fromToDelta = Mathf.Abs(Mathf.DeltaAngle(stateMachine.transform.eulerAngles.y, targetRot.eulerAngles.y));
        float fromToDelta = Vector3.Angle(stateMachine.transform.position, targetRot);

        Debug.Log(fromToDelta);

        if (fromToDelta <= marginOfErrorAmount)
        {
            isFinishedRotating = true;
        }

        //if (fromToDelta > marginOfErrorAmount)
        //{
        //    float t = Mathf.SmoothDampAngle(fromToDelta, 0.0f, ref angularVelocity, stateMachine.rotationSpeed);
        //    t = 1.0f - t / fromToDelta;
        //    stateMachine.transform.rotation = Quaternion.Slerp(stateMachine.transform.rotation, targetRot, t);
        //    //Debug.Log("t: " + t + "\t delta: " + fromToDelta);
        //}
        //else
        //{
        //    isFinishedRotating = true;
        //}
    }
}

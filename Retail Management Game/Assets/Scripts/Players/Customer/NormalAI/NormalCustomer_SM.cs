using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface NormalCustomer_SM
{
    // Essentials

    void StartState();

    void UpdateState();

    void FixedUpdateState();

    // States
    void ToWalkToPositionState();

    void ToPickupState();

    void ToPurchaseState();

    void ToDecideProductState();

    void ToFacePosition();

    void ToDecideRegisterState();

    void ToQueuingState();
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface NormalCustomer_SM
{
    // Essentials

    void StartState();

    void UpdateState();

    // States
    void ToWalkToPositionState();

    void ToPickupState();

    void ToPurchaseState();
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface NormalCustomer_SM
{
    // Main functions

    void StartState();

    void UpdateState();

    void FixedUpdateState();

    // Transitions

    void ToGetProductState();

    void ToPurchaseState();

    void ToLeaveStoreState();

    void ToWaitForProductState();

    void ToWaitForRegisterState();
}

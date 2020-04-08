using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface NormalCustomer_SM : Base_SM
{
    // Transitions

    void ToGetProductState();

    void ToPurchaseState();

    void ToLeaveStoreState();

    void ToWaitForProductState();

    void ToWaitForRegisterState();
}

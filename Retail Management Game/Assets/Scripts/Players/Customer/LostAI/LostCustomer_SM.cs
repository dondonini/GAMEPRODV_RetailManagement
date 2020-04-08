using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface LostCustomer_SM : Base_SM
{

    // Transition Functions

    void ToGetProductState();

    void ToPurchaseState();

    void ToLeaveStoreState();

    void ToWaitForProductState();

    void ToWaitForRegisterState();
}

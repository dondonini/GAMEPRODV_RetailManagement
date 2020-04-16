public interface ILostCustomer_SM : IBase_SM
{

    // Transition Functions

    void ToWanderState();

    void ToFollowState();
    
    void ToGetProductState();

    void ToPurchaseState();

    void ToLeaveStoreState();

    void ToWaitForProductState();

    void ToWaitForRegisterState();
}

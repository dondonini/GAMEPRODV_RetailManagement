public interface INormalCustomer_SM : IBase_SM
{
    // Transitions

    void ToGetProductState();

    void ToPurchaseState();

    void ToLeaveStoreState();

    void ToWaitForProductState();

    void ToWaitForRegisterState();
}

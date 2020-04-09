public interface ILostCustomer_SM : IBase_SM
{

    // Transition Functions

    void ToGetProductState();

    void ToPurchaseState();

    void ToLeaveStoreState();

    void ToWaitForProductState();

    void ToWaitForRegisterState();
}

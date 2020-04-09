public interface IBase_SM
{
    void StartState();

    void ExitState();

    void UpdateState();

    void FixedUpdateState();

    void InterruptState();

    void UpdateActions();
}

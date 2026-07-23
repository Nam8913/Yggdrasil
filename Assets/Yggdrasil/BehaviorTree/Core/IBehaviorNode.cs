namespace BehaviorTree
{
    public interface IBehaviorNode
    {
        BHState CurrentState { get; }
        bool IsRunning { get; }
        void Reset();
        BHState Tick();
    }
}

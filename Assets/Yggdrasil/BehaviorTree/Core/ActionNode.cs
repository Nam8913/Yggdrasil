namespace BehaviorTree
{
    public abstract class ActionNode : NodeBT
    {
        // Default: OnEvaluate() calls OnUpdate() (backward compatible)
        // Override OnEvaluate() for pure logic (thread-safe)
        // Override OnExecute() for Unity API calls (main thread only)
    }
}

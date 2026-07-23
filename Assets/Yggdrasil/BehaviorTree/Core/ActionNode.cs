namespace BehaviorTree
{
    /// <summary>
    /// Base class for leaf nodes that perform actions (movement, physics, animation).
    /// Lớp cơ sở cho các node lá thực thi hành vi (di chuyển, vật lý, animation).
    ///
    /// Override OnExecute() for Unity API calls (main thread only).
    /// Override OnEvaluate() for pure logic that can run on worker thread.
    /// Ghi đè OnExecute() cho các lệnh gọi Unity API (chỉ main thread).
    /// Ghi đè OnEvaluate() cho logic thuần túy có thể chạy trên worker thread.
    /// </summary>
    public abstract class ActionNode : NodeBT
    {
    }
}

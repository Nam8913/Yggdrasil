namespace BehaviorTree
{
    /// <summary>
    /// Always returns Failure regardless of the child's result.
    /// Luôn trả về Failure bất kể kết quả của con.
    ///
    /// The child still runs (and may have side effects), but its
    /// Success/Failure/Running is converted to Failure.
    /// Con vẫn chạy (và có thể có side effects), nhưng
    /// Success/Failure/Running của nó đều được chuyển thành Failure.
    ///
    /// Usage: "try this but force failure" / "simulate failure for testing".
    /// Sử dụng: "thử cái này nhưng bắt buộc thất bại" / "mô phỏng thất bại để test".
    /// </summary>
    public class FailerNode : DecoratorNode
    {
        protected override BHState OnUpdate()
        {
            Child?.Tick();
            return BHState.Failure;
        }

        protected override BHState OnEvaluate()
        {
            Child?.Evaluate();
            return BHState.Failure;
        }

        protected override BHState OnExecute()
        {
            Child?.Execute();
            return BHState.Failure;
        }
    }
}

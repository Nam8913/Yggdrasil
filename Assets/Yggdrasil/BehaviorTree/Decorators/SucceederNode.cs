namespace BehaviorTree
{
    /// <summary>
    /// Always returns Success regardless of the child's result.
    /// Luôn trả về Success bất kể kết quả của con.
    ///
    /// The child still runs (and may have side effects), but its
    /// Success/Failure/Running is converted to Success.
    /// Con vẫn chạy (và có thể có side effects), nhưng
    /// Success/Failure/Running của nó đều được chuyển thành Success.
    ///
    /// Usage: "run this action but don't care if it fails" / "ensure sequence continues".
    /// Sử dụng: "chạy hành vi này nhưng không quan tâm nếu nó thất bại" / "đảm bảo sequence tiếp tục".
    /// </summary>
    public class SucceederNode : DecoratorNode
    {
        protected override BHState OnUpdate()
        {
            Child?.Tick();
            return BHState.Success;
        }

        protected override BHState OnEvaluate()
        {
            Child?.Evaluate();
            return BHState.Success;
        }

        protected override BHState OnExecute()
        {
            Child?.Execute();
            return BHState.Success;
        }
    }
}

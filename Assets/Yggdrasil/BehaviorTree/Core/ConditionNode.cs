namespace BehaviorTree
{
    /// <summary>
    /// Base class for condition check nodes (pure logic, no side effects).
    /// Lớp cơ sở cho các node kiểm tra điều kiện (logic thuần túy, không có tác dụng phụ).
    ///
    /// Returns Success if Check() returns true, Failure otherwise.
    /// Trả về Success nếu Check() trả về true, ngược lại trả về Failure.
    ///
    /// Conditions never return Running — they are instant checks.
    /// Condition không bao giờ trả về Running — chúng là kiểm tra tức thì.
    /// </summary>
    public abstract class ConditionNode : NodeBT
    {
        protected override BHState OnUpdate()
        {
            return Check() ? BHState.Success : BHState.Failure;
        }

        protected override BHState OnEvaluate()
        {
            return Check() ? BHState.Success : BHState.Failure;
        }

        // Conditions have no execute phase — result is cached from Evaluate
        // Condition không có giai đoạn execute — kết quả được cache từ Evaluate
        protected override BHState OnExecute()
        {
            return EvaluatedState;
        }

        // Subclasses implement this to define the condition check
        // Các lớp con triển khai phương thức này để xác định điều kiện kiểm tra
        protected abstract bool Check();
    }
}

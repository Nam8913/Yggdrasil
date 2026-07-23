namespace BehaviorTree
{
    /// <summary>
    /// NAND gate: succeeds if ANY child fails, fails only if ALL children succeed.
    /// Cổng NAND: thành công nếu BẤT KỲ con nào thất bại, chỉ thất bại khi TẤT CẢ thành công.
    ///
    /// Truth table (2 children):
    /// Bảng chân trị (2 con):
    ///   A=Success, B=Success → Failure
    ///   A=Success, B=Failure → Success
    ///   A=Failure, B=Success → Success
    ///   A=Failure, B=Failure → Success
    ///
    /// Usage: "abort if everything is working fine" / "trigger alarm if no failure yet".
    /// Sử dụng: "hủy nếu mọi thứ đang hoạt động tốt" / "kích hoạt cảnh báo nếu chưa có thất bại".
    /// </summary>
    public class NandNode : CompositeNode
    {
        public bool BreakOnFirstFailure { get; set; } = false;
        protected override BHState OnUpdate()
        {
            bool anyFailed = false;

            for (int i = 0; i < Children.Count; i++)
            {
                var state = Children[i].Tick();

                if (state == BHState.Running)
                    return BHState.Running;

                if (state == BHState.Failure)
                {
                    anyFailed = true;
                    if (BreakOnFirstFailure)
                        return BHState.Success;
                }
            }

            // NAND: fail only if ALL succeeded (no failures found)
            // NAND: chỉ thất bại khi TẤT CẢ thành công (không tìm thấy thất bại nào)
            return anyFailed ? BHState.Success : BHState.Failure;
        }

        protected override BHState OnEvaluate()
        {
            bool anyFailed = false;

            for (int i = 0; i < Children.Count; i++)
            {
                var state = Children[i].Evaluate();

                if (state == BHState.Running)
                    return BHState.Running;

                if (state == BHState.Failure)
                    anyFailed = true;
            }

            return anyFailed ? BHState.Success : BHState.Failure;
        }

        protected override BHState OnExecute()
        {
            // Execute the last ticked child
            // Execute con cuối cùng đã được tick
            if (CurrentChildIndex < Children.Count)
                return Children[CurrentChildIndex].Execute();

            return BHState.Success;
        }
    }
}

namespace BehaviorTree
{
    /// <summary>
    /// NOR gate: succeeds only if ALL children fail, fails if ANY child succeeds.
    /// Cổng NOR: chỉ thành công khi TẤT CẢ con thất bại, thất bại nếu BẤT KỲ con nào thành công.
    ///
    /// Truth table (2 children):
    /// Bảng chân trị (2 con):
    ///   A=Failure, B=Failure → Success
    ///   A=Failure, B=Success → Failure
    ///   A=Success, B=Failure → Failure
    ///   A=Success, B=Success → Failure
    ///
    /// Usage: "proceed only when all attempts failed" / "fallback when nothing works".
    /// Sử dụng: "tiến hành chỉ khi tất cả nỗ lực thất bại" / "dự phòng khi mọi thứ không hoạt động".
    /// </summary>
    public class NorNode : CompositeNode
    {
        protected override BHState OnUpdate()
        {
            bool anySucceeded = false;

            for (int i = 0; i < Children.Count; i++)
            {
                var state = Children[i].Tick();

                if (state == BHState.Running)
                    return BHState.Running;

                if (state == BHState.Success)
                {
                    anySucceeded = true;
                    // Don't break — still need to tick remaining children
                    // Không break — vẫn cần tick các con còn lại
                }
            }

            // NOR: succeed only if ALL failed (no successes found)
            // NOR: chỉ thành công khi TẤT CẢ thất bại (không tìm thấy thành công nào)
            return anySucceeded ? BHState.Failure : BHState.Success;
        }

        protected override BHState OnEvaluate()
        {
            bool anySucceeded = false;

            for (int i = 0; i < Children.Count; i++)
            {
                var state = Children[i].Evaluate();

                if (state == BHState.Running)
                    return BHState.Running;

                if (state == BHState.Success)
                    anySucceeded = true;
            }

            return anySucceeded ? BHState.Failure : BHState.Success;
        }

        protected override BHState OnExecute()
        {
            if (CurrentChildIndex < Children.Count)
                return Children[CurrentChildIndex].Execute();

            return BHState.Failure;
        }
    }
}

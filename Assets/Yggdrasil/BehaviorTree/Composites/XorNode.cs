namespace BehaviorTree
{
    /// <summary>
    /// XOR gate: succeeds when children return a MIX of Success and Failure.
    /// Cổng XOR: thành công khi các con trả về CẢ Success lẫn Failure.
    ///
    /// Truth table (2 children):
    /// Bảng chân trị (2 con):
    ///   A=Success, B=Success → Failure  (all same = no disagreement)
    ///   A=Success, B=Failure → Success  (mixed = disagreement)
    ///   A=Failure, B=Success → Success  (mixed = disagreement)
    ///   A=Failure, B=Failure → Failure  (all same = no disagreement)
    ///
    /// Usage: "detect conflicting states" / "check if behaviors disagree".
    /// Sử dụng: "phát hiện trạng thái mâu thuẫn" / "kiểm tra nếu hành vi bất đồng".
    /// </summary>
    public class XorNode : CompositeNode
    {
        protected override BHState OnUpdate()
        {
            bool hasSuccess = false;
            bool hasFailure = false;

            for (int i = 0; i < Children.Count; i++)
            {
                var state = Children[i].Tick();

                if (state == BHState.Running)
                    return BHState.Running;

                if (state == BHState.Success)
                    hasSuccess = true;

                if (state == BHState.Failure)
                    hasFailure = true;

                // Both found — no need to tick remaining children
                // Cả hai đã tìm thấy — không cần tick các con còn lại
                if (hasSuccess && hasFailure)
                    return BHState.Success;
            }

            // XOR: succeed only if mixed (both success and failure present)
            // XOR: chỉ thành công nếu hỗn hợp (cả success và failure đều có)
            return (hasSuccess && hasFailure) ? BHState.Success : BHState.Failure;
        }

        protected override BHState OnEvaluate()
        {
            bool hasSuccess = false;
            bool hasFailure = false;

            for (int i = 0; i < Children.Count; i++)
            {
                var state = Children[i].Evaluate();

                if (state == BHState.Running)
                    return BHState.Running;

                if (state == BHState.Success)
                    hasSuccess = true;

                if (state == BHState.Failure)
                    hasFailure = true;

                if (hasSuccess && hasFailure)
                    return BHState.Success;
            }

            return (hasSuccess && hasFailure) ? BHState.Success : BHState.Failure;
        }

        protected override BHState OnExecute()
        {
            if (CurrentChildIndex < Children.Count)
                return Children[CurrentChildIndex].Execute();

            return BHState.Failure;
        }
    }
}

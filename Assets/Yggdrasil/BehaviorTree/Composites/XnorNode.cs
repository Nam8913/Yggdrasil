namespace BehaviorTree
{
    /// <summary>
    /// XNOR gate (NOT XOR): succeeds when ALL children agree (all succeed or all fail).
    /// Cổng XNOR (NOT XOR): thành công khi TẤT CẢ con đồng ý (đồng thành công hoặc đồng thất bại).
    ///
    /// Truth table (2 children):
    /// Bảng chân trị (2 con):
    ///   A=Success, B=Success → Success  (all agree)
    ///   A=Success, B=Failure → Failure  (disagree)
    ///   A=Failure, B=Success → Failure  (disagree)
    ///   A=Failure, B=Failure → Success  (all agree)
    ///
    /// Usage: "ensure consistent behavior" / "all branches must agree".
    /// Sử dụng: "đảm bảo hành vi nhất quán" / "tất cả nhánh phải đồng ý".
    /// </summary>
    public class XnorNode : CompositeNode
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

                // Disagreement found — no need to continue
                // Tìm thấy bất đồng — không cần tiếp tục
                if (hasSuccess && hasFailure)
                    return BHState.Failure;
            }

            // XNOR: succeed if all agree (no disagreement found)
            // XNOR: thành công nếu tất cả đồng ý (không tìm thấy bất đồng)
            return BHState.Success;
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
                    return BHState.Failure;
            }

            return BHState.Success;
        }

        protected override BHState OnExecute()
        {
            if (CurrentChildIndex < Children.Count)
                return Children[CurrentChildIndex].Execute();

            return BHState.Success;
        }
    }
}

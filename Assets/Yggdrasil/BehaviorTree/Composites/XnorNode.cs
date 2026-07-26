namespace BehaviorTree
{
    /// <summary>
    /// XNOR gate (NOT XOR): succeeds when ALL children agree (all succeed or all fail).
    /// Cổng XNOR (NOT XOR): thành công khi TẤT CẢ con đồng ý (đồng thành công hoặc đồng thất bại).
    ///
    /// - BreakOnMixed = true (default): return Failure as soon as disagreement is found.
    /// - BreakOnMixed = false: tick all children first, then check.
    /// BreakOnMixed = true (mặc định): trả về Failure ngay khi tìm thấy bất đồng.
    /// BreakOnMixed = false: tick tất cả con trước, rồi kiểm tra.
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
        // If true, return Failure immediately when disagreement (success + failure) is found
        // Nếu true, trả về Failure ngay khi tìm thấy bất đồng (success + failure)
        public bool BreakOnMixed { get; set; } = true;

        protected override BHState OnUpdate(ref RunnerObserver observer)
        {
            bool hasSuccess = false, hasFailure = false;
            while (CurrentChildIndex < Children.Count)
            {
                observer.SetChildIndex(CurrentChildIndex);
                observer.Descend();
                var state = Children[CurrentChildIndex].Tick(ref observer);
                observer.Ascend();

                if (state == BHState.Running) return BHState.Running;
                if (state == BHState.Success) hasSuccess = true;
                if (state == BHState.Failure) hasFailure = true;
                CurrentChildIndex++;
                if (BreakOnMixed && hasSuccess && hasFailure) break;
            }
            CurrentChildIndex = 0;
            return (hasSuccess && hasFailure) ? BHState.Failure : BHState.Success;
        }

        protected override BHState OnEvaluate(ref RunnerObserver observer)
        {
            bool hasSuccess = false, hasFailure = false;
            while (CurrentChildIndex < Children.Count)
            {
                observer.SetChildIndex(CurrentChildIndex);
                observer.Descend();
                var state = Children[CurrentChildIndex].Evaluate(ref observer);
                observer.Ascend();
                if (state == BHState.Running) return BHState.Running;
                if (state == BHState.Success) hasSuccess = true;
                if (state == BHState.Failure) hasFailure = true;
                CurrentChildIndex++;
                if (BreakOnMixed && hasSuccess && hasFailure) break;
            }
            CurrentChildIndex = 0;
            return (hasSuccess && hasFailure) ? BHState.Failure : BHState.Success;
        }

        protected override BHState OnExecute()
        {
            if (CurrentChildIndex < Children.Count) return Children[CurrentChildIndex].Execute();
            return BHState.Success;
        }
    }
}

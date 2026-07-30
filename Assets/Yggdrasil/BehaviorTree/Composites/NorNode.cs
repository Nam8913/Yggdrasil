namespace BehaviorTree
{
    /// <summary>
    /// NOR gate: succeeds only if ALL children fail, fails if ANY child succeeds.
    /// Cổng NOR: chỉ thành công khi TẤT CẢ con thất bại, thất bại nếu BẤT KỲ con nào thành công.
    ///
    /// - BreakOnFirstSuccess = false: tick all children, then decide.
    /// - BreakOnFirstSuccess = true (default): stop and return Failure as soon as any child succeeds.
    /// BreakOnFirstSuccess = false: tick tất cả con, rồi quyết định.
    /// BreakOnFirstSuccess = true (mặc định): dừng và trả về Failure ngay khi bất kỳ con nào thành công.
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
        // If true, stop ticking remaining children when first success is found
        // Nếu true, ngừng tick các con còn lại khi tìm thấy thành công đầu tiên
        public bool BreakOnFirstSuccess { get; set; } = true;

        protected override BHState OnUpdate(ref RunnerObserver observer)
        {
            bool anySucceeded = false;
            while (CurrentChildIndex < Children.Count)
            {
                observer.SetChildIndex(CurrentChildIndex);
                observer.Descend();
                var state = Children[CurrentChildIndex].Tick(ref observer);
                observer.Ascend();

                if (state == BHState.Running) return BHState.Running;
                if (state == BHState.Success)
                {
                    anySucceeded = true;
                    if (BreakOnFirstSuccess) { CurrentChildIndex = 0; return BHState.Failure; }
                }
                CurrentChildIndex++;
            }
            CurrentChildIndex = 0;
            return anySucceeded ? BHState.Failure : BHState.Success;
        }

        protected override BHState OnEvaluate(ref RunnerObserver observer)
        {
            bool anySucceeded = false;
            while (CurrentChildIndex < Children.Count)
            {
                observer.SetChildIndex(CurrentChildIndex);
                observer.Descend();
                var state = Children[CurrentChildIndex].Evaluate(ref observer);
                observer.Ascend();
                if (state == BHState.Running) return BHState.Running;
                if (state == BHState.Success)
                {
                    anySucceeded = true;
                    if (BreakOnFirstSuccess) { CurrentChildIndex = 0; return BHState.Failure; }
                }
                CurrentChildIndex++;
            }
            CurrentChildIndex = 0;
            return anySucceeded ? BHState.Failure : BHState.Success;
        }

        protected override BHState OnExecute()
        {
            if (CurrentChildIndex < Children.Count) return Children[CurrentChildIndex].Execute();
            return BHState.Failure;
        }
    }
}

namespace BehaviorTree
{
    /// <summary>
    /// NAND gate: succeeds if ANY child fails, fails only if ALL children succeed.
    /// Cổng NAND: thành công nếu BẤT KỲ con nào thất bại, chỉ thất bại khi TẤT CẢ thành công.
    ///
    /// - BreakOnFirstFailure = false: tick all children, then decide.
    /// - BreakOnFirstFailure = true (default): stop and return Success as soon as any child fails.
    /// BreakOnFirstFailure = false: tick tất cả con, rồi quyết định.
    /// BreakOnFirstFailure = true (mặc định): dừng và trả về Success ngay khi bất kỳ con nào thất bại.
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
        public bool BreakOnFirstFailure { get; set; } = true;

        protected override BHState OnUpdate(ref RunnerObserver observer)
        {
            bool anyFailed = false;
            while (CurrentChildIndex < Children.Count)
            {
                observer.SetChildIndex(CurrentChildIndex);
                observer.Descend();
                var state = Children[CurrentChildIndex].Tick(ref observer);
                observer.Ascend();

                if (state == BHState.Running) return BHState.Running;
                if (state == BHState.Failure)
                {
                    anyFailed = true;
                    if (BreakOnFirstFailure) { CurrentChildIndex = 0; return BHState.Success; }
                }
                CurrentChildIndex++;
            }
            CurrentChildIndex = 0;
            return anyFailed ? BHState.Success : BHState.Failure;
        }

        protected override BHState OnEvaluate(ref RunnerObserver observer)
        {
            bool anyFailed = false;
            while (CurrentChildIndex < Children.Count)
            {
                observer.SetChildIndex(CurrentChildIndex);
                observer.Descend();
                var state = Children[CurrentChildIndex].Evaluate(ref observer);
                observer.Ascend();
                if (state == BHState.Running) return BHState.Running;
                if (state == BHState.Failure) anyFailed = true;
                CurrentChildIndex++;
            }
            CurrentChildIndex = 0;
            return anyFailed ? BHState.Success : BHState.Failure;
        }

        protected override BHState OnExecute()
        {
            if (CurrentChildIndex < Children.Count) return Children[CurrentChildIndex].Execute();
            return BHState.Success;
        }
    }
}

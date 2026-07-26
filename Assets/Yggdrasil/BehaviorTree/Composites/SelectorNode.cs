namespace BehaviorTree
{
    /// <summary>
    /// Tries children in order. Returns Success if ANY child succeeds.
    /// Thử các con theo thứ tự. Trả về Success nếu BẤT KỲ con nào thành công.
    ///
    /// - BreakOnFirstSuccess = true (default): stop and return Success immediately.
    /// - BreakOnFirstSuccess = false: tick all children, then decide.
    /// BreakOnFirstSuccess = true (mặc định): dừng và trả về Success ngay lập tức.
    /// BreakOnFirstSuccess = false: tick tất cả con, rồi quyết định.
    /// </summary>
    public class SelectorNode : CompositeNode
    {
        // If true, return Success as soon as a child succeeds
        // Nếu true, trả về Success ngay khi một con thành công
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

                if (state == BHState.Running)
                    return BHState.Running;

                if (state == BHState.Success)
                {
                    anySucceeded = true;
                    if (BreakOnFirstSuccess)
                    {
                        CurrentChildIndex = 0;
                        return BHState.Success;
                    }
                }

                CurrentChildIndex++;
            }

            CurrentChildIndex = 0;
            return anySucceeded ? BHState.Success : BHState.Failure;
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
                    if (BreakOnFirstSuccess)
                    {
                        CurrentChildIndex = 0;
                        return BHState.Success;
                    }
                }
                CurrentChildIndex++;
            }
            CurrentChildIndex = 0;
            return anySucceeded ? BHState.Success : BHState.Failure;
        }

        protected override BHState OnExecute()
        {
            if (CurrentChildIndex < Children.Count)
                return Children[CurrentChildIndex].Execute();
            return BHState.Failure;
        }
    }
}

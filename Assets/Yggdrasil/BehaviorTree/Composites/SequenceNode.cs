namespace BehaviorTree
{
    /// <summary>
    /// Executes children in order. Returns Success only if ALL children succeed.
    /// Thực thi các con theo thứ tự. Chỉ trả về Success khi TẤT CẢ con thành công.
    ///
    /// - BreakOnFirstFailure = true (default): stop and return Failure immediately.
    /// - BreakOnFirstFailure = false: tick all children, then decide.
    /// BreakOnFirstFailure = true (mặc định): dừng và trả về Failure ngay lập tức.
    /// BreakOnFirstFailure = false: tick tất cả con, rồi quyết định.
    /// </summary>
    public class SequenceNode : CompositeNode
    {
        // If true, return Failure as soon as a child fails
        // Nếu true, trả về Failure ngay khi một con thất bại
        public bool BreakOnFirstFailure { get; set; } = true;

        protected override BHState OnUpdate()
        {
            bool anyFailed = false;

            while (CurrentChildIndex < Children.Count)
            {
                var state = Children[CurrentChildIndex].Tick();

                if (state == BHState.Running)
                    return BHState.Running;

                if (state == BHState.Failure)
                {
                    anyFailed = true;
                    if (BreakOnFirstFailure)
                    {
                        CurrentChildIndex = 0;
                        return BHState.Failure;
                    }
                }

                CurrentChildIndex++;
            }

            CurrentChildIndex = 0;
            return anyFailed ? BHState.Failure : BHState.Success;
        }

        protected override BHState OnEvaluate()
        {
            bool anyFailed = false;

            while (CurrentChildIndex < Children.Count)
            {
                var state = Children[CurrentChildIndex].Evaluate();

                if (state == BHState.Running)
                    return BHState.Running;

                if (state == BHState.Failure)
                {
                    anyFailed = true;
                    if (BreakOnFirstFailure)
                    {
                        CurrentChildIndex = 0;
                        return BHState.Failure;
                    }
                }

                CurrentChildIndex++;
            }

            CurrentChildIndex = 0;
            return anyFailed ? BHState.Failure : BHState.Success;
        }

        protected override BHState OnExecute()
        {
            if (CurrentChildIndex < Children.Count)
                return Children[CurrentChildIndex].Execute();

            return BHState.Success;
        }
    }
}

namespace BehaviorTree
{
    /// <summary>
    /// Tries children in order. Returns Success if ANY child succeeds.
    /// Thử các con theo thứ tự. Trả về Success nếu BẤT KỲ con nào thành công.
    ///
    /// - If a child returns Success → selector succeeds immediately, resets index.
    /// - If a child returns Running → selector pauses, returns Running.
    /// Nếu một con trả về Success → selector thành công ngay, reset chỉ số.
    /// Nếu một con trả về Running → selector tạm dừng, trả về Running.
    ///
    /// Analogous to OR logic gate / logical disjunction.
    /// Tương tự cổng logic OR / phép tuyển.
    /// </summary>
    public class SelectorNode : CompositeNode
    {
        protected override BHState OnUpdate()
        {
            while (CurrentChildIndex < Children.Count)
            {
                var state = Children[CurrentChildIndex].Tick();

                if (state == BHState.Running)
                    return BHState.Running;

                if (state == BHState.Success)
                {
                    CurrentChildIndex = 0;
                    return BHState.Success;
                }

                CurrentChildIndex++;
            }

            CurrentChildIndex = 0;
            return BHState.Failure;
        }

        protected override BHState OnEvaluate()
        {
            while (CurrentChildIndex < Children.Count)
            {
                var state = Children[CurrentChildIndex].Evaluate();

                if (state == BHState.Running)
                    return BHState.Running;

                if (state == BHState.Success)
                {
                    CurrentChildIndex = 0;
                    return BHState.Success;
                }

                CurrentChildIndex++;
            }

            CurrentChildIndex = 0;
            return BHState.Failure;
        }

        protected override BHState OnExecute()
        {
            if (CurrentChildIndex < Children.Count)
            {
                return Children[CurrentChildIndex].Execute();
            }
            return BHState.Failure;
        }
    }
}

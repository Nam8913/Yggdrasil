namespace BehaviorTree
{
    /// <summary>
    /// Executes children in order. Returns Success only if ALL children succeed.
    /// Thực thi các con theo thứ tự. Chỉ trả về Success khi TẤT CẢ con thành công.
    ///
    /// - If a child returns Running → sequence pauses, returns Running.
    /// - If a child returns Failure → sequence fails immediately, resets index.
    /// Nếu một con trả về Running → sequence tạm dừng, trả về Running.
    /// Nếu một con trả về Failure → sequence thất bại ngay lập tức, reset chỉ số.
    ///
    /// Analogous to AND logic gate / logical conjunction.
    /// Tương tự cổng logic AND / phép hội.
    /// </summary>
    public class SequenceNode : CompositeNode
    {
        protected override BHState OnUpdate()
        {
            while (CurrentChildIndex < Children.Count)
            {
                var state = Children[CurrentChildIndex].Tick();

                if (state == BHState.Running)
                    return BHState.Running;

                if (state == BHState.Failure)
                {
                    CurrentChildIndex = 0;
                    return BHState.Failure;
                }

                CurrentChildIndex++;
            }

            CurrentChildIndex = 0;
            return BHState.Success;
        }

        protected override BHState OnEvaluate()
        {
            while (CurrentChildIndex < Children.Count)
            {
                var state = Children[CurrentChildIndex].Evaluate();

                if (state == BHState.Running)
                    return BHState.Running;

                if (state == BHState.Failure)
                {
                    CurrentChildIndex = 0;
                    return BHState.Failure;
                }

                CurrentChildIndex++;
            }

            CurrentChildIndex = 0;
            return BHState.Success;
        }

        protected override BHState OnExecute()
        {
            if (CurrentChildIndex < Children.Count)
            {
                return Children[CurrentChildIndex].Execute();
            }
            return BHState.Success;
        }
    }
}

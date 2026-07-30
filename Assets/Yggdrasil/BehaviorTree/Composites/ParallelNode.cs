using System.Collections.Generic;

namespace BehaviorTree
{
    /// <summary>
    /// Execution policy for ParallelNode.
    /// Chính sách thực thi cho ParallelNode.
    /// </summary>
    public enum ParallelPolicy
    {
        // AND: succeed only when ALL children succeed
        // AND: chỉ thành công khi TẤT CẢ con thành công
        RequireAll,

        // OR: succeed when ANY child succeeds
        // OR: thành công khi BẤT KỲ con nào thành công
        RequireOne,

        // NAND: succeed if ANY child fails (fail only if all succeed)
        // NAND: thành công nếu BẤT KỲ con nào thất bại
        RequireAnyFailure,

        // NOR: succeed only when ALL children fail
        // NOR: chỉ thành công khi TẤT CẢ con thất bại
        RequireAllFailure,

        // XOR: succeed if children return mixed results (both success and failure)
        // XOR: thành công nếu kết quả hỗn hợp (cả success và failure)
        RequireMixed,

        // XNOR: succeed if all children agree (all succeed or all fail)
        // XNOR: thành công nếu tất cả con đồng ý (đồng success hoặc đồng failure)
        RequireConsistent
    }

    /// <summary>
    /// Executes ALL children simultaneously every tick.
    /// Thực thi TẤT CẢ con đồng thời mỗi lần tick.
    ///
    /// Unlike Sequence/Selector which short-circuit, ParallelNode always
    /// ticks every child that is still Running.
    /// Khác với Sequence/Selector dừng lại sớm, ParallelNode luôn tick
    /// tất cả con đang trong trạng thái Running.
    ///
    /// Use cases: simultaneous movement + perception, coordinated actions.
    /// Trường hợp sử dụng: di chuyển + cảm nhận đồng thời, hành vi phối hợp.
    /// </summary>
    public class ParallelNode : CompositeNode
    {
        // Policy determines success condition
        // Chính sách xác định điều kiện thành công
        public ParallelPolicy Policy { get; set; } = ParallelPolicy.RequireAll;

        // Tracks the state of each child independently
        // Theo dõi trạng thái của mỗi con một cách độc lập
        private readonly List<BHState> _childStates = new List<BHState>();

        protected override void OnEnter()
        {
            _childStates.Clear();
            for (int i = 0; i < Children.Count; i++)
                _childStates.Add(BHState.Running);
        }

        protected override void OnReset()
        {
            base.OnReset();
            _childStates.Clear();
        }

        private void EnsureChildStates()
        {
            if (_childStates.Count != Children.Count)
            {
                _childStates.Clear();
                for (int i = 0; i < Children.Count; i++)
                    _childStates.Add(BHState.Running);
            }
        }

        protected override BHState OnUpdate(ref RunnerObserver observer)
        {
            EnsureChildStates();
            if (TickChildren(ref observer)) return BHState.Running;
            return EvaluatePolicy();
        }

        protected override BHState OnEvaluate(ref RunnerObserver observer)
        {
            EnsureChildStates();
            if (EvaluateChildren(ref observer)) return BHState.Running;
            return EvaluatePolicy();
        }

        protected override BHState OnExecute()
        {
            EnsureChildStates();
            bool anyRunning = false;

            for (int i = 0; i < Children.Count; i++)
            {
                if (_childStates[i] != BHState.Running)
                    continue;

                _childStates[i] = Children[i].Execute();

                if (_childStates[i] == BHState.Running)
                    anyRunning = true;
            }

            if (anyRunning)
                return BHState.Running;

            return EvaluatePolicy();
        }

        // Evaluate all children that are still Running (two-phase)
        // Đánh giá tất cả con đang Running (hai phase)
        // Returns true if any child is still Running
        private bool EvaluateChildren(ref RunnerObserver observer)
        {
            bool anyRunning = false;

            for (int i = 0; i < Children.Count; i++)
            {
                if (_childStates[i] != BHState.Running)
                    continue;

                observer.SetChildIndex(i);
                observer.Descend();
                _childStates[i] = Children[i].Evaluate(ref observer);
                observer.Ascend();

                if (_childStates[i] == BHState.Running)
                    anyRunning = true;
            }

            return anyRunning;
        }

        // Tick all children that are still Running (single-phase legacy)
        // Tick tất cả con đang Running (đơn phase cũ)
        // Returns true if any child is still Running
        private bool TickChildren(ref RunnerObserver observer)
        {
            bool anyRunning = false;

            for (int i = 0; i < Children.Count; i++)
            {
                if (_childStates[i] != BHState.Running)
                    continue;

                observer.SetChildIndex(i);
                observer.Descend();
                _childStates[i] = Children[i].Tick(ref observer);
                observer.Ascend();

                if (_childStates[i] == BHState.Running)
                    anyRunning = true;
            }

            return anyRunning;
        }

        // Evaluate the final result based on the policy
        // Đánh giá kết quả cuối cùng dựa trên chính sách
        private BHState EvaluatePolicy()
        {
            int successCount = 0;
            int failureCount = 0;

            for (int i = 0; i < _childStates.Count; i++)
            {
                if (_childStates[i] == BHState.Success)
                    successCount++;
                else if (_childStates[i] == BHState.Failure)
                    failureCount++;
            }

            int total = _childStates.Count;

            return Policy switch
            {
                // AND: all must succeed
                ParallelPolicy.RequireAll => successCount == total ? BHState.Success : BHState.Failure,

                // OR: any must succeed
                ParallelPolicy.RequireOne => successCount > 0 ? BHState.Success : BHState.Failure,

                // NAND: any must fail (fail only if all succeed)
                ParallelPolicy.RequireAnyFailure => failureCount > 0 ? BHState.Success : BHState.Failure,

                // NOR: all must fail
                ParallelPolicy.RequireAllFailure => failureCount == total ? BHState.Success : BHState.Failure,

                // XOR: mixed results (both success and failure)
                ParallelPolicy.RequireMixed => (successCount > 0 && failureCount > 0) ? BHState.Success : BHState.Failure,

                // XNOR: all agree (all success or all failure)
                ParallelPolicy.RequireConsistent => (successCount == 0 || failureCount == 0) ? BHState.Success : BHState.Failure,

                _ => BHState.Failure
            };
        }
    }
}

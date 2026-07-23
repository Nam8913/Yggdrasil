using System.Collections.Generic;

namespace BehaviorTree
{
    /// <summary>
    /// Execution policy for ParallelNode.
    /// Chính sách thực thi cho ParallelNode.
    /// </summary>
    public enum ParallelPolicy
    {
        // Succeed only when ALL children succeed (AND logic)
        // Chỉ thành công khi TẤT CẢ con thành công (logic AND)
        RequireAll,

        // Succeed when ANY child succeeds (OR logic)
        // Thành công khi BẤT KỲ con nào thành công (logic OR)
        RequireOne
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
        // Policy determines success condition: RequireAll or RequireOne
        // Chính sách xác định điều kiện thành công: RequireAll hoặc RequireOne
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

        protected override BHState OnUpdate()
        {
            EnsureChildStates();
            bool anyRunning = false;

            for (int i = 0; i < Children.Count; i++)
            {
                if (_childStates[i] != BHState.Running)
                    continue;

                _childStates[i] = Children[i].Tick();

                if (_childStates[i] == BHState.Running)
                    anyRunning = true;
            }

            if (anyRunning)
                return BHState.Running;

            return Policy == ParallelPolicy.RequireAll ? CheckAll() : CheckOne();
        }

        // Phase 1: Evaluate logic (thread-safe)
        protected override BHState OnEvaluate()
        {
            EnsureChildStates();
            bool anyRunning = false;

            for (int i = 0; i < Children.Count; i++)
            {
                if (_childStates[i] != BHState.Running)
                    continue;

                _childStates[i] = Children[i].Evaluate();

                if (_childStates[i] == BHState.Running)
                    anyRunning = true;
            }

            if (anyRunning)
                return BHState.Running;

            return Policy == ParallelPolicy.RequireAll ? CheckAll() : CheckOne();
        }

        // Phase 2: Execute Unity API (main thread)
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

            return Policy == ParallelPolicy.RequireAll ? CheckAll() : CheckOne();
        }

        private BHState CheckAll()
        {
            for (int i = 0; i < _childStates.Count; i++)
            {
                if (_childStates[i] == BHState.Failure)
                    return BHState.Failure;
            }
            return BHState.Success;
        }

        private BHState CheckOne()
        {
            for (int i = 0; i < _childStates.Count; i++)
            {
                if (_childStates[i] == BHState.Success)
                    return BHState.Success;
            }
            return BHState.Failure;
        }
    }
}

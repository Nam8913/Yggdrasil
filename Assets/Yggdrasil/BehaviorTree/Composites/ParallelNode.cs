using System.Collections.Generic;

namespace BehaviorTree
{
    public enum ParallelPolicy
    {
        RequireAll,
        RequireOne
    }

    public class ParallelNode : CompositeNode
    {
        public ParallelPolicy Policy { get; set; } = ParallelPolicy.RequireAll;

        private readonly List<BHState> _childStates = new List<BHState>();

        protected override void OnEnter()
        {
            _childStates.Clear();
            for (int i = 0; i < Children.Count; i++)
            {
                _childStates.Add(BHState.Running);
            }
        }

        protected override BHState OnUpdate()
        {
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

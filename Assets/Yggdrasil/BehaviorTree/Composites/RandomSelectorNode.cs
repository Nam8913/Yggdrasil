using System.Collections.Generic;

namespace BehaviorTree
{
    public class RandomSelectorNode : SelectorNode
    {
        private readonly List<int> _shuffledOrder = new List<int>();
        private bool _shuffled;

        protected override void OnEnter()
        {
            if (!_shuffled)
            {
                ShuffleChildren();
                _shuffled = true;
            }
        }

        private void ShuffleChildren()
        {
            _shuffledOrder.Clear();
            for (int i = 0; i < Children.Count; i++)
                _shuffledOrder.Add(i);

            for (int i = _shuffledOrder.Count - 1; i > 0; i--)
            {
                int j = UnityEngine.Random.Range(0, i + 1);
                (_shuffledOrder[i], _shuffledOrder[j]) = (_shuffledOrder[j], _shuffledOrder[i]);
            }
        }

        protected override BHState OnUpdate()
        {
            while (CurrentChildIndex < _shuffledOrder.Count)
            {
                var state = Children[_shuffledOrder[CurrentChildIndex]].Tick();

                if (state == BHState.Running)
                    return BHState.Running;

                if (state == BHState.Success)
                {
                    CurrentChildIndex = 0;
                    _shuffled = false;
                    return BHState.Success;
                }

                CurrentChildIndex++;
            }

            CurrentChildIndex = 0;
            _shuffled = false;
            return BHState.Failure;
        }

        // Phase 1: Evaluate logic (thread-safe)
        protected override BHState OnEvaluate()
        {
            while (CurrentChildIndex < _shuffledOrder.Count)
            {
                var state = Children[_shuffledOrder[CurrentChildIndex]].Evaluate();

                if (state == BHState.Running)
                    return BHState.Running;

                if (state == BHState.Success)
                {
                    CurrentChildIndex = 0;
                    _shuffled = false;
                    return BHState.Success;
                }

                CurrentChildIndex++;
            }

            CurrentChildIndex = 0;
            _shuffled = false;
            return BHState.Failure;
        }

        // Phase 2: Execute Unity API (main thread) - uses shuffled order
        protected override BHState OnExecute()
        {
            if (CurrentChildIndex < _shuffledOrder.Count)
            {
                return Children[_shuffledOrder[CurrentChildIndex]].Execute();
            }
            return BHState.Failure;
        }

        protected override void OnReset()
        {
            base.OnReset();
            _shuffled = false;
        }
    }
}

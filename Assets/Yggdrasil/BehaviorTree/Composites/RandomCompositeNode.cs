using System.Collections.Generic;

namespace BehaviorTree
{
    /// <summary>
    /// Logic gate modes for RandomCompositeNode.
    /// Chế độ cổng logic cho RandomCompositeNode.
    /// </summary>
    public enum LogicMode
    {
        AND,
        OR,
        NAND,
        NOR,
        XOR,
        XNOR
    }

    /// <summary>
    /// Execution mode for RandomCompositeNode.
    /// Chế độ thực thi cho RandomCompositeNode.
    /// </summary>
    public enum RandomTickMode
    {
        // Tick children sequentially (like Sequence/Selector)
        // Tick con theo thứ tự (giống Sequence/Selector)
        Sequential,

        // Tick ALL children simultaneously (like Parallel)
        // Tick TẤT CẢ con đồng thời (giống Parallel)
        Parallel
    }

    /// <summary>
    /// Random composite node that shuffles children order and applies logic gate evaluation.
    /// Node composite ngẫu nhiên xáo trộn thứ tự con và áp dụng đánh giá cổng logic.
    ///
    /// Combines RandomSelector + RandomSequence + Logic Gates into one configurable node.
    /// Kết hợp RandomSelector + RandomSequence + Logic Gates thành một node có thể cấu hình.
    ///
    /// Features:
    /// - Shuffled order: children evaluated in random order each activation
    /// - Logic modes: AND, OR, NAND, NOR, XOR, XNOR
    /// - Parallel tick: tick all children simultaneously or sequentially
    /// - Early exit: configurable per logic mode
    /// </summary>
    public class RandomCompositeNode : CompositeNode
    {
        // Logic evaluation mode
        public LogicMode Logic { get; set; } = LogicMode.AND;

        // Sequential or Parallel ticking
        public RandomTickMode TickMode { get; set; } = RandomTickMode.Sequential;

        // Early exit when first match is found
        public bool BreakOnFirstMatch { get; set; } = true;

        private readonly List<int> _shuffledOrder = new();
        private bool _shuffled;

        // Parallel mode: tracks state of each child
        private readonly List<BHState> _childStates = new();

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

        // ==================== OnUpdate (Legacy) ====================

        protected override BHState OnUpdate(ref RunnerObserver observer)
        {
            if (TickMode == RandomTickMode.Parallel)
                return TickParallel(ref observer, useEvaluate: false);
            return TickSequential(ref observer, useEvaluate: false);
        }

        protected override BHState OnEvaluate(ref RunnerObserver observer)
        {
            if (TickMode == RandomTickMode.Parallel)
                return TickParallel(ref observer, useEvaluate: true);
            return TickSequential(ref observer, useEvaluate: true);
        }

        // ==================== OnExecute ====================

        protected override BHState OnExecute()
        {
            if (TickMode == RandomTickMode.Parallel)
            {
                bool anyRunning = false;
                for (int i = 0; i < Children.Count; i++)
                {
                    if (_childStates[i] != BHState.Running)
                        continue;
                    _childStates[i] = Children[_shuffledOrder[i]].Execute();
                    if (_childStates[i] == BHState.Running)
                        anyRunning = true;
                }
                if (anyRunning) return BHState.Running;
                return EvaluateLogic(_childStates);
            }
            else
            {
                if (CurrentChildIndex < Children.Count)
                    return Children[_shuffledOrder[CurrentChildIndex]].Execute();
                return GetDefaultResult();
            }
        }

        // ==================== Sequential Mode ====================

        private BHState TickSequential(ref RunnerObserver observer, bool useEvaluate)
        {
            while (CurrentChildIndex < _shuffledOrder.Count)
            {
                observer.SetChildIndex(CurrentChildIndex);
                observer.Descend();
                var child = Children[_shuffledOrder[CurrentChildIndex]];
                var state = useEvaluate ? child.Evaluate(ref observer) : child.Tick(ref observer);
                observer.Ascend();

                if (state == BHState.Running)
                    return BHState.Running;

                if (state == BHState.Failure)
                {
                    if (Logic == LogicMode.AND && BreakOnFirstMatch)
                        return ResetAndReturn(BHState.Failure);

                    if (Logic == LogicMode.NAND && BreakOnFirstMatch)
                        return ResetAndReturn(BHState.Success);
                }

                if (state == BHState.Success)
                {
                    if (Logic == LogicMode.OR && BreakOnFirstMatch)
                        return ResetAndReturn(BHState.Success);

                    if (Logic == LogicMode.NOR && BreakOnFirstMatch)
                        return ResetAndReturn(BHState.Failure);
                }

                // Check XOR/XNOR early exit (mismatch found)
                if (Logic == LogicMode.XOR || Logic == LogicMode.XNOR)
                {
                    // For sequential XOR/XNOR, we need to track results
                    // But since we're sequential, we can't know the full picture
                    // until all children are ticked — so no early exit
                }

                CurrentChildIndex++;
            }

            CurrentChildIndex = 0;
            _shuffled = false;
            return GetDefaultResult();
        }

        // ==================== Parallel Mode ====================

        private BHState TickParallel(ref RunnerObserver observer, bool useEvaluate)
        {
            _childStates.Clear();
            for (int i = 0; i < Children.Count; i++)
                _childStates.Add(BHState.Running);

            bool anyRunning = false;
            for (int i = 0; i < Children.Count; i++)
            {
                if (_childStates[i] != BHState.Running)
                    continue;

                observer.SetChildIndex(i);
                observer.Descend();
                var child = Children[_shuffledOrder[i]];
                _childStates[i] = useEvaluate ? child.Evaluate(ref observer) : child.Tick(ref observer);
                observer.Ascend();

                if (_childStates[i] == BHState.Running)
                    anyRunning = true;

                // Early exit checks
                if (BreakOnFirstMatch)
                {
                    if (Logic == LogicMode.AND && _childStates[i] == BHState.Failure)
                        return ResetAndReturn(BHState.Failure);

                    if (Logic == LogicMode.OR && _childStates[i] == BHState.Success)
                        return ResetAndReturn(BHState.Success);

                    if (Logic == LogicMode.NAND && _childStates[i] == BHState.Failure)
                        return ResetAndReturn(BHState.Success);

                    if (Logic == LogicMode.NOR && _childStates[i] == BHState.Success)
                        return ResetAndReturn(BHState.Failure);
                }
            }

            if (anyRunning)
                return BHState.Running;

            return EvaluateLogic(_childStates);
        }

        // ==================== Logic Evaluation ====================

        private BHState EvaluateLogic(List<BHState> states)
        {
            int successCount = 0;
            int failureCount = 0;

            for (int i = 0; i < states.Count; i++)
            {
                if (states[i] == BHState.Success) successCount++;
                else if (states[i] == BHState.Failure) failureCount++;
            }

            int total = states.Count;

            return Logic switch
            {
                LogicMode.AND => successCount == total ? BHState.Success : BHState.Failure,
                LogicMode.OR => successCount > 0 ? BHState.Success : BHState.Failure,
                LogicMode.NAND => failureCount > 0 ? BHState.Success : BHState.Failure,
                LogicMode.NOR => failureCount == total ? BHState.Success : BHState.Failure,
                LogicMode.XOR => (successCount > 0 && failureCount > 0) ? BHState.Success : BHState.Failure,
                LogicMode.XNOR => (successCount == 0 || failureCount == 0) ? BHState.Success : BHState.Failure,
                _ => BHState.Failure
            };
        }

        // ==================== Helpers ====================

        private BHState GetDefaultResult()
        {
            return Logic switch
            {
                LogicMode.AND => BHState.Success,
                LogicMode.OR => BHState.Failure,
                LogicMode.NAND => BHState.Success,
                LogicMode.NOR => BHState.Failure,
                LogicMode.XOR => BHState.Failure,
                LogicMode.XNOR => BHState.Success,
                _ => BHState.Failure
            };
        }

        private BHState ResetAndReturn(BHState result)
        {
            CurrentChildIndex = 0;
            _shuffled = false;
            return result;
        }

        protected override void OnReset()
        {
            base.OnReset();
            _shuffled = false;
            _childStates.Clear();
        }
    }
}

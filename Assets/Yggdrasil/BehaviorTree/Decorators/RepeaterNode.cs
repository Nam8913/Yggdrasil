namespace BehaviorTree
{
    /// <summary>
    /// Repeats the child node multiple times. Resets and re-runs the child
    /// each time it completes (Success or Failure).
    /// Lặp lại node con nhiều lần. Reset và chạy lại con
    /// mỗi lần nó hoàn thành (Success hoặc Failure).
    ///
    /// - MaxRepeats = -1 (default): repeat forever (infinite loop).
    /// - MaxRepeats = N: repeat N times, then return Success.
    /// MaxRepeats = -1 (mặc định): lặp vô hạn.
    /// MaxRepeats = N: lặp N lần, rồi trả về Success.
    ///
    /// Usage: patrol waypoints, repeat attack sequence, loop animations.
    /// Sử dụng: tuần tra các điểm, lặp chuỗi tấn công, lặp animation.
    /// </summary>
    public class RepeaterNode : DecoratorNode
    {
        // Maximum number of repeats. -1 = infinite.
        // Số lần lặp tối đa. -1 = vô hạn.
        public int MaxRepeats { get; set; } = -1;

        private int _count;

        protected override void OnEnter()
        {
            _count = 0;
        }

        protected override BHState OnUpdate()
        {
            var state = Child.Tick();

            if (state == BHState.Running)
                return BHState.Running;

            _count++;

            if (MaxRepeats > 0 && _count >= MaxRepeats)
                return BHState.Success;

            // Reset child for next repeat
            // Reset con để lặp tiếp
            Child.Reset();
            return BHState.Running;
        }

        protected override BHState OnEvaluate()
        {
            var state = Child.Evaluate();

            if (state == BHState.Running)
                return BHState.Running;

            _count++;

            if (MaxRepeats > 0 && _count >= MaxRepeats)
                return BHState.Success;

            Child.Reset();
            return BHState.Running;
        }

        protected override BHState OnExecute()
        {
            return Child.Execute();
        }
    }
}

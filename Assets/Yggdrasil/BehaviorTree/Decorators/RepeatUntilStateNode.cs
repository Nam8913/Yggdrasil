namespace BehaviorTree
{
    /// <summary>
    /// Repeats the child until it returns a specific BHState.
    /// Lặp lại con cho đến khi nó trả về một BHState cụ thể.
    ///
    /// Each tick:
    ///   1. Ticks the child.
    ///   2. If child returns Running → this node returns Running.
    ///   3. If child returns the desired Value → returns Success.
    ///   4. If child returns any other state → resets child, returns Running.
    /// Mỗi tick:
    ///   1. Tick con.
    ///   2. Nếu con trả về Running → node này trả về Running.
    ///   3. Nếu con trả về giá trị Value mong muốn → trả về Success.
    ///   4. Nếu con trả về state khác → reset con, trả về Running.
    ///
    /// Usage: "retry action until it succeeds", "repeat until failure (for testing)".
    /// Sử dụng: "thử lại hành động cho đến khi thành công", "lặp cho đến khi thất bại (để test)".
    /// </summary>
    public class RepeatUntilStateNode : DecoratorNode
    {
        // The desired state that stops the repetition
        // Trạng thái mong muốn dừng sự lặp lại
        public BHState Value { get; set; } = BHState.Success;

        protected override BHState OnUpdate(ref RunnerObserver observer)
        {
            if (Child == null)
                return BHState.Failure;

            observer.Descend();
            var childState = Child.Tick(ref observer);
            observer.Ascend();
            // Child still running — wait
            // Con vẫn đang chạy — chờ

            if (childState == BHState.Running)
                return BHState.Running;

            // Child reached desired state — done
            // Con đạt trạng thái mong muốn — xong
            if (childState == Value)
                return BHState.Success;

            // Child finished but wrong state — reset and retry
            // Con hoàn thành nhưng sai trạng thái — reset và thử lại
            Child.Reset();
            return BHState.Running;
        }

        protected override BHState OnEvaluate(ref RunnerObserver observer)
        {
            if (Child == null)
                return BHState.Failure;

            observer.Descend();
            var childState = Child.Evaluate(ref observer);
            observer.Ascend();

            if (childState == BHState.Running)
                return BHState.Running;

            if (childState == Value)
                return BHState.Success;

            Child.Reset();
            return BHState.Running;
        }

        protected override BHState OnExecute()
        {
            if (Child == null)
                return BHState.Failure;

            return Child.Execute();
        }

        protected override void OnReset()
        {
            base.OnReset();
            Child?.Reset();
        }
    }
}

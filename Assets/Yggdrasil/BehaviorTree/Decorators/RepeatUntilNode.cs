using System;

namespace BehaviorTree
{
    /// <summary>
    /// Repeats the child until a condition becomes true.
    /// Lặp lại con cho đến khi điều kiện trở thành đúng.
    ///
    /// Each tick:
    ///   1. Ticks the child (may succeed, fail, or run).
    ///   2. Checks the condition.
    ///   3. If condition is true → returns Success (stops repeating).
    ///   4. If condition is false → returns Running (keeps repeating).
    /// Mỗi tick:
    ///   1. Tick con (có thể thành công, thất bại, hoặc đang chạy).
    ///   2. Kiểm tra điều kiện.
    ///   3. Nếu điều kiện đúng → trả về Success (ngừng lặp).
    ///   4. Nếu điều kiện sai → trả về Running (tiếp tục lặp).
    ///
    /// Usage: "keep attacking until health is low", "repeat until enemy is dead".
    /// Sử dụng: "tiếp tục tấn công cho đến khi máu thấp", "lặp cho đến khi kẻ thù chết".
    /// </summary>
    public class RepeatUntilNode : DecoratorNode
    {
        // Condition that stops the repetition when true
        // Điều kiện dừng lặp lại khi đúng
        public Func<bool> Condition { get; set; }

        protected override BHState OnUpdate(ref RunnerObserver observer)
        {
            if (Child == null) return BHState.Failure;

            observer.Descend();
            // Tick the child first — it may be doing work each frame
            // Tick con trước — nó có thể đang thực hiện công việc mỗi frame
            var childState = Child.Tick(ref observer);
            observer.Ascend();

            // If child is still running, we're not done yet
            // Nếu con vẫn đang chạy, chưa xong
            if (childState == BHState.Running) return BHState.Running;

            // Child completed (Success or Failure) — check the exit condition
            // Con hoàn thành (Success hoặc Failure) — kiểm tra điều kiện thoát
            if (Condition != null && Condition.Invoke())
                return BHState.Success;

            // Condition not met yet — reset child and keep repeating
            // Điều kiện chưa đạt — reset con và tiếp tục lặp
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

            if (Condition != null && Condition.Invoke())
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

using System;

namespace BehaviorTree
{
    /// <summary>
    /// Conditional decorator that only runs its child when a condition is true.
    /// If the condition becomes false while the child is running, the child is aborted.
    /// Decorator có điều kiện — chỉ chạy con khi điều kiện đúng.
    /// Nếu điều kiện trở thành sai trong khi con đang chạy, con bị hủy.
    ///
    /// Usage: "only attack if health > 50%", "only flee if enemy is strong".
    /// Sử dụng: "chỉ tấn công nếu máu > 50%", "chỉ bỏ chạy nếu kẻ thù mạnh".
    /// </summary>
    public class GuardNode : DecoratorNode
    {
        // Condition that must be true for the child to execute
        // Điều kiện phải đúng để con thực thi
        public Func<bool> Condition { get; set; }

        // Priority for ordering multiple guards (higher = checked first)
        // Độ ưu tiên để sắp xếp nhiều guard (cao hơn = kiểm tra trước)
        public int Priority { get; set; } = 0;

        public GuardNode(Func<bool> condition, NodeBT child, int priority = 0)
        {
            Condition = condition;
            Child = child;
            Priority = priority;
        }

        protected override BHState OnUpdate()
        {
            if (Child == null)
                return BHState.Failure;

            if (!Condition.Invoke())
            {
                // Abort child if it was running when condition became false
                // Hủy con nếu nó đang chạy khi điều kiện trở thành sai
                if (IsRunning)
                {
                    Child.Abort();
                    return BHState.Failure;
                }
                return BHState.Failure;
            }

            return Child.Tick();
        }

        protected override BHState OnEvaluate()
        {
            if (Child == null)
                return BHState.Failure;

            if (!Condition.Invoke())
            {
                if (IsRunning)
                {
                    Child.Abort();
                    return BHState.Failure;
                }
                return BHState.Failure;
            }

            return Child.Evaluate();
        }

        protected override BHState OnExecute()
        {
            if (Child == null)
                return BHState.Failure;

            return Child.Execute();
        }
    }
}

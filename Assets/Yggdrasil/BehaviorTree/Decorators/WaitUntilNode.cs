using System;

namespace BehaviorTree
{
    /// <summary>
    /// Waits until a condition becomes true before executing the child.
    /// Chờ đến khi điều kiện trở thành đúng trước khi thực thi con.
    ///
    /// While the condition is false, returns Running.
    /// Once the condition is true, delegates to the child normally.
    /// Khi điều kiện sai, trả về Running.
    /// Khi điều kiện đúng, ủy quyền cho con bình thường.
    ///
    /// Usage: wait for animation finish, wait for resource availability,
    /// wait for NPC to reach a point.
    /// Sử dụng: chờ animation kết thúc, chờ tài nguyên sẵn sàng,
    /// chờ NPC đến một điểm.
    /// </summary>
    public class WaitUntilNode : DecoratorNode
    {
        // Condition that must become true before the child runs
        // Điều kiện phải trở thành đúng trước khi con chạy
        public Func<bool> Condition { get; set; }
        public BHState ReturnIfChildNull { get; set; } = BHState.Failure;

        public WaitUntilNode(Func<bool> condition)
        {
            Condition = condition;
        }

        protected override BHState OnUpdate(ref RunnerObserver observer)
        {
            if (Condition != null && !Condition.Invoke())
                return BHState.Running;

            if(Child == null)
                return ReturnIfChildNull;

            observer.Descend();
            var state = Child.Tick(ref observer);
            observer.Ascend();
            return state;
        }

        protected override BHState OnEvaluate(ref RunnerObserver observer)
        {
            if (Condition != null && !Condition.Invoke())
                return BHState.Running;
            
            if(Child == null)
                return ReturnIfChildNull;

            observer.Descend();
            var state = Child.Evaluate(ref observer);
            observer.Ascend();
            return state;
        }

        protected override BHState OnExecute()
        {
            if (Condition != null && !Condition.Invoke())
                return BHState.Running;
            
            if(Child == null)
                return ReturnIfChildNull;

            return Child.Execute();
        }
    }
}

namespace BehaviorTree
{
    /// <summary>
    /// Entry point of the behavior tree. Must be the first node created
    /// via BehaviorTreeBuilder.Root().
    /// Điểm vào của cây hành vi. Phải là node đầu tiên được tạo
    /// qua BehaviorTreeBuilder.Root().
    ///
    /// Simply delegates all calls to its single child.
    /// Đơn giản ủy quyền tất cả gọi xuống con duy nhất của nó.
    /// </summary>
    public class RootNode : DecoratorNode
    {
        protected override BHState OnUpdate(ref RunnerObserver observer)
        {
            observer.Descend();
            var state = Child.Tick(ref observer);
            observer.Ascend();
            return state;
        }

        protected override BHState OnEvaluate(ref RunnerObserver observer)
        {
            observer.Descend();
            var state = Child.Evaluate(ref observer);
            observer.Ascend();
            return state;
        }

        protected override BHState OnExecute()
        {
            if(CurrentState != BHState.Running)
            {
                return CurrentState;
            }
            return Child.Execute();
        }
    }
}

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
        protected override BHState OnUpdate()
        {
            return Child.Tick();
        }

        protected override BHState OnEvaluate()
        {
            return Child.Evaluate();
        }

        protected override BHState OnExecute()
        {
            return Child.Execute();
        }
    }
}

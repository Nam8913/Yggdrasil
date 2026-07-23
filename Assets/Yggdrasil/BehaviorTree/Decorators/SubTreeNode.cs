namespace BehaviorTree
{
    /// <summary>
    /// Wraps a sub-tree as a single node, enabling tree composition and reuse.
    /// Bọc một cây con thành một node duy nhất, cho phép kết hợp và tái sử dụng cây.
    ///
    /// Usage: extract common behavior into a reusable sub-tree
    /// and reference it from multiple places in the main tree.
    /// Sử dụng: trích xuất hành vi chung thành cây con tái sử dụng
    /// và tham chiếu từ nhiều nơi trong cây chính.
    /// </summary>
    public class SubTreeNode : DecoratorNode
    {
        // Optional name for debugging/identification
        // Tùy chọn tên để debug/định danh
        public string SubTreeName { get; set; }

        public SubTreeNode(NodeBT subTreeRoot)
        {
            Child = subTreeRoot;
        }

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

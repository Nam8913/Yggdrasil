namespace BehaviorTree
{
    public class SubTreeNode : DecoratorNode
    {
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

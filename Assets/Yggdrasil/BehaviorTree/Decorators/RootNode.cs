namespace BehaviorTree
{
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

namespace BehaviorTree
{
    public abstract class ConditionNode : NodeBT
    {
        protected override BHState OnUpdate()
        {
            return Check() ? BHState.Success : BHState.Failure;
        }

        // Conditions are pure logic - always run on Evaluate phase
        protected override BHState OnEvaluate()
        {
            return Check() ? BHState.Success : BHState.Failure;
        }

        // Conditions don't need Execute phase
        protected override BHState OnExecute()
        {
            return EvaluatedState;
        }

        protected abstract bool Check();
    }
}

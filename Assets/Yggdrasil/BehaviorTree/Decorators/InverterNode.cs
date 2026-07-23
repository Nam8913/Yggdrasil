namespace BehaviorTree
{
    public class InverterNode : DecoratorNode
    {
        protected override BHState OnUpdate()
        {
            var state = Child.Tick();
            return Invert(state);
        }

        protected override BHState OnEvaluate()
        {
            var state = Child.Evaluate();
            return Invert(state);
        }

        protected override BHState OnExecute()
        {
            return Invert(Child.Execute());
        }

        private BHState Invert(BHState state)
        {
            return state switch
            {
                BHState.Success => BHState.Failure,
                BHState.Failure => BHState.Success,
                _ => BHState.Running
            };
        }
    }
}

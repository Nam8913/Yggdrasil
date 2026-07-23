namespace BehaviorTree
{
    public class RepeaterNode : DecoratorNode
    {
        public int MaxRepeats { get; set; } = -1;
        private int _count;

        protected override void OnEnter()
        {
            _count = 0;
        }

        protected override BHState OnUpdate()
        {
            var state = Child.Tick();

            if (state == BHState.Running)
                return BHState.Running;

            _count++;

            if (MaxRepeats > 0 && _count >= MaxRepeats)
                return BHState.Success;

            Child.Reset();
            return BHState.Running;
        }

        protected override BHState OnEvaluate()
        {
            var state = Child.Evaluate();

            if (state == BHState.Running)
                return BHState.Running;

            _count++;

            if (MaxRepeats > 0 && _count >= MaxRepeats)
                return BHState.Success;

            Child.Reset();
            return BHState.Running;
        }

        protected override BHState OnExecute()
        {
            return Child.Execute();
        }
    }
}

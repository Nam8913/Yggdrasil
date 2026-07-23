using System;

namespace BehaviorTree
{
    public class WaitUntilNode : DecoratorNode
    {
        public Func<bool> Condition { get; set; }

        public WaitUntilNode(Func<bool> condition)
        {
            Condition = condition;
        }

        protected override BHState OnUpdate()
        {
            if (Condition != null && !Condition.Invoke())
                return BHState.Running;

            return Child.Tick();
        }

        protected override BHState OnEvaluate()
        {
            if (Condition != null && !Condition.Invoke())
                return BHState.Running;

            return Child.Evaluate();
        }

        protected override BHState OnExecute()
        {
            if (Condition != null && !Condition.Invoke())
                return BHState.Running;

            return Child.Execute();
        }
    }
}

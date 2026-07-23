using System;

namespace BehaviorTree
{
    public class GuardNode : DecoratorNode
    {
        public Func<bool> Condition { get; set; }
        public int Priority { get; set; } = 0;

        public GuardNode(Func<bool> condition, NodeBT child, int priority = 0)
        {
            Condition = condition;
            Child = child;
            Priority = priority;
        }

        protected override BHState OnUpdate()
        {
            if (Child == null)
                return BHState.Failure;

            if (!Condition.Invoke())
            {
                if (IsRunning)
                {
                    Child.Abort();
                    return BHState.Failure;
                }
                return BHState.Failure;
            }

            return Child.Tick();
        }

        protected override BHState OnEvaluate()
        {
            if (Child == null)
                return BHState.Failure;

            if (!Condition.Invoke())
            {
                if (IsRunning)
                {
                    Child.Abort();
                    return BHState.Failure;
                }
                return BHState.Failure;
            }

            return Child.Evaluate();
        }

        protected override BHState OnExecute()
        {
            if (Child == null)
                return BHState.Failure;

            return Child.Execute();
        }
    }
}

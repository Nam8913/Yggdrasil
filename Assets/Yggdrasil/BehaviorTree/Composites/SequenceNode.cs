namespace BehaviorTree
{
    public class SequenceNode : CompositeNode
    {
        protected override BHState OnUpdate()
        {
            while (CurrentChildIndex < Children.Count)
            {
                var state = Children[CurrentChildIndex].Tick();

                if (state == BHState.Running)
                    return BHState.Running;

                if (state == BHState.Failure)
                {
                    CurrentChildIndex = 0;
                    return BHState.Failure;
                }

                CurrentChildIndex++;
            }

            CurrentChildIndex = 0;
            return BHState.Success;
        }

        // Phase 1: Evaluate logic (thread-safe)
        protected override BHState OnEvaluate()
        {
            while (CurrentChildIndex < Children.Count)
            {
                var state = Children[CurrentChildIndex].Evaluate();

                if (state == BHState.Running)
                    return BHState.Running;

                if (state == BHState.Failure)
                {
                    CurrentChildIndex = 0;
                    return BHState.Failure;
                }

                CurrentChildIndex++;
            }

            CurrentChildIndex = 0;
            return BHState.Success;
        }

        // Phase 2: Execute Unity API (main thread)
        protected override BHState OnExecute()
        {
            if (CurrentChildIndex < Children.Count)
            {
                return Children[CurrentChildIndex].Execute();
            }
            return BHState.Success;
        }
    }
}

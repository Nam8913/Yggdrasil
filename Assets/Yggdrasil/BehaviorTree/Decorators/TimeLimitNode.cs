using UnityEngine;

namespace BehaviorTree
{
    public class TimeLimitNode : DecoratorNode
    {
        public float LimitSeconds { get; set; } = 5f;
        private float _startTime;

        protected override void OnEnter()
        {
            _startTime = Time.time;
        }

        protected override BHState OnUpdate()
        {
            if (Time.time - _startTime >= LimitSeconds)
            {
                Child.Abort();
                return BHState.Failure;
            }

            return Child.Tick();
        }

        protected override BHState OnEvaluate()
        {
            if (Time.time - _startTime >= LimitSeconds)
            {
                Child.Abort();
                return BHState.Failure;
            }

            return Child.Evaluate();
        }

        protected override BHState OnExecute()
        {
            return Child.Execute();
        }
    }
}

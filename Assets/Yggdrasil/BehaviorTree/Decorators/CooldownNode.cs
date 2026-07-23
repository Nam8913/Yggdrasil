using UnityEngine;

namespace BehaviorTree
{
    public class CooldownNode : DecoratorNode
    {
        public float CooldownSeconds { get; set; } = 1f;
        private float _lastExecuteTime = -999f;

        protected override BHState OnUpdate()
        {
            if (Time.time - _lastExecuteTime < CooldownSeconds)
                return BHState.Failure;

            var state = Child.Tick();

            if (state != BHState.Running)
                _lastExecuteTime = Time.time;

            return state;
        }

        protected override BHState OnEvaluate()
        {
            if (Time.time - _lastExecuteTime < CooldownSeconds)
                return BHState.Failure;

            var state = Child.Evaluate();

            if (state != BHState.Running)
                _lastExecuteTime = Time.time;

            return state;
        }

        protected override BHState OnExecute()
        {
            return Child.Execute();
        }

        protected override void OnReset()
        {
            base.OnReset();
            _lastExecuteTime = -999f;
        }
    }
}

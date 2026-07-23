using UnityEngine;

namespace BehaviorTree
{
    public class WaitNode : DecoratorNode
    {
        public float WaitSeconds { get; set; } = 1f;
        private float _startTime;
        private bool _waiting;

        protected override void OnEnter()
        {
            _startTime = Time.time;
            _waiting = true;
        }

        protected override BHState OnUpdate()
        {
            if (_waiting)
            {
                if (Time.time - _startTime < WaitSeconds)
                    return BHState.Running;

                _waiting = false;
            }

            return Child.Tick();
        }

        protected override BHState OnEvaluate()
        {
            if (_waiting)
            {
                if (Time.time - _startTime < WaitSeconds)
                    return BHState.Running;

                _waiting = false;
            }

            return Child.Evaluate();
        }

        protected override BHState OnExecute()
        {
            if (_waiting)
                return BHState.Running;

            return Child.Execute();
        }

        protected override void OnReset()
        {
            base.OnReset();
            _waiting = false;
        }
    }
}

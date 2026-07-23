using UnityEngine;

namespace BehaviorTree
{
    public abstract class ServiceNode : DecoratorNode
    {
        public float ServiceInterval { get; set; } = 0.5f;
        private float _lastServiceTime;

        protected override void OnEnter()
        {
            _lastServiceTime = Time.time;
            ExecuteService();
        }

        protected override BHState OnUpdate()
        {
            if (Time.time - _lastServiceTime >= ServiceInterval)
            {
                ExecuteService();
                _lastServiceTime = Time.time;
            }

            return Child.Tick();
        }

        protected override BHState OnEvaluate()
        {
            if (Time.time - _lastServiceTime >= ServiceInterval)
            {
                ExecuteService();
                _lastServiceTime = Time.time;
            }

            return Child.Evaluate();
        }

        protected override BHState OnExecute()
        {
            return Child.Execute();
        }

        protected abstract void ExecuteService();
    }
}

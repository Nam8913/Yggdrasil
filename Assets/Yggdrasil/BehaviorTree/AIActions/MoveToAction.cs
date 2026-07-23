using UnityEngine;

namespace BehaviorTree
{
    public class MoveToAction : ActionNode
    {
        public float Speed { get; set; } = 3f;
        public float ArrivalThreshold { get; set; } = 0.5f;

        private Rigidbody2D _rb;
        private Vector3 _targetPosition;

        protected override void OnInitialize()
        {
            _rb = Blackboard.Get(BBKeys.Self).GetComponent<Rigidbody2D>();
        }

        protected override void OnEnter()
        {
            if (Blackboard.Has(BBKeys.MoveTarget))
                _targetPosition = Blackboard.Get(BBKeys.MoveTarget);
        }

        protected override BHState OnUpdate()
        {
            return Move();
        }

        protected override BHState OnExecute()
        {
            return Move();
        }

        private BHState Move()
        {
            if (_rb == null)
                return BHState.Failure;

            Vector2 currentPos = _rb.position;
            Vector2 target = (Vector2)_targetPosition;
            Vector2 direction = target - currentPos;
            float distance = direction.magnitude;

            if (distance <= ArrivalThreshold)
            {
                _rb.linearVelocity = Vector2.zero;
                return BHState.Success;
            }

            _rb.linearVelocity = direction.normalized * Speed;
            return BHState.Running;
        }

        protected override void OnExit()
        {
            if (_rb != null)
                _rb.linearVelocity = Vector2.zero;
        }
    }
}

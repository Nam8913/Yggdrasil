using UnityEngine;

namespace BehaviorTree
{
    public class RotateToAction : ActionNode
    {
        public float TurnSpeed { get; set; } = 360f;
        public float AngleThreshold { get; set; } = 5f;

        private Vector3 _targetPosition;

        protected override void OnEnter()
        {
            if (Blackboard.Has(BBKeys.LookTarget))
                _targetPosition = Blackboard.Get(BBKeys.LookTarget);
            else if (Blackboard.Has(BBKeys.MoveTarget))
                _targetPosition = Blackboard.Get(BBKeys.MoveTarget);
        }

        protected override BHState OnUpdate()
        {
            return Rotate();
        }

        protected override BHState OnExecute()
        {
            return Rotate();
        }

        private BHState Rotate()
        {
            var self = Blackboard.Get(BBKeys.Self);
            if (self == null)
                return BHState.Failure;

            Vector2 direction = (Vector2)(_targetPosition - self.transform.position);
            if (direction.sqrMagnitude < 0.001f)
                return BHState.Success;

            float targetAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            float currentAngle = self.transform.eulerAngles.z;
            float angle = Mathf.DeltaAngle(currentAngle, targetAngle);

            if (Mathf.Abs(angle) <= AngleThreshold)
                return BHState.Success;

            float step = TurnSpeed * Time.deltaTime;
            float newAngle = Mathf.MoveTowards(currentAngle, currentAngle + angle, step);
            self.transform.rotation = Quaternion.Euler(0f, 0f, newAngle);

            return BHState.Running;
        }
    }
}

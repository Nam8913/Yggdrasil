using UnityEngine;

namespace BehaviorTree
{
    /// <summary>
    /// Rotates the NPC to face a target position (2D rotation around Z-axis).
    /// Quay NPC để nhìn về phía vị trí mục tiêu (quay 2D quanh trục Z).
    ///
    /// Reads target from LookTarget, falls back to MoveTarget if not set.
    /// Đọc mục tiêu từ LookTarget, nếu không có thì dùng MoveTarget.
    /// </summary>
    public class RotateToAction : ActionNode
    {
        // Rotation speed in degrees per second
        // Tốc độ quay (độ/giây)
        public float TurnSpeed { get; set; } = 360f;

        // Angle threshold to consider "facing" the target
        // Ngưỡng góc để coi là "đang nhìn" mục tiêu
        public float AngleThreshold { get; set; } = 5f;

        private Vector3 _targetPosition;
        private bool failedToGetTarget = false;

        protected override void OnEnter()
        {
            // Prefer LookTarget, fall back to MoveTarget
            // Ưu tiên LookTarget, nếu không có thì dùng MoveTarget
            if (Blackboard.Has(BBKeys.LookTarget))
                _targetPosition = Blackboard.Get(BBKeys.LookTarget);
            else if (Blackboard.Has(BBKeys.MoveTarget))
                _targetPosition = Blackboard.Get(BBKeys.MoveTarget);
            else if (Blackboard.Has(BBKeys.ThreatTarget))
                _targetPosition = Blackboard.Get(BBKeys.ThreatTarget).position;
            else
               failedToGetTarget = true;
        }

        protected override BHState OnUpdate(ref RunnerObserver observer)
        {
            return Rotate();
        }

        protected override BHState OnEvaluate(ref RunnerObserver observer)
        {
            var self = Blackboard.Get(BBKeys.Self);
            if (self == null)
            {   
                UnityEngine.Debug.LogWarning($"[{self?.name ?? "Unknown"}] RotateToAction: No target to rotate to.");
                return BHState.Failure;
            }

            //lỗi tiềm năng: lỗi WanderAction không set MoveTarget khi Evaluate. Hiện tại nên tạm thời trả về Running
            if(failedToGetTarget)
            {
                return BHState.Running;
            }

            Vector2 direction = (Vector2)(_targetPosition - self.transform.position);
            if (direction.sqrMagnitude < 0.001f)
                return BHState.Success; 

            float targetAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            float currentAngle = self.transform.eulerAngles.z;
            float angle = Mathf.DeltaAngle(currentAngle, targetAngle);

            if (Mathf.Abs(angle) <= AngleThreshold)
                return BHState.Success;
            return BHState.Running; // Always running, no condition to stop
        }

        protected override BHState OnExecute()
        {
            if(CurrentState != BHState.Running)
            {
                return CurrentState;
            }

            var self = Blackboard.Get(BBKeys.Self);
            if (self == null)
            {   
                UnityEngine.Debug.LogWarning($"[{self?.name ?? "Unknown"}] RotateToAction: No target to rotate to.");
                return BHState.Failure;
            }
            // TODO: hiện tại có trường hợp BTScheduler gọi OnExecute trước OnEnter, dẫn đến _targetPosition chưa được set. Cần fix lại.
            // Dùng tạm fallback để lấy target từ Blackboard nếu OnEnter chưa được gọi.
            if(failedToGetTarget)
            {
                if (Blackboard.Has(BBKeys.LookTarget))
                _targetPosition = Blackboard.Get(BBKeys.LookTarget);
                else if (Blackboard.Has(BBKeys.MoveTarget))
                _targetPosition = Blackboard.Get(BBKeys.MoveTarget);
                else if (Blackboard.Has(BBKeys.ThreatTarget))
                _targetPosition = Blackboard.Get(BBKeys.ThreatTarget).position;
                else
                {
                    return BHState.Failure;
                }
            }

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

        private BHState Rotate()
        {
            var self = Blackboard.Get(BBKeys.Self);
            if (self == null || failedToGetTarget)
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

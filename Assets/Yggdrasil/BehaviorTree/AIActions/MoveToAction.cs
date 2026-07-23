using UnityEngine;

namespace BehaviorTree
{
    /// <summary>
    /// Moves the NPC toward a target position read from the Blackboard.
    /// Di chuyển NPC đến vị trí mục tiêu đọc từ Blackboard.
    ///
    /// Uses Rigidbody2D for physics-based movement.
    /// Returns Success when within ArrivalThreshold of the target.
    /// Dùng Rigidbody2D cho di chuyển dựa trên vật lý.
    /// Trả về Success khi trong khoảng ArrivalThreshold so với mục tiêu.
    /// </summary>
    public class MoveToAction : ActionNode
    {
        // Movement speed in units per second
        // Tốc độ di chuyển (đơn vị/giây)
        public float Speed { get; set; } = 3f;

        // Distance to consider "arrived" at the target
        // Khoảng cách để coi là "đã đến" mục tiêu
        public float ArrivalThreshold { get; set; } = 0.5f;

        private Rigidbody2D _rb;
        private Vector3 _targetPosition;

        protected override void OnInitialize()
        {
            _rb = Blackboard.Get(BBKeys.Self).GetComponent<Rigidbody2D>();
        }

        protected override void OnEnter()
        {
            // Read target position from blackboard each time this node starts
            // Đọc vị trí mục tiêu từ blackboard mỗi khi node này bắt đầu
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

        // Stop movement when exiting this node
        // Dừng di chuyển khi thoát khỏi node này
        protected override void OnExit()
        {
            if (_rb != null)
                _rb.linearVelocity = Vector2.zero;
        }
    }
}

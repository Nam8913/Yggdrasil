using System.Collections.Generic;
using UnityEngine;

namespace BehaviorTree
{
    /// <summary>
    /// Makes the NPC wander randomly within a radius around its current position.
    /// Di chuyển NPC lang thang ngẫu nhiên trong bán kính xung quanh vị trí hiện tại.
    ///
    /// Picks a random point within WanderRadius and moves toward it.
    /// When reached, picks a new random point (returns Success, tree can re-enter).
    /// Chọn một điểm ngẫu nhiên trong WanderRadius và di chuyển đến đó.
    /// Khi đến nơi, chọn điểm ngẫu nhiên mới (trả về Success, cây có thể tái kích hoạt).
    /// </summary>
    public class WanderAction : ActionNode
    {
        // Maximum distance from current position for random target
        // Khoảng cách tối đa từ vị trí hiện tại đến mục tiêu ngẫu nhiên
        public float WanderRadius { get; set; } = 10f;

        // Movement speed in units per second
        // Tốc độ di chuyển (đơn vị/giây)
        public float MoveSpeed { get; set; } = 1f;

        // Distance threshold to consider a waypoint reached
        // Ngưỡng khoảng cách để coi một điểm đã đến
        public float WaypointReachDistance { get; set; } = 0.15f;

        private List<Vector2> _path;
        private int _pathIndex;
        private Vector2 _targetPosition;
        private Transform _transform;

        public WanderAction(Transform transform)
        {
            _transform = transform;
        }

        protected override BHState OnUpdate(ref RunnerObserver observer)
        {
            OnEvaluate(ref observer);
            return OnExecute();
        }

        // Phase 1: Decide where to go (pure logic)
        // Giai đoạn 1: Quyết định đi đâu (logic thuần túy)
        protected override BHState OnEvaluate(ref RunnerObserver observer)
        {
            if (_transform == null)
                return BHState.Failure;

            // If we have an active path, keep running
            // Nếu đang có đường đi, tiếp tục chạy
            if (_path != null && _path.Count > 0 && _pathIndex < _path.Count)
                return BHState.Running;

            // Acquire new random target
            // Lấy mục tiêu ngẫu nhiên mới
            Vector2 origin = _transform.position;
            _targetPosition = origin + Random.insideUnitCircle * WanderRadius;
            _path = new List<Vector2> { _targetPosition };
            _pathIndex = 0;

            return BHState.Running;
        }

        // Phase 2: Actually move (Unity API, main thread)
        // Giai đoạn 2: Di chuyển thực tế (Unity API, main thread)
        protected override BHState OnExecute()
        {
            if (_path == null || _path.Count == 0 || _transform == null)
            {
                return BHState.Failure;
            }
            if (_pathIndex >= _path.Count)
            {
                _path = null;
                return BHState.Success;
            }

            Vector2 currentTarget = _path[_pathIndex];
            Blackboard.Set(BBKeys.MoveTarget, currentTarget);
            _transform.position = Vector2.MoveTowards(
                _transform.position,
                currentTarget,
                MoveSpeed * Time.deltaTime
            );

            if (Vector2.Distance(_transform.position, currentTarget) <= WaypointReachDistance)
            {
                _pathIndex++;
                if (_pathIndex >= _path.Count)
                {
                    return BHState.Success;
                }
            }

            return BHState.Running;
        }

        protected override void OnReset()
        {
            base.OnReset();
            _path = null;
            _pathIndex = 0;
        }
    }
}

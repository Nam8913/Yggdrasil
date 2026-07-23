using System.Collections.Generic;
using UnityEngine;

namespace BehaviorTree
{
    public class WanderAction : ActionNode
    {
        public float WanderRadius { get; set; } = 10f;
        public float MoveSpeed { get; set; } = 2f;
        public float WaypointReachDistance { get; set; } = 0.15f;

        private List<Vector2> _path;
        private int _pathIndex;
        private Vector2 _targetPosition;
        private Transform _transform;

        public WanderAction(Transform transform)
        {
            _transform = transform;
        }

        // Legacy: full tick on main thread (backward compatible)
        protected override BHState OnUpdate()
        {
            OnEvaluate();
            return OnExecute();
        }

        // Phase 1: Pure logic (thread-safe) - decide where to go
        protected override BHState OnEvaluate()
        {
            if (_transform == null)
                return BHState.Failure;

            // If we have an active path, keep running
            if (_path != null && _path.Count > 0 && _pathIndex < _path.Count)
                return BHState.Running;

            // Acquire new random target
            Vector2 origin = _transform.position;
            _targetPosition = origin + Random.insideUnitCircle * WanderRadius;
            _path = new List<Vector2> { _targetPosition };
            _pathIndex = 0;

            return BHState.Running;
        }

        // Phase 2: Unity API (main thread) - actually move
        protected override BHState OnExecute()
        {
            if (_path == null || _path.Count == 0 || _transform == null)
                return BHState.Failure;

            if (_pathIndex >= _path.Count)
            {
                _path = null;
                return BHState.Success;
            }

            Vector2 currentTarget = _path[_pathIndex];
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
                    _path = null;
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

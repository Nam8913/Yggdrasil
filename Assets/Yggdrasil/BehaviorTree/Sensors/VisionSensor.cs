using System.Collections.Generic;
using UnityEngine;

namespace BehaviorTree
{
    public class VisionSensor : SensorBase
    {
        [SerializeField] private float _sightRange = 20f;
        public float SightRange => _sightRange;
        [SerializeField] private float _sightAngle = 90f;
        [SerializeField] private LayerMask _targetLayers = ~0;
        [SerializeField] private LayerMask _obstacleLayers;

        [SerializeField] private List<GameObject> lastDetectedTargets = new List<GameObject>();
        [SerializeField] private GameObject closestTarget;

        [Header("Debug")]
        [SerializeField]
        private bool drawGizmosDebug = true;
        [SerializeField]
        private bool drawVisionSphere = true;
        [SerializeField]
        private bool drawvisionCone = true;
        [SerializeField]
        private bool drawVisionRays = true;
        [SerializeField]
        private bool drawLineToTarget = true;

        protected override void Sense()
        {
            var colliders = Physics2D.OverlapCircleAll(transform.position, _sightRange, _targetLayers);
            lastDetectedTargets.Clear();

            Transform closestThreat = null;
            float closestDist = float.MaxValue;

            for (int i = 0; i < colliders.Length; i++)
            {
                if (colliders[i].gameObject == gameObject)
                    continue;

                var targetPos = (Vector2)colliders[i].transform.position;
                var dir = targetPos - (Vector2)transform.position;
                float distance = dir.magnitude;

                // Kiểm tra góc nhìn (dùng transform.right)
                float angle = Vector2.Angle(transform.right, dir);
                if (angle > _sightAngle * 0.5f)
                    continue;

                // Kiểm tra obstacle chắn đường
                var hit = Physics2D.Raycast(transform.position, dir.normalized, distance, _obstacleLayers);
                if (hit.collider != null && hit.collider.gameObject != colliders[i].gameObject)
                    continue;

                var creature = colliders[i].GetComponent<Transform>();
                if (creature == null)
                    continue;

                if (creature.gameObject.CompareTag("ZOMBIE"))
                    continue;

                lastDetectedTargets.Add(colliders[i].gameObject);

                float distSqr = dir.sqrMagnitude;
                if (distSqr < closestDist)
                {
                    closestDist = distSqr;
                    closestThreat = creature;
                    closestTarget = creature.gameObject;
                }
            }

            if (closestThreat != null)
            {
                Blackboard.Set(BBKeys.CanSeeEnemy, true);
                Blackboard.Set(BBKeys.ThreatTarget, closestThreat);
                Blackboard.Set(BBKeys.LastKnownEnemyPosition, closestThreat.transform.position);
            }
            else
            {
                Blackboard.Set(BBKeys.CanSeeEnemy, false);
            }
        }

        private void OnDrawGizmosSelected()
        {
            if(!drawGizmosDebug)
                return;


            Vector2 forward = transform.right;
            
            // Vẽ tầm nhìn
            if(drawVisionSphere)
            {
                Gizmos.color = new Color(1f, 1f, 0f, 0.05f);
                Gizmos.DrawWireSphere(transform.position, _sightRange);
            }
            
            float halfAngle = _sightAngle * 0.5f;
            // Vẽ hình nón tầm nhìn
            if (drawvisionCone)
            {
                int segments = 20;
                Gizmos.color = Color.black;
                for (int i = 0; i <= segments; i++)
                {
                    float t = (float)i / segments;
                    float currentAngle = Mathf.Lerp(-halfAngle, halfAngle, t);
                    Vector2 dir = Quaternion.Euler(0, 0, currentAngle) * forward;
                    Vector2 point = (Vector2)transform.position + dir * _sightRange;

                    Gizmos.DrawLine(transform.position, point);
                }
            }
           

            // Vẽ đường viền hai bên
            if(drawvisionCone)
            {
                Vector2 leftDir = Quaternion.Euler(0, 0, -halfAngle) * forward;
                Vector2 rightDir = Quaternion.Euler(0, 0, halfAngle) * forward;

                Gizmos.color = Color.yellow;
                Gizmos.DrawLine(transform.position, (Vector2)transform.position + leftDir * _sightRange);
                Gizmos.DrawLine(transform.position, (Vector2)transform.position + rightDir * _sightRange);

            }
            
            // Vẽ hướng nhìn
            if(drawVisionRays)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawLine(transform.position, (Vector2)transform.position + forward * _sightRange * 0.2f);
            }
            

            // Vẽ line đến target
            if (closestTarget != null && drawLineToTarget)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawLine(transform.position, closestTarget.transform.position);
            }
        }
    }
}

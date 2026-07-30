using UnityEngine;

namespace BehaviorTree.Debug
{
    public class BTGizmos : MonoBehaviour
    {
        [Header("Gizmo Settings")]
        [SerializeField] private bool _showMoveTarget = true;
        [SerializeField] private bool _showSensorRanges = true;
        [SerializeField] private bool _showStateColor = true;

        [Header("Colors")]
        [SerializeField] private Color _idleColor = Color.green;
        [SerializeField] private Color _taskColor = Color.yellow;
        [SerializeField] private Color _combatColor = Color.red;

        private BehaviorTreeRunner _runner;

        private void Awake()
        {
            _runner = this.gameObject.GetComponent<BehaviorTreeRunner>();
        }

        private void OnDrawGizmosSelected()
        {
            if (_runner == null || !_runner.IsInitialized)
                return;

            var bb = _runner.Blackboard;
            if (bb == null)
                return;

            if (_showStateColor)
            {
                DrawStateIndicator(bb);
            }

            if (_showMoveTarget)
            {
                DrawMoveTarget(bb);
            }

            if (_showSensorRanges)
            {
                DrawSensorRanges();
            }
        }

        private void DrawStateIndicator(Blackboard bb)
        {
            if (bb.Has(BBKeys.InCombat))
                Gizmos.color = _combatColor;
            else if (bb.Has(BBKeys.ThreatTarget))
                Gizmos.color = _combatColor;
            else
                Gizmos.color = _idleColor;

            Gizmos.DrawWireSphere(transform.position, 0.3f);
        }

        private void DrawMoveTarget(Blackboard bb)
        {
            if (bb.TryGet(BBKeys.MoveTarget, out Vector3 target))
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawLine(transform.position, target);
                Gizmos.DrawWireSphere(target, 0.2f);
            }

            if (bb.TryGet(BBKeys.ThreatTarget, out Transform threat) && threat != null)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawLine(transform.position, threat.transform.position);
                Gizmos.DrawWireSphere(threat.transform.position, 0.3f);
            }
        }

        private void DrawSensorRanges()
        {
            var sensors = GetComponents<SensorBase>();
            foreach (var sensor in sensors)
            {
                if (sensor is VisionSensor vision)
                {
                    Gizmos.color = new Color(1f, 1f, 0f, 0.2f);
                    Gizmos.DrawWireSphere(transform.position, vision.SightRange);
                }
                else if (sensor is HearingSensor hearing)
                {
                    Gizmos.color = new Color(1f, 0.5f, 0f, 0.2f);
                    Gizmos.DrawWireSphere(transform.position, hearing.HearingRange);
                }
            }
        }
    }
}

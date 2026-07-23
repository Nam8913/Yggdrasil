using UnityEngine;

namespace BehaviorTree
{
    public class DamageSensor : SensorBase
    {
        private Transform _lastAttacker;
        private bool _tookDamage;
        private bool _damageProcessed = true;

        public void ReportDamage(Transform attacker)
        {
            _lastAttacker = attacker;
            _tookDamage = true;
            _damageProcessed = false;
        }

        protected override void Sense()
        {
            if (_tookDamage && !_damageProcessed && _lastAttacker != null)
            {
                Blackboard.Set(BBKeys.ThreatTarget, _lastAttacker);
                Blackboard.Set(BBKeys.InCombat, true);
                _damageProcessed = true;
            }
        }
    }
}

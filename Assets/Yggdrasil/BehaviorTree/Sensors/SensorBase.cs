using UnityEngine;

namespace BehaviorTree
{
    public abstract class SensorBase : MonoBehaviour
    {
        [SerializeField] protected float _updateInterval = 0.2f;
        protected Blackboard Blackboard;
        private float _lastUpdateTime;

        public void Initialize(Blackboard blackboard)
        {
            Blackboard = blackboard;
        }

        private void Update()
        {
            if (Blackboard == null)
                return;

            if (Time.time - _lastUpdateTime < _updateInterval)
                return;

            _lastUpdateTime = Time.time;
            Sense();
        }

        protected abstract void Sense();
    }
}

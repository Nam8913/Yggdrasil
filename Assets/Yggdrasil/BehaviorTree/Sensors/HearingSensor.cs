using UnityEngine;

namespace BehaviorTree
{
    public class HearingSensor : SensorBase
    {
        [SerializeField] private float _hearingRange = 15f;
        public float HearingRange => _hearingRange;

        private Vector3 _lastSoundPosition;
        private bool _heardSound;
        private bool _soundProcessed = true;

        public void ReportSound(Vector3 position, float volume = 1f)
        {
            float dist = Vector3.Distance(transform.position, position);
            if (dist <= _hearingRange * volume)
            {
                _lastSoundPosition = position;
                _heardSound = true;
                _soundProcessed = false;
            }
        }

        protected override void Sense()
        {
            // Only update blackboard if there's a new sound or previous was processed
            if (_heardSound && !_soundProcessed)
            {
                Blackboard.Set(BBKeys.HeardNoise, true);
                _soundProcessed = true;
            }
            else if (_soundProcessed)
            {
                Blackboard.Set(BBKeys.HeardNoise, false);
                _heardSound = false;
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(1f, 0.5f, 0f, 0.3f);
            Gizmos.DrawWireSphere(transform.position, _hearingRange);
        }
    }
}

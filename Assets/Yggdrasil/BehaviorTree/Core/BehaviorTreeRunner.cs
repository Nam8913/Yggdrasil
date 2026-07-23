using UnityEngine;

namespace BehaviorTree
{
    public class BehaviorTreeRunner : MonoBehaviour
    {
        [SerializeField] private float _tickInterval = 0f;

        public RootNode Root { get; private set; }
        public Blackboard Blackboard { get; private set; }
        public BHState CurrentState { get; private set; } = BHState.Failure;
        public BHState LastEvaluatedState { get; private set; } = BHState.Failure;
        public bool IsInitialized { get; private set; }

        private float _timeSinceLastTick;
        private bool _needsReset;

        public void Initialize(RootNode root, Blackboard blackboard = null)
        {
            Root = root;
            Blackboard = blackboard ?? new Blackboard();

            Root.Initialize(Blackboard);
            CurrentState = BHState.Running;
            LastEvaluatedState = BHState.Running;
            _timeSinceLastTick = 0f;
            _needsReset = false;
            IsInitialized = true;

            Blackboard.Set(BBKeys.Self, this.gameObject);
        }

        private void Update()
        {
            if (!IsInitialized || Root == null)
                return;

            // Delayed reset: reset one frame after completion so state is observable
            if (_needsReset)
            {
                Root.Reset();
                CurrentState = BHState.Running;
                LastEvaluatedState = BHState.Running;
                _needsReset = false;
            }

            if (_tickInterval <= 0f)
            {
                TickTree();
                return;
            }

            _timeSinceLastTick += Time.deltaTime;
            if (_timeSinceLastTick < _tickInterval)
                return;

            _timeSinceLastTick = 0f;
            TickTree();
        }

        private void TickTree()
        {
            CurrentState = Root.Tick();

            if (CurrentState != BHState.Running)
            {
                _needsReset = true; // Reset next frame, not immediately
            }
        }

        // Legacy: full tick on main thread
        public void TickManually()
        {
            if (!IsInitialized || Root == null)
                return;

            if (_needsReset)
            {
                Root.Reset();
                CurrentState = BHState.Running;
                LastEvaluatedState = BHState.Running;
                _needsReset = false;
            }

            CurrentState = Root.Tick();

            if (CurrentState != BHState.Running)
            {
                _needsReset = true;
            }
        }

        // Phase 1: Evaluate logic (can run on worker thread)
        public void Evaluate()
        {
            if (!IsInitialized || Root == null)
                return;

            LastEvaluatedState = Root.Evaluate();
            CurrentState = LastEvaluatedState;

            if (CurrentState != BHState.Running)
            {
                _needsReset = true;
            }
        }

        // Phase 2: Execute Unity API (main thread only)
        // Runs every frame, uses EvaluatedState from last Evaluate
        public void Execute()
        {
            if (!IsInitialized || Root == null)
                return;

            if (_needsReset)
            {
                Root.Reset();
                CurrentState = BHState.Running;
                LastEvaluatedState = BHState.Running;
                _needsReset = false;
                return;
            }

            if (LastEvaluatedState != BHState.Running)
                return; // Nothing to execute

            var result = Root.Execute();
            CurrentState = result;

            if (result != BHState.Running)
            {
                _needsReset = true;
            }
        }

        public void ResetTree()
        {
            Root?.Reset();
            CurrentState = BHState.Running;
            LastEvaluatedState = BHState.Running;
            _timeSinceLastTick = 0f;
            _needsReset = false;
        }

        private void OnDestroy()
        {
            Root?.Abort();
        }

        public void SetTickInterval(float interval)
        {
            _tickInterval = interval;
        }
    }
}

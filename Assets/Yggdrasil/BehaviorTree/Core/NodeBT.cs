using System.Diagnostics;
using UnityEngine;
using BehaviorTree.Debug;

namespace BehaviorTree
{
    public abstract class NodeBT : IBehaviorNode
    {
        public BHState CurrentState { get; private set; } = BHState.Failure;
        public bool IsRunning { get; private set; }
        public NodeBT Parent { get; internal set; }

        // Profiling counters
        public long TotalTicks { get; private set; }
        public float LastTickDurationMs { get; private set; }

        // Two-phase state
        protected BHState EvaluatedState { get; private set; } = BHState.Failure;

        protected Blackboard Blackboard { get; private set; }

        public void Initialize(Blackboard blackboard)
        {
            Blackboard = blackboard;
            OnInitialize();
        }

        protected virtual void OnInitialize() { }
        protected virtual void OnEnter() { }
        protected virtual void OnExit() { }

        // Legacy: full tick on main thread (backward compatible)
        protected abstract BHState OnUpdate();

        // Phase 1: Logic evaluation
        // NOTE: Currently runs on main thread via BTScheduler.
        // OnEnter/OnExit may use Unity APIs safely in current implementation.
        // If moving to worker thread in future, audit all OnEnter/OnExit overrides.
        protected virtual BHState OnEvaluate()
        {
            return OnUpdate();
        }

        // Phase 2: Unity API execution (main thread only)
        // Override this for nodes that need Unity API (movement, physics, animation)
        // Default: returns EvaluatedState (no-op for pure logic nodes)
        protected virtual BHState OnExecute()
        {
            return EvaluatedState;
        }

        // Legacy: full tick on main thread
        public BHState Tick()
        {
            var sw = Stopwatch.StartNew();
            var previousState = CurrentState;

            if (!IsRunning)
            {
                OnEnter();
                IsRunning = true;
            }

            CurrentState = OnUpdate();

            if (CurrentState != BHState.Running)
            {
                OnExit();
                IsRunning = false;
            }

            sw.Stop();
            TotalTicks++;
            LastTickDurationMs = (float)sw.Elapsed.TotalMilliseconds;

            LogStateChange(previousState, CurrentState);

            return CurrentState;
        }

        // Phase 1: Evaluate (can run on worker thread)
        public BHState Evaluate()
        {
            var sw = Stopwatch.StartNew();
            var previousState = CurrentState;

            if (!IsRunning)
            {
                OnEnter();
                IsRunning = true;
            }

            EvaluatedState = OnEvaluate();
            CurrentState = EvaluatedState;

            if (CurrentState != BHState.Running)
            {
                OnExit();
                IsRunning = false;
            }

            sw.Stop();
            TotalTicks++;
            LastTickDurationMs = (float)sw.Elapsed.TotalMilliseconds;

            LogStateChange(previousState, CurrentState);

            return CurrentState;
        }

        // Phase 2: Execute (main thread only)
        // Called every frame, uses cached EvaluatedState
        public BHState Execute()
        {
            if (EvaluatedState != BHState.Running)
                return EvaluatedState;

            var result = OnExecute();
            if (result != BHState.Running)
            {
                CurrentState = result;
                if (IsRunning)
                {
                    OnExit();
                    IsRunning = false;
                }
            }
            return result;
        }

        public void Reset()
        {
            if (IsRunning)
            {
                OnExit();
            }
            IsRunning = false;
            CurrentState = BHState.Failure;
            EvaluatedState = BHState.Failure;
            OnReset();
        }

        protected virtual void OnReset() { }

        public virtual void Abort()
        {
            if (IsRunning)
            {
                OnExit();
                IsRunning = false;
                CurrentState = BHState.Failure;
                EvaluatedState = BHState.Failure;
            }
        }

        private void LogStateChange(BHState previousState, BHState newState)
        {
            if (previousState != newState && BTLogger.Instance != null)
            {
                string npcName = Parent != null ? "NPC" : "Root";
                BTLogger.Instance.Log(npcName, GetType().Name, previousState, newState, LastTickDurationMs);
            }

            if (BTStats.Instance != null)
            {
                string statsName = Parent != null ? "NPC" : "Root";
                BTStats.Instance.RecordTick(statsName, LastTickDurationMs);
            }
        }
    }
}

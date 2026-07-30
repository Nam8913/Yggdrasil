using System.Diagnostics;
using UnityEngine;
using BehaviorTree.Debug;

namespace BehaviorTree
{
    /// <summary>
    /// Abstract base class for all behavior tree nodes.
    /// Lớp cơ sở trừu tượng cho tất cả các node trong cây hành vi.
    ///
    /// Implements two-phase evaluation:
    /// - Phase 1 (Evaluate): Pure logic, can run on worker thread in future.
    /// - Phase 2 (Execute): Unity API calls, main thread only.
    /// Triển khai đánh giá hai giai đoạn:
    /// - Giai đoạn 1 (Evaluate): Logic thuần túy, trong tương lai có thể chạy trên worker thread.
    /// - Giai đoạn 2 (Execute): Gọi Unity API, chỉ chạy trên main thread.
    /// </summary>
    public abstract class NodeBT : IBehaviorNode
    {
        // Current state of this node (Running, Success, Failure)
        // Trạng thái hiện tại của node (Running, Success, Failure)
        public BHState CurrentState { get; private set; } = BHState.Failure;

        // Whether this node is currently executing (between OnEnter and OnExit)
        // Node có đang trong quá trình thực thi hay không (giữa OnEnter và OnExit)
        public bool IsRunning { get; private set; }

        // Parent node in the tree hierarchy
        // Node cha trong cấu trúc cây
        public NodeBT Parent { get; internal set; }

        // Profiling: total number of times this node has been ticked
        // Profiling: tổng số lần node này được tick
        public long TotalTicks { get; private set; }

        // Profiling: duration of the last tick in milliseconds
        // Profiling: thời gian thực thi của lần tick cuối cùng (ms)
        public float LastTickDurationMs { get; private set; }

        // Cached result from Evaluate(), used by Execute() phase
        // Kết quả được cache từ Evaluate(), dùng bởi giai đoạn Execute()
        protected BHState EvaluatedState { get; private set; } = BHState.Failure;

        // Shared blackboard for data communication between nodes
        // Blackboard dùng chung để trao đổi dữ liệu giữa các node
        protected Blackboard Blackboard { get; private set; }

        public void Initialize(Blackboard blackboard)
        {
            Blackboard = blackboard;
            OnInitialize();
        }

        protected virtual void OnInitialize() { }
        public virtual void OnEnterTree() { }
        public virtual void OnExitTree() { }
        protected virtual void OnEnter() { }
        protected virtual void OnExit() { }

        // Legacy: full tick on main thread (backward compatible) — all subclasses must implement with observer
        protected abstract BHState OnUpdate(ref RunnerObserver observer);

        // Phase 1: Logic evaluation
        // NOTE: Currently runs on main thread via BTScheduler.
        // OnEnter/OnExit may use Unity APIs safely in current implementation.
        // If moving to worker thread in future, audit all OnEnter/OnExit overrides.
        protected virtual BHState OnEvaluate(ref RunnerObserver observer)
        {
            return OnUpdate(ref observer);
        }

        // Phase 2: Unity API execution (main thread only)
        // Override this for nodes that need Unity API (movement, physics, animation)
        // Default: returns EvaluatedState (no-op for pure logic nodes)
        protected virtual BHState OnExecute()
        {
            return EvaluatedState;
        }

        // Full tick with observer
        public BHState Tick(ref RunnerObserver observer)
        {
            var sw = Stopwatch.StartNew();
            var previousState = CurrentState;

            observer.EnterNode(this);

            if (!IsRunning)
            {
                OnEnter();
                IsRunning = true;
            }

            CurrentState = OnUpdate(ref observer);

            if (CurrentState != BHState.Running)
            {
                OnExit();
                IsRunning = false;
            }
 
            sw.Stop();
            TotalTicks++;
            LastTickDurationMs = (float)sw.Elapsed.TotalMilliseconds;

            observer.ExitNode(this, CurrentState, LastTickDurationMs);
            LogStateChange(previousState, CurrentState);

            return CurrentState;
        }

        // Phase 1: Evaluate with observer
        public BHState Evaluate(ref RunnerObserver observer)
        {
            var sw = Stopwatch.StartNew();
            var previousState = CurrentState;

            observer.EnterNode(this);

            if (!IsRunning)
            {
                OnEnter();
                IsRunning = true;
            }

            EvaluatedState = OnEvaluate(ref observer);
            CurrentState = EvaluatedState;

            if (CurrentState != BHState.Running)
            {
                OnExit();
                IsRunning = false;
            }

            sw.Stop();
            TotalTicks++;
            LastTickDurationMs = (float)sw.Elapsed.TotalMilliseconds;

            observer.ExitNode(this, CurrentState, LastTickDurationMs);
            LogStateChange(previousState, CurrentState);

            return CurrentState;
        }

        // Phase 2: Execute
        public BHState Execute()
        {
            if (CurrentState != BHState.Running)
                return CurrentState;

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

        public virtual NodeBT DeepCopy()
        {
            var copy = (NodeBT)MemberwiseClone();
            copy.Parent = null; // Reset parent for the copy
            return copy;
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

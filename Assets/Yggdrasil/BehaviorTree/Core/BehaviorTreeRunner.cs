using System.Collections.Generic;
using UnityEngine;

namespace BehaviorTree
{
    public class BehaviorTreeRunner : MonoBehaviour
    {
        [SerializeField] private float _tickInterval = 0f;

        public RootNode Root { get; private set; }
        public Blackboard Blackboard { get; private set; }
        public RunnerObserver Observer => runnerObserver;
        public BHState CurrentState { get; private set; } = BHState.Failure;
        public BHState LastEvaluatedState { get; private set; } = BHState.Failure;
        public bool IsInitialized { get; private set; }

        private RunnerObserver runnerObserver;
        private float _timeSinceLastTick;
        private bool _needsReset;
        private bool warmUp = false;

        public void Initialize(RootNode root, Blackboard blackboard = null)
        {
            Root = root;
            Blackboard = blackboard ?? new Blackboard();

            Root.Initialize(Blackboard);
            CurrentState = BHState.Running;
            LastEvaluatedState = BHState.Running;
            _timeSinceLastTick = 0f;
            _needsReset = false;
            warmUp = true;
            IsInitialized = true;
            

            runnerObserver = new RunnerObserver(Root);
            Blackboard.Set(BBKeys.Self, this.gameObject);
        }

        private void Recursive(NodeBT node, System.Action<NodeBT> action)
        {
            if (node == null) return;
            action(node);
            if (node is CompositeNode composite)
            {
                foreach (var child in composite.GetChildren())
                    Recursive(child, action);
            }
            else if (node is DecoratorNode decorator)
            {
                Recursive(decorator.Child, action);
            }
        }

        private void Update()
        {
            if (!IsInitialized || Root == null)
                return;

            if (_needsReset)
            {
                float saveLastTimeSinceLastTick = _timeSinceLastTick;
                ResetTree();
                _timeSinceLastTick = saveLastTimeSinceLastTick;
            }

            if(warmUp)
            {
                EnterTree();
                warmUp = false;
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
            CurrentState = Root.Tick(ref runnerObserver);
            if (CurrentState != BHState.Running)
            {
                _needsReset = true;
            }
        }

        public void TickManually()
        {
            if (!IsInitialized || Root == null)
                return;

            if (_needsReset)
            {
                float saveLastTimeSinceLastTick = _timeSinceLastTick;
                ResetTree();
                _timeSinceLastTick = saveLastTimeSinceLastTick;
            }

            CurrentState = Root.Tick(ref runnerObserver);

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

            if (_needsReset)
            {
                float saveLastTimeSinceLastTick = _timeSinceLastTick;
                ResetTree();
                _timeSinceLastTick = saveLastTimeSinceLastTick;
            }

            if(warmUp)
            {
                EnterTree();
                warmUp = false;
            }

            LastEvaluatedState = Root.Evaluate(ref runnerObserver);
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
                float saveLastTimeSinceLastTick = _timeSinceLastTick;
                ResetTree();
                _timeSinceLastTick = saveLastTimeSinceLastTick;
                return;
            }

            if (LastEvaluatedState != BHState.Running)
                return;

            var result = Root.Execute();
            CurrentState = result;
        }

        private void EnterTree()
        {
            Recursive(Root, n => n.OnEnterTree());
        }

        private void ExitTree()
        {
            Recursive(Root, n => n.OnExitTree());
        }

        public void ResetTree()
        {
            Root?.Reset();
            CurrentState = BHState.Running;
            LastEvaluatedState = BHState.Running;
            _timeSinceLastTick = 0f;
            _needsReset = false;
            runnerObserver.ResetTree(Root);
            
            ExitTree();
            warmUp = true;
        }

        /// <summary>
        /// Abort only nodes that are currently running in a subtree.
        /// </summary>
        private void AbortRunningSubtree(NodeBT node)
        {
            if (node == null) return;

            // Only abort nodes that are actually running
            if (!node.IsRunning) return;

            node.Abort();

            if (node is CompositeNode composite)
            {
                foreach (var child in composite.GetChildren())
                    AbortRunningSubtree(child);
            }
            else if (node is DecoratorNode decorator)
            {
                AbortRunningSubtree(decorator.Child);
            }
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

    /// <summary>
    /// Tracks the traversal path and node history during tree execution.
    /// Theo dõi đường đi và lịch sử node trong quá trình thực thi tree.
    ///
    /// Records each node ONCE when it first completes (Success/Failure).
    /// Ghi lại mỗi node MỘT LẦN khi nó hoàn thành lần đầu (Success/Failure).
    /// Running nodes are not recorded until they complete.
    /// Node Running không được ghi lại cho đến khi hoàn thành.
    /// </summary>
    public struct RunnerObserver
    {
        // History of completed nodes (accumulates across frames until tree resets)
        // Lịch sử các node đã hoàn thành (tích lũy qua các frame直到 tree reset)
        public List<NodeVisit> Visits;

        // Current path from root to the node being ticked
        // Đường đi hiện tại từ root đến node đang được tick
        public List<int> Path;

        public NodeBT CurrentNode;

        // Depth hiện tại (thay đổi khi descend/ascend)
        public int Depth;

        // Depth của node cuối cùng được tick (dùng để hiển thị)
        public int LastDepth;

        private HashSet<NodeBT> _visitedNodes;

        public RunnerObserver(NodeBT rootNode)
        {
            Visits = new List<NodeVisit>();
            Path = new List<int>();
            CurrentNode = rootNode;
            Depth = 0;
            LastDepth = 0;
            _visitedNodes = new HashSet<NodeBT>();
        }

        // Full reset — called when tree resets (Running → Success/Failure)
        public void ResetTree(NodeBT rootNode)
        {
            Visits.Clear();
            Path.Clear();
            CurrentNode = rootNode;
            Depth = 0;
            LastDepth = 0;
            _visitedNodes.Clear();
        }
        // Called when entering a node — only record if not already visited
        public void EnterNode(NodeBT node)
        {
            CurrentNode = node;
            LastDepth = Depth; // Lưu depth của node hiện tại

            if (!_visitedNodes.Contains(node))
            {
                _visitedNodes.Add(node);
                Visits.Add(new NodeVisit
                {
                    Node = node,
                    Depth = Depth,
                    Timestamp = Time.time
                });
            }
        }

        public void SetChildIndex(int index)
        {
            if (Path.Count > Depth)
                Path[Depth] = index;
            else
                Path.Add(index);
        }

        // Called after a node finishes — update result if not already set
        public void ExitNode(NodeBT node, BHState state, float durationMs)
        {
            for (int i = Visits.Count - 1; i >= 0; i--)
            {
                if (Visits[i].Node == node)
                {
                    var visit = Visits[i];
                    if (visit.Result == BHState.Running)
                    {
                        visit.Result = state;
                        visit.DurationMs = durationMs;
                        Visits[i] = visit;
                    }
                    break;
                }
            }
        }

        public void Descend() { Depth++; }
        public void Ascend() { if (Depth > 0) Depth--; }

        public string GetPathString()
        {
            if (Path.Count == 0) return "/";
            var sb = new System.Text.StringBuilder();
            sb.Append("/");
            for (int i = 0; i < Path.Count; i++)
            {
                sb.Append(Path[i]);
                if (i < Path.Count - 1) sb.Append("->");
            }
            return sb.ToString();
        }
    }

    public struct NodeVisit
    {
        public NodeBT Node;
        public int Depth;
        public BHState Result;
        public float DurationMs;
        public float Timestamp;
    }
}

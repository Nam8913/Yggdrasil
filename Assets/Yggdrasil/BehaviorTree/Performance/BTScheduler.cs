using System;
using System.Collections.Generic;
using UnityEngine;

namespace BehaviorTree.Performance
{
    public class BTScheduler : MonoBehaviour
    {
        public static BTScheduler Instance { get; private set; }

        List<RangeInterval> _evalIntervals = new List<RangeInterval>
        {
            new RangeInterval(10f, 0f, int.MaxValue), // Close range: every frame
            new RangeInterval(25f, 0.1f, 10), // Mid range: every 0.1 seconds
            new RangeInterval(50f, 1f, 10), // Far range: every 1 second
            new RangeInterval(100f, 3f, 10), // Far range: every 3 seconds
            new RangeInterval(float.MaxValue, float.MaxValue, int.MaxValue) // Far range: never evaluate
        };

        private readonly List<RegisteredNPC> _npcs = new List<RegisteredNPC>();
        private Transform _playerTransform;

        // Cached eval counts per interval — allocated once, reused every frame
        // Đánh số lần evaluate theo interval — cấp phát 1 lần, dùng lại mỗi frame
        private int[] _evalCounts;
        private List<int> _runnerRemove = new List<int>();

        private struct RegisteredNPC
        {
            public BehaviorTreeRunner Runner;
            // Thời gian tích lũy kể từ lần evaluate gần nhất (tính bằng giây)
            public double PreviousEvalTime;            
            public bool IsActive;
            public bool NeedsExecute;
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // Init eval counts once — no GC allocation per frame
            _evalCounts = new int[_evalIntervals.Count];
        }

        public void SetPlayerTransform(Transform player)
        {
            _playerTransform = player;
        }

        public void Register(BehaviorTreeRunner runner)
        {
            runner.enabled = false; // Disable the runner initially; it will be enabled when evaluated
            _npcs.Add(new RegisteredNPC
            {
                Runner = runner,
                PreviousEvalTime = 0f,
                IsActive = true
            });
        }

        public void Unregister(BehaviorTreeRunner runner)
        {
            for (int i = _npcs.Count - 1; i >= 0; i--)
            {
                if (_npcs[i].Runner == runner)
                {
                    _npcs.RemoveAt(i);
                    runner.enabled = true; // Enable the runner when unregistered
                    return;
                }
            }
        }

        void Start()
        {
        }
        Vector2 getNextTarget = new Vector2(0, 0);
        private void Update()
        {
            if (_playerTransform == null || _npcs.Count == 0)
                return;

            Vector3 playerPos = _playerTransform.position;
            double deltaTime = Time.deltaTime;

            // Sort theo PreviousEvalTime giảm dần: NPC chờ lâu nhất được evaluate trước
            _npcs.Sort((a, b) => b.PreviousEvalTime.CompareTo(a.PreviousEvalTime));

            // Reset eval counts — Array.Clear is cheaper than new Dictionary()
            // Reset số lần evaluate — Array.Clear rẻ hơn new Dictionary()
            Array.Clear(_evalCounts, 0, _evalCounts.Length);
            
            for (int i = 0; i < _npcs.Count; i++)
            {
                var npc = _npcs[i];

                if (npc.Runner == null || npc.Runner.gameObject == null)
                {
                    AddInvalidRunnerToRemoveList(npc, i);
                    continue;
                }

                if (!npc.IsActive || !npc.Runner.IsInitialized)
                    continue;

                bool evaluated = false;
                float distance = Vector3.Distance(npc.Runner.transform.position, playerPos);
                int intervalIndex = GetIntervalIndexForDistance(distance);
                RangeInterval requiredEvalInterval = _evalIntervals[intervalIndex];

                if(intervalIndex == _evalIntervals.Count - 1)
                {
                    continue; // Never evaluate: skip execution
                }

                if (_evalCounts[intervalIndex] < requiredEvalInterval.MaxEvalsPerFrame
                    && npc.PreviousEvalTime >= requiredEvalInterval.Interval)
                {
                    npc.Runner.Evaluate();
                    npc.PreviousEvalTime = 0;
                    _evalCounts[intervalIndex]++;
                    evaluated = true;
                }

                if (!evaluated)
                    npc.PreviousEvalTime += deltaTime;
                
                // Chỉ Execute() khi: vừa evaluate HOẶC đang Running từ frame trước
                npc.NeedsExecute = evaluated || npc.Runner.CurrentState == BHState.Running;
                
                if(npc.NeedsExecute)
                {
                    npc.Runner.Execute();
                }
                _npcs[i] = npc;
            }

            if (_runnerRemove.Count > 0)
            {
                // Remove invalid runners
                _npcs.RemoveAll(npc => _runnerRemove.Contains(_npcs.IndexOf(npc)));
                _runnerRemove.Clear();
            }
           
        }

        private void OnDrawGizmos()
        {
            foreach(var i in _npcs)
            {
                if (i.Runner != null && i.Runner.gameObject != null)
                {
                    Gizmos.color = Color.blueViolet;
                    Gizmos.DrawWireSphere(i.Runner.transform.position, 0.5f);
                }
            }
            Gizmos.color = Color.green;
            foreach (var interval in _evalIntervals)
            {
                if(_evalIntervals.IndexOf(interval) == _evalIntervals.Count - 1)
                {
                    continue; // Last interval: never evaluate
                }
                Gizmos.DrawWireSphere(transform.position, interval.Range);
            }
        }
        
        private int GetIntervalIndexForDistance(float distance)
        {
            for (int i = 0; i < _evalIntervals.Count; i++)
            {
                if (distance < _evalIntervals[i].Range)
                    return i;
            }
            return _evalIntervals.Count - 1;
        }
        
        private void AddInvalidRunnerToRemoveList(RegisteredNPC runner , int index)
        {
            if (!_runnerRemove.Contains(index))
            {
                _runnerRemove.Add(index);
            }
        }
        public int RegisteredCount => _npcs.Count;
    }

    [System.Serializable]
    public struct RangeInterval
    {
        public float Range;
        public float Interval;
        public int MaxEvalsPerFrame;

        public RangeInterval(float range, float interval, int maxEvalsPerFrame = 10)
        {
            Range = range;
            Interval = interval;
            MaxEvalsPerFrame = maxEvalsPerFrame;
        }
    }
}

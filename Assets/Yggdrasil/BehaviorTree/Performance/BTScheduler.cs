using System.Collections.Generic;
using UnityEngine;

namespace BehaviorTree.Performance
{
    public class BTScheduler : MonoBehaviour
    {
        public static BTScheduler Instance { get; private set; }

        List<RangeInterval> _evalIntervals = new List<RangeInterval>
        {
            new RangeInterval(15f, 0f), // Close range: every frame
            new RangeInterval(50f, 0.2f), // Mid range: every 0.2 seconds
            new RangeInterval(float.MaxValue, -1f, int.MaxValue) // Far range: never evaluate
        };

        private readonly List<RegisteredNPC> _npcs = new List<RegisteredNPC>();
        private Transform _playerTransform;

        private struct RegisteredNPC
        {
            public BehaviorTreeRunner Runner;
            // Thời gian tích lũy kể từ lần evaluate gần nhất (tính bằng giây)
            public double PreviousEvalTime;
            // Khoảng thời gian giữa các lần evaluate hiện tại (tính bằng giây), dựa trên khoảng cách tới player
            public float CurrentEvalInterval;
            public bool IsActive;
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
                CurrentEvalInterval = 0f,
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
                    return;
                }
            }
            runner.enabled = true; // Enable the runner when unregistered
        }

        private void Update()
        {
            if (_playerTransform == null || _npcs.Count == 0)
                return;

            Vector3 playerPos = _playerTransform.position;
            double deltaTime = Time.deltaTime;

            // Sort theo PreviousEvalTime giảm dần: NPC chờ lâu nhất được evaluate trước
            _npcs.Sort((a, b) => b.PreviousEvalTime.CompareTo(a.PreviousEvalTime));

            Dictionary<RangeInterval, int> evalsPerInterval = new Dictionary<RangeInterval, int>();
            foreach (var interval in _evalIntervals)
            {
                evalsPerInterval.Add(interval, 0);
            }

            for (int i = 0; i < _npcs.Count; i++)
            {
                var npc = _npcs[i];
                if (!npc.IsActive || npc.Runner == null || !npc.Runner.IsInitialized || npc.Runner.gameObject == null)
                    continue;

                bool evaluated = false;
                float distance = Vector3.Distance(npc.Runner.transform.position, playerPos);
                RangeInterval requiredEvalInterval = GetRangeIntervalForDistance(distance);

                if (evalsPerInterval.TryGetValue(requiredEvalInterval, out int evalCount)
                    && evalCount < requiredEvalInterval.MaxEvalsPerFrame
                    && npc.PreviousEvalTime >= requiredEvalInterval.Interval)
                {
                    npc.Runner.Evaluate();
                    npc.PreviousEvalTime = 0;
                    npc.CurrentEvalInterval = requiredEvalInterval.Interval;
                    evalsPerInterval[requiredEvalInterval]++;
                    evaluated = true;
                }

                if (!evaluated)
                    npc.PreviousEvalTime += deltaTime;

                _npcs[i] = npc;

                npc.Runner.Execute();
            }
        }

        private RangeInterval GetRangeIntervalForDistance(float distance)
        {
            foreach (var interval in _evalIntervals)
            {
                if (distance < interval.Range)
                    return interval;
            }
            return new RangeInterval(float.MaxValue, -1f, int.MaxValue);
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

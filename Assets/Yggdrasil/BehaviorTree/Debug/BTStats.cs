using System.Collections.Generic;
using UnityEngine;

namespace BehaviorTree.Debug
{
    public class BTStats : MonoBehaviour
    {
        public static BTStats Instance { get; private set; }

        public static BTStats GetInstance()
        {
            if (Instance == null)
            {
                var statsGO = new GameObject("BTStats");
                Instance = statsGO.AddComponent<BTStats>();
                DontDestroyOnLoad(statsGO);
            }
            return Instance;
        }

        private readonly Dictionary<string, NPCStats> _npcStats = new Dictionary<string, NPCStats>();
        private readonly object _lock = new object();

        public struct NPCStats
        {
            public string Name;
            public int TotalTicks;
            public float AverageTickMs;
            public float MaxTickMs;
            public int InterruptCount;
            public int BlackboardKeyCount;
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

        public void RecordTick(string npcName, float tickDurationMs)
        {
            lock (_lock)
            {
                if (!_npcStats.TryGetValue(npcName, out var stats))
                {
                    stats = new NPCStats { Name = npcName };
                }

                stats.TotalTicks++;
                stats.AverageTickMs = ((stats.AverageTickMs * (stats.TotalTicks - 1)) + tickDurationMs) / stats.TotalTicks;
                stats.MaxTickMs = Mathf.Max(stats.MaxTickMs, tickDurationMs);

                _npcStats[npcName] = stats;
            }
        }

        public void RecordInterrupt(string npcName)
        {
            lock (_lock)
            {
                if (!_npcStats.TryGetValue(npcName, out var stats))
                {
                    stats = new NPCStats { Name = npcName };
                }

                stats.InterruptCount++;
                _npcStats[npcName] = stats;
            }
        }

        public void SetBlackboardKeyCount(string npcName, int count)
        {
            lock (_lock)
            {
                if (!_npcStats.TryGetValue(npcName, out var stats))
                {
                    stats = new NPCStats { Name = npcName };
                }

                stats.BlackboardKeyCount = count;
                _npcStats[npcName] = stats;
            }
        }

        public NPCStats? GetStats(string npcName)
        {
            lock (_lock)
            {
                if (_npcStats.TryGetValue(npcName, out var stats))
                    return stats;
                return null;
            }
        }

        public Dictionary<string, NPCStats> GetAllStats()
        {
            lock (_lock)
            {
                return new Dictionary<string, NPCStats>(_npcStats);
            }
        }

        public void Clear()
        {
            lock (_lock)
            {
                _npcStats.Clear();
            }
        }
    }
}

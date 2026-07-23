using System.Collections.Generic;
using UnityEngine;

namespace BehaviorTree.Debug
{
    public class BTLogger : MonoBehaviour
    {
        public static BTLogger Instance { get; private set; }

        public static BTLogger GetInstance()
        {
            if (Instance == null)
            {
                var loggerGO = new GameObject("BTLogger");
                Instance = loggerGO.AddComponent<BTLogger>();
                DontDestroyOnLoad(loggerGO);
            }
            return Instance;
        }

        [SerializeField] private int _maxEntries = 500;
        [SerializeField] private bool _logToConsole = false;

        private readonly Queue<BTLogEntry> _entries = new Queue<BTLogEntry>();
        private readonly object _lock = new object();

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

        public void Log(string npcName, string nodeName, BHState previousState, BHState newState, float durationMs)
        {
            var entry = new BTLogEntry(
                npcName,
                nodeName,
                previousState,
                newState,
                durationMs,
                Time.time
            );

            lock (_lock)
            {
                _entries.Enqueue(entry);

                while (_entries.Count > _maxEntries)
                {
                    _entries.Dequeue();
                }
            }

            if (_logToConsole)
            {
                UnityEngine.Debug.Log($"[BT] {entry}");
            }
        }

        public List<BTLogEntry> GetEntries()
        {
            lock (_lock)
            {
                return new List<BTLogEntry>(_entries);
            }
        }

        public List<BTLogEntry> GetEntriesForNpc(string npcName)
        {
            lock (_lock)
            {
                var result = new List<BTLogEntry>();
                foreach (var entry in _entries)
                {
                    if (entry.NpcName == npcName)
                        result.Add(entry);
                }
                return result;
            }
        }

        public void Clear()
        {
            lock (_lock)
            {
                _entries.Clear();
            }
        }

        public int EntryCount
        {
            get
            {
                lock (_lock)
                {
                    return _entries.Count;
                }
            }
        }
    }
}

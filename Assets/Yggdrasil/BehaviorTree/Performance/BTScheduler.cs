using System.Collections.Generic;
using UnityEngine;

namespace BehaviorTree.Performance
{
    public class BTScheduler : MonoBehaviour
    {
        public static BTScheduler Instance { get; private set; }

        [Header("Distance Thresholds (for Evaluate phase only)")]
        [SerializeField] private float _closeRange = 15f;
        [SerializeField] private float _midRange = 50f;

        [Header("Evaluate Intervals (logic phase)")]
        [SerializeField] private float _midRangeEvalInterval = 0.2f;
        [SerializeField] private float _farRangeEvalInterval = 1f;

        [Header("Staggering")]
        [SerializeField] private int _maxEvalsPerFrame = 10;

        private readonly List<RegisteredNPC> _npcs = new List<RegisteredNPC>();
        private Transform _playerTransform;

        private struct RegisteredNPC
        {
            public BehaviorTreeRunner Runner;
            public float LastEvalTime;
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
            _npcs.Add(new RegisteredNPC
            {
                Runner = runner,
                LastEvalTime = 0f,
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
        }

        private void Update()
        {
            if (_playerTransform == null || _npcs.Count == 0)
                return;

            Vector3 playerPos = _playerTransform.position;
            float currentTime = Time.time;
            int evalsThisFrame = 0;

            for (int i = 0; i < _npcs.Count; i++)
            {
                var npc = _npcs[i];
                if (!npc.IsActive || npc.Runner == null || !npc.Runner.IsInitialized || npc.Runner.gameObject == null)
                    continue;

                // Phase 2: Execute EVERY frame (no throttling)
                // This ensures movement speed is consistent for all NPCs
                npc.Runner.Execute();

                // Phase 1: Evaluate at throttled rate based on distance
                float distance = Vector3.Distance(
                    npc.Runner.transform.position,
                    playerPos
                );

                float requiredEvalInterval = GetEvalIntervalForDistance(distance);

                if (evalsThisFrame < _maxEvalsPerFrame &&
                    currentTime - npc.LastEvalTime >= requiredEvalInterval)
                {
                    npc.Runner.Evaluate();
                    npc.LastEvalTime = currentTime;
                    npc.CurrentEvalInterval = requiredEvalInterval;
                    evalsThisFrame++;
                }

                _npcs[i] = npc;
            }
        }

        private float GetEvalIntervalForDistance(float distance)
        {
            if (distance < _closeRange)
                return 0f; // Every frame
            if (distance < _midRange)
                return _midRangeEvalInterval;
            return _farRangeEvalInterval;
        }

        public int RegisteredCount => _npcs.Count;
    }
}

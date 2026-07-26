using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BehaviorTree;
using UnityEngine;
using UnityEngine.Profiling;

public class Scene : MonoBehaviour
{
    Thread mainThread;
    [SerializeField]
    public float FPS;
    private decimal last;

    [SerializeField]
    [TextArea(10, 50)]
    string debug;
    private long lastMemoryRecorded;
    System.Diagnostics.Stopwatch stopwatch;

    void Start()
    {
        stopwatch = new System.Diagnostics.Stopwatch();
        stopwatch.Start();

        mainThread = Thread.CurrentThread;
        Debug.Log($"Main thread ID: {mainThread.ManagedThreadId}");
        lastMemoryRecorded = Profiler.GetTotalAllocatedMemoryLong();

        GameObject npc = new GameObject("NPC");
        var builder = BuildSurvivalBehaviorTree(npc, new Blackboard());
        var runner = npc.AddComponent<BehaviorTreeRunner>();
        runner.Initialize(builder.root, builder.blackboard);
         
       Record();
       Log($"Scene.Start end - Time elapsed: {stopwatch.ElapsedMilliseconds} ms.");
       stopwatch.Stop();
    }

    class Test
    {
        public int Value { get; set; }
        public void LogValue()
        {
            Debug.Log($"Value: {Value}");
        }
    }

    IEnumerator Count()
    {
        int count = 100000;
        int n = count;
        ///int iterationsPerFrame = 1000; // Number of iterations to perform per frame

        long timer = 5; // 1 second in milliseconds
        decimal totalTime = 0;
        
        
        while(n > 0)
        {
            totalTime += stopwatch.ElapsedMilliseconds;
            stopwatch.Restart();
            while (stopwatch.ElapsedMilliseconds < timer && n > 0)
            {
                Debug.Log($"{n}");
                n--;
            }
            yield return null; // Wait for the next frame
        }
        stopwatch.Stop();
        Debug.Log($"Completed {count} iterations in {totalTime} ms.");
        yield return null;
    }

    // Update is called once per frame
    void Update()
    {
        last += ((decimal)Time.unscaledDeltaTime - last) * 0.1m;
        FPS = 1f / (float)last;
    }

    void OnGUI()
    {
        GUI.Label(new Rect(10, 10, 300, 20), $"FPS: {FPS:F2}");
        GUI.Label(new Rect(10, 30, 500, 500), $"Debug Info:\n{debug}");
    }

    void Log(string message)
    {
        debug += message + "\n";
    }

    void Record()
    {
        // Log memory usage
        long monoMemory = Profiler.GetMonoUsedSizeLong();
        long monoHeapSize = Profiler.GetMonoHeapSizeLong();
        long totalMemory = Profiler.GetTotalAllocatedMemoryLong();
        long reservedMemory = Profiler.GetTotalReservedMemoryLong();
        long unusedReservedMemory = Profiler.GetTotalUnusedReservedMemoryLong();

        Log($"Bộ nhớ Mono/C# đang dùng - Mono Memory: {GetUnitMemoryFor(monoMemory)}");
        Log($"Bộ nhớ Heap của Mono - Mono Heap Size: {GetUnitMemoryFor(monoHeapSize)}");
        Log($"Tổng bộ nhớ đã phân bổ - Total Allocated Memory: {GetUnitMemoryFor(totalMemory)}");
        Log($"Tổng bộ nhớ đã dành riêng - Total Reserved Memory: {GetUnitMemoryFor(reservedMemory)}");
        Log($"Tổng bộ nhớ dành riêng chưa sử dụng - Total Unused Reserved Memory: {GetUnitMemoryFor(unusedReservedMemory)}");

        long memoryDifference = totalMemory - lastMemoryRecorded;
        Log($"Sự khác biệt bộ nhớ kể từ lần ghi cuối cùng: {GetUnitMemoryFor(memoryDifference)}");
    }

    string GetUnitMemoryFor(long bytes)
    {
        if (bytes < 1024)
            return $"{bytes} B";
        else if (bytes < 1024 * 1024)
            return $"{bytes / 1024f:F2} KB";
        else if (bytes < 1024 * 1024 * 1024)
            return $"{bytes / (1024f * 1024f):F2} MB";
        else
            return $"{bytes / (1024f * 1024f * 1024f):F2} GB";
    }

    private (RootNode root, Blackboard blackboard) BuildSurvivalBehaviorTree(GameObject GO, Blackboard bb)
    {
        return new BehaviorTreeBuilder(bb)
        .Root()
            .Sequence()
                .Action(new WanderAction(GO.transform))
                .Wait(1f)
                    .Action(new WanderAction(GO.transform))
                .End()
                .Wait(2f)
                .End()
            .End()
            
        .BuildWithBlackboard();
    }

    public class LookAroundAction : ActionNode
    {
        private readonly Transform _creature;
        private float _lookDuration = 2f;
        private float _elapsed;
        private float _startAngle;

        public LookAroundAction(Transform creature) { _creature = creature; }

        protected override void OnEnter()
        {
            _elapsed = 0f;
            _startAngle = Mathf.Atan2(_creature.transform.right.y, _creature.transform.right.x) * Mathf.Rad2Deg;
            Debug.Log($"[{_creature.name}] Looking around...");
            Blackboard.Remove(BBKeys.HeardNoise);
        }

        protected override void OnExit()
        {
            Debug.Log($"[{_creature.name}] Finished looking around.");
        }

        protected override BHState OnEvaluate(ref RunnerObserver observer)
        {
            _elapsed += Time.deltaTime;
            return _elapsed >= _lookDuration ? BHState.Success : BHState.Running;
        }

        protected override BHState OnExecute()
        {
            // Quay đầu nhìn xung quanh (quay 360 độ trong thời gian _lookDuration)
            float t = _elapsed / _lookDuration;
            float angle = _startAngle + t * 360f;
            Vector2 dir = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad));
            _creature.transform.right = dir;

            return EvaluatedState;
        }

        protected override BHState OnUpdate(ref RunnerObserver observer) { OnEvaluate(ref observer); return OnExecute(); }
    }
}
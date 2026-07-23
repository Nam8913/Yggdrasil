# AGENTS.md

## Project

Unity 6 (6000.5.4f1) C# project — behavior tree framework for NPC AI. Located at `Assets/Yggdrasil/BehaviorTree/`.

## Architecture: Two-Phase Evaluation

The core design splits node ticking into two phases:

- **Phase 1 — `Evaluate()`**: Pure logic, returns `BHState`. Designed for future worker-thread execution (not yet active).
- **Phase 2 — `Execute()`**: Unity API calls (movement, physics, animation). Main thread only. Runs every frame.

`BehaviorTreeRunner` orchestrates both phases. `BTScheduler` manages batched evaluation across all NPCs based on distance to player.

**Critical rule**: When overriding `ActionNode`, override `OnExecute()` for Unity API work, NOT `OnUpdate()`. `OnUpdate()` is legacy and will be removed. `OnEvaluate()` defaults to calling `OnUpdate()` for backward compatibility.

## Node Hierarchy

```
NodeBT (abstract)
├── CompositeNode → SequenceNode, SelectorNode, RandomSelectorNode, ParallelNode
├── DecoratorNode → RootNode, InverterNode, CooldownNode, RepeaterNode, TimeLimitNode, GuardNode, SubTreeNode
├── ActionNode (leaf — extend this for NPC behaviors)
├── ConditionNode (leaf — pure logic checks)
└── ServiceNode (not yet implemented)
```

All nodes are constructed via `BehaviorTreeBuilder` (fluent API). Example:

```csharp
var tree = new BehaviorTreeBuilder()
    .Root()
        .Sequence()
            .Condition(() => blackboard.Get(BBKeys.CanSeeEnemy))
            .Action(new ChaseAction())
        .End()
    .Build();
```

## BTScheduler — Distance-Based Throttling

`BTScheduler` (singleton MonoBehaviour) throttles NPC evaluation:

| Distance | Interval | MaxEvalsPerFrame |
|----------|----------|-----------------|
| < 15m | 0s (every frame) | 10 |
| < 50m | 0.2s | 10 |
| >= 50m | never | — |

NPCs are sorted by `PreviousEvalTime` descending each frame — longest-waiting NPCs evaluate first. Fair round-robin behavior across all NPCs in the same distance band.

`BTScheduler` calls `Evaluate()` + `Execute()` on each NPC. Do NOT call `BehaviorTreeRunner.Update()` manually when using the scheduler.

## Blackboard

Thread-safe key-value store with observer pattern. Keys defined in `BBKeys.cs` (partial static class). Use `BBKey<T>` for type-safe access:

```csharp
blackboard.Set(BBKeys.MoveTarget, somePosition);
var target = blackboard.Get(BBKeys.MoveTarget);
blackboard.Subscribe<Vector3>(BBKeys.MoveTarget, OnTargetChanged);
```

`Self` key is auto-populated with the owning GameObject on `Initialize()`.

## Namespace

All framework code lives in `namespace BehaviorTree`.

## Conventions

- Node state enum: `BHState` — `Running`, `Success`, `Failure`
- Singleton pattern: `Instance` property with duplicate-destroy in `Awake()`
- Thread safety: `lock` on shared collections in `Blackboard`, `BTLogger`, `NodePool`
- No unit test files exist yet (`Assets/Yggdrasil/BehaviorTree/Tests/` is empty)
- `Assets/Scripts/Scene.cs` is a debug/profiling helper, not part of the framework

## Gotchas

- `RootNode` must be the first node in the builder chain (`.Root()`)
- Missing `End()` calls in the builder produce warnings but don't crash
- `Service()` in the builder is a no-op (logs warning, not implemented)
- `Evaluate()` and `Execute()` are separate calls — never call `Tick()` when using the scheduler
- `_needsReset` flag in `BehaviorTreeRunner` delays reset to next frame so state is observable in the editor

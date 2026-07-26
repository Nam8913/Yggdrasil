# Yggdrasil — Behavior Tree Framework for NPC AI

A modular, two-phase behavior tree framework for Unity 6, designed for scalable NPC AI with distance-based performance throttling.

## Features

- **Two-Phase Architecture** — Separate `Evaluate()` (logic) and `Execute()` (Unity API) phases for future thread safety
- **9 Composite Nodes** — Sequence, Selector, RandomComposite, Parallel, NAND, NOR, XOR, XNOR
- **14 Decorator Nodes** — Inverter, Cooldown, Repeater, TimeLimit, Wait, WaitUntil, Guard, Succeeder, Failer, and more
- **14 Condition Nodes** — Blackboard comparisons (int, float, bool, string, generic), GameObject checks (tag, component, layer, active)
- **5 Action Nodes** — Wander, MoveTo, RotateTo, Stop, SetVelocity
- **RunnerObserver** — Tree traversal tracking for debugging and visualization
- **Distance-Based Scheduling** — BTScheduler throttles NPC evaluation based on distance to player
- **Thread-Safe Blackboard** — Key-value store with observer pattern and type-safe `BBKey<T>`
- **Fluent Builder API** — Construct trees with readable, chainable syntax

## Quick Start

```csharp
using BehaviorTree;

// Build a simple NPC tree
var tree = new BehaviorTreeBuilder()
    .Root()
        .Selector()
            // Attack if enemy is visible
            .Sequence()
                .Condition(new BBCompareBool { Key = "CanSeeEnemy", Value = true })
                .Action(new ChaseAction())
            .End()
            // Otherwise wander
            .Action(new WanderAction(transform))
        .End()
    .Build();

// Attach to NPC
var runner = npc.AddComponent<BehaviorTreeRunner>();
runner.Initialize(tree);
```

## Project Structure

```
Assets/Yggdrasil/BehaviorTree/
├── Core/                  — NodeBT, BehaviorTreeRunner, BehaviorTreeBuilder, BHState
├── Composites/            — Sequence, Selector, RandomComposite, Parallel, NAND, NOR, XOR, XNOR
├── Decorators/            — Inverter, Cooldown, Repeater, Wait, Guard, Succeeder, Failer, etc.
├── Condition/             — BBCompare*, GOCheck*, ConditionNode base class
├── AIActions/             — Wander, MoveTo, RotateTo, Stop, SetVelocity
├── Blackboard/            — Blackboard, BBKey<T>, BBKeys
├── Sensors/               — VisionSensor, HearingSensor, DamageSensor (2D)
├── Performance/           — BTScheduler, NodePool
└── Debug/                 — BTLogger, BTStats, BTGizmos, BTDebugWindow
```

## Node Reference

### Composites

| Node | Logic | Early-Exit Field | Description |
|------|-------|-----------------|-------------|
| `Sequence` | AND | `BreakOnFirstFailure` | All children must succeed |
| `Selector` | OR | `BreakOnFirstSuccess` | Any child can succeed |
| `RandomComposite` | Configurable | `BreakOnFirstMatch` | Shuffled order + logic gate |
| `Parallel` | Configurable | — | All children tick simultaneously |
| `Nand` | NAND | `BreakOnFirstFailure` | Fails only if ALL children succeed |
| `Nor` | NOR | `BreakOnFirstSuccess` | Succeeds only if ALL children fail |
| `Xor` | XOR | `BreakOnMixed` | Succeeds if children disagree |
| `Xnor` | XNOR | `BreakOnMixed` | Succeeds if all children agree |

### Decorators

| Node | Description |
|------|-------------|
| `Root` | Tree entry point (must be first in builder) |
| `Inverter` | Flip Success ↔ Failure |
| `Succeeder` | Always return Success |
| `Failer` | Always return Failure |
| `Cooldown` | Rate-limit execution |
| `Repeater` | Repeat child N times or infinitely |
| `TimeLimit` | Abort child if it takes too long |
| `Wait` | Delay before executing child |
| `WaitUntil` | Wait for condition before executing child |
| `RepeatUntil` | Repeat until condition is true |
| `RepeatUntilState` | Repeat until child returns specific state |
| `Guard` | Conditional execution with abort support |
| `SubTree` | Wrap a sub-tree as a single node |
| `Service` | Execute periodic service action |

### Conditions

| Node | Description |
|------|-------------|
| `BBHasKey` | Check if Blackboard key exists |
| `BBCompareBool` | Compare bool value |
| `BBCompareInt` | Compare int with 6 operators |
| `BBCompareFloat` | Compare float with 6 operators |
| `BBCompareString` | Compare string (equal/not equal) |
| `BBCompareValue<T>` | Generic comparison for any `IComparable<T>` |
| `GOCondition` | Custom GameObject check via lambda |
| `GOHasComponent<T>` | Check if component exists |
| `GOHasTag` | Check if tag matches |
| `IsGOEnabled` | Check if GameObject is active |
| `IsGOInLayer` | Check if GameObject is on a layer |

### Actions

| Node | Description |
|------|-------------|
| `Wander` | Random movement within radius |
| `MoveTo` | Move to Blackboard target position |
| `RotateTo` | Face toward target direction |
| `Stop` | Halt all movement |
| `SetVelocity` | Set Rigidbody2D velocity directly |

## RunnerObserver

Tracks tree traversal for debugging:

```csharp
var observer = runner.Observer;
Debug.Log($"Path: {observer.GetPathString()}");  // "/0.1.2"
Debug.Log($"Depth: {observer.LastDepth}");        // 2
Debug.Log($"Visits: {observer.Visits.Count}");    // nodes completed
```

## Performance

The `BTScheduler` automatically throttles NPC evaluation based on distance to the player:

| Distance | Evaluation Rate |
|----------|----------------|
| < 15m | Every frame |
| < 50m | Every 0.2s |
| >= 50m | Never evaluated |

NPCs are sorted by wait time each frame for fair round-robin scheduling. Uses `int[]` array for eval counts (zero GC allocation per frame).

## Requirements

- Unity 6 (6000.5.4f1 or later)
- Input System package
- URP (Universal Render Pipeline)

## License

Personal / educational use. Contact author for commercial licensing.

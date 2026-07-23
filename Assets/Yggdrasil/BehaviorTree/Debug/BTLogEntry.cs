namespace BehaviorTree.Debug
{
    public readonly struct BTLogEntry
    {
        public readonly string NodeName;
        public readonly BHState PreviousState;
        public readonly BHState NewState;
        public readonly float DurationMs;
        public readonly float Timestamp;
        public readonly string NpcName;

        public BTLogEntry(string npcName, string nodeName, BHState previousState, BHState newState, float durationMs, float timestamp)
        {
            NpcName = npcName;
            NodeName = nodeName;
            PreviousState = previousState;
            NewState = newState;
            DurationMs = durationMs;
            Timestamp = timestamp;
        }

        public override string ToString()
        {
            return $"[{NpcName}] {NodeName}: {PreviousState} -> {NewState} ({DurationMs:F2}ms)";
        }
    }
}

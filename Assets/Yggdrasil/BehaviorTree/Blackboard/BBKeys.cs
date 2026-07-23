using UnityEngine;

namespace BehaviorTree
{
    // This class contains all the blackboard keys used in the behavior tree system.
    public static partial class BBKeys
    {
        // General
        public static readonly BBKey<GameObject> Self = new("Self");

        // Movement
        public static readonly BBKey<Vector3> MoveTarget = new("MoveTarget");
        public static readonly BBKey<Vector3> LookTarget = new("LookTarget");

        // Combat
        public static readonly BBKey<Transform> ThreatTarget = new("ThreatTarget");
        public static readonly BBKey<bool> InCombat = new("InCombat");

        // Perception
        public static readonly BBKey<bool> CanSeeEnemy = new("CanSeeEnemy");
        public static readonly BBKey<bool> HeardNoise = new("HeardNoise");
        public static readonly BBKey<Vector3> LastKnownEnemyPosition = new("LastKnownEnemyPosition");
        public static readonly BBKey<Vector3> NoisePosition = new("NoisePosition");

        // Tasks
        public static readonly BBKey<Transform> InteractTarget = new("InteractTarget");
    }
}

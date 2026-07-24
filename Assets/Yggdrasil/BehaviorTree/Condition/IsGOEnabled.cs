using UnityEngine;

namespace BehaviorTree
{
    /// <summary>
    /// Checks if the NPC's GameObject is active.
    /// Kiểm tra xem GameObject của NPC có đang hoạt động hay không.
    ///
    /// - ActiveInHierarchy = false (default): checks activeSelf (this object only).
    /// - ActiveInHierarchy = true: checks activeInHierarchy (including parents).
    ///
    /// Usage: ".Condition(new IsGOEnabled())" or ".Condition(new IsGOEnabled { ActiveInHierarchy = true })"
    /// </summary>
    public class IsGOEnabled : ConditionNode
    {
        public bool ActiveInHierarchy { get; set; } = false;

        protected override bool Check()
        {
            var self = Blackboard.Get(BBKeys.Self);
            if (self == null)
                return false;

            return ActiveInHierarchy ? self.activeInHierarchy : self.activeSelf;
        }
    }
}
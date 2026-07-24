using UnityEngine;

namespace BehaviorTree
{
    /// <summary>
    /// Checks if the NPC's GameObject is on a specific layer.
    /// Kiểm tra xem GameObject của NPC có ở một layer cụ thể hay không.
    ///
    /// Usage: ".Condition(new IsGOInLayer { LayerName = "Enemy" })"
    /// </summary>
    public class IsGOInLayer : ConditionNode
    {
        public string LayerName { get; set; } = "Default";

        private int _cachedLayer = -2;
        private string _cachedLayerName;

        protected override bool Check()
        {
            if (_cachedLayerName != LayerName)
            {
                _cachedLayer = LayerMask.NameToLayer(LayerName);
                _cachedLayerName = LayerName;
            }

            if (_cachedLayer == -1)
            {
                UnityEngine.Debug.LogWarning($"Layer '{LayerName}' does not exist.");
                return false;
            }

            var self = Blackboard.Get(BBKeys.Self);
            if (self == null)
                return false;

            return self.layer == _cachedLayer;
        }
    }
}

using UnityEngine;

namespace BehaviorTree
{
    /// <summary>
    /// Checks if the NPC's GameObject has a specific component.
    /// Kiểm tra xem GameObject của NPC có một component cụ thể hay không.
    ///
    /// Usage: ".Condition(new GOHasComponent<Rigidbody2D>())"
    /// Sử dụng: kiểm tra NPC có Rigidbody2D không.
    /// </summary>
    public class GOHasComponent<T> : ConditionNode where T : Component
    {
        protected override bool Check()
        {
            var self = Blackboard.Get(BBKeys.Self);
            if (self == null)
                return false;

            return self.GetComponent<T>() != null;
        }
    }

    /// <summary>
    /// Checks if the NPC's GameObject has a component by type name.
    /// Kiểm tra xem GameObject của NPC có component theo tên type hay không.
    ///
    /// Usage: ".Condition(new GOHasComponent("Rigidbody2D"))"
    /// Sử dụng: kiểm tra NPC có Rigidbody2D không (dùng string).
    /// </summary>
    public class GOHasComponent : ConditionNode
    {
        // Component type name to check (e.g., "Rigidbody2D", "Collider2D")
        // Tên type component để kiểm tra (ví dụ: "Rigidbody2D", "Collider2D")
        public string ComponentTypeName { get; set; } = string.Empty;

        protected override bool Check()
        {
            var self = Blackboard.Get(BBKeys.Self);
            if (self == null)
                return false;

            // Use Type.GetType with Unity's assemblies
            // Dùng Type.GetType với các assembly của Unity
            var component = self.GetComponent(ComponentType);
            return component != null;
        }

        // Cached Component type to avoid reflection every tick
        // Cache Component type để tránh reflection mỗi frame
        private System.Type _componentType;
        private bool _typeCached;

        private System.Type ComponentType
        {
            get
            {
                if (!_typeCached)
                {
                    // Search in UnityEngine and UnityEngine.CoreModule assemblies
                    // Tìm trong các assembly UnityEngine và UnityEngine.CoreModule
                    foreach (var assembly in System.AppDomain.CurrentDomain.GetAssemblies())
                    {
                        _componentType = assembly.GetType("UnityEngine." + ComponentTypeName);
                        if (_componentType != null)
                            break;
                    }

                    // Fallback: try without namespace
                    // Fallback: thử không có namespace
                    if (_componentType == null)
                    {
                        foreach (var assembly in System.AppDomain.CurrentDomain.GetAssemblies())
                        {
                            _componentType = assembly.GetType(ComponentTypeName);
                            if (_componentType != null)
                                break;
                        }
                    }

                    _typeCached = true;
                }
                return _componentType;
            }
        }
    }
}

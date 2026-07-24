using System;

namespace BehaviorTree
{
    /// <summary>
    /// Checks a bool value on the Blackboard.
    /// Kiểm tra giá trị bool trên Blackboard.
    ///
    /// Usage: ".Condition(new BBCompareBool { Key = "CanSeeEnemy", Value = true })"
    /// Sử dụng: kiểm tra NPC có thấy kẻ thù không.
    /// </summary>
    public class BBCompareBool : ConditionNode
    {
        // Blackboard key name to read
        // Tên key trên Blackboard để đọc
        public string Key { get; set; } = string.Empty;

        // Expected bool value
        // Giá trị bool mong đợi
        public bool Value { get; set; }

        private BBKey<bool> _cachedKey;
        private bool _keyCached;

        protected override bool Check()
        {
            if (!_keyCached || _cachedKey.Name != Key)
            {
                _cachedKey = new BBKey<bool>(Key);
                _keyCached = true;
            }

            if (Blackboard.TryGet(_cachedKey, out bool currentValue))
            {
                return currentValue == Value;
            }

            return false;
        }

        public BBCompareBool SetValue(Func<bool> func)
        {
            this.Value = func.Invoke();
            return this;
        }

        public BBCompareBool SetValue(bool value)
        {
            this.Value = value;
            return this;
        }
    }
}

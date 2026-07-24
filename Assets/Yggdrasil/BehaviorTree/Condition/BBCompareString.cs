using System;

namespace BehaviorTree
{
    /// <summary>
    /// Checks a string value on the Blackboard.
    /// Kiểm tra giá trị string trên Blackboard.
    ///
    /// Usage: ".Condition(new BBCompareString { Key = "CurrentState", Value = "Idle", Comparison = ComparisonType.Equal })"
    /// Sử dụng: kiểm tra trạng thái NPC có phải "Idle" không.
    /// </summary>
    public class BBCompareString : ConditionNode
    {
        // Blackboard key name to read
        // Tên key trên Blackboard để đọc
        public string Key { get; set; } = string.Empty;

        // String value to compare against
        // Giá trị string để so sánh
        public string Value { get; set; } = string.Empty;

        // Comparison type: Equal, NotEqual
        // Loại so sánh: Equal, NotEqual
        public ComparisonType Comparison { get; set; } = ComparisonType.Equal;

        private BBKey<string> _cachedKey;
        private bool _keyCached;

        protected override bool Check()
        {
            if (!_keyCached || _cachedKey.Name != Key)
            {
                _cachedKey = new BBKey<string>(Key);
                _keyCached = true;
            }

            if (Blackboard.TryGet(_cachedKey, out string currentValue))
            {
                return Comparison switch
                {
                    ComparisonType.Equal => currentValue == Value,
                    ComparisonType.NotEqual => currentValue != Value,
                    _ => false
                };
            }

            return false;
        }

        public BBCompareString SetValue(Func<string> func)
        {
            this.Value = func.Invoke();
            return this;
        }

        public BBCompareString SetValue(string value)
        {
            this.Value = value;
            return this;
        }
    }
}

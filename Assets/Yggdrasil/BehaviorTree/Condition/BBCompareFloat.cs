using System;

namespace BehaviorTree
{
    /// <summary>
    /// Checks a float value on the Blackboard against a threshold.
    /// Kiểm tra giá trị float trên Blackboard so với ngưỡng.
    ///
    /// Usage: ".Condition(new BBCompareFloat { Key = "Distance", Value = 10f, Comparison = ComparisonType.LessThan })"
    /// Sử dụng: kiểm tra khoảng cách < 10, tốc độ > 5, v.v.
    /// </summary>
    public class BBCompareFloat : ConditionNode
    {
        // Blackboard key name to read
        // Tên key trên Blackboard để đọc
        public string Key { get; set; } = string.Empty;

        // Value to compare against
        // Giá trị để so sánh
        public float Value { get; set; }

        // Comparison operator to use
        // Phép so sánh cần sử dụng
        public ComparisonType Comparison { get; set; } = ComparisonType.Equal;

        private BBKey<float> _cachedKey;
        private bool _keyCached;

        protected override bool Check()
        {
            if (!_keyCached || _cachedKey.Name != Key)
            {
                _cachedKey = new BBKey<float>(Key);
                _keyCached = true;
            }

            if (Blackboard.TryGet(_cachedKey, out float currentValue))
            {
                return Comparison switch
                {
                    ComparisonType.Equal => currentValue == Value,
                    ComparisonType.NotEqual => currentValue != Value,
                    ComparisonType.GreaterThan => currentValue > Value,
                    ComparisonType.LessThan => currentValue < Value,
                    ComparisonType.GreaterThanOrEqual => currentValue >= Value,
                    ComparisonType.LessThanOrEqual => currentValue <= Value,
                    _ => false
                };
            }

            return false;
        }

        public BBCompareFloat SetValue(Func<float> func)
        {
            this.Value = func.Invoke();
            return this;
        }

        public BBCompareFloat SetValue(float value)
        {
            this.Value = value;
            return this;
        }
    }
}

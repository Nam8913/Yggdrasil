using System;

namespace BehaviorTree
{
    /// <summary>
    /// Checks an int value on the Blackboard against a threshold.
    /// Kiểm tra giá trị int trên Blackboard so với ngưỡng.
    ///
    /// Usage: ".Condition(new BBCompareInt { Key = "Health", Value = 50, Comparison = ComparisonType.LessThan })"
    /// Sử dụng: kiểm tra máu < 50, kẻ thù > 3, v.v.
    /// </summary>
    public class BBCompareInt : ConditionNode
    {
        // Blackboard key name to read
        // Tên key trên Blackboard để đọc
        public string Key { get; set; } = string.Empty;

        // Value to compare against
        // Giá trị để so sánh
        public int Value { get; set; }

        // Comparison operator to use
        // Phép so sánh cần sử dụng
        public ComparisonType Comparison { get; set; } = ComparisonType.Equal;

        // Cached key to avoid allocation every tick
        // Key được cache để tránh phân bổ mỗi frame
        private BBKey<int> _cachedKey;
        private bool _keyCached;

        protected override bool Check()
        {
            // Lazy init cached key
            // Lazy init key đã cache
            if (!_keyCached || _cachedKey.Name != Key)
            {
                _cachedKey = new BBKey<int>(Key);
                _keyCached = true;
            }

            if (Blackboard.TryGet(_cachedKey, out int currentValue))
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

            // Key not found — condition fails
            // Không tìm thấy key — điều kiện thất bại
            return false;
        }

        public BBCompareInt SetValue(Func<int> func)
        {
            this.Value = func.Invoke();
            return this;
        }

        public BBCompareInt SetValue(int value)
        {
            this.Value = value;
            return this;
        }
    }
}

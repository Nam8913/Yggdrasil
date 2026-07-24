using System;

namespace BehaviorTree
{
    /// <summary>
    /// Generic condition that compares any IComparable value on the Blackboard.
    /// Điều kiện_generic so sánh bất kỳ giá trị IComparable nào trên Blackboard.
    ///
    /// Usage: ".Condition(new BBCompareValue<float> { Key = "Distance", Value = 10f, Comparison = ComparisonType.LessThan })"
    /// Sử dụng: kiểm tra khoảng cách < 10, máu > 50, v.v.
    /// </summary>
    public class BBCompareValue<T> : ConditionNode where T : IComparable<T>
    {
        // Blackboard key name to read
        // Tên key trên Blackboard để đọc
        public string Key { get; set; } = string.Empty;

        // Value to compare against
        // Giá trị để so sánh
        public T Value { get; set; }

        // Comparison operator to use
        // Phép so sánh cần sử dụng
        public ComparisonType Comparison { get; set; } = ComparisonType.Equal;

        // Cached key to avoid allocation every tick
        // Key được cache để tránh phân bổ mỗi frame
        private BBKey<T> _cachedKey;

        private bool _keyCached;

        protected override bool Check()
        {
            // Lazy init cached key
            if (!_keyCached || _cachedKey.Name != Key)
            {
                _cachedKey = new BBKey<T>(Key);
                _keyCached = true;
            }

            if (Blackboard.TryGet(_cachedKey, out T currentValue))
            {
                int cmp = currentValue.CompareTo(Value);
                return Comparison switch
                {
                    ComparisonType.Equal => cmp == 0,
                    ComparisonType.NotEqual => cmp != 0,
                    ComparisonType.GreaterThan => cmp > 0,
                    ComparisonType.LessThan => cmp < 0,
                    ComparisonType.GreaterThanOrEqual => cmp >= 0,
                    ComparisonType.LessThanOrEqual => cmp <= 0,
                    _ => false
                };
            }

            return false;
        }
 

        public BBCompareValue<T> SetValue(Func<T> func)
        {
            this.Value = func.Invoke();
            return this;
        }

        public BBCompareValue<T> SetValue(T value)
        {
            this.Value = value;
            return this;
        }
    }
}
namespace BehaviorTree
{
    /// <summary>
    /// Checks if a key exists on the Blackboard (regardless of its value).
    /// Kiểm tra xem một key có tồn tại trên Blackboard hay không (bất kể giá trị).
    ///
    /// Usage: ".Condition(new BBHasKey { Key = "ThreatTarget" })"
    /// Sử dụng: kiểm tra NPC có đang nhắm mục tiêu không.
    /// </summary>
    public class BBHasKey : ConditionNode
    {
        // Blackboard key name to check
        // Tên key trên Blackboard để kiểm tra
        public string Key { get; set; } = string.Empty;

        protected override bool Check()
        {
            // Try to get as object — if key exists, TryGet returns true
            // Thử lấy dạng object — nếu key tồn tại, TryGet trả về true
            // Use a dummy BBKey<object> to probe existence
            var probeKey = new BBKey<object>(Key);
            return Blackboard.TryGet(probeKey, out _);
        }
    }
}

using System;

namespace BehaviorTree
{
    /// <summary>
    /// Concrete ConditionNode that wraps a Func&lt;bool&gt; lambda.
    /// ConditionNode cụ thể bọc một lambda Func&lt;bool&gt;.
    ///
    /// Usage in builder:
    /// Sử dụng trong builder:
    /// <code>
    /// .Condition(() => blackboard.Get(BBKeys.CanSeeEnemy))
    /// </code>
    /// </summary>
    public class ActionConditionNode : ConditionNode
    {
        private readonly Func<bool> _condition;

        public ActionConditionNode(Func<bool> condition)
        {
            _condition = condition;
        }

        protected override bool Check()
        {
            return _condition != null && _condition.Invoke();
        }
    }
}

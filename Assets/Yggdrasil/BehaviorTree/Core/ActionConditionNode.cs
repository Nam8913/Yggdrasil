using System;

namespace BehaviorTree
{
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

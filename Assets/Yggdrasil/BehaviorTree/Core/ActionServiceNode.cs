using System;

namespace BehaviorTree
{
    /// <summary>
    /// Concrete ServiceNode that wraps a System.Action lambda.
    /// ServiceNode cụ thể bọc một lambda System.Action.
    ///
    /// Usage in builder:
    /// Sử dụng trong builder:
    /// <code>
    /// .Service(0.5f, () => blackboard.Set(BBKeys.CanSeeEnemy, CheckVision()))
    /// </code>
    /// </summary>
    public class ActionServiceNode : ServiceNode
    {
        private readonly Action _serviceAction;

        public ActionServiceNode(Action serviceAction, float interval = 0.5f)
        {
            _serviceAction = serviceAction;
            ServiceInterval = interval;
        }

        protected override void ExecuteService()
        {
            _serviceAction?.Invoke();
        }
    }
}

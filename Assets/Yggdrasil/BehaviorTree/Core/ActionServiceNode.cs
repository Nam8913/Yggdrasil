using System;

namespace BehaviorTree
{
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

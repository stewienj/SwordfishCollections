using System;

namespace Swordfish.NET.Collections.Auxiliary
{
    public class UnthrottledAction : IControlledAction
    {
        private volatile Action _action;

        public bool InvokeAction()
        {
            _action?.Invoke();
            return true;
        }

        public bool InvokeAction(Action action)
        {
            action?.Invoke();
            return true;
        }

        public void SetAction(Action action)
        {
            _action = action;
        }
    }
}

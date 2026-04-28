using System;

namespace Swordfish.NET.Collections.Auxiliary
{
    public interface IControlledAction
    {
        void SetAction(Action action);
        bool InvokeAction();
        bool InvokeAction(Action action);
    }
}

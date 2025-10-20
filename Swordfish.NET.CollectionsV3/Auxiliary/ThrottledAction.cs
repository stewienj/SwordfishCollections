using System;
using System.Collections.Generic;
using System.Text;

namespace Swordfish.NET.Collections.Auxiliary
{
    internal class ThrottledAction : ThrottledActionTaskDelay
    {
        internal ThrottledAction() : this(TimeSpan.FromMilliseconds(20)) { }

        internal ThrottledAction(TimeSpan timeBetweenInvokations) : base(timeBetweenInvokations) { }

        internal ThrottledAction(Action action, TimeSpan timeBetweenInvokations) : base (action, timeBetweenInvokations) { }
    }
}

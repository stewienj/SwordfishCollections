using System.ComponentModel;

namespace Swordfish.NET.Collections.Auxiliary
{
    public class ExtendedPropertyChangedEventArgs : PropertyChangedEventArgs
    {
        public ExtendedPropertyChangedEventArgs(string propertyName, object oldValue, object newValue) : base(propertyName)
        {
            OldValue = oldValue;
            NewValue = newValue;
        }

        public object OldValue { get; }
        public object NewValue { get; }
    }
}

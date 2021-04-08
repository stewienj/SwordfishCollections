using System;
using System.Collections.Generic;
using System.Text;

namespace Swordfish.NET.Collections
{
    public interface IEditableCollection
    {
        void BeginEditingItem();
        void EndedEditingItem();
    }
}

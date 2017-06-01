using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Swordfish.NET.WPF.ViewModel
{
  public class MaxColumnsArgs : EventArgs
  {
    public MaxColumnsArgs(int maxColumns)
    {
      MaxColumns = maxColumns;
    }

    public int MaxColumns
    {
      get;
      private set;
    }
  }
}

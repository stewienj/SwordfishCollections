using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Swordfish.NET.Collections.Auxiliary
{
  public class LazySingleton<T>
  {
    private static Lazy<T> _instance = new Lazy<T>(true);
    public static T Instance
    {
      get
      {
        return _instance.Value;
      }
    }

  }
}

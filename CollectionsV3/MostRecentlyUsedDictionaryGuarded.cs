using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Swordfish.NET.Collections {
  /// <summary>
  /// A variant on MostRecentlyUsedDictionaryGuarded that uses a lock to limit access to 1 thread at a time
  /// </summary>
  public class MostRecentlyUsedDictionaryGuarded<TKey, TValue> : MostRecentlyUsedDictionary<TKey, TValue> {

    public override TValue Add(TKey key, TValue value) {
      lock(this) {
        return base.Add(key, value);
      }
    }

    public override void Clear() {
      lock(this) {
        base.Clear();
      }
    }

    public override bool ContainsKey(TKey key) {
      lock(this) {
        return base.ContainsKey(key);
      }
    }

    public override int Count {
      get {
        lock(this) {
          return base.Count;
        }
      }
    }

    public override IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() {
      lock(this) {
        return base.GetEnumerator();
      }
    }

    public override TValue LeastRecentlyUsedValue {
      get {
        lock(this) {
          return base.LeastRecentlyUsedValue;
        }
      }
    }

    public override bool Remove(TKey key) {
      lock(this) {
        return base.Remove(key);
      }
    }

    public override TValue this[TKey key] {
      get {
        lock(this) {
          return base[key];
        }
      }
      set {
        lock(this) {
          base[key] = value;
        }
      }
    }

    public override void TrimLeastRecentlyUsed() {
      lock(this) {
        base.TrimLeastRecentlyUsed();
      }
    }

    public override bool TryGetValue(TKey key, out TValue value) {
      lock(this) {
        return base.TryGetValue(key, out value);
      }
    }
  }
}

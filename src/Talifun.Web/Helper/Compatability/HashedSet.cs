using System.Collections.Generic;
#if NET35
using System;
using System.Collections;
#endif

namespace Talifun.Web
{
#if !NET35
    // Redirect for 4.0 to native
    class HashedSet<T> : HashSet<T>
    {
        public HashedSet()
        { }

        public HashedSet(IEnumerable<T> initialValues)
            : base(initialValues) { }
    }

    // Redirect to Native implementation
    class HashedCompareSet<T> : HashSet<T>
    {
        public HashedCompareSet(IEnumerable<T> initialValues, IEqualityComparer<T> comparer)
            : base(initialValues, comparer) { }
    }
#else

    /// <summary>
  /// Implements a <c>Set</c> based on a hash table.  This will give the best lookup, add, and remove
  ///             performance for very large data-sets, but iteration will occur in no particular order.
  /// 
  /// </summary>
  [Serializable]
  public class HashedSet : DictionarySet
  {
    /// <summary>
    /// Creates a new set instance based on a hash table.
    /// 
    /// </summary>
    public HashedSet()
    {
      this.InternalDictionary = (IDictionary) new Hashtable();
    }

    /// <summary>
    /// Creates a new set instance based on a hash table and
    ///             initializes it based on a collection of elements.
    /// 
    /// </summary>
    /// <param name="initialValues">A collection of elements that defines the initial set contents.</param>
    public HashedSet(ICollection initialValues)
      : this()
    {
      this.AddAll(initialValues);
    }
  }

     /// <summary>
  /// Implements a <c>Set</c> based on a Dictionary (which is equivalent of
  ///             non-genric <c>HashTable</c>) This will give the best lookup, add, and remove
  ///             performance for very large data-sets, but iteration will occur in no particular order.
  /// 
  /// </summary>
  [Serializable]
  public class HashedSet<T> : DictionarySet<T>
  {
    /// <summary>
    /// Creates a new set instance based on a Dictinary.
    /// 
    /// </summary>
    public HashedSet()
    {
      this.InternalDictionary = (IDictionary<T, object>) new Dictionary<T, object>();
    }

    /// <summary>
    /// Creates a new set instance based on a Dictinary and
    ///             initializes it based on a collection of elements.
    /// 
    /// </summary>
    /// <param name="initialValues">A collection of elements that defines the initial set contents.</param>
    public HashedSet(ICollection<T> initialValues)
      : this()
    {
      this.AddAll(initialValues);
    }
  }

    sealed class HashedCompareSet<T> : DictionarySet<T>
    {
        public HashedCompareSet(ICollection<T> initialValues, IEqualityComparer<T> comparer)
        {
            InternalDictionary = new Dictionary<T, object>(comparer);
            AddAll(initialValues);
        }
    }
#endif
}

#if NET35
using System;
using System.Collections;
using System.Collections.Generic;

namespace Talifun.Web
{
    /// <summary>
    /// <p><c>DictionarySet</c> is an abstract class that supports the creation of new <c>Set</c>
    ///              types where the underlying data store is an <c>IDictionary</c> instance.</p><p>You can use any object that implements the <c>IDictionary</c> interface to hold set data.
    ///              You can define your own, or you can use one of the objects provided in the Framework.
    ///              The type of <c>IDictionary</c> you choose will affect both the performance and the behavior
    ///              of the <c>Set</c> using it. </p><p>To make a <c>Set</c> typed based on your own <c>IDictionary</c>, simply derive a
    ///              new class with a constructor that takes no parameters.  Some <c>Set</c> implmentations
    ///              cannot be defined with a default constructor.  If this is the case for your class,
    ///              you will need to override <c>Clone()</c> as well.</p><p>It is also standard practice that at least one of your constructors takes an <c>ICollection</c> or
    ///              an <c>ISet</c> as an argument.</p>
    /// </summary>
    [Serializable]
    public abstract class DictionarySet : Set
    {
        private static readonly object PlaceholderObject = new object();
        /// <summary>
        /// Provides the storage for elements in the <c>Set</c>, stored as the key-set
        ///             of the <c>IDictionary</c> object.  Set this object in the constructor
        ///             if you create your own <c>Set</c> class.
        /// 
        /// </summary>
        protected IDictionary InternalDictionary;

        /// <summary>
        /// The placeholder object used as the value for the <c>IDictionary</c> instance.
        /// 
        /// </summary>
        /// 
        /// <remarks>
        /// There is a single instance of this object globally, used for all <c>Sets</c>.
        /// 
        /// </remarks>
        protected object Placeholder
        {
            get
            {
                return DictionarySet.PlaceholderObject;
            }
        }

        /// <summary>
        /// Returns <see langword="true"/> if this set contains no elements.
        /// 
        /// </summary>
        public override bool IsEmpty
        {
            get
            {
                return this.InternalDictionary.Count == 0;
            }
        }

        /// <summary>
        /// The number of elements contained in this collection.
        /// 
        /// </summary>
        public override int Count
        {
            get
            {
                return this.InternalDictionary.Count;
            }
        }

        /// <summary>
        /// None of the objects based on <c>DictionarySet</c> are synchronized.  Use the
        ///             <c>SyncRoot</c> property instead.
        /// 
        /// </summary>
        public override bool IsSynchronized
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Returns an object that can be used to synchronize the <c>Set</c> between threads.
        /// 
        /// </summary>
        public override object SyncRoot
        {
            get
            {
                return this.InternalDictionary.SyncRoot;
            }
        }

        static DictionarySet()
        {
        }

        /// <summary>
        /// Adds the specified element to this set if it is not already present.
        /// 
        /// </summary>
        /// <param name="o">The object to add to the set.</param>
        /// <returns>
        /// <see langword="true"/> is the object was added, <see langword="false"/> if it was already present.
        /// </returns>
        public override bool Add(object o)
        {
            if (this.InternalDictionary[o] != null)
                return false;
            this.InternalDictionary.Add(o, DictionarySet.PlaceholderObject);
            return true;
        }

        /// <summary>
        /// Adds all the elements in the specified collection to the set if they are not already present.
        /// 
        /// </summary>
        /// <param name="c">A collection of objects to add to the set.</param>
        /// <returns>
        /// <see langword="true"/> is the set changed as a result of this operation, <see langword="false"/> if not.
        /// </returns>
        public override bool AddAll(ICollection c)
        {
            bool flag = false;
            foreach (object o in (IEnumerable)c)
                flag = flag | this.Add(o);
            return flag;
        }

        /// <summary>
        /// Removes all objects from the set.
        /// 
        /// </summary>
        public override void Clear()
        {
            this.InternalDictionary.Clear();
        }

        /// <summary>
        /// Returns <see langword="true"/> if this set contains the specified element.
        /// 
        /// </summary>
        /// <param name="o">The element to look for.</param>
        /// <returns>
        /// <see langword="true"/> if this set contains the specified element, <see langword="false"/> otherwise.
        /// </returns>
        public override bool Contains(object o)
        {
            return this.InternalDictionary[o] != null;
        }

        /// <summary>
        /// Returns <see langword="true"/> if the set contains all the elements in the specified collection.
        /// 
        /// </summary>
        /// <param name="c">A collection of objects.</param>
        /// <returns>
        /// <see langword="true"/> if the set contains all the elements in the specified collection, <see langword="false"/> otherwise.
        /// </returns>
        public override bool ContainsAll(ICollection c)
        {
            foreach (object o in (IEnumerable)c)
            {
                if (!this.Contains(o))
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Removes the specified element from the set.
        /// 
        /// </summary>
        /// <param name="o">The element to be removed.</param>
        /// <returns>
        /// <see langword="true"/> if the set contained the specified element, <see langword="false"/> otherwise.
        /// </returns>
        public override bool Remove(object o)
        {
            bool flag = this.Contains(o);
            if (flag)
                this.InternalDictionary.Remove(o);
            return flag;
        }

        /// <summary>
        /// Remove all the specified elements from this set, if they exist in this set.
        /// 
        /// </summary>
        /// <param name="c">A collection of elements to remove.</param>
        /// <returns>
        /// <see langword="true"/> if the set was modified as a result of this operation.
        /// </returns>
        public override bool RemoveAll(ICollection c)
        {
            bool flag = false;
            foreach (object o in (IEnumerable)c)
                flag = flag | this.Remove(o);
            return flag;
        }

        /// <summary>
        /// Retains only the elements in this set that are contained in the specified collection.
        /// 
        /// </summary>
        /// <param name="c">Collection that defines the set of elements to be retained.</param>
        /// <returns>
        /// <see langword="true"/> if this set changed as a result of this operation.
        /// </returns>
        public override bool RetainAll(ICollection c)
        {
            Set set1 = (Set)new HybridSet(c);
            Set set2 = (Set)new HybridSet();
            foreach (object o in (Set)this)
            {
                if (!set1.Contains(o))
                    set2.Add(o);
            }
            return this.RemoveAll((ICollection)set2);
        }

        /// <summary>
        /// Copies the elements in the <c>Set</c> to an array.  The type of array needs
        ///             to be compatible with the objects in the <c>Set</c>, obviously.
        /// 
        /// </summary>
        /// <param name="array">An array that will be the target of the copy operation.</param><param name="index">The zero-based index where copying will start.</param>
        public override void CopyTo(Array array, int index)
        {
            this.InternalDictionary.Keys.CopyTo(array, index);
        }

        /// <summary>
        /// Gets an enumerator for the elements in the <c>Set</c>.
        /// 
        /// </summary>
        /// 
        /// <returns>
        /// An <c>IEnumerator</c> over the elements in the <c>Set</c>.
        /// </returns>
        public override IEnumerator GetEnumerator()
        {
            return this.InternalDictionary.Keys.GetEnumerator();
        }
    }

    /// <summary>
    /// <p><c>DictionarySet</c> is an abstract class that supports the creation of new <c>Set</c>
    ///              types where the underlying data store is an <c>IDictionary</c> instance.</p><p>You can use any object that implements the <c>IDictionary</c> interface to hold set data.
    ///              You can define your own, or you can use one of the objects provided in the Framework.
    ///              The type of <c>IDictionary</c> you choose will affect both the performance and the behavior
    ///              of the <c>Set</c> using it. </p><p>To make a <c>Set</c> typed based on your own <c>IDictionary</c>, simply derive a
    ///              new class with a constructor that takes no parameters.  Some <c>Set</c> implmentations
    ///              cannot be defined with a default constructor.  If this is the case for your class,
    ///              you will need to override <c>Clone()</c> as well.</p><p>It is also standard practice that at least one of your constructors takes an <c>ICollection</c> or
    ///              an <c>ISet</c> as an argument.</p>
    /// </summary>
    [Serializable]
    public abstract class DictionarySet<T> : Set<T>
    {
        private static readonly object PlaceholderObject = new object();
        /// <summary>
        /// Provides the storage for elements in the <c>Set</c>, stored as the key-set
        ///             of the <c>IDictionary</c> object.  Set this object in the constructor
        ///             if you create your own <c>Set</c> class.
        /// 
        /// </summary>
        protected IDictionary<T, object> InternalDictionary;

        /// <summary>
        /// The placeholder object used as the value for the <c>IDictionary</c> instance.
        /// 
        /// </summary>
        /// 
        /// <remarks>
        /// There is a single instance of this object globally, used for all <c>Sets</c>.
        /// 
        /// </remarks>
        protected object Placeholder
        {
            get
            {
                return DictionarySet<T>.PlaceholderObject;
            }
        }

        /// <summary>
        /// Returns <see langword="true"/> if this set contains no elements.
        /// 
        /// </summary>
        public override bool IsEmpty
        {
            get
            {
                return this.InternalDictionary.Count == 0;
            }
        }

        /// <summary>
        /// The number of elements contained in this collection.
        /// 
        /// </summary>
        public override int Count
        {
            get
            {
                return this.InternalDictionary.Count;
            }
        }

        /// <summary>
        /// None of the objects based on <c>DictionarySet</c> are synchronized.  Use the
        ///             <c>SyncRoot</c> property instead.
        /// 
        /// </summary>
        public override bool IsSynchronized
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Returns an object that can be used to synchronize the <c>Set</c> between threads.
        /// 
        /// </summary>
        public override object SyncRoot
        {
            get
            {
                return ((ICollection)this.InternalDictionary).SyncRoot;
            }
        }

        /// <summary>
        /// Indicates wether the <c>Set</c> is read-only or not
        /// 
        /// </summary>
        public override bool IsReadOnly
        {
            get
            {
                return this.InternalDictionary.IsReadOnly;
            }
        }

        static DictionarySet()
        {
        }

        /// <summary>
        /// Adds the specified element to this set if it is not already present.
        /// 
        /// </summary>
        /// <param name="o">The <typeparamref name="T"/> to add to the set.</param>
        /// <returns>
        /// <see langword="true"/> is the object was added, <see langword="false"/> if it was already present.
        /// </returns>
        public override bool Add(T o)
        {
            if (this.InternalDictionary.ContainsKey(o))
                return false;
            this.InternalDictionary.Add(o, DictionarySet<T>.PlaceholderObject);
            return true;
        }

        /// <summary>
        /// Adds all the elements in the specified collection to the set if they are not already present.
        /// 
        /// </summary>
        /// <param name="c">A collection of objects to add to the set.</param>
        /// <returns>
        /// <see langword="true"/> is the set changed as a result of this operation, <see langword="false"/> if not.
        /// </returns>
        public override bool AddAll(ICollection<T> c)
        {
            bool flag = false;
            foreach (T o in (IEnumerable<T>)c)
                flag = flag | this.Add(o);
            return flag;
        }

        /// <summary>
        /// Removes all objects from the set.
        /// 
        /// </summary>
        public override void Clear()
        {
            this.InternalDictionary.Clear();
        }

        /// <summary>
        /// Returns <see langword="true"/> if this set contains the specified element.
        /// 
        /// </summary>
        /// <param name="o">The element to look for.</param>
        /// <returns>
        /// <see langword="true"/> if this set contains the specified element, <see langword="false"/> otherwise.
        /// </returns>
        public override bool Contains(T o)
        {
            return this.InternalDictionary.ContainsKey(o);
        }

        /// <summary>
        /// Returns <see langword="true"/> if the set contains all the elements in the specified collection.
        /// 
        /// </summary>
        /// <param name="c">A collection of objects.</param>
        /// <returns>
        /// <see langword="true"/> if the set contains all the elements in the specified collection, <see langword="false"/> otherwise.
        /// </returns>
        public override bool ContainsAll(ICollection<T> c)
        {
            foreach (T o in (IEnumerable<T>)c)
            {
                if (!this.Contains(o))
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Removes the specified element from the set.
        /// 
        /// </summary>
        /// <param name="o">The element to be removed.</param>
        /// <returns>
        /// <see langword="true"/> if the set contained the specified element, <see langword="false"/> otherwise.
        /// </returns>
        public override bool Remove(T o)
        {
            bool flag = this.Contains(o);
            if (flag)
                this.InternalDictionary.Remove(o);
            return flag;
        }

        /// <summary>
        /// Remove all the specified elements from this set, if they exist in this set.
        /// 
        /// </summary>
        /// <param name="c">A collection of elements to remove.</param>
        /// <returns>
        /// <see langword="true"/> if the set was modified as a result of this operation.
        /// </returns>
        public override bool RemoveAll(ICollection<T> c)
        {
            bool flag = false;
            foreach (T o in (IEnumerable<T>)c)
                flag = flag | this.Remove(o);
            return flag;
        }

        /// <summary>
        /// Retains only the elements in this set that are contained in the specified collection.
        /// 
        /// </summary>
        /// <param name="c">Collection that defines the set of elements to be retained.</param>
        /// <returns>
        /// <see langword="true"/> if this set changed as a result of this operation.
        /// </returns>
        public override bool RetainAll(ICollection<T> c)
        {
            Set<T> set1 = (Set<T>)new HashedSet<T>(c);
            Set<T> set2 = (Set<T>)new HashedSet<T>();
            foreach (T o in (Set<T>)this)
            {
                if (!set1.Contains(o))
                    set2.Add(o);
            }
            return this.RemoveAll((ICollection<T>)set2);
        }

        /// <summary>
        /// Copies the elements in the <c>Set</c> to an array of T.  The type of array needs
        ///             to be compatible with the objects in the <c>Set</c>, obviously.
        /// 
        /// </summary>
        /// <param name="array">An array that will be the target of the copy operation.</param><param name="index">The zero-based index where copying will start.</param>
        public override void CopyTo(T[] array, int index)
        {
            this.InternalDictionary.Keys.CopyTo(array, index);
        }

        /// <summary>
        /// Gets an enumerator for the elements in the <c>Set</c>.
        /// 
        /// </summary>
        /// 
        /// <returns>
        /// An <c>IEnumerator</c> over the elements in the <c>Set</c>.
        /// </returns>
        public override IEnumerator<T> GetEnumerator()
        {
            return this.InternalDictionary.Keys.GetEnumerator();
        }

        /// <summary>
        /// Copies the elements in the <c>Set</c> to an array.  The type of array needs
        ///             to be compatible with the objects in the <c>Set</c>, obviously. Needed for
        ///             non-generic ISet methods implementation
        /// 
        /// </summary>
        /// <param name="array">An array that will be the target of the copy operation.</param><param name="index">The zero-based index where copying will start.</param>
        protected override void NonGenericCopyTo(Array array, int index)
        {
            ((ICollection)this.InternalDictionary.Keys).CopyTo(array, index);
        }
    }
}
#endif
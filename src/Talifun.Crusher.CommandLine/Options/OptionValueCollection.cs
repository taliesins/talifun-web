using System;
using System.Collections;
using System.Collections.Generic;

namespace Talifun.Crusher.CommandLine.Options
{
    public class OptionValueCollection : IList, IList<string>
    {
        List<string> values = new List<string>();
        OptionContext c;

        internal OptionValueCollection(OptionContext c)
        {
            this.c = c;
        }

        #region ICollection
        void ICollection.CopyTo(Array array, int index) { (values as ICollection).CopyTo(array, index); }
        bool ICollection.IsSynchronized { get { return (values as ICollection).IsSynchronized; } }
        object ICollection.SyncRoot { get { return (values as ICollection).SyncRoot; } }
        #endregion

        #region ICollection<T>
        public void Add(string item) { values.Add(item); }
        public void Clear() { values.Clear(); }
        public bool Contains(string item) { return values.Contains(item); }
        public void CopyTo(string[] array, int arrayIndex) { values.CopyTo(array, arrayIndex); }
        public bool Remove(string item) { return values.Remove(item); }
        public int Count { get { return values.Count; } }
        public bool IsReadOnly { get { return false; } }
        #endregion

        #region IEnumerable
        IEnumerator IEnumerable.GetEnumerator() { return values.GetEnumerator(); }
        #endregion

        #region IEnumerable<T>
        public IEnumerator<string> GetEnumerator() { return values.GetEnumerator(); }
        #endregion

        #region IList
        int IList.Add(object value) { return (values as IList).Add(value); }
        bool IList.Contains(object value) { return (values as IList).Contains(value); }
        int IList.IndexOf(object value) { return (values as IList).IndexOf(value); }
        void IList.Insert(int index, object value) { (values as IList).Insert(index, value); }
        void IList.Remove(object value) { (values as IList).Remove(value); }
        void IList.RemoveAt(int index) { (values as IList).RemoveAt(index); }
        bool IList.IsFixedSize { get { return false; } }
        object IList.this[int index] { get { return this[index]; } set { (values as IList)[index] = value; } }
        #endregion

        #region IList<T>
        public int IndexOf(string item) { return values.IndexOf(item); }
        public void Insert(int index, string item) { values.Insert(index, item); }
        public void RemoveAt(int index) { values.RemoveAt(index); }

        private void AssertValid(int index)
        {
            if (c.Option == null)
                throw new InvalidOperationException("OptionContext.Option is null.");
            if (index >= c.Option.MaxValueCount)
                throw new ArgumentOutOfRangeException("index");
            if (c.Option.OptionValueType == OptionValueType.Required &&
                    index >= values.Count)
                throw new OptionException(string.Format(
                            c.OptionSet.MessageLocalizer("Missing required value for option '{0}'."), c.OptionName),
                        c.OptionName);
        }

        public string this[int index]
        {
            get
            {
                AssertValid(index);
                return index >= values.Count ? null : values[index];
            }
            set
            {
                values[index] = value;
            }
        }
        #endregion

        public List<string> ToList()
        {
            return new List<string>(values);
        }

        public string[] ToArray()
        {
            return values.ToArray();
        }

        public override string ToString()
        {
            return string.Join(", ", values.ToArray());
        }
    }
}

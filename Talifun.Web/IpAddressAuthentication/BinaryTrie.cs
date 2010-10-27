// Copyright © 2003 by Jeffrey Sax
// All rights reserved.
// http://www.extremeoptimization.com/
// Filename: BinaryTrie.cs
// Description: Classes to represent a binary trie structure
// Last modified: May 28, 2003.

using System;

namespace Talifun.Web.IpAddressAuthentication
{
    /// <summary>
    /// Represents a trie with keys that are binary values of
    /// length up to 32.
    /// </summary>
    public class BinaryTrie<T>
    {
        protected T _defaultValue;

        /// <summary>
        /// Constructs a <see cref="BinaryTrie{T}"/> with a given index length.
        /// </summary>
        /// <param name="defaultValue"></param>
        /// <param name="indexLength">The index length.</param>
        public BinaryTrie(ref T defaultValue, int indexLength)
        {
            _defaultValue = defaultValue;
            Count = 0;
            IndexLength = 0;
            if ((indexLength < 1) || (indexLength > 18))
                throw new ArgumentOutOfRangeException("indexLength");
            IndexLength = indexLength;
            _roots = new BinaryTrieNode<T>[1 << indexLength];
        }

        protected BinaryTrieNode<T>[] _roots;	// Roots of the trie

        /// <summary>
        /// Gets the collection of root <see cref="BinaryTrieNode{T}"/>
        /// objects in this <see cref="BinaryTrie{T}"/>.
        /// </summary>
        protected BinaryTrieNode<T>[] Roots
        {
            get { return _roots; }
        }

        /// <summary>
        /// Gets or sets the number of keys in the trie.
        /// </summary>
        protected int CountInternal
        {
            get { return Count; }
            set { Count = value; }
        }

        /// <summary>
        /// Adds a key with the given index to the trie.
        /// </summary>
        /// <param name="index">The index of the root <see cref="BinaryTrieNode{T}"/>
        /// for the given key value.</param>
        /// <param name="key">An <see cref="int"/> key value.</param>
        /// <param name="keyLength">The length in bits of the significant
        /// portion of the key.</param>
        /// <returns>The <see cref="BinaryTrieNode{T}"/> that was added to the 
        /// trie.</returns>
        protected BinaryTrieNode<T> AddInternal(int index, int key, int keyLength)
        {
            CountInternal++;
            var root = Roots[index];
            return null == root
                       ? (_roots[index] = new BinaryTrieNode<T>(ref _defaultValue, key, keyLength))
                       : root.AddInternal(key, keyLength);
        }

        protected T FindBestMatchInternal(int index, int key)
        {
            var root = _roots[index];
            return null == root ? default(T) : root.FindBestMatch(key).UserData;
        }

        protected T FindExactMatchInternal(int index, int key)
        {
            var root = _roots[index];
            return null == root ? default(T) : root.FindExactMatch(key).UserData;
        }

        /// <summary>
        /// Gets the index length of this <see cref="BinaryTrie{T}"/>.
        /// </summary>
        /// <remarks>The index length indicates the number of bits
        /// that is to be used to preselect the root nodes.
        /// </remarks>
        public int IndexLength { get; private set; }

        /// <summary>
        /// Gets the number of keys in the trie.
        /// </summary>
        public int Count { get; private set; }

        /// <summary>
        /// Adds a node to the trie.
        /// </summary>
        /// <param name="key">An <see cref="int"/> key value.</param>
        /// <param name="keyLength">The length in bits of the significant
        /// portion of the key.</param>
        /// <returns>The <see cref="BinaryTrieNode{T}"/> that was added to the 
        /// trie.</returns>
        public BinaryTrieNode<T> Add(int key, int keyLength)
        {
            var index = key >> (32 - IndexLength);
            return AddInternal(index, key, keyLength);
        }

        public T FindBestMatch(int key)
        {
            var index = key >> (32 - IndexLength);
            return FindBestMatchInternal(index, key);
        }
    }
}

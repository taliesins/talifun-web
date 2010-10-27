// Copyright © 2003 by Jeffrey Sax
// All rights reserved.
// http://www.extremeoptimization.com/
// Filename: BinaryTrie.cs
// Description: Classes to represent a binary trie structure
// Last modified: May 28, 2003.

using System;

namespace Talifun.Web.IpAddressAuthentication
{
    public class BinaryTrieNode<T>
    {
        private static int[] _bit
            = {0x7FFFFFFF, 0x7FFFFFFF,0x40000000,0x20000000,0x10000000,
				  0x8000000,0x4000000,0x2000000,0x1000000,
				  0x800000,0x400000,0x200000,0x100000,
				  0x80000,0x40000,0x20000,0x10000,
				  0x8000,0x4000,0x2000,0x1000,
				  0x800,0x400,0x200,0x100,
				  0x80,0x40,0x20,0x10,
				  0x8,0x4,0x2,0x1,0};

        private int _keyLength;	// Length of the key
        private BinaryTrieNode<T> _zero;	// First child
        private BinaryTrieNode<T> _one;	// Second child
        private T _userData;
        private T _emptyData;

        /// <summary>
        /// Gets or sets the country code for this entry.
        /// </summary>
        public T UserData
        {
            get {
                return IsKey ? _userData : default(T);
            }
            set { _userData = value; }
        }

        public int Key { get; private set; }

        public bool IsKey
        {
            get { return (!ReferenceEquals(_userData, _emptyData)); }
        }

        /// <summary>
        /// Constructs an <see cref="BinaryTrieNode{T}"/> object.
        /// </summary>
        /// <param name="defaultValue"></param>
        /// <param name="key">Key</param>
        /// <param name="keyLength">Length of the key</param>
        internal BinaryTrieNode(ref T defaultValue, int key, int keyLength)
        {
            _emptyData = defaultValue;
            Key = key;
            _keyLength = keyLength;
            _userData = _emptyData;
        }

        /// <summary>
        /// Adds a record to the trie using the internal representation
        /// of an IP address.
        /// </summary>
        internal BinaryTrieNode<T> AddInternal(int key, int keyLength)
        {
            // Find the common key keyLength
            var difference = key ^ Key;
            // We are only interested in matches up to the keyLength...
            var commonKeyLength = Math.Min(_keyLength, keyLength);
            // ...so count down from there.
            while (difference >= _bit[commonKeyLength])
                commonKeyLength--;

            // If the new key length is smaller than the common key length, 
            // or equal but smaller than the current key length,
            // the new key should be the parent of the current node.
            if ((keyLength < commonKeyLength)
                || ((keyLength == commonKeyLength) && (keyLength < _keyLength)))
            {
                // Make a copy that will be the child node.
                var copy = (BinaryTrieNode<T>)MemberwiseClone(); // new BinaryTrieNode(this);
                // Fill in the child references based on the first
                // bit after the common key.
                if ((Key & _bit[keyLength + 1]) != 0)
                {
                    _zero = null;
                    _one = copy;
                }
                else
                {
                    _zero = copy;
                    _one = null;
                }
                Key = key;
                _keyLength = keyLength;
                UserData = _emptyData;
                return this;
            }

            // Do we have a complete match?
            if (commonKeyLength == _keyLength)
            {
                if (keyLength == _keyLength)
                    return this;

                // Yes. Add the key as a child.
                if ((key & _bit[_keyLength + 1]) == 0)
                {
                    // The remainder of the key starts with a zero.
                    // Do we have a child in this position?
                    if (null == _zero)
                        // No. Create one.
                        return _zero = new BinaryTrieNode<T>(ref _emptyData, key, keyLength);
                    else
                        // Yes. Add this key to the child.
                        return _zero.AddInternal(key, keyLength);
                }
                else
                {
                    // The remainder of the key starts with a one.
                    // Do we have a child in this position?
                    if (null == _one)
                        // No. Create one.
                        return _one = new BinaryTrieNode<T>(ref _emptyData, key, keyLength);
                    else
                        // Yes. Add this key to the child.
                        return _one.AddInternal(key, keyLength);
                }
            }
            else
            {
                // No. The match is only partial, so split this node.
                // Make a copy that will be the first child node.
                var copy = (BinaryTrieNode<T>)MemberwiseClone(); // new BinaryTrieNode(this);
                // And create the other child node.
                var newEntry = new BinaryTrieNode<T>(ref _emptyData, key, keyLength);
                // Fill in the child references based on the first
                // bit after the common key.
                if ((Key & _bit[commonKeyLength + 1]) != 0)
                {
                    _zero = newEntry;
                    _one = copy;
                }
                else
                {
                    _zero = copy;
                    _one = newEntry;
                }
                _keyLength = commonKeyLength;
                return newEntry;
            }
        }

        public BinaryTrieNode<T> FindExactMatch(int key)
        {
            if ((key ^ Key) == 0)
                return this;

            // Pick the child to investigate.
            if ((key & _bit[_keyLength + 1]) == 0)
            {
                // If the key matches the child's key, pass on the request.
                if (null != _zero)
                {
                    if ((key ^ _zero.Key) < _bit[_zero._keyLength])
                        return _zero.FindExactMatch(key);
                }
            }
            else
            {
                // If the key matches the child's key, pass on the request.
                if (null != _one)
                {
                    if ((key ^ _one.Key) < _bit[_one._keyLength])
                        return _one.FindExactMatch(key);
                }
            }
            // If we got here, neither child was a match, so the current
            // node is the best match.
            return null;
        }

        /// <summary>
        /// Looks up a key value in the trie.
        /// </summary>
        /// <param name="key">The key to look up.</param>
        /// <returns>The best matching <see cref="BinaryTrieNode{T}"/>
        /// in the trie.</returns>
        public BinaryTrieNode<T> FindBestMatch(int key)
        {
            // Pick the child to investigate.
            if ((key & _bit[_keyLength + 1]) == 0)
            {
                // If the key matches the child's key, pass on the request.
                if (null != _zero)
                {
                    if ((key ^ _zero.Key) < _bit[_zero._keyLength])
                        return _zero.FindBestMatch(key);
                }
            }
            else
            {
                // If the key matches the child's key, pass on the request.
                if (null != _one)
                {
                    if ((key ^ _one.Key) < _bit[_one._keyLength])
                        return _one.FindBestMatch(key);
                }
            }
            // If we got here, neither child was a match, so the current
            // node is the best match.
            return this;
        }
    }
}

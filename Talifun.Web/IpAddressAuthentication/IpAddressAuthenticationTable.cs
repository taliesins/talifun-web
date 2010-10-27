using System;
using System.IO;

namespace Talifun.Web.IpAddressAuthentication
{
    /// <summary>
    /// Represents a trie that can be used to look up the country
    /// corresponding to an IP address.
    /// </summary>
    public class IpAddressAuthorizationTable : BinaryTrie<bool>
    {
        private int _extraNodes = 0;

        static protected int GetKeyLength(int length)
        {
            if (length < 0)
                return 1;
            var keyLength = 33;
            while (length != 0)
            {
                length >>= 1;
                keyLength--;
            }
            return keyLength;
        }

        private int _indexOffset; // Number of bits after index part.

        /// <summary>
        /// Constructs an <see cref="IpAddressAuthorizationTable"/> object.
        /// </summary>
        public IpAddressAuthorizationTable(bool defaultAuthorizationMode, int indexLength)
            : base(ref defaultAuthorizationMode, indexLength)
        {
            _indexOffset = 32 - indexLength;
        }

        /// <summary>
        /// Loads an IP-country database file into the trie.
        /// </summary>
        /// <param name="filename">The path and filename of the file
        /// that holds the database.</param>
        /// <param name="calculateKeyLength">A boolean value that
        /// indicates whether the <em>size</em> field in the database
        /// contains the total length (<strong>true</strong>) or the 
        /// exponent of the length (<strong>false</strong> of the
        /// allocated segment.</param>
        public void LoadStatisticsFile(string filename, bool calculateKeyLength)
        {
            var reader = new StreamReader(filename);
            try
            {
                string record;
                while (null != (record = reader.ReadLine()))
                {
                    var fields = record.Split('|');

                    // Skip if not the right number of fields
                    if (fields.Length != 7)
                        continue;
                    // Skip if not an IPv4 record
                    if (fields[2] != "ipv4")
                        continue;
                    // Skip if header or info line
                    if (fields[1] == "*")
                        continue;

                    var ip = fields[3];

                    var length = int.Parse(fields[4]);

                    // Convert number of available IP's to key length
                    var keyLength = calculateKeyLength ? GetKeyLength(length) : length;

                    // Interning the country strings saves us a little bit of memory.
                    var isAuthorized = bool.Parse(fields[1]);

                    var parts = ip.Split('.');

                    // The first IndexLength bits of the IP address get
                    // to be the index into our table of roots.
                    var indexBase = ((int.Parse(parts[0]) << 8)
                        + int.Parse(parts[1]));
                    var keyBase = (indexBase << 16)
                        + (int.Parse(parts[2]) << 8)
                        + int.Parse(parts[3]);
                    indexBase >>= (_indexOffset - 16);

                    // If the keyLength is less than our IndexLength,
                    // the current record spans multiple root nodes.
                    var count = (1 << (IndexLength - Math.Min(keyLength, IndexLength)));

                    // The key length should be at least the IndexLength.
                    keyLength = Math.Max(keyLength, IndexLength);

                    for (var index = 0; index < count; index++)
                    {
                        // keyBase already contains the indexBase part,
                        // so just add the shifted index.
                        var key = (index << _indexOffset) + keyBase;
                        AddInternal(indexBase + index, key, keyLength).UserData = isAuthorized;
                    }
                    // We want the count to reflect the actual number of 
                    // networks, so remove the duplicates from the count.
                    _extraNodes += count - 1;
                }
            }
            finally
            {
                reader.Close();
            }
        }

        /// <summary>
        /// Gets the total number of entries in the trie.
        /// </summary>
        public int NetworkCodeCount
        {
            get { return Count - _extraNodes; }
        }

        /// <summary>
        /// Attempts to find if a given ip address is authorized.
        /// </summary>
        /// <param name="ipAddress">A <see cref="String"/> value
        /// representing the </param>
        /// <returns>True if authorized; otherwise false.</returns>
        public bool IsAuthorized(string ipAddress)
        {
            var parts = ipAddress.Split('.');

            // The first IndexLength bits form the key into the
            // array of root nodes.
            var indexBase = ((int.Parse(parts[0]) << 8)
                + int.Parse(parts[1]));
            var index = indexBase >> (_indexOffset - 16);

            var root = Roots[index];
            // If we don't have a root, we don't have a value.
            if (null == root)
                return default(bool);

            // Calculate the full key...
            var key = (indexBase << 16)
                + (int.Parse(parts[2]) << 8)
                + int.Parse(parts[3]);
            // ...and look it up.
            return root.FindBestMatch(key).UserData;
        }
    }
}

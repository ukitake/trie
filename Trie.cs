using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;

namespace Whammy
{
    public class Trie
    {
        [StructLayout(LayoutKind.Explicit)]
        protected unsafe struct TrieNode
        {
            public const int SIZE_BYTES = 2 + 1 + 26 * 4; // 107

            public TrieNode(char c)
            {
                Character = c;
                WordMarker = 0;
            }

            [FieldOffset(0)]
            private char Character;

            [FieldOffset(2)]
            private byte WordMarker;

            [FieldOffset(3)]
            private fixed int NextChars[26];

            public int GetNextChar(char c)
            {
                int charPos = Trie.CharToAlphabetPos(c);
                if (charPos < 0 || charPos >= 26)
                {
                    throw new ArgumentOutOfRangeException(nameof(c), "Character out of range.  Only ascii characters are allowed.");
                }

                return NextChars[charPos];
            }

            public void SetNextChar(char c, int index)
            {
                int charPos = Trie.CharToAlphabetPos(c);
                if (charPos < 0 || charPos >= 26)
                {
                    throw new ArgumentOutOfRangeException(nameof(c), "Character out of range.  Only ascii characters are allowed.");
                }

                NextChars[charPos] = index;
            }

            public void SetWordMarker()
            {
                WordMarker = 1;
            }

            public bool HasChild(char c)
            {
                int charPos = Trie.CharToAlphabetPos(c);
                if (charPos < 0 || charPos >= 26)
                {
                    throw new ArgumentOutOfRangeException(nameof(c), "Character out of range.  Only ascii characters are allowed.");
                }

                return NextChars[charPos] > 0;
            }

            public bool IsWord => WordMarker > 0;

            public void Write(BinaryWriter writer)
            {
                writer.Write(Character);
                writer.Write(WordMarker);

                for (int i = 0; i < 26; i++)
                {
                    writer.Write(NextChars[i]);
                }
            }

            public TrieNode Read(BinaryReader reader)
            {
                Character = reader.ReadChar();
                WordMarker = reader.ReadByte();

                for (int i = 0; i < 26; i++)
                {
                    NextChars[i] = reader.ReadInt32();
                }

                return this;
            }
        }

        public static Trie FromFile(string path)
        {
            Trie trie = new Trie();

            using (FileStream file = File.OpenRead(path))
            {
                using (GZipStream gzip = new GZipStream(file, CompressionMode.Decompress))
                {
                    using (MemoryStream memStream = new MemoryStream())
                    {
                        gzip.CopyTo(memStream);
                        using (BinaryReader reader = new BinaryReader(memStream))
                        {
                            memStream.Position = 0;
                            int count = reader.ReadInt32();
                            
                            trie.nodes = new List<TrieNode>();

                            for (int i = 0; i < count; i++)
                            {
                                trie.nodes.Add(new TrieNode());
                                trie.nodes[i] = trie.nodes[i].Read(reader);
                            }
                        }
                    }
                }
            }

            return trie;
        }

        public static void ToFile(Trie trie, string path)
        {
            using (FileStream file = File.OpenWrite(path))
            {
                using (GZipStream gzip = new GZipStream(file, CompressionLevel.Optimal))
                {
                    var bytes = trie.Serialize();
                    gzip.Write(bytes, 0, bytes.Length);
                }
            }
        }

        public static int CharToAlphabetPos(char c) {
            return char.ToUpper(c) - 65;
        }

        private List<TrieNode> nodes = new List<TrieNode>();

        public Trie()
        {
            nodes.Add(new TrieNode());
        }

        public int Size => nodes.Count;
        public int SizeBytes => nodes.Count * TrieNode.SIZE_BYTES;

        public void Insert(string word) {
            int position = 0;

            int currentIdx = 0;
            TrieNode current = nodes[currentIdx];

            foreach (char c in word) 
            {
                if (!current.HasChild(c)) 
                {
                    var newTrieNode = new TrieNode(c);
                    nodes.Add(newTrieNode);
                    current.SetNextChar(c, nodes.Count - 1);
                    nodes[currentIdx] = current;
                    currentIdx = nodes.Count - 1;
                    current = nodes[currentIdx];
                } 
                else 
                {
                    int nodeIndex = current.GetNextChar(c);
                    if (nodeIndex == 0) 
                    {
                        throw new ArithmeticException("This should never happen");
                    }

                    currentIdx = nodeIndex;
                    current = nodes[nodeIndex];
                }
                position++;
                if (position == word.Length)
                {
                    current.SetWordMarker();
                    nodes[currentIdx] = current;
                }
            }
        }

        public void InsertSubstrings(string word)
        {
            for (int i = 0; i < word.Length; i++)
            {
                for (int j = i + 1; j <= word.Length; j++)
                {
                    if (j - i > 0)
                    {
                        Insert(word.Substring(i, j - i));
                    }
                }
            }
        }

        public bool Contains(string word)
        {
            if (word.Length == 0)
            {
                return false;
            }

            TrieNode current = nodes[0];

            for (int i = 0; i < word.Length; i++)
            {
                char c = word[i];

                if (!current.HasChild(c))
                {
                    return false;
                }

                int nextIndex = current.GetNextChar(c);
                if (nextIndex < 1)
                {
                    return false;
                }

                if (i == word.Length - 1)
                {
                    return nodes[nextIndex].IsWord;
                }

                current = nodes[nextIndex];
            }

            return false;
        }

        private byte[] Serialize()
        {
            using (MemoryStream memStream = new MemoryStream())
            using (BinaryWriter writer = new BinaryWriter(memStream))
            {
                writer.Write(nodes.Count);
                foreach (var node in nodes)
                {
                    node.Write(writer);
                }

                return memStream.ToArray();
            }
        }
    }
}

using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using NUnit.Framework;
using Whammy;

namespace Tests
{
    public class Tests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void Test_single_character_word()
        {
            Trie trie = new Trie();
            trie.Insert("a");
            Assert.True(trie.Contains("a"));
        }

        [Test]
        public void Test_A_and_AS()
        {
            Trie trie = new Trie();
            trie.Insert("a");
            trie.Insert("as");
            Assert.True(trie.Contains("a"));
            Assert.True(trie.Contains("as"));
        }

        [Test]
        public void Test_AS_and_not_A()
        {
            Trie trie = new Trie();
            trie.Insert("as");
            Assert.False(trie.Contains("a"));
            Assert.True(trie.Contains("as"));
            Assert.AreEqual(trie.Size, 3);
        }

        [Test]
        public void Test_same_word()
        {
            Trie trie = new Trie();
            trie.Insert("but");
            trie.Insert("but");
            Assert.True(trie.Contains("but"));
            Assert.True(trie.Size == 4);
        }

        [Test]
        public void Test_case_invariant()
        {
            Trie trie = new Trie();
            trie.Insert("A");
            Assert.True(trie.Contains("a"));
        }

        [Test]
        public void Test_insert_substrings_1()
        {
            Trie trie = new Trie();
            trie.InsertSubstrings("abc");
            Assert.True(trie.Contains("a"));
            Assert.True(trie.Contains("ab"));
            Assert.True(trie.Contains("abc"));
            Assert.True(trie.Contains("b"));
            Assert.True(trie.Contains("bc"));
            Assert.True(trie.Contains("c"));
        }

        [Test]
        public void Test_insert_substrings_2()
        {
            Trie trie = new Trie();
            trie.InsertSubstrings("abcd");
            Assert.True(trie.Contains("a"));
            Assert.True(trie.Contains("ab"));
            Assert.True(trie.Contains("abc"));
            Assert.True(trie.Contains("abcd"));
            Assert.True(trie.Contains("b"));
            Assert.True(trie.Contains("bc"));
            Assert.True(trie.Contains("bcd"));
            Assert.True(trie.Contains("c"));
            Assert.True(trie.Contains("cd"));
            Assert.True(trie.Contains("d"));
        }

        private void ReadDictionary(Trie trie)
        {
            using (var file = File.OpenRead("words_alpha.txt"))
            {
                Stopwatch sw = new Stopwatch();
                sw.Start();
                using (var reader = new StreamReader(file))
                {
                    while (!reader.EndOfStream)
                    {
                        var word = reader.ReadLine();
                        trie.Insert(word);
                    }
                }

                sw.Stop();
                Console.WriteLine($"{sw.ElapsedMilliseconds} ms");
            }
        }

        [Test]
        public void Test_load_dictionary()
        {
            Trie trie = new Trie();
            ReadDictionary(trie);

            Assert.True(trie.Contains("abash"));
            Assert.True(trie.Contains("abjectedness"));
            Assert.True(trie.Contains("bounteousness"));
        }

        [Test, Order(1)]
        public void Test_serialization()
        {
            Trie trie = new Trie();
            ReadDictionary(trie);

            Trie.ToFile(trie, "trie.gz");
        }

        [Test, Order(2)]
        public void Test_deserialization()
        {
            Trie trie = Trie.FromFile("trie.gz");

            Assert.True(trie.Contains("abash"));
            Assert.True(trie.Contains("abjectedness"));
            Assert.True(trie.Contains("bounteousness"));

            File.Delete("trie.gz");
        }
    }
}
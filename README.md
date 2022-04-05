# trie
High performance trie data structure written in C# .NET Standard 2.1

### Memory Layout

The trie uses a List<T> which is a dynamically sized array.  The 'T' in this case is a `TrieNode` which is an unsafe struct with an explicit memory layout for compactness.  
  
`TrieNode` - the memory layout of the tree nodes
 ``` 
 _______________________________________________________________________________________________________________________________
|                       |                 |                                                                                     |  
|     character (2)     | word marker (1) |                               next characters (26 * 4)                              |
|_______________________|_________________|_____________________________________________________________________________________|
```

### character (2 bytes)
The `char` for this node
  
### word marker (1 byte)
A bit that marks whether this node marks the end of a word (1) or not (0)
  
### next characters (26 * 4 bytes)
A fixed size array of 26 integers that contains the next node index for each letter in the alphabet after this node.  
  
Asside from the 0th node in the array, which serves as the root of the tree, there are no wasted nodes.  Since `TrieNode` is a `struct`, the `List<TrieNode>` should result in a single contiguous array as the memory footprint of this data structure.  This makes it dead simple to serialize and deserialize.  
  
### API
Example of reading a word list from file into a `Trie`
```
  Trie trie = new Trie();
  using (var file = File.OpenRead("words_alpha.txt"))
  {
      using (var reader = new StreamReader(file))
      {
          while (!reader.EndOfStream)
          {
              var word = reader.ReadLine();
              trie.Insert(word);
          }
      }
  }
  
  if (trie.Contains("word"))
  {
    Console.WriteLine("It works");
  }
```

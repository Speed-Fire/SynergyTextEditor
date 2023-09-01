using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#nullable disable

namespace SynergyTextEditor.Classes.Utilities
{
    public class Trie
    {
        private class TrieNode
        {
            public char Symbol;
            public Dictionary<char, TrieNode> Relations = new();
            public int WordCount = 0;
            public TrieNode() { }
            public TrieNode(char symbol) { Symbol = symbol; }
        }

        private TrieNode root;

        public Trie()
        {
            root = new TrieNode();
        }

        public Trie(IEnumerable<string> words)
        {
            root = new TrieNode();

            foreach (string word in words)
            {
                Insert(word);
            }
        }

        public void Insert(string word)
        {
            var tmp = root;

            foreach (var symb in word)
            {
                TrieNode next;

                if (!tmp.Relations.ContainsKey(symb))
                {
                    next = new TrieNode(symb);
                    tmp.Relations.Add(symb, next);
                }
                else
                {
                    next = tmp.Relations[symb];
                }

                tmp = next;
            }

            tmp.WordCount++;
        }

        public bool IsPrefixExist(string word)
        {
            var tmp = root;

            foreach (var symb in word)
            {
                if (!tmp.Relations.ContainsKey(symb))
                    return false;

                tmp = tmp.Relations[symb];
            }

            return true;
        }

        public bool Search(string word)
        {
            var tmp = root;

            foreach (var symb in word)
            {
                if (!tmp.Relations.ContainsKey(symb))
                    return false;

                tmp = tmp.Relations[symb];
            }

            return tmp.WordCount > 0;
        }

        public bool Delete(string word)
        {
            if (word.Length == 0)
                return false;

            var curNode = root;
            TrieNode lastBranchNode = root;
            char lastBranchChar = word[0];

            if (root.Relations.Count == 0)
                return false;

            foreach (var symb in word)
            {
                if (root.Relations.Count > 1)
                {
                    lastBranchNode = root;
                    lastBranchChar = symb;
                }

                if (!root.Relations.ContainsKey(symb))
                    return false;

                curNode = root.Relations[symb];
            }

            curNode.WordCount--;

            if (curNode.WordCount > 0)
            {
                return true;
            }

            if (curNode.WordCount == 0 && curNode.Relations.Count == 0)
            {
                lastBranchNode.Relations.Remove(lastBranchChar);
                return true;
            }

            return true;
        }
    }
}

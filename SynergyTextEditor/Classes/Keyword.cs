using Microsoft.Extensions.DependencyInjection;
using SynergyTextEditor.Classes.Converters;
using SynergyTextEditor.Classes.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media.Animation;
using System.Xml.Serialization;

#nullable disable

namespace SynergyTextEditor.Classes
{
    public class KeywordLanguage
    {
        public string Name { get; private set; }

        private List<KeywordGroup> keywordGroups;
        private List<string> fileExtensions;

        public IEnumerable<string> FileExtensions => fileExtensions;

        public KeywordLanguage(string name, List<KeywordGroup> keywordGroups, List<string> fileExtensions)
        {
            Name = name;
            this.keywordGroups = keywordGroups;
            this.fileExtensions = fileExtensions;
        }

        public bool IsSpecial(string key)
        {
            foreach (KeywordGroup keywordGroup in keywordGroups.Where(g => g.Type == KeywordGroupType.Special))
            {
                if (keywordGroup.Contains(key))
                    return true;
            }

            return false;
        }

        public bool TryPut(Keyword tag)
        {
            foreach (var group in keywordGroups)
            {
                if (group.TryPut(tag))
                    return true;
            }

            return false;
        }

        public void ApplyStyling()
        {
            foreach (var group in keywordGroups)
            {
                group.ApplyStyling();
            }
        }
    }

    public class KeywordGroup
    {
        private readonly Trie keywords;
        private readonly List<Tuple<DependencyProperty, object>> styles;
        private readonly KeywordGroupType keywordGroupType;

        private List<Keyword> tags = new();

        public KeywordGroupType Type { get { return keywordGroupType; } }

        public KeywordGroup(IEnumerable<string> words, List<Tuple<DependencyProperty, object>> stls, KeywordGroupType keywordGroupType)
        {
            keywords = new Trie(words);
            styles = stls; 
            this.keywordGroupType = keywordGroupType;
        }

        public bool Contains(string word)
        {
            return keywords.Search(word);
        }

        public bool TryPut(Keyword tag)
        {
            if (!keywords.Search(tag.Word))
                return false;

            tags.Add(tag);
            return true;
        }

        public void ApplyStyling()
        {
            foreach (var tag in tags)
            {
                try
                {
                    var range = new TextRange(tag.StartPosition, tag.EndPosition);

                    foreach (var tuple in styles)
                    {
                        range.ApplyPropertyValue(tuple.Item1, tuple.Item2);
                    }
                }
                catch { }
            }

            tags.Clear();
        }
    }

    public struct Keyword
    {
        public TextPointer StartPosition;
        public TextPointer EndPosition;
        public string Word;
    }

    public enum KeywordGroupType
    {
        Normal,
        Special,
        Commentaries
    }

    public class TupleSerializable<T1, T2>
    {
        public T1 Item1;
        public T2 Item2;

        public TupleSerializable() { }

        public TupleSerializable(T1 item1, T2 item2)
        {
            Item1 = item1;
            Item2 = item2;
        }
    }

    public interface IKeywordLanguageLoader
    {
        public KeywordLanguage Load(string path);
    }

    public class KeywordLanguageLoader : IKeywordLanguageLoader
    {
        [Serializable]
        public class KeywordLanguageSerializable
        {
            public string languageName;
            public List<KeywordGroupSerializable> keywordGroups;
            public List<string> fileExtensions;

            [Serializable]
            public class KeywordGroupSerializable
            {
                public KeywordGroupType keywordGroupType;
                public List<string> keywords;
                public List<TupleSerializable<string, string>> styles;
            }
        }

        public KeywordLanguage Load(string path)
        {
            if (!File.Exists(path))
                throw new FileNotFoundException(path);

            KeywordLanguage res = null;
            var formatter = new XmlSerializer(typeof(KeywordLanguageSerializable));

            KeywordLanguageSerializable deser;
            using(var fs = File.OpenRead(path))
            {
                deser = formatter.Deserialize(fs) as KeywordLanguageSerializable;
            }

            var converter = Program.AppHost.Services.GetService<IConverter<TupleSerializable<string, string>, Tuple<DependencyProperty, object>>>();

            var groups = new List<KeywordGroup>();
            foreach (var group in deser.keywordGroups)
            {
                var styles = group.styles.ConvertAll(t => converter.Convert(t));

                groups.Add(new KeywordGroup(group.keywords, styles, group.keywordGroupType));
            }

            res = new KeywordLanguage(deser.languageName, groups, deser.fileExtensions);

            return res;
        }

        public void Save(KeywordLanguageSerializable language, string path)
        {
            var formatter = new XmlSerializer(typeof(KeywordLanguageSerializable));

            using(var fs = File.OpenWrite(path))
            {
                formatter.Serialize(fs, language);
            }
        }
    }
}

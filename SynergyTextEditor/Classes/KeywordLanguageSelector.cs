using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Shapes;
using static SynergyTextEditor.Classes.KeywordLanguageLoader;
using System.Xml.Serialization;
using System.Text.RegularExpressions;

#nullable disable

namespace SynergyTextEditor.Classes
{
    public interface IKeywordLanguageSelector
    {
        KeywordLanguage GetLanguage(string fileExt);
        KeywordLanguage GetLanguageByName(string name);
        bool UploadLanguage(string path, out Exception ex);

        IEnumerable<string> GetAllLanguages();
    }

    internal class KeywordLanguageSelector : IKeywordLanguageSelector
    {
        static readonly string keywordLanguageMapperPath = "\\KeywordLangMapper.xaml";
        static readonly string keywordLanguageDirectory = "\\KeywordLanguages\\";

        private readonly IKeywordLanguageLoader keywordLanguageLoader;
        private KeywordLanguageMapper _keywordLanguageMapper;

        [Serializable]
        private class KeywordLanguageMapper
        {
            public Dictionary<string, string> _extensionDictionary;
            public Dictionary<string, string> _languageNameDictionary;
        }

        internal KeywordLanguageSelector(IKeywordLanguageLoader keywordLanguageLoader)
        {
            this.keywordLanguageLoader = keywordLanguageLoader;
            LoadKeywordLanguageMapper();
        }

        private void LoadKeywordLanguageMapper()
        {
            var formatter = new XmlSerializer(typeof(KeywordLanguageMapper));

            var path = Directory.GetCurrentDirectory() + keywordLanguageMapperPath;

            if (!File.Exists(path))
            {
                CreateBaseKeywordLanguageMapper(formatter);
            }

            using(var fs = File.OpenRead(path))
            {
                _keywordLanguageMapper = formatter.Deserialize(fs) as KeywordLanguageMapper;
            }
        }

        private void SaveKeywordLanguageMapper()
        {
            var formatter = new XmlSerializer(typeof(KeywordLanguageMapper));

            using(var fs = File.OpenWrite(keywordLanguageMapperPath))
            {
                formatter.Serialize(fs, _keywordLanguageMapper);
            }
        }

        private void CreateBaseKeywordLanguageMapper(XmlSerializer formatter)
        {
            var mapper = new KeywordLanguageMapper()
            {
                _extensionDictionary = new()
                {
                    { ".extension", "KeywordLanguagePath" }
                },
                _languageNameDictionary = new()
                {
                    { "testName", "KeywordLanguagePath" }
                }
            };

            var path = Directory.GetCurrentDirectory() + keywordLanguageMapperPath;

            using (var fs = File.OpenWrite(path))
            {
                formatter.Serialize(fs, mapper);
            }
        }

        public KeywordLanguage GetLanguage(string fileExt)
        {
            if (!_keywordLanguageMapper._extensionDictionary.ContainsKey(fileExt))
                return null;

            var languagePath =
                Directory.GetCurrentDirectory() 
                + _keywordLanguageMapper._extensionDictionary[fileExt];

            return keywordLanguageLoader.Load(languagePath);
        }

        public KeywordLanguage GetLanguageByName(string name)
        {
            if (!_keywordLanguageMapper._languageNameDictionary.ContainsKey(name))
                return null;

            var langPath =
                Directory.GetCurrentDirectory()
                + _keywordLanguageMapper._languageNameDictionary[name];

            return keywordLanguageLoader.Load(langPath);
        }

        public bool UploadLanguage(string path, out Exception ex)
        {
            ex = null;

            try
            {
                // Preload language
                var lang = keywordLanguageLoader.Load(path);

                // Check if language is not taken
                if (_keywordLanguageMapper._languageNameDictionary.ContainsKey(lang.Name))
                    throw new Exception($"Specified language name \'{lang.Name}\' is already taken!");

                // Check language file extensions
                var regex = new Regex(@"^[.]\w+$");
                foreach (var ext in lang.FileExtensions)
                {
                    // check if extension is valid
                    if (!regex.IsMatch(ext))
                        throw new Exception($"Specified extension \'{ext}\' is invalid!");

                    // check if extension is unknown
                    if (_keywordLanguageMapper._extensionDictionary.ContainsKey(ext))
                        throw new Exception($"Extension \'{ext}\' is already taken!");
                }

                // local copy path to put into dictionary
                var localCopyPath =
                    keywordLanguageDirectory
                    + System.IO.Path.GetFileName(path);

                // copy path to save a copy
                var copyPath =
                    Directory.GetCurrentDirectory()
                    + localCopyPath;

                // if file already exists, insert guid string at the beginning of file name 
                if (File.Exists(copyPath))
                {
                    localCopyPath =
                        keywordLanguageDirectory
                        + Guid.NewGuid().ToString()
                        + System.IO.Path.GetFileName(path);

                    copyPath =
                        Directory.GetCurrentDirectory()
                        + localCopyPath;
                }

                // Make a copy
                File.Copy(path, copyPath, false);

                // Fill _keywordLanguageMapper with new extensions
                foreach (var ext in lang.FileExtensions)
                {
                    _keywordLanguageMapper._extensionDictionary[ext] = localCopyPath;
                }

                // Add name to language mapping to _languageNameDictionary
                _keywordLanguageMapper._languageNameDictionary[lang.Name] = localCopyPath;

                // Save updated _keywordLanguageMapper
                SaveKeywordLanguageMapper();

                return true;
            }
            catch (Exception _ex)
            {
                ex = _ex;
                return false;
            }
        }

        public IEnumerable<string> GetAllLanguages()
        {
            return _keywordLanguageMapper
                ._languageNameDictionary
                .Keys;
        }
    }
}

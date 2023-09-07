using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.DependencyInjection;
using SynergyTextEditor.Classes.Extensions;
using SynergyTextEditor.Messages;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Threading;



namespace SynergyTextEditor.Classes.SyntaxHighlighting.Highlighters.KeywordHighlighting
{
    public sealed class SequentialKeywordHighlighter :
        KeywordHighlighterBase,
        INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private KeywordLanguage language;
        private KeywordLanguage Language
        {
            get => language;

            set
            {
                language = value;
                OnPropertyChanged(nameof(Language));
            }
        }

        protected override string CurrentLanguageName => Language is null ? null : Language.Name;

        public SequentialKeywordHighlighter(IKeywordLanguageSelector languageSelector) : base(languageSelector)
        {
            PropertyChanged += TextHighlighter_PropertyChanged;
        }

        protected override void Highlight(TextPointer start, TextPointer end)
        {
            // Unsubscribe from TextChanged
            WeakReferenceMessenger.Default.Unregister<TextChangedMessage>(this);

            // clear all text styling in the range
            var range = new TextRange(start, end);
            range.ClearAllProperties();

            if (language != null)
            {
                // Find all keywords in the range
                for (var navigator = start;
                    navigator.CompareTo(end) < 0;
                    navigator = navigator.GetNextContextPosition(LogicalDirection.Forward))
                {
                    var context = navigator.GetPointerContext(LogicalDirection.Backward);

                    if (context == TextPointerContext.ElementStart && navigator.Parent is Run run)
                    {
                        if (run.Text != "")
                            DesignateKeywordsInRun(run);
                    }
                }

                // when all keywords are found, styling them
                language.ApplyStyling();
            }

            // Subscribe to TextChanged
            WeakReferenceMessenger.Default.Register<TextChangedMessage>(this);
        }

        private void DesignateKeywordsInRun(Run run)
        {
            var text = run.Text;

            int endId, startId = endId = 0;

            for (int i = 0; i < text.Length; i++)
            {
                var isSpecial = language.IsSpecial(text[i].ToString());

                // specifying special symbol for coloring
                if (isSpecial)
                {
                    var keyword = new Keyword()
                    {
                        StartPosition = run.ContentStart.GetPositionAtOffset(i, LogicalDirection.Forward),
                        EndPosition = run.ContentStart.GetPositionAtOffset(i + 1, LogicalDirection.Backward),
                        Word = text[i].ToString()
                    };

                    language.TryPut(keyword);
                }

                // specifying other words for coloring
                if (char.IsWhiteSpace(text[i]) || isSpecial)
                {
                    if (i > 0 && !(char.IsWhiteSpace(text[i - 1]) || language.IsSpecial(text[i - 1].ToString())))
                    {
                        endId = i - 1;

                        var word = text.Substring(startId, endId - startId + 1);

                        var keyword = new Keyword()
                        {
                            StartPosition = run.ContentStart.GetPositionAtOffset(startId, LogicalDirection.Forward),
                            EndPosition = run.ContentStart.GetPositionAtOffset(endId + 1, LogicalDirection.Backward),
                            Word = word
                        };

                        language.TryPut(keyword);
                    }

                    startId = i + 1;
                }
            }

            var lastword = text.Substring(startId, text.Length - startId);

            var lkeyword = new Keyword()
            {
                StartPosition = run.ContentStart.GetPositionAtOffset(startId, LogicalDirection.Forward),
                EndPosition = run.ContentStart.GetPositionAtOffset(text.Length, LogicalDirection.Backward),
                Word = lastword
            };

            language.TryPut(lkeyword);
        }

        #region Message handlers

        public override void Receive(FileOpenedMessage message)
        {
            Language = languageSelector.GetLanguage(Path.GetExtension(message.Value));

            FullHighlight();
        }

        public override void Receive(KeywordLanguageUploadedMessage message)
        {
            string filename = WeakReferenceMessenger.Default.Send<OpenedFileNameRequestMessage>();

            Language = languageSelector.GetLanguage(Path.GetExtension(filename));

            FullHighlight();
        }

        public override void Receive(SelectKeywordLanguageMessage message)
        {
            var langname = message.Value;

            Language = languageSelector.GetLanguageByName(langname);

            FullHighlight();
        }

        #endregion

        #region Property changed

        private void TextHighlighter_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(Language):
                    {
                        WeakReferenceMessenger.Default.Send(new KeywordLanguageChangedMessage(""));
                    }
                    break;
            }
        }

        private void OnPropertyChanged(string v)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(v));
        }

        #endregion
    }
}
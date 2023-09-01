using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.DependencyInjection;
using SynergyTextEditor.Classes.Extensions;
using SynergyTextEditor.Messages;
using System;
using System.Collections;
using System.Collections.Generic;
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

#nullable disable

namespace SynergyTextEditor.Classes
{
    public class TextHighlighter : IRecipient<TextChangedMessage>, IRecipient<FileOpenedMessage>
    {
        private readonly RichTextBox rtb;
        private readonly IKeywordLanguageSelector languageSelector;

        private KeywordLanguage language;

        public TextHighlighter(RichTextBox rtb)
        {
            this.rtb = rtb;    
            languageSelector = Program.AppHost.Services.GetService<IKeywordLanguageSelector>();

            WeakReferenceMessenger.Default.RegisterAll(this);

            //language = KeywordLanguageLoader.Load("C:\\Users\\Влад\\Desktop\\CSlangHighlight.xaml");
        }

        public void TextChanged(object sender, TextChangedEventArgs e)
        {
            if (rtb.Document is null)
                return;

            if (e.Changes.Count == 0) return;

            // Get current paragraph (row)
            /// this optimization won't work with multirow text styling (such as commentaries in VS)
            var paragraph = rtb.Document.ContentStart.GetPositionAtOffset(e.Changes.First().Offset).Paragraph;
            if (paragraph is null)
            {
                return;
            }

            Highlight(paragraph.ContentStart, paragraph.ContentEnd);
        }

        public void FullHighlight()
        {
            if (rtb.Document is null)
                return;

            Highlight(rtb.Document.ContentStart, rtb.Document.ContentEnd);
        }

        private object locker = new();

        private void Highlight(TextPointer start, TextPointer end)
        {
            if (language is null)
                return;

            // Unsubscribe from TextChanged
            WeakReferenceMessenger.Default.Unregister<TextChangedMessage>(this);
            
            // clear all text styling in the range
            var range = new TextRange(start, end);
            range.ClearAllProperties();

            List<Keyword> tags = new();

            // Find all keywords in the range
            for (var navigator = start;
                navigator.CompareTo(end) < 0;
                navigator = navigator.GetNextContextPosition(LogicalDirection.Forward))
            {
                var context = navigator.GetPointerContext(LogicalDirection.Backward);

                if (context == TextPointerContext.ElementStart && navigator.Parent is Run run)
                {
                        if (run.Text != "")
                            tags.AddRange(GetTagsFromRun(run));
                }
            }

            // when all keywords are found, styling them
            language.ApplyStyling();

            // Subscribe to TextChanged
            WeakReferenceMessenger.Default.Register<TextChangedMessage>(this);
        }

        private List<Keyword> GetTagsFromRun(Run run)
        {
            var res = new List<Keyword>();
            var text = run.Text;

            int endId, startId = endId = 0;

            for(int i = 0; i < text.Length; i++)
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
                if (char.IsWhiteSpace(text[i]) || /*SeparatorChars.Contains(text[i])*/ isSpecial)
                {
                    if(i > 0 && !(char.IsWhiteSpace(text[i - 1]) || /*SeparatorChars.Contains(text[i - 1])*/language.IsSpecial(text[i - 1].ToString())))
                    {
                        endId = i - 1;

                        var word = text.Substring(startId, endId - startId + 1);

                        //if (IsKeyword(word))
                        //{
                        //    var tag = new Keyword()
                        //    {
                        //        StartPosition = run.ContentStart.GetPositionAtOffset(startId, LogicalDirection.Forward),
                        //        EndPosition = run.ContentStart.GetPositionAtOffset(endId + 1, LogicalDirection.Backward),
                        //        Word = word
                        //    };
                        //    res.Add(tag);
                        //}

                        var keyword = new Keyword()
                        {
                            StartPosition = run.ContentStart.GetPositionAtOffset(startId, LogicalDirection.Forward),
                            EndPosition = run.ContentStart.GetPositionAtOffset(endId + 1, LogicalDirection.Backward),
                            Word = word
                        };

                        language.TryPut(keyword);

                        //res.Add(tag);
                    }

                    startId = i + 1;
                }
            }

            var lastword = text.Substring(startId, text.Length - startId);
            //if (IsKeyword(lastword))
            //{
            //    var tag = new Keyword()
            //    {
            //        StartPosition = run.ContentStart.GetPositionAtOffset(startId, LogicalDirection.Forward),
            //        EndPosition = run.ContentStart.GetPositionAtOffset(text.Length, LogicalDirection.Backward),
            //        Word = lastword
            //    };
            //    res.Add(tag);
            //}

            var lkeyword = new Keyword()
            {
                StartPosition = run.ContentStart.GetPositionAtOffset(startId, LogicalDirection.Forward),
                EndPosition = run.ContentStart.GetPositionAtOffset(text.Length, LogicalDirection.Backward),
                Word = lastword
            };

            language.TryPut(lkeyword);

            return res;
        }

        #region Message handlers

        void IRecipient<TextChangedMessage>.Receive(TextChangedMessage message)
        {
            TextChanged(null, message.Value);
        }

        void IRecipient<FileOpenedMessage>.Receive(FileOpenedMessage message)
        {
            FullHighlight();
        }

        #endregion
    }
}

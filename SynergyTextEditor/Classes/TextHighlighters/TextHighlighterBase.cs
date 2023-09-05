using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.DependencyInjection;
using SynergyTextEditor.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Documents;

#nullable disable

namespace SynergyTextEditor.Classes.TextHighlighters
{
    public abstract class TextHighlighterBase :
        IDisposable,
        IRecipient<TextChangedMessage>,
        IRecipient<FileOpenedMessage>,
        IRecipient<KeywordLanguageUploadedMessage>,
        IRecipient<SelectKeywordLanguageMessage>
    {
        protected RichTextBox rtb;
        protected readonly IKeywordLanguageSelector languageSelector;

        protected abstract string CurrentLanguageName { get; }

        protected TextHighlighterBase(IKeywordLanguageSelector languageSelector)
        {
            this.languageSelector = languageSelector;
        }

        public virtual void Init(RichTextBox rtb)
        {
            this.rtb = rtb;

            WeakReferenceMessenger.Default.RegisterAll(this);

            WeakReferenceMessenger.Default.Register<CurrentLanguageNameRequestMessage>(this, (r, m) =>
            {
                if (CurrentLanguageName == null)
                {
                    m.Reply("");
                }
                else
                {
                    m.Reply(CurrentLanguageName);
                }
            });
        }

        protected void TextChanged(object sender, TextChangedEventArgs e)
        {
            if (rtb.Document is null)
                return;

            if (e.Changes.Count == 0) return;

            foreach (var change in e.Changes)
            {
                PartialHighlight(change.Offset, change.Offset + change.AddedLength);
            }
        }

        protected void PartialHighlight(int start, int end)
        {
            if (rtb.Document is null)
                return;

            // Get changed interval
            /// this optimization won't work with multirow text styling (such as commentaries in VS)

            var paragraphStart = GetParagraphByOffset(start, LogicalDirection.Forward);
            if (paragraphStart is null)
                return;

            var paragraphEnd = GetParagraphByOffset(end, LogicalDirection.Backward);
            if (paragraphEnd is null)
                return;

            Highlight(paragraphStart.ContentStart, paragraphEnd.ContentEnd);
        }

        protected void FullHighlight()
        {
            if (rtb.Document is null)
                return;

            Highlight(rtb.Document.ContentStart, rtb.Document.ContentEnd);
        }

        private Paragraph GetParagraphByOffset(int offset, LogicalDirection direction)
        {
            var tpOffset = rtb.Document.ContentStart.GetPositionAtOffset(offset);

            if (tpOffset is null)
                return null;

            var tpOffsetForwardContext = tpOffset.GetPointerContext(LogicalDirection.Forward);
            var tpOffsetBackwardContext = tpOffset.GetPointerContext(LogicalDirection.Backward);

            var directionalContext =
                direction == LogicalDirection.Forward ? tpOffsetForwardContext : tpOffsetBackwardContext;

            // Check if tpOffset is not at the start or the begin of the text
            if (directionalContext == TextPointerContext.None)
                return null;

            // Get next insertion position, if tpOffset is not one:
            while (tpOffset != null && !tpOffset.IsAtInsertionPosition)
                tpOffset = tpOffset.GetNextInsertionPosition(direction);

            if (tpOffset is null)
                return null;

            return tpOffset.Paragraph;
        }

        protected abstract void Highlight(TextPointer start, TextPointer end);

        public abstract void Dispose();
        public abstract void Receive(TextChangedMessage message);
        public abstract void Receive(FileOpenedMessage message);
        public abstract void Receive(KeywordLanguageUploadedMessage message);
        public abstract void Receive(SelectKeywordLanguageMessage message);
    }
}

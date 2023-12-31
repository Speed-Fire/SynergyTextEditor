﻿using CommunityToolkit.Mvvm.Messaging;
using SynergyTextEditor.Classes.Blockers;
using SynergyTextEditor.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Documents;

namespace SynergyTextEditor.Classes.SyntaxHighlighting.Highlighters
{
    public abstract class SyntaxHighlighter :
        IDisposable,
        IRecipient<TextChangedMessage>
    {
        private bool disposedValue;

        protected RichTextBox rtb;
        protected readonly Blocker blocker;

        public SyntaxHighlighter()
        {
            blocker = new Blocker();
        }

        public virtual void Init(SyntaxHighlighterInitArgs args)
        {
            this.rtb = args.Rtb;

            WeakReferenceMessenger.Default.RegisterAll(this);
        }

        protected void TextChanged(object sender, TextChangedEventArgs e)
        {
            if (blocker.IsBlocked()) return;

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

        public void Receive(TextChangedMessage message)
        {
            TextChanged(null, message.Value);
        }

        #region Disposing

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: освободить управляемое состояние (управляемые объекты)
                }

                // TODO: освободить неуправляемые ресурсы (неуправляемые объекты) и переопределить метод завершения
                // TODO: установить значение NULL для больших полей
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            // Не изменяйте этот код. Разместите код очистки в методе "Dispose(bool disposing)".
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}

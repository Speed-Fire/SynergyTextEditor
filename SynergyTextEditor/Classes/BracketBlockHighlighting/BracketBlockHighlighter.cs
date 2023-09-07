using CommunityToolkit.Mvvm.Messaging;
using SynergyTextEditor.Classes.Workers;
using SynergyTextEditor.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Documents;

namespace SynergyTextEditor.Classes.BracketBlockHighlighting
{
    public class BracketBlockHighlighter
        :
        IRecipient<TextChangedMessage>,
        IDisposable
    {
        private readonly BracketBlockHighlightingWorker worker;

        private RichTextBox rtb;
        private Canvas canvas;

        public BracketBlockHighlighter(BracketBlockHighlightingWorker worker)
        {
            this.worker = worker;

            WeakReferenceMessenger.Default.RegisterAll(this);
        }

        public void Init(RichTextBox rtb, Canvas canvas)
        {
            this.rtb = rtb;
            this.canvas = canvas;

            var args = new BracketBlockHighlightingWorkerArgs(rtb, canvas);

            worker.Run(args, null);
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

        protected void Highlight(TextPointer start, TextPointer end)
        {
            worker.PushInterval(start, end);
        }

        public void Receive(TextChangedMessage message)
        {
            TextChanged(null, message.Value);
        }

        public void Dispose()
        {
            worker.Abort();
            worker.Dispose();
        }
    }
}

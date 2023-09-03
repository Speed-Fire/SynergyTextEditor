using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.DependencyInjection;
using SynergyTextEditor.Classes.Workers;
using SynergyTextEditor.Messages;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Documents;

#nullable disable

namespace SynergyTextEditor.Classes.TextHighlighters
{
    public sealed class ParallelTextHighlighter :
        TextHighlighterBase
    {
        private readonly TextHighlightingWorker highlightingWorker;

        protected override string CurrentLanguageName => highlightingWorker.CurrentLanguageName;

        public ParallelTextHighlighter(RichTextBox rtb) : base(rtb)
        {
            highlightingWorker = Program.AppHost.Services.GetService<TextHighlightingWorker>();

            highlightingWorker.Run(new TextHighlightingWorkerArgs()
            {
                rtb = rtb,
                listener = this
            });
        }

        protected override void Highlight(TextPointer start, TextPointer end)
        {
            highlightingWorker.PushInterval(start, end);
        }

        #region Message handlers

        public override void Receive(TextChangedMessage message)
        {
            TextChanged(null, message.Value);
        }

        public override void Receive(FileOpenedMessage message)
        {
            highlightingWorker.Suspend();
            highlightingWorker.ClearIntervals();

            highlightingWorker
                .SetLanguage(languageSelector.GetLanguage(Path.GetExtension(message.Value)));

            FullHighlight();

            highlightingWorker.Resume();
        }

        public override void Receive(KeywordLanguageUploadedMessage message)
        {
            WeakReferenceMessenger.Default.Send(new BlockTextEditorChangeStateMessage(true));

            highlightingWorker.ClearIntervals();

            string filename = WeakReferenceMessenger.Default.Send<OpenedFileNameRequestMessage>();
            highlightingWorker
                .SetLanguage(languageSelector.GetLanguage(Path.GetExtension(filename)));

            FullHighlight();

            WeakReferenceMessenger.Default.Send(new BlockTextEditorChangeStateMessage(false));
        }

        public override void Receive(SelectKeywordLanguageMessage message)
        {
            WeakReferenceMessenger.Default.Send(new BlockTextEditorChangeStateMessage(true));

            highlightingWorker.ClearIntervals();

            var langname = message.Value;
            highlightingWorker
                .SetLanguage(languageSelector.GetLanguageByName(langname));

            FullHighlight();

            WeakReferenceMessenger.Default.Send(new BlockTextEditorChangeStateMessage(false));
        }

        #endregion

        public override void Dispose()
        {
            highlightingWorker.Abort();
        }
    }
}

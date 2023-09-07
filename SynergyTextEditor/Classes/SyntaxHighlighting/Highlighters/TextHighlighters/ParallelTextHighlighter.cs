using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.DependencyInjection;
using SynergyTextEditor.Classes.Workers;
using SynergyTextEditor.Messages;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Documents;



namespace SynergyTextEditor.Classes.SyntaxHighlighting.Highlighters.TextHighlighters
{
    public sealed class ParallelTextHighlighter :
        TextHighlighterBase
    {
        private readonly TextHighlightingWorker highlightingWorker;

        protected override string CurrentLanguageName => highlightingWorker.CurrentLanguageName;

        public ParallelTextHighlighter(IKeywordLanguageSelector languageSelector,
            TextHighlightingWorker highlightingWorker) 
            :
            base(languageSelector)
        {
            this.highlightingWorker = highlightingWorker;
        }

        public override void Init(SyntaxHighlighterInitArgs args)
        {
            base.Init(args);

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
            highlightingWorker.ClearIntervals();

            string filename = WeakReferenceMessenger.Default.Send<OpenedFileNameRequestMessage>();
            highlightingWorker
                .SetLanguage(languageSelector.GetLanguage(Path.GetExtension(filename)));

            FullHighlight();
        }

        public override void Receive(SelectKeywordLanguageMessage message)
        {
            highlightingWorker.ClearIntervals();

            var langname = message.Value;
            highlightingWorker
                .SetLanguage(languageSelector.GetLanguageByName(langname));

            FullHighlight();
        }

        #endregion

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            highlightingWorker.Abort();
            highlightingWorker.Dispose();
        }
    }
}

using CommunityToolkit.Mvvm.Messaging;
using SynergyTextEditor.Classes.Structs;
using SynergyTextEditor.Messages;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Documents;



namespace SynergyTextEditor.Classes.Workers
{
    public class KeywordHighlightingWorkerArgs : IWorkerArgs
    {
        public RichTextBox rtb;
    }

    public class KeywordHighlightingWorker : 
        BaseWorker,
        INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private readonly ConcurrentQueue<HighlightingInterval> _changedIntervals = new();

        private RichTextBox _rtb;

        private KeywordLanguage _language = null;
        private KeywordLanguage Language
        {
            get => _language;

            set
            {
                _language = value;
                OnPropertyChanged(nameof(Language));
            }
        }

        public string CurrentLanguageName => _language is null ? null : _language.Name;

        public KeywordHighlightingWorker() 
            :
            base("Text highlighting worker", true)
        {
            PropertyChanged += TextHighlightingWorker_PropertyChanged;
        }

        protected override void DoWork(IWorkerArgs args)
        {
            if (!_changedIntervals.IsEmpty)
            {
                if (_changedIntervals.TryDequeue(out HighlightingInterval interval))
                {
                    Highlight(interval.Start, interval.End);
                }
            }
        }

        #region Interval collection

        public void PushInterval(TextPointer intervalStart, TextPointer intervalEnd)
        {
            PushInterval(new HighlightingInterval() { Start = intervalStart, End = intervalEnd });
        }

        public void PushInterval(HighlightingInterval interval)
        {
            _changedIntervals.Enqueue(interval);
        }

        public void ClearIntervals()
        {
            _changedIntervals.Clear();
        }

        #endregion

        #region Settings

        public void SetLanguage(KeywordLanguage language)
        {
            var wasRunning = false;

            if (WorkerState != System.Threading.ThreadState.Unstarted ||
               WorkerState != System.Threading.ThreadState.Stopped ||
               !IsSuspended)
            {
                try
                {
                    SuspendWait();

                    wasRunning = true;
                }
                catch { }
            }

            Language = language;

            if (wasRunning)
            {
                Resume();
            }
        }

        protected override void ApplyInitializationArgs(IWorkerArgs initializationArgs)
        {
            var args = initializationArgs as KeywordHighlightingWorkerArgs;

            _rtb = args.rtb;
        }

        #endregion

        #region Highlighting

        private void Highlight(TextPointer start, TextPointer end)
        {
            // Unsubscribe from TextChanged
            WeakReferenceMessenger.Default.Send(new StopTextChangedMessage(true));

            // clear all text styling in the range
            App.Current.Dispatcher?.Invoke(() =>
            {
                var range = new TextRange(start, end);
                range.ClearAllProperties();
            });

            if (_language != null)
            {
                // Find all keywords in the range
                for (var navigator = start;
                    navigator.CompareTo(end) < 0;)
                {
                    var context = navigator.GetPointerContext(LogicalDirection.Backward);

                    if (context == TextPointerContext.ElementStart && navigator.Parent is Run run)
                    {
                        bool isEquals = false;

                        App.Current.Dispatcher?.Invoke(() =>
                        {
                            isEquals = run.Text != "";
                        });

                        if (isEquals)
                            DesignateKeywordsInRun(run);
                    }

                    App.Current.Dispatcher?.Invoke(() =>
                    {
                        navigator = navigator.GetNextContextPosition(LogicalDirection.Forward);
                    });
                }

                // when all keywords are found, styling them

                App.Current.Dispatcher?.Invoke(_language.ApplyStyling);
            }

            // Subscribe to TextChanged
            WeakReferenceMessenger.Default.Send(new StopTextChangedMessage(false));
        }

        private void DesignateKeywordsInRun(Run run)
        {
            string text = "";

            App.Current.Dispatcher?.Invoke(() =>
            {
                text = run.Text;
            });

            int endId, startId = endId = 0;

            for (int i = 0; i < text.Length; i++)
            {
                var isSpecial = _language.IsSpecial(text[i].ToString());

                // specifying special symbol for coloring
                if (isSpecial)
                {
                    var keyword = new Keyword();
                    App.Current.Dispatcher.Invoke(() =>
                    {
                        keyword.StartPosition = run.ContentStart.GetPositionAtOffset(i, LogicalDirection.Forward);
                        keyword.EndPosition = run.ContentStart.GetPositionAtOffset(i + 1, LogicalDirection.Backward);
                        keyword.Word = text[i].ToString();
                    });

                    _language.TryPut(keyword);
                }

                // specifying other words for coloring
                if (char.IsWhiteSpace(text[i]) || isSpecial)
                {
                    if (i > 0 && !(char.IsWhiteSpace(text[i - 1]) || _language.IsSpecial(text[i - 1].ToString())))
                    {
                        endId = i - 1;

                        var word = text.Substring(startId, endId - startId + 1);

                        var keyword = new Keyword();

                        App.Current.Dispatcher.Invoke(() =>
                        {
                            keyword.StartPosition = run.ContentStart.GetPositionAtOffset(startId, LogicalDirection.Forward);
                            keyword.EndPosition = run.ContentStart.GetPositionAtOffset(endId + 1, LogicalDirection.Backward);
                            keyword.Word = word;
                        });

                        _language.TryPut(keyword);
                    }

                    startId = i + 1;
                }
            }

            var lastword = text.Substring(startId, text.Length - startId);

            var lkeyword = new Keyword();
            App.Current.Dispatcher.Invoke(() =>
            {
                lkeyword.StartPosition = run.ContentStart.GetPositionAtOffset(startId, LogicalDirection.Forward);
                lkeyword.EndPosition = run.ContentStart.GetPositionAtOffset(text.Length, LogicalDirection.Backward);
                lkeyword.Word = lastword;
            });

            _language.TryPut(lkeyword);
        }

        #endregion

        #region Property changed

        private void TextHighlightingWorker_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(Language):
                    {
                        WeakReferenceMessenger.Default.Send(new KeywordLanguageChangedMessage(""));
                    }
                    break;
                default:
                    break;
            }
        }

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}
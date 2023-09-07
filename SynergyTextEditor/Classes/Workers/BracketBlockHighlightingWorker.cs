using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Windows.Shapes;
using SynergyTextEditor.Classes.Structs;

namespace SynergyTextEditor.Classes.Workers
{
    public class BracketBlockHighlightingWorkerArgs : IWorkerArgs
    {
        public RichTextBox Rtb { get; }
        public Canvas Canvas { get; }

        public BracketBlockHighlightingWorkerArgs(RichTextBox rtb, Canvas canvas)
        {
            Rtb = rtb;
            Canvas = canvas;
        }
    }

    public class BracketBlockHighlightingWorker : BaseWorker
    {
        #region Embedded classes

        private enum BracketType
        {
            Open,
            Close
        }

        private class Bracket
        {
            public TextPointer Position;
            public BracketType Type;
        }

        #endregion

        private static readonly string _workerName = "Bracket highlighting worker";

        private readonly ConcurrentQueue<HighlightingInterval> _changedIntervals = new();

        private readonly LinkedList<Bracket> _brackets = new();
        private readonly HashSet<Paragraph> _knownParagraphs = new();

        private RichTextBox _rtb;
        private Canvas _canvas;

        private volatile int _isScrolled = 0;

        public BracketBlockHighlightingWorker() : base(_workerName, true)
        {

        }

        protected override void ApplyInitializationArgs(IWorkerArgs initializationArgs)
        {
            if(initializationArgs is null ||
               !(initializationArgs is BracketBlockHighlightingWorkerArgs))
                throw new ArgumentException(nameof(initializationArgs));

            var args = initializationArgs as BracketBlockHighlightingWorkerArgs;

            _rtb = args.Rtb;
            _canvas = args.Canvas;

            _rtb.AddHandler(ScrollViewer.ScrollChangedEvent, new ScrollChangedEventHandler(OnRtbScrollChanged));
        }

        protected override void DoWork(IWorkerArgs args)
        {
            var hasChanged = false;

            while (!_changedIntervals.IsEmpty)
            {
                if (IsSuspended)
                    return;

                ClearAllInvalidBrackets();

                if (_changedIntervals.TryDequeue(out HighlightingInterval interval))
                {
                    UpdateBracketList(interval.Start, interval.End);
                    hasChanged = true;
                }
            }

            if (IsSuspended)
                return;

            if (hasChanged || Interlocked.And(ref _isScrolled, 1) == 1)
            {
                RedrawBracketBlockLines();

                Interlocked.Decrement(ref _isScrolled);
            }
        }

        #region Bracket lines calculation

        private void UpdateBracketList(TextPointer start, TextPointer end)
        {
            //Find all brackets in the range
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
                    {
                        RemoveAllKnownBracketsOnParagraph(navigator.Paragraph);
                        FindBracketsInRun(run, navigator.Paragraph);
                    }
                }

                App.Current.Dispatcher?.Invoke(() =>
                {
                    navigator = navigator.GetNextContextPosition(LogicalDirection.Forward);
                });
            }
        }

        private void FindBracketsInRun(Run run, Paragraph currentLine)
        {
            string text = "";

            App.Current.Dispatcher?.Invoke(() =>
            {
                text = run.Text;
            });

            for (int i = 0; i < text.Length; i++)
            {
                if (text[i] == '{')
                {
                    var pos = run.ContentStart.GetPositionAtOffset(i, LogicalDirection.Forward);

                    InsertBracket(pos, BracketType.Open);
                    _knownParagraphs.Add(currentLine);
                }
                else if (text[i] == '}')
                {
                    var pos = run.ContentStart.GetPositionAtOffset(i, LogicalDirection.Forward);

                    InsertBracket(pos, BracketType.Close);
                    _knownParagraphs.Add(currentLine);
                }
            }
        }

        private void ClearAllInvalidBrackets()
        {
            var toRem = new List<LinkedListNode<Bracket>>();
            var paragraphs = new List<Paragraph>();

            for(var bracket = _brackets.First; bracket != null; bracket = bracket.Next)
            {
                bool check = false;
                bool check2 = false;
                bool check3 = false;

                App.Current.Dispatcher?.Invoke(() =>
                {
                    check = bracket.Value.Position.Paragraph == null;
                    check2 = !bracket.Value.Position.IsInSameDocument(_rtb.Document.ContentStart);
                    check3 = !bracket.Value.Position.IsAtInsertionPosition;
                });

                if (check ||
                    check2 ||
                    check3)
                {
                    toRem.Add(bracket);
                    continue;
                }

                TextPointer nextPointer = null;

                App.Current.Dispatcher?.Invoke(() =>
                {
                    nextPointer = bracket.Value.Position.GetNextContextPosition(LogicalDirection.Forward);
                });

                if (nextPointer is null)
                {
                    toRem.Add(bracket);
                    continue;
                }

                bool isEmpty = false;
                string text = "";

                App.Current.Dispatcher?.Invoke(() =>
                {
                    var range = new TextRange(bracket.Value.Position, nextPointer);
                    isEmpty = range.IsEmpty;
                    text = range.Text;
                });

                if (isEmpty ||
                       bracket.Value.Type == BracketType.Open &&
                       text[0] != '{' ||
                       bracket.Value.Type == BracketType.Close &&
                       text[0] != '}')
                {
                    toRem.Add(bracket);
                    continue;
                }

                // if bracket will not be removed, then specify its paragraph as still known
                paragraphs.Add(bracket.Value.Position.Paragraph);
            }

            // remove invalid brackets
            foreach(var bracket in toRem)
            {
                if (bracket.List == null)
                    continue;

                _brackets.Remove(bracket);
            }

            // remove all unknown paragraphs
            _knownParagraphs.IntersectWith(paragraphs);
        }

        private void InsertBracket(TextPointer pos, BracketType type)
        {
            var bracket = new Bracket()
            {
                Position = pos,
                Type = type
            };

            if(_brackets.Count == 0)
            {
                _brackets.AddFirst(bracket);
                return;
            }

            if (_brackets.First.Value.Position.CompareTo(pos) == 0 ||
            _brackets.Last.Value.Position.CompareTo(pos) == 0)
                return;

            if (IsTextPointerBetween(pos, _rtb.Document.ContentStart, _brackets.First.Value.Position))
            {
                _brackets.AddFirst(bracket);
                return;
            }

            if(_brackets.Count > 1)
            {
                var start = _brackets.First;
                var next = start.Next;

                while(next != null)
                {
                    if (start.Value.Position.CompareTo(pos) == 0)
                        return;

                    if(IsTextPointerBetween(pos, start.Value.Position, next.Value.Position))
                    {
                        _brackets.AddAfter(start, bracket);
                        return;
                    }

                    start = next;
                    next = next.Next;
                }
            }

            if (IsTextPointerBetween(pos, _brackets.Last.Value.Position, _rtb.Document.ContentEnd))
            {
                _brackets.AddLast(bracket);
                return;
            }
        }

        private void RemoveAllKnownBracketsOnParagraph(Paragraph paragraph)
        {
            if (paragraph is null)
                return;

            if (!_knownParagraphs.Contains(paragraph))
                return;

            var toRem = new List<LinkedListNode<Bracket>>();
            var groupFound = false;

            for(var navigator = _brackets.First;  navigator != null; navigator = navigator.Next)
            {
                var check = navigator.Value.Position.Paragraph.Equals(paragraph);

                if (check)
                {
                    toRem.Add(navigator);
                    groupFound = true;
                }
                else if (groupFound)
                    break;
            }

            foreach(var bracket in toRem)
            {
                _brackets.Remove(bracket);
            }

            _knownParagraphs.Remove(paragraph);
        }

        private bool IsTextPointerBetween(TextPointer probe, TextPointer start, TextPointer end)
        {
            return start.CompareTo(probe) <= 0 && probe.CompareTo(end) <= 0;
        }

        #endregion

        #region Line drawing

        private void RedrawBracketBlockLines()
        {
            var bracketStack = new Stack<Bracket>();

            App.Current.Dispatcher?.Invoke(() =>
            {
                _canvas.Children.Clear();
            });

            for (var navigator = _brackets.First; navigator != null; navigator = navigator.Next)
            {
                if (navigator.Value.Type == BracketType.Open)
                {
                    bracketStack.Push(navigator.Value);
                }
                else
                {
                    if (bracketStack.Count == 0)
                        continue;

                    var openBracket = bracketStack.Pop();
                    var closeBracket = navigator.Value;

                    RedrawBracketBlock(openBracket, closeBracket);
                }
            }
        }

        private void RedrawBracketBlock(Bracket start, Bracket end)
        {
            if (start.Position.Paragraph is null ||
               end.Position.Paragraph is null)
                return;

            if (start.Position.Paragraph == end.Position.Paragraph)
                return;

            Rect rectStart, rectEnd;

            App.Current.Dispatcher?.Invoke(() =>
            {
                rectStart = start.Position.GetCharacterRect(LogicalDirection.Forward);
                rectEnd = end.Position.GetCharacterRect(LogicalDirection.Forward);

                if (rectEnd.Y < 0 || rectStart.Y > _rtb.ActualHeight)
                    return;

                var line = new Line()
                {
                    X1 = 0,
                    Y1 = 0,
                    X2 = 0,
                    Y2 = rectEnd.Y - (rectStart.Y + rectStart.Height),
                    Stroke = new SolidColorBrush(Colors.LightGray),
                    StrokeDashArray = { 4, 3 }
                };

                line.SetValue(Canvas.TopProperty, rectStart.Y + rectStart.Height);
                line.SetValue(Canvas.LeftProperty, rectStart.X + GetCharWidth() / 2);

                _canvas.Children.Add(line);
            });

        }

        private double GetCharWidth()
        {
            var formattedText = new FormattedText(
           "{",
           CultureInfo.CurrentCulture,
           FlowDirection.LeftToRight,
           new Typeface(_rtb.FontFamily, _rtb.FontStyle, _rtb.FontWeight, _rtb.FontStretch),
           _rtb.FontSize,
           Brushes.Black,
           new NumberSubstitution(),
           1);

            return formattedText.Width;
        }

        public void ClearBracketList()
        {
            _brackets.Clear();
            _knownParagraphs.Clear();
        }

        #endregion

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

        private void OnRtbScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            //RedrawBracketBlockLines();
            Interlocked.Exchange(ref _isScrolled, 1);
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            _rtb.RemoveHandler(ScrollViewer.ScrollChangedEvent, new ScrollChangedEventHandler(OnRtbScrollChanged));
        }
    }
}

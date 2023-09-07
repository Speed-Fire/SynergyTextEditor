using CommunityToolkit.Mvvm.Messaging;
using SynergyTextEditor.Classes.Extensions;
using SynergyTextEditor.Classes.Helpers;
using SynergyTextEditor.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;



namespace SynergyTextEditor.Classes
{
    public class LineNumerator : 
        IRecipient<FileOpenedMessage>,
        IRecipient<ThemeChangedMessage>,
        IDisposable
    {
        private readonly RichTextBox _lineNumbers;
        private readonly RichTextBox _inputText;

        private readonly Dictionary<int, double> _widthDictionary = new();

        private const int _widthStep = 10; // 25 per 2 ciphers
        private const int _minWidth = 28;  // 35 per 3 ciphers
        private const int _maxWidth = 55;  // 45 per 4 ciphers

        private int _lineCount;

        private Block _highlightedLineNumber;

        public LineNumerator(RichTextBox lineNumbers, RichTextBox inputText)
        {
            _lineNumbers = lineNumbers;
            _inputText = inputText;

            InitWidthDictionary();
            StartupInitialization();

            WeakReferenceMessenger.Default.RegisterAll(this);

            var width = SymbolHelper.GetStringWidth(_inputText, "28");

            _inputText.SelectionChanged += _inputText_SelectionChanged;
            _inputText.TextChanged += _inputText_TextChanged;
        }

        private void InitWidthDictionary()
        {
            var width = double.Ceiling(SymbolHelper.GetCharWidth(_inputText, '2'));

            var first = width * 4;

            _lineNumbers.MaxWidth = first;

            _widthDictionary.Add(2, first);
            _widthDictionary.Add(3, first + width);
            _widthDictionary.Add(4, first + width * 2);
            _widthDictionary.Add(5, first + width * 3);
        }

        private void StartupInitialization()
        {
            if (_lineNumbers.Document == null)
                _lineNumbers.Document = new FlowDocument();

            _lineCount = 0;

            _lineNumbers.Document.Blocks.Clear();

            if (_inputText.Document == null)
            {
                _inputText.Document = new FlowDocument();
                _inputText.Document.Blocks.Clear();
                _inputText.Document.Blocks.Add(new Paragraph());
            }

            _lineCount = _inputText.Document.Blocks.Count;

            for (int i = 0; i < _lineCount; i++)
            {
                _lineNumbers.Document.Blocks.Add(new Paragraph(new Run((i + 1).ToString())));
            }

            _highlightedLineNumber = _lineNumbers.Document.Blocks.FirstBlock;
        }

        #region Highlighting block with caret

        private void HighlightCaretBlockNumber()
        {
            var blockId = GetCaretBlockId();

            var range = new TextRange(_highlightedLineNumber.ContentStart, _highlightedLineNumber.ContentEnd);
            range.ClearAllProperties();

            _highlightedLineNumber = _lineNumbers.Document.Blocks.ElementAt(blockId);

            ApplyCaretLineNumberHighlighting();
        }

        private void ApplyCaretLineNumberHighlighting()
        {
            var brush = App.Current.FindResource("RichTextBox.Foreground") as SolidColorBrush;

            var range = new TextRange(_highlightedLineNumber.ContentStart, _highlightedLineNumber.ContentEnd);
            range.ApplyPropertyValue(TextElement.ForegroundProperty, brush);
        }

        int GetCaretBlockId()
        {
            var currentBlock = (Block)_inputText.CaretPosition.Paragraph;

            if (currentBlock is null)
                currentBlock = _inputText.Document.Blocks.LastBlock;

            var blockId = 0;
            var blockItr = _inputText.Document.Blocks.FirstBlock;
            for (; blockId < _inputText.Document.Blocks.Count && blockItr != null;
                blockId++, blockItr = blockItr.NextBlock)
            {
                if (currentBlock.Equals(blockItr))
                    break;
            }

            return blockId;
        }

        #endregion

        #region Line numerating

        private void UpdateLineNumbers()
        {
            if (_lineCount == _inputText.Document.Blocks.Count)
                return;

            if (_lineCount < _inputText.Document.Blocks.Count)
            {
                AddLineNumbers();
            }

            if (_lineCount > _inputText.Document.Blocks.Count)
            {
                RemoveLineNumbers();
            }

            UpdateLineNumbersWidth();
        }

        private void RemoveLineNumbers()
        {
            var toRemove = new List<Block>();

            var itr = _lineNumbers.Document.Blocks.LastBlock;
            for (int i = 0; i < _lineCount - _inputText.Document.Blocks.Count; i++)
            {
                toRemove.Add(itr);
                itr = itr.PreviousBlock;
            }

            foreach (var block in toRemove)
            {
                _lineNumbers.Document.Blocks.Remove(block);
            }

            _lineCount = _inputText.Document.Blocks.Count;

            if(_lineCount == 0)
            {
                _lineCount = 1;
                _lineNumbers.Document.Blocks.Add(new Paragraph(new Run("1")));
            }
        }

        private void AddLineNumbers()
        {
            for (int i = _lineCount; i < _inputText.Document.Blocks.Count; i++)
            {
                _lineNumbers.Document.Blocks.Add(new Paragraph(new Run((i + 1).ToString())));
            }

            _lineCount = _inputText.Document.Blocks.Count;
        }

        private void UpdateLineNumbersWidth()
        {
            var count = _lineCount.GetCipherCount();
            count = Math.Min(Math.Max(count, 2), 5);

            _lineNumbers.MaxWidth = _widthDictionary[count];
        }

        #endregion

        #region Event handlers

        private void _inputText_SelectionChanged(object sender, System.Windows.RoutedEventArgs e)
        {
            HighlightCaretBlockNumber();
        }

        private void _inputText_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateLineNumbers();
        }

        #endregion

        #region Message handlers

        void IRecipient<FileOpenedMessage>.Receive(FileOpenedMessage message)
        {
            HighlightCaretBlockNumber();
        }

        public void Receive(ThemeChangedMessage message)
        {
            ApplyCaretLineNumberHighlighting();
        }

        #endregion

        public void Dispose()
        {
            _inputText.SelectionChanged -= _inputText_SelectionChanged;
            _inputText.TextChanged -= _inputText_TextChanged;
        }
    }
}

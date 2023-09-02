using SynergyTextEditor.Classes.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Documents;

#nullable disable

namespace SynergyTextEditor.Classes
{
    public class LineNumerator
    {
        private readonly RichTextBox _lineNumbers;
        private readonly RichTextBox _inputText;

        private readonly Dictionary<int, double> _widthDictionary = new()
        {
            { 2, 28},
            { 3, 35},
            { 4, 45},
            { 5, 55}
        };

        private const int _widthStep = 10; // 25 per 2 ciphers
        private const int _minWidth = 28;  // 35 per 3 ciphers
        private const int _maxWidth = 55;  // 45 per 4 ciphers

        private int _lineCount;

        public LineNumerator(RichTextBox lineNumbers, RichTextBox inputText)
        {
            _lineNumbers = lineNumbers;
            _inputText = inputText;

            StartupInitialization();

            _inputText.TextChanged += _inputText_TextChanged;
        }

        private void StartupInitialization()
        {
            if(_lineNumbers.Document == null)
                _lineNumbers.Document = new FlowDocument();

            _lineCount = 0;

            _lineNumbers.Document.Blocks.Clear();

            if( _inputText.Document == null)
            {
                _inputText.Document = new FlowDocument();
                _inputText.Document.Blocks.Clear();
                _inputText.Document.Blocks.Add(new Paragraph());
            }

            _lineCount = _inputText.Document.Blocks.Count;

            for(int i = 0; i < _lineCount; i++)
            {
                _lineNumbers.Document.Blocks.Add(new Paragraph(new Run((i + 1).ToString())));
            }
        }

        private void _inputText_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_lineCount == _inputText.Document.Blocks.Count)
                return;

            if(_lineCount < _inputText.Document.Blocks.Count)
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
    }
}

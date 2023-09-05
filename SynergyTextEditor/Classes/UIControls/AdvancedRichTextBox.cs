using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;

#nullable disable

namespace SynergyTextEditor.Classes.UIControls
{
    public enum TextContentChangeType
    {
        TextInput,
        WhitespaceCharInput,
        Paste,
        Delete
    }

    public class TextContentChangedEventArgs : EventArgs
    {
        public TextContentChangeType ChangeType { get; private set;}

        public TextContentChangedEventArgs(TextContentChangeType changeType)
        {
            this.ChangeType = changeType;
        }
    }

    public class AdvancedRichTextBox : RichTextBox
    {
        public event Action<object, TextContentChangedEventArgs> TextContentChanged;

        public AdvancedRichTextBox()
        {
            DataObject.AddPastingHandler(this, OnPaste);
        }

        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            base.OnPreviewKeyDown(e);

            if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
                return;

            if (e.Key == Key.Enter ||
               e.Key == Key.Space)
            {
                TextContentChanged?.Invoke(this, new TextContentChangedEventArgs(TextContentChangeType.WhitespaceCharInput));
            }

            if(e.Key == Key.Back && !IsCaretAtTheEdge(CaretPosition, LogicalDirection.Backward) ||
               e.Key == Key.Delete && !IsCaretAtTheEdge(CaretPosition, LogicalDirection.Forward)) 
            {
                TextContentChanged?.Invoke(this, new TextContentChangedEventArgs(TextContentChangeType.Delete));
            }
        }

        private bool IsCaretAtTheEdge(TextPointer caretPos, LogicalDirection direction)
        {
            if (!caretPos.IsAtInsertionPosition)
                return true;

            var nextPos = caretPos.GetNextInsertionPosition(direction);

            if (nextPos is null || !nextPos.IsAtInsertionPosition)
                return true;

            return false;
        }

        protected override void OnPreviewTextInput(TextCompositionEventArgs e)
        {
            base.OnPreviewTextInput(e);

            TextContentChanged?.Invoke(this, new TextContentChangedEventArgs(TextContentChangeType.TextInput));
        }

        private void OnPaste(object sender, DataObjectPastingEventArgs e)
        {
            TextContentChanged?.Invoke(this, new TextContentChangedEventArgs(TextContentChangeType.Paste));

            e.Handled = true;
        }
    }
}

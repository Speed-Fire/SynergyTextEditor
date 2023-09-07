using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace SynergyTextEditor.Classes.SyntaxHighlighting.Highlighters.TextHighlighters
{
    public record TextHighlighterInitArgs(RichTextBox rtb) : SyntaxHighlighterInitArgs(rtb);
}

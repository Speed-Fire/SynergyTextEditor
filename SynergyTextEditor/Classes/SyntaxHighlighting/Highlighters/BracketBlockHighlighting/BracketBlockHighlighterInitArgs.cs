using System.Windows.Controls;

namespace SynergyTextEditor.Classes.SyntaxHighlighting.Highlighters.BracketBlockHighlighting
{
    public record BracketBlockHighlighterInitArgs(RichTextBox Rtb, Canvas Canvas) :
        SyntaxHighlighterInitArgs(Rtb);
}

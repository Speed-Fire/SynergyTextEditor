using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Interop;
using System.Windows.Media;

#nullable disable

namespace SynergyTextEditor.Classes
{
    struct Tag
    {
        public TextPointer StartPosition;
        public TextPointer EndPosition;
        //public string Word;
    }

    public class TextHighlighter
    {
        private readonly RichTextBox rtb;

        private bool IsColoring { get; set; } = false;

        private Paragraph CurrentParagraph { get; set; }

        private char[] SeparatorChars = new char[] { '{', '}', '(', ')', '[', ']', '+', '-', '=', '*', '/', '!', '|', '&', '^' };

        public TextHighlighter(RichTextBox rtb)
        {
            this.rtb = rtb;

            rtb.TextChanged += TextChanged;
        }

        public void TextChanged(object sender, TextChangedEventArgs e)
        {
            if (rtb.Document is null)
                return;

            if(e.Changes.Count == 0) return;

            rtb.TextChanged -= TextChanged;

            // Get current paragraph (row)
            /// this optimization won't work with multirow text styling (such as commentaries in VS)
            var paragraph = rtb.Document.ContentStart.GetPositionAtOffset(e.Changes.First().Offset).Paragraph;
            if(paragraph is null)
            {
                rtb.TextChanged += TextChanged;
                return;
            }

            // clear all text styling in the paragraph
            var range = new TextRange(paragraph.ContentStart, paragraph.ContentEnd);
            range.ClearAllProperties();

            List<Tag> tags = new();

            // Find all keywords in the paragraph
            for (var navigator = /*rtb.Document*/paragraph.ContentStart;
                navigator.CompareTo(/*rtb.Document*/paragraph.ContentEnd) < 0;
                navigator = navigator.GetNextContextPosition(LogicalDirection.Forward))
            {
                var context = navigator.GetPointerContext(LogicalDirection.Backward);

                if(context == TextPointerContext.ElementStart && navigator.Parent is Run run)
                {
                    if (run.Text != "")
                        tags.AddRange(GetTagsFromRun(run));
                }
            }

            // when all keywords are found, styling them
            for (int i = 0; i < tags.Count; i++) 
            {
                try
                {
                    var colorRange = new TextRange(tags[i].StartPosition, tags[i].EndPosition);
                    colorRange.ApplyPropertyValue(TextElement.ForegroundProperty,
                        new SolidColorBrush(Color.FromRgb(82, 156, 214)));
                    //colorRange.ApplyPropertyValue(TextElement.FontWeightProperty,
                    //    FontWeights.Bold);
                }
                catch { }
            }

            rtb.TextChanged += TextChanged;
        }

        private List<Tag> GetTagsFromRun(Run run)
        {
            var res = new List<Tag>();
            var text = run.Text;

            int endId, startId = endId = 0;

            for(int i = 0; i < text.Length; i++)
            {
                if (char.IsWhiteSpace(text[i]) || SeparatorChars.Contains(text[i]))
                {
                    if(i > 0 && !(char.IsWhiteSpace(text[i - 1]) || SeparatorChars.Contains(text[i - 1])))
                    {
                        endId = i - 1;

                        var word = text.Substring(startId, endId - startId + 1);

                        if (IsKeyword(word))
                        {
                            var tag = new Tag()
                            {
                                StartPosition = run.ContentStart.GetPositionAtOffset(startId, LogicalDirection.Forward),
                                EndPosition = run.ContentStart.GetPositionAtOffset(endId + 1, LogicalDirection.Backward)
                            };
                            res.Add(tag);
                        }
                    }

                    startId = i + 1;
                }
            }

            var lastword = text.Substring(startId, text.Length - startId);
            if (IsKeyword(lastword))
            {
                var tag = new Tag()
                {
                    StartPosition = run.ContentStart.GetPositionAtOffset(startId, LogicalDirection.Forward),
                    EndPosition = run.ContentStart.GetPositionAtOffset(text.Length, LogicalDirection.Backward)
                };
                res.Add(tag);
            }

            return res;
        }

        private bool IsKeyword(string word)
        {
            var keywords = new List<string>()
            {
                "object",
                "string",
                "int",
                "var"
            };

            return keywords.Contains(word);
        }
    }
}

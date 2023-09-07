using CommunityToolkit.Mvvm.Messaging;
using SynergyTextEditor.Classes.Workers;
using SynergyTextEditor.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Documents;

namespace SynergyTextEditor.Classes.SyntaxHighlighting.Highlighters.BracketBlockHighlighting
{
    public class BracketBlockHighlighter :
        SyntaxHighlighter
    {
        private readonly BracketBlockHighlightingWorker worker;

        private Canvas canvas;

        public BracketBlockHighlighter(BracketBlockHighlightingWorker worker)
        {
            this.worker = worker;
        }

        public override void Init(SyntaxHighlighterInitArgs args)
        {
            if(!(args is  BracketBlockHighlighterInitArgs))
            {
                throw new ArgumentException("Incorrect type of inition args!");
            }

            var _args = args as BracketBlockHighlighterInitArgs;
            base.Init(_args);
            canvas = _args.Canvas;

            var workerArgs = new BracketBlockHighlightingWorkerArgs(rtb, canvas);
            worker.Run(workerArgs, null);
        }

        protected override void Highlight(TextPointer start, TextPointer end)
        {
            worker.PushInterval(start, end);
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            worker.Abort();
            worker.Dispose();
        }
    }
}

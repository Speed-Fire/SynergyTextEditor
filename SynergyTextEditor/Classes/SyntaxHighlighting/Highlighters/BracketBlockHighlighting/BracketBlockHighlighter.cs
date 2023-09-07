using CommunityToolkit.Mvvm.Messaging;
using SynergyTextEditor.Classes.Blockers;
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
        SyntaxHighlighter,
        IRecipient<EnableBracketBlockHighlightingMessage>
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

        void IRecipient<EnableBracketBlockHighlightingMessage>.Receive(EnableBracketBlockHighlightingMessage message)
        {
            if (message.Value) // unblock
            {
                if (!blocker.IsBlocked()) return;

                blocker.SetState(!message.Value);

                FullHighlight();
            }
            else // block
            {
                if (blocker.IsBlocked()) return;

                blocker.SetState(!message.Value);

                worker.ClearIntervals();
                worker.ClearBracketList();
            }
        }
    }
}

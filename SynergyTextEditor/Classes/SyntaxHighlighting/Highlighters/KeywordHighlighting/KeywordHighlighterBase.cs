using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.DependencyInjection;
using SynergyTextEditor.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Documents;



namespace SynergyTextEditor.Classes.SyntaxHighlighting.Highlighters.KeywordHighlighting
{
    public abstract class KeywordHighlighterBase :
        SyntaxHighlighter,
        IRecipient<FileOpenedMessage>,
        IRecipient<KeywordLanguageUploadedMessage>,
        IRecipient<SelectKeywordLanguageMessage>
    {
        protected readonly IKeywordLanguageSelector languageSelector;

        protected abstract string CurrentLanguageName { get; }

        protected KeywordHighlighterBase(IKeywordLanguageSelector languageSelector)
        {
            this.languageSelector = languageSelector;
        }

        public override void Init(SyntaxHighlighterInitArgs args)
        {
            if (!(args is KeywordHighlighterInitArgs))
            {
                throw new ArgumentException("Incorrect type of inition args!");
            }

            base.Init(args);

            WeakReferenceMessenger.Default.Register<CurrentLanguageNameRequestMessage>(this, (r, m) =>
            {
                if (CurrentLanguageName == null)
                {
                    m.Reply("");
                }
                else
                {
                    m.Reply(CurrentLanguageName);
                }
            });
        }

        public abstract void Receive(FileOpenedMessage message);
        public abstract void Receive(KeywordLanguageUploadedMessage message);
        public abstract void Receive(SelectKeywordLanguageMessage message);
    }
}

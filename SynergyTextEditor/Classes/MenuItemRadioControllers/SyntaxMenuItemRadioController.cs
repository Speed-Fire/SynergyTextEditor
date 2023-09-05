using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using SynergyTextEditor.Classes.Utilities.UI;
using SynergyTextEditor.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;



namespace SynergyTextEditor.Classes.MenuItemRadioControllers
{
    public class SyntaxMenuItemRadioController : IMenuItemRadioController
    {
        private readonly IKeywordLanguageSelector _languageSelector;
        private List<MenuItem> _items = null;

        public SyntaxMenuItemRadioController(IKeywordLanguageSelector languageSelector)
        {
            _languageSelector = languageSelector;
        }

        public void Fill(MenuItem root)
        {
            RelayCommand<string> command =
                WeakReferenceMessenger.Default.Send(new SelectSyntaxCommandRequestMessage());

            var langNames = _languageSelector.GetAllLanguages().ToList();
            var comParams = langNames.ToList();

            langNames.Insert(0, "None");
            comParams.Insert(0, "");

            _items = MenuItemRadioFiller.Fill(root, command, langNames, comParams);

            Select();
        }

        public void Select()
        {
            string langName =
                WeakReferenceMessenger.Default.Send(new CurrentLanguageNameRequestMessage());

            if (langName == "")
                langName = "None";

            MenuItemRadioFiller.Select(_items, langName);
        }
    }
}

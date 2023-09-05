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
    public class ThemeMenuItemRadioController : IMenuItemRadioController
    {
        private readonly AppThemeController _themeController;
        private List<MenuItem> _items;

        public ThemeMenuItemRadioController(AppThemeController themeController)
        {
            _themeController = themeController;
        }

        public void Fill(MenuItem root)
        {
            RelayCommand<string> command = 
                WeakReferenceMessenger.Default.Send(new SetThemeCommandRequestMessage());

            var names = _themeController.Themes.ToList();

            _items = MenuItemRadioFiller.Fill(root, command, names/*, comParams*/);

            Select();
        }

        public void Select()
        {
            var curTheme = _themeController.CurrentThemeName;

            MenuItemRadioFiller.Select(_items, curTheme);
        }
    }
}

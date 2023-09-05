using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using SynergyTextEditor.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;



namespace SynergyTextEditor.Classes.Utilities.UI
{
    public static class MenuItemRadioFiller
    {
        #region Additional variants

        public static List<MenuItem> Fill(
            MenuItem root,
            ICommand command,
            IEnumerable<string> btnNames)
        {
            var groupName = Guid.NewGuid().ToString();

            return Fill(root, groupName, command, btnNames, btnNames);
        }

        public static List<MenuItem> Fill<T>(
            MenuItem root,
            ICommand command,
            IEnumerable<string> btnNames,
            IEnumerable<T> commandParams)
        {
            var groupName = Guid.NewGuid().ToString();

            return Fill(root, groupName, command, btnNames, commandParams);
        }

        public static List<MenuItem> Fill(
            MenuItem root,
            string radioGroupName,
            ICommand command,
            IEnumerable<string> btnNames)
        {
            return Fill(root, radioGroupName, command, btnNames, btnNames);
        }

        #endregion

        public static List<MenuItem>  Fill<T>(
            MenuItem root,
            string radioGroupName,
            ICommand command,
            IEnumerable<string> btnNames,
            IEnumerable<T> commandParams)
        {
            #region Validations

            if (root is null)
                throw new ArgumentNullException(nameof(root));

            if (string.IsNullOrEmpty(radioGroupName))
                throw new ArgumentNullException(nameof(radioGroupName));

            if (command is null)
                throw new ArgumentNullException(nameof(command));

            if (btnNames is null)
                throw new ArgumentNullException(nameof(btnNames));

            if (commandParams is null)
                throw new ArgumentNullException(nameof(commandParams));

            if (btnNames.Count() != commandParams.Count())
                throw new ArgumentException("Button count doesn't match command parameter count!");

            #endregion

            var menuItems = new List<MenuItem>();

            root.Items.Clear();

            for (int i = 0; i < btnNames.Count(); i++)
            {
                var newNode =
                    CreateNode(
                        btnNames.ElementAt(i), 
                        radioGroupName, 
                        command, 
                        commandParams.ElementAt(i));

                root.Items.Add(newNode);
                menuItems.Add(newNode);
            }

            return menuItems;
        }

        private static MenuItem CreateNode<T>( 
            string name, 
            string groupName, 
            ICommand command, 
            T parameter)
        {
            var radBtn = new RadioButton()
            {
                GroupName = groupName,
                Command = command,
                CommandParameter = parameter
            };

            var item = new MenuItem()
            {
                Header = name,
                Icon = radBtn,
                Command = command,
                CommandParameter = parameter
            };

            item.Click += (sender, e) =>
            {
                radBtn.IsChecked = true;
            };

            return item;
        }

        public static void Select(IEnumerable<MenuItem> items, string name)
        {
            #region Validations

            if(items is null)
                throw new ArgumentNullException(nameof(items));

            if (name is null) 
                throw new ArgumentNullException(nameof(name));

            #endregion

            foreach (var item in items)
            {
                if(item.Header as string == name)
                {
                    var radBtn = item.Icon as RadioButton;

                    radBtn.IsChecked = true;

                    break;
                }
            }
        }
    }
}

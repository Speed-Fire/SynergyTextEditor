using CommunityToolkit.Mvvm.Messaging;
using SynergyTextEditor.Classes;
using SynergyTextEditor.Messages;
using SynergyTextEditor.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using static SynergyTextEditor.Classes.KeywordLanguageLoader.KeywordLanguageSerializable.KeywordGroupSerializable;

namespace SynergyTextEditor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private MainVM viewModel;

        private TextHighlighter textHighlighter;

        public MainWindow()
        {
            InitializeComponent();

            switch (AppThemeController.Instance.CurrentThemeName)
            {
                case "light":
                    LightThemeSetting.IsChecked = true;
                    break;
                case "dark":
                default:
                    DarkThemeSetting.IsChecked = true;
                    break;
            }

            var vm = new MainVM(Editor.Document);

            Editor.TextChanged += Editor_TextChanged;

            DataContext = viewModel = vm;
            CommandBindings.AddRange(viewModel.CommandBindings);

            textHighlighter = new TextHighlighter(Editor);

            //CreateHighlightLanguage();
        }

        private void Editor_TextChanged(object sender, TextChangedEventArgs e)
        {
            WeakReferenceMessenger.Default.Send(new TextChangedMessage(e));
        }

        private static void CreateHighlightLanguage()
        {
            var lang = new KeywordLanguageLoader.KeywordLanguageSerializable();
            lang.languageName = "C#";

            var blueGroup = new KeywordLanguageLoader.KeywordLanguageSerializable.KeywordGroupSerializable();
            blueGroup.keywordGroupType = KeywordGroupType.Normal;
            blueGroup.keywords = new List<string>()
            {
                "abstract",
                "as",
                "base",
                "bool",
                "byte",
                "char",
                "checked",
                "class",
                "const",
                "decimal",
                "delegate",
                "double",
                "enum",
                "event",
                "explicit",
                "extern",
                "false",
                "fixed",
                "float",
                "for",
                "implicit",
                "int",
                "interface",
                "internal",
                "is",
                "lock",
                "long",
                "namespace",
                "new",
                "null",
                "object",
                "operator",
                "out",
                "override",
                "params",
                "private",
                "protected",
                "public",
                "readonly",
                "ref",
                "sbyte",
                "sealed",
                "short",
                "sizeof",
                "stackalloc",
                "static",
                "string",
                "struct",
                "this",
                "true",
                "typeof",
                "uint",
                "ulong",
                "unchecked",
                "unsafe",
                "ushort",
                "using",
                "virtual",
                "void",
                "var",
                "volatile", "async",
                "await", "get", "set", "global", "nameof", "dynamic", "partial",
                "unmanaged", "value", "yield", "record"
            };
            blueGroup.styles = new()
            {
                 new TupleSerializable<string, string>("ForegroundProperty", "solid(86,156,203)")
            };

            var purpleGroup = new KeywordLanguageLoader.KeywordLanguageSerializable.KeywordGroupSerializable();
            purpleGroup.keywordGroupType = KeywordGroupType.Normal;
            purpleGroup.keywords = new List<string>()
            {
                "do", "while", "foreach", "break", "continue", "goto",
                "case", "default", "switch", "return", "try", "catch",
                "finally", "throw", "if", "else", "in"
            };
            purpleGroup.styles = new()
            {
                new TupleSerializable < string, string > ("ForegroundProperty", "solid(216, 160, 223)")
            };

            var special1Group = new KeywordLanguageLoader.KeywordLanguageSerializable.KeywordGroupSerializable();
            special1Group.keywordGroupType = KeywordGroupType.Special;
            special1Group.keywords = new List<string>()
            {
                "{","}","(",")","[","]"
            };
            special1Group.styles = new()
            {
                new TupleSerializable<string, string>("FontWeightProperty", "Bold")
            };

            var special2Group = new KeywordLanguageLoader.KeywordLanguageSerializable.KeywordGroupSerializable();
            special2Group.keywordGroupType = KeywordGroupType.Special;
            special2Group.keywords = new List<string>()
            {
                ".",",","&","*","/","<",">","=","+","-","|","^","!",":",";","?","%"
            };
            special2Group.styles = new()
            {

            };

            lang.keywordGroups = new()
            {
                blueGroup,
                purpleGroup,
                special1Group,
                special2Group
            };

            var loader = new KeywordLanguageLoader();

            loader.Save(lang, "C:\\Users\\Влад\\Desktop\\CSlangHighlight.xaml");
        }

        private void MenuSettingsThemeLight_Click(object sender, RoutedEventArgs e)
        {
            LightThemeSetting.IsChecked = true;
        }

        private void MenuSettingsThemeDark_Click(object sender, RoutedEventArgs e)
        {
            DarkThemeSetting.IsChecked = true;
        }
    }
}

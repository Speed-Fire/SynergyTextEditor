﻿using CommunityToolkit.Mvvm.Messaging;
using SynergyTextEditor.Classes;
using SynergyTextEditor.Classes.Blockers;
using SynergyTextEditor.Classes.SyntaxHighlighting.Highlighters.BracketBlockHighlighting;
using SynergyTextEditor.Classes.MenuItemRadioControllers;
using SynergyTextEditor.Classes.SyntaxHighlighting.Highlighters.KeywordHighlighting;
using SynergyTextEditor.Classes.UIControls;
using SynergyTextEditor.Messages;
using SynergyTextEditor.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;

namespace SynergyTextEditor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : 
        Window,
        IRecipient<KeywordLanguageUploadedMessage>,
        IRecipient<KeywordLanguageChangedMessage>
    {
        private readonly SyntaxMenuItemRadioController _syntaxMenuItemRadioController;
        private readonly ThemeMenuItemRadioController _themeMenuItemRadioController;
        private readonly LineNumerator _lineNumerator;

        private readonly BracketBlockHighlighter _bracketBlockHighlighter;

        private readonly TextChangedBlocker _textChangedBlocker;

        private readonly MainVM _viewModel;
        private readonly KeywordHighlighterBase _textHighlighter;

        public MainWindow(SyntaxMenuItemRadioController syntaxMenuItemRadioController,
            ThemeMenuItemRadioController themeMenuItemRadioController,
            KeywordHighlighterBase textHighlighter,
            BracketBlockHighlighter bracketBlockHighlighter)
        {
            InitializeComponent();

            WeakReferenceMessenger.Default.RegisterAll(this);

            var vm = new MainVM(Editor.Document);

            _textChangedBlocker = new TextChangedBlocker();

            DataContext = _viewModel = vm;
            CommandBindings.AddRange(_viewModel.CommandBindings);
            

            #region Initialize visual components

            _textHighlighter = textHighlighter;
            _textHighlighter.Init(new KeywordHighlighterInitArgs(Editor));

            _syntaxMenuItemRadioController = syntaxMenuItemRadioController;
            _syntaxMenuItemRadioController.Fill(SyntaxList);

            _themeMenuItemRadioController = themeMenuItemRadioController;
            _themeMenuItemRadioController.Fill(ThemeList);

            _lineNumerator = new LineNumerator(LineNumbers, Editor);

            _bracketBlockHighlighter = bracketBlockHighlighter;
            _bracketBlockHighlighter.Init(new BracketBlockHighlighterInitArgs(Editor, DrawingPanel));

            #endregion
        }

        /// <summary>
        /// All text changes including text format changes.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Editor_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_textChangedBlocker.IsBlocked())
                return;

            WeakReferenceMessenger.Default.Send(new TextChangedMessage(e));
        }

        /// <summary>
        /// User entering/removing/pasting/replacing text.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Editor_TextContentChanged(object sender, TextContentChangedEventArgs e)
        {
            WeakReferenceMessenger.Default.Send(new FileChangedMessage(e));
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

            lang.fileExtensions = new()
            {
                ".cs"
            };

            var loader = new KeywordLanguageLoader();

            loader.Save(lang, "C:\\Users\\Влад\\Desktop\\CSlangHighlight.xaml");
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);

            _lineNumerator.Dispose();
            _textHighlighter.Dispose();
            _bracketBlockHighlighter.Dispose();
        }

        #region Message handlers

        void IRecipient<KeywordLanguageUploadedMessage>.Receive(KeywordLanguageUploadedMessage message)
        {
            _syntaxMenuItemRadioController.Fill(SyntaxList);
        }

        void IRecipient<KeywordLanguageChangedMessage>.Receive(KeywordLanguageChangedMessage message)
        {
            App.Current.Dispatcher?.Invoke(_syntaxMenuItemRadioController.Select);
        }

        #endregion

        #region Text scaling

        private const double ScaleStep = 0.1;

        private void Window_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            {
                if (e.Delta > 0 && Scale.Value < Scale.Maximum)
                {
                    Scale.Value += ScaleStep;
                }
                else if (e.Delta < 0 && Scale.Value > Scale.Minimum)
                {
                    Scale.Value -= ScaleStep;
                }
            }
        }

        #endregion

        #region Scrolling

        private bool _isScrolling = false;

        private void Editor_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (_isScrolling) return;
            _isScrolling = true;

            LineNumbers.ScrollToVerticalOffset(e.VerticalOffset);

            _isScrolling = false;
        }

        private void LineNumbers_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (_isScrolling) return;
            _isScrolling = true;

            Editor.ScrollToVerticalOffset(e.VerticalOffset);

            _isScrolling = false;
        }

        #endregion
    }
}

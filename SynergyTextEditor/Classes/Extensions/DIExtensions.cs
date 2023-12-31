﻿using Microsoft.Extensions.DependencyInjection;
using SynergyTextEditor.Classes.SyntaxHighlighting.Highlighters.BracketBlockHighlighting;
using SynergyTextEditor.Classes.Converters;
using SynergyTextEditor.Classes.MenuItemRadioControllers;
using SynergyTextEditor.Classes.SyntaxHighlighting.Highlighters.KeywordHighlighting;
using SynergyTextEditor.Classes.Workers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace SynergyTextEditor.Classes.Extensions
{
    public static class DIExtensions
    {
        public static IServiceCollection RegisterConverters(this IServiceCollection services)
        {
            services
                .AddTransient<IConverter<TupleSerializable<string, string>, Tuple<DependencyProperty, object>>,
                              StringTuple2DPTupleConverter>();

            return services;
        }

        #region Highlighting system

        public static IServiceCollection RegisterHighlightingSystem(this IServiceCollection services)
        {
            services
                .RegisterKeywordHighlighter()
                .RegisterBracketBlockHighlighter();
            
            return services;
        }

        public static IServiceCollection RegisterKeywordHighlighter(this IServiceCollection services)
        {
            services
                .AddSingleton<IKeywordLanguageLoader, KeywordLanguageLoader>()
                .AddSingleton<IKeywordLanguageSelector, KeywordLanguageSelector>()
                .AddTransient<KeywordHighlighterBase, ParallelKeywordHighlighter>();

            return services;
        }

        public static IServiceCollection RegisterBracketBlockHighlighter(this IServiceCollection services)
        {
            services
                .AddTransient<BracketBlockHighlighter>();

            return services;
        }

        #endregion

        public static IServiceCollection RegisterMenuItemRadioControllers(this IServiceCollection services)
        {
            services
                .AddSingleton<SyntaxMenuItemRadioController>()
                .AddSingleton<ThemeMenuItemRadioController>();

            return services;
        }

        public static IServiceCollection RegisterThemeServices(this IServiceCollection services)
        {
            services
                .AddSingleton<AppThemeController>();

            return services;
        }

        public static IServiceCollection RegisterWorkers(this IServiceCollection services)
        {
            services
                .AddSingleton<KeywordHighlightingWorker>()
                .AddSingleton<BracketBlockHighlightingWorker>();

            return services;
        }
    }
}

using Microsoft.Extensions.DependencyInjection;
using SynergyTextEditor.Classes.Converters;
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

        public static IServiceCollection RegisterDPConverters(this IServiceCollection services)
        {


            return services;
        }
    }
}

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SynergyTextEditor.Classes.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#nullable disable

namespace SynergyTextEditor
{
    internal class Program
    {
        internal static IHost AppHost { get; private set; }

        [STAThread]
        internal static void Main(string[] args)
        {
            var builder = Host.CreateDefaultBuilder(args);

            builder.ConfigureServices(services =>
            {
                services.RegisterConverters();
                services.RegisterDPConverters();

                services.AddSingleton<App>();
                services.AddSingleton<MainWindow>();
            });

            AppHost = builder.Build();

            var app = AppHost.Services.GetRequiredService<App>();
            app.Run();
        }
    }
}

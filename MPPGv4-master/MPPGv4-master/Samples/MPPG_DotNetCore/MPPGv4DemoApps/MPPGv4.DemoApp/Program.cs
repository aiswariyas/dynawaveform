using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MPPGv4.ServiceFactory;
using MPPGv4.UIFactory;
using System;
using System.IO;

namespace MPPGv4.DemoApp
{
    class Program
    {
        static void Main(string[] args)
        {
           

            IConfiguration config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", true, true)
                .Build();

            IServiceCollection services = new ServiceCollection();
            services.AddSingleton<IConfiguration>(config);
            services.AddSingleton<IMppgv4UIFactory, Mppgv4UIfactory>();
            services.AddSingleton<IProcessCardSwipeClient, ProcessCardSwipeClient>();
            services.AddSingleton<IProcessKeyPadEntryClient, ProcessKeyPadEntryClient>();
            services.AddSingleton<IProcessDataClient, ProcessDataClient>();
            services.AddSingleton<IProcessManualEntryClient, ProcessManualEntryClient>();
            services.AddSingleton<IProcessTokenClient, ProcessTokenClient>();
            IServiceProvider serviceProvider = services.BuildServiceProvider();
            var uiFactory = serviceProvider.GetService<IMppgv4UIFactory>();

            uiFactory.ShowUI(MPPGv4UI.PROCESSDATA);
        }
    }
}

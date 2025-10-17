using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace iWebIT_PrintAgent
{
    internal static class Program
    {
        static void Main()
        {
            IHost host = Host.CreateDefaultBuilder()
                .UseWindowsService()
                .ConfigureAppConfiguration((context, config) =>
                {
                    config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
                })
                .ConfigureServices((context, services) =>
                {
                    services.AddSingleton<WindowsPrintService>();
                    services.AddHostedService(provider => provider.GetRequiredService<WindowsPrintService>());
                })
                .Build();

            host.Run();
        }
    }
}

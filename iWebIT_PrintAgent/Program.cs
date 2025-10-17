using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;


namespace iWebIT_PrintAgent
{
public class Program
{
public static void Main(string[] args)
{
var host = Host.CreateDefaultBuilder(args)
.UseWindowsService() // run as windows service when installed
.ConfigureServices((context, services) =>
{
services.AddHostedService<PrintService>();
})
.Build();


host.Run();
}
}
}
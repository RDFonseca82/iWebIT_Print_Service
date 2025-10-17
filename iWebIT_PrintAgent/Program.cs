using System;
using System.ServiceProcess;

namespace iWebIT_PrintAgent
{
    internal static class Program
    {
        static void Main()
        {
            ServiceBase[] ServicesToRun;
            ServicesToRun = new ServiceBase[]
            {
                new WindowsPrintService()
            };
            ServiceBase.Run(ServicesToRun);
        }
    }
}

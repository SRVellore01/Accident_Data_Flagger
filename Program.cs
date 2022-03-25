using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace AccidentDataFlagger
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main()
        {
            //Service1 service = new Service1();
            //if (Environment.UserInteractive)
            //{
            //    service.RunAsConsole();
            //}

            log4net.Config.XmlConfigurator.Configure();
            ServiceBase[] ServicesToRun;
            ServicesToRun = new ServiceBase[]
            {
                new Service1()

            };
            ServiceBase.Run(ServicesToRun);
        }
    }
}

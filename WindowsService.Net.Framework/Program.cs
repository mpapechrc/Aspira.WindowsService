﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace WindowsService.Net.Framework
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main()
        {
            try
            {

                ServiceBase[] ServicesToRun;
                ServicesToRun = new ServiceBase[]
                {
                new DatabaseBackup()
                };
                ServiceBase.Run(ServicesToRun);
            }
            catch (Exception ex)
            {

                EventLog.WriteEntry("Application",ex.Message);
            }
        }
    }
}

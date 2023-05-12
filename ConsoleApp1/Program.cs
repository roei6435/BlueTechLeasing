using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Configuration;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Tooling.Connector;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Net.Http;
using Microsoft.Xrm.Sdk.Async;
using Microsoft.Xrm.Sdk.Messages;

namespace ConsoleApp1
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Waiting to connected...");
            if (Common.CheckConnectToServiceSucssesfully())
            {
                DateTime startTime = DateTime.Now;
                Console.WriteLine($"Connected succsesfully. Job started in {startTime}");             
                await UpdateData.Execute();
                DateTime stopTime = DateTime.Now;
                TimeSpan elapsedTime = stopTime.Subtract(startTime);
                Console.WriteLine($"Running time: {elapsedTime}");
            }
            else
            {
                Console.WriteLine("Feild connect to server");
            }
            Console.ReadKey();
        }


    }
}

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
            Console.WriteLine($"Started on {DateTime.Now}");
            //if (Common.CheckConnectToServiceSucssesfully())
            //{
            //    Console.WriteLine("Connected succsesfully");
            //}
            //var response = await Update.UpdateManufacturers();
            //Console.WriteLine($"Started on {DateTime.Now}");
            //Console.ReadKey();
        }


    }
}
